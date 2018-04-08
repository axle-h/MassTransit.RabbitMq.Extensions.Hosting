dotnet publish -c Release -o bin/publish example/Example.Client
docker-compose -f docker-compose.example.yml up --build --force-recreate --abort-on-container-exit