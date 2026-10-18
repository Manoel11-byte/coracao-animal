import {renderMonthlyCharts} from './home-charts.js';
const E=x=>String(x??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
let animals=[], user=null;
const go=path=>{location.href='/portal.html#'+path;};
async function api(path,method='GET'){
 const r=await fetch('/api/'+path,{method,credentials:'same-origin',headers:{'X-CSRF':'1'}});
 if(!r.ok)throw new Error('Não foi possível carregar os dados. Tente novamente.');
 return r.status===204?null:r.json();
}
function theme(value){document.documentElement.dataset.theme=value;const i=document.querySelector('#temaIcone');if(i)i.textContent=value==='dark'?'🌙':'☀️';}
try{theme(localStorage.getItem('ca_theme')||localStorage.getItem('tema')||'light');}catch{theme('light');}
function card(a){
 const image=a.fotoUrl&&a.fotoUrl.startsWith('/')?`<img src="${E(a.fotoUrl)}" alt="Foto de ${E(a.nome)}" loading="lazy">`:`<div class="animal-foto-placeholder">${a.especie==='gato'?'🐱':'🐕'}</div>`;
 return `<article class="animal-card"><a href="/portal.html#animal/${a.id}" class="animal-foto">${image}</a><div class="animal-info"><div class="animal-nome-row"><div class="animal-nome">${E(a.nome)}</div><span class="animal-badge">${a.especie==='gato'?'Gato':'Cão'}</span></div><div class="animal-meta">${E(a.raca)} · ${a.idade} ano(s) · ${E(a.porte)}</div><div class="animal-desc">${E(a.descricao)}</div><a class="btn-outline-full" href="/portal.html#animal/${a.id}">🧡 Quero adotar</a></div></article>`;
}
function render(filter='all'){
 const list=animals.filter(a=>filter==='all'||a.especie===filter);
 document.querySelector('#animalGrid').innerHTML=list.slice(0,6).map(card).join('')||'<div class="empty"><p>🐾 Nenhum animal disponível no momento.</p></div>';
}
document.addEventListener('click',async e=>{
 const filter=e.target.closest('[data-home-filter]');if(filter){document.querySelectorAll('[data-home-filter]').forEach(b=>b.classList.toggle('active',b===filter));render(filter.dataset.homeFilter);return;}
 const b=e.target.closest('[data-home-action]');if(!b)return;
 switch(b.dataset.homeAction){
  case'theme':{const v=document.documentElement.dataset.theme==='dark'?'light':'dark';theme(v);try{localStorage.setItem('ca_theme',v);}catch{}break;}
  case'menu':document.querySelector('#navMobile')?.classList.toggle('aberto');document.querySelector('#navHamburguer')?.classList.toggle('aberto');break;
  case'match':go('recomendacoes');break;
  case'logout':try{await api('auth/logout','POST');location.reload();}catch(ex){alert(ex.message);}break;
 }
});
let stopChart;
function failure(target, retry) {
 target.replaceChildren();
 const box=document.createElement('div');box.className='home-data-error';box.setAttribute('role','status');
 const message=document.createElement('p');message.textContent='Não foi possível carregar os dados. Tente novamente.';
 const button=document.createElement('button');button.className='btn-sec';button.textContent='Tentar novamente';
 button.addEventListener('click',async()=>{button.disabled=true;try{await retry();}finally{button.disabled=false;}});
 box.append(message,button);target.append(box);
}
async function loadAnimals(){
 try{const page=await api('animais?status=disponivel&tamanho=50');animals=page.itens;render(document.querySelector('[data-home-filter].active')?.dataset.homeFilter||'all');}
 catch{failure(document.querySelector('#animalGrid'),loadAnimals);}
}
async function loadStats(){
 try{
  const stats=await api('resumo');
  const counts={'statDisponiveis':stats.disponiveis,'dash-total-animals':stats.totalAnimais,'dash-available':stats.disponiveis,'dash-adoptions':stats.adocoesConcluidas,'dash-month-adoptions':stats.adocoesMes,'dash-adopters':stats.adotantes,'dash-raised':stats.doacoesConfirmadas.toLocaleString('pt-BR',{style:'currency',currency:'BRL'}),'dash-adopted':stats.adocoesConcluidas,'dash-rate':stats.taxa+'%'};
  for(const[id,v]of Object.entries(counts)){const el=document.getElementById(id);if(el)el.textContent=v;}
  const bar=document.querySelector('#dash-rate-bar');if(bar)bar.style.width=Math.max(0,Math.min(100,stats.taxa))+'%';
  stopChart?.();stopChart=renderMonthlyCharts(document.querySelector('#monthlyChart'),stats);
 }catch{failure(document.querySelector('#monthlyChart'),loadStats);}
}
async function loadAccount(){
try{user=await api('me/');}catch{}
document.querySelectorAll('[data-auth]').forEach(el=>{const rule=el.dataset.auth;const visible=rule==='admin'?user?.papel==='admin':rule==='logged-in'?!!user:!user;el.hidden=!visible;el.style.display=visible?(rule==='logged-in'?'flex':rule==='admin'?'inline-flex':''):'none';});
document.querySelectorAll('[data-username]').forEach(el=>el.textContent=user?.nome||'');
const actions=document.querySelector('.nav-actions');if(actions){const a=document.createElement('a');a.href='/portal.html#'+(user?'perfil':'entrar');a.className='btn-cta';a.textContent=user?'Minha conta':'Entrar';actions.append(a);}
}
await Promise.all([loadAnimals(),loadStats(),loadAccount()]);
