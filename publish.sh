#!/bin/bash

rm ./publish.zip
cd Spune.Desktop
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained true
cd ..
dotnet run --project ./Tools/NSubsys/NSubsys.csproj -- ./Spune.Desktop/bin/Release/net9.0/win-x64/publish/Spune.Desktop.exe
cp -R ./MasterStories ./Spune.Desktop/bin/Release/net9.0/win-x64/publish/MasterStories
cd ./Spune.Desktop/bin/Release/net9.0/win-x64
zip -r ./../../../../../publish.zip ./publish
cd ./../../../../..
