# Builds the error-monitor React app (src/Web/ClientApp) separately, in a Node stage, so the .NET
# SDK stage below never needs Node installed. vite.config.ts's build.outDir ("../wwwroot") resolves
# relative to this stage's WORKDIR, landing at /src/Web/wwwroot - matching this same layout - so
# it can be copied straight into the .NET build context at the identical path below.
FROM node:22-alpine AS client-build
WORKDIR /src/Web/ClientApp
COPY src/Web/ClientApp/package.json src/Web/ClientApp/package-lock.json ./
RUN npm ci
COPY src/Web/ClientApp/ ./
RUN npm run build

# Builds the single Web service - it owns both the HTTP API and Hangfire job execution (RSS/API
# crawl schedules). There is no separate Worker project anymore (retired so this app fits a
# free-tier host with no paid background-worker service required), so this Dockerfile no longer
# needs the old --build-arg PROJECT=Worker|Web switch - it always builds Web.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/ServiceDefaults/ServiceDefaults.csproj src/ServiceDefaults/
COPY src/Web/Web.csproj src/Web/
RUN dotnet restore "src/Web/Web.csproj"

COPY src/ src/
# Overwrites the (empty/dev) wwwroot placeholder with the React app's actual production build -
# dotnet publish below then copies it into the publish output the same way it already does for
# any other content under wwwroot.
COPY --from=client-build /src/Web/wwwroot src/Web/wwwroot
RUN dotnet publish "src/Web/Web.csproj" -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV DOTNET_EnableDiagnostics=0
# The base image ships an unprivileged "app" user (APP_UID) but runs as root unless told
# otherwise - drop to it so the container never runs the actual process as root.
USER $APP_UID
ENTRYPOINT ["dotnet", "Web.dll"]
