from ortools.constraint_solver import pywrapcp
from ortools.constraint_solver import routing_enums_pb2
from json import JSONEncoder
import  collections
import json
import math
import sys
import simplejson

# Distance callback

def distance(lat1, long1, lat2, long2):
    # Note: The formula used in this function is not exact, as it assumes
# the Earth is a perfect sphere.
# Mean radius of Earth in miles
    radius_earth = 3959

    # Convert latitude and longitude to
    # spherical coordinates in radians.
    degrees_to_radians = math.pi/180.0
    phi1 = lat1 * degrees_to_radians
    phi2 = lat2 * degrees_to_radians
    lambda1 = long1 * degrees_to_radians
    lambda2 = long2 * degrees_to_radians
    dphi = phi2 - phi1
    dlambda = lambda2 - lambda1

    a = haversine(dphi) + math.cos(phi1) * math.cos(phi2) * haversine(dlambda)
    c = 2 * math.atan2(math.sqrt(a), math.sqrt(1-a))
    d = radius_earth * c
    return d

def haversine(angle):
    h = math.sin(angle / 2) ** 2
    return h

class SPFResponse (object):
    def toJSON(self):
        return json.dumps(self, default=lambda o: o.__dict__, sort_keys=True, indent=4)

def dumper(obj):
    try:
        return obj.toJSON()
    except:
        return obj.__dict__


class Datacentre (object):
    def __init__(self, displayName, latitude, longitude, name):
        self.displayName = displayName
        #self.id = id
        self.latitude = latitude
        self.longitude = longitude
        self.name = name
        #self.subscriptionId = subscriptionId
    def toJSON(self):
        return json.dumps(self, default=lambda o: o.__dict__, 
            sort_keys=False, indent=4)



class CreateDistanceCallback(object):
  """Create callback to calculate distances between points."""
  
  def Distance(self, from_node, to_node):
    return self.matrix[from_node][to_node]


  def __init__(self, x):
    """Array of distances between points."""
    # Latitudes and longitudes of Azure Datacenters
    self.coordinates2 = x
       
    """Create the distance matrix."""
    size = len(self.coordinates2)
    self.matrix = {}

    for i in range(0,len(self.coordinates2)):
        self.matrix[i] = {}
        for j in range(0,len(self.coordinates2)):
            if i == j:
                self.matrix[i][j] = 0
            else:
                x1 = float(self.coordinates2.items()[i][1][0])
                y1 = float(self.coordinates2.items()[i][1][1])
                x2 = float(self.coordinates2.items()[j][1][0])
                y2 = float(self.coordinates2.items()[j][1][1])
                self.matrix[i][j] = distance(x1, y1, x2, y2)



def main(args={}):

  if( len(args) > 1):
    coordinates=args
  else:
    # Cities
    coordinates = collections.OrderedDict(
                                                [
                                                    ('eastasia',  [22.267,114.188]), 
                                                    ('southeastasia', [1.283,103.833]),
                                                    ('centralus',[41.5908,-93.6208]), 
                                                    ('eastus',[37.3719,-79.8164]), 
                                                    ('eastus2',[36.6681,-78.3889]), 
                                                    ('westus',[37.783,-122.417]), 
                                                    ('northcentralus',[41.8819,-87.6278]), 
                                                    ('southcentralus',[29.4167,-98.5]), 
                                                    ('northeurope',[53.3478,-6.2597]), 
                                                    ('westeurope',[52.3667,4.9]), 
                                                    ('japanwest',[34.6939,135.502]), 
                                                    ('japaneast',[35.68,139.77]), 
                                                    ('brazilsouth',[-23.55,-46.633]), 
                                                    ('australiaeast',[-33.86,151.209]), 
                                                    ('australiasoutheast',[-37.8136,144.963]), 
                                                    ('southindia',[12.9822,80.1636]), 
                                                    ('centralindia',[18.5822,73.9197]), 
                                                    ('westindia',[19.088,72.868]), 
                                                    ('canadacentral',[43.653,-79.383]), 
                                                    ('canadaeast',[46.817,-71.217]), 
                                                    ('uksouth',[50.941,-0.799]), 
                                                    ('ukwest',[53.427,-3.084]), 
                                                    ('westcentralus',[40.89,-110.234]), 
                                                    ('westus2',[47.233,-119.852]), 
                                                    ('koreacentral',[37.5665,126.978]), 
                                                    ('koreasouth',[35.1796,129.076]) 
                                                ]
                                            )
 
  
  
  tsp_size = len(coordinates)

  # Create routing model
  if tsp_size > 0:
    # TSP of size tsp_size
    # Second argument = 1 to build a single tour (it's a TSP).
    # Nodes are indexed from 0 to tsp_size - 1. By default the start of
    # the route is node 0.
    routing = pywrapcp.RoutingModel(tsp_size, 1,0)
    search_parameters = pywrapcp.RoutingModel.DefaultSearchParameters()

    # Setting first solution heuristic: the
    # method for finding a first solution to the problem.
    search_parameters.first_solution_strategy = (
        routing_enums_pb2.FirstSolutionStrategy.PATH_CHEAPEST_ARC)

    # Create the distance callback, which takes two arguments (the from and to node indices)
    # and returns the distance between these nodes.

    dist_between_nodes = CreateDistanceCallback(coordinates)
    dist_callback = dist_between_nodes.Distance
    routing.SetArcCostEvaluatorOfAllVehicles(dist_callback)
    # Solve, returns a solution if any.
    assignment = routing.SolveWithParameters(search_parameters)
    if assignment:
      # Solution cost.
      print "Total distance: " + str(assignment.ObjectiveValue()) + " miles\n"
      # Inspect solution.
      # Only one route here; otherwise iterate from 0 to routing.vehicles() - 1
      route_number = 0
      step = 0
      index = routing.Start(route_number) # Index of the variable for the starting node.
     
      route = ''
      latlin = ''
      spf = collections.OrderedDict()
      coordinate = collections.OrderedDict()
     
      while not routing.IsEnd(index):
        # Convert variable indices to node indices in the displayed route.
        route += str(coordinates.keys()[routing.IndexToNode(index)]) + ' -> '
        latlin  +=  '{lat: ' + str(coordinates[str(coordinates.keys()[routing.IndexToNode(index)])][0]) + ', lng: ' + str(coordinates[str(coordinates.keys()[routing.IndexToNode(index)])][1]) + ' },'
        spf[str(coordinates.keys()[routing.IndexToNode(index)])] = [ 'lat: ' + str(coordinates[str(coordinates.keys()[routing.IndexToNode(index)])][0]) , 'lin: ' + str(coordinates[str(coordinates.keys()[routing.IndexToNode(index)])][1]) ]
        DD = Datacentre( str(coordinates.keys()[routing.IndexToNode(index)]), str(coordinates[str(coordinates.keys()[routing.IndexToNode(index)])][0]), str(coordinates[str(coordinates.keys()[routing.IndexToNode(index)])][1]), str(coordinates.keys()[routing.IndexToNode(index)]))
        coordinate[step] = DD
        step = step+1
        # Updating Index
        index = assignment.Value(routing.NextVar(index))
       

      route += str(coordinates.keys()[routing.IndexToNode(index)])
      latlin  +=  '{lat: ' + str(coordinates[str(coordinates.keys()[routing.IndexToNode(index)])][0]) + ', lng: ' + str(coordinates[str(coordinates.keys()[routing.IndexToNode(index)])][1]) + ' }'
      spf[str(coordinates.keys()[routing.IndexToNode(index)])] = [ 'lat: ' + str(coordinates[str(coordinates.keys()[routing.IndexToNode(index)])][0]) , 'lin: ' + str(coordinates[str(coordinates.keys()[routing.IndexToNode(index)])][1]) ]
      DD = Datacentre( str(coordinates.keys()[routing.IndexToNode(index)]), str(coordinates[str(coordinates.keys()[routing.IndexToNode(index)])][0]), str(coordinates[str(coordinates.keys()[routing.IndexToNode(index)])][1]), str(coordinates.keys()[routing.IndexToNode(index)]))
      coordinate[step] = DD



      print "Route:\n\n" + route
      print "\nLatLin: \n\n " + latlin
      print "\n SPF: " + json.dumps(spf)
      print "JSON: " + json.dumps(coordinate, default=dumper, sort_keys=False, indent=4 )

      return (json.dumps(coordinate, default=dumper, sort_keys=False, indent=4 ))
            
      
     
    else:
      print 'No solution found.'
  else:
    print 'Specify an instance greater than 0.'

if __name__ == '__main__':
  main()