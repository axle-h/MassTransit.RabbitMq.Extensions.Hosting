FROM microsoft/aspnetcore:2.0.6
WORKDIR /app

COPY ./bin/publish .
EXPOSE 80
ENTRYPOINT [ "dotnet", "Integration.Client.dll" ]