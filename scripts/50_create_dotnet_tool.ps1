cd ../src/Tool/BSMake

dotnet pack -c Release
# dotnet tool install --global --add-source ./nupkg BSMake

cd ../../../scripts

cd ../src/Tool/BurnSystems.Make.BuildAgent

dotnet pack -c Release

cd ../../../scripts
