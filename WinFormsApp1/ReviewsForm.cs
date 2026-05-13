using System.Data;
using System.Drawing;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class ReviewsForm : Form
    {
        public DataGridView dgvReviews = null!;
        public TextBox txtRideID = null!, txtComment = null!, txtSearchID = null!;
        public ComboBox cmbRating = null!;
        public Button btnAdd = null!, btnUpdate = null!, btnDelete = null!, btnClear = null!, btnSearch = null!, btnLoadAll = null!;
        public Label lblStatus = null!;

        private readonly Color BG = Color.FromArgb(18, 18, 24);
        private readonly Color PANEL = Color.FromArgb(28, 28, 38);
        private readonly Color CARD = Color.FromArgb(38, 38, 52);
        private readonly Color ACCENT = Color.FromArgb(255, 183, 77);
        private readonly Color GREEN = Color.FromArgb(72, 199, 142);
        private readonly Color RED = Color.FromArgb(252, 95, 95);
        private readonly Color ORANGE = Color.FromArgb(251, 176, 64);
        private readonly Color TXT = Color.FromArgb(220, 220, 235);
        private readonly Color SUBTLE = Color.FromArgb(120, 120, 145);

        private int? selectedRideId;

        private static readonly Dictionary<string, string> Headers = new()
        {
            ["RideID"] = "Ride ID",
            ["Rating"] = "Rating",
            ["Comment"] = "Comment"
        };

        public ReviewsForm()
        {
            Text = "Reviews Management";
            Size = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = BG;
            ForeColor = TXT;
            Font = new Font("Segoe UI", 10);
            MinimumSize = new Size(900, 600);

            BuildUI();
            Load += async (_, _) => await LoadReviewsAsync();
        }

        private void BuildUI()
        {

            Panel left = new() { Width = 300, Dock = DockStyle.Left, BackColor = PANEL };
            

            int y = 70;
            Label section = new() { Text = "REVIEW DETAILS", ForeColor = SUBTLE, Location = new Point(15, y), AutoSize = true };
            left.Controls.Add(section);
            y += 30;

            txtRideID = AddField(left, "Ride ID", ref y);

            left.Controls.Add(MakeLabel("Rating (1-5)", new Point(15, y)));
            y += 22;
            cmbRating = new ComboBox { Location = new Point(15, y), Width = 260, BackColor = CARD, ForeColor = TXT, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRating.Items.AddRange(new object[] { "1", "2", "3", "4", "5" });
            left.Controls.Add(cmbRating);
            y += 45;

            left.Controls.Add(MakeLabel("Comment", new Point(15, y)));
            y += 22;
            txtComment = new TextBox { Location = new Point(15, y), Width = 260, Height = 80, Multiline = true, BackColor = CARD, ForeColor = TXT, BorderStyle = BorderStyle.FixedSingle };
            left.Controls.Add(txtComment);
            y += 100;

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

            left.Controls.Add(MakeLabel("Search by Ride ID", new Point(15, y)));
            y += 22;
            txtSearchID = MakeTextBox(new Point(15, y), 160);
            left.Controls.Add(txtSearchID);
            btnSearch = MakeButton("Search", ACCENT, new Point(182, y), 93);
            left.Controls.Add(btnSearch);
            y += 50;

            btnLoadAll = new Button { Text = "Load All Reviews", BackColor = PANEL, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand, Location = new Point(15, y), Width = 260, Height = 38 };
            btnLoadAll.FlatAppearance.BorderColor = ACCENT;
            btnLoadAll.FlatAppearance.BorderSize = 0;
            btnLoadAll.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            left.Controls.Add(btnLoadAll);

            lblStatus = new Label { Text = "Ready", ForeColor = SUBTLE, Location = new Point(15, 630), AutoSize = true };
            left.Controls.Add(lblStatus);

            Panel right = new() { Dock = DockStyle.Fill, BackColor = BG, Padding = new Padding(15) };
            Controls.Add(right);
            Controls.Add(left);

            dgvReviews = MakeGrid();
            dgvReviews.Dock = DockStyle.Fill;
            right.Controls.Add(dgvReviews);

            btnAdd.Click += AddReview;
            btnUpdate.Click += UpdateReview;
            btnDelete.Click += DeleteReview;
            btnClear.Click += ClearFields;
            btnSearch.Click += SearchReview;
            btnLoadAll.Click += async (_, _) => await LoadReviewsAsync();
            dgvReviews.CellClick += FillFieldsFromGrid;
        }

        private async void AddReview(object? sender, EventArgs e)
        {
            if (!ValidateReview(out int rideId, out int rating)) return;

            try
            {
                await Database.ExecuteAsync(
                    """
                    INSERT INTO Reviews (RideID, Rating, Comment)
                    VALUES (@RideID, @Rating, @Comment)
                    """,
                    Database.Parameter("@RideID", SqlDbType.Int, rideId),
                    Database.Parameter("@Rating", SqlDbType.Int, rating),
                    Database.Parameter("@Comment", SqlDbType.NVarChar, 1000, EmptyToNull(txtComment.Text)));

                ClearFields(sender, e);
                await LoadReviewsAsync();
                lblStatus.Text = "Review added";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Add review", ex);
            }
        }

        private async void UpdateReview(object? sender, EventArgs e)
        {
            if (selectedRideId is null)
            {
                MessageBox.Show("Select a review first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateReview(out int rideId, out int rating)) return;

            try
            {
                await Database.ExecuteAsync(
                    """
                    UPDATE Reviews
                    SET RideID = @RideID, Rating = @Rating, Comment = @Comment
                    WHERE RideID = @OriginalRideID
                    """,
                    Database.Parameter("@OriginalRideID", SqlDbType.Int, selectedRideId.Value),
                    Database.Parameter("@RideID", SqlDbType.Int, rideId),
                    Database.Parameter("@Rating", SqlDbType.Int, rating),
                    Database.Parameter("@Comment", SqlDbType.NVarChar, 1000, EmptyToNull(txtComment.Text)));

                await LoadReviewsAsync();
                lblStatus.Text = "Review updated";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Update review", ex);
            }
        }

        private async void DeleteReview(object? sender, EventArgs e)
        {
            if (selectedRideId is null)
            {
                MessageBox.Show("Select a review first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Delete selected review?", "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                await Database.ExecuteAsync("DELETE FROM Reviews WHERE RideID = @RideID",
                    Database.Parameter("@RideID", SqlDbType.Int, selectedRideId.Value));
                ClearFields(sender, e);
                await LoadReviewsAsync();
                lblStatus.Text = "Review deleted";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Delete review", ex);
            }
        }

        private void ClearFields(object? sender, EventArgs e)
        {
            selectedRideId = null;
            txtRideID.Clear();
            cmbRating.SelectedIndex = -1;
            txtComment.Clear();
            txtSearchID.Clear();
            lblStatus.Text = "Ready";
        }

        private async void SearchReview(object? sender, EventArgs e)
        {
            if (!FormUtilities.TryReadInt(txtSearchID, "Ride ID", out int id)) return;

            try
            {
                DataTable table = await Database.QueryAsync(
                    """
                    SELECT RideID, Rating, Comment
                    FROM Reviews
                    WHERE RideID = @RideID
                    """,
                    Database.Parameter("@RideID", SqlDbType.Int, id));
                FormUtilities.Bind(dgvReviews, table, Headers);
                lblStatus.Text = table.Rows.Count == 0 ? "No review found" : "Review found";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Search review", ex);
            }
        }

        private void FillFieldsFromGrid(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvReviews.Rows[e.RowIndex];
            selectedRideId = int.TryParse(FormUtilities.CellText(row, "RideID"), out int id) ? id : null;
            txtRideID.Text = FormUtilities.CellText(row, "RideID");
            cmbRating.Text = FormUtilities.CellText(row, "Rating");
            txtComment.Text = FormUtilities.CellText(row, "Comment");
        }

        private async Task LoadReviewsAsync()
        {
            try
            {
                DataTable table = await Database.QueryAsync(
                    """
                    SELECT RideID, Rating, Comment
                    FROM Reviews
                    ORDER BY RideID
                    """);
                FormUtilities.Bind(dgvReviews, table, Headers);
                lblStatus.Text = $"Loaded {table.Rows.Count} reviews";
            }
            catch (Exception ex)
            {
                FormUtilities.ShowError(lblStatus, "Load reviews", ex);
            }
        }

        private bool ValidateReview(out int rideId, out int rating)
        {
            rideId = rating = default;
            return FormUtilities.TryReadInt(txtRideID, "Ride ID", out rideId)
                && int.TryParse(cmbRating.SelectedItem?.ToString() ?? string.Empty, out rating)
                && rating >= 1 && rating <= 5;
        }

        private string? EmptyToNull(string? text) => string.IsNullOrWhiteSpace(text) ? null : text;

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
