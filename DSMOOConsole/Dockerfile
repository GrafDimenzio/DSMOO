FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base

USER root
RUN mkdir /dsmoo && chmod 777 /dsmoo

USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DSMOOConsole/DSMOOConsole.csproj", "DSMOOConsole/"]
COPY ["DSMOOServer/DSMOOServer.csproj", "DSMOOServer/"]
COPY ["DSMOOFramework/DSMOOFramework.csproj", "DSMOOFramework/"]
RUN dotnet restore "DSMOOConsole/DSMOOConsole.csproj"
COPY . .
WORKDIR "/src/DSMOOConsole"
RUN dotnet build "./DSMOOConsole.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DSMOOConsole.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DSMOOConsole.dll"]