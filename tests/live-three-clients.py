"""Opt-in Azure protocol integration using one synthetic account and owned records.
Does not operate native GUIs or change animals. No credentials are printed.
"""
import json
import os
from pathlib import Path
import secrets
import urllib.error
import urllib.request
import uuid
if os.environ.get('CORACAO_LIVE_TEST')!='1':raise SystemExit('Set CORACAO_LIVE_TEST=1 to authorize synthetic live records.')
base='https://coracao-pkzqbhzfiqlry.azurewebsites.net'
headers={name:{'Content-Type':'application/json','X-CSRF':'1'} for name in ('web','windows','android')}
email='qa-three-'+uuid.uuid4().hex[:12]+'@example.test';password=secrets.token_urlsafe(24)
checks=[];donation_id=None

def call(client,path,method='GET',body=None):
 request=urllib.request.Request(base+path,data=json.dumps(body).encode() if body is not None else (b'' if method=='POST' else None),method=method,headers=headers[client])
 with urllib.request.urlopen(request,timeout=45) as response:
  raw=response.read();return (json.loads(raw) if raw else None),response.headers

def check(name):checks.append(name);print('PASS:',name,flush=True)
try:
 call('web','/api/auth/cadastro','POST',{'nome':'QA integracao tres clientes','email':email,'senha':password})
 for client in headers:
  session,h=call(client,'/api/auth/login','POST',{'email':email,'senha':password,'cliente':'web' if client=='web' else 'app'})
  if client=='web':headers[client]['Cookie']=h.get('Set-Cookie').split(';',1)[0]
  else:headers[client]['Authorization']='Bearer '+session['token']
 users=[call(c,'/api/me/')[0] for c in headers];assert len({u['id'] for u in users})==1
 check('tres sessoes independentes autenticadas no mesmo usuario no Azure')
 profile=users[0];profile.update(nome='QA Windows Azure',telefone='11999999999',endereco='Endereco ficticio QA')
 call('windows','/api/me/','PUT',profile)
 assert all(call(c,'/api/me/')[0]['nome']=='QA Windows Azure' for c in ('web','android'))
 check('alteracao de perfil na sessao Windows visivel em web e Android')
 d,_=call('android','/api/me/doacoes','POST',{'valor':1,'formaPagamento':'pix','descricao':'QA ficticio: sem pagamento, cancelado pelo teste','anonima':True});donation_id=d['id']
 assert all(any(item['id']==donation_id for item in call(c,'/api/me/doacoes')[0]) for c in ('windows','web'))
 call('web',f'/api/me/doacoes/{donation_id}/cancelar','POST')
 assert all(next(x for x in call(c,'/api/me/doacoes')[0] if x['id']==donation_id)['status']=='cancelada' for c in ('windows','android'))
 check('intencao Android lida nas outras sessoes e cancelada pela sessao web')
 call('windows','/api/me/voluntariado','PUT',{'area':'QA ficticio sem disponibilidade real','disponibilidade':'Registro de teste'})
 assert call('android','/api/me/voluntariado')[0]['area'].startswith('QA ficticio')
 call('web','/api/me/voluntariado','DELETE')
 assert call('windows','/api/me/voluntariado')[0]['status']=='inativo'
 check('voluntariado compartilhado e desativado ao final')
 call('android','/api/me/senha','PUT',{'atual':password,'nova':secrets.token_urlsafe(24)})
 for c in ('web','windows','android'):
  try:call(c,'/api/me/');raise AssertionError('Session was not revoked: '+c)
  except urllib.error.HTTPError as e:assert e.code==401
 check('troca de senha invalida as tres sessoes no Azure')
 out=Path('artifacts/live-three-clients.json');out.parent.mkdir(exist_ok=True)
 out.write_text(json.dumps({'passed':checks,'base':base,'mode':'HTTP protocol; not simultaneous native GUI','syntheticAccount':email,'donationStatus':'cancelada','volunteerStatus':'inativo','sessions':'revoked'},ensure_ascii=False,indent=2),encoding='utf-8')
finally:
 # Best-effort cleanup only of records created by this run, using its own session.
 for c in ('web','windows','android'):
  try:
   if donation_id:
    history=call(c,'/api/me/doacoes')[0]
    if any(x['id']==donation_id and x['status']=='pendente' for x in history):call(c,f'/api/me/doacoes/{donation_id}/cancelar','POST')
   call(c,'/api/me/voluntariado','DELETE');call(c,'/api/auth/logout','POST')
  except urllib.error.HTTPError:pass
