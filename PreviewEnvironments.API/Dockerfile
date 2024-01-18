FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
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
RUN dotnet publish "PreviewEnvironments.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
# Add Docker's official GPG key:
RUN apt-get update
RUN apt-get install ca-certificates curl gnupg -y
RUN install -m 0755 -d /etc/apt/keyrings
RUN curl -fsSL https://download.docker.com/linux/debian/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
RUN chmod a+r /etc/apt/keyrings/docker.gpg
# Add the repository to Apt sources:
RUN echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/debian \
  $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \
  tee /etc/apt/sources.list.d/docker.list > /dev/null
RUN apt-get update
RUN apt-get install docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin -y
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PreviewEnvironments.API.dll"]
