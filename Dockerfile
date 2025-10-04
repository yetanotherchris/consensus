# Multi-stage Dockerfile for Consensus.Console

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY consensus.sln .
COPY Directory.Packages.props .
COPY src/Consensus.Console/Consensus.Console.csproj src/Consensus.Console/
COPY src/Consensus.Core/Consensus.Core.csproj src/Consensus.Core/

# Restore dependencies
RUN dotnet restore src/Consensus.Console/Consensus.Console.csproj

# Copy all source code
COPY src/ src/

# Build and publish the application
RUN dotnet publish src/Consensus.Console/Consensus.Console.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
WORKDIR /app

# Copy the published application
COPY --from=build /app/publish .

# Create directories for input files and output
RUN mkdir -p /app/data /app/output/logs /app/output/responses

# Set working directory to /app/data for file operations
WORKDIR /app/data

# Environment variables (with defaults)
ENV PROMPT_FILE=/app/data/prompt.txt
ENV MODELS_FILE=/app/data/models.txt
ENV OUTPUT_FILENAMES_ID=""
ENV CONSENSUS_API_ENDPOINT=""
ENV CONSENSUS_API_KEY=""

# Create entrypoint script
RUN echo '#!/bin/sh\n\
ARGS="--prompt-file $PROMPT_FILE --models-file $MODELS_FILE"\n\
if [ -n "$OUTPUT_FILENAMES_ID" ]; then\n\
    ARGS="$ARGS --output-filenames-id $OUTPUT_FILENAMES_ID"\n\
fi\n\
\n\
exec /app/consensus $ARGS\n\
' > /app/entrypoint.sh && chmod +x /app/entrypoint.sh

ENTRYPOINT ["/app/entrypoint.sh"]

# Usage:
# Build: docker build -t consensus .
# Run:   docker run --rm \
#          -v $(pwd):/app/data \
#          -e CONSENSUS_API_ENDPOINT=https://openrouter.ai/api/v1 \
#          -e CONSENSUS_API_KEY=your-key-here \
#          -e PROMPT_FILE=/app/data/prompt.txt \
#          -e MODELS_FILE=/app/data/models.txt \
#          -e OUTPUT_FILENAMES_ID=custom-id \
#          consensus
