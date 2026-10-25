
# Step before: 

# The .csproj file in
#  BSMake and
#  BurnSystems.Make.BuildAgent
# must include the current version. 

# via 51_publish_dotnet_tool.ps1, you can upload the Packages to Nuget

$ScriptDir = $PSScriptRoot

dotnet pack (Join-Path $ScriptDir "../src/Tool/BSMake/BSMake.csproj") -c Release
dotnet pack (Join-Path $ScriptDir "../src/Tool/BurnSystems.Make.BuildAgent/BurnSystems.Make.BuildAgent.csproj") -c Release
