dotnet publish -c Release -o bin/publish example/Example.Client
docker-compose up --build --force-recreate --abort-on-container-exit