using System;
using System.Windows.Forms;

namespace Lab_Basic_Command
{
    public class MainForm : Form
    {
        private readonly TextBox id = new TextBox { ReadOnly = true };
        private readonly TextBox name = new TextBox();
        private readonly NumericUpDown capacity = new NumericUpDown { Minimum = 1, Maximum = 500, Value = 4 };
        private readonly ComboBox status = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly DataGridView grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect };

        public MainForm()
        {
            Text = "Quản lý nhà hàng";
            Width = 1000;
            Height = 620;
            status.Items.AddRange(new object[] { "Trống", "Có khách" }); status.SelectedIndex = 0;
            var menu = new MenuStrip();
            var manage = new ToolStripMenuItem("Chức năng");
            manage.DropDownItems.Add("Món ăn", null, (s, e) => new CategoryForm().Show(this));
            manage.DropDownItems.Add("Hóa đơn", null, (s, e) => new BillsForm().Show(this));
            manage.DropDownItems.Add("Tài khoản", null, (s, e) => new AccountManagerForm().Show(this));
            manage.DropDownItems.Add("Nhật ký hóa đơn", null, (s, e) => new BillLogForm().Show(this));
            menu.Items.Add(manage); MainMenuStrip = menu; Controls.Add(menu);
            var editor = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 78, Padding = new Padding(6) };
            Add(editor, "Mã", id); Add(editor, "Tên bàn", name); Add(editor, "Sức chứa", capacity); Add(editor, "Trạng thái", status);
            var add = new Button { Text = "Thêm", Width = 70 }; var save = new Button { Text = "Lưu", Width = 70 }; var delete = new Button { Text = "Xóa", Width = 70 }; var bill = new Button { Text = "Hóa đơn hiện tại", Width = 120 };
            editor.Controls.Add(add); editor.Controls.Add(save); editor.Controls.Add(delete); editor.Controls.Add(bill);
            Controls.Add(grid); Controls.Add(editor);
            add.Click += (s, e) => { id.Clear(); name.Clear(); capacity.Value = 4; status.SelectedIndex = 0; };
            save.Click += (s, e) => SaveTable(); delete.Click += (s, e) => DeleteTable(); bill.Click += (s, e) => OpenCurrentBill();
            grid.CellClick += (s, e) => SelectTable(e.RowIndex);
            var context = new ContextMenuStrip(); context.Items.Add("Xóa bàn", null, (s, e) => DeleteTable()); context.Items.Add("Xem danh mục hóa đơn", null, (s, e) => OpenTableBills()); context.Items.Add("Xem nhật ký hóa đơn", null, (s, e) => new BillLogForm().ShowDialog(this)); grid.ContextMenuStrip = context;
            Load += (s, e) => LoadTables();
        }

        private static void Add(Control parent, string label, Control control) { parent.Controls.Add(new Label { Text = label, AutoSize = true, Padding = new Padding(4, 7, 0, 0) }); parent.Controls.Add(control); }

        private void LoadTables() { Database.Try(() => grid.DataSource = Database.Query("SELECT ID, Name, Status, Capacity FROM [Table] ORDER BY ID"), this); }
        private void SelectTable(int rowIndex) { if (rowIndex < 0 || rowIndex >= grid.Rows.Count) return; var row = grid.Rows[rowIndex]; id.Text = Cell(row, "ID"); name.Text = Cell(row, "Name"); capacity.Value = Convert.ToDecimal(Cell(row, "Capacity")); status.SelectedIndex = Cell(row, "Status") == "1" ? 1 : 0; }
        private void SaveTable()
        {
            if (string.IsNullOrWhiteSpace(name.Text)) { MessageBox.Show("Tên bàn không được trống."); return; }
            if (Database.Try(() => { if (string.IsNullOrWhiteSpace(id.Text)) Database.Execute("INSERT INTO [Table](Name,Status,Capacity) VALUES(@name,@status,@capacity)", Database.Parameter("@name", name.Text), Database.Parameter("@status", status.SelectedIndex), Database.Parameter("@capacity", capacity.Value)); else Database.Execute("UPDATE [Table] SET Name=@name, Status=@status, Capacity=@capacity WHERE ID=@id", Database.Parameter("@name", name.Text), Database.Parameter("@status", status.SelectedIndex), Database.Parameter("@capacity", capacity.Value), Database.Parameter("@id", id.Text)); LoadTables(); }, this)) { }
        }
        private void DeleteTable()
        {
            if (string.IsNullOrWhiteSpace(id.Text)) return;
            if (MessageBox.Show("Xóa bàn đang chọn?", "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            if (Database.Try(() => Database.Execute("DELETE FROM [Table] WHERE ID=@id", Database.Parameter("@id", id.Text)), this)) { id.Clear(); LoadTables(); }
        }
        private void OpenCurrentBill() { if (!string.IsNullOrWhiteSpace(id.Text)) new TableBillsForm(Convert.ToInt32(id.Text)).ShowDialog(this); }
        private void OpenTableBills() { if (!string.IsNullOrWhiteSpace(id.Text)) new TableBillsForm(Convert.ToInt32(id.Text)).ShowDialog(this); }
        private static string Cell(DataGridViewRow row, string name) { return row.Cells[name].Value == null ? "" : row.Cells[name].Value.ToString(); }
    }
}
