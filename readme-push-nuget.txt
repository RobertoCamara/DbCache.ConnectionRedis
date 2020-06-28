https://www.meziantou.net/how-to-debug-nuget-packages-using-sourcelink.htm

dotnet pack --configuration Release


dotnet nuget push "package.nupkg" --api-key "<Insert your API Key>" --source https://api.nuget.org/v3/index.json --force-english-output


OBS: apenas com o comando acima, tanto o arquivo .nupkg, quanto o .snupkg foram enviados 