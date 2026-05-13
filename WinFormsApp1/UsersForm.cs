using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class UsersForm : Form
    {
        public DataGridView dgvUsers = null!;
        public TextBox txtFirstName = null!, txtLastName = null!, txtEmail = null!, txtDOB = null!, txtPhone = null!, txtSearchID = null!;
        public Button btnAdd = null!, btnUpdate = null!, btnDelete = null!, btnSearch = null!, btnClear = null!, btnLoadAll = null!;
        public Label lblStatus = null!;

        private readonly Color BG = Color.FromArgb(18, 18, 24);
        private readonly Color PANEL = Color.FromArgb(28, 28, 38);
        private readonly Color CARD = Color.FromArgb(38, 38, 52);
        private readonly Color ACCENT = Color.FromArgb(99, 179, 237);
        private readonly Color GREEN = Color.FromArgb(72, 199, 142);
        private readonly Color RED = Color.FromArgb(252, 95, 95);
        private readonly Color ORANGE = Color.FromArgb(251, 176, 64);
        private readonly Color TXT = Color.FromArgb(220, 220, 235);
        private readonly Color LABEL_COLOR = Color.FromArgb(170, 170, 195);

        private int? selectedUserId;

        private static readonly Dictionary<string, string> Headers = new()
        {
            ["UserID"] = "User ID",
            ["FirstName"] = "First Name",
            ["LastName"] = "Last Name",
            ["Email"] = "Email",
            ["DateOfBirth"] = "Date of Birth",
            ["Phone_Number"] = "Phone"
        };

        public UsersForm(FormWindowState callerState)
        {
            Text = "Users Management System";
            Size = new Size(1280, 850);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = BG;
            ForeColor = TXT;
            MinimumSize = new Size(1100, 700);
            WindowState = callerState;
            Font = new Font("Segoe UI", 10F);

            BuildUI();
            Load += async (_, _) => await LoadUsersAsync();
        }

            private void BuildUI()
            {        
                Panel sidebar = new() { Width = 400, Dock = DockStyle.Left, BackColor = PANEL, Padding = new Padding(10) };
                Controls.Add(sidebar);

                // Added a container to handle the sidebar scroll better
                Panel scrollContainer = new() { Dock = DockStyle.Fill, AutoScroll = true };
                sidebar.Controls.Add(scrollContainer);

                TableLayoutPanel tbl = new()
                {
                    ColumnCount = 1,
                    Dock = DockStyle.Top, // Allows scrolling
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Padding = new Padding(15),
                    BackColor = PANEL
                };
                scrollContainer.Controls.Add(tbl);

                // USER DETAILS SECTION
                AddControlToTable(tbl, MakeSectionLabel("USER DETAILS"));

                AddControlToTable(tbl, MakeFieldLabel("First Name"));
                txtFirstName = MakeTextBox(); AddControlToTable(tbl, txtFirstName);

                AddControlToTable(tbl, MakeFieldLabel("Last Name"));
                txtLastName = MakeTextBox(); AddControlToTable(tbl, txtLastName);

                AddControlToTable(tbl, MakeFieldLabel("Email Address"));
                txtEmail = MakeTextBox(); AddControlToTable(tbl, txtEmail);

                AddControlToTable(tbl, MakeFieldLabel("Date of Birth"));
                txtDOB = MakeTextBox(); AddControlToTable(tbl, txtDOB);

                AddControlToTable(tbl, MakeFieldLabel("Phone Number"));
                txtPhone = MakeTextBox(); AddControlToTable(tbl, txtPhone);

                // ACTIONS SECTION (Replaced empty panel with Margin)
                var lblActions = MakeSectionLabel("ACTIONS");
                lblActions.Margin = new Padding(0, 25, 0, 5); // Spacing above the label
                AddControlToTable(tbl, lblActions);

                TableLayoutPanel btnRow1 = MakeTwoColumnRow();
                btnAdd = MakeBtn("Add User", GREEN);
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

                // SEARCH SECTION
                var lblSearch = MakeSectionLabel("SEARCH BY USER ID");
                lblSearch.Margin = new Padding(0, 25, 0, 5);
                AddControlToTable(tbl, lblSearch);

                TableLayoutPanel searchRow = new() { Height = 42, ColumnCount = 2, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 10) };
                searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
                searchRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
                txtSearchID = MakeTextBox();
                btnSearch = MakeBtn("Search", ACCENT);
                searchRow.Controls.Add(txtSearchID, 0, 0);
                searchRow.Controls.Add(btnSearch, 1, 0);
                AddControlToTable(tbl, searchRow);

                btnLoadAll = MakeBtn("Load All Users", ACCENT);
                btnLoadAll.Height = 42;
                AddControlToTable(tbl, btnLoadAll);

                lblStatus = new Label { Text = "Ready", ForeColor = GREEN, Height = 40, TextAlign = ContentAlignment.BottomLeft };
                AddControlToTable(tbl, lblStatus);

                // MAIN GRID AREA
                Panel mainContent = new() { Dock = DockStyle.Fill, Padding = new Padding(25), BackColor = BG };
                Controls.Add(mainContent);
                Controls.Add(sidebar);

                dgvUsers = MakeGrid();
                dgvUsers.Dock = DockStyle.Fill;
                mainContent.Controls.Add(dgvUsers);

                btnAdd.Click += AddUser;
                btnUpdate.Click += UpdateUser;
                btnDelete.Click += DeleteUser;
                btnClear.Click += ClearFields;
                btnSearch.Click += SearchUser;
                btnLoadAll.Click += async (_, _) => await LoadUsersAsync();
                dgvUsers.CellClick += FillFieldsFromGrid;
            }

        private async void AddUser(object? sender, EventArgs e)
        {
            if (!ValidateUser(out DateTime dob)) return;

            try
            {
                await Database.ExecuteAsync(
                    """
                    INSERT INTO Users (FirstName, LastName, Email, DateOfBirth)
                    VALUES (@FirstName, @LastName, @Email, @DateOfBirth);

                    DECLARE @NewUserID int = CONVERT(int, SCOPE_IDENTITY());
                    INSERT INTO UsersPhones (UserID, Phone_Number)
                    VALUES (@NewUserID, @Phone);
                    """,
                    Database.Parameter("@FirstName", SqlDbType.NVarChar, 50, txtFirstName.Text.Trim()),
                    Database.Parameter("@LastName", SqlDbType.NVarChar, 50, txtLastName.Text.Trim()),
                    Database.Parameter("@Email", SqlDbType.NVarChar, 100, txtEmail.Text.Trim()),
                    Database.Parameter("@DateOfBirth", SqlDbType.Date, dob),
                    Database.Parameter("@Phone", SqlDbType.NVarChar, 30, txtPhone.Text.Trim()));

                ClearFields(sender, e);
                await LoadUsersAsync();
                lblStatus.Text = "User added";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Add user", ex);
            }
        }

        private async void UpdateUser(object? sender, EventArgs e)
        {
            if (selectedUserId is null)
            {
                MessageBox.Show("Select a user first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateUser(out DateTime dob)) return;

            try
            {
                await Database.ExecuteAsync(
                    """
                    UPDATE Users
                    SET FirstName = @FirstName, LastName = @LastName, Email = @Email, DateOfBirth = @DateOfBirth
                    WHERE UserID = @UserID;

                    IF EXISTS (SELECT 1 FROM UsersPhones WHERE UserID = @UserID)
                        UPDATE UsersPhones SET Phone_Number = @Phone WHERE UserID = @UserID;
                    ELSE
                        INSERT INTO UsersPhones (UserID, Phone_Number) VALUES (@UserID, @Phone);
                    """,
                    Database.Parameter("@FirstName", SqlDbType.NVarChar, 50, txtFirstName.Text.Trim()),
                    Database.Parameter("@LastName", SqlDbType.NVarChar, 50, txtLastName.Text.Trim()),
                    Database.Parameter("@Email", SqlDbType.NVarChar, 100, txtEmail.Text.Trim()),
                    Database.Parameter("@DateOfBirth", SqlDbType.Date, dob),
                    Database.Parameter("@Phone", SqlDbType.NVarChar, 30, txtPhone.Text.Trim()),
                    Database.Parameter("@UserID", SqlDbType.Int, selectedUserId.Value));

                await LoadUsersAsync();
                lblStatus.Text = "User updated";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Update user", ex);
            }
        }

        private async void DeleteUser(object? sender, EventArgs e)
        {
            if (selectedUserId is null)
            {
                MessageBox.Show("Select a user first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Delete selected user?", "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                await Database.ExecuteAsync(
                    """
                    DELETE FROM Reviews WHERE RideID IN (SELECT RideID FROM Rides WHERE UserID = @UserID);
                    DELETE FROM Payments WHERE RideID IN (SELECT RideID FROM Rides WHERE UserID = @UserID);
                    DELETE FROM Rides WHERE UserID = @UserID;
                    DELETE FROM UsersPhones WHERE UserID = @UserID;
                    DELETE FROM Users WHERE UserID = @UserID;
                    """,
                    Database.Parameter("@UserID", SqlDbType.Int, selectedUserId.Value));
                ClearFields(sender, e);
                await LoadUsersAsync();
                lblStatus.Text = "User deleted";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Delete user", ex);
            }
        }

        private void ClearFields(object? sender, EventArgs e)
        {
            selectedUserId = null;
            txtFirstName.Clear();
            txtLastName.Clear();
            txtEmail.Clear();
            txtDOB.Clear();
            txtPhone.Clear();
            txtSearchID.Clear();
            lblStatus.Text = "Ready";
        }

        private async void SearchUser(object? sender, EventArgs e)
        {
            if (!FormUtilities.TryReadInt(txtSearchID, "User ID", out int id)) return;

            try
            {
                DataTable table = await Database.QueryAsync(
                    """
                    SELECT u.UserID, u.FirstName, u.LastName, u.Email, u.DateOfBirth,
                           COALESCE(MIN(up.Phone_Number), '') AS Phone_Number
                    FROM Users u
                    LEFT JOIN UsersPhones up ON up.UserID = u.UserID
                    WHERE u.UserID = @UserID
                    GROUP BY u.UserID, u.FirstName, u.LastName, u.Email, u.DateOfBirth
                    """,
                    Database.Parameter("@UserID", SqlDbType.Int, id));

                FormUtilities.Bind(dgvUsers, table, Headers);
                lblStatus.Text = table.Rows.Count == 0 ? "No user found" : "User found";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Search user", ex);
            }
        }

        private void FillFieldsFromGrid(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvUsers.Rows[e.RowIndex];
            selectedUserId = int.TryParse(FormUtilities.CellText(row, "UserID"), out int id) ? id : null;
            txtFirstName.Text = FormUtilities.CellText(row, "FirstName");
            txtLastName.Text = FormUtilities.CellText(row, "LastName");
            txtEmail.Text = FormUtilities.CellText(row, "Email");
            txtDOB.Text = FormUtilities.CellText(row, "DateOfBirth");
            txtPhone.Text = FormUtilities.CellText(row, "Phone_Number");
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                DataTable table = await Database.QueryAsync(
                    """
                    SELECT u.UserID, u.FirstName, u.LastName, u.Email, u.DateOfBirth,
                           COALESCE(MIN(up.Phone_Number), '') AS Phone_Number
                    FROM Users u
                    LEFT JOIN UsersPhones up ON up.UserID = u.UserID
                    GROUP BY u.UserID, u.FirstName, u.LastName, u.Email, u.DateOfBirth
                    ORDER BY u.UserID
                    """);
                FormUtilities.Bind(dgvUsers, table, Headers);
                lblStatus.Text = $"Loaded {table.Rows.Count} users";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Load users", ex);
            }
        }

        private bool ValidateUser(out DateTime dob)
        {
            dob = default;
            return FormUtilities.RequireText(txtFirstName, "First name")
                && FormUtilities.RequireText(txtLastName, "Last name")
                && FormUtilities.RequireText(txtEmail, "Email")
                && FormUtilities.TryReadDate(txtDOB, "Date of birth", out dob)
                && FormUtilities.RequireText(txtPhone, "Phone");
        }

        private DataGridView MakeGrid()
        {
            DataGridView dg = new()
            {
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

        private void AddControlToTable(TableLayoutPanel tbl, Control ctrl)
        {
            ctrl.Dock = DockStyle.Top;
            tbl.Controls.Add(ctrl);
        }

        private Label MakeSectionLabel(string text) => new() { Text = text, ForeColor = ACCENT, Height = 28 };

        private Label MakeFieldLabel(string text) => new() { Text = text, ForeColor = LABEL_COLOR, Height = 24 };

        private TextBox MakeTextBox() => new() { BackColor = CARD, ForeColor = TXT, Height = 35, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Top };

        private TableLayoutPanel MakeTwoColumnRow()
        {
            TableLayoutPanel p = new() { ColumnCount = 2, Dock = DockStyle.Top, Height = 45, AutoSize = false };
            p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            return p;
        }

        private Button MakeBtn(string text, Color bg) =>
            new() { Text = text, BackColor = bg, ForeColor = Color.White, Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat, Height = 40, MinimumSize = new Size(0, 40) };
    }
}
