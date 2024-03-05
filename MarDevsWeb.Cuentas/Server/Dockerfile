#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MarDevsWeb.Cuentas/Server/MarDevsWeb.Cuentas.Server.csproj", "MarDevsWeb.Cuentas/Server/"]
COPY ["MarDevsWeb.Cuentas/Client/MarDevsWeb.Cuentas.Client.csproj", "MarDevsWeb.Cuentas/Client/"]
COPY ["MarDevsWeb.Cuentas/Shared/MarDevsWeb.Cuentas.Shared.csproj", "MarDevsWeb.Cuentas/Shared/"]
RUN dotnet restore "./MarDevsWeb.Cuentas/Server/./MarDevsWeb.Cuentas.Server.csproj"
COPY . .
WORKDIR "/src/MarDevsWeb.Cuentas/Server"
RUN dotnet build "./MarDevsWeb.Cuentas.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MarDevsWeb.Cuentas.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MarDevsWeb.Cuentas.Server.dll"]