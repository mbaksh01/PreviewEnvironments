FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PreviewEnvironments.API/PreviewEnvironments.API.csproj", "PreviewEnvironments.API/"]
COPY ["PreviewEnvironments.Application/PreviewEnvironments.Application.csproj", "PreviewEnvironments.Application/"]
RUN dotnet restore "PreviewEnvironments.API/PreviewEnvironments.API.csproj"
COPY . .
WORKDIR "/src/PreviewEnvironments.API"
RUN dotnet build "PreviewEnvironments.API.csproj" --no-restore -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PreviewEnvironments.API.csproj" --no-build -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PreviewEnvironments.API.dll"]
