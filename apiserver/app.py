"""
This script runs the application using a development server.
"""

import bottle
import os
import sys
import subprocess
import json
import logging
import collections
import simplejson
import pickle
import tspcalculator

from bottle import *



# routes contains the HTTP handlers for our server and must be imported.
import routes

if '--debug' in sys.argv[1:] or 'SERVER_DEBUG' in os.environ:
    # Debug mode will enable more verbose output in the console window.
    # It must be set at the beginning of the script.
    bottle.debug(True)

def wsgi_app():
    """Returns the application to make available through wfastcgi. This is used
    when the site is published to Microsoft Azure."""
    return bottle.default_app()

if __name__ == '__main__':
    PROJECT_ROOT = os.path.abspath(os.path.dirname(__file__))
    STATIC_ROOT = os.path.join(PROJECT_ROOT, 'static').replace('\\', '/')
    HOST = os.environ.get('SERVER_HOST', 'localhost')
    try:
        PORT = int(os.environ.get('SERVER_PORT', '5555'))
    except ValueError:
        PORT = 5555

    @bottle.route('/static/<filepath:path>')
    def server_static(filepath):
        """Handler for static files, used with the development server.
        When running under a production server such as IIS or Apache,
        the server should be configured to serve the static files."""
        return bottle.static_file(filepath, root=STATIC_ROOT)


    @bottle.route('/calculate',method = ['OPTIONS'])
    def enableCORSAfterRequestHook():
        bottle.response.set_header('Access-Control-Allow-Origin', '*')

    @bottle.route('/calculate',method = ['POST'])
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
        return temp2
    


    # Starts a local test server.
    bottle.run(server='wsgiref', host=HOST, port=PORT)
