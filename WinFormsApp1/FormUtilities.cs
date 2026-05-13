using System.Data;
using System.Globalization;
using System.Windows.Forms;

namespace WinFormsApp1;

internal static class FormUtilities
{
    internal static void ConfigureGrid(DataGridView grid)
    {
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AutoGenerateColumns = true;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
        grid.DataError += (_, e) => e.ThrowException = false;
        grid.DefaultCellStyle.NullValue = "";
        grid.MultiSelect = false;
        grid.ReadOnly = true;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
    }

    internal static void Bind(DataGridView grid, DataTable table, IReadOnlyDictionary<string, string> headers)
    {
        grid.DataSource = table;

        foreach (DataGridViewColumn column in grid.Columns)
        {
            column.HeaderText = headers.TryGetValue(column.Name, out string? header) ? header : SplitName(column.Name);
            column.DefaultCellStyle.NullValue = "";

            if (column.ValueType == typeof(DateTime))
            {
                column.DefaultCellStyle.Format = "yyyy-MM-dd";
            }
            else if (column.ValueType == typeof(decimal) || column.ValueType == typeof(double) || column.ValueType == typeof(float))
            {
                column.DefaultCellStyle.Format = "N2";
            }
        }
    }

    internal static string? CellText(DataGridViewRow row, string columnName)
    {
        return row.DataGridView?.Columns.Contains(columnName) == true
            ? Convert.ToString(row.Cells[columnName].Value, CultureInfo.CurrentCulture)
            : string.Empty;
    }

    internal static bool TryReadInt(TextBox textBox, string fieldName, out int value)
    {
        if (int.TryParse(textBox.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value) && value > 0)
        {
            return true;
        }

        MessageBox.Show($"{fieldName} must be a positive whole number.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        textBox.Focus();
        return false;
    }

    internal static bool TryReadDecimal(TextBox textBox, string fieldName, out decimal value)
    {
        if (decimal.TryParse(textBox.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out value) && value >= 0)
        {
            return true;
        }

        MessageBox.Show($"{fieldName} must be a valid non-negative amount.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        textBox.Focus();
        return false;
    }

    internal static bool TryReadDate(TextBox textBox, string fieldName, out DateTime value)
    {
        if (DateTime.TryParse(textBox.Text.Trim(), CultureInfo.CurrentCulture, DateTimeStyles.None, out value))
        {
            value = value.Date;
            return true;
        }

        MessageBox.Show($"{fieldName} must be a valid date.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        textBox.Focus();
        return false;
    }

    internal static bool RequireText(TextBox textBox, string fieldName)
    {
        if (!string.IsNullOrWhiteSpace(textBox.Text))
        {
            return true;
        }

        MessageBox.Show($"{fieldName} is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        textBox.Focus();
        return false;
    }

    internal static void ShowError(Label status, string action, Exception ex)
    {
        status.Text = $"{action} failed";
        MessageBox.Show(ex.Message, action, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private static string SplitName(string value)
    {
        return System.Text.RegularExpressions.Regex.Replace(value, "([a-z])([A-Z])", "$1 $2");
    }
}
