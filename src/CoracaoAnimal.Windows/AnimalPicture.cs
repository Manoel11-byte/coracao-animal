using SkiaSharp;
namespace CoracaoAnimal.Windows;

// GDI+ decoding varies by Windows version. Decode the API's JPG/PNG/WebP
// formats explicitly, without depending on an optional system image codec.
internal sealed class AnimalPicture : PictureBox
{
 const int MaxBytes=5*1024*1024;
 static readonly HttpClient Photos=new(new HttpClientHandler {AllowAutoRedirect=false}) {Timeout=TimeSpan.FromSeconds(30)};
 static readonly SemaphoreSlim Decoders=new(3);
 readonly CancellationTokenSource lifetime=new();
 readonly string animalName;
 string message="Carregando foto…";
 internal AnimalPicture(string name)
 {
  animalName=name;SizeMode=PictureBoxSizeMode.Zoom;BackColor=Theme.Soft;ForeColor=Theme.Muted;
  AccessibleName="Foto de "+name;ErrorImage=null;InitialImage=null;
 }
 internal async Task LoadPhoto(Uri uri)
 {
  var token=lifetime.Token;
  try
  {
   using var response=await Photos.GetAsync(uri,HttpCompletionOption.ResponseHeadersRead,token);
   response.EnsureSuccessStatusCode();
   if(response.Content.Headers.ContentLength>MaxBytes)throw new InvalidDataException("Foto muito grande.");
   await using var source=await response.Content.ReadAsStreamAsync(token);using var data=new MemoryStream();
   var buffer=new byte[16384];int read;
   while((read=await source.ReadAsync(buffer,token))>0){if(data.Length+read>MaxBytes)throw new InvalidDataException("Foto muito grande.");data.Write(buffer,0,read);}
   await Decoders.WaitAsync(token);
   Bitmap image;
   try {image=await Task.Run(()=>Decode(data.ToArray()),token);}finally{Decoders.Release();}
   if(IsDisposed||token.IsCancellationRequested){image.Dispose();return;}
   Image=image;message="";AccessibleName="Foto de "+animalName+" — carregada";Invalidate();
  }
  catch(OperationCanceledException) when(token.IsCancellationRequested) { }
  catch(Exception ex) when(ex is HttpRequestException or TaskCanceledException or InvalidDataException or ArgumentException)
  {if(!IsDisposed){message="Foto indisponível";AccessibleName="Foto de "+animalName+" — indisponível";Invalidate();}}
 }
 static Bitmap Decode(byte[] bytes)
 {
  using var encoded=SKData.CreateCopy(bytes);using var codec=SKCodec.Create(encoded);
  if(codec==null||codec.Info.Width<=0||codec.Info.Height<=0||(long)codec.Info.Width*codec.Info.Height>20_000_000)throw new InvalidDataException("Imagem inválida ou com dimensões excessivas.");
  using var pixels=SKBitmap.Decode(codec)??throw new InvalidDataException("Formato de imagem inválido.");
  using var decoded=SKImage.FromBitmap(pixels);using var png=decoded.Encode(SKEncodedImageFormat.Png,100);
  if(png==null)throw new InvalidDataException("Não foi possível decodificar a foto.");
  using var stream=png.AsStream();using var image=System.Drawing.Image.FromStream(stream);return new Bitmap(image);
 }
 protected override void OnPaint(PaintEventArgs e)
 {
  base.OnPaint(e);
  if(Image==null)TextRenderer.DrawText(e.Graphics,message,Theme.SmallFont,ClientRectangle,ForeColor,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.WordBreak);
 }
 protected override void Dispose(bool disposing)
 {
  if(disposing&&!IsDisposed){lifetime.Cancel();lifetime.Dispose();Image?.Dispose();Image=null;}
  base.Dispose(disposing);
 }
}
