using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace CarWashManagementSystem
{
    internal static class UiTheme
    {
        private static readonly Color LegacyCyan = Color.FromArgb(17, 205, 239);
        private static readonly Color Navy = Color.FromArgb(26, 43, 64);
        private static readonly Color Teal = Color.FromArgb(20, 145, 160);
        private static readonly Color Page = Color.FromArgb(246, 249, 251);
        private static readonly ConditionalWeakTable<Control, object> StyledControls = new ConditionalWeakTable<Control, object>();

        public static void StyleOpenForms(object sender, EventArgs e)
        {
            foreach (Form form in Application.OpenForms)
                StyleTree(form);
        }

        private static void StyleTree(Control control)
        {
            if (!StyledControls.TryGetValue(control, out _))
            {
                StyledControls.Add(control, new object());
                StyleControl(control);
            }

            foreach (Control child in control.Controls)
                StyleTree(child);
        }

        private static void StyleControl(Control control)
        {
            control.Font = new Font("Segoe UI", control.Font.Size, control.Font.Style, control.Font.Unit);

            if (control is Form)
                control.BackColor = Page;
            else if (control.BackColor.ToArgb() == LegacyCyan.ToArgb())
                control.BackColor = Navy;

            if (control.ForeColor.ToArgb() == LegacyCyan.ToArgb())
                control.ForeColor = Navy;
            else if (control.ForeColor == SystemColors.AppWorkspace)
                control.ForeColor = Color.FromArgb(104, 119, 131);

            if (control is Button button)
            {
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                if (button.BackColor.ToArgb() == Navy.ToArgb() || button.BackColor.ToArgb() == LegacyCyan.ToArgb())
                {
                    button.BackColor = Teal;
                    button.ForeColor = Color.White;
                }
                button.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 62, 88);
            }

            if (control is DataGridView grid)
            {
                grid.EnableHeadersVisualStyles = false;
                grid.ColumnHeadersDefaultCellStyle.BackColor = Navy;
                grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Navy;
                grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(213, 238, 239);
                grid.DefaultCellStyle.SelectionForeColor = Navy;
                grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(244, 248, 250);
                grid.RowTemplate.Height = Math.Max(grid.RowTemplate.Height, 32);
            }
        }
    }
}
