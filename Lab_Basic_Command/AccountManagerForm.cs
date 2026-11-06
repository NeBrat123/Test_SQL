using System;
using System.Data;
using System.Windows.Forms;

namespace Lab_Basic_Command
{
    public class AccountManagerForm : Form
    {
        private readonly TextBox account = new TextBox();
        private readonly TextBox password = new TextBox();
        private readonly TextBox fullName = new TextBox();
        private readonly TextBox email = new TextBox();
        private readonly ComboBox role = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly CheckBox active = new CheckBox { Text = "Đang hoạt động" };
        private readonly DataGridView grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
        private readonly ComboBox filterRole = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
        private readonly CheckBox filterActive = new CheckBox { Text = "Chỉ Active" };

        public AccountManagerForm()
        {
            Text = "Quản lý tài khoản";
            Width = 1000;
            Height = 620;
            var editor = new TableLayoutPanel { Dock = DockStyle.Top, Height = 125, ColumnCount = 4, RowCount = 3, Padding = new Padding(6) };
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90)); editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90)); editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            Add(editor, "Tài khoản", account, 0, 0); Add(editor, "Mật khẩu", password, 2, 0);
            Add(editor, "Họ tên", fullName, 0, 1); Add(editor, "Email", email, 2, 1);
            editor.Controls.Add(active, 0, 2); editor.SetColumnSpan(active, 2);
            var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(6) };
            var save = new Button { Text = "Thêm / Cập nhật", Width = 120 };
            var reset = new Button { Text = "Reset mật khẩu", Width = 120 };
            actions.Controls.Add(new Label { Text = "Vai trò", AutoSize = true, Padding = new Padding(0, 7, 0, 0) }); actions.Controls.Add(role);
            actions.Controls.Add(save); actions.Controls.Add(reset); actions.Controls.Add(filterActive);
            actions.Controls.Add(filterRole);
            Controls.Add(grid); Controls.Add(actions); Controls.Add(editor);
            save.Click += (s, e) => SaveAccount(); reset.Click += (s, e) => ResetPassword();
            filterActive.CheckedChanged += (s, e) => LoadAccounts(); filterRole.SelectedIndexChanged += (s, e) => LoadAccounts();
            grid.CellClick += (s, e) => SelectAccount(e.RowIndex);
            var menu = new ContextMenuStrip();
            menu.Items.Add("Xóa tài khoản", null, (s, e) => DeactivateAccount());
            menu.Items.Add("Xem danh sách vai trò", null, (s, e) => ViewRoles());
            grid.ContextMenuStrip = menu;
            Load += (s, e) => { LoadRoles(); LoadAccounts(); };
        }

        private static void Add(TableLayoutPanel panel, string label, Control control, int column, int row)
        {
            panel.Controls.Add(new Label { Text = label, AutoSize = true, Padding = new Padding(0, 7, 0, 0) }, column, row);
            panel.Controls.Add(control, column + 1, row);
        }

        private void LoadRoles()
        {
            Database.Try(() =>
            {
                var table = Database.Query("SELECT ID, RoleName FROM Role ORDER BY RoleName");
                role.DataSource = table.Copy(); role.DisplayMember = "RoleName"; role.ValueMember = "ID";
                var all = table.Copy(); var item = all.NewRow(); item["ID"] = 0; item["RoleName"] = "Tất cả"; all.Rows.InsertAt(item, 0);
                filterRole.DataSource = all; filterRole.DisplayMember = "RoleName"; filterRole.ValueMember = "ID";
            }, this);
        }

        private void LoadAccounts()
        {
            var sql = @"SELECT a.AccountName, a.FullName, a.Email, a.DateCreated,
                CASE WHEN EXISTS (SELECT 1 FROM RoleAccount r WHERE r.AccountName=a.AccountName AND r.Actived=1) THEN 1 ELSE 0 END AS Active,
                ISNULL(STUFF((SELECT ', ' + r2.RoleName FROM RoleAccount ra2 INNER JOIN Role r2 ON r2.ID=ra2.RoleID WHERE ra2.AccountName=a.AccountName AND ra2.Actived=1 FOR XML PATH('')),1,2,''),'') AS Roles
                FROM Account a WHERE (@active=0 OR EXISTS (SELECT 1 FROM RoleAccount r WHERE r.AccountName=a.AccountName AND r.Actived=1))
                AND (@role=0 OR EXISTS (SELECT 1 FROM RoleAccount r WHERE r.AccountName=a.AccountName AND r.RoleID=@role AND r.Actived=1)) ORDER BY a.AccountName";
            int selectedRole = filterRole.SelectedValue == null ? 0 : Convert.ToInt32(filterRole.SelectedValue);
            Database.Try(() => grid.DataSource = Database.Query(sql, Database.Parameter("@active", filterActive.Checked ? 1 : 0), Database.Parameter("@role", selectedRole)), this);
        }

        private void SelectAccount(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= grid.Rows.Count) return;
            var row = grid.Rows[rowIndex]; account.Text = TextOf(row, "AccountName"); fullName.Text = TextOf(row, "FullName"); email.Text = TextOf(row, "Email"); active.Checked = TextOf(row, "Active") == "1"; password.Clear();
        }

        private void SaveAccount()
        {
            if (string.IsNullOrWhiteSpace(account.Text) || string.IsNullOrWhiteSpace(fullName.Text)) { MessageBox.Show("Tài khoản và họ tên không được trống."); return; }
            Database.Try(() =>
            {
                var exists = Database.Scalar("SELECT COUNT(*) FROM Account WHERE AccountName=@account", Database.Parameter("@account", account.Text));
                if (Convert.ToInt32(exists) == 0)
                {
                    if (string.IsNullOrWhiteSpace(password.Text)) { MessageBox.Show("Tài khoản mới cần có mật khẩu."); return; }
                    Database.Execute("INSERT INTO Account(AccountName,Password,FullName,Email,DateCreated) VALUES(@account,@password,@name,@email,GETDATE())", Database.Parameter("@account", account.Text), Database.Parameter("@password", password.Text), Database.Parameter("@name", fullName.Text), Database.Parameter("@email", email.Text));
                }
                else Database.Execute("UPDATE Account SET FullName=@name, Email=@email WHERE AccountName=@account", Database.Parameter("@account", account.Text), Database.Parameter("@name", fullName.Text), Database.Parameter("@email", email.Text));
                Database.Execute("IF EXISTS (SELECT 1 FROM RoleAccount WHERE AccountName=@account AND RoleID=@role) UPDATE RoleAccount SET Actived=@active WHERE AccountName=@account AND RoleID=@role ELSE INSERT INTO RoleAccount(RoleID,AccountName,Actived) VALUES(@role,@account,@active)", Database.Parameter("@account", account.Text), Database.Parameter("@role", role.SelectedValue), Database.Parameter("@active", active.Checked ? 1 : 0));
                LoadAccounts();
            }, this);
        }

        private void ResetPassword()
        {
            if (string.IsNullOrWhiteSpace(account.Text) || string.IsNullOrWhiteSpace(password.Text)) { MessageBox.Show("Chọn tài khoản và nhập mật khẩu mới."); return; }
            if (Database.Try(() => Database.Execute("UPDATE Account SET Password=@password WHERE AccountName=@account", Database.Parameter("@password", password.Text), Database.Parameter("@account", account.Text)), this)) password.Clear();
        }

        private void DeactivateAccount()
        {
            if (string.IsNullOrWhiteSpace(account.Text)) return;
            if (Database.Try(() => Database.Execute("UPDATE RoleAccount SET Actived=0 WHERE AccountName=@account", Database.Parameter("@account", account.Text)), this)) LoadAccounts();
        }

        private void ViewRoles()
        {
            if (!string.IsNullOrWhiteSpace(account.Text)) new RoleAccountForm(account.Text).ShowDialog(this);
        }

        private static string TextOf(DataGridViewRow row, string column) { return row.Cells[column].Value == null ? "" : row.Cells[column].Value.ToString(); }
    }
}
