using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Lab_Basic_Command
{
    public partial class FoodForm : Form
    {
        private int currentCategoryId;
        public FoodForm()
        {
            InitializeComponent();
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42, Padding = new Padding(6) };
            var save = new Button { Text = "Lưu", Width = 90 };
            var delete = new Button { Text = "Xóa", Width = 90 };
            save.Click += btnSave_Click;
            delete.Click += btnDelete_Click;
            buttons.Controls.Add(save);
            buttons.Controls.Add(delete);
            Controls.Add(buttons);
            dgvFood.AllowUserToAddRows = true;
            dgvFood.AutoGenerateColumns = false;
        }

        private void FoodForm_Load(object sender, EventArgs e)
        {

        }

        public void LoadFoodList(int categoryID)
        {
            currentCategoryId = categoryID;
            var categoryName = Database.Scalar("SELECT Name FROM Category WHERE ID = @id", Database.Parameter("@id", categoryID));
            Text = "Danh sách món ăn thuộc nhóm: " + (categoryName ?? "");
            dgvFood.DataSource = Database.Query("SELECT ID, Name, Unit, FoodCategoryID, Price, Notes AS Note FROM Food WHERE FoodCategoryID = @id ORDER BY ID", Database.Parameter("@id", categoryID));
        }

        private void btnSave_Click(object sender, System.EventArgs e)
        {
            if (dgvFood.CurrentRow == null || dgvFood.CurrentRow.IsNewRow)
                return;

            var row = dgvFood.CurrentRow;
            int id;
            int categoryId;
            int price;
            if (!int.TryParse(Value(row, "ID"), out id) || !int.TryParse(Value(row, "FoodCategoryID"), out categoryId) || !int.TryParse(Value(row, "Price"), out price))
            {
                MessageBox.Show("Mã nhóm và đơn giá phải là số.");
                return;
            }
            var name = Value(row, "Name");
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Tên món không được để trống.");
                return;
            }

            var parameters = new[]
            {
                Database.Parameter("@name", name),
                Database.Parameter("@unit", Value(row, "Unit")),
                Database.Parameter("@category", categoryId),
                Database.Parameter("@price", price),
                Database.Parameter("@notes", Value(row, "Note"))
            };
            bool ok = Database.Try(() =>
            {
                if (id > 0)
                {
                    Database.Execute("UPDATE Food SET Name=@name, Unit=@unit, FoodCategoryID=@category, Price=@price, Notes=@notes WHERE ID=@id", Add(parameters, Database.Parameter("@id", id)));
                }
                else
                {
                    Database.Execute("INSERT INTO Food(Name, Unit, FoodCategoryID, Price, Notes) VALUES(@name, @unit, @category, @price, @notes)", parameters);
                }
            }, this);
            if (ok)
                LoadFoodList(categoryId);
        }

        private void btnDelete_Click(object sender, System.EventArgs e)
        {
            if (dgvFood.CurrentRow == null || dgvFood.CurrentRow.IsNewRow)
                return;
            int id;
            if (!int.TryParse(Value(dgvFood.CurrentRow, "ID"), out id) || id <= 0)
                return;
            if (MessageBox.Show("Xóa món ăn đang chọn?", "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            if (Database.Try(() => Database.Execute("DELETE FROM Food WHERE ID=@id", Database.Parameter("@id", id)), this))
                LoadFoodList(currentCategoryId);
        }

        private static string Value(DataGridViewRow row, string column)
        {
            var value = row.Cells[column].Value;
            return value == null ? "" : value.ToString().Trim();
        }

        private static SqlParameter[] Add(SqlParameter[] values, SqlParameter value)
        {
            var result = new SqlParameter[values.Length + 1];
            values.CopyTo(result, 0);
            result[values.Length] = value;
            return result;
        }
    }
}
