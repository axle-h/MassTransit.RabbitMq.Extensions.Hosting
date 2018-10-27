FROM microsoft/dotnet:2.1.5-aspnetcore-runtime
WORKDIR /app

COPY ./bin/publish .
EXPOSE 80
ENTRYPOINT [ "dotnet", "Integration.Client.dll" ]