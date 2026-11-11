using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace HRMSystem.DAL
{
    public class ChamCongLuongDAL
    {
        private MySqlDataAdapter adapterChamCong;
        private DataTable dtChamCong;

        // 1. Nạp bảng chấm công theo mô hình ngắt kết nối (Disconnected Model)
        public DataTable LoadBangChamCong(string thangNam)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Tự động khởi tạo dòng chấm công mặc định (0 ngày, 0 OT) cho nhân viên chưa có trong tháng
                string initSql = @"INSERT IGNORE INTO BangChamCong (MaNV, ThangNam, SoNgayCong, SoGioOT, DaChot)
                                   SELECT MaNV, @ThangNam, 0, 0, 0 FROM NhanVien";
                using (MySqlCommand initCmd = new MySqlCommand(initSql, conn))
                {
                    initCmd.Parameters.AddWithValue("@ThangNam", thangNam);
                    initCmd.ExecuteNonQuery();
                }
            }

            // Lưu ý: Để CommandBuilder tự sinh lệnh UPDATE, câu SELECT của Adapter phải truy vấn trên 1 bảng chính (BangChamCong)
            MySqlConnection adapterConn = DatabaseHelper.GetConnection();
            string selectSql = "SELECT MaCong, MaNV, ThangNam, SoNgayCong, SoGioOT, DaChot FROM BangChamCong WHERE ThangNam = @ThangNam";
            MySqlCommand selectCmd = new MySqlCommand(selectSql, adapterConn);
            selectCmd.Parameters.AddWithValue("@ThangNam", thangNam);

            adapterChamCong = new MySqlDataAdapter(selectCmd);
            dtChamCong = new DataTable("BangChamCong");
            adapterChamCong.Fill(dtChamCong);

            return dtChamCong;
        }

        // 2. Đồng bộ dữ liệu chấm công từ DataGridView về MySQL bằng CommandBuilder + adapter.Update()
        public int SaveBangChamCong()
        {
            if (adapterChamCong == null || dtChamCong == null)
                throw new InvalidOperationException("Chưa nạp dữ liệu chấm công!");

            // Tự động sinh INSERT / UPDATE / DELETE command
            MySqlCommandBuilder builder = new MySqlCommandBuilder(adapterChamCong);
            return adapterChamCong.Update(dtChamCong);
        }

        // 3. Tính lương & Chốt lương bằng Transaction
        public void ChotLuongThang(string thangNam)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (MySqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        // Kiểm tra xem tháng này đã khóa sổ chưa
                        string checkSql = "SELECT COUNT(*) FROM BangChamCong WHERE ThangNam = @ThangNam AND DaChot = 1";
                        using (MySqlCommand checkCmd = new MySqlCommand(checkSql, conn, trans))
                        {
                            checkCmd.Parameters.AddWithValue("@ThangNam", thangNam);
                            long lockedCount = Convert.ToInt64(checkCmd.ExecuteScalar());
                            if (lockedCount > 0)
                            {
                                throw new Exception($"Dữ liệu chấm công tháng {thangNam} đã được chốt và khóa trước đó!");
                            }
                        }

                        // Lấy danh sách nhân viên kèm dữ liệu chấm công trong tháng
                        string getSql = @"SELECT nv.MaNV, nv.LuongCoBan, nv.HeSoLuong, cc.SoNgayCong, cc.SoGioOT
                                          FROM NhanVien nv
                                          INNER JOIN BangChamCong cc ON nv.MaNV = cc.MaNV
                                          WHERE cc.ThangNam = @ThangNam";
                        DataTable dtData = new DataTable();
                        using (MySqlCommand getCmd = new MySqlCommand(getSql, conn, trans))
                        {
                            getCmd.Parameters.AddWithValue("@ThangNam", thangNam);
                            using (MySqlDataAdapter da = new MySqlDataAdapter(getCmd))
                            {
                                da.Fill(dtData);
                            }
                        }

                        if (dtData.Rows.Count == 0)
                            throw new Exception("Không có dữ liệu chấm công để chốt lương!");

                        // Tính lương từng nhân viên và ghi vào BangLuong
                        // Công thức: ThucLinh = (LuongCoBan * HeSoLuong / 22) * SoNgayCong + (SoGioOT * 50,000)
                        foreach (DataRow row in dtData.Rows)
                        {
                            int maNV = Convert.ToInt32(row["MaNV"]);
                            decimal luongCB = Convert.ToDecimal(row["LuongCoBan"]);
                            decimal heSo = Convert.ToDecimal(row["HeSoLuong"]);
                            decimal ngayCong = Convert.ToDecimal(row["SoNgayCong"]);
                            decimal gioOT = Convert.ToDecimal(row["SoGioOT"]);

                            if (ngayCong < 0 || ngayCong > 31)
                                throw new Exception($"Nhân viên mã {maNV} có số ngày công không hợp lệ ({ngayCong})!");

                            decimal thucLinh = Math.Round((luongCB * heSo / 22m) * ngayCong + (gioOT * 50000m), 2);

                            string insertLuongSql = @"INSERT INTO BangLuong (MaNV, ThangNam, ThucLinh)
                                                      VALUES (@MaNV, @ThangNam, @ThucLinh)
                                                      ON DUPLICATE KEY UPDATE ThucLinh = @ThucLinh";
                            using (MySqlCommand cmdLuong = new MySqlCommand(insertLuongSql, conn, trans))
                            {
                                cmdLuong.Parameters.AddWithValue("@MaNV", maNV);
                                cmdLuong.Parameters.AddWithValue("@ThangNam", thangNam);
                                cmdLuong.Parameters.AddWithValue("@ThucLinh", thucLinh);
                                cmdLuong.ExecuteNonQuery();
                            }
                        }

                        // Khóa dữ liệu chấm công tháng đó (DaChot = 1)
                        string lockSql = "UPDATE BangChamCong SET DaChot = 1 WHERE ThangNam = @ThangNam";
                        using (MySqlCommand lockCmd = new MySqlCommand(lockSql, conn, trans))
                        {
                            lockCmd.Parameters.AddWithValue("@ThangNam", thangNam);
                            lockCmd.ExecuteNonQuery();
                        }

                        // Hoàn tất giao dịch
                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        // Có lỗi xảy ra -> Khôi phục toàn bộ dữ liệu (Rollback)
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        // Lấy bảng lương hiển thị lên lưới
        public DataTable GetBangLuong(string thangNam)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                string sql = @"SELECT bl.MaLuong, bl.MaNV, nv.HoTen, pb.TenPB, bl.ThangNam, 
                                      nv.LuongCoBan, nv.HeSoLuong, cc.SoNgayCong, cc.SoGioOT, bl.ThucLinh
                               FROM BangLuong bl
                               INNER JOIN NhanVien nv ON bl.MaNV = nv.MaNV
                               LEFT JOIN PhongBan pb ON nv.MaPB = pb.MaPB
                               LEFT JOIN BangChamCong cc ON bl.MaNV = cc.MaNV AND bl.ThangNam = cc.ThangNam
                               WHERE bl.ThangNam = @ThangNam";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ThangNam", thangNam);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }
}
