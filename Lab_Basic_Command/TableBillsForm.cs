using System;
using System.Windows.Forms;

namespace Lab_Basic_Command
{
    public class TableBillsForm : Form
    {
        private readonly int tableId;
        private readonly ListBox dates = new ListBox { Dock = DockStyle.Fill };
        private readonly DataGridView details = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };

        public TableBillsForm(int tableId)
        {
            this.tableId = tableId;
            Text = "Danh mục hóa đơn của bàn #" + tableId;
            Width = 900; Height = 500;
            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 220 };
            split.Panel1.Controls.Add(dates); split.Panel2.Controls.Add(details); Controls.Add(split);
            dates.SelectedIndexChanged += (s, e) => LoadDetails();
            Load += (s, e) => LoadDates();
        }
        private void LoadDates()
        {
            Database.Try(() => { dates.DisplayMember = "Display"; dates.ValueMember = "ID"; dates.DataSource = Database.Query("SELECT ID, CONVERT(varchar(16), CheckoutDate, 120) AS Display FROM Bills WHERE TableID=@table ORDER BY CheckoutDate DESC", Database.Parameter("@table", tableId)); }, this);
        }
        private void LoadDetails()
        {
            if (dates.SelectedValue == null) return;
            Database.Try(() => details.DataSource = Database.Query(@"SELECT f.Name AS FoodName, f.Unit, f.Price, d.Quantity, f.Price*d.Quantity AS LineTotal
                FROM BillDetails d INNER JOIN Food f ON f.ID=d.FoodID WHERE d.InvoiceID=@id", Database.Parameter("@id", dates.SelectedValue)), this);
        }
    }
}
