using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class DashBoard : Form
    {
        public DashBoard()
        {
            InitializeComponent();
            BuildDashboard();
        }

        private void BuildDashboard()
        {
            Text = "Riding Application System - Dashboard";
            Size = new Size(600, 500);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(30, 30, 30);

            Label title = new()
            {
                Text = "Riding Application System",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(130, 40)
            };
            Controls.Add(title);

            Label subtitle = new()
            {
                Text = "Admin Dashboard - Select a module to manage",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.LightGray,
                AutoSize = true,
                Location = new Point(165, 80)
            };
            Controls.Add(subtitle);

            string[] names = { "Users", "Drivers", "Rides", "Payments", "Reviews" };
            for (int i = 0; i < names.Length; i++)
            {
                Button btn = new()
                {
                    Text = names[i],
                    Size = new Size(200, 50),
                    Location = new Point(190, 140 + i * 65),
                    Font = new Font("Segoe UI", 11),
                    BackColor = Color.FromArgb(0, 120, 215),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    Tag = names[i]
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += NavButton_Click;
                Controls.Add(btn);
            }
        }

        private void NavButton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button clicked || clicked.Tag is not string tag)
            {
                return;
            }

            Form? targetForm = tag switch
            {
                "Users" => new UsersForm(WindowState),
                "Drivers" => new DriversForm(WindowState),
                "Rides" => new RidesForm { WindowState = WindowState },
                "Payments" => new PaymentsForm { WindowState = WindowState },
                "Reviews" => new ReviewsForm { WindowState = WindowState },
                _ => null
            };

            if (targetForm is null)
            {
                return;
            }

            targetForm.FormClosed += (_, _) => Show();
            targetForm.Show();
            Hide();
        }
    }
}
