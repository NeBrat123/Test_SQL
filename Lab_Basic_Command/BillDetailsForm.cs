using System;
using System.Windows.Forms;

namespace Lab_Basic_Command
{
    public class BillDetailsForm : Form
    {
        private readonly int billId;
        private readonly DataGridView grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };

        public BillDetailsForm(int billId)
        {
            this.billId = billId;
            Text = "Chi tiết hóa đơn #" + billId;
            Width = 700;
            Height = 400;
            Controls.Add(grid);
            Load += (s, e) => LoadDetails();
        }

        private void LoadDetails()
        {
            Database.Try(() =>
            {
                grid.DataSource = Database.Query(@"SELECT f.Name AS FoodName, f.Unit, f.Price,
                    d.Quantity, f.Price * d.Quantity AS LineTotal
                    FROM BillDetails d INNER JOIN Food f ON f.ID = d.FoodID
                    WHERE d.InvoiceID = @id ORDER BY f.Name", Database.Parameter("@id", billId));
            }, this);
        }
    }
}
