FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY isale-api.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:2.1
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "isale-api.dll"]
