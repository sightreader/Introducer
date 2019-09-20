#!/bin/bash
dotnet publish -r linux-x64 --configuration release --output bin/linux-dist
# warp-packer has a bug that doesn't set chmod on the extracted files
# warp-packer --arch linux-x64 --input_dir bin/Release/netcoreapp3.0/linux-x64/publish --exec Introducer --output bin/introducer 
scp bin/linux-dist/introducer root@relay.sightread.xyz:~/
rm -rf bin/linux-dist
#ssh root@relay.sightread.xyz "cd ~/ && chmod +x ./introducer && ./introducer; chmod 777 ~/.local/share/warp/packages/introducer"