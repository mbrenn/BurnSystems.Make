
./50_create_dotnet_tool.ps1

cd ../src/Tool/BSMake

# nuget setapikey {key} --source https://api.nuget.org/v3/index.json <-- currently not working under linux

dotnet nuget push nupkg/BSMake.1.0.0.nupkg --source https://api.nuget.org/v3/index.json --api-key {key}

cd ../../../scripts