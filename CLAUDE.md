# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Consensus is a .NET 9.0 application that queries multiple AI models with the same prompt and synthesizes a consensus answer. It supports both CLI and web API modes with a React frontend.

## Project Structure

The solution follows a clean architecture with SOLID principles:

- **Consensus.Core** - Core business logic and services (shared library)
- **Consensus.Console** - CLI application for standalone execution
- **Consensus.Api** - ASP.NET Core Web API with Quartz.NET job scheduling
- **Consensus.Web** - React + TypeScript + Vite frontend with Tailwind CSS
- **Consensus.RedisTest** - Redis integration testing utilities

### Key Components

#### Consensus.Core Architecture

- **ConsensusOrchestrator** - Main orchestration of the parallel-then-synthesize workflow
- **Services/** - Service layer implementing SOLID principles:
  - `IAgentService` / `AgentService` - AI agent initialization and queries
  - `IPromptBuilder` / `PromptBuilder` - Prompt construction for different scenarios
  - `ISynthesizerService` / `SynthesizerService` - Synthesizes responses from multiple models
  - `IMarkdownOutputService` / `MarkdownOutputService` - Markdown output generation
  - `IHtmlOutputService` / `HtmlOutputService` - HTML output generation
  - `IOutputWriter` / `FileOutputWriter` - File system output operations
- **Configuration/** - Configuration and settings management
- **Models/** - Domain models (ConsensusResult, ModelResponse, ConsensusLevel, etc.)
- **Logging/** - SimpleFileLogger for structured logging
- **Channels/** - ConsensusRunTracker for tracking consensus runs via channels
- **Templates/** - Go text templates (.tmpl files) for output formatting
- **DI/** - Dependency injection extension methods

All services use constructor dependency injection and are registered in `ServiceCollectionExtensions`.

#### Consensus.Api Architecture

- **Controllers/** - API endpoints for consensus requests
- **Jobs/** - Quartz.NET scheduled jobs
- **Services/** - API-specific services (log reading, output file reading)
- **Models/** - API request/response models

## Common Commands

### Building

```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build src/Consensus.Console/Consensus.Console.csproj
dotnet build src/Consensus.Api/Consensus.Api.csproj

# Build in Release mode
dotnet build -c Release
```

### Running

#### Console Application

```bash
# Run console app with arguments
dotnet run --project src/Consensus.Console -- --prompt-file prompt.txt --models-file models.txt

# With optional output ID
dotnet run --project src/Consensus.Console -- --prompt-file prompt.txt --models-file models.txt --output-filenames-id my-run-id
```

Required environment variables for console:
- `CONSENSUS_API_ENDPOINT` - API endpoint (e.g., https://openrouter.ai/api/v1)
- `CONSENSUS_API_KEY` - API key for the endpoint

#### Web API

```bash
# Run API (includes frontend)
dotnet run --project src/Consensus.Api
```

Required environment variables for API:
- `Consensus__ApiEndpoint` - API endpoint
- `Consensus__ApiKey` - API key
- `Consensus__RedisConnectionString` - Redis connection string (default: localhost:6379)

#### Web Frontend

```bash
cd src/Consensus.Web

# Install dependencies
npm install

# Development mode
npm run dev

# Development mode with auto-open browser
npm run dev:open

# Development mode accessible from network
npm run dev:host

# Build for production
npm run build

# Lint
npm run lint

# Preview production build
npm run preview
```

### Testing

```bash
# Run all tests (when test projects are added)
dotnet test

# Run specific test project
dotnet test src/Consensus.RedisTest/Consensus.RedisTest.csproj
```

### Docker

#### Console Application (Docker)

```bash
# Build
docker build -t consensus . && docker image prune -f

# Run (Linux/macOS)
docker run --rm \
    -v $(pwd):/app/data \
    -e CONSENSUS_API_ENDPOINT=https://openrouter.ai/api/v1 \
    -e CONSENSUS_API_KEY=your-key-here \
    -e PROMPT_FILE=/app/data/prompt.txt \
    -e MODELS_FILE=/app/data/models.txt \
    -e OUTPUT_FILENAMES_ID=custom-id \
    consensus

# Run (PowerShell)
docker run --rm `
    -v "${PWD}:/app/data" `
    -e CONSENSUS_API_ENDPOINT="https://openrouter.ai/api/v1" `
    -e CONSENSUS_API_KEY="your-key-here" `
    -e PROMPT_FILE="/app/data/prompt.txt" `
    -e MODELS_FILE="/app/data/models.txt" `
    -e OUTPUT_FILENAMES_ID="custom-id" `
    consensus
```

#### Full Stack (Docker Compose)

```bash
# Start services (API + Redis)
cd docker
docker-compose up -d

# Stop services
docker-compose down

# View logs
docker-compose logs -f
```

The API will be accessible at `http://localhost:5000` with Redis on port 6379.

## Development Guidelines

### Dependency Management

This solution uses **Central Package Management** via `Directory.Packages.props`. All package versions are defined centrally:

- To add a package: Add `<PackageVersion Include="PackageName" Version="x.y.z" />` to `Directory.Packages.props`
- Then reference it in `.csproj` files with just `<PackageReference Include="PackageName" />`

### SOLID Principles

This codebase strictly follows SOLID principles:

- **Single Responsibility** - Each class has one clear purpose
- **Open/Closed** - Services are open for extension via interfaces, closed for modification
- **Liskov Substitution** - All implementations can be substituted with their interfaces
- **Interface Segregation** - Focused, minimal interfaces
- **Dependency Inversion** - High-level modules depend on abstractions, not concrete implementations

When adding new functionality:
1. Define an interface in the appropriate namespace
2. Create an implementation
3. Register it in `ServiceCollectionExtensions.cs`
4. Inject it via constructor where needed

### Logging

The system uses a hybrid logging approach with channel-based async logging:

- **Console App**: Uses `SimpleFileLogger` for instance-based file logging with format `[LEVEL][DATETIME] Message`
- **API**: Uses `Microsoft.Extensions.Logging.ILogger<T>` for structured logging
- **Channel-Based Logging**: `ConsensusRunTracker` manages async logging via System.Threading.Channels
  - Producers (services) write log messages to an unbounded channel
  - Single background consumer writes to per-run log files asynchronously
  - `ConsensusFileLogger` handles thread-safe file writes with per-file semaphores
  - Implements `IDisposable` for graceful shutdown with 10-second timeout
  - Automatically cleans up semaphores to prevent memory leaks
- All logs from consensus runs are saved to `output/logs/consensus-{runId}.log`

**Key Implementation Details:**
- Channel is configured with `SingleReader = true`, `SingleWriter = false`
- Uses async file I/O (`File.AppendAllTextAsync`) to prevent thread pool starvation
- Validates all inputs (runId, message) before writing
- Falls back to `ILogger` if file writes fail to prevent message loss
- Log directory is configurable via DI registration

### Output Files

Consensus results are saved to:
- Markdown: `output/responses/consensus-{identifier}.md`
- HTML: `output/responses/consensus-{identifier}.html` (when generated)
- Logs: `output/logs/conversation-log-{identifier}.txt`

The `{identifier}` is either a timestamp (yyyyMMddHHmmss) or a custom ID passed via `--output-filenames-id`.

### Templates

Go text templates are stored in `src/Consensus.Core/Templates/` and are copied to output during build. Templates use Go template syntax and are rendered via the `go-text-template` NuGet package.

### Agent Communication

The system uses Microsoft Agents AI framework (`Microsoft.Agents.AI.OpenAI`) for AI model communication. Models are queried in parallel, then responses are synthesized by a judge model.

## Target Framework

All projects target **.NET 9.0** (`net9.0`).

## Redis Integration

The API uses Redis for:
- Caching consensus results
- Storing job state for Quartz.NET
- Connection string configured via `Consensus__RedisConnectionString`

## Frontend-Backend Integration

The API serves the React frontend from `wwwroot` in production. During development:
- Frontend runs on Vite dev server (typically port 5173)
- API runs on configured ASP.NET port
- CORS is configured to allow frontend requests
