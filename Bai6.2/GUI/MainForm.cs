using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using HRMSystem.BUS;
using HRMSystem.DTO;

namespace HRMSystem.GUI
{
    public partial class MainForm : Form
    {
        private readonly HRMBUS bus = new HRMBUS();

        // Controls Tab 1: Nhân sự
        private TextBox txtMaNV, txtHoTen, txtCCCD, txtLuongCB, txtHeSo, txtTimKiem;
        private DateTimePicker dtpNgaySinh;
        private ComboBox cboPhongBan, cboLocPhongBan;
        private DataGridView dgvNhanVien;

        // Controls Tab 2: Chấm công & Lương
        private TextBox txtThangNam;
        private DataGridView dgvChamCong, dgvBangLuong;
        private Button btnLuuChamCong, btnChotLuong;

        public MainForm()
        {
            InitializeComponent();
            BuildUI();
            LoadInitialData();
        }

        // Bổ sung hàm MainForm_Load để tương thích nếu bạn lỡ nhấp đúp vào Form trong Designer
        private void MainForm_Load(object sender, EventArgs e)
        {
        }

        private void BuildUI()
        {
            this.Text = "BÀI 6.2: HỆ THỐNG QUẢN LÝ NHÂN SỰ & TÍNH LƯƠNG (3-TIER & DISCONNECTED MODEL)";
            this.Size = new Size(1100, 720);
            this.StartPosition = FormStartPosition.CenterScreen;

            TabControl tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
            TabPage tabNhanSu = new TabPage("1. Quản lý Hồ sơ Nhân sự (CRUD)");
            TabPage tabChamCong = new TabPage("2. Chấm công (Disconnected) & Tính Lương (Transaction)");

            // ================= TAB 1: NHÂN SỰ =================
            Panel pnlInput = new Panel { Dock = DockStyle.Top, Height = 190, Padding = new Padding(10) };

            pnlInput.Controls.Add(new Label { Text = "Mã NV:", Location = new Point(20, 20), AutoSize = true });
            txtMaNV = new TextBox { Location = new Point(120, 17), Width = 180, ReadOnly = true };
            pnlInput.Controls.Add(txtMaNV);

            pnlInput.Controls.Add(new Label { Text = "Họ tên:", Location = new Point(20, 55), AutoSize = true });
            txtHoTen = new TextBox { Location = new Point(120, 52), Width = 180 };
            pnlInput.Controls.Add(txtHoTen);

            pnlInput.Controls.Add(new Label { Text = "Ngày sinh:", Location = new Point(20, 90), AutoSize = true });
            dtpNgaySinh = new DateTimePicker { Location = new Point(120, 87), Width = 180, Format = DateTimePickerFormat.Short };
            pnlInput.Controls.Add(dtpNgaySinh);

            pnlInput.Controls.Add(new Label { Text = "Số CCCD:", Location = new Point(350, 20), AutoSize = true });
            txtCCCD = new TextBox { Location = new Point(460, 17), Width = 180 };
            pnlInput.Controls.Add(txtCCCD);

            pnlInput.Controls.Add(new Label { Text = "Phòng ban:", Location = new Point(350, 55), AutoSize = true });
            cboPhongBan = new ComboBox { Location = new Point(460, 52), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            pnlInput.Controls.Add(cboPhongBan);

            pnlInput.Controls.Add(new Label { Text = "Lương cơ bản:", Location = new Point(350, 90), AutoSize = true });
            txtLuongCB = new TextBox { Location = new Point(460, 87), Width = 180, Text = "10000000" };
            pnlInput.Controls.Add(txtLuongCB);

            pnlInput.Controls.Add(new Label { Text = "Hệ số lương:", Location = new Point(680, 20), AutoSize = true });
            txtHeSo = new TextBox { Location = new Point(790, 17), Width = 120, Text = "1.0" };
            pnlInput.Controls.Add(txtHeSo);

            // Các nút CRUD
            Button btnThem = new Button { Text = "Thêm NV", Location = new Point(20, 135), Width = 100, Height = 35 };
            btnThem.Click += BtnThem_Click;
            Button btnSua = new Button { Text = "Sửa / Chuyển PB", Location = new Point(130, 135), Width = 140, Height = 35 };
            btnSua.Click += BtnSua_Click;
            Button btnXoa = new Button { Text = "Xóa NV", Location = new Point(280, 135), Width = 100, Height = 35 };
            btnXoa.Click += BtnXoa_Click;

            // Tìm kiếm
            pnlInput.Controls.Add(new Label { Text = "Tìm (Tên/CCCD):", Location = new Point(410, 142), AutoSize = true });
            txtTimKiem = new TextBox { Location = new Point(540, 139), Width = 160 };
            cboLocPhongBan = new ComboBox { Location = new Point(710, 139), Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            Button btnTim = new Button { Text = "Tìm kiếm", Location = new Point(880, 135), Width = 100, Height = 35 };
            btnTim.Click += BtnTim_Click;

            pnlInput.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa, txtTimKiem, cboLocPhongBan, btnTim });

            dgvNhanVien = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true
            };
            dgvNhanVien.CellClick += DgvNhanVien_CellClick;

            tabNhanSu.Controls.Add(dgvNhanVien);
            tabNhanSu.Controls.Add(pnlInput);

            // ================= TAB 2: CHẤM CÔNG & TÍNH LƯƠNG =================
            Panel pnlTopCC = new Panel { Dock = DockStyle.Top, Height = 65 };
            pnlTopCC.Controls.Add(new Label { Text = "Tháng/Năm (MM/yyyy):", Location = new Point(20, 20), AutoSize = true });
            txtThangNam = new TextBox { Location = new Point(200, 17), Width = 100, Text = "11/2026" };
            pnlTopCC.Controls.Add(txtThangNam);

            Button btnNapCC = new Button { Text = "1. Nạp Chấm Công", Location = new Point(320, 13), Width = 160, Height = 36 };
            btnNapCC.Click += BtnNapCC_Click;

            btnLuuChamCong = new Button { Text = "2. Lưu Chấm Công (Disconnected)", Location = new Point(500, 13), Width = 240, Height = 36 };
            btnLuuChamCong.Click += BtnLuuChamCong_Click;

            btnChotLuong = new Button { Text = "3. Chốt Lương Tháng (Transaction)", Location = new Point(760, 13), Width = 260, Height = 36 };
            btnChotLuong.Click += BtnChotLuong_Click;

            pnlTopCC.Controls.AddRange(new Control[] { btnNapCC, btnLuuChamCong, btnChotLuong });

            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 260 };

            GroupBox grpCC = new GroupBox { Text = "Bảng Chấm Công Tháng (Nhập trực tiếp Số Ngày Công & Giờ OT trên lưới)", Dock = DockStyle.Fill };
            dgvChamCong = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false };
            grpCC.Controls.Add(dgvChamCong);

            GroupBox grpLuong = new GroupBox { Text = "Bảng Lương Chi Tiết Đã Chốt", Dock = DockStyle.Fill };
            dgvBangLuong = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true };
            grpLuong.Controls.Add(dgvBangLuong);

            split.Panel1.Controls.Add(grpCC);
            split.Panel2.Controls.Add(grpLuong);

            tabChamCong.Controls.Add(split);
            tabChamCong.Controls.Add(pnlTopCC);

            tabControl.TabPages.Add(tabNhanSu);
            tabControl.TabPages.Add(tabChamCong);
            this.Controls.Add(tabControl);
        }

        private void LoadInitialData()
        {
            try
            {
                DataTable dtPB = bus.GetPhongBan();
                cboPhongBan.DataSource = dtPB.Copy();
                cboPhongBan.DisplayMember = "TenPB";
                cboPhongBan.ValueMember = "MaPB";

                DataTable dtLoc = dtPB.Copy();
                DataRow allRow = dtLoc.NewRow();
                allRow["MaPB"] = DBNull.Value;
                allRow["TenPB"] = "-- Tất cả phòng ban --";
                dtLoc.Rows.InsertAt(allRow, 0);
                cboLocPhongBan.DataSource = dtLoc;
                cboLocPhongBan.DisplayMember = "TenPB";
                cboLocPhongBan.ValueMember = "MaPB";

                dgvNhanVien.DataSource = bus.GetDanhSachNhanVien();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvNhanVien_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvNhanVien.Rows[e.RowIndex];
                txtMaNV.Text = row.Cells["MaNV"].Value?.ToString();
                txtHoTen.Text = row.Cells["HoTen"].Value?.ToString();
                if (DateTime.TryParse(row.Cells["NgaySinh"].Value?.ToString(), out DateTime ns))
                    dtpNgaySinh.Value = ns;
                txtCCCD.Text = row.Cells["CCCD"].Value?.ToString();
                cboPhongBan.SelectedValue = row.Cells["MaPB"].Value;
                txtLuongCB.Text = row.Cells["LuongCoBan"].Value?.ToString();
                txtHeSo.Text = row.Cells["HeSoLuong"].Value?.ToString();
            }
        }

        private void BtnThem_Click(object sender, EventArgs e)
        {
            try
            {
                NhanVienDTO nv = new NhanVienDTO
                {
                    HoTen = txtHoTen.Text.Trim(),
                    NgaySinh = dtpNgaySinh.Value,
                    CCCD = txtCCCD.Text.Trim(),
                    MaPB = Convert.ToInt32(cboPhongBan.SelectedValue),
                    LuongCoBan = decimal.Parse(txtLuongCB.Text),
                    HeSoLuong = float.Parse(txtHeSo.Text)
                };
                if (bus.ThemNhanVien(nv))
                {
                    MessageBox.Show("Thêm nhân viên thành công!");
                    dgvNhanVien.DataSource = bus.GetDanhSachNhanVien();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thêm nhân viên: " + ex.Message, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnSua_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtMaNV.Text)) return;
                NhanVienDTO nv = new NhanVienDTO
                {
                    MaNV = int.Parse(txtMaNV.Text),
                    HoTen = txtHoTen.Text.Trim(),
                    NgaySinh = dtpNgaySinh.Value,
                    CCCD = txtCCCD.Text.Trim(),
                    MaPB = Convert.ToInt32(cboPhongBan.SelectedValue),
                    LuongCoBan = decimal.Parse(txtLuongCB.Text),
                    HeSoLuong = float.Parse(txtHeSo.Text)
                };
                if (bus.SuaNhanVien(nv))
                {
                    MessageBox.Show("Cập nhật thông tin / chuyển phòng ban thành công!");
                    dgvNhanVien.DataSource = bus.GetDanhSachNhanVien();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật: " + ex.Message, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaNV.Text)) return;
            if (MessageBox.Show("Bạn có chắc muốn xóa nhân viên này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    if (bus.XoaNhanVien(int.Parse(txtMaNV.Text)))
                    {
                        MessageBox.Show("Đã xóa nhân viên!");
                        dgvNhanVien.DataSource = bus.GetDanhSachNhanVien();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xóa: " + ex.Message);
                }
            }
        }

        private void BtnTim_Click(object sender, EventArgs e)
        {
            int? maPB = null;
            if (cboLocPhongBan.SelectedValue != null && cboLocPhongBan.SelectedValue != DBNull.Value)
                maPB = Convert.ToInt32(cboLocPhongBan.SelectedValue);

            dgvNhanVien.DataSource = bus.TimKiemNhanVien(txtTimKiem.Text.Trim(), maPB);
        }

        // ================= SỰ KIỆN TAB 2: CHẤM CÔNG & LƯƠNG =================
        private void BtnNapCC_Click(object sender, EventArgs e)
        {
            try
            {
                string thangNam = txtThangNam.Text.Trim();
                DataTable dt = bus.NapBangChamCong(thangNam);
                dgvChamCong.DataSource = dt;

                dgvChamCong.Columns["MaCong"].ReadOnly = true;
                dgvChamCong.Columns["MaNV"].ReadOnly = true;
                dgvChamCong.Columns["ThangNam"].ReadOnly = true;
                dgvChamCong.Columns["DaChot"].ReadOnly = true;

                bool isLocked = dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["DaChot"]) == 1;
                dgvChamCong.ReadOnly = isLocked;
                btnLuuChamCong.Enabled = !isLocked;

                dgvBangLuong.DataSource = bus.XemBangLuong(thangNam);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nạp chấm công: " + ex.Message);
            }
        }

        private void BtnLuuChamCong_Click(object sender, EventArgs e)
        {
            try
            {
                dgvChamCong.EndEdit();
                int rowsUpdated = bus.LuuChamCong();
                MessageBox.Show($"Đã đồng bộ thành công {rowsUpdated} dòng chấm công về CSDL bằng CommandBuilder & adapter.Update()!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu chấm công (Kiểm tra ràng buộc Số ngày công <= 31): " + ex.Message, "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnChotLuong_Click(object sender, EventArgs e)
        {
            try
            {
                string thangNam = txtThangNam.Text.Trim();
                bus.ChotLuong(thangNam);
                MessageBox.Show($"Đã tính lương, chốt sổ và khóa bảng chấm công tháng {thangNam} thành công (Transaction Commit)!", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);

                BtnNapCC_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Giao dịch bị hủy (Transaction Rollback): " + ex.Message, "Lỗi Transaction", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
