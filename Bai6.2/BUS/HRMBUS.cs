using System;
using System.Data;
using HRMSystem.DAL;
using HRMSystem.DTO;

namespace HRMSystem.BUS
{
    public class HRMBUS
    {
        private readonly NhanVienDAL nvDal = new NhanVienDAL();
        private readonly ChamCongLuongDAL ccDal = new ChamCongLuongDAL();

        public DataTable GetPhongBan() => nvDal.GetPhongBan();
        public DataTable GetDanhSachNhanVien() => nvDal.GetDanhSachNhanVien();

        public bool ThemNhanVien(NhanVienDTO nv)
        {
            if (string.IsNullOrWhiteSpace(nv.HoTen) || string.IsNullOrWhiteSpace(nv.CCCD))
                throw new ArgumentException("Họ tên và CCCD không được để trống!");
            if (nv.LuongCoBan <= 0)
                throw new ArgumentException("Lương cơ bản phải lớn hơn 0!");
            return nvDal.InsertNhanVien(nv);
        }

        public bool SuaNhanVien(NhanVienDTO nv) => nvDal.UpdateNhanVien(nv);
        public bool XoaNhanVien(int maNV) => nvDal.DeleteNhanVien(maNV);
        public DataTable TimKiemNhanVien(string keyword, int? maPB) => nvDal.SearchNhanVien(keyword, maPB);

        public DataTable NapBangChamCong(string thangNam) => ccDal.LoadBangChamCong(thangNam);
        public int LuuChamCong() => ccDal.SaveBangChamCong();
        public void ChotLuong(string thangNam) => ccDal.ChotLuongThang(thangNam);
        public DataTable XemBangLuong(string thangNam) => ccDal.GetBangLuong(thangNam);
    }
}
