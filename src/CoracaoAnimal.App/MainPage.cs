using CoracaoAnimal.Client;
using CoracaoAnimal.Core;
using Microsoft.Maui.Controls.Shapes;
namespace CoracaoAnimal.App;

public sealed class MainPage : ContentPage
{
 static readonly Color Ink=Color.FromArgb("#2A1F14"), Green=Color.FromArgb("#4A7C59"), Muted=Color.FromArgb("#7A6355"), Line=Color.FromArgb("#E5D9CC"), Canvas=Color.FromArgb("#F7F3EE"), Orange=Color.FromArgb("#B8431B"), Brand=Color.FromArgb("#3D2B1F");
 readonly ApiClient api=new(new Storage());
 readonly VerticalStackLayout body=new(){Spacing=16,Padding=new Thickness(20,24,20,32),MaximumWidthRequest=720,HorizontalOptions=LayoutOptions.Fill};
 readonly Label state=new(){FontSize=12,TextColor=Muted,VerticalOptions=LayoutOptions.Center};
 readonly ActivityIndicator activity=new(){Color=Orange,WidthRequest=20,HeightRequest=20,IsVisible=false};
 readonly Grid navigation=new(){ColumnSpacing=4,Padding=new Thickness(8,10),BackgroundColor=Colors.White};
 readonly Dictionary<string,Button> tabs=new();
 readonly ScrollView scroll;
 bool loaded,busy;
 public MainPage()
 {
  Title="Coração Animal";BackgroundColor=Canvas;
  var brand=new Grid {Padding=new Thickness(20,14),ColumnSpacing=12,BackgroundColor=Brand,ColumnDefinitions={new ColumnDefinition(48),new ColumnDefinition(GridLength.Star)}};
  brand.Add(new Image {Source="brand_logo.png",HeightRequest=46,WidthRequest=46,Aspect=Aspect.AspectFit},0,0);
  var names=new VerticalStackLayout {Spacing=2,VerticalOptions=LayoutOptions.Center};
  names.Add(new Label {Text="Coração Animal",TextColor=Colors.White,FontAttributes=FontAttributes.Bold,FontSize=21});
  names.Add(new Label {Text="Um encontro. Uma nova história.",TextColor=Color.FromArgb("#EFE9E1"),FontSize=12});brand.Add(names,1,0);
  var links=new (string Key,string Title,Func<Task> Open)[]{("explorar","Explorar",()=>Catalog()),("adocoes","Adoções",Adoptions),("ajudar","Ajudar",Help),("conta","Conta",Profile)};
  for(int i=0;i<links.Length;i++)
  {
   var item=links[i];navigation.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
   var button=Button(item.Title,item.Open,true);button.FontSize=12;button.Padding=new Thickness(4,10);tabs[item.Key]=button;navigation.Add(button,i,0);
  }
  scroll=new ScrollView {Content=body};
  var status=new HorizontalStackLayout {Children={activity,state},Spacing=8,Padding=new Thickness(20,6)};
  SemanticProperties.SetDescription(status,"Situação da operação");
  var grid=new Grid {RowDefinitions={new RowDefinition(GridLength.Auto),new RowDefinition(GridLength.Star),new RowDefinition(GridLength.Auto),new RowDefinition(GridLength.Auto)}};
  grid.Add(brand,0,0);grid.Add(scroll,0,1);grid.Add(status,0,2);grid.Add(navigation,0,3);Content=grid;SelectTab("explorar");
 }
 protected override async void OnAppearing(){base.OnAppearing();if(loaded)return;loaded=true;await Run(async()=>{await api.Initialize();await Catalog();});}
 Button Button(string text,Func<Task> work,bool secondary=false)
 {
  var button=new Button {Text=text,BackgroundColor=secondary?Colors.White:Orange,TextColor=secondary?Ink:Colors.White,BorderColor=Line,BorderWidth=secondary?1:0,CornerRadius=16,MinimumHeightRequest=50,FontSize=15,FontAttributes=FontAttributes.Bold,Padding=new Thickness(16,12)};
  button.Clicked+=async(_,_)=>await Run(work);return button;
 }
 async Task Run(Func<Task> work)
 {
  if(busy)return;busy=true;body.IsEnabled=false;navigation.IsEnabled=false;activity.IsVisible=true;activity.IsRunning=true;state.Text="Carregando…";
  try{await work();state.Text="Tudo pronto";}
  catch(UnauthorizedAccessException ex){await Login();state.Text=ex.Message;}
  catch(Exception ex){state.Text="Não foi possível concluir";await DisplayAlertAsync("Vamos tentar novamente?",ex.Message,"Entendi");}
  finally{busy=false;body.IsEnabled=true;navigation.IsEnabled=true;activity.IsRunning=false;activity.IsVisible=false;}
 }
 void SelectTab(string key)
 {
  foreach(var pair in tabs){var selected=pair.Key==key;pair.Value.BackgroundColor=selected?Orange:Colors.White;pair.Value.TextColor=selected?Colors.White:Muted;SemanticProperties.SetDescription(pair.Value,pair.Value.Text+(selected?", seção atual":""));}
 }
 void Page(string title,string subtitle="",string tab="explorar")
 {
  SelectTab(tab);body.Clear();
  var heading=new Label {Text=title,FontSize=29,FontAttributes=FontAttributes.Bold,TextColor=Ink};SemanticProperties.SetHeadingLevel(heading,SemanticHeadingLevel.Level1);body.Add(heading);
  if(subtitle.Length>0)Note(subtitle);_ = scroll.ScrollToAsync(0,0,false);
 }
 void Note(string text)=>body.Add(new Label {Text=text,TextColor=Muted,FontSize=15,LineHeight=1.2});
 Border Surface(View content)=>new(){Content=content,BackgroundColor=Colors.White,Stroke=Line,StrokeThickness=1,StrokeShape=new RoundRectangle {CornerRadius=20},Padding=18};
 Entry Field(string name,string value="",bool password=false,Keyboard? keyboard=null)
 {
  var entry=new Entry {Text=value,Placeholder=name,IsPassword=password,Keyboard=keyboard??Keyboard.Default,TextColor=Ink,PlaceholderColor=Muted,BackgroundColor=Colors.Transparent,MinimumHeightRequest=48,FontSize=16,ClearButtonVisibility=password?ClearButtonVisibility.Never:ClearButtonVisibility.WhileEditing};
  SemanticProperties.SetDescription(entry,name);
  var group=new VerticalStackLayout {Spacing=4};group.Add(new Label {Text=name,FontSize=13,FontAttributes=FontAttributes.Bold,TextColor=Ink});group.Add(entry);body.Add(Surface(group));return entry;
 }
 sealed record Option(string Key,string Label){public override string ToString()=>Label;}
 static string Friendly(string key)=>key switch {""=>"Todos", "cao"=>"Cão", "gato"=>"Gato", "pequeno"=>"Pequeno", "medio"=>"Médio", "grande"=>"Grande", "nenhuma"=>"Primeira experiência", "alguma"=>"Alguma experiência", "muita"=>"Muita experiência", "disponivel"=>"Disponível", "em_processo"=>"Em avaliação", "adotado"=>"Adotado", "em_tratamento"=>"Em tratamento", "pendente"=>"Pendente", "aprovada"=>"Aprovada", "concluida"=>"Concluída", "cancelada"=>"Cancelada", "confirmada"=>"Confirmada", "ativo"=>"Ativo", "inativo"=>"Inativo", "pix"=>"Pix", "transferencia"=>"Transferência", "dinheiro"=>"Dinheiro", _=>key};
 Picker Choice(string label,string[] items,string? selected=null)
 {
  var picker=new Picker {Title=label,ItemsSource=items.Select(x=>new Option(x,Friendly(x))).ToArray(),SelectedIndex=Math.Max(0,Array.IndexOf(items,selected)),TextColor=Ink,BackgroundColor=Colors.Transparent,MinimumHeightRequest=48};
  SemanticProperties.SetDescription(picker,label);var group=new VerticalStackLayout {Spacing=4};group.Add(new Label {Text=label,FontSize=13,FontAttributes=FontAttributes.Bold,TextColor=Ink});group.Add(picker);body.Add(Surface(group));return picker;
 }
 CheckBox Check(string label)
 {
  var check=new CheckBox {Color=Orange,MinimumHeightRequest=48,MinimumWidthRequest=48};SemanticProperties.SetDescription(check,label);
  var row=new Grid {ColumnDefinitions={new ColumnDefinition(48),new ColumnDefinition(GridLength.Star)},ColumnSpacing=8};row.Add(check,0,0);row.Add(new Label {Text=label,VerticalOptions=LayoutOptions.Center,TextColor=Ink,FontSize=14},1,0);body.Add(row);return check;
 }
 static string Value(Picker picker)=>(picker.SelectedItem as Option)?.Key??"";
 void Action(string text,Func<Task> work,bool secondary=false)=>body.Add(Button(text,work,secondary));
 Task<bool> Confirm(string text)=>DisplayAlertAsync("Confirmar",text,"Sim","Não");
 Task Help()
 {
  Page("Cada gesto faz diferença","Escolha como você quer participar do acolhimento.","ajudar");
  Note("Apoie os cuidados");Action("Registrar uma doação",Donations);
  Note("Doe seu tempo e suas habilidades");Action("Quero ser voluntário",Volunteer,true);return Task.CompletedTask;
 }
 void AnimalCard(AnimalDto animal)
 {
  var content=new VerticalStackLayout {Spacing=12};
  if(animal.FotoUrl is not null){var photo=new Image {Source=new Uri(api.BaseUri!,animal.FotoUrl).AbsoluteUri,HeightRequest=210,Aspect=Aspect.AspectFill};SemanticProperties.SetDescription(photo,"Foto de "+animal.Nome);content.Add(photo);}
  content.Add(new Label {Text=animal.Nome,FontSize=23,FontAttributes=FontAttributes.Bold,TextColor=Ink});
  content.Add(new Label {Text=$"{Friendly(animal.Especie)} · {animal.Idade} ano(s) · Porte {Friendly(animal.Porte).ToLowerInvariant()}",FontSize=14,TextColor=Muted});
  content.Add(new Label {Text=Friendly(animal.Status)+(animal.Compatibilidade.HasValue?$" · Afinidade {animal.Compatibilidade:0}%":""),FontSize=13,TextColor=Green,FontAttributes=FontAttributes.Bold});
  content.Add(Button("Conhecer "+animal.Nome,()=>Details(animal.Id)));body.Add(Surface(content));
 }
 Task Login(){Page("Bem-vindo de volta","Entre para acompanhar suas adoções e ajudar os animais.","conta");var email=Field("E-mail",keyboard:Keyboard.Email);var senha=Field("Senha",password:true);Action("Entrar",async()=>{await api.Login(email.Text??"",senha.Text??"");await Profile();});Action("Criar conta",Register,true);return Task.CompletedTask;}
 Task Register(){Page("Vamos começar?","Crie sua conta para dar o primeiro passo.","conta");var nome=Field("Nome completo");var email=Field("E-mail",keyboard:Keyboard.Email);var senha=Field("Senha (mínimo 10 caracteres)",password:true);Action("Cadastrar",async()=>{await api.Post<object>("api/auth/cadastro",new Cadastro(nome.Text??"",email.Text??"",senha.Text??""));await api.Login(email.Text??"",senha.Text??"");await Profile();});return Task.CompletedTask;}
 async Task Catalog(bool recommend=false,int page=1,string busca="",string especie="",string porte="")
 {
  if(recommend)await api.Me();
  var data=recommend?new Pagina<AnimalDto>(await api.Get<List<AnimalDto>>("api/me/recomendacoes")??[],0,1,12):await api.Get<Pagina<AnimalDto>>($"api/animais?pagina={page}&tamanho=12&busca={Uri.EscapeDataString(busca)}&especie={especie}&porte={porte}");
  Page(recommend?"Companheiros para você":"Seu novo amigo está aqui",recommend?"Sugestões baseadas no seu perfil. A equipe acompanha cada adoção.":"Conheça histórias, encontre afinidades e adote com responsabilidade.");
  if(!recommend)
  {
   var filters=new VerticalStackLayout {Spacing=10,IsVisible=especie.Length>0||porte.Length>0};
   var sp=Choice("Espécie",["","cao","gato"],especie);var speciesView=body.Children[^1];body.Remove(speciesView);filters.Add(speciesView);
   var size=Choice("Porte",["","pequeno","medio","grande"],porte);var sizeView=body.Children[^1];body.Remove(sizeView);filters.Add(sizeView);
   var search=new Entry {Text=busca,Placeholder="Nome ou raça",TextColor=Ink,PlaceholderColor=Muted,BackgroundColor=Colors.Transparent,MinimumHeightRequest=48,FontSize=16,ReturnType=ReturnType.Search};
   SemanticProperties.SetDescription(search,"Buscar por nome ou raça");
   Func<Task> searchAnimals=()=>Catalog(false,1,search.Text??"",Value(sp),Value(size));
   search.Completed+=async(_,_)=>await Run(searchAnimals);
   var searchRow=new Grid {ColumnDefinitions={new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Auto)},ColumnSpacing=8};
   var searchBox=Surface(search);searchBox.Padding=new Thickness(12,0);searchRow.Add(searchBox,0,0);searchRow.Add(Button("Buscar",searchAnimals),1,0);body.Add(searchRow);
   var tools=new Grid {ColumnDefinitions={new ColumnDefinition(GridLength.Star),new ColumnDefinition(GridLength.Star)},ColumnSpacing=8};
   tools.Add(Button("Filtros",()=>{filters.IsVisible=!filters.IsVisible;return Task.CompletedTask;},true),0,0);tools.Add(Button("Para você",()=>Catalog(true),true),1,0);body.Add(tools);
   filters.Add(Button("Aplicar filtros",searchAnimals));body.Add(filters);
   Note($"{data?.Total??0} animal(is) encontrado(s)");
  }
  else Action("Voltar a todos os animais",()=>Catalog(),true);
  foreach(var animal in data?.Itens??[])AnimalCard(animal);
  if(data?.Itens.Count==0)Note("Nenhum animal encontrado. Tente outros filtros ou volte em breve.");
  if(!recommend&&data!=null){if(page>1)Action("Página anterior",()=>Catalog(false,page-1,busca,especie,porte),true);if(page*12<data.Total)Action("Próxima página",()=>Catalog(false,page+1,busca,especie,porte),true);}
 }
 async Task Details(int id){var a=await api.Get<AnimalDto>($"api/animais/{id}")??throw new Exception("Animal não encontrado.");Page(a.Nome,"Conheça melhor quem pode fazer parte da sua família.");Action("Voltar ao catálogo",()=>Catalog(),true);if(a.FotoUrl!=null)body.Add(new Image(){Source=new Uri(api.BaseUri!,a.FotoUrl).AbsoluteUri,HeightRequest=260,Aspect=Aspect.AspectFit});Note($"{Friendly(a.Especie)} · {a.Raca} · {a.Idade} anos\n{Friendly(a.Porte)} · {Friendly(a.Status)}");Note(a.Descricao);if(a.Status=="disponivel"){var obs=Field("Como você pretende acolher este animal?");var terms=Check("Entendo que a solicitação será avaliada pela equipe.");Action("Solicitar adoção",async()=>{await api.Post<object>("api/me/adocoes",new AdocaoInput(id,obs.Text??"",terms.IsChecked));await Adoptions();});}}
 async Task Adoptions(){var items=await api.Get<List<AdocaoDto>>("api/me/adocoes")??[];Page("Minhas adoções","Acompanhe o andamento de cada solicitação.","adocoes");if(items.Count==0)Note("Você ainda não solicitou uma adoção.");foreach(var a in items){Note($"{a.AnimalNome} · {Friendly(a.Status)}\n{a.CriadoEm.ToLocalTime():d} · {a.Observacoes}");if(a.Status is "pendente" or "aprovada")Action($"Cancelar solicitação #{a.Id}",async()=>{if(await Confirm("Cancelar esta solicitação?")){await api.Post<object>($"api/me/adocoes/{a.Id}/cancelar");await Adoptions();}});}}
 async Task Donations(){var items=await api.Get<List<DoacaoDto>>("api/me/doacoes")??[];Page("Ajude o acolhimento","Sua ajuda se transforma em cuidado.","ajudar");Note("Registro de intenção de doação. Não realiza cobrança. A equipe confirma o recebimento pelo site.");var valor=Field("Valor em reais",keyboard:Keyboard.Numeric);var forma=Choice("Forma de pagamento",["pix","transferencia","dinheiro"]);var desc=Field("Observação");var anon=Check("Não divulgar meu nome");Action("Registrar intenção",async()=>{var v=DonationAmount.Parse(valor.Text);await api.Post<object>("api/me/doacoes",new DoacaoInput(v,Value(forma),desc.Text??"",anon.IsChecked));await Donations();});foreach(var d in items){Note($"#{d.Id} · R$ {d.Valor:N2} · {Friendly(d.Status)}\n{Friendly(d.FormaPagamento)}");if(d.Status=="pendente")Action($"Cancelar doação #{d.Id}",async()=>{if(await Confirm("Cancelar esta intenção?")){await api.Post<object>($"api/me/doacoes/{d.Id}/cancelar");await Donations();}});}}
 async Task Volunteer(){var v=await api.Get<VoluntarioDto>("api/me/voluntariado");Page("Seja voluntário","Encontre uma forma de ajudar que combine com sua rotina.","ajudar");Note(v==null?"Conte como você pode ajudar.":"Situação: "+Friendly(v.Status));var area=Field("Área de interesse",v?.Area??"");var disp=Field("Dias e horários disponíveis",v?.Disponibilidade??"");Action("Salvar inscrição",async()=>{await api.Put<object>("api/me/voluntariado",new VoluntarioInput(area.Text??"",disp.Text??""));await Volunteer();});if(v!=null&&v.Status!="inativo")Action("Desativar inscrição",async()=>{if(await Confirm("Desativar sua inscrição?")){await api.Delete("api/me/voluntariado");await Volunteer();}});}
 async Task Profile(){var u=await api.Me()??throw new Exception("Entre para continuar.");Page("Minha conta","Mantenha seus dados e preferências atualizados.","conta");Note(u.Email);var nome=Field("Nome completo",u.Nome);var cpf=Field("CPF",u.Cpf??"",keyboard:Keyboard.Numeric);var tel=Field("Telefone",u.Telefone,keyboard:Keyboard.Telephone);var end=Field("Endereço",u.Endereco);var sp=Choice("Espécie preferida",["cao","gato"],u.EspeciePreferida);var esp=Choice("Espaço disponível",["pequeno","medio","grande"],u.Espaco);var exp=Choice("Experiência",["nenhuma","alguma","muita"],u.Experiencia);Action("Salvar perfil",async()=>{await api.Put<object>("api/me/",new PerfilInput(nome.Text??"",cpf.Text,tel.Text??"",end.Text??"",Value(sp),Value(esp),Value(exp)));await Profile();});Action("Alterar senha",Password);Action("Sair da conta",async()=>{await api.Logout();await Login();});}
 Task Password(){Page("Alterar senha","Escolha uma senha exclusiva com pelo menos 10 caracteres.","conta");var old=Field("Senha atual",password:true);var next=Field("Nova senha (mínimo 10 caracteres)",password:true);Action("Salvar nova senha",async()=>{await api.Put<object>("api/me/senha",new SenhaInput(old.Text??"",next.Text??""));await api.Forget();await Login();});return Task.CompletedTask;}
}
