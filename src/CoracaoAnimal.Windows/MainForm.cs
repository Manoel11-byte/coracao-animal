using CoracaoAnimal.Client;
using CoracaoAnimal.Core;
namespace CoracaoAnimal.Windows;

public sealed class MainForm:Form
{
 readonly ApiClient api=new(new Storage());
 readonly FlowLayoutPanel body=new(){Dock=DockStyle.Fill,AutoScroll=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,BackColor=Theme.Beige};
 readonly Label state=new(){Dock=DockStyle.Bottom,BackColor=Color.White,ForeColor=Theme.Muted,AccessibleName="Situação da operação"};
 readonly FlowLayoutPanel menu=new(){FlowDirection=FlowDirection.TopDown,WrapContents=false,BackColor=Theme.Green};
 readonly Dictionary<string,Button> tabs=new();
 FlowLayoutPanel? catalogGrid;SurfaceCard? errorCard;bool busy,resizing;
 int D(int value)=>(int)Math.Round(value*DeviceDpi/96d);
 static Image? BrandLogo(){var a=typeof(MainForm).Assembly;var resource=a.GetManifestResourceNames().FirstOrDefault(n=>n.EndsWith("Assets.logo.png",StringComparison.OrdinalIgnoreCase));if(resource==null)return null;using var stream=a.GetManifestResourceStream(resource);if(stream==null)return null;using var image=Image.FromStream(stream);return new Bitmap(image);}
 public MainForm()
 {
  Text="Coração Animal • Windows 1.0.3";Width=1180;Height=840;MinimumSize=new Size(900,650);StartPosition=FormStartPosition.CenterScreen;
  Font=Theme.BodyFont;BackColor=Theme.Beige;ForeColor=Theme.Brown;AutoScaleMode=AutoScaleMode.Dpi;
  var sidebar=new Panel {Dock=DockStyle.Left,Width=D(216),BackColor=Theme.Green};
  var logo=new PictureBox {Image=BrandLogo(),SizeMode=PictureBoxSizeMode.Zoom,Bounds=new Rectangle(D(24),D(24),D(52),D(52)),AccessibleName="Logo Coração Animal"};logo.Disposed+=(_,_)=>logo.Image?.Dispose();sidebar.Controls.Add(logo);
  sidebar.Controls.Add(new Label {Text="Coração\nAnimal",Font=Theme.CardTitleFont,ForeColor=Color.White,AutoSize=true,Location=new Point(D(86),D(24))});
  sidebar.Controls.Add(new Label {Text="CUIDAR. ACOLHER. CONECTAR.",Font=new Font("Segoe UI",8),ForeColor=Theme.Soft,AutoSize=true,Location=new Point(D(24),D(98))});
  menu.Bounds=new Rectangle(D(16),D(144),D(184),D(330));
  foreach(var item in new(string Key,string Text,Func<Task> Open)[]{("explorar","Explorar animais",()=>Catalog()),("adocoes","Minhas adoções",Adoptions),("ajudar","Como ajudar",Help),("conta","Minha conta",Profile)})
  {var b=Button(item.Text,item.Open,true);b.AutoSize=false;b.Width=D(184);b.Height=D(50);b.Margin=new Padding(0,0,0,D(10));b.TextAlign=ContentAlignment.MiddleLeft;b.Padding=new Padding(D(12),0,0,0);tabs[item.Key]=b;menu.Controls.Add(b);}
  sidebar.Controls.Add(menu);
  sidebar.Controls.Add(new Label {Text="Um encontro.\nUma nova história.",Dock=DockStyle.Bottom,Height=D(76),Padding=new Padding(D(24),D(8),0,0),ForeColor=Theme.Soft,Font=Theme.SmallFont});
  var workspace=new Panel {Dock=DockStyle.Fill,BackColor=Theme.Beige};
  var header=new Panel {Dock=DockStyle.Top,Height=D(66),BackColor=Color.White};
  header.Controls.Add(new Label {Text="Adoção responsável transforma vidas",AutoSize=true,ForeColor=Theme.Muted,Location=new Point(D(28),D(24)),Font=Theme.SmallFont});
  var version=new Label {Text="Windows 1.0.3",Dock=DockStyle.Right,Width=D(140),TextAlign=ContentAlignment.MiddleCenter,ForeColor=Theme.Green,Font=Theme.SmallFont};header.Controls.Add(version);
  body.Padding=new Padding(D(28),D(28),D(28),D(32));state.Height=D(38);state.Padding=new Padding(D(28),D(10),0,0);state.Font=Theme.SmallFont;
  workspace.Controls.Add(body);workspace.Controls.Add(state);workspace.Controls.Add(header);Controls.Add(workspace);Controls.Add(sidebar);
  body.SizeChanged+=(_,_)=>ResizeBody();body.ControlAdded+=(_,_)=>ResizeBody();
  Page("Seu novo amigo está aqui","Estamos buscando os animais disponíveis para adoção.");
  Shown+=async(_,_)=>await Run(async()=>{await api.Initialize();await Catalog();});
 }
 Button Button(string text,Func<Task> work,bool secondary=false)
 {
  var button=new RoundedButton {Text=text,AutoSize=true,AutoSizeMode=AutoSizeMode.GrowAndShrink,Margin=new Padding(0,0,D(10),D(10)),Padding=new Padding(D(18),D(10),D(18),D(10)),AccessibleName=text,MinimumSize=new Size(D(112),D(44))};
  Theme.Button(button,secondary);button.Click+=async(_,_)=>await Run(work);return button;
 }
 async Task Run(Func<Task> work)
 {
  if(busy)return;busy=true;body.Enabled=false;menu.Enabled=false;UseWaitCursor=true;state.Text="Carregando…";state.ForeColor=Theme.Muted;
  try{if(errorCard is {IsDisposed:false})errorCard.Dispose();errorCard=null;await work();state.Text="Tudo pronto";state.ForeColor=Theme.Green;}
  catch(UnauthorizedAccessException ex){await Login();state.Text=ex.Message;state.ForeColor=Theme.Orange;}
  catch(Exception ex){state.Text="Não foi possível concluir";state.ForeColor=Theme.Orange;ShowError(ex.Message,work);}
  finally{busy=false;body.Enabled=true;menu.Enabled=true;UseWaitCursor=false;}
 }
 void SelectTab(string key)
 {
  foreach(var pair in tabs){bool selected=pair.Key==key;pair.Value.BackColor=selected?Color.White:Theme.Green;pair.Value.ForeColor=selected?Theme.Green:Color.White;pair.Value.FlatAppearance.MouseOverBackColor=selected?Theme.Soft:Color.FromArgb(31,73,57);pair.Value.AccessibleDescription=selected?"Seção atual":null;}
 }
 void ResizeBody()
 {
  if(resizing)return;resizing=true;
  try
  {
   int width=Math.Max(D(180),body.ClientSize.Width-body.Padding.Horizontal-SystemInformation.VerticalScrollBarWidth-D(4));
   foreach(Control control in body.Controls)
   {
    if(control==catalogGrid){control.Width=width;control.MaximumSize=new Size(width,0);control.MinimumSize=new Size(width,0);int count=width>=D(660)?2:1;int cardWidth=(width-D(16)*count)/count;foreach(Control card in control.Controls)card.Width=cardWidth;}
    else if(control.Tag as string=="full-width")control.Width=width;
    else if(control.Tag as string=="field")control.Width=Math.Min(D(640),width);
    else if(control is Label or CheckBox)control.MaximumSize=new Size(width,0);
    else if(control is PictureBox)control.Width=Math.Min(D(680),width);
   }
  }finally{resizing=false;}
 }
 void Page(string title,string subtitle="",string tab="explorar")
 {
  body.SuspendLayout();catalogGrid=null;errorCard=null;while(body.Controls.Count>0)body.Controls[0].Dispose();SelectTab(tab);
  body.Controls.Add(new Label {Text="CORAÇÃO ANIMAL",AutoSize=true,Font=Theme.StrongFont,ForeColor=Theme.Green,Margin=new Padding(0,0,0,D(8))});
  body.Controls.Add(new Label {Text=title,AutoSize=true,Font=Theme.TitleFont,ForeColor=Theme.Brown,Margin=new Padding(0,0,0,D(10)),AccessibleRole=AccessibleRole.StaticText});
  if(subtitle.Length>0)Note(subtitle);body.ResumeLayout();body.AutoScrollPosition=Point.Empty;
 }
 void Note(string text)=>body.Controls.Add(new Label {Text=text,AutoSize=true,Font=Theme.BodyFont,ForeColor=Theme.Muted,Margin=new Padding(0,0,0,D(18))});
 void ShowError(string text,Func<Task> retry)
 {
  var content=new FlowLayoutPanel {Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(D(16)),BackColor=Color.White};
  content.Controls.Add(new Label {Text=text,AutoSize=true,MaximumSize=new Size(D(570),0),ForeColor=Theme.Orange,Font=Theme.SmallFont,Margin=new Padding(0,0,0,D(10))});content.Controls.Add(Button("Tentar novamente",retry,true));
  errorCard=new SurfaceCard {Width=D(640),Height=D(150),Tag="field",Margin=new Padding(0,0,0,D(20)),Padding=new Padding(2)};errorCard.Controls.Add(content);body.Controls.Add(errorCard);body.Controls.SetChildIndex(errorCard,0);ResizeBody();body.AutoScrollPosition=Point.Empty;
 }
 TextBox Field(string label,string value="",bool password=false)
 {
  var field=new SurfaceCard {Width=D(640),Height=D(82),Tag="field",Margin=new Padding(0,0,0,D(14))};
  var caption=new Label {Text=label,AutoSize=true,Location=new Point(D(14),D(10)),ForeColor=Theme.Brown,BackColor=Color.White,Font=Theme.StrongFont};
  var input=new TextBox {Text=value,UseSystemPasswordChar=password,AccessibleName=label,BorderStyle=BorderStyle.None,Location=new Point(D(14),D(40)),Width=field.Width-D(28)};Theme.Field(input);
  field.Controls.Add(caption);field.Controls.Add(input);field.Resize+=(_,_)=>input.Width=Math.Max(D(50),field.ClientSize.Width-D(28));body.Controls.Add(field);return input;
 }
 sealed record Option(string Key,string Label){public override string ToString()=>Label;}
 static string Friendly(string key)=>key switch {""=>"Todos", "cao"=>"Cão", "gato"=>"Gato", "pequeno"=>"Pequeno", "medio"=>"Médio", "grande"=>"Grande", "nenhuma"=>"Primeira experiência", "alguma"=>"Alguma experiência", "muita"=>"Muita experiência", "disponivel"=>"Disponível", "em_processo"=>"Em avaliação", "adotado"=>"Adotado", "em_tratamento"=>"Em tratamento", "pendente"=>"Pendente", "aprovada"=>"Aprovada", "concluida"=>"Concluída", "cancelada"=>"Cancelada", "confirmada"=>"Confirmada", "ativo"=>"Ativo", "inativo"=>"Inativo", "pix"=>"Pix", "transferencia"=>"Transferência", "dinheiro"=>"Dinheiro", _=>key};
 ComboBox Choice(string label,string[] items,string? value=null)
 {
  var field=new SurfaceCard {Width=D(640),Height=D(84),Tag="field",Margin=new Padding(0,0,0,D(14))};
  field.Controls.Add(new Label {Text=label,AutoSize=true,Location=new Point(D(14),D(10)),BackColor=Color.White,ForeColor=Theme.Brown,Font=Theme.StrongFont});
  var choice=new ComboBox {DropDownStyle=ComboBoxStyle.DropDownList,FlatStyle=FlatStyle.Flat,AccessibleName=label,Location=new Point(D(14),D(38)),Width=field.Width-D(28)};Theme.Field(choice);
  choice.Items.AddRange(items.Select(x=>(object)new Option(x,Friendly(x))).ToArray());choice.SelectedIndex=Math.Max(0,Array.IndexOf(items,value));
  field.Controls.Add(choice);field.Resize+=(_,_)=>choice.Width=Math.Max(D(50),field.ClientSize.Width-D(28));body.Controls.Add(field);return choice;
 }
 static string Value(ComboBox choice)=>(choice.SelectedItem as Option)?.Key??"";
 CheckBox Check(string label){var check=new CheckBox {Text=label,AccessibleName=label,AutoSize=true,ForeColor=Theme.Brown,Font=Theme.BodyFont,Margin=new Padding(0,D(4),0,D(18))};body.Controls.Add(check);return check;}
 PictureBox Picture(AnimalDto animal,int height)
 {
  var picture=new PictureBox {Height=height,SizeMode=PictureBoxSizeMode.Zoom,AccessibleName="Foto de "+animal.Nome,ErrorImage=null,InitialImage=null,BackColor=Theme.Soft};
  picture.Disposed+=(_,_)=>{picture.CancelAsync();picture.Image?.Dispose();};picture.LoadAsync(new Uri(api.BaseUri!,animal.FotoUrl!).AbsoluteUri);return picture;
 }
 void Photo(AnimalDto animal,int height){var picture=Picture(animal,D(height));picture.Width=D(680);picture.Margin=new Padding(0,0,0,D(18));body.Controls.Add(picture);}
 void Action(string text,Func<Task> work,bool secondary=false)=>body.Controls.Add(Button(text,work,secondary));
 bool Confirm(string message)=>MessageBox.Show(this,message,"Confirmar",MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.Yes;
 Task Help(){Page("Cada gesto faz diferença","Escolha como você quer participar do acolhimento.","ajudar");Note("Apoie os cuidados com os animais.");Action("Registrar uma doação",Donations);Note("Doe seu tempo e suas habilidades.");Action("Quero ser voluntário",Volunteer,true);return Task.CompletedTask;}
 void AnimalCard(AnimalDto animal)
 {
  var card=new SurfaceCard {Height=D(382),Width=D(360),Padding=new Padding(D(16)),Margin=new Padding(0,0,D(16),D(18))};
  var content=new FlowLayoutPanel {Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,BackColor=Color.White,Margin=Padding.Empty};
  Control photo=animal.FotoUrl!=null?Picture(animal,D(170)):new Label {Text="Foto em breve",Height=D(170),TextAlign=ContentAlignment.MiddleCenter,BackColor=Theme.Soft,ForeColor=Theme.Muted};photo.Margin=new Padding(0,0,0,D(12));content.Controls.Add(photo);
  content.Controls.Add(new Label {Text=animal.Nome,Font=Theme.CardTitleFont,ForeColor=Theme.Brown,Height=D(32),AutoEllipsis=true,Margin=new Padding(0,0,0,D(4))});
  content.Controls.Add(new Label {Text=$"{Friendly(animal.Especie)} · {animal.Idade} ano(s) · Porte {Friendly(animal.Porte).ToLowerInvariant()}",Font=Theme.SmallFont,ForeColor=Theme.Muted,Height=D(40),Margin=new Padding(0,0,0,D(8))});
  content.Controls.Add(new Label {Text=Friendly(animal.Status)+(animal.Compatibilidade.HasValue?$" · Afinidade {animal.Compatibilidade:0}%":""),Font=Theme.StrongFont,ForeColor=Theme.Green,Height=D(24),Margin=new Padding(0,0,0,D(10))});
  var open=Button("Conhecer "+animal.Nome,()=>Details(animal.Id));open.AutoSize=false;open.Height=D(44);open.Margin=Padding.Empty;content.Controls.Add(open);
  card.Controls.Add(content);card.Resize+=(_,_)=>{foreach(Control child in content.Controls)child.Width=Math.Max(D(80),card.ClientSize.Width-card.Padding.Horizontal-D(4));};catalogGrid!.Controls.Add(card);
 }
 Task Login(){Page("Bem-vindo de volta","Entre para acompanhar suas adoções e ajudar os animais.","conta");var email=Field("E-mail");var senha=Field("Senha","",true);Action("Entrar",async()=>{await api.Login(email.Text,senha.Text);await Profile();});Action("Criar conta",Register,true);return Task.CompletedTask;}
 Task Register(){Page("Vamos começar?","Crie sua conta para dar o primeiro passo.","conta");var nome=Field("Nome completo");var email=Field("E-mail");var senha=Field("Senha (mínimo 10 caracteres)","",true);Action("Cadastrar",async()=>{await api.Post<object>("api/auth/cadastro",new Cadastro(nome.Text,email.Text,senha.Text));await api.Login(email.Text,senha.Text);await Profile();});return Task.CompletedTask;}
 async Task Catalog(bool recommend=false,int page=1,string busca="",string especie="",string porte="")
 {
  if(recommend)await api.Me();var data=recommend?new Pagina<AnimalDto>(await api.Get<List<AnimalDto>>("api/me/recomendacoes")??[],0,1,12):await api.Get<Pagina<AnimalDto>>($"api/animais?pagina={page}&tamanho=12&busca={Uri.EscapeDataString(busca)}&especie={especie}&porte={porte}");
  Page(recommend?"Companheiros para você":"Seu novo amigo está aqui",recommend?"Sugestões baseadas no seu perfil. A equipe acompanha cada adoção.":"Conheça histórias, encontre afinidades e adote com responsabilidade.");
  if(!recommend)
  {
   var search=Field("Nome ou raça",busca);var sp=Choice("Espécie",["","cao","gato"],especie);var size=Choice("Porte",["","pequeno","medio","grande"],porte);
   var filters=new TableLayoutPanel {Height=D(98),Tag="full-width",ColumnCount=3,RowCount=1,Margin=Padding.Empty,BackColor=Theme.Beige};
   filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,40));filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,30));filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,30));
   foreach(var control in new Control[]{search.Parent!,sp.Parent!,size.Parent!}){body.Controls.Remove(control);control.Dock=DockStyle.Fill;control.Margin=new Padding(0,0,D(10),D(14));filters.Controls.Add(control);}body.Controls.Add(filters);
   Func<Task> find=()=>Catalog(false,1,search.Text,Value(sp),Value(size));
   var actions=new FlowLayoutPanel {AutoSize=true,AutoSizeMode=AutoSizeMode.GrowAndShrink,WrapContents=true,Tag="full-width",Margin=new Padding(0,0,0,D(10))};actions.Controls.Add(Button("Buscar animais",find));actions.Controls.Add(Button("Para você",()=>Catalog(true),true));body.Controls.Add(actions);
   search.KeyDown+=async(_,e)=>{if(e.KeyCode==Keys.Enter){e.SuppressKeyPress=true;await Run(find);}};Note($"{data?.Total??0} animal(is) encontrado(s)");
  }
  else Action("Voltar a todos os animais",()=>Catalog(),true);
  catalogGrid=new FlowLayoutPanel {AutoSize=true,AutoSizeMode=AutoSizeMode.GrowAndShrink,WrapContents=true,FlowDirection=FlowDirection.LeftToRight,Tag="full-width",Margin=Padding.Empty,BackColor=Theme.Beige};
  body.Controls.Add(catalogGrid);catalogGrid.SuspendLayout();foreach(var animal in data?.Itens??[])AnimalCard(animal);ResizeBody();catalogGrid.ResumeLayout();
  if(data?.Itens.Count==0)Note("Nenhum animal encontrado. Tente outros filtros ou volte em breve.");
  if(!recommend&&data!=null){if(page>1)Action("Página anterior",()=>Catalog(false,page-1,busca,especie,porte),true);if(page*12<data.Total)Action("Próxima página",()=>Catalog(false,page+1,busca,especie,porte),true);}
 }
 async Task Details(int id){var a=await api.Get<AnimalDto>($"api/animais/{id}")??throw new Exception("Animal não encontrado.");Page(a.Nome,"Conheça melhor quem pode fazer parte da sua família.");Action("Voltar ao catálogo",()=>Catalog(),true);Note($"{Friendly(a.Especie)} · {a.Raca} · {a.Idade} anos · {Friendly(a.Porte)} · {Friendly(a.Status)}");if(a.FotoUrl!=null)Photo(a,280);Note(a.Descricao);if(a.Status=="disponivel"){var obs=Field("Conte como pretende acolher este animal");var terms=Check("Entendo que esta é uma solicitação, sujeita à avaliação da equipe.");Action("Solicitar adoção",async()=>{await api.Post<object>("api/me/adocoes",new AdocaoInput(id,obs.Text,terms.Checked));await Adoptions();});}}
 async Task Adoptions(){var items=await api.Get<List<AdocaoDto>>("api/me/adocoes")??[];Page("Minhas adoções","Acompanhe o andamento de cada solicitação.","adocoes");if(items.Count==0)Note("Você ainda não solicitou uma adoção.");foreach(var a in items){Note($"{a.AnimalNome} · {Friendly(a.Status)} · {a.CriadoEm.ToLocalTime():d}\n{a.Observacoes}");if(a.Status is "pendente" or "aprovada")Action($"Cancelar solicitação #{a.Id}",async()=>{if(Confirm("Cancelar esta solicitação?")){await api.Post<object>($"api/me/adocoes/{a.Id}/cancelar");await Adoptions();}});}}
 async Task Donations(){var items=await api.Get<List<DoacaoDto>>("api/me/doacoes")??[];Page("Ajude o acolhimento","Sua ajuda se transforma em cuidado.","ajudar");Note("Este formulário registra uma intenção de doação. Não realiza cobrança. A equipe confirma o recebimento pelo site.");var valor=Field("Valor em reais");var forma=Choice("Forma de pagamento",["pix","transferencia","dinheiro"]);var desc=Field("Observação");var anon=Check("Não divulgar meu nome");Action("Registrar intenção",async()=>{var v=DonationAmount.Parse(valor.Text);await api.Post<object>("api/me/doacoes",new DoacaoInput(v,Value(forma),desc.Text,anon.Checked));await Donations();});foreach(var d in items){Note($"#{d.Id} · {d.Valor:C} · {Friendly(d.Status)} · {Friendly(d.FormaPagamento)}");if(d.Status=="pendente")Action($"Cancelar doação #{d.Id}",async()=>{if(Confirm("Cancelar esta intenção?")){await api.Post<object>($"api/me/doacoes/{d.Id}/cancelar");await Donations();}});}}
 async Task Volunteer(){var v=await api.Get<VoluntarioDto>("api/me/voluntariado");Page("Seja voluntário","Encontre uma forma de ajudar que combine com sua rotina.","ajudar");Note(v==null?"Cadastre sua disponibilidade para ajudar.":"Situação: "+Friendly(v.Status));var area=Field("Área de interesse",v?.Area??"");var disp=Field("Dias e horários disponíveis",v?.Disponibilidade??"");Action("Salvar inscrição",async()=>{await api.Put<object>("api/me/voluntariado",new VoluntarioInput(area.Text,disp.Text));await Volunteer();});if(v!=null&&v.Status!="inativo")Action("Desativar inscrição",async()=>{if(Confirm("Desativar sua inscrição?")){await api.Delete("api/me/voluntariado");await Volunteer();}});}
 async Task Profile(){var u=await api.Me()??throw new Exception("Entre para continuar.");Page("Minha conta","Mantenha seus dados e preferências atualizados.","conta");Note(u.Email);var nome=Field("Nome completo",u.Nome);var cpf=Field("CPF",u.Cpf??"");var tel=Field("Telefone",u.Telefone);var end=Field("Endereço",u.Endereco);var sp=Choice("Espécie preferida",["cao","gato"],u.EspeciePreferida);var esp=Choice("Espaço disponível",["pequeno","medio","grande"],u.Espaco);var exp=Choice("Experiência",["nenhuma","alguma","muita"],u.Experiencia);Action("Salvar perfil",async()=>{await api.Put<object>("api/me/",new PerfilInput(nome.Text,cpf.Text,tel.Text,end.Text,Value(sp),Value(esp),Value(exp)));await Profile();});Action("Alterar senha",Password);Action("Sair da conta",async()=>{await api.Logout();await Login();});}
 Task Password(){Page("Alterar senha","Escolha uma senha exclusiva com pelo menos 10 caracteres.","conta");var old=Field("Senha atual","",true);var next=Field("Nova senha (mínimo 10 caracteres)","",true);Action("Salvar nova senha",async()=>{await api.Put<object>("api/me/senha",new SenhaInput(old.Text,next.Text));await api.Forget();await Login();});return Task.CompletedTask;}
}
