#!/bin/bash
python assets/web-server.py & "\Program Files (x86)\Google\Chrome\Application\chrome.exe" --ignore-certificate-errors "https://localhost/assets/client-test.html"