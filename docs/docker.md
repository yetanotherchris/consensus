# Docker Web Deployment Guide

This guide covers building and deploying the combined Consensus Web + API Docker image.

## Overview

The `Dockerfile.web` creates a Docker image that includes:
- React frontend (built with Vite)
- ASP.NET Core Web API backend
- The API serves the frontend from its `wwwroot` directory

## Quick Start

### Local Build and Run

```bash
# Build the image locally
docker build -f Dockerfile.web -t consensus-web:latest .

# Run the container
docker run -d \
  -p 8080:8080 \
  -e Consensus__ApiEndpoint="https://openrouter.ai/api/v1" \
  -e Consensus__ApiKey="your-api-key-here" \
  -v $(pwd)/output:/app/output \
  --name consensus-web \
  consensus-web:latest

# View logs
docker logs -f consensus-web

# Stop and remove
docker stop consensus-web
docker rm consensus-web
```

### Using PowerShell (Windows)

```powershell
# Build
docker build -f Dockerfile.web -t consensus-web:latest .

# Run
docker run -d `
  -p 8080:8080 `
  -e Consensus__ApiEndpoint="https://openrouter.ai/api/v1" `
  -e Consensus__ApiKey="your-api-key-here" `
  -v "${PWD}/output:/app/output" `
  --name consensus-web `
  consensus-web:latest
```

## Environment Variables

### Required

- `Consensus__ApiEndpoint` - The AI API endpoint (e.g., `https://openrouter.ai/api/v1`)
- `Consensus__ApiKey` - Your API key for the endpoint

### Optional

- `Consensus__Domain` - Domain context for queries (default: `General`)
- `Consensus__AgentTimeoutSeconds` - Timeout for AI agent responses (default: `120`)
- `Consensus__IncludeIndividualResponses` - Include individual model responses (default: `true`)
- `OutputDirectory` - Directory for output files (default: `/app/output`)
- `Consensus__Models` - JSON array of models to query (can also be in appsettings.json)

## GitHub Container Registry

### Automated Publishing

The Docker image is automatically built and published to GitHub Container Registry (GHCR) when you push a version tag:

```bash
# Create and push a version tag
git tag v1.0.1
git push origin v1.0.1
```

This triggers the GitHub Actions workflow which:
1. Builds the Docker image
2. Publishes to `ghcr.io/<your-username>/consensus`
3. Tags it with both the version number and `latest`

### Pulling from GHCR

```bash
# Pull a specific version
docker pull ghcr.io/<your-username>/consensus:1.0.1

# Pull the latest version
docker pull ghcr.io/<your-username>/consensus:latest

# Run from GHCR
docker run -d \
  -p 8080:8080 \
  -e Consensus__ApiEndpoint="https://openrouter.ai/api/v1" \
  -e Consensus__ApiKey="your-api-key-here" \
  ghcr.io/<your-username>/consensus:latest
```

### Authentication

To pull private images from GHCR, you need to authenticate:

```bash
# Create a personal access token (PAT) with read:packages scope
# Then login to GHCR
echo $GITHUB_TOKEN | docker login ghcr.io -u USERNAME --password-stdin
```

## Docker Compose

Create a `docker-compose.web.yml` file:

```yaml
version: '3.8'

services:
  consensus-web:
    image: ghcr.io/<your-username>/consensus:latest
    ports:
      - "8080:8080"
    environment:
      - Consensus__ApiEndpoint=https://openrouter.ai/api/v1
      - Consensus__ApiKey=${CONSENSUS_API_KEY}
      - Consensus__Domain=General
      - Consensus__AgentTimeoutSeconds=120
      - Consensus__IncludeIndividualResponses=true
    volumes:
      - ./output:/app/output
    restart: unless-stopped
```

Run with:

```bash
# Set your API key
export CONSENSUS_API_KEY="your-api-key-here"

# Start
docker-compose -f docker-compose.web.yml up -d

# Stop
docker-compose -f docker-compose.web.yml down
```

## Accessing the Application

Once the container is running, access the application at:

- Web Interface: `http://localhost:8080`
- API Endpoints: `http://localhost:8080/api/*`

## Volume Mounts

The `/app/output` directory contains:
- `/app/output/logs/` - Consensus run logs
- `/app/output/responses/` - Markdown and HTML response files

Mount this directory to persist outputs:

```bash
-v $(pwd)/output:/app/output
```

## Troubleshooting

### Check container logs

```bash
docker logs consensus-web
```

### Exec into the container

```bash
docker exec -it consensus-web /bin/bash
```

### Verify environment variables

```bash
docker exec consensus-web env | grep Consensus
```

### Check output files

```bash
docker exec consensus-web ls -la /app/output/logs
docker exec consensus-web ls -la /app/output/responses
```

## CI/CD Workflow Details

The GitHub Actions workflow (`.github/workflows/docker-publish.yml`) runs on version tags and:

1. Checks out the repository
2. Sets up Docker Buildx for multi-platform builds
3. Logs in to GHCR using `GITHUB_TOKEN`
4. Extracts version from git tag (e.g., `v1.0.1` → `1.0.1`)
5. Builds and pushes the image with two tags:
   - Version tag: `ghcr.io/<repo>:1.0.1`
   - Latest tag: `ghcr.io/<repo>:latest`
6. Uses GitHub Actions cache for faster builds

### Required Permissions

The workflow needs these permissions (already configured):
- `contents: read` - To checkout the repository
- `packages: write` - To push to GHCR

### Making Images Public

By default, GHCR images are private. To make them public:

1. Go to your GitHub profile → Packages
2. Select the `consensus` package
3. Click "Package settings"
4. Scroll to "Danger Zone"
5. Click "Change visibility" → "Public"

## Production Deployment

For production deployments, consider:

1. **Use specific version tags** instead of `latest`
2. **Set resource limits**:
   ```bash
   docker run --memory="2g" --cpus="2" ...
   ```
3. **Use secrets management** for API keys
4. **Enable health checks**
5. **Use a reverse proxy** (nginx, Traefik) for HTTPS
6. **Monitor logs** and metrics

## Example Production Deployment

```bash
# Pull specific version
docker pull ghcr.io/<your-username>/consensus:1.0.1

# Run with resource limits and restart policy
docker run -d \
  --name consensus-web \
  --memory="2g" \
  --cpus="2" \
  -p 8080:8080 \
  -e Consensus__ApiEndpoint="${API_ENDPOINT}" \
  -e Consensus__ApiKey="${API_KEY}" \
  -v /var/consensus/output:/app/output \
  --restart=unless-stopped \
  --log-driver json-file \
  --log-opt max-size=10m \
  --log-opt max-file=3 \
  ghcr.io/<your-username>/consensus:1.0.1
```

## Next Steps

1. Push a version tag to trigger the first build: `git tag v1.0.0 && git push origin v1.0.0`
2. Wait for the GitHub Actions workflow to complete
3. Pull and run the image from GHCR
4. Configure your production environment
