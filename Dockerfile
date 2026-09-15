# ---------- 1-bosqich: build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["SandboxApi.csproj", "./"]
RUN dotnet restore "SandboxApi.csproj"

COPY . .
RUN dotnet publish "./SandboxApi.csproj" \
    --no-restore \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false 

# ---------- 2-bosqich: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

USER $APP_UID
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SandboxApi.dll"]