# Builds either service from this one Dockerfile:
#   docker build --build-arg PROJECT=PoliticalNews.Worker -t politicalnews-worker .
#   docker build --build-arg PROJECT=PoliticalNews.Web    -t politicalnews-web    .
ARG PROJECT=PoliticalNews.Worker

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG PROJECT
WORKDIR /src

COPY Rudraa.InElection.RSSFeed.slnx ./
COPY src/PoliticalNews.Domain/PoliticalNews.Domain.csproj src/PoliticalNews.Domain/
COPY src/PoliticalNews.Application/PoliticalNews.Application.csproj src/PoliticalNews.Application/
COPY src/PoliticalNews.Infrastructure/PoliticalNews.Infrastructure.csproj src/PoliticalNews.Infrastructure/
COPY src/PoliticalNews.ServiceDefaults/PoliticalNews.ServiceDefaults.csproj src/PoliticalNews.ServiceDefaults/
COPY src/PoliticalNews.Worker/PoliticalNews.Worker.csproj src/PoliticalNews.Worker/
COPY src/PoliticalNews.Web/PoliticalNews.Web.csproj src/PoliticalNews.Web/
RUN dotnet restore "src/${PROJECT}/${PROJECT}.csproj"

COPY src/ src/
RUN dotnet publish "src/${PROJECT}/${PROJECT}.csproj" -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ARG PROJECT
ENV DOTNET_EnableDiagnostics=0
ENV PROJECT_DLL="${PROJECT}.dll"
ENTRYPOINT ["sh", "-c", "dotnet ${PROJECT_DLL}"]
