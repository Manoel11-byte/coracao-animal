"""Importa a página pública original, com fluxos autenticados no portal integrado."""
from pathlib import Path
import shutil
import urllib.request
from bs4 import BeautifulSoup

source = Path(__file__).resolve().parents[1] / 'frontend'
target = Path(__file__).resolve().parents[1] / 'src/CoracaoAnimal.Api/wwwroot'
video = source / 'img/video_ong.mp4'
if not video.exists():
    urllib.request.urlretrieve('https://raw.githubusercontent.com/m4rcosJr/coracao-animal/main/frontend/img/video_ong.mp4', video)
portal = target / 'portal.html'
if not portal.exists():
    shutil.copy2(target / 'index.html', portal)
for folder in ('css', 'img'):
    shutil.copytree(source / folder, target / folder, dirs_exist_ok=True)
soup = BeautifulSoup((source / 'index.html').read_text(encoding='utf-8-sig'), 'html.parser')
styles = '\n'.join(tag.get_text() for tag in soup.find_all('style'))
(target / 'css/original-home-inline.css').write_text(styles, encoding='utf-8')
for tag in soup.find_all('style'):
    tag.decompose()
link = soup.new_tag('link', rel='stylesheet', href='/css/original-home-inline.css')
soup.head.append(link)
for tag in soup.find_all('script'):
    tag.decompose()
modal = soup.find(id='adoptionModal')
if modal:
    modal.decompose()
widget = soup.find(attrs={'vw': True})
if widget:
    widget.decompose()
routes = {'animais.html':'animais', 'adocao.html':'animais', 'voluntarios.html':'voluntariado', 'doacoes.html':'doacoes', 'admin.html':'admin', 'register-animal.html':'admin/animais', 'login.html':'entrar', 'cadastro.html':'cadastro'}
for tag in soup.find_all(True):
    for attr in list(tag.attrs):
        if attr.startswith('on'):
            code = tag[attr]
            if attr == 'onclick':
                if 'alternarTema' in code: tag['data-home-action'] = 'theme'
                elif 'toggleMenu' in code: tag['data-home-action'] = 'menu'
                elif 'logout' in code: tag['data-home-action'] = 'logout'
                elif 'filterHome' in code:
                    tag['data-home-filter'] = code.split("'")[1]
                elif 'Recomendacao' in code: tag['data-home-action'] = 'match'
                elif 'voluntarios' in code: tag['href'] = '/portal.html#voluntariado'
                elif 'doacoes' in code: tag['href'] = '/portal.html#doacoes'
                elif 'AdoptClick' in code: tag['href'] = '/portal.html#animais'
            del tag[attr]
    href = tag.get('href', '')
    filename = href.rsplit('/', 1)[-1]
    if filename in routes:
        tag['href'] = '/portal.html#' + routes[filename]
    if tag.get('href', '').startswith('javascript:'):
        tag['href'] = '/portal.html#animais'
# Keep integration fixes after imported CSS so re-import/build cannot undo them.
soup.head.append(soup.new_tag('link', rel='stylesheet', href='/css/integrated-home.css?v=1.0.2'))
chart = soup.find(id='monthlyChart')
if chart:
    replacement = soup.new_tag('div', id='monthlyChart')
    replacement['aria-live'] = 'polite'
    chart.replace_with(replacement)
label = soup.select_one('.dash-chart-label')
if label:
    label.string = 'Adoções e doações mensais'
for tag in soup.select('[data-auth]'):
    tag['hidden'] = ''
script = soup.new_tag('script', src='/js/home-integrated.js?v=1.0.2', type='module')
soup.body.append(script)
(target / 'index.html').write_text(str(soup), encoding='utf-8')
