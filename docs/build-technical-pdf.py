from pathlib import Path
import re,html,sys
from reportlab.pdfgen import canvas
from reportlab.platypus import SimpleDocTemplate,Paragraph,Spacer,Table,TableStyle,PageBreak,KeepTogether,Image,CondPageBreak
from reportlab.lib.styles import getSampleStyleSheet,ParagraphStyle
from reportlab.lib.colors import HexColor,white
from reportlab.lib.enums import TA_LEFT
from reportlab.lib.pagesizes import A4
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.graphics.shapes import Drawing,Rect,String,Line,Polygon
root=Path(__file__).resolve().parents[1]
out=Path(sys.argv[1]) if len(sys.argv)>1 else root/'artifacts/docs/Documentacao-Tecnica-Coracao-Animal.pdf'
out.parent.mkdir(parents=True,exist_ok=True)
font_root=Path('/usr/share/fonts/truetype/dejavu')
for name,file in [('Body','DejaVuSans.ttf'),('Strong','DejaVuSans-Bold.ttf')]:pdfmetrics.registerFont(TTFont(name,str(font_root/file)))
pdfmetrics.registerFontFamily('Body',normal='Body',bold='Strong',italic='Body',boldItalic='Strong')
ink=HexColor('#2A1F14');brown=HexColor('#3D2B1F');orange=HexColor('#B8431B');muted=HexColor('#7A6355');line=HexColor('#E5D9CC');cream=HexColor('#F7F3EE')
styles={
 'body':ParagraphStyle('body',fontName='Body',fontSize=9,leading=14,textColor=ink,spaceAfter=9,splitLongWords=True),
 'h1':ParagraphStyle('h1',fontName='Strong',fontSize=18,leading=23,textColor=brown,spaceBefore=17,spaceAfter=12,keepWithNext=True),
 'h2':ParagraphStyle('h2',fontName='Strong',fontSize=11,leading=16,textColor=orange,spaceBefore=13,spaceAfter=8,keepWithNext=True),
 'cell':ParagraphStyle('cell',fontName='Body',fontSize=7.7,leading=11,textColor=ink,splitLongWords=True),
 'th':ParagraphStyle('th',fontName='Strong',fontSize=7.8,leading=11,textColor=white,splitLongWords=True),
 'cover':ParagraphStyle('cover',fontName='Strong',fontSize=31,leading=39,textColor=brown,spaceAfter=16),
 'sub':ParagraphStyle('sub',fontName='Body',fontSize=13,leading=20,textColor=muted,spaceAfter=14),
 'small':ParagraphStyle('small',fontName='Body',fontSize=8,leading=12,textColor=muted,spaceAfter=6)
}
def markup(text):
 text=html.escape(text)
 text=re.sub(r'`([^`]+)`',r'<font name="Body">\1</font>',text)
 text=re.sub(r'\*\*(.+?)\*\*',r'<b>\1</b>',text)
 text=re.sub(r'(https://[^\s<]+)',r'<link href="\1" color="#B8431B">\1</link>',text)
 return text

def architecture():
 d=Drawing(470,220)
 def box(x,y,w,label,fill=cream):
  d.add(Rect(x,y,w,34,rx=8,ry=8,fillColor=fill,strokeColor=line))
  d.add(String(x+w/2,y+13,label,fontName='Strong',fontSize=8.5,textAnchor='middle',fillColor=white if fill==brown else ink))
 def arrow(x1,y1,x2,y2):
  d.add(Line(x1,y1,x2,y2,strokeColor=muted,strokeWidth=1))
  d.add(Polygon([x2-3,y2+5,x2+3,y2+5,x2,y2],fillColor=muted,strokeColor=muted))
 for x,label in [(8,'Site'),(177,'Android'),(346,'Windows')]:box(x,174,116,label);arrow(x+58,174,235,126)
 box(153,92,164,'API ASP.NET Core',brown)
 for x,label in [(8,'Azure SQL'),(177,'Blob privado'),(346,'Key Vault')]:arrow(235,92,x+58,48);box(x,14,116,label)
 return d

def page(c,doc,first=False):
 w,h=A4
 if first:
  c.setFillColor(cream);c.rect(0,0,w,h,fill=1,stroke=0)
  c.setFillColor(orange);c.rect(0,h-18,w,18,fill=1,stroke=0)
 else:
  c.setStrokeColor(line);c.line(48,h-41,w-48,h-41)
  c.setFont('Strong',8);c.setFillColor(brown);c.drawString(48,h-30,'CORAÇÃO ANIMAL')
  c.setFont('Body',7);c.setFillColor(muted);c.drawRightString(w-48,h-30,'Documentação técnica | PIM IV')
 c.setStrokeColor(line);c.line(48,39,w-48,39)
 c.setFont('Body',7);c.setFillColor(muted);c.drawString(48,26,'Revisão de 05/11/2026 | Sem credenciais ou segredos')
 c.drawRightString(w-48,26,str(doc.page))

story=[Spacer(1,45),Image(str(root/'src/CoracaoAnimal.Windows/Assets/logo.png'),width=84,height=84,hAlign='LEFT'),Spacer(1,30),Paragraph('Coração Animal',styles['cover']),Paragraph('Documentação técnica<br/>e revisão de testes',styles['sub']),Spacer(1,22)]
for t in ['Site, Windows e Android sobre uma API central','Arquitetura · Dados · Segurança · Publicação · Homologação','Projeto acadêmico PIM IV / UNIP','Android 1.0.4 | Windows 1.0.5']:
 story.append(Paragraph(t,styles['body']))
story.extend([Spacer(1,24),Paragraph('Este documento descreve a implementação observada no repositório e diferencia testes isolados, verificações no Azure e limites de homologação. Os resultados não equivalem a certificação de produção.',styles['body']),Spacer(1,30),Paragraph('Fonte: github.com/Manoel11-byte/coracao-animal',styles['small']),PageBreak()])
lines=(root/'docs/DOCUMENTACAO-TECNICA.md').read_text().splitlines()
i=0;paragraph=[]
def flush():
 if paragraph:story.append(Paragraph(markup(' '.join(paragraph)),styles['body']));paragraph.clear()
while i<len(lines):
 l=lines[i]
 if l.startswith('# '):i+=1;continue
 if l.startswith('```'):
  flush();lang=l[3:];i+=1
  while i<len(lines) and not lines[i].startswith('```'):i+=1
  if lang=='mermaid':story.append(architecture())
  i+=1;continue
 if l.startswith('## '):flush();story.append(CondPageBreak(100));story.append(Paragraph(markup(l[3:]),styles['h1']));i+=1;continue
 if l.startswith('### '):flush();story.append(CondPageBreak(85));story.append(Paragraph(markup(l[4:]),styles['h2']));i+=1;continue
 if l.startswith('|'):
  flush();rows=[]
  while i<len(lines) and lines[i].startswith('|'):
   cols=[c.strip() for c in lines[i].strip('|').split('|')]
   if not all(re.fullmatch(r'[-: ]+',c) for c in cols):rows.append(cols)
   i+=1
  n=len(rows[0]);width=A4[0]-96
  if rows[0][0]=='Método':widths=[60,207,width-267]
  elif n==4:widths=[87,109,143,width-339]
  elif n==2:widths=[205,width-205]
  else:widths=[101,196,width-297]
  data=[[Paragraph(markup(c),styles['th' if r==0 else 'cell']) for c in row] for r,row in enumerate(rows)]
  table=Table(data,colWidths=widths,repeatRows=1,hAlign='LEFT')
  table.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,0),brown),('VALIGN',(0,0),(-1,-1),'TOP'),('LEFTPADDING',(0,0),(-1,-1),8),('RIGHTPADDING',(0,0),(-1,-1),8),('TOPPADDING',(0,0),(-1,-1),7),('BOTTOMPADDING',(0,0),(-1,-1),7),('ROWBACKGROUNDS',(0,1),(-1,-1),[cream,white]),('LINEBELOW',(0,0),(-1,0),.6,brown),('LINEBELOW',(0,1),(-1,-1),.3,line)]))
  story.extend([table,Spacer(1,11)]);continue
 if not l.strip():flush()
 else:paragraph.append(l)
 i+=1
flush()
class TechnicalDoc(SimpleDocTemplate):
 def afterFlowable(self,flowable):
  if isinstance(flowable,Paragraph) and flowable.style.name=='h1':
   title=flowable.getPlainText();key='section-'+str(getattr(self,'sectionNumber',0));self.sectionNumber=getattr(self,'sectionNumber',0)+1;self.canv.bookmarkPage(key);self.canv.addOutlineEntry(title,key,level=0,closed=False)
doc=TechnicalDoc(str(out),pagesize=A4,rightMargin=48,leftMargin=48,topMargin=57,bottomMargin=57,title='Coração Animal - Documentação técnica',author='Projeto Coração Animal',pageCompression=1)
doc.build(story,onFirstPage=lambda c,d:page(c,d,True),onLaterPages=page)
print(out)
