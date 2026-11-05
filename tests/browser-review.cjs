// A fresh, disposable local SQLite database only. No production credentials.
const assert=require('node:assert/strict');
const fs=require('node:fs'),path=require('node:path'),{spawn}=require('node:child_process');
const {chromium}=require('playwright');
const root=path.resolve('.'),base='http://127.0.0.1:5098';
fs.mkdirSync('artifacts/review',{recursive:true});
const log=fs.openSync('artifacts/review/server.log','w');
const server=spawn(process.env.DOTNET_EXECUTABLE||'dotnet',[root+'/src/CoracaoAnimal.Api/bin/Release/net10.0/CoracaoAnimal.Api.dll'],{cwd:root+'/src/CoracaoAnimal.Api',env:{...process.env,ASPNETCORE_ENVIRONMENT:'Development',Urls:base,ConnectionStrings__CoracaoAnimal:'Data Source='+root+'/artifacts/review/'+Date.now()+'.db',Database__SeedDemo:'true',Bootstrap__Email:'admin@example.test',Bootstrap__Password:'Browser-Test-987654'},stdio:['ignore',log,log]});
const sleep=ms=>new Promise(r=>setTimeout(r,ms));
let browser;
async function checkTextContrast(page, selector) {
 const colors=await page.locator(selector).evaluateAll(elements=>elements.map(el=>{
  let parent=el,background;
  while(parent){const value=getComputedStyle(parent).backgroundColor;if(value!=='rgba(0, 0, 0, 0)'&&value!=='transparent'){background=value;break;}parent=parent.parentElement;}
  return {text:el.textContent.trim().slice(0,50)||el.getAttribute('name'),color:getComputedStyle(el).color,background:background||'rgb(255, 255, 255)'};
 }));
 const luminance=color=>{const values=color.match(/[\d.]+/g).slice(0,3).map(Number).map(v=>{v/=255;return v<=.04045?v/12.92:((v+.055)/1.055)**2.4;});return values[0]*.2126+values[1]*.7152+values[2]*.0722;};
 for(const item of colors){const a=luminance(item.color),b=luminance(item.background),ratio=(Math.max(a,b)+.05)/(Math.min(a,b)+.05);assert(ratio>=4.5,`${item.text}: contrast ${ratio.toFixed(2)} (${item.color} / ${item.background})`);}
}
(async()=>{
 for(let i=0;;i++){try{if((await fetch(base+'/health/ready')).ok)break;}catch{}if(i>=100)throw Error('Local test server did not start');await sleep(300);}
 browser=await chromium.launch({headless:true});
 const context=await browser.newContext({viewport:{width:1440,height:1000},deviceScaleFactor:2});
 const page=await context.newPage(),errors=[];page.on('pageerror',e=>errors.push(e.message));
 await page.goto(base);await page.locator('.animal-card').first().waitFor();await page.locator('#monthlyChart svg').nth(1).waitFor();
 assert.equal(await page.locator('.animal-card').count(),6);
 const photos=await page.locator('.animal-foto').evaluateAll(els=>els.map(el=>({height:el.getBoundingClientRect().height,display:getComputedStyle(el).display})));
 assert(photos.every(p=>p.height===240&&p.display==='block'),JSON.stringify(photos));
 await page.locator('.animal-card').first().screenshot({path:'artifacts/review/card-desktop.png'});
 await page.locator('.dashboard-wrap').screenshot({path:'artifacts/review/dashboard-empty-desktop.png'});
 await page.getByRole('button',{name:'🐱 Gatos',exact:true}).click();assert.equal(await page.locator('.animal-card').count(),2);
 await page.getByRole('button',{name:'Todos',exact:true}).click();assert.equal(await page.locator('.animal-card').count(),6);
 await page.route('**/api/resumo',route=>route.fulfill({status:503,contentType:'application/json',body:'{}'}));await page.reload();
 await page.locator('#monthlyChart').getByRole('button',{name:'Tentar novamente'}).waitFor();assert.equal(await page.locator('.animal-card').count(),6);
 await page.unroute('**/api/resumo');await page.locator('#monthlyChart').getByRole('button',{name:'Tentar novamente'}).click();await page.locator('#monthlyChart svg').nth(1).waitFor();
 const summary=await (await fetch(base+'/api/resumo')).json();
 await page.route('**/api/resumo',route=>route.fulfill({json:{...summary,adocoesPorMes:[1,0,2,3,1,4,2,1,3,2,4,5],doacoesPorMes:[25.5,0,100,50,120,300,45,60,90,80,65,100]}}));
 await page.reload();await page.locator('#monthlyChart .chart-bar.donations').nth(11).waitFor();
 assert.equal(await page.locator('#monthlyChart .chart-bar.adoptions').count(),12);
 assert.equal(await page.locator('#monthlyChart .chart-bar.donations').count(),12);
 await page.locator('.dashboard-wrap').screenshot({path:'artifacts/review/dashboard-values-desktop.png'});
 for(const width of [390,320]){
  await page.setViewportSize({width,height:844});await page.locator('#monthlyChart summary').click();
  assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth),false,'Horizontal overflow at '+width);
  const charts=await page.locator('#monthlyChart svg').evaluateAll(els=>els.map(el=>({actual:el.getBoundingClientRect().width,viewbox:el.viewBox.baseVal.width})));
  assert(charts.every(c=>Math.abs(c.actual-c.viewbox)<2),JSON.stringify(charts));
  await page.locator('.dashboard-wrap').screenshot({path:`artifacts/review/dashboard-mobile-${width}.png`});
  await page.locator('#monthlyChart summary').click();
 }
 await page.unroute('**/api/resumo');
 await page.setViewportSize({width:1440,height:1000});
 for(const width of [1440,390,320]){
  await page.setViewportSize({width,height:1000});
  for(const dark of [false,true]){
   await page.goto(base+'/portal.html#entrar');await page.locator('#login').waitFor();
   if(await page.locator('body').evaluate(el=>el.classList.contains('dark'))!==dark)await page.getByRole('button',{name:'Alternar tema',exact:true}).click();
   await checkTextContrast(page,'.form-card h1,.form-card .eyebrow,.form-card p,.form-card label,.form-card input,.form-card .btn,.form-card .link');
   assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth>innerWidth),false,'Login overflow at '+width);
   await page.locator('.form-card').screenshot({path:`artifacts/review/login-${dark?'dark':'light'}-${width}.png`});
   await page.reload();await page.locator('#login').waitFor();assert.equal(await page.locator('body').evaluate(el=>el.classList.contains('dark')),dark,'Theme must persist after reload');
   await page.goto(base+'/portal.html#cadastro');await page.locator('#register').waitFor();await checkTextContrast(page,'.form-card h1,.form-card label,.form-card input,.form-card .btn,.form-card .link');
  }
 }
 await page.setViewportSize({width:1440,height:1000});
 if(await page.locator('body').evaluate(el=>el.classList.contains('dark')))await page.getByRole('button',{name:'Alternar tema',exact:true}).click();
 await page.goto(base+'/portal.html#entrar');await page.locator('main [name=email]').fill('admin@example.test');await page.locator('main [name=senha]').fill('Browser-Test-987654');await page.locator('#login button').click();
 await page.getByRole('link',{name:'Administração',exact:true}).waitFor();
 await page.goto(base+'/portal.html#admin/animais');await page.getByRole('button',{name:'Cadastrar animal',exact:true}).click();
 await page.locator('#animal-form [name=nome]').fill('Amigo QA');await page.locator('#animal-form [name=descricao]').fill('Animal fictício para validação local.');await page.getByRole('button',{name:'Salvar animal',exact:true}).click();await page.getByRole('cell',{name:'Amigo QA',exact:true}).waitFor();
 await page.goto(base+'/portal.html#perfil');await page.getByRole('button',{name:'Sair da conta',exact:true}).click();await page.waitForURL('**/index.html');
 await page.goto(base+'/portal.html#cadastro');await page.locator('main [name=nome]').fill('Pessoa QA');await page.locator('main [name=email]').fill('qa@example.test');await page.locator('main [name=senha]').fill('Browser-User-987654');await page.getByRole('button',{name:'Criar minha conta',exact:true}).click();
 await page.locator('#login [name=email]').fill('qa@example.test');await page.locator('main [name=senha]').fill('Browser-User-987654');await page.locator('#login button').click();await page.locator('#profile').waitFor();
 await page.locator('main [name=cpf]').fill('52998224725');await page.locator('main [name=telefone]').fill('11999999999');await page.locator('main [name=endereco]').fill('Rua QA, 123');await page.getByRole('button',{name:'Salvar perfil',exact:true}).click();await page.getByText('Perfil atualizado.',{exact:true}).waitFor();
 await page.goto(base+'/portal.html#animais');await page.getByRole('link',{name:/Conhecer Amigo QA/}).click();await page.getByRole('button',{name:/Quero adotar Amigo QA/}).click();await page.locator('#adopt-form [name=aceitaTermos]').check();await page.getByRole('button',{name:'Enviar solicitação',exact:true}).click();await page.getByRole('heading',{name:'Minhas adoções.',exact:true}).waitFor();
 await page.goto(base+'/portal.html#doacoes');await page.locator('main [name=valor]').fill('25.50');await page.getByRole('button',{name:'Registrar intenção',exact:true}).click();await page.getByRole('heading',{name:/25,50/}).waitFor();
 await page.goto(base+'/portal.html#voluntariado');await page.locator('main [name=area]').fill('Passeios');await page.locator('main [name=disponibilidade]').fill('Sábados');await page.getByRole('button',{name:'Salvar interesse',exact:true}).click();await page.getByText('Interesse atualizado.',{exact:true}).waitFor();

 // Exercise cancellation dialogs and verify the persisted states, not just notices.
 page.on('dialog',dialog=>dialog.accept());
 await page.goto(base+'/portal.html#adocoes');await page.getByRole('button',{name:'Cancelar solicitação',exact:true}).click();await page.getByText('Registro cancelado.',{exact:true}).waitFor();
 assert.equal((await page.evaluate(()=>fetch('/api/me/adocoes').then(r=>r.json())))[0].status,'cancelada');
 await page.goto(base+'/portal.html#doacoes');await page.getByRole('button',{name:'Cancelar intenção',exact:true}).click();await page.getByText('Registro cancelado.',{exact:true}).waitFor();
 assert.equal((await page.evaluate(()=>fetch('/api/me/doacoes').then(r=>r.json())))[0].status,'cancelada');
 await page.goto(base+'/portal.html#voluntariado');await page.getByRole('button',{name:'Encerrar meu interesse',exact:true}).click();await page.getByText('Inativo',{exact:true}).waitFor();
 await page.locator('main [name=area]').fill('Fotografia QA');await page.getByRole('button',{name:'Salvar interesse',exact:true}).click();await page.getByText('Interesse atualizado.',{exact:true}).waitFor();
 await page.goto(base+'/portal.html#recomendacoes');await page.getByRole('heading',{name:'Uma afinidade para começar.',exact:true}).waitFor();
 await page.goto(base+'/portal.html#animais');await page.getByRole('link',{name:/Conhecer Amigo QA/}).click();await page.getByRole('button',{name:/Quero adotar Amigo QA/}).click();await page.locator('#adopt-form [name=aceitaTermos]').check();await page.getByRole('button',{name:'Enviar solicitação',exact:true}).click();await page.getByRole('heading',{name:'Minhas adoções.',exact:true}).waitFor();
 await page.goto(base+'/portal.html#doacoes');await page.locator('main [name=valor]').fill('10');await page.getByRole('button',{name:'Registrar intenção',exact:true}).click();await page.getByText('Intenção registrada. Aguarde orientação da equipe.',{exact:true}).waitFor();
 await page.goto(base+'/portal.html#perfil');await page.locator('#password [name=atual]').fill('Browser-User-987654');await page.locator('#password [name=nova]').fill('Browser-Changed-987654');await page.getByRole('button',{name:'Alterar senha',exact:true}).click();await page.locator('#login').waitFor();
 await page.locator('#login [name=email]').fill('qa@example.test');await page.locator('#login [name=senha]').fill('Browser-User-987654');await page.locator('#login button').click();await page.locator('#notice.error').waitFor();assert(await page.locator('#login').isVisible());
 await page.locator('#login [name=senha]').fill('Browser-Changed-987654');await page.locator('#login button').click();await page.locator('#profile').waitFor();
 await page.screenshot({path:'artifacts/review/profile-authenticated.png'});
 await page.getByRole('button',{name:'Sair da conta',exact:true}).click();await page.waitForURL('**/index.html');
 await page.goto(base+'/portal.html#admin');await page.locator('#login').waitFor();
 await page.locator('#login [name=email]').fill('admin@example.test');await page.locator('#login [name=senha]').fill('Browser-Test-987654');await page.locator('#login button').click();await page.getByRole('link',{name:'Administração',exact:true}).waitFor();
 for(const tab of ['resumo','animais','adocoes','adotantes','doacoes','voluntarios']){
  await page.goto(base+'/portal.html#admin/'+tab);await page.getByRole('navigation',{name:'Módulos administrativos'}).waitFor();await page.locator(tab==='resumo'?'.stats':'.table-wrap table').waitFor();
  await page.screenshot({path:`artifacts/review/admin-${tab}.png`});
 }
 await page.goto(base+'/portal.html#admin/adocoes');let row=page.getByRole('row').filter({hasText:'Amigo QA'}).filter({hasText:'Pendente'});await row.getByRole('button',{name:'Aprovar',exact:true}).click();await page.getByText('Status atualizado.',{exact:true}).waitFor();
 row=page.getByRole('row').filter({hasText:'Amigo QA'}).filter({hasText:'Aprovada'});await row.getByRole('button',{name:'Concluir',exact:true}).click();await page.getByText('Status atualizado.',{exact:true}).waitFor();await page.getByRole('row').filter({hasText:'Amigo QA'}).filter({hasText:'Concluída'}).waitFor();
 await page.goto(base+'/portal.html#admin/doacoes');await page.getByRole('button',{name:'Confirmar recebimento',exact:true}).click();await page.getByText('Status atualizado.',{exact:true}).waitFor();await page.getByRole('cell',{name:'Confirmada',exact:true}).waitFor();
 await page.goto(base+'/portal.html#admin/voluntarios');await page.getByRole('button',{name:'Ativar',exact:true}).click();await page.getByText('Status atualizado.',{exact:true}).waitFor();await page.getByRole('button',{name:'Desativar',exact:true}).click();await page.getByRole('cell',{name:'Inativo',exact:true}).waitFor();
 await page.goto(base+'/portal.html#admin/animais');await page.getByRole('button',{name:'Cadastrar animal',exact:true}).click();await page.locator('#animal-form [name=nome]').fill('Arquivo QA');await page.locator('#animal-form [name=descricao]').fill('Registro descartável para testar edição, foto e arquivamento.');await page.getByRole('button',{name:'Salvar animal',exact:true}).click();await page.getByRole('cell',{name:'Arquivo QA',exact:true}).waitFor();
 row=page.getByRole('row').filter({hasText:'Arquivo QA'});await row.getByRole('button',{name:'Editar',exact:true}).click();await page.locator('#animal-form [name=nome]').fill('Arquivo QA Editado');await page.getByRole('button',{name:'Salvar animal',exact:true}).click();await page.getByRole('cell',{name:'Arquivo QA Editado',exact:true}).waitFor();
 row=page.getByRole('row').filter({hasText:'Arquivo QA Editado'});await row.getByRole('button',{name:'Foto',exact:true}).click();await page.locator('#photo-form [name=foto]').setInputFiles(root+'/src/CoracaoAnimal.Api/wwwroot/img/poodle_imagem.webp');await page.getByRole('button',{name:'Enviar foto',exact:true}).click();await page.getByText('Foto atualizada.',{exact:true}).waitFor();
 await row.getByRole('button',{name:'Arquivar',exact:true}).click();await page.getByRole('cell',{name:'Arquivo QA Editado',exact:true}).waitFor({state:'detached'});
 await page.goto(base+'/portal.html#sobre');await page.getByRole('heading',{name:'Tecnologia a serviço do cuidado.',exact:true}).waitFor();
 await context.setOffline(true);await page.getByRole('link',{name:'Quero adotar',exact:true}).click();await page.getByRole('button',{name:'Tentar novamente',exact:true}).waitFor();await context.setOffline(false);await page.getByRole('button',{name:'Tentar novamente',exact:true}).click();await page.locator('#filters').waitFor();
 await page.goto(base+'/portal.html#perfil');await page.getByRole('button',{name:'Sair da conta',exact:true}).click();await page.waitForURL('**/index.html');await page.goto(base+'/portal.html#perfil');await page.locator('#login').waitFor();
 console.log('PASS: cancellation, reactivation, password change and old password rejection, all six administration tabs, approval/completion, donation confirmation, volunteer activation/deactivation, edit/photo/archive, privacy, offline recovery and logout access control.');
 assert.deepEqual(errors,[]);console.log('PASS: responsive photos and charts, empty/valued series, 320/390px screens, retry isolation, registration, login, profile, adoption, donation, volunteer and administration.');
})().catch(e=>{console.error(e);process.exitCode=1;}).finally(async()=>{await browser?.close();server.kill();});
