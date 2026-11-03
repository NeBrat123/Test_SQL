using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab_Basic_Command
{
    public partial class CategoryForm : Form
    {
        public CategoryForm()
        {
            InitializeComponent();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            string connectionString = "server=.; database=RestaurantManagement; integrated security=true;";

            SqlConnection sqlConnection = new SqlConnection(connectionString);

            SqlCommand sqlCommand = sqlConnection.CreateCommand();

            string query = "select ID, Name, Type FROM Category";
            sqlCommand.CommandText = query;

            sqlConnection.Open();

            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();

            this.DisplayCategory(sqlDataReader);

            sqlConnection.Close();
        }

        private void DisplayCategory(SqlDataReader sqlDataReader)
        {
            this.lvCategory.Items.Clear();

            while (sqlDataReader.Read())
            {
                ListViewItem listViewItem = new ListViewItem(sqlDataReader["ID"].ToString());
                listViewItem.SubItems.Add(sqlDataReader["Name"].ToString());
                listViewItem.SubItems.Add(sqlDataReader["Type"].ToString());
                this.lvCategory.Items.Add(listViewItem);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string connectionString = "server=.; database=RestaurantManagement; integrated security=true;";

            SqlConnection sqlConnection = new SqlConnection(connectionString);

            SqlCommand sqlCommand = sqlConnection.CreateCommand();

            sqlCommand.CommandText = "INSERT INTO Category VALUES(Name, [Type])" + "VALUES (N'" + this.txtName.Text + "', N'" + this.txtType.Text + "')";

            sqlConnection.Open();

            int numOfRowsAffected = sqlCommand.ExecuteNonQuery();

            sqlConnection.Close();

            if (numOfRowsAffected == 1)
            {
                MessageBox.Show("Thêm thành công!");

                btnLoad.PerformClick();

                txtName.Text = "";
                txtType.Text = "";
            }
            else
            {
                MessageBox.Show("Thêm thất bại!");
            }
        }

        private void lvCategory_Click(object sender, EventArgs e)
        {
            ListViewItem listViewItem = this.lvCategory.SelectedItems[0];

            txtID.Text = listViewItem.SubItems[0].Text;
            txtName.Text = listViewItem.SubItems[1].Text;
            txtType.Text = listViewItem.SubItems[1].Text == "0" ? "Thức ăn" : "Đồ uống";

            btnUpdate.Enabled = true;
            btnDelete.Enabled = true;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            string connectionString = "server=.; database=RestaurantManagement; integrated security=true;";
            SqlConnection sqlConnection = new SqlConnection(connectionString);

            SqlCommand sqlCommand = sqlConnection.CreateCommand();

            sqlCommand.CommandText = "UPDATE Category SET Name = N'" + this.txtName.Text + "', [Type] = N'" + this.txtType.Text + "' WHERE ID = " + this.txtID.Text;

            sqlConnection.Open();

            int numOfRowsAffected = sqlCommand.ExecuteNonQuery();

            sqlConnection.Close();

            if (numOfRowsAffected == 1)
            {
                ListViewItem listViewItem = this.lvCategory.SelectedItems[0];

                listViewItem.SubItems[1].Text = this.txtName.Text;
                listViewItem.SubItems[2].Text = this.txtType.Text == "Thức ăn" ? "0" : "1";

                txtID.Text = "";
                txtName.Text = "";
                txtType.Text = "";

                MessageBox.Show("Cập nhật thành công!");


                btnUpdate.Enabled = false;
                btnDelete.Enabled = false;
            }
            else
            {
                MessageBox.Show("Cập nhật thất bại!");
            }

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string connectionString = "server=.; database=RestaurantManagement; integrated security=true;";
            SqlConnection sqlConnection = new SqlConnection(connectionString);

            SqlCommand sqlCommand = sqlConnection.CreateCommand();

            sqlCommand.CommandText = "DELETE FROM Category WHERE ID = " + this.txtID.Text;

            sqlConnection.Open();

            int numOfRowsAffected = sqlCommand.ExecuteNonQuery();

            sqlConnection.Close();

            if (numOfRowsAffected == 1)
            {
                ListViewItem listViewItem = this.lvCategory.SelectedItems[0];
                this.lvCategory.Items.Remove(listViewItem);

                txtID.Text = "";
                txtName.Text = "";
                txtType.Text = "";

                MessageBox.Show("Xóa thành công!");
                btnUpdate.Enabled = false;
                btnDelete.Enabled = false;
            }
            else
            {
                MessageBox.Show("Xóa thất bại!");
            }
        }

        private void tsmDelete_Click(object sender, EventArgs e)
        {
            if (this.lvCategory.SelectedItems.Count > 0)
            {
                ListViewItem listViewItem = this.lvCategory.SelectedItems[0];
                txtID.Text = listViewItem.SubItems[0].Text;
                txtName.Text = listViewItem.SubItems[1].Text;
                txtType.Text = listViewItem.SubItems[1].Text == "0" ? "Thức ăn" : "Đồ uống";
                btnDelete.PerformClick();
            }
        }

        private void tsmViewFood_Click(object sender, EventArgs e)
        {

        }
    }
}
