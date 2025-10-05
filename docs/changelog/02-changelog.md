# Changelog 02 - React Frontend Implementation

## Original Request
Create a React project inside src using dotnet new, with an app that will call the API to schedule a job, poll the API for status, poll the API for logs. The frontend should resemble ChatGPT/Grok/Ollama UI with a prompt textbox and send button. Once the prompt is sent, logs should be displayed in the UI and polled every 10 seconds. Once the job is complete, HTML should be displayed in a textbox and logs hidden but viewable via a "view logs" button. The HTML should be downloadable as a file.

## Follow-up Questions and Answers
1. **Run ID Generation**: Use UUID as the ID - implemented
2. **Prompt Parameter**: Add prompt as POST body parameter - implemented
3. **React Project Type**: Use Vite for Docker compatibility - implemented
4. **UI Library**: Use Material-UI - implemented
5. **API Base URL**: Make it configurable - implemented via environment variables
6. **HTML Display**: Render HTML in iframe - implemented

## Changes Made

### API Changes

#### New Files
- `src/Consensus.Api/Models/PromptRequest.cs` - Request model for prompt submission

#### Modified Files
- `src/Consensus.Api/Controllers/ConsensusController.cs`
  - Changed `POST /{runId}/start` to `POST /start`
  - Now accepts `PromptRequest` in request body
  - Auto-generates UUID for runId
  - Returns job status with generated runId

- `src/Consensus.Api/Program.cs`
  - Added CORS configuration with configurable allowed origins
  - Added static file serving for React frontend
  - Added fallback routing to support React client-side routing
  - Configured to serve frontend from wwwroot in production

- `src/Consensus.Api/appsettings.json`
  - Added `Cors.AllowedOrigins` configuration

### Frontend Implementation

#### Project Structure
```
src/Consensus.Web/
├── src/
│   ├── components/
│   │   ├── PromptInput.tsx      # ChatGPT-like input with send button
│   │   ├── LogViewer.tsx        # Real-time log display
│   │   └── ResultViewer.tsx     # HTML iframe + download + log toggle
│   ├── services/
│   │   └── api.ts               # API service layer
│   ├── types/
│   │   └── api.ts               # TypeScript type definitions
│   └── App.tsx                  # Main application with polling logic
├── .env                         # Development environment config
├── .env.example                 # Environment config template
├── .env.production              # Production environment config
└── vite.config.ts               # Vite configuration with proxy
```

#### Key Features
- **PromptInput Component**: Material-UI styled input with multiline support and send icon button
- **LogViewer Component**: Dark-themed console-like log display with timestamps
- **ResultViewer Component**: 
  - HTML rendered in iframe
  - Collapsible log viewer
  - Download button for HTML export
- **Polling Mechanism**: 
  - Job status polling every 10 seconds
  - Log polling every 10 seconds during job execution
  - Automatic cleanup on unmount
- **API Service**: 
  - Configurable base URL via environment variables
  - Automatic origin detection (production vs development)
  - Full TypeScript type safety

### Docker Configuration

#### New Files
- `Dockerfile.api` - Multi-stage build for API + React frontend
  - Stage 1: Build React frontend with Node
  - Stage 2: Build .NET API
  - Stage 3: Runtime with both components
- `docker/.env.example` - Docker environment template

#### Modified Files
- `docker/docker-compose.yml`
  - Added API service with React frontend
  - Configured networking between services
  - Added environment variable support
  - Exposed API on port 5000

### Development Tools

#### New Files
- `run-dev.ps1` - PowerShell script to run both API and frontend simultaneously
- `RUNNING.md` - Comprehensive guide for running the application in various modes

## Technical Details

### Architecture
- **Frontend**: React 18 + TypeScript + Material-UI + Vite
- **API Communication**: RESTful API with JSON
- **Polling Strategy**: useEffect hooks with interval cleanup
- **State Management**: React useState for local component state
- **Styling**: Material-UI's emotion-based CSS-in-JS

### API Flow
1. User submits prompt via PromptInput
2. Frontend generates request, sends POST to `/api/consensus/start`
3. API generates UUID, schedules job, returns status
4. Frontend polls status and logs every 10 seconds
5. When status becomes "Finished" (2), fetch and display HTML
6. User can download HTML or toggle log visibility

### Environment Configuration
- **Development**: 
  - Frontend runs on localhost:5173
  - API runs on localhost:5000
  - Vite proxy handles API requests
- **Production**: 
  - Frontend served from API's wwwroot
  - Same-origin API calls
  - Single port (5000) for everything

## Testing
- Frontend build verified successfully (418.56 kB bundle)
- API build verified successfully
- No TypeScript compilation errors
- No C# compilation errors

## Files Changed Summary
- **Created**: 13 files (React components, services, types, Docker configs, scripts)
- **Modified**: 6 files (API controller, Program.cs, appsettings.json, vite.config.ts, docker-compose.yml)

## How to Use

### Development
```powershell
.\run-dev.ps1
```

### Docker
```powershell
cd docker
cp .env.example .env
# Edit .env with your API keys
docker-compose up -d
```

Access at http://localhost:5000 (Docker) or http://localhost:5173 (Dev)
