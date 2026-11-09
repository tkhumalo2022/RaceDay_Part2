from pathlib import Path
from html.parser import HTMLParser
import json
ROOT = Path(__file__).resolve().parent
class Refs(HTMLParser):
    def __init__(self):
        super().__init__()
        self.refs=[]
    def handle_starttag(self,tag,attrs):
        a=dict(attrs)
        if tag in {"img","script","link"}:
            val=a.get("src") or a.get("href")
            if val and not val.startswith(("http:","https:","#")):
                self.refs.append(val)
page=Refs()
text=(ROOT/"index.html").read_text(encoding="utf-8")
page.feed(text)
assert not [x for x in page.refs if not (ROOT/x).is_file()], page.refs
assert (ROOT/"assets/portal-desktop.png").stat().st_size>10000
assert (ROOT/"assets/portal-mobile.png").stat().st_size>10000
assert 'noindex,nofollow' in text
assert 'READ-ONLY REVIEW' in text
assert 'not' in text.lower()
assert all(x in text for x in ('59 / 59','40 / 40','42647341004','MVC','API','SQL SERVER'))
assert 'data-image' in text and 'role="tab"' in text
print(json.dumps({"status":"PASS","local_refs":page.refs,"images_kb":[round((ROOT/p).stat().st_size/1024) for p in ('assets/portal-desktop.png','assets/portal-mobile.png')]}))