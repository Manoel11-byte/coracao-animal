"""Real native UI actions, checked through a separate web session on disposable SQLite.
Test builds use loopback URLs at compile time; distributable builds retain Azure.
No production database or account is used.
"""
import concurrent.futures
import json
import os
from pathlib import Path
import re
import shlex
import subprocess
import sys
import time
import urllib.error
import urllib.request
import uuid
import xml.etree.ElementTree as ET

platform=sys.argv[1]
root=Path(__file__).resolve().parents[1]
out=root/'artifacts'/('full-'+platform)
out.mkdir(parents=True,exist_ok=True)
base='http://127.0.0.1:5098'
email='native-'+uuid.uuid4().hex[:10]+'@example.test'
password='Native-QA-987654321'
new_password='Native-Changed-987654321'
server=None
web_cookie=None
checks=[]

def call(path,method='GET',body=None,cookie=None,token=None):
    headers={'X-CSRF':'1','Content-Type':'application/json'}
    if cookie: headers['Cookie']=cookie
    if token: headers['Authorization']='Bearer '+token
    request=urllib.request.Request(base+path,data=json.dumps(body).encode() if body is not None else (b'' if method=='POST' else None),method=method,headers=headers)
    with urllib.request.urlopen(request,timeout=20) as response:
        raw=response.read()
        return (json.loads(raw) if raw else None),response.headers

def web(path,method='GET',body=None):
    return call(path,method,body,cookie=web_cookie)[0]

def eventually(predicate,description):
    end=time.monotonic()+30
    while time.monotonic()<end:
        try:
            result=predicate()
            if result: return result
        except (urllib.error.URLError,KeyError,IndexError): pass
        time.sleep(.3)
    raise AssertionError(description)

def passed(name):
    checks.append(name)
    print('PASS:',name,flush=True)
    (out/'results.json').write_text(json.dumps({'platform':platform,'passed':checks},ensure_ascii=False,indent=2),encoding='utf-8')

def start_server():
    global server
    env={**os.environ,'ASPNETCORE_ENVIRONMENT':'Development','Urls':base,'Database__Provider':'Sqlite','Database__Initialize':'true','Database__SeedDemo':'true','ConnectionStrings__CoracaoAnimal':'Data Source='+str(out/'test.db'),'Bootstrap__Email':'admin@example.test','Bootstrap__Password':'Local-Admin-987654321'}
    log=(out/'server.log').open('a',encoding='utf-8')
    server=subprocess.Popen(['dotnet',str(root/'src/CoracaoAnimal.Api/bin/Release/net10.0/CoracaoAnimal.Api.dll')],cwd=root/'src/CoracaoAnimal.Api',env=env,stdout=log,stderr=subprocess.STDOUT)
    eventually(lambda:call('/health/ready')[0]['status']=='ok','Local server startup')

def stop_server():
    global server
    if server:
        server.terminate()
        try: server.wait(timeout=10)
        except subprocess.TimeoutExpired: server.kill();server.wait()
        server=None

class Windows:
    def __init__(self):
        self.process=subprocess.Popen(['powershell','-NoProfile','-ExecutionPolicy','Bypass','-File',str(root/'tests/windows-driver.ps1'),str(root/'artifacts/native-test/windows/CoracaoAnimal.exe')],stdin=subprocess.PIPE,stdout=subprocess.PIPE,stderr=(out/'driver.log').open('w'),text=True,encoding='utf-8')
    def command(self,op,name='',value='',**extra):
        self.process.stdin.write(json.dumps(dict(op=op,name=name,value=value,**extra))+'\n');self.process.stdin.flush()
        line=self.process.stdout.readline().lstrip('\ufeff')
        if not line: raise AssertionError('Windows driver stopped')
        result=json.loads(line)
        assert 'error' not in result,result.get('error')
        return result.get('value')
    def wait(self,name): return self.command('wait',name)
    def click(self,name): return self.command('click',name)
    def fill(self,name,value):return self.command('fill',name,value)
    def read(self,name):return self.command('read',name)
    def check(self):return self.command('check')
    def confirm(self):return self.click('CONFIRM')
    def capture(self,name):return self.command('capture',path=str(out/(name+'.png')))
    def close(self):
        self.process.stdin.close()
        try:self.process.wait(timeout=10)
        except subprocess.TimeoutExpired:self.process.kill()

class Android:
    def __init__(self):
        apks=list((root/'src/CoracaoAnimal.App/bin/Release/net10.0-android').glob('*Signed.apk'))
        assert apks,'Test APK missing'
        self.adb('install','-r',str(apks[0]))
        self.adb('shell','monkey','-p','br.org.coracaoanimal.app','-c','android.intent.category.LAUNCHER','1')
        size=self.adb('shell','wm','size').decode()
        self.width,self.height=map(int,re.findall(r'\d+',size)[-2:])
    def adb(self,*args):return subprocess.check_output(['adb',*args],timeout=30,stderr=subprocess.STDOUT)
    def tree(self):
        self.adb('shell','uiautomator','dump','/sdcard/full-ui.xml')
        return ET.fromstring(self.adb('shell','cat','/sdcard/full-ui.xml'))
    def scroll(self,up=False):
        self.adb('shell','input','swipe',str(self.width//2),str(int(self.height*(.38 if up else .72))),str(self.width//2),str(int(self.height*(.72 if up else .38))),'250')
    def find(self,name,edit=False,button=False):
        previous=None;up=False
        for attempt in range(36):
            try:
                tree=self.tree()
                nodes=list(tree.iter('node'))
                signature=tuple((n.get('text'),n.get('content-desc'),n.get('bounds')) for n in nodes if n.get('package')=='br.org.coracaoanimal.app')
                for node in nodes:
                    if node.get('enabled')!='true':continue
                    if edit and not node.get('class','').endswith('EditText'):continue
                    if button and not node.get('clickable')=='true':continue
                    text=node.get('text','');desc=node.get('content-desc','')
                    if text==name or desc==name or (name=='TERMS' and desc.startswith('Entendo que')):return node
                if signature==previous:up=not up
                previous=signature
                if attempt>1:self.scroll(up=up)
            except (subprocess.CalledProcessError,ET.ParseError):pass
            time.sleep(.2)
        self.capture('missing-control')
        raise AssertionError('Android control missing: '+name)
    def tap_node(self,node):
        x1,y1,x2,y2=map(int,re.findall(r'\d+',node.get('bounds')))
        self.adb('shell','input','tap',str((x1+x2)//2),str((y1+y2)//2))
    def wait(self,name):return self.find(name)
    def click(self,name):self.tap_node(self.find(name,button=True))
    def fill(self,name,value):
        node=self.find(name,edit=True);self.tap_node(node)
        self.adb('shell','input','keycombination','KEYCODE_CTRL_LEFT','KEYCODE_A')
        self.adb('shell','input','text',shlex.quote(value.replace(' ','%s')))
        self.adb('shell','input','keyevent','KEYCODE_BACK')
    def read(self,name):return self.find(name,edit=True).get('text')
    def check(self):self.tap_node(self.find('TERMS'))
    def confirm(self):self.click('Sim')
    def capture(self,name):(out/(name+'.png')).write_bytes(self.adb('exec-out','screencap','-p'))
    def close(self):self.adb('shell','am','force-stop','br.org.coracaoanimal.app')

ui=None
try:
    start_server()
    ui=Windows() if platform=='windows' else Android()
    explore='Explorar animais' if platform=='windows' else 'Explorar'
    help_tab='Como ajudar' if platform=='windows' else 'Ajudar'
    account='Minha conta' if platform=='windows' else 'Conta'
    search_button='Buscar animais' if platform=='windows' else 'Buscar'
    ui.wait(search_button)
    ui.click(account);ui.wait('Entrar');ui.click('Criar conta');ui.wait('Cadastrar')
    ui.fill('Nome completo','QA Native');ui.fill('E-mail',email);ui.fill('Senha (mínimo 10 caracteres)',password);ui.click('Cadastrar')
    ui.wait('Salvar perfil')
    passed('cadastro e login pelo aplicativo nativo')
    data,headers=call('/api/auth/login','POST',{'email':email,'senha':password,'cliente':'web'})
    web_cookie=headers.get('Set-Cookie').split(';',1)[0]
    assert data['token'] is None
    ui.fill('Nome completo','QA '+platform);ui.fill('CPF','52998224725');ui.fill('Telefone','11999999999');ui.fill('Endereço','Rua QA 123')
    ui.click('Salvar perfil')
    eventually(lambda:web('/api/me/')['nome']=='QA '+platform,'Native profile not visible to web session')
    ui.capture('01-profile')
    passed('perfil salvo na UI nativa e lido pela sessão web')
    profile=web('/api/me/');profile['nome']='QA From Web'
    web('/api/me/','PUT',profile)
    ui.click(explore);ui.wait(search_button);ui.click(account);ui.wait('Salvar perfil')
    assert ui.read('Nome completo')=='QA From Web'
    passed('perfil alterado na sessão web e refletido no aplicativo')
    ui.click(explore);ui.wait(search_button)
    ui.click('Para você');ui.wait('Companheiros para você');ui.click('Voltar a todos os animais');ui.wait(search_button)
    ui.fill('Nome ou raça' if platform=='windows' else 'Buscar por nome ou raça','Amora');ui.click(search_button);ui.wait('Conhecer Amora');ui.click('Conhecer Amora')
    ui.wait('Solicitar adoção');ui.check();ui.click('Solicitar adoção')
    adoption=eventually(lambda:next(iter(web('/api/me/adocoes')),None),'Adoption did not persist')
    assert adoption['status']=='pendente'
    ui.wait('Cancelar solicitação #'+str(adoption['id']));ui.capture('02-adoption')
    ui.click('Cancelar solicitação #'+str(adoption['id']));ui.confirm()
    eventually(lambda:web('/api/me/adocoes')[0]['status']=='cancelada','Cancellation missing')
    passed('busca, recomendações, detalhes, adoção e cancelamento pela UI; sessão web confirma')
    ui.click(help_tab);ui.click('Registrar uma doação');ui.wait('Registrar intenção')
    ui.fill('Valor em reais','25.50');ui.fill('Observação','QA intention');ui.click('Registrar intenção')
    donation=eventually(lambda:next(iter(web('/api/me/doacoes')),None),'Donation missing')
    assert donation['valor']==25.5
    ui.wait('Cancelar doação #'+str(donation['id']));ui.capture('03-donation')
    ui.click('Cancelar doação #'+str(donation['id']));ui.confirm()
    eventually(lambda:web('/api/me/doacoes')[0]['status']=='cancelada','Donation cancellation missing')
    passed('intenção de doação e cancelamento pela UI, sem cobrança')
    ui.click(help_tab);ui.click('Quero ser voluntário');ui.wait('Salvar inscrição')
    ui.fill('Área de interesse','QA walks');ui.fill('Dias e horários disponíveis','QA Saturday');ui.click('Salvar inscrição')
    eventually(lambda:(web('/api/me/voluntariado') or {}).get('area')=='QA walks','Volunteer missing')
    ui.wait('Desativar inscrição');ui.capture('04-volunteer');ui.click('Desativar inscrição');ui.confirm()
    eventually(lambda:web('/api/me/voluntariado')['status']=='inativo','Volunteer cancellation missing')
    passed('inscrição e desativação de voluntariado pela UI')
    ui.click(account);ui.wait('Salvar perfil');ui.click('Alterar senha');ui.wait('Salvar nova senha')
    ui.fill('Senha atual',password);ui.fill('Nova senha (mínimo 10 caracteres)',new_password);ui.click('Salvar nova senha');ui.wait('Entrar')
    try: web('/api/me/');raise AssertionError('Old web session survived password change')
    except urllib.error.HTTPError as ex: assert ex.code==401
    try:call('/api/auth/login','POST',{'email':email,'senha':password,'cliente':'app'});raise AssertionError('Old password accepted')
    except urllib.error.HTTPError as ex:assert ex.code==401
    ui.fill('E-mail',email);ui.fill('Senha',new_password);ui.click('Entrar');ui.wait('Salvar perfil')
    passed('troca de senha, invalidação da senha anterior e de outras sessões, novo login')
    stop_server();ui.click(explore)
    if platform=='windows':ui.wait('Tentar novamente')
    else:ui.wait('Entendi');ui.click('Entendi')
    ui.capture('05-offline');start_server()
    if platform=='windows':ui.click('Tentar novamente')
    else:ui.click(explore)
    ui.wait(search_button)
    passed('falha de conexão visível e recuperação após reinício do serviço')
    ui.click(account);ui.wait('Salvar perfil');ui.click('Sair da conta');ui.wait('Entrar')
    ui.click('Minhas adoções' if platform=='windows' else 'Adoções');ui.wait('Entrar')
    passed('logout e bloqueio de aba autenticada após sair')
    ui.capture('06-logout')
    print('PASS: full native review completed with disposable SQLite database.',flush=True)
except Exception as exc:
    (out/'failure.txt').write_text(str(exc),encoding='utf-8')
    if ui:
        try:ui.capture('failure')
        except Exception:pass
    raise
finally:
    if ui:ui.close()
    stop_server()
