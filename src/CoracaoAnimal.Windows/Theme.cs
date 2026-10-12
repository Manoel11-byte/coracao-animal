namespace CoracaoAnimal.Windows;

internal static class Theme
{
    internal static readonly Color Orange = ColorTranslator.FromHtml("#E05C2A");
    internal static readonly Color Green = ColorTranslator.FromHtml("#4A7C59");
    internal static readonly Color Brown = ColorTranslator.FromHtml("#3D2B1F");
    internal static readonly Color Beige = ColorTranslator.FromHtml("#F7F3EE");
    internal static readonly Color Muted = ColorTranslator.FromHtml("#7A6355");

    internal static void Button(Button button, bool navigation = false)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = navigation ? Brown : Orange;
        button.ForeColor = Color.White;
        button.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
        button.MinimumSize = new Size(112, 42);
        button.FlatAppearance.MouseOverBackColor = Green;
    }

    internal static void Field(Control control)
    {
        control.Font = new Font("Segoe UI", 11);
        control.BackColor = Color.White;
        control.ForeColor = Brown;
        control.Margin = new Padding(0, 4, 0, 12);
        control.MinimumSize = new Size(control.MinimumSize.Width, 38);
    }
}
