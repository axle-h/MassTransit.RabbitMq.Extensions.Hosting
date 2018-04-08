dotnet publish -c Release -o bin/publish tests/Integration.Client
dotnet publish -c Release -o bin/publish tests/Integration.Server

docker-compose -f docker-compose.integration.yml up --build --force-recreate --abort-on-container-exit --exit-code-from newman
$result=$LastExitCode

docker-compose -f docker-compose.integration.yml down --remove-orphans

exit $result