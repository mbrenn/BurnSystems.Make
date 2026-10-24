
# Step before: 

# The .csproj file in
#  BSMake and
#  BurnSystems.Make.BuildAgent
# must include the current version. 

# via 51_publish_dotnet_tool.ps1, you can upload the Packages to Nuget

cd ../src/Tool/BSMake

dotnet pack -c Release
# dotnet tool install --global --add-source ./nupkg BSMake

cd ../../../scripts

cd ../src/Tool/BurnSystems.Make.BuildAgent

dotnet pack -c Release

cd ../../../scripts
