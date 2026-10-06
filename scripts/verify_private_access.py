"""Read-only privacy smoke test. Set MYNOTES_TEST_PASSWORD or enter it at the prompt."""
import getpass
import http.cookiejar
import os
import urllib.error
import urllib.parse
import urllib.request
from html.parser import HTMLParser

BASE = os.environ.get('MYNOTES_TEST_URL', 'http://localhost:5197').rstrip('/')
password = os.environ.get('MYNOTES_TEST_PASSWORD') or getpass.getpass('Private area password: ')
jar = http.cookiejar.CookieJar()
opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(jar))


class HiddenInputs(HTMLParser):
    def __init__(self):
        super().__init__()
        self.values = {}

    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        if tag == 'input' and attrs.get('type') == 'hidden':
            self.values[attrs['name']] = attrs.get('value', '')


def get(path):
    response = opener.open(BASE + path)
    return response, response.read().decode()


def post(path, fields):
    return opener.open(BASE + path, urllib.parse.urlencode(fields).encode())


def fields_from(html):
    parser = HiddenInputs()
    parser.feed(html)
    return parser.values


response, html = get('/')
assert 'A little less to watch later.' in html and 'href="/journal"' in html
assert '/moviereview' not in html and 'MovieReview' not in html
response, html = get('/journal')
assert 'Every story leaves' in html and 'href="/moviereview"' not in html
for path in ['/moviereview', '/moviereview/notes', '/moviereview/scenes/4/view', '/moviereview/app.css']:
    response, html = get(path)
    assert '/private' in response.url and 'Private area' in html
print('Public navigation and private URL/asset protection passed.')

try:
    post('/private/unlock', {'password': 'wrong'})
    raise AssertionError('Missing antiforgery token accepted')
except urllib.error.HTTPError as error:
    assert error.code == 400
response, html = get('/private?returnUrl=/moviereview/notes')
response = post('/private/unlock', fields_from(html) | {'password': 'deliberately incorrect'})
assert 'error=1' in response.url
print('Antiforgery and incorrect password rejection passed.')

response, html = get('/private?returnUrl=/moviereview/notes')
response = post('/private/unlock', fields_from(html) | {'password': password})
html = response.read().decode()
assert response.url.endswith('/moviereview/notes')
assert 'Thoughts and details worth keeping.' in html and 'Lock private area' in html
old_cookie = '; '.join(cookie.name + '=' + cookie.value for cookie in jar)
post('/private/lock', fields_from(html)).read()
response, html = get('/moviereview/notes')
assert '/private' in response.url
stale = urllib.request.build_opener().open(urllib.request.Request(
    BASE + '/moviereview/notes', headers={'Cookie': old_cookie}))
assert '/private' in stale.url
print('Unlock, protected database read, lock and old-cookie revocation passed.')
