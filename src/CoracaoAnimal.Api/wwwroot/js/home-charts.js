const months = ['Jan','Fev','Mar','Abr','Mai','Jun','Jul','Ago','Set','Out','Nov','Dez'];
const money = value => value.toLocaleString('pt-BR', {style:'currency', currency:'BRL'});
const values = series => Array.from({length:12}, (_,i) => {
 const value = Number(series?.[i]);
 return Number.isFinite(value) && value > 0 ? value : 0;
});

// SVG uses CSS pixels at the current container size, retaining crisp labels on
// high-density screens without stretching a fixed 300x150 canvas bitmap.
function plot(host, series, title, kind, format) {
 const width = Math.max(260, Math.round(host.getBoundingClientRect().width));
 const height = 240, left = kind === 'donations' ? 70 : 34, right = 12, top = 22, bottom = 38;
 const plotWidth = width-left-right, plotHeight = height-top-bottom;
 const peak = Math.max(...series);
 const step = kind === 'adoptions' ? Math.max(1, Math.ceil(peak/4)) : Math.max(1, Math.ceil(peak/4));
 const max = step*4;
 const compact = value => kind === 'donations' ? 'R$ '+new Intl.NumberFormat('pt-BR',{notation:'compact',maximumFractionDigits:1}).format(value) : String(value);
 let drawing = `<title>${title}</title>`;
 for(let i=0;i<=4;i++) {
  const value=i*step, y=height-bottom-value/max*plotHeight;
  drawing+=`<line class="chart-grid" x1="${left}" y1="${y}" x2="${width-right}" y2="${y}"/><text x="${left-8}" y="${y+4}" text-anchor="end">${compact(value)}</text>`;
 }
 series.forEach((value,i)=>{
  const slot=plotWidth/12, x=left+i*slot+slot*.18, h=value/max*plotHeight;
  drawing+=`<rect class="chart-bar ${kind}" x="${x}" y="${height-bottom-h}" width="${slot*.64}" height="${h}" rx="2"><title>${months[i]}: ${format(value)}</title></rect><text x="${left+(i+.5)*slot}" y="${height-13}" text-anchor="middle">${months[i]}</text>`;
 });
 host.innerHTML=`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${width} ${height}" role="img" aria-label="${title}. Valores disponíveis na tabela abaixo." style="font:11px sans-serif;fill:currentColor">${drawing}</svg>`;
}

export function renderMonthlyCharts(container, stats) {
 const year=Number.isInteger(stats.ano)?stats.ano:new Date().getUTCFullYear();
 const adoptions=values(stats.adocoesPorMes), donations=values(stats.doacoesPorMes);
 container.innerHTML=`<div class="monthly-charts"><section class="monthly-series"><h3>Adoções concluídas em ${year}</h3><p>Quantidade por mês de conclusão</p><div data-series="adoptions"></div><p>${adoptions.some(Boolean)?'Total no ano: '+adoptions.reduce((a,b)=>a+b,0):'Nenhuma adoção concluída neste ano.'}</p></section><section class="monthly-series"><h3>Doações confirmadas em ${year}</h3><p>Valores por mês de criação do registro</p><div data-series="donations"></div><p>${donations.some(Boolean)?'Total no ano: '+money(donations.reduce((a,b)=>a+b,0)):'Nenhuma doação confirmada neste ano.'}</p></section></div><details class="chart-values"><summary>Consultar valores mensais em tabela</summary><div class="chart-table-scroll"><table><caption>Resumo mensal de ${year}</caption><thead><tr><th scope="col">Mês</th><th scope="col">Adoções concluídas</th><th scope="col">Doações confirmadas</th></tr></thead><tbody>${months.map((month,i)=>`<tr><th scope="row">${month}</th><td>${adoptions[i]}</td><td>${money(donations[i])}</td></tr>`).join('')}</tbody></table></div></details><p class="chart-note">Doações são intenções até a confirmação administrativa. O gráfico considera o mês de criação dessas doações, não a data de recebimento.</p>`;
 const a=container.querySelector('[data-series="adoptions"]'), d=container.querySelector('[data-series="donations"]');
 const draw=()=>{plot(a,adoptions,'Adoções concluídas em '+year,'adoptions',String);plot(d,donations,'Doações confirmadas em '+year,'donations',money);};
 draw();
 const observer=typeof ResizeObserver==='undefined'?null:new ResizeObserver(draw);
 observer?.observe(a);observer?.observe(d);
 return ()=>observer?.disconnect();
}
