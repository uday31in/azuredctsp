import requests
from collections import OrderedDict
import simplejson as json


with open('azurelocations.json') as json_data:
    d = json.load(json_data,object_pairs_hook=OrderedDict)
    #print(d)


r = requests.post('http://localhost:8080/calculate', json=d)

jsondata = r.json();


myobj = json.loads(r.content, object_pairs_hook=OrderedDict)


for key in myobj:
     print 'key=%s, value=%s' % (key, myobj[key])
     record = json.loads(myobj[key])
     print 'Step: %s Name: %s' % (key,record['name'])

print (r.status_code)
print (r.json())


"""
for datacenter in r.json():
    print (datacenter['name'] + "-" +  datacenter['longitude'] + "-" +  datacenter['latitude'])


print (r.json()[0].keys())
print (r.json()[0]['name'])
print (r.json()[0]['longitude'])
print (r.json()[0]['latitude'])
print (r.json()[0].values())


"""