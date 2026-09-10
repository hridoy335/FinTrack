# ===== 1) Restore & build =====
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first (better layer cache)
COPY FinTrackCore.slnx ./
COPY src/FinTrackCore.Domain/FinTrackCore.Domain.csproj src/FinTrackCore.Domain/
COPY src/FinTrackCore.Application/FinTrackCore.Application.csproj src/FinTrackCore.Application/
COPY src/FinTrackCore.Infrastructure/FinTrackCore.Infrastructure.csproj src/FinTrackCore.Infrastructure/
COPY src/FinTrackCore.Api/FinTrackCore.Api.csproj src/FinTrackCore.Api/

RUN dotnet restore src/FinTrackCore.Api/FinTrackCore.Api.csproj

# Copy the rest of the source
COPY src/ src/

RUN dotnet publish src/FinTrackCore.Api/FinTrackCore.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ===== 2) Runtime image (smaller) =====
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# API listens on 8080 inside the container
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FinTrackCore.Api.dll"]