using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class DriversForm : Form
    {
        public DataGridView dgvDrivers = null!;
        public TextBox txtDriverName = null!, txtLicense = null!, txtDriverPhone = null!, txtPlateNumber = null!, txtCarModel = null!, txtSearchID = null!;
        public Button btnAdd = null!, btnUpdate = null!, btnDelete = null!, btnSearch = null!, btnClear = null!, btnLoadAll = null!;
        public Label lblStatus = null!;

        private readonly Color BG = Color.FromArgb(18, 18, 24);
        private readonly Color PANEL = Color.FromArgb(28, 28, 38);
        private readonly Color CARD = Color.FromArgb(38, 38, 52);
        private readonly Color ACCENT = Color.FromArgb(129, 199, 132); // Driver Green Theme
        private readonly Color GREEN = Color.FromArgb(72, 199, 142);
        private readonly Color RED = Color.FromArgb(252, 95, 95);
        private readonly Color ORANGE = Color.FromArgb(251, 176, 64);
        private readonly Color TXT = Color.FromArgb(220, 220, 235);
        private readonly Color LABEL_COLOR = Color.FromArgb(170, 170, 195);

        private int? selectedDriverId;

        private static readonly Dictionary<string, string> Headers = new()
        {
            ["DriverID"] = "Driver ID",
            ["DriverName"] = "Driver Name",
            ["LicenseNumber"] = "License",
            ["PhoneNumber"] = "Phone",
            ["PlateNumber"] = "Plate",
            ["CarModel"] = "Car Model"
        };

        public DriversForm(FormWindowState callerState)
        {
            Text = "Drivers Management System";
            Size = new Size(1280, 850);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = BG;
            ForeColor = TXT;
            MinimumSize = new Size(1100, 700);
            WindowState = callerState;
            Font = new Font("Segoe UI", 10F);

            BuildUI();
            Load += async (_, _) => await LoadDriversAsync();
        }

        private void BuildUI()
        {

            // --- SIDEBAR ---
            Panel sidebar = new() { Width = 400, Dock = DockStyle.Left, BackColor = PANEL, Padding = new Padding(10) };
            

            Panel scrollContainer = new() { Dock = DockStyle.Fill, AutoScroll = true };
            sidebar.Controls.Add(scrollContainer);

            TableLayoutPanel tbl = new()
            {
                ColumnCount = 1,
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(15),
                BackColor = PANEL
            };
            scrollContainer.Controls.Add(tbl);

            // DRIVER SECTION
            AddControlToTable(tbl, MakeSectionLabel("DRIVER DETAILS"));
            AddControlToTable(tbl, MakeFieldLabel("Driver Name"));
            txtDriverName = MakeTextBox(); AddControlToTable(tbl, txtDriverName);
            AddControlToTable(tbl, MakeFieldLabel("License Number"));
            txtLicense = MakeTextBox(); AddControlToTable(tbl, txtLicense);
            AddControlToTable(tbl, MakeFieldLabel("Phone Number"));
            txtDriverPhone = MakeTextBox(); AddControlToTable(tbl, txtDriverPhone);

            // VEHICLE SECTION
            var lblVehicle = MakeSectionLabel("VEHICLE DETAILS");
            lblVehicle.Margin = new Padding(0, 20, 0, 5);
            AddControlToTable(tbl, lblVehicle);
            AddControlToTable(tbl, MakeFieldLabel("Plate Number"));
            txtPlateNumber = MakeTextBox(); AddControlToTable(tbl, txtPlateNumber);
            AddControlToTable(tbl, MakeFieldLabel("Car Model"));
            txtCarModel = MakeTextBox(); AddControlToTable(tbl, txtCarModel);

            // ACTIONS
            var lblActions = MakeSectionLabel("ACTIONS");
            lblActions.Margin = new Padding(0, 25, 0, 5);
            AddControlToTable(tbl, lblActions);

            TableLayoutPanel btnRow1 = MakeTwoColumnRow();
            btnAdd = MakeBtn("Add Driver", GREEN);
            btnUpdate = MakeBtn("Update", ORANGE);
            btnRow1.Controls.Add(btnAdd, 0, 0);
            btnRow1.Controls.Add(btnUpdate, 1, 0);
            AddControlToTable(tbl, btnRow1);

            TableLayoutPanel btnRow2 = MakeTwoColumnRow();
            btnDelete = MakeBtn("Delete", RED);
            btnClear = MakeBtn("Clear", CARD);
            btnRow2.Controls.Add(btnDelete, 0, 0);
            btnRow2.Controls.Add(btnClear, 1, 0);
            AddControlToTable(tbl, btnRow2);

            // SEARCH
            var lblSearch = MakeSectionLabel("SEARCH BY ID");
            lblSearch.Margin = new Padding(0, 25, 0, 5);
            AddControlToTable(tbl, lblSearch);

            TableLayoutPanel searchRow = new() { Height = 42, ColumnCount = 2, Dock = DockStyle.Top };
            searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            txtSearchID = MakeTextBox();
            btnSearch = MakeBtn("Search", ACCENT);
            searchRow.Controls.Add(txtSearchID, 0, 0);
            searchRow.Controls.Add(btnSearch, 1, 0);
            AddControlToTable(tbl, searchRow);

            btnLoadAll = MakeBtn("Load All Drivers", ACCENT);
            btnLoadAll.Height = 42;
            AddControlToTable(tbl, btnLoadAll);

            lblStatus = new Label { Text = "Ready", ForeColor = GREEN, Height = 40, TextAlign = ContentAlignment.BottomLeft };
            AddControlToTable(tbl, lblStatus);

            // --- MAIN CONTENT (GRID) ---
            Panel mainContent = new() { Dock = DockStyle.Fill, Padding = new Padding(25), BackColor = BG };
            Controls.Add(mainContent);
            Controls.Add(sidebar);

            dgvDrivers = MakeGrid();
            dgvDrivers.Dock = DockStyle.Fill;
            mainContent.Controls.Add(dgvDrivers);

            // Events
            btnAdd.Click += AddDriver;
            btnUpdate.Click += UpdateDriver;
            btnDelete.Click += DeleteDriver;
            btnClear.Click += ClearFields;
            btnSearch.Click += SearchDriver;
            btnLoadAll.Click += async (_, _) => await LoadDriversAsync();
            dgvDrivers.CellClick += FillFieldsFromGrid;
        }

        private bool ValidateDriver()
        {
            return FormUtilities.RequireText(txtDriverName, "Driver name")
                && FormUtilities.RequireText(txtLicense, "License number")
                && FormUtilities.RequireText(txtDriverPhone, "Phone number")
                && FormUtilities.RequireText(txtPlateNumber, "Plate number")
                && FormUtilities.RequireText(txtCarModel, "Car model");
        }

        private async void AddDriver(object? sender, EventArgs e)
        {
            if (!ValidateDriver()) return;

            try
            {
                await Database.ExecuteAsync(
                    """
            INSERT INTO Drivers (DriverName, LicenseNumber, PhoneNumber)
            VALUES (@DriverName, @LicenseNumber, @PhoneNumber);

            DECLARE @NewDriverID int = CONVERT(int, SCOPE_IDENTITY());
            INSERT INTO Vehicles (PlateNumber, CarModel)
            VALUES (@PlateNumber, @CarModel);

            DECLARE @NewVehicleID int = CONVERT(int, SCOPE_IDENTITY());
            INSERT INTO Owns (DriverID, VehicleID)
            VALUES (@NewDriverID, @NewVehicleID);
            """,
                    Database.Parameter("@DriverName", SqlDbType.NVarChar, 100, txtDriverName.Text.Trim()),
                    Database.Parameter("@LicenseNumber", SqlDbType.NVarChar, 50, txtLicense.Text.Trim()),
                    Database.Parameter("@PhoneNumber", SqlDbType.NVarChar, 30, txtDriverPhone.Text.Trim()),
                    Database.Parameter("@PlateNumber", SqlDbType.NVarChar, 30, txtPlateNumber.Text.Trim()),
                    Database.Parameter("@CarModel", SqlDbType.NVarChar, 100, txtCarModel.Text.Trim()));

                ClearFields(sender, e);
                await LoadDriversAsync();
                lblStatus.Text = "Driver added";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Add driver", ex);
            }
        }

        private async void UpdateDriver(object? sender, EventArgs e)
        {
            if (selectedDriverId is null)
            {
                MessageBox.Show("Select a driver first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateDriver()) return;

            try
            {
                await Database.ExecuteAsync(
                    """
            UPDATE Drivers
            SET DriverName = @DriverName, LicenseNumber = @LicenseNumber, PhoneNumber = @PhoneNumber
            WHERE DriverID = @DriverID;

            UPDATE v
            SET PlateNumber = @PlateNumber, CarModel = @CarModel
            FROM Vehicles v
            INNER JOIN Owns o ON o.VehicleID = v.VehicleID
            WHERE o.DriverID = @DriverID;
            """,
                    Database.Parameter("@DriverID", SqlDbType.Int, selectedDriverId.Value),
                    Database.Parameter("@DriverName", SqlDbType.NVarChar, 100, txtDriverName.Text.Trim()),
                    Database.Parameter("@LicenseNumber", SqlDbType.NVarChar, 50, txtLicense.Text.Trim()),
                    Database.Parameter("@PhoneNumber", SqlDbType.NVarChar, 30, txtDriverPhone.Text.Trim()),
                    Database.Parameter("@PlateNumber", SqlDbType.NVarChar, 30, txtPlateNumber.Text.Trim()),
                    Database.Parameter("@CarModel", SqlDbType.NVarChar, 100, txtCarModel.Text.Trim()));

                await LoadDriversAsync();
                lblStatus.Text = "Driver updated";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Update driver", ex);
            }
        }

        private async void DeleteDriver(object? sender, EventArgs e)
        {
            if (selectedDriverId is null)
            {
                MessageBox.Show("Select a driver first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Delete selected driver and vehicle?", "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                await Database.ExecuteAsync(
                    """
                    DECLARE @VehicleIds TABLE (VehicleID int);
                    INSERT INTO @VehicleIds (VehicleID)
                    SELECT VehicleID FROM Owns WHERE DriverID = @DriverID;

                    DELETE FROM Reviews WHERE RideID IN (SELECT RideID FROM Rides WHERE DriverID = @DriverID);
                    DELETE FROM Payments WHERE RideID IN (SELECT RideID FROM Rides WHERE DriverID = @DriverID);
                    DELETE FROM Rides WHERE DriverID = @DriverID;
                    DELETE FROM Owns WHERE DriverID = @DriverID;
                    DELETE FROM Vehicles WHERE VehicleID IN (SELECT VehicleID FROM @VehicleIds);
                    DELETE FROM Drivers WHERE DriverID = @DriverID;
                    """,
                    Database.Parameter("@DriverID", SqlDbType.Int, selectedDriverId.Value));
                ClearFields(sender, e);
                await LoadDriversAsync();
                lblStatus.Text = "Driver deleted";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Delete driver", ex);
            }
        }

        private void ClearFields(object? sender, EventArgs e)
        {
            selectedDriverId = null;
            txtDriverName.Clear();
            txtLicense.Clear();
            txtDriverPhone.Clear();
            txtPlateNumber.Clear();
            txtCarModel.Clear();
            txtSearchID.Clear();
            lblStatus.Text = "Ready";
        }

        private async void SearchDriver(object? sender, EventArgs e)
        {
            if (!FormUtilities.TryReadInt(txtSearchID, "Driver ID", out int id)) return;

            try
            {
                DataTable table = await LoadDriversTableAsync("WHERE d.DriverID = @DriverID",
                    Database.Parameter("@DriverID", SqlDbType.Int, id));
                FormUtilities.Bind(dgvDrivers, table, Headers);
                lblStatus.Text = table.Rows.Count == 0 ? "No driver found" : "Driver found";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Search driver", ex);
            }
        }

        private void FillFieldsFromGrid(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvDrivers.Rows[e.RowIndex];
            selectedDriverId = int.TryParse(FormUtilities.CellText(row, "DriverID"), out int id) ? id : null;
            txtDriverName.Text = FormUtilities.CellText(row, "DriverName");
            txtLicense.Text = FormUtilities.CellText(row, "LicenseNumber");
            txtDriverPhone.Text = FormUtilities.CellText(row, "PhoneNumber");
            txtPlateNumber.Text = FormUtilities.CellText(row, "PlateNumber");
            txtCarModel.Text = FormUtilities.CellText(row, "CarModel");
        }

        private async Task LoadDriversAsync()
        {
            try
            {
                DataTable table = await LoadDriversTableAsync(string.Empty);
                FormUtilities.Bind(dgvDrivers, table, Headers);
                lblStatus.Text = $"Loaded {table.Rows.Count} drivers";
            }
            catch (Exception ex) { FormUtilities.ShowError(lblStatus, "Load drivers", ex); }
        }

        private static Task<DataTable> LoadDriversTableAsync(string whereClause, params SqlParameter[] parameters)
        {
            return Database.QueryAsync(
                $"""
                SELECT d.DriverID, d.DriverName, d.LicenseNumber, d.PhoneNumber,
                       v.PlateNumber, v.CarModel
                FROM Drivers d
                LEFT JOIN Owns o ON o.DriverID = d.DriverID
                LEFT JOIN Vehicles v ON v.VehicleID = o.VehicleID
                {whereClause}
                ORDER BY d.DriverID
                """, parameters);
        }

        // --- HELPER UI METHODS (The "User-Like" Look) ---
        private void AddControlToTable(TableLayoutPanel tbl, Control ctrl)
        {
            ctrl.Dock = DockStyle.Top;
            if (ctrl.Margin == Padding.Empty) ctrl.Margin = new Padding(0, 0, 0, 8);
            tbl.Controls.Add(ctrl);
        }

        private Label MakeSectionLabel(string text) => new() { Text = text, ForeColor = ACCENT, Height = 28, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        private Label MakeFieldLabel(string text) => new() { Text = text, ForeColor = LABEL_COLOR, Height = 20, TextAlign = ContentAlignment.BottomLeft };
        private TextBox MakeTextBox() => new() { BackColor = CARD, ForeColor = TXT, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 11F) };

        private TableLayoutPanel MakeTwoColumnRow()
        {
            TableLayoutPanel p = new() { ColumnCount = 2, Dock = DockStyle.Top, Height = 48 };
            p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            return p;
        }

        private Button MakeBtn(string text, Color bg) =>
            new() { Text = text, BackColor = bg, ForeColor = Color.White, Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat, Height = 40, Margin = new Padding(2) };

        private DataGridView MakeGrid()
        {
            DataGridView dg = new() { 
                BackgroundColor = CARD, 
                BorderStyle = BorderStyle.None, 
                GridColor = Color.FromArgb(50, 50, 70),
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