using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace RetailSalesSystem
{
    public partial class Form1 : Form
    {
        private SanPhamBUS bus = new SanPhamBUS();
        private DataGridView dgvSanPham, dgvGioHang;
        private TextBox txtMaSP, txtTenSP, txtGiaBan, txtSoLuongTon, txtTimKiem, txtMaScan, txtSoLuongMua, txtMaKH, txtMaNV;
        private Button btnThem, btnSua, btnXoa, btnTimKiem, btnThanhToan, btnThongKe;
        private Label lblTongTien;
        private DataTable dtGioHang;

        public Form1()
        {
            InitializeComponent();
            KhoiTaoGiaoDienBanHang();
        }

        private void KhoiTaoGiaoDienBanHang()
        {
            this.Text = "Hệ thống Quản lý Bán hàng & Siêu thị Mini - 3-Tier Architecture";
            this.Size = new Size(1020, 680);
            this.StartPosition = FormStartPosition.CenterScreen;

            // --- PHẦN 1: QUẢN LÝ DANH MỤC SẢN PHẨM ---
            Label lbl1 = new Label() { Text = "1. QUẢN LÝ DANH MỤC SẢN PHẨM", Font = new Font("Arial", 11, FontStyle.Bold), ForeColor = Color.DarkBlue, Location = new Point(20, 15), AutoSize = true };
            this.Controls.Add(lbl1);

            CreateLabelAndTextBox("Mã SP:", 20, 50, out txtMaSP);
            CreateLabelAndTextBox("Tên SP:", 20, 90, out txtTenSP);
            CreateLabelAndTextBox("Giá bán:", 340, 50, out txtGiaBan);
            CreateLabelAndTextBox("Tồn kho:", 340, 90, out txtSoLuongTon);

            btnThem = CreateButton("Thêm", 20, 130, Color.LightBlue, BtnThem_Click);
            btnSua = CreateButton("Sửa", 120, 130, Color.LightYellow, BtnSua_Click);
            btnXoa = CreateButton("Xóa", 220, 130, Color.LightPink, BtnXoa_Click);

            txtTimKiem = new TextBox() { Location = new Point(340, 132), Size = new Size(140, 25) };
            this.Controls.Add(txtTimKiem);
            btnTimKiem = CreateButton("Tìm kiếm (%)", 490, 130, Color.LightGray, BtnTimKiem_Click);
            btnTimKiem.Size = new Size(100, 28);

            dgvSanPham = new DataGridView() { Location = new Point(20, 170), Size = new Size(570, 400), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            dgvSanPham.CellClick += DgvSanPham_CellClick;
            this.Controls.Add(dgvSanPham);

            // Nút Thống kê
            btnThongKe = CreateButton("Thống kê Doanh Thu & Top 5", 20, 585, Color.Orange, BtnThongKe_Click);
            btnThongKe.Size = new Size(200, 35);
            btnThongKe.Font = new Font("Arial", 9, FontStyle.Bold);

            // --- PHẦN 2: MÀN HÌNH BÁN HÀNG & TRANSACTION ---
            Label lbl2 = new Label() { Text = "2. MÀN HÌNH BÁN HÀNG & TRANSACTION", Font = new Font("Arial", 11, FontStyle.Bold), ForeColor = Color.DarkGreen, Location = new Point(610, 15), AutoSize = true };
            this.Controls.Add(lbl2);

            Label lblNV = new Label() { Text = "Mã Nhân Viên:", Location = new Point(610, 53), Size = new Size(95, 20) };
            this.Controls.Add(lblNV);
            txtMaNV = new TextBox() { Location = new Point(720, 50), Size = new Size(100, 25), Text = "1" };
            this.Controls.Add(txtMaNV);

            Label lblKH = new Label() { Text = "Mã KH:", Location = new Point(835, 53), Size = new Size(50, 20) };
            this.Controls.Add(lblKH);
            txtMaKH = new TextBox() { Location = new Point(890, 50), Size = new Size(90, 25), Text = "1" };
            this.Controls.Add(txtMaKH);

            Label lblScan = new Label() { Text = "Quét/Nhập mã SP:", Location = new Point(610, 88), Size = new Size(105, 20) };
            this.Controls.Add(lblScan);
            txtMaScan = new TextBox() { Location = new Point(720, 85), Size = new Size(170, 25) };
            txtMaScan.KeyDown += TxtMaScan_KeyDown;
            this.Controls.Add(txtMaScan);

            Label lblSL = new Label() { Text = "SL:", Location = new Point(900, 88), Size = new Size(25, 20) };
            this.Controls.Add(lblSL);
            txtSoLuongMua = new TextBox() { Location = new Point(930, 85), Size = new Size(50, 25), Text = "1" };
            this.Controls.Add(txtSoLuongMua);

            dgvGioHang = new DataGridView() { Location = new Point(610, 125), Size = new Size(370, 360), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true };
            this.Controls.Add(dgvGioHang);

            dtGioHang = new DataTable();
            dtGioHang.Columns.Add("MaSP", typeof(string));
            dtGioHang.Columns.Add("SoLuong", typeof(int));
            dtGioHang.Columns.Add("DongGia", typeof(decimal));
            dtGioHang.Columns.Add("ThanhTien", typeof(decimal), "SoLuong * DongGia");
            dgvGioHang.DataSource = dtGioHang;

            lblTongTien = new Label() { Text = "Tổng tiền: 0 VNĐ", Font = new Font("Arial", 12, FontStyle.Bold), ForeColor = Color.Red, Location = new Point(610, 500), AutoSize = true };
            this.Controls.Add(lblTongTien);

            btnThanhToan = CreateButton("THANH TOÁN (MYSQL TRANSACTION)", 610, 535, Color.LightGreen, BtnThanhToan_Click);
            btnThanhToan.Size = new Size(370, 45);
            btnThanhToan.Font = new Font("Arial", 10, FontStyle.Bold);
        }

        private void CreateLabelAndTextBox(string text, int x, int y, out TextBox txt)
        {
            Label lbl = new Label() { Text = text, Location = new Point(x, y), Size = new Size(60, 20) };
            this.Controls.Add(lbl);
            txt = new TextBox() { Location = new Point(x + 65, y), Size = new Size(175, 25) };
            this.Controls.Add(txt);
        }

        private Button CreateButton(string text, int x, int y, Color color, EventHandler onClick)
        {
            Button btn = new Button() { Text = text, Location = new Point(x, y), Size = new Size(90, 30), BackColor = color };
            btn.Click += onClick;
            this.Controls.Add(btn);
            return btn;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadDanhSach("");
        }

        private void LoadDanhSach(string keyword)
        {
            dgvSanPham.DataSource = bus.TimKiemSanPham(keyword);
        }

        private void DgvSanPham_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvSanPham.Rows[e.RowIndex];
                txtMaSP.Text = row.Cells["MaSP"].Value.ToString();
                txtTenSP.Text = row.Cells["TenSP"].Value.ToString();
                txtGiaBan.Text = row.Cells["GiaBan"].Value.ToString();
                txtSoLuongTon.Text = row.Cells["SoLuongTon"].Value.ToString();
            }
        }

        private void BtnThem_Click(object sender, EventArgs e)
        {
            try
            {
                if (bus.ThemSanPham(txtMaSP.Text.Trim(), txtTenSP.Text.Trim(), Convert.ToDecimal(txtGiaBan.Text), Convert.ToInt32(txtSoLuongTon.Text)))
                {
                    MessageBox.Show("Thêm thành công!");
                    LoadDanhSach("");
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void BtnSua_Click(object sender, EventArgs e)
        {
            try
            {
                if (bus.SuaSanPham(txtMaSP.Text.Trim(), txtTenSP.Text.Trim(), Convert.ToDecimal(txtGiaBan.Text), Convert.ToInt32(txtSoLuongTon.Text)))
                {
                    MessageBox.Show("Cập nhật thành công!");
                    LoadDanhSach("");
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            try
            {
                if (bus.XoaSanPham(txtMaSP.Text.Trim()))
                {
                    MessageBox.Show("Xóa thành công!");
                    LoadDanhSach("");
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void BtnTimKiem_Click(object sender, EventArgs e)
        {
            LoadDanhSach(txtTimKiem.Text.Trim());
        }

        private void TxtMaScan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string maSP = txtMaScan.Text.Trim();
                if (string.IsNullOrEmpty(maSP)) return;

                DataTable dt = bus.TimKiemSanPham(maSP);
                bool found = false;

                foreach (DataRow row in dt.Rows)
                {
                    if (row["MaSP"].ToString().Equals(maSP, StringComparison.OrdinalIgnoreCase))
                    {
                        decimal giaBan = Convert.ToDecimal(row["GiaBan"]);
                        int soLuongMua = 1;
                        int.TryParse(txtSoLuongMua.Text, out soLuongMua);
                        if (soLuongMua <= 0) soLuongMua = 1;

                        bool daCo = false;
                        foreach (DataRow cartRow in dtGioHang.Rows)
                        {
                            if (cartRow["MaSP"].ToString().Equals(maSP, StringComparison.OrdinalIgnoreCase))
                            {
                                cartRow["SoLuong"] = Convert.ToInt32(cartRow["SoLuong"]) + soLuongMua;
                                daCo = true;
                                break;
                            }
                        }

                        if (!daCo)
                        {
                            dtGioHang.Rows.Add(maSP, soLuongMua, giaBan);
                        }

                        TinhTongTienGioHang();
                        txtMaScan.Clear();
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    MessageBox.Show("Không tìm thấy mã sản phẩm: " + maSP, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void TinhTongTienGioHang()
        {
            decimal tong = 0;
            foreach (DataRow row in dtGioHang.Rows)
            {
                tong += Convert.ToDecimal(row["ThanhTien"]);
            }
            lblTongTien.Text = "Tổng tiền: " + tong.ToString("N0") + " VNĐ";
        }

        private void BtnThanhToan_Click(object sender, EventArgs e)
        {
            if (dtGioHang.Rows.Count == 0)
            {
                MessageBox.Show("Giỏ hàng đang trống, vui lòng quét sản phẩm trước khi thanh toán!");
                return;
            }

            int maNV = 1;
            int.TryParse(txtMaNV.Text.Trim(), out maNV);

            int maKH = 1;
            int.TryParse(txtMaKH.Text.Trim(), out maKH);

            decimal tongTien = 0;
            foreach (DataRow row in dtGioHang.Rows)
            {
                tongTien += Convert.ToDecimal(row["ThanhTien"]);
            }

            string error;
            bool ketQua = bus.ThucHienThanhToan(maNV, maKH, tongTien, dtGioHang, out error);

            if (ketQua)
            {
                MessageBox.Show("Thanh toán thành công!\n- Đã ghi nhận Hóa Đơn & Chi Tiết Hóa Đơn.\n- Đã trừ số lượng tồn kho.\n- Đã cộng điểm tích lũy thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dtGioHang.Clear();
                TinhTongTienGioHang();
                LoadDanhSach("");
            }
            else
            {
                MessageBox.Show("Thanh toán thất bại (Đã kích hoạt Rollback hoàn tác dữ liệu):\n" + error, "Lỗi Transaction", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnThongKe_Click(object sender, EventArgs e)
        {
            try
            {
                decimal doanhThu = bus.TongDoanhThu();
                DataTable dtTop5 = bus.ThongKeTop5SanPham();

                string msg = $"--- BÁO CÁO THỐNG KÊ KINH DOANH ---\n\n";
                msg += $"Tổng doanh thu toàn hệ thống: {doanhThu:N0} VNĐ\n\n";
                msg += $"Top sản phẩm bán chạy:\n";

                foreach (DataRow row in dtTop5.Rows)
                {
                    msg += $"- {row["TenSP"]} (Mã: {row["MaSP"]}): Đã bán {row["TongBan"]} sản phẩm\n";
                }

                MessageBox.Show(msg, "Thống kê Doanh Thu & Top 5", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lấy dữ liệu thống kê: " + ex.Message);
            }
        }
    }
}
