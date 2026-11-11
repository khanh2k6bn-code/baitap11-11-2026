using System;
using System.Data;
using MySql.Data.MySqlClient;
using HRMSystem.DTO;

namespace HRMSystem.DAL
{
    public class NhanVienDAL
    {
        public DataTable GetPhongBan()
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                string sql = "SELECT MaPB, TenPB, Mota FROM PhongBan";
                MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public DataTable GetDanhSachNhanVien()
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                string sql = @"SELECT nv.MaNV, nv.HoTen, nv.NgaySinh, nv.CCCD, 
                                      nv.MaPB, pb.TenPB, nv.LuongCoBan, nv.HeSoLuong
                               FROM NhanVien nv
                               LEFT JOIN PhongBan pb ON nv.MaPB = pb.MaPB";
                MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Thêm nhân viên dùng Parameterized Query chống SQL Injection 100%
        public bool InsertNhanVien(NhanVienDTO nv)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                string sql = @"INSERT INTO NhanVien (HoTen, NgaySinh, CCCD, MaPB, LuongCoBan, HeSoLuong)
                               VALUES (@HoTen, @NgaySinh, @CCCD, @MaPB, @LuongCoBan, @HeSoLuong)";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@HoTen", nv.HoTen);
                cmd.Parameters.AddWithValue("@NgaySinh", nv.NgaySinh);
                cmd.Parameters.AddWithValue("@CCCD", nv.CCCD);
                cmd.Parameters.AddWithValue("@MaPB", nv.MaPB);
                cmd.Parameters.AddWithValue("@LuongCoBan", nv.LuongCoBan);
                cmd.Parameters.AddWithValue("@HeSoLuong", nv.HeSoLuong);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Sửa thông tin & Chuyển phòng ban
        public bool UpdateNhanVien(NhanVienDTO nv)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                string sql = @"UPDATE NhanVien 
                               SET HoTen = @HoTen, NgaySinh = @NgaySinh, CCCD = @CCCD, 
                                   MaPB = @MaPB, LuongCoBan = @LuongCoBan, HeSoLuong = @HeSoLuong
                               WHERE MaNV = @MaNV";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@MaNV", nv.MaNV);
                cmd.Parameters.AddWithValue("@HoTen", nv.HoTen);
                cmd.Parameters.AddWithValue("@NgaySinh", nv.NgaySinh);
                cmd.Parameters.AddWithValue("@CCCD", nv.CCCD);
                cmd.Parameters.AddWithValue("@MaPB", nv.MaPB);
                cmd.Parameters.AddWithValue("@LuongCoBan", nv.LuongCoBan);
                cmd.Parameters.AddWithValue("@HeSoLuong", nv.HeSoLuong);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Xóa nhân viên
        public bool DeleteNhanVien(int maNV)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                string sql = "DELETE FROM NhanVien WHERE MaNV = @MaNV";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@MaNV", maNV);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Tìm kiếm theo Tên, CCCD hoặc Phòng ban bằng Parameterized Query
        public DataTable SearchNhanVien(string keyword, int? maPB)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                string sql = @"SELECT nv.MaNV, nv.HoTen, nv.NgaySinh, nv.CCCD, 
                                      nv.MaPB, pb.TenPB, nv.LuongCoBan, nv.HeSoLuong
                               FROM NhanVien nv
                               LEFT JOIN PhongBan pb ON nv.MaPB = pb.MaPB
                               WHERE (nv.HoTen LIKE @Kw OR nv.CCCD LIKE @Kw)
                                 AND (@MaPB IS NULL OR nv.MaPB = @MaPB)";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Kw", "%" + keyword + "%");
                cmd.Parameters.AddWithValue("@MaPB", maPB.HasValue ? (object)maPB.Value : DBNull.Value);

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }
}
