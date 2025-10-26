FROM node:22-alpine AS frontend-build
WORKDIR /src/web

COPY src/Consensus.Web/package*.json ./
RUN npm ci

COPY src/Consensus.Web/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /src

COPY consensus.sln .
COPY Directory.Packages.props .
COPY src/Consensus.Api/Consensus.Api.csproj src/Consensus.Api/
COPY src/Consensus.Core/Consensus.Core.csproj src/Consensus.Core/

RUN dotnet restore src/Consensus.Api/Consensus.Api.csproj

COPY src/Consensus.Core/ src/Consensus.Core/
COPY src/Consensus.Api/ src/Consensus.Api/
COPY --from=frontend-build /src/web/dist /src/src/Consensus.Api/wwwroot

RUN dotnet publish src/Consensus.Api/Consensus.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=backend-build /app/publish .

RUN mkdir -p /app/output/logs /app/output/responses

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    Consensus__ApiEndpoint="" \
    Consensus__ApiKey="" \
    Consensus__Domain="General" \
    Consensus__AgentTimeoutSeconds="120" \
    Consensus__IncludeIndividualResponses="true" \
    OutputDirectory="/app/output"

EXPOSE 8080

ENTRYPOINT ["dotnet", "Consensus.Api.dll"]
