import subprocess
import json
import logging
import  collections
import simplejson
import pickle
import tspcalculator
from bottle import *
import bottle


def dumper(obj):
    try:
        return obj.toJSON()
    except:
        return obj.__dict__


class Datacentre (object):
    def __init__(self, displayName, id,  latitude, longitude, name, subscriptionId):
        self.displayName = displayName
        self.id = id
        self.latitude = latitude
        self.longitude = longitude
        self.name = name
        self.subscriptionId = subscriptionId
    def toJSON(self):
        return json.dumps(self, default=lambda o: o.__dict__, 
            sort_keys=False, indent=4)


@route('/')
def hello():
    return "Hello World!"


@bottle.hook('after_request')
def allow_cors():
    def wrapper(*args, **kwargs):
        response.headers['Access-Control-Allow-Origin'] = '*' # * in case you want to be accessed via any website
        response.headers['Access-Control-Allow-Methods'] = 'PUT, GET, POST, DELETE, OPTIONS'
        response.headers['Access-Control-AlloW-Headers'] = 'Origin, Accept, Content-Type, X-Requested-With, X-CSRF-Token'
        #return func(*args, **kwargs)

    #return wrapper


@bottle.error(405)
def method_not_allowed(res):
    if request.method == 'OPTIONS':
        new_res = bottle.HTTPResponse()
        new_res.set_header('Access-Control-Allow-Origin', '*')
        return new_res
    res.headers['Allow'] += ', OPTIONS'
    return request.app.default_error_handler(res)


@bottle.hook('after_request')
def enableCORSAfterRequestHook():
    response.headers['Access-Control-Allow-Origin'] = '*' # * in case you want to be accessed via any website
    response.headers['Access-Control-Allow-Methods'] = 'PUT, GET, POST, DELETE, OPTIONS'
    response.headers['Access-Control-AlloW-Headers'] = 'Origin, Accept, Content-Type, X-Requested-With, X-CSRF-Token'
     

@route('/calculate',method = ['OPTIONS'])
def enableCORSAfterRequestHook():
    bottle.response.set_header('Access-Control-Allow-Origin', '*')

@route('/calculate',method = ['POST'])
#@allow_cors 
def calculate():

    postrequeststring = json.dumps(request.json, sort_keys=False, indent=4)
    python_obj = json.loads(postrequeststring)

    coordinate = collections.OrderedDict()
    i=0
    coordinatesarray = collections.OrderedDict()
    for dc in python_obj:
        print (dc['name'] + "-" +  dc['longitude'] + "-" +  dc['latitude'])
         
        coordinatesarray[dc['name']] =  ( dc['latitude'], dc['longitude'] ) #('eastasia',  [22.267,114.188])
        DD = Datacentre( dc['displayName'], dc['id'], dc['latitude'], dc['longitude'], dc['name'], dc['subscriptionId'])
        coordinate[i] = DD
        i=  i+1
  
   

    temp2 = tspcalculator.main(coordinatesarray)
    
    response.content_type = 'application/json'
    #response.body = json.dumps(temp2, default=dumper, sort_keys=False, indent=4 )
    #return response.
    
    #return json.dumps(temp2, default=dumper, sort_keys=False, indent=4 )
    #return json.dumps(temp2, default=dumper, sort_keys=False, indent=4 )
    """return   dict(data=temp2)
    """
    return temp2
    """
    return 
    [
        {
            "displayName": "East Asia",
            "id": "/subscriptions/dddbadc2-9d31-43f4-a7e3-ae83c9da646a/locations/eastasia",
            "latitude": "22.267",
            "longitude": "114.188",
            "name": "eastasia",
            "subscriptionId": null
        }
    ]

    """

    """
    def process(path):
        return subprocess.check_output(['python',path+'.py'],shell=True)
    """



run(host='0.0.0.0', port=8080, debug=True)

