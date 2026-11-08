"""Run against a local disposable demo instance; creates a smoke change and policy."""
import urllib.request,urllib.parse,http.cookiejar,re,json,os
base=os.environ.get('ADVISOR_SMOKE_URL', 'http://127.0.0.1:5187')
client=urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
def get(path):return client.open(base+path).read().decode()
def post(path,data,html):
 data['__RequestVerificationToken']=re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"',html)[1]
 return client.open(base+path,urllib.parse.urlencode(data).encode())
r=post('/infra/changes',dict(Title='Advisor smoke',SystemName='edge',Environment='prod',RequestedBy='tester'),get('/infra/changes/new'))
id=re.search(r'/changes/([^/]+)/view',r.url)[1]
path=f'/infra/changes/{id}'
h=get(path+'/advisor');assert 'Deployment plan' in h
r=post(path+'/plan',dict(description='Rotate cert',implementation='Install cert',rollback='Restore previous cert',verification='Validate TLS'),h)
h=r.read().decode();h=post(path+'/knowledge',dict(text='Verify rollback before production deployment'),h).read().decode()
h=post(path+'/analyses',{},h).read().decode();assert 'Needs human review' in h and 'knowledge:' in h
assert json.loads(get(path+'/analyses/latest'))['isStale']==False
h=post(path+'/plan',dict(description='Rotate cert changed',implementation='Install cert',rollback='Restore previous cert',verification='Validate TLS'),h).read().decode()
assert 'Outdated' in h and json.loads(get(path+'/analyses/latest'))['isStale']==True
print('PASS: antiforgery forms, create, plan, policy, analysis, evidence and staleness HTTP workflow',id)

