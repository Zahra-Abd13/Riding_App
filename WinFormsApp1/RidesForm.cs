using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class RidesForm : Form
    {
        public DataGridView dgvRides = null!;
        public TextBox txtUserID = null!, txtDriverID = null!, txtPickupID = null!, txtDropoffID = null!, txtRideDate = null!, txtFare = null!, txtDuration = null!, txtSearchID = null!;
        public Button btnAdd = null!, btnUpdate = null!, btnDelete = null!, btnSearch = null!, btnClear = null!, btnLoadAll = null!;
        public Label lblStatus = null!;

        private readonly Color BG = Color.FromArgb(18, 18, 24);
        private readonly Color PANEL = Color.FromArgb(28, 28, 38);
        private readonly Color CARD = Color.FromArgb(38, 38, 52);
        private readonly Color ACCENT = Color.FromArgb(251, 176, 64);
        private readonly Color GREEN = Color.FromArgb(72, 199, 142);
        private readonly Color RED = Color.FromArgb(252, 95, 95);
        private readonly Color ORANGE = Color.FromArgb(251, 176, 64);
        private readonly Color TXT = Color.FromArgb(220, 220, 235);
        private readonly Color SUBTLE = Color.FromArgb(120, 120, 145);

        private int? selectedRideId;

        private static readonly Dictionary<string, string> Headers = new()
        {
            ["RideID"] = "Ride ID",
            ["UserID"] = "User ID",
            ["DriverID"] = "Driver ID",
            ["PickupLocationID"] = "Pickup Location ID",
            ["DropoffLocationID"] = "Dropoff Location ID",
            ["RideDate"] = "Ride Date",
            ["Fare"] = "Fare",
            ["RideDuration"] = "Duration (Minutes)"
        };

        public RidesForm()
        {
            Text = "Rides Management";
            Size = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = BG;
            ForeColor = TXT;
            Font = new Font("Segoe UI", 10);
            MinimumSize = new Size(900, 600);

            BuildUI();
            Load += async (_, _) => await LoadRidesAsync();
        }

        private void BuildUI()
        {
            

            Panel leftPanel = new() { Width = 300, Dock = DockStyle.Left, BackColor = PANEL };
           

            int y = 70;
            leftPanel.Controls.Add(MakeSection("RIDE DETAILS", new Point(15, y)));
            y += 25;

            string[] fieldLabels =
            {
                "User ID", "Driver ID", "Pickup Location ID", "Dropoff Location ID",
                "Ride Date", "Fare (EGP)", "Duration (minutes)"
            };
            TextBox[] boxes = new TextBox[7];

            for (int i = 0; i < fieldLabels.Length; i++)
            {
                leftPanel.Controls.Add(MakeLabel(fieldLabels[i], new Point(15, y)));
                y += 22;
                boxes[i] = MakeTextBox(new Point(15, y), 260);
                leftPanel.Controls.Add(boxes[i]);
                y += 42;
            }

            txtUserID = boxes[0];
            txtDriverID = boxes[1];
            txtPickupID = boxes[2];
            txtDropoffID = boxes[3];
            txtRideDate = boxes[4];
            txtFare = boxes[5];
            txtDuration = boxes[6];

            y += 5;
            btnAdd = MakeButton("Book Ride", GREEN, new Point(15, y), 125);
            btnUpdate = MakeButton("Update", ORANGE, new Point(150, y), 125);
            leftPanel.Controls.Add(btnAdd);
            leftPanel.Controls.Add(btnUpdate);
            y += 50;

            btnDelete = MakeButton("Delete", RED, new Point(15, y), 125);
            btnClear = MakeButton("Clear", CARD, new Point(150, y), 125);
            leftPanel.Controls.Add(btnDelete);
            leftPanel.Controls.Add(btnClear);
            y += 55;

            leftPanel.Controls.Add(MakeLabel("Search by Ride ID", new Point(15, y)));
            y += 22;
            txtSearchID = MakeTextBox(new Point(15, y), 160);
            leftPanel.Controls.Add(txtSearchID);
            btnSearch = MakeButton("Search", ACCENT, new Point(182, y), 93);
            leftPanel.Controls.Add(btnSearch);
            y += 50;

            btnLoadAll = new Button { Text = "Load All Rides", BackColor = PANEL, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand, Location = new Point(15, y), Width = 260, Height = 38 };
            btnLoadAll.FlatAppearance.BorderColor = ACCENT;
            btnLoadAll.FlatAppearance.BorderSize = 0;
            btnLoadAll.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            leftPanel.Controls.Add(btnLoadAll);

            lblStatus = new Label { Text = "Ready", ForeColor = SUBTLE, Font = new Font("Segoe UI", 9), Location = new Point(15, 630), AutoSize = true };
            leftPanel.Controls.Add(lblStatus);

            Panel rightPanel = new() { Dock = DockStyle.Fill, BackColor = BG, Padding = new Padding(15) };
            Controls.Add(rightPanel);
            Controls.Add(leftPanel);

            dgvRides = MakeGrid();
            dgvRides.Dock = DockStyle.Fill;
            rightPanel.Controls.Add(dgvRides);

            btnAdd.Click += AddRide;
            btnUpdate.Click += UpdateRide;
            btnDelete.Click += DeleteRide;
            btnClear.Click += ClearFields;
            btnSearch.Click += SearchRide;
            btnLoadAll.Click += async (_, _) => await LoadRidesAsync();
            dgvRides.CellClick += FillFieldsFromGrid;
        }

        private async void AddRide(object? sender, EventArgs e)
        {
            if (!ValidateRide(out int userId, out int driverId, out int pickupId, out int dropoffId, out DateTime rideDate, out decimal fare, out int duration)) return;

            try
            {
                await Database.ExecuteAsync(
                    """
                    INSERT INTO Rides (UserID, DriverID, PickupLocationID, DropoffLocationID, RideDate, Fare, RideDuration)
                    VALUES (@UserID, @DriverID, @PickupLocationID, @DropoffLocationID, @RideDate, @Fare, @RideDuration)
                    """,
                    Database.Parameter("@UserID", SqlDbType.Int, userId),
                    Database.Parameter("@DriverID", SqlDbType.Int, driverId),
                    Database.Parameter("@PickupLocationID", SqlDbType.Int, pickupId),
                    Database.Parameter("@DropoffLocationID", SqlDbType.Int, dropoffId),
                    Database.Parameter("@RideDate", SqlDbType.DateTime, rideDate),
                    Database.Parameter("@Fare", SqlDbType.Decimal, fare),
                    Database.Parameter("@RideDuration", SqlDbType.Int, duration));

                ClearFields(sender, e);
                await LoadRidesAsync();
                lblStatus.Text = "Ride booked";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Add ride", ex);
            }
        }

        private async void UpdateRide(object? sender, EventArgs e)
        {
            if (selectedRideId is null)
            {
                MessageBox.Show("Select a ride first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateRide(out int userId, out int driverId, out int pickupId, out int dropoffId, out DateTime rideDate, out decimal fare, out int duration)) return;

            try
            {
                await Database.ExecuteAsync(
                    """
                    UPDATE Rides
                    SET UserID = @UserID, DriverID = @DriverID, PickupLocationID = @PickupLocationID,
                        DropoffLocationID = @DropoffLocationID, RideDate = @RideDate,
                        Fare = @Fare, RideDuration = @RideDuration
                    WHERE RideID = @RideID
                    """,
                    Database.Parameter("@RideID", SqlDbType.Int, selectedRideId.Value),
                    Database.Parameter("@UserID", SqlDbType.Int, userId),
                    Database.Parameter("@DriverID", SqlDbType.Int, driverId),
                    Database.Parameter("@PickupLocationID", SqlDbType.Int, pickupId),
                    Database.Parameter("@DropoffLocationID", SqlDbType.Int, dropoffId),
                    Database.Parameter("@RideDate", SqlDbType.DateTime, rideDate),
                    Database.Parameter("@Fare", SqlDbType.Decimal, fare),
                    Database.Parameter("@RideDuration", SqlDbType.Int, duration));

                await LoadRidesAsync();
                lblStatus.Text = "Ride updated";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Update ride", ex);
            }
        }

        private async void DeleteRide(object? sender, EventArgs e)
        {
            if (selectedRideId is null)
            {
                MessageBox.Show("Select a ride first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Delete selected ride?", "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                await Database.ExecuteAsync(
                    """
                    DELETE FROM Reviews WHERE RideID = @RideID;
                    DELETE FROM Payments WHERE RideID = @RideID;
                    DELETE FROM Rides WHERE RideID = @RideID;
                    """,
                    Database.Parameter("@RideID", SqlDbType.Int, selectedRideId.Value));
                ClearFields(sender, e);
                await LoadRidesAsync();
                lblStatus.Text = "Ride deleted";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Delete ride", ex);
            }
        }

        private void ClearFields(object? sender, EventArgs e)
        {
            selectedRideId = null;
            txtUserID.Clear();
            txtDriverID.Clear();
            txtPickupID.Clear();
            txtDropoffID.Clear();
            txtRideDate.Clear();
            txtFare.Clear();
            txtDuration.Clear();
            txtSearchID.Clear();
            lblStatus.Text = "Ready";
        }

        private async void SearchRide(object? sender, EventArgs e)
        {
            if (!FormUtilities.TryReadInt(txtSearchID, "Ride ID", out int id)) return;

            try
            {
                DataTable table = await Database.QueryAsync(
                    """
                    SELECT RideID, UserID, DriverID, PickupLocationID, DropoffLocationID, RideDate, Fare, RideDuration
                    FROM Rides
                    WHERE RideID = @RideID
                    """,
                    Database.Parameter("@RideID", SqlDbType.Int, id));
                FormUtilities.Bind(dgvRides, table, Headers);
                lblStatus.Text = table.Rows.Count == 0 ? "No ride found" : "Ride found";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Search ride", ex);
            }
        }

        private void FillFieldsFromGrid(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvRides.Rows[e.RowIndex];
            selectedRideId = int.TryParse(FormUtilities.CellText(row, "RideID"), out int id) ? id : null;
            txtUserID.Text = FormUtilities.CellText(row, "UserID");
            txtDriverID.Text = FormUtilities.CellText(row, "DriverID");
            txtPickupID.Text = FormUtilities.CellText(row, "PickupLocationID");
            txtDropoffID.Text = FormUtilities.CellText(row, "DropoffLocationID");
            txtRideDate.Text = FormUtilities.CellText(row, "RideDate");
            txtFare.Text = FormUtilities.CellText(row, "Fare");
            txtDuration.Text = FormUtilities.CellText(row, "RideDuration");
        }

        private async Task LoadRidesAsync()
        {
            try
            {
                DataTable table = await Database.QueryAsync(
                    """
                    SELECT RideID, UserID, DriverID, PickupLocationID, DropoffLocationID, RideDate, Fare, RideDuration
                    FROM Rides
                    ORDER BY RideID
                    """);
                FormUtilities.Bind(dgvRides, table, Headers);
                lblStatus.Text = $"Loaded {table.Rows.Count} rides";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Load rides", ex);
            }
        }

        private bool ValidateRide(out int userId, out int driverId, out int pickupId, out int dropoffId, out DateTime rideDate, out decimal fare, out int duration)
        {
            userId = driverId = pickupId = dropoffId = duration = default;
            rideDate = default;
            fare = default;

            return FormUtilities.TryReadInt(txtUserID, "User ID", out userId)
                && FormUtilities.TryReadInt(txtDriverID, "Driver ID", out driverId)
                && FormUtilities.TryReadInt(txtPickupID, "Pickup Location ID", out pickupId)
                && FormUtilities.TryReadInt(txtDropoffID, "Dropoff Location ID", out dropoffId)
                && FormUtilities.TryReadDate(txtRideDate, "Ride Date", out rideDate)
                && FormUtilities.TryReadDecimal(txtFare, "Fare", out fare)
                && FormUtilities.TryReadInt(txtDuration, "Duration", out duration);
        }

        private Label MakeSection(string text, Point loc) =>
            new() { Text = text, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = SUBTLE, Location = loc, AutoSize = true };

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
