# Generate certificate for program
openssl genrsa 2048 > private.pem
openssl req -x509 -new -key private.pem -out public.pem -subj "/C=US/ST=Hawaii/L=Honolulu/O=Sightreader/CN=localhost"
openssl pkcs12 -export -in public.pem -inkey private.pem -out cert.pfx

# Generate certificate for local HTTPS webserver
openssl req -new -x509 -keyout server.pem -out server.pem -days 365 -nodes -subj "/C=US/ST=Hawaii/L=Honolulu/O=Sightreader Web Server/CN=localhost"
# Add to store
certutil –addstore -enterprise –f "Root" server.pem