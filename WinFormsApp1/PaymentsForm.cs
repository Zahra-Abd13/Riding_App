using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class PaymentsForm : Form
    {
        public DataGridView dgvPayments = null!;
        public TextBox txtRideID = null!, txtAmount = null!, txtSearchID = null!;
        public ComboBox cmbMethod = null!;
        public DateTimePicker dtDate = null!;
        public Button btnAdd = null!, btnUpdate = null!, btnDelete = null!, btnClear = null!, btnSearch = null!, btnLoadAll = null!;
        public Label lblStatus = null!;

        private readonly Color BG = Color.FromArgb(18, 18, 24);
        private readonly Color PANEL = Color.FromArgb(28, 28, 38);
        private readonly Color CARD = Color.FromArgb(38, 38, 52);
        private readonly Color ACCENT = Color.FromArgb(100, 181, 246);
        private readonly Color GREEN = Color.FromArgb(72, 199, 142);
        private readonly Color RED = Color.FromArgb(252, 95, 95);
        private readonly Color ORANGE = Color.FromArgb(251, 176, 64);
        private readonly Color TXT = Color.FromArgb(220, 220, 235);
        private readonly Color SUBTLE = Color.FromArgb(120, 120, 145);

        private int? selectedPaymentId;

        private static readonly Dictionary<string, string> Headers = new()
        {
            ["PaymentID"] = "Payment ID",
            ["RideID"] = "Ride ID",
            ["Amount"] = "Amount",
            ["PaymentMethod"] = "Method",
            ["PaymentDate"] = "Payment Date"
        };

        public PaymentsForm()
        {
            Text = "Payments Management";
            Size = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = BG;
            ForeColor = TXT;
            Font = new Font("Segoe UI", 10);
            MinimumSize = new Size(900, 600);

            BuildUI();
            Load += async (_, _) => await LoadPaymentsAsync();
        }

        private void BuildUI()
        {
            Panel left = new() { Width = 300, Dock = DockStyle.Left, BackColor = PANEL };
           

            int y = 70;
            Label section = new() { Text = "PAYMENT DETAILS", ForeColor = SUBTLE, Location = new Point(15, y), AutoSize = true };
            left.Controls.Add(section);
            y += 30;

            txtRideID = AddField(left, "Ride ID", ref y);
            txtAmount = AddField(left, "Amount (EGP)", ref y);

            left.Controls.Add(MakeLabel("Payment Method", new Point(15, y)));
            y += 22;
            cmbMethod = new ComboBox { Location = new Point(15, y), Width = 260, BackColor = CARD, ForeColor = TXT, FlatStyle = FlatStyle.Flat, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbMethod.Items.AddRange(new object[] { "Cash", "Credit Card", "Wallet" });
            left.Controls.Add(cmbMethod);
            y += 45;

            left.Controls.Add(MakeLabel("Payment Date", new Point(15, y)));
            y += 22;
            dtDate = new DateTimePicker { Location = new Point(15, y), Width = 260, Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd" };
            left.Controls.Add(dtDate);
            y += 50;

            btnAdd = MakeButton("Add", GREEN, new Point(15, y), 125);
            btnUpdate = MakeButton("Update", ORANGE, new Point(150, y), 125);
            left.Controls.Add(btnAdd);
            left.Controls.Add(btnUpdate);
            y += 50;

            btnDelete = MakeButton("Delete", RED, new Point(15, y), 125);
            btnClear = MakeButton("Clear", CARD, new Point(150, y), 125);
            left.Controls.Add(btnDelete);
            left.Controls.Add(btnClear);
            y += 60;

            left.Controls.Add(MakeLabel("Search by Payment ID", new Point(15, y)));
            y += 22;
            txtSearchID = MakeTextBox(new Point(15, y), 160);
            left.Controls.Add(txtSearchID);
            btnSearch = MakeButton("Search", ACCENT, new Point(182, y), 93);
            left.Controls.Add(btnSearch);
            y += 50;

            btnLoadAll = new Button { Text = "Load All Payments", BackColor = PANEL, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand, Location = new Point(15, y), Width = 260, Height = 38 };
            btnLoadAll.FlatAppearance.BorderColor = ACCENT;
            btnLoadAll.FlatAppearance.BorderSize = 0;
            btnLoadAll.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            left.Controls.Add(btnLoadAll);

            lblStatus = new Label { Text = "Ready", ForeColor = SUBTLE, Location = new Point(15, 630), AutoSize = true };
            left.Controls.Add(lblStatus);

            Panel right = new() { Dock = DockStyle.Fill, BackColor = BG, Padding = new Padding(15) };
            Controls.Add(right);
            Controls.Add(left);

            dgvPayments = MakeGrid();
            dgvPayments.Dock = DockStyle.Fill;
            right.Controls.Add(dgvPayments);

            btnAdd.Click += AddPayment;
            btnUpdate.Click += UpdatePayment;
            btnDelete.Click += DeletePayment;
            btnClear.Click += ClearFields;
            btnSearch.Click += SearchPayment;
            btnLoadAll.Click += async (_, _) => await LoadPaymentsAsync();
            dgvPayments.CellClick += FillFieldsFromGrid;
        }

        private async void AddPayment(object? sender, EventArgs e)
        {
            if (!ValidatePayment(out int rideId, out decimal amount, out string method)) return;

            try
            {
                await Database.ExecuteAsync(
                    """
                    INSERT INTO Payments (RideID, Amount, PaymentMethod, PaymentDate)
                    VALUES (@RideID, @Amount, @PaymentMethod, @PaymentDate)
                    """,
                    Database.Parameter("@RideID", SqlDbType.Int, rideId),
                    Database.Parameter("@Amount", SqlDbType.Decimal, amount),
                    Database.Parameter("@PaymentMethod", SqlDbType.NVarChar, 30, method),
                    Database.Parameter("@PaymentDate", SqlDbType.Date, dtDate.Value.Date));

                ClearFields(sender, e);
                await LoadPaymentsAsync();
                lblStatus.Text = "Payment added";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Add payment", ex);
            }
        }

        private async void UpdatePayment(object? sender, EventArgs e)
        {
            if (selectedPaymentId is null)
            {
                MessageBox.Show("Select a payment first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidatePayment(out int rideId, out decimal amount, out string method)) return;

            try
            {
                await Database.ExecuteAsync(
                    """
                    UPDATE Payments
                    SET RideID = @RideID, Amount = @Amount, PaymentMethod = @PaymentMethod, PaymentDate = @PaymentDate
                    WHERE PaymentID = @PaymentID
                    """,
                    Database.Parameter("@PaymentID", SqlDbType.Int, selectedPaymentId.Value),
                    Database.Parameter("@RideID", SqlDbType.Int, rideId),
                    Database.Parameter("@Amount", SqlDbType.Decimal, amount),
                    Database.Parameter("@PaymentMethod", SqlDbType.NVarChar, 30, method),
                    Database.Parameter("@PaymentDate", SqlDbType.Date, dtDate.Value.Date));

                await LoadPaymentsAsync();
                lblStatus.Text = "Payment updated";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Update payment", ex);
            }
        }

        private async void DeletePayment(object? sender, EventArgs e)
        {
            if (selectedPaymentId is null)
            {
                MessageBox.Show("Select a payment first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Delete selected payment?", "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                await Database.ExecuteAsync("DELETE FROM Payments WHERE PaymentID = @PaymentID",
                    Database.Parameter("@PaymentID", SqlDbType.Int, selectedPaymentId.Value));
                ClearFields(sender, e);
                await LoadPaymentsAsync();
                lblStatus.Text = "Payment deleted";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Delete payment", ex);
            }
        }

        private void ClearFields(object? sender, EventArgs e)
        {
            selectedPaymentId = null;
            txtRideID.Clear();
            txtAmount.Clear();
            cmbMethod.SelectedIndex = -1;
            dtDate.Value = DateTime.Today;
            txtSearchID.Clear();
            lblStatus.Text = "Ready";
        }

        private async void SearchPayment(object? sender, EventArgs e)
        {
            if (!FormUtilities.TryReadInt(txtSearchID, "Payment ID", out int id)) return;

            try
            {
                DataTable table = await Database.QueryAsync(
                    """
                    SELECT PaymentID, RideID, Amount, PaymentMethod, PaymentDate
                    FROM Payments
                    WHERE PaymentID = @PaymentID
                    """,
                    Database.Parameter("@PaymentID", SqlDbType.Int, id));
                FormUtilities.Bind(dgvPayments, table, Headers);
                lblStatus.Text = table.Rows.Count == 0 ? "No payment found" : "Payment found";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Search payment", ex);
            }
        }

        private void FillFieldsFromGrid(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvPayments.Rows[e.RowIndex];
            selectedPaymentId = int.TryParse(FormUtilities.CellText(row, "PaymentID"), out int id) ? id : null;
            txtRideID.Text = FormUtilities.CellText(row, "RideID");
            txtAmount.Text = FormUtilities.CellText(row, "Amount");
            cmbMethod.Text = FormUtilities.CellText(row, "PaymentMethod");
            if (DateTime.TryParse(FormUtilities.CellText(row, "PaymentDate"), out DateTime date))
            {
                dtDate.Value = date;
            }
        }

        private async Task LoadPaymentsAsync()
        {
            try
            {
                DataTable table = await Database.QueryAsync(
                    """
                    SELECT PaymentID, RideID, Amount, PaymentMethod, PaymentDate
                    FROM Payments
                    ORDER BY PaymentID
                    """);
                FormUtilities.Bind(dgvPayments, table, Headers);
                lblStatus.Text = $"Loaded {table.Rows.Count} payments";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Load payments", ex);
            }
        }

        private bool ValidatePayment(out int rideId, out decimal amount, out string method)
        {
            rideId = default;
            amount = default;
            method = string.Empty;
            return FormUtilities.TryReadInt(txtRideID, "Ride ID", out rideId)
                && FormUtilities.TryReadDecimal(txtAmount, "Amount", out amount)
                && cmbMethod.SelectedIndex >= 0
                && (method = cmbMethod.SelectedItem?.ToString() ?? string.Empty) != string.Empty;
        }

        private TextBox AddField(Panel parent, string label, ref int y)
        {
            parent.Controls.Add(MakeLabel(label, new Point(15, y)));
            y += 22;
            TextBox txt = MakeTextBox(new Point(15, y), 260);
            parent.Controls.Add(txt);
            y += 42;
            return txt;
        }

        private Label MakeLabel(string text, Point loc) =>
            new() { Text = text, ForeColor = SUBTLE, Font = new Font("Segoe UI", 9), Location = loc, AutoSize = true };

        private TextBox MakeTextBox(Point loc, int width) =>
            new() { Location = loc, Width = width, Height = 32, BackColor = CARD, ForeColor = TXT, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10) };

        private Button MakeButton(string text, Color color, Point loc, int width)
        {
            Button b = new() { Text = text, Location = loc, Size = new Size(width, 38), BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private DataGridView MakeGrid()
        {
            DataGridView dg = new()
            {
                BackgroundColor = CARD,
                GridColor = Color.FromArgb(50, 50, 70),
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                AllowUserToResizeColumns = true
            };
            dg.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dg.DefaultCellStyle.Padding = new Padding(5);
            dg.DefaultCellStyle.BackColor = CARD;
            dg.DefaultCellStyle.ForeColor = TXT;
            dg.DefaultCellStyle.SelectionForeColor = Color.White;
            dg.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(44, 44, 60);
            FormUtilities.ConfigureGrid(dg);
            return dg;
        }
    }
}
