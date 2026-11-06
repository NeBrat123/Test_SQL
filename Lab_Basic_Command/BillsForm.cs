using System;
using System.Data;
using System.Windows.Forms;

namespace Lab_Basic_Command
{
    public class BillsForm : Form
    {
        private readonly DateTimePicker from = new DateTimePicker { Format = DateTimePickerFormat.Short };
        private readonly DateTimePicker to = new DateTimePicker { Format = DateTimePickerFormat.Short };
        private readonly DataGridView grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
        private readonly Label summary = new Label { Dock = DockStyle.Bottom, Height = 32, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };

        public BillsForm()
        {
            Text = "Danh sách hóa đơn";
            Width = 900;
            Height = 520;
            var filter = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, Padding = new Padding(6) };
            var load = new Button { Text = "Xem", Width = 80 };
            filter.Controls.Add(new Label { Text = "Từ ngày", AutoSize = true, Padding = new Padding(0, 7, 0, 0) });
            filter.Controls.Add(from);
            filter.Controls.Add(new Label { Text = "Đến ngày", AutoSize = true, Padding = new Padding(8, 7, 0, 0) });
            filter.Controls.Add(to);
            filter.Controls.Add(load);
            Controls.Add(grid);
            Controls.Add(summary);
            Controls.Add(filter);
            load.Click += (s, e) => LoadBills();
            grid.CellDoubleClick += Grid_CellDoubleClick;
            from.Value = DateTime.Today.AddDays(-30);
            to.Value = DateTime.Today;
            Load += (s, e) => LoadBills();
        }

        private void LoadBills()
        {
            if (from.Value.Date > to.Value.Date)
            {
                MessageBox.Show("Ngày bắt đầu phải trước ngày kết thúc.");
                return;
            }
            var sql = @"SELECT b.ID, b.Name, b.TableID, b.Amount,
                        ISNULL(b.Amount * b.Discount, 0) AS DiscountAmount,
                        ISNULL(b.Amount * b.Tax, 0) AS TaxAmount,
                        b.Status, b.CheckoutDate, b.Account,
                        b.Amount - ISNULL(b.Amount * b.Discount, 0) + ISNULL(b.Amount * b.Tax, 0) AS ActualAmount
                        FROM Bills b
                        WHERE b.CheckoutDate >= @from AND b.CheckoutDate < @to
                        ORDER BY b.CheckoutDate DESC";
            Database.Try(() =>
            {
                grid.DataSource = Database.Query(sql, Database.Parameter("@from", from.Value.Date), Database.Parameter("@to", to.Value.Date.AddDays(1)));
                decimal amount = 0, discount = 0, actual = 0;
                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (row.IsNewRow) continue;
                    amount += Convert.ToDecimal(row.Cells["Amount"].Value);
                    discount += Convert.ToDecimal(row.Cells["DiscountAmount"].Value);
                    actual += Convert.ToDecimal(row.Cells["ActualAmount"].Value);
                }
                summary.Text = string.Format("Tổng trước giảm: {0:N0}    Giảm giá: {1:N0}    Thực thu: {2:N0}", amount, discount, actual);
            }, this);
        }

        private void Grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int id = Convert.ToInt32(grid.Rows[e.RowIndex].Cells["ID"].Value);
            new BillDetailsForm(id).ShowDialog(this);
        }
    }
}
