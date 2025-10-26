# Docker Deployment Guide

This guide covers building and running the Consensus application with Docker.

## Overview

The default `Dockerfile` builds a production-ready image that includes:
- React frontend (built with Vite)
- ASP.NET Core Web API backend
- The API serves the frontend from `wwwroot` on port 8080

## Quick Start

### Build Locally

```bash
docker build -t consensus .
```

### Run with Required Environment Variables

```bash
# Linux/macOS
docker run -d \
  -p 8080:8080 \
  -e Consensus__ApiEndpoint="https://openrouter.ai/api/v1" \
  -e Consensus__ApiKey="your-api-key-here" \
  -v $(pwd)/output:/app/output \
  --name consensus \
  consensus

# PowerShell (Windows)
docker run -d `
  -p 8080:8080 `
  -e Consensus__ApiEndpoint="https://openrouter.ai/api/v1" `
  -e Consensus__ApiKey="your-api-key-here" `
  -v "${PWD}/output:/app/output" `
  --name consensus `
  consensus
```

### Access the Application

Once running, the application is accessible at:
- **Web Interface**: `http://localhost:8080`
- **API Endpoints**: `http://localhost:8080/api/*`

### Container Management

```bash
# View logs
docker logs -f consensus

# Stop container
docker stop consensus

# Start stopped container
docker start consensus

# Stop and remove container
docker stop consensus && docker rm consensus

# View running containers
docker ps

# Exec into container
docker exec -it consensus /bin/bash
```

## Environment Variables

### Required

- `Consensus__ApiEndpoint` - The AI API endpoint (e.g., `https://openrouter.ai/api/v1`)
- `Consensus__ApiKey` - Your API key for the endpoint

### Optional

- `Consensus__Domain` - Domain context for queries (default: `General`)
- `Consensus__AgentTimeoutSeconds` - Timeout for AI agent responses in seconds (default: `120`)
- `Consensus__IncludeIndividualResponses` - Include individual model responses (default: `true`)
- `OutputDirectory` - Directory for output files (default: `/app/output`)

### Passing Model Arrays

To specify which AI models to query, you can pass an array of models using indexed environment variables:

```bash
# Linux/macOS
docker run -d \
  -p 8080:8080 \
  -e Consensus__ApiEndpoint="https://openrouter.ai/api/v1" \
  -e Consensus__ApiKey="your-api-key" \
  -e Consensus__Models__0="openai/gpt-4" \
  -e Consensus__Models__1="anthropic/claude-3-opus" \
  -e Consensus__Models__2="google/gemini-pro" \
  -v $(pwd)/output:/app/output \
  consensus

# PowerShell (Windows)
docker run -d `
  -p 8080:8080 `
  -e Consensus__ApiEndpoint="https://openrouter.ai/api/v1" `
  -e Consensus__ApiKey="your-api-key" `
  -e Consensus__Models__0="openai/gpt-4" `
  -e Consensus__Models__1="anthropic/claude-3-opus" `
  -e Consensus__Models__2="google/gemini-pro" `
  -v "${PWD}/output:/app/output" `
  consensus
```

**Note**: ASP.NET Core configuration uses double underscores (`__`) as separators and array indices. Each model is specified with `Consensus__Models__<index>` where index starts at 0.

### Using an Environment File

For easier management, create a `.env` file:

```env
Consensus__ApiEndpoint=https://openrouter.ai/api/v1
Consensus__ApiKey=your-api-key-here
Consensus__Domain=General
Consensus__AgentTimeoutSeconds=120
Consensus__Models__0=openai/gpt-4
Consensus__Models__1=anthropic/claude-3-opus
Consensus__Models__2=google/gemini-pro
```

Then run with:

```bash
docker run -d \
  -p 8080:8080 \
  --env-file .env \
  -v $(pwd)/output:/app/output \
  --name consensus \
  consensus
```

## Volume Mounts

The `/app/output` directory contains generated files:
- `/app/output/logs/` - Consensus run logs
- `/app/output/responses/` - Markdown and HTML response files

Mount this directory to persist outputs on the host:

```bash
-v $(pwd)/output:/app/output
```

Or specify a different host directory:

```bash
-v /path/to/your/output:/app/output
```

## GitHub Container Registry

### Pulling from GHCR

Images are automatically published to GitHub Container Registry when you push version tags:

```bash
# Pull specific version
docker pull ghcr.io/<your-username>/consensus:1.0.0

# Pull latest
docker pull ghcr.io/<your-username>/consensus:latest
```

### Running from GHCR

```bash
docker run -d \
  -p 8080:8080 \
  -e Consensus__ApiEndpoint="https://openrouter.ai/api/v1" \
  -e Consensus__ApiKey="your-api-key" \
  -v $(pwd)/output:/app/output \
  ghcr.io/<your-username>/consensus:latest
```

### Authenticating with GHCR

For private images, authenticate first:

```bash
# Create a personal access token with read:packages scope
echo $GITHUB_TOKEN | docker login ghcr.io -u <your-username> --password-stdin
```

### Publishing New Versions

To trigger a new image build and publish:

```bash
# Create and push a version tag
git tag v1.0.1
git push origin v1.0.1
```

The GitHub Actions workflow will automatically:
1. Build the Docker image
2. Tag it with the version number and `latest`
3. Push to `ghcr.io/<your-username>/consensus`

## Docker Compose

For more complex setups, create a `docker-compose.yml` file:

```yaml
version: '3.8'

services:
  consensus:
    image: consensus:latest
    # Or use GHCR: ghcr.io/<your-username>/consensus:latest
    ports:
      - "8080:8080"
    environment:
      - Consensus__ApiEndpoint=https://openrouter.ai/api/v1
      - Consensus__ApiKey=${CONSENSUS_API_KEY}
      - Consensus__Domain=General
      - Consensus__Models__0=openai/gpt-4
      - Consensus__Models__1=anthropic/claude-3-opus
      - Consensus__Models__2=google/gemini-pro
    volumes:
      - ./output:/app/output
    restart: unless-stopped
```

Run with:

```bash
# Set API key
export CONSENSUS_API_KEY="your-api-key-here"

# Start
docker-compose up -d

# View logs
docker-compose logs -f

# Stop
docker-compose down
```

## Production Deployment

For production deployments, consider these best practices:

### Resource Limits

```bash
docker run -d \
  --memory="2g" \
  --cpus="2" \
  -p 8080:8080 \
  -e Consensus__ApiEndpoint="${API_ENDPOINT}" \
  -e Consensus__ApiKey="${API_KEY}" \
  -v /var/consensus/output:/app/output \
  --restart=unless-stopped \
  --name consensus \
  consensus
```

### Logging Configuration

```bash
docker run -d \
  --log-driver json-file \
  --log-opt max-size=10m \
  --log-opt max-file=3 \
  -p 8080:8080 \
  -e Consensus__ApiEndpoint="${API_ENDPOINT}" \
  -e Consensus__ApiKey="${API_KEY}" \
  consensus
```

### Health Checks

Add a health check to your deployment:

```yaml
services:
  consensus:
    image: consensus:latest
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/api/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
```

### Using Specific Version Tags

In production, always use specific version tags rather than `latest`:

```bash
docker pull ghcr.io/<your-username>/consensus:1.0.1
docker run -d \
  -p 8080:8080 \
  -e Consensus__ApiEndpoint="${API_ENDPOINT}" \
  -e Consensus__ApiKey="${API_KEY}" \
  ghcr.io/<your-username>/consensus:1.0.1
```

## Troubleshooting

### Check Container Logs

```bash
docker logs consensus
docker logs --tail 100 consensus
docker logs -f consensus  # Follow logs
```

### Verify Environment Variables

```bash
docker exec consensus env | grep Consensus
```

### Check Running Processes

```bash
docker exec consensus ps aux
```

### Inspect Container

```bash
docker inspect consensus
```

### Test Network Connectivity

```bash
docker exec consensus curl -I http://localhost:8080
```

### Check Output Files

```bash
docker exec consensus ls -la /app/output/logs
docker exec consensus ls -la /app/output/responses
```

### Rebuild Without Cache

If you encounter build issues:

```bash
docker build --no-cache -t consensus .
```

### Remove All Stopped Containers and Images

```bash
# Remove stopped containers
docker container prune

# Remove unused images
docker image prune

# Remove everything
docker system prune -a
```

## Common Issues

### Port Already in Use

If port 8080 is already in use, map to a different host port:

```bash
docker run -d -p 9000:8080 ... consensus
```

Then access at `http://localhost:9000`

### Permission Denied on Volume Mount

On Linux, you may need to adjust permissions:

```bash
mkdir -p output
chmod 777 output
docker run -v $(pwd)/output:/app/output ... consensus
```

### Container Exits Immediately

Check logs for errors:

```bash
docker logs consensus
```

Common causes:
- Missing required environment variables
- Invalid API endpoint or key
- Port conflict

## Advanced Configuration

### Custom appsettings.json

Mount a custom configuration file:

```bash
docker run -d \
  -p 8080:8080 \
  -v $(pwd)/appsettings.Production.json:/app/appsettings.Production.json \
  -e ASPNETCORE_ENVIRONMENT=Production \
  consensus
```

### Multiple Instances

Run multiple instances on different ports:

```bash
# Instance 1
docker run -d -p 8080:8080 --name consensus-1 consensus

# Instance 2
docker run -d -p 8081:8080 --name consensus-2 consensus

# Instance 3
docker run -d -p 8082:8080 --name consensus-3 consensus
```

### Network Configuration

Create a custom network for multiple containers:

```bash
docker network create consensus-net
docker run -d --network consensus-net --name consensus consensus
```
