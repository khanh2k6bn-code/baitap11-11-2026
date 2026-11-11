using System;
using System.Data;
using MySql.Data.MySqlClient;

public class SanPhamDAL
{
    private string connString = "Server=localhost;Database=SalesDB;Uid=root;Pwd=;";

    public DataTable TimKiemSanPham(string keyword)
    {
        using (MySqlConnection conn = new MySqlConnection(connString))
        {
            string query = "SELECT * FROM SanPham WHERE MaSP LIKE @kw OR TenSP LIKE @kw";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@kw", "%" + keyword + "%");
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    public bool ThemSanPham(string maSP, string tenSP, decimal giaBan, int soLuongTon)
    {
        using (MySqlConnection conn = new MySqlConnection(connString))
        {
            conn.Open();
            string query = "INSERT INTO SanPham (MaSP, TenSP, GiaBan, SoLuongTon) VALUES (@MaSP, @TenSP, @GiaBan, @SoLuongTon)";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@MaSP", maSP);
            cmd.Parameters.AddWithValue("@TenSP", tenSP);
            cmd.Parameters.AddWithValue("@GiaBan", giaBan);
            cmd.Parameters.AddWithValue("@SoLuongTon", soLuongTon);
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    public bool SuaSanPham(string maSP, string tenSP, decimal giaBan, int soLuongTon)
    {
        using (MySqlConnection conn = new MySqlConnection(connString))
        {
            conn.Open();
            string query = "UPDATE SanPham SET TenSP = @TenSP, GiaBan = @GiaBan, SoLuongTon = @SoLuongTon WHERE MaSP = @MaSP";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@MaSP", maSP);
            cmd.Parameters.AddWithValue("@TenSP", tenSP);
            cmd.Parameters.AddWithValue("@GiaBan", giaBan);
            cmd.Parameters.AddWithValue("@SoLuongTon", soLuongTon);
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    public bool XoaSanPham(string maSP)
    {
        using (MySqlConnection conn = new MySqlConnection(connString))
        {
            conn.Open();
            string query = "DELETE FROM SanPham WHERE MaSP = @MaSP";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@MaSP", maSP);
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    // Xử lý Transaction thanh toán, trừ kho, cộng điểm và Commit
    public bool ThucHienThanhToan(int maNV, int maKH, decimal tongTien, DataTable dtChiTietGioHang, out string errorMessage)
    {
        errorMessage = "";
        using (MySqlConnection conn = new MySqlConnection(connString))
        {
            conn.Open();
            MySqlTransaction trans = conn.BeginTransaction();
            try
            {
                // 1. Chèn hóa đơn
                string queryHD = "INSERT INTO HoaDon (NgayLap, MaNV, MaKH, TongTien) VALUES (NOW(), @MaNV, @MaKH, @TongTien)";
                MySqlCommand cmdHD = new MySqlCommand(queryHD, conn, trans);
                cmdHD.Parameters.AddWithValue("@MaNV", maNV);
                cmdHD.Parameters.AddWithValue("@MaKH", maKH);
                cmdHD.Parameters.AddWithValue("@TongTien", tongTien);
                cmdHD.ExecuteNonQuery();

                long maHD = cmdHD.LastInsertedId;

                // 2. Chèn chi tiết và trừ tồn kho
                foreach (DataRow row in dtChiTietGioHang.Rows)
                {
                    string maSP = row["MaSP"].ToString();
                    int soLuongMua = Convert.ToInt32(row["SoLuong"]);
                    decimal dongGia = Convert.ToDecimal(row["DongGia"]);

                    // Kiểm tra tồn kho
                    string queryCheckTon = "SELECT SoLuongTon FROM SanPham WHERE MaSP = @MaSP";
                    MySqlCommand cmdCheck = new MySqlCommand(queryCheckTon, conn, trans);
                    cmdCheck.Parameters.AddWithValue("@MaSP", maSP);
                    int tonKho = Convert.ToInt32(cmdCheck.ExecuteScalar());

                    if (tonKho < soLuongMua)
                    {
                        throw new Exception($"Sản phẩm {maSP} không đủ số lượng tồn kho (Còn lại: {tonKho})!");
                    }

                    // Chèn chi tiết hóa đơn
                    string queryCT = "INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, DongGia) VALUES (@MaHD, @MaSP, @SoLuong, @DongGia)";
                    MySqlCommand cmdCT = new MySqlCommand(queryCT, conn, trans);
                    cmdCT.Parameters.AddWithValue("@MaHD", maHD);
                    cmdCT.Parameters.AddWithValue("@MaSP", maSP);
                    cmdCT.Parameters.AddWithValue("@SoLuong", soLuongMua);
                    cmdCT.Parameters.AddWithValue("@DongGia", dongGia);
                    cmdCT.ExecuteNonQuery();

                    // Trừ tồn kho
                    string queryUpdateTon = "UPDATE SanPham SET SoLuongTon = SoLuongTon - @SoLuong WHERE MaSP = @MaSP";
                    MySqlCommand cmdUpdate = new MySqlCommand(queryUpdateTon, conn, trans);
                    cmdUpdate.Parameters.AddWithValue("@SoLuong", soLuongMua);
                    cmdUpdate.Parameters.AddWithValue("@MaSP", maSP);
                    cmdUpdate.ExecuteNonQuery();
                }

                // 3. Cộng điểm tích lũy khách hàng
                string queryKH = "UPDATE KhachHang SET DiemTichLuy = DiemTichLuy + @DiemThem WHERE MaKH = @MaKH";
                MySqlCommand cmdKH = new MySqlCommand(queryKH, conn, trans);
                cmdKH.Parameters.AddWithValue("@DiemThem", (int)(tongTien / 100000));
                cmdKH.Parameters.AddWithValue("@MaKH", maKH);
                cmdKH.ExecuteNonQuery();

                // 4. Xác nhận lưu vĩnh viễn giao dịch
                trans.Commit();
                return true;
            }
            catch (Exception ex)
            {
                // Hoàn tác toàn bộ nếu có lỗi
                trans.Rollback();
                errorMessage = ex.Message;
                return false;
            }
        }
    }

    // Báo cáo thống kê
    public DataTable ThongKeTop5SanPham()
    {
        using (MySqlConnection conn = new MySqlConnection(connString))
        {
            string query = @"SELECT sp.MaSP, sp.TenSP, SUM(ct.SoLuong) AS TongBan 
                             FROM ChiTietHoaDon ct 
                             JOIN SanPham sp ON ct.MaSP = sp.MaSP 
                             GROUP BY sp.MaSP, sp.TenSP 
                             ORDER BY TongBan DESC 
                             LIMIT 5";
            MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    public decimal TongDoanhThu()
    {
        using (MySqlConnection conn = new MySqlConnection(connString))
        {
            conn.Open();
            string query = "SELECT IFNULL(SUM(TongTien), 0) FROM HoaDon";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }
    }
}