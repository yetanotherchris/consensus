# AGENTS Instructions

This repository uses the .NET SDK. Prior to committing changes, run:

```bash
dotnet restore
dotnet build
dotnet test
```

Make sure the build and tests succeed before opening a pull request.

For a full example, see <https://agentsmd.net/#example>.

## Programmatic Checks for OpenAI Codex
- Run the `dotnet` commands above to ensure the project builds and tests pass.

## Project Structure for OpenAI Codex Navigation
- The solution file `Consensus.sln` references the project in `src/Consensus.Console`.
- Source code resides under `src/Consensus.Console/Consensus.Console`.

## Coding Conventions for OpenAI Codex
- Use four spaces for indentation in C# files.
- Follow standard `.NET` naming conventions for classes, methods, and variables.
