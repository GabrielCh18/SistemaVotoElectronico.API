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

# ⚠️ COPIAMOS TODOS LOS PROYECTOS NECESARIOS
# Esto es vital para que el MVC pueda compilarse
COPY ["SistemaVotoElectronico.MVC/SistemaVotoElectronico.MVC.csproj", "SistemaVotoElectronico.MVC/"]
COPY ["SistemaVoto.Modelos/SistemaVoto.Modelos.csproj", "SistemaVoto.Modelos/"]
COPY ["SistemaVotoElectronico.ApiConsumer/SistemaVotoElectronico.ApiConsumer.csproj", "SistemaVotoElectronico.ApiConsumer/"]

# Restauramos las dependencias del proyecto principal
RUN dotnet restore "./SistemaVotoElectronico.MVC/SistemaVotoElectronico.MVC.csproj"

# Copiamos el resto del código de la solución
COPY . .
WORKDIR "/src/SistemaVotoElectronico.MVC"
RUN dotnet build "./SistemaVotoElectronico.MVC.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ==========================================
# 3. PUBLICACIÓN
# ==========================================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SistemaVotoElectronico.MVC.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ==========================================
# 4. FINAL (Ejecución)
# ==========================================
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SistemaVotoElectronico.MVC.dll"]