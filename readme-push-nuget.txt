https://www.meziantou.net/how-to-debug-nuget-packages-using-sourcelink.htm

dotnet pack --configuration Release


dotnet nuget push "package.nupkg" --api-key "<Insert your API Key>" --source https://api.nuget.org/v3/index.json --force-english-output


OBS: para pacotes novos apenas o envio do arquivo .nupkg é suficiente.
     para adicionar o sourcelink em pacotes existente, enviar somente o arquivo .snupkg