#!/bin/bash

./publish.sh
launchctl setenv OLLAMA_ORIGINS "*"
brew services stop ollama
brew services start ollama
cd Spune.Server
dotnet run -c Release &
cd ..
cd Spune.Browser
dotnet run -c Release