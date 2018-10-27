FROM microsoft/dotnet:2.1-runtime
WORKDIR /app

COPY ./bin/publish .
EXPOSE 80
ENTRYPOINT [ "dotnet", "Example.Client.dll" ]