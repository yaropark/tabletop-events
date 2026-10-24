# 1. Build the Angular app
FROM node:22-alpine AS web
WORKDIR /web
COPY web/package.json web/package-lock.json ./
RUN npm ci
COPY web/ ./
RUN npx ng build

# 2. Publish the API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api
WORKDIR /src
COPY nuget.config ./
COPY src/TabletopEvents.Api/TabletopEvents.Api.csproj src/TabletopEvents.Api/
RUN dotnet restore src/TabletopEvents.Api/TabletopEvents.Api.csproj
COPY src/ src/
RUN dotnet publish src/TabletopEvents.Api -c Release -o /app --no-restore

# 3. One container: the API serves the Angular build from wwwroot
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=api /app ./
COPY --from=web /web/dist/web/browser ./wwwroot
RUN mkdir -p /app/data && chown $APP_UID /app/data
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ConnectionStrings__Default="Data Source=/app/data/tabletop-events.db"
EXPOSE 8080
ENTRYPOINT ["dotnet", "TabletopEvents.Api.dll"]
