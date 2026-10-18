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
 assert.deepEqual(errors,[]);console.log('PASS: responsive photos and charts, empty/valued series, 320/390px screens, retry isolation, registration, login, profile, adoption, donation, volunteer and administration.');
})().catch(e=>{console.error(e);process.exitCode=1;}).finally(async()=>{await browser?.close();server.kill();});
