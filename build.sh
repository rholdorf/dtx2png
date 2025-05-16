#!/bin/bash

echo "🛠️ Building and publishing..."
dotnet publish ./src/dtx2png.csproj -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./bin

