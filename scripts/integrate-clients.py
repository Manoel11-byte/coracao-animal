"""Atualização mecânica dos clientes para conexão automática e do portal web."""
from pathlib import Path
root = Path(__file__).resolve().parents[1]
for file, entry in [('src/CoracaoAnimal.App/MainPage.cs','("Servidor",Settings)'),('src/CoracaoAnimal.Windows/MainForm.cs','("Conexão",Settings)')]:
    path = root / file
    text = path.read_text()
    text = text.replace(','+entry,'').replace('if(api.BaseUri==null)await Settings();else await Catalog();','await Catalog();')
    path.write_text(text)
path=root/'src/CoracaoAnimal.Api/Program.cs'
text=path.read_text().replace("style-src 'self'; img-src", "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; font-src 'self' https://fonts.gstatic.com; img-src")
path.write_text(text)
path=root/'src/CoracaoAnimal.App/CoracaoAnimal.App.csproj'
text=path.read_text().replace('<ApplicationDisplayVersion>1.0.0</ApplicationDisplayVersion>','<ApplicationDisplayVersion>1.0.1</ApplicationDisplayVersion>').replace('<ApplicationVersion>1</ApplicationVersion>','<ApplicationVersion>2</ApplicationVersion>')
path.write_text(text)
path=root/'src/CoracaoAnimal.Api/wwwroot/app.js'
text=path.read_text().replace("case'inicio':html=await home();break;", "case'inicio':location.replace('/index.html');return;")
text=text.replace("location.hash=back&&!back.includes('://')?back:'inicio';", "location.hash=back&&!back.includes('://')?back:'perfil';")
path.write_text(text)
