FROM microsoft/aspnetcore-build:2.0 as dotnet-build
WORKDIR /build
COPY MassTransit.RabbitMq.Extensions.Hosting/MassTransit.RabbitMq.Extensions.Hosting.csproj MassTransit.RabbitMq.Extensions.Hosting/
COPY Example.Client/Example.Client.csproj Example.Client/
COPY MassTransit.RabbitMq.Extensions.Hosting.sln .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /dist Example.Client

FROM microsoft/aspnetcore:2.0
WORKDIR /app
COPY --from=dotnet-build /dist .
CMD [ "dotnet", "Example.Client.dll" ]