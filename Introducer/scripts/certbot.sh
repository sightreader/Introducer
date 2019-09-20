certbot certonly --manual --preferred-challenges dns --server https://acme-v02.api.letsencrypt.org/directory -d '*.sightread.xyz'

# Generate .pfx for .NET Core
openssl pkcs12 -export -out certificate.pfx -inkey privkey.pem -in cert.pem -certfile chain.pem