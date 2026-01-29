# ==========================================
# 1. IMAGEN BASE (Runtime)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ==========================================
# 2. COMPILACIÓN (SDK)
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# ⚠️ OJO: Esto asume que el "Build Context" en Render será la carpeta RAÍZ de todo el repo
COPY ["SistemaVotoElectronico.API/SistemaVotoElectronico.API.csproj", "SistemaVotoElectronico.API/"]
COPY ["SistemaVoto.Modelos/SistemaVoto.Modelos.csproj", "SistemaVoto.Modelos/"]

RUN dotnet restore "./SistemaVotoElectronico.API/SistemaVotoElectronico.API.csproj"

COPY . .
WORKDIR "/src/SistemaVotoElectronico.API"
RUN dotnet build "./SistemaVotoElectronico.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ==========================================
# 3. PUBLICACIÓN
# ==========================================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SistemaVotoElectronico.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ==========================================
# 4. FINAL (Ejecución)
# ==========================================
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SistemaVotoElectronico.API.dll"]