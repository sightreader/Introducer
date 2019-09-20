import http.server
import ssl
import os

pathname = os.path.dirname(os.path.realpath(__file__)) 

httpd = http.server.HTTPServer(('0.0.0.0', 443), http.server.SimpleHTTPRequestHandler)
httpd.socket = ssl.wrap_socket(httpd.socket, certfile=os.path.join(pathname, 'server.pem'), server_side=True, ssl_version=ssl.PROTOCOL_TLSv1_2)
httpd.serve_forever()	