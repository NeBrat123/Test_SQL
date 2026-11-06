using System.Windows.Forms;

namespace Lab_Basic_Command
{
    public class RoleAccountForm : Form
    {
        private readonly string account;
        private readonly DataGridView grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };

        public RoleAccountForm(string account)
        {
            this.account = account;
            Text = "Vai trò của " + account;
            Width = 500;
            Height = 350;
            Controls.Add(grid);
            Load += (s, e) => Database.Try(() => grid.DataSource = Database.Query(@"SELECT r.ID, r.RoleName, ra.Actived, ra.Notes FROM RoleAccount ra INNER JOIN Role r ON r.ID=ra.RoleID WHERE ra.AccountName=@account ORDER BY r.RoleName", Database.Parameter("@account", account)), this);
        }
    }
}
