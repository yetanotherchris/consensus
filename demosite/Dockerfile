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

RUN apt-get update \
    && apt-get install -y --no-install-recommends nginx \
    && rm -rf /var/lib/apt/lists/*

COPY --from=backend-build /app/publish .
COPY demosite/nginx.conf /etc/nginx/nginx.conf
COPY demosite/start.sh /app/start.sh

RUN mkdir -p /app/output/logs /app/output/responses \
    && chmod +x /app/start.sh

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://127.0.0.1:8081 \
    Consensus__ApiEndpoint="https://openrouter.ai/api/v1" \
    Consensus__ApiKey="" \
    Consensus__Domain="General" \
    Consensus__AgentTimeoutSeconds="120" \
    Consensus__IncludeIndividualResponses="true" \
    Consensus__Models__0="nvidia/nemotron-3-ultra-550b-a55b:free" \
    Consensus__Models__1="openrouter/free" \
    Consensus__Models__2="nvidia/nemotron-3-super-120b-a12b:free" \
    Consensus__Models__3="qwen/qwen3.8-27b:free" \
    Consensus__Models__4="google/gemma-4-31b-it:free" \
    Consensus__Models__5="thinkingmachines/inkling:free" \
    Consensus__Models__6="inclusionai/ling-3.1-flash" \
    Consensus__Models__7="nvidia/nemotron-3-nano-omni-30b-a3b-reasoning:free" \
    Consensus__Models__8="google/gemma-4-26b-a4b-it:free" \
    Consensus__Models__9="thinkingmachines/inkling-small:free" \
    Consensus__Models__10="apodex/apodex-1.1-mini:free" \
    Consensus__Models__11="stealth/space-bunny-alpha" \
    Consensus__Models__12="poolside/laguna-s-2.1:free" \
    Consensus__Models__13="cohere/north-mini-code:free" \
    Consensus__Models__14="poolside/laguna-xs-2.1:free" \
    Consensus__Models__15="liquid/lfm-2.5-2.6b:free" \
    OutputDirectory="/app/output"

EXPOSE 8080

ENTRYPOINT ["/app/start.sh"]
