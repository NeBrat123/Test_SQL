using System;
using System.Windows.Forms;

namespace Lab_Basic_Command
{
    public class BillLogForm : Form
    {
        private readonly Label summary = new Label { Dock = DockStyle.Top, Height = 34, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
        private readonly DataGridView grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        public BillLogForm()
        {
            Text = "Nhật ký mua hàng"; Width = 1000; Height = 550;
            Controls.Add(grid); Controls.Add(summary); Load += (s, e) => LoadLog();
        }
        private void LoadLog()
        {
            Database.Try(() =>
            {
                var table = Database.Query(@"SELECT b.ID, b.CheckoutDate, b.Account, b.TableID, b.Amount,
                    ISNULL(b.Amount*b.Tax,0) AS TaxAmount, ISNULL(b.Amount*b.Discount,0) AS DiscountAmount
                    FROM Bills b ORDER BY b.CheckoutDate DESC");
                grid.DataSource = table;
                decimal amount = 0, tax = 0, discount = 0;
                foreach (System.Data.DataRow row in table.Rows) { amount += Convert.ToDecimal(row["Amount"]); tax += Convert.ToDecimal(row["TaxAmount"]); discount += Convert.ToDecimal(row["DiscountAmount"]); }
                summary.Text = string.Format("Số hóa đơn: {0}    Tổng tiền: {1:N0}    Tổng thuế: {2:N0}    Tổng giảm giá: {3:N0}", table.Rows.Count, amount, tax, discount);
            }, this);
        }
    }
}
