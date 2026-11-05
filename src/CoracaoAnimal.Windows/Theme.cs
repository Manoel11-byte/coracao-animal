using System.Drawing.Drawing2D;
namespace CoracaoAnimal.Windows;

internal static class Theme
{
 internal static readonly Color Green=ColorTranslator.FromHtml("#4A7C59");
 internal static readonly Color Brown=ColorTranslator.FromHtml("#2A1F14");
 internal static readonly Color Beige=ColorTranslator.FromHtml("#F7F3EE");
 internal static readonly Color Muted=ColorTranslator.FromHtml("#7A6355");
 internal static readonly Color Orange=ColorTranslator.FromHtml("#B8431B");
 internal static readonly Color Line=ColorTranslator.FromHtml("#E5D9CC");
 internal static readonly Color Soft=ColorTranslator.FromHtml("#EFE9E1");
 internal static readonly Color Sidebar=ColorTranslator.FromHtml("#3D2B1F");
 internal static readonly Color SidebarHover=ColorTranslator.FromHtml("#513A2A");
 internal static readonly Font BodyFont=new("Segoe UI",10.5f);
 internal static readonly Font SmallFont=new("Segoe UI",9.5f);
 internal static readonly Font StrongFont=new("Segoe UI",10.5f,FontStyle.Bold);
 internal static readonly Font TitleFont=new("Segoe UI",27,FontStyle.Bold);
 internal static readonly Font CardTitleFont=new("Segoe UI",17,FontStyle.Bold);
 internal static void Button(Button button,bool secondary=false)
 {
  button.FlatStyle=FlatStyle.Flat;button.FlatAppearance.BorderSize=0;
  button.BackColor=secondary?Color.White:Orange;button.ForeColor=secondary?Brown:Color.White;
  button.Font=StrongFont;button.Cursor=Cursors.Hand;button.MinimumSize=new Size(112,44);
  button.FlatAppearance.MouseOverBackColor=secondary?Soft:ColorTranslator.FromHtml("#9D3716");
  if(button is RoundedButton rounded)rounded.Secondary=secondary;
 }
 internal static void Field(Control control)
 {
  control.Font=BodyFont;control.BackColor=Color.White;control.ForeColor=Brown;
 }
 internal static GraphicsPath Round(Rectangle bounds,int radius)
 {
  var path=new GraphicsPath();int d=Math.Min(radius*2,Math.Min(bounds.Width,bounds.Height));
  if(d<2){path.AddRectangle(bounds);return path;}
  path.AddArc(bounds.Left,bounds.Top,d,d,180,90);path.AddArc(bounds.Right-d,bounds.Top,d,d,270,90);
  path.AddArc(bounds.Right-d,bounds.Bottom-d,d,d,0,90);path.AddArc(bounds.Left,bounds.Bottom-d,d,d,90,90);path.CloseFigure();return path;
 }
}
internal sealed class RoundedButton:Button
{
 internal bool Secondary;
 protected override void OnResize(EventArgs e)
 {
  base.OnResize(e);if(Width<2||Height<2)return;
  using var path=Theme.Round(new Rectangle(0,0,Width,Height),(int)(12*DeviceDpi/96f));
  var previous=Region;Region=new Region(path);previous?.Dispose();
 }
 protected override void OnPaint(PaintEventArgs e)
 {
  base.OnPaint(e);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
  if(Secondary){using var path=Theme.Round(new Rectangle(0,0,Width-1,Height-1),(int)(12*DeviceDpi/96f));using var pen=new Pen(Theme.Line);e.Graphics.DrawPath(pen,path);}
 }
}
internal sealed class SurfaceCard:Panel
{
 internal SurfaceCard(){DoubleBuffered=true;ResizeRedraw=true;BackColor=Theme.Beige;}
 protected override void OnPaint(PaintEventArgs e)
 {
  base.OnPaint(e);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
  using var path=Theme.Round(new Rectangle(0,0,Width-1,Height-1),(int)(18*DeviceDpi/96f));
  using var fill=new SolidBrush(Color.White);using var pen=new Pen(Theme.Line);e.Graphics.FillPath(fill,path);e.Graphics.DrawPath(pen,path);
 }
}
