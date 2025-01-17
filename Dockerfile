# Usar la imagen oficial de .NET para construir la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["InvertirOnlineApp.csproj", "./"]
RUN dotnet restore "./InvertirOnlineApp.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet publish -c Release -o /app

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "InvertirOnlineApp.dll"]
