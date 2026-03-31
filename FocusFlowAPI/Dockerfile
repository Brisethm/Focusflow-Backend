# syntax=docker/dockerfile:1

# Etapa de construcción
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /source

# Copiar solo los archivos de proyecto y restaurar dependencias
COPY *.csproj .
RUN dotnet restore

# Copiar el resto del código
COPY . .

# Publicar la aplicación en modo Release
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish -c Release -o /app/publish

# Etapa final: imagen ligera para producción
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Copiar binarios publicados desde la etapa de build
COPY --from=build /app/publish .

# Crear y usar un usuario no privilegiado
RUN adduser --disabled-password --home /app --gecos '' appuser
USER appuser

# Definir el punto de entrada
ENTRYPOINT ["dotnet", "FocusFlowAPI.dll"]
