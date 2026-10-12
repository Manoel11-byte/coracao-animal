"""Verifica que sessões web e app compartilham o SQL. Usa somente dados fictícios."""
import secrets, uuid, urllib.request, urllib.error, json, http.cookiejar
base='https://coracao-pkzqbhzfiqlry.azurewebsites.net'
jar=http.cookiejar.CookieJar()
web=urllib.request.build_opener(urllib.request.HTTPCookieProcessor(jar))
app=urllib.request.build_opener()
token=None
def call(client,path,method='GET',body=None):
    headers={'X-CSRF':'1','Content-Type':'application/json'}
    if client is app and token: headers['Authorization']='Bearer '+token
    req=urllib.request.Request(base+path,data=json.dumps(body).encode() if body is not None else None,headers=headers,method=method)
    with client.open(req,timeout=40) as r:
        raw=r.read()
        return json.loads(raw) if raw else None
email='integracao-'+uuid.uuid4().hex[:12]+'@example.test'
password=secrets.token_urlsafe(24)
call(web,'/api/auth/cadastro','POST',{'nome':'Teste de integração Azure','email':email,'senha':password})
session=call(app,'/api/auth/login','POST',{'email':email,'senha':password,'cliente':'app'})
token=session['token']
call(web,'/api/auth/login','POST',{'email':email,'senha':password,'cliente':'web'})
digits=[secrets.randbelow(10) for _ in range(9)]
for n in (10,11):
    d=11-sum(v*(n-i) for i,v in enumerate(digits))%11
    digits.append(0 if d>=10 else d)
call(app,'/api/me/','PUT',{'nome':'Teste de integração Azure','cpf':''.join(map(str,digits)),'telefone':'11999999999','endereco':'Endereço fictício de teste, 123','especiePreferida':'cao','espaco':'medio','experiencia':'alguma'})
assert call(web,'/api/me/')['id']==session['usuario']['id']
animals=call(app,'/api/animais')['itens']
assert len(animals)>=6
animal=next(a for a in animals if a['status']=='disponivel' and 'demonstração' in a['descricao'])
adoption=call(app,'/api/me/adocoes','POST',{'animalId':animal['id'],'observacoes':'Solicitação fictícia de integração; cancelada ao fim do teste.','aceitaTermos':True})
assert any(a['id']==adoption['id'] for a in call(web,'/api/me/adocoes'))
call(web,f"/api/me/adocoes/{adoption['id']}/cancelar",'POST')
assert call(app,f"/api/animais/{animal['id']}")['status']=='disponivel'
donation=call(app,'/api/me/doacoes','POST',{'valor':1,'formaPagamento':'pix','descricao':'TESTE fictício sem pagamento; cancelado ao fim da validação.','anonima':True})
assert any(d['id']==donation['id'] for d in call(web,'/api/me/doacoes'))
call(web,f"/api/me/doacoes/{donation['id']}/cancelar",'POST')
call(app,'/api/me/voluntariado','PUT',{'area':'Teste fictício de integração','disponibilidade':'Sem disponibilidade real — teste'})
assert call(web,'/api/me/voluntariado')['area']=='Teste fictício de integração'
call(web,'/api/me/voluntariado','DELETE')
assert call(app,'/api/me/voluntariado')['status']=='inativo'
call(app,'/api/auth/logout','POST')
call(web,'/api/auth/logout','POST')
print('OK: catálogo, conta compartilhada, adoção, doação e voluntariado em sessões app/web. Adoção e doação canceladas; voluntariado inativo.')
