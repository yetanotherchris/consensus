# ConsensusAgent Architecture

This project follows **SOLID principles** with a clean, maintainable architecture.

## Project Structure

```
ConsensusAgent/
├── Program.cs                          # Entry point - CLI parsing & DI setup
├── ConsensusOrchestrator.cs           # Main orchestration logic
├── Configuration/
│   └── ConsensusConfiguration.cs      # Configuration settings
├── Logging/
│   └── SimpleLogger.cs                # Simple one-line logger: [LEVEL][DATETIME] Message
├── Services/
│   ├── IAgentService.cs               # Interface for AI agent management
│   ├── AgentService.cs                # AI agent initialization & queries
│   ├── IPromptBuilder.cs              # Interface for prompt construction
│   ├── PromptBuilder.cs               # Builds prompts for different scenarios
│   ├── IVotingService.cs              # Interface for voting logic
│   ├── VotingService.cs               # Conducts votes on model responses
│   ├── IOutputService.cs              # Interface for file output
│   └── OutputService.cs               # Handles file saving operations
├── DI/
│   └── ServiceCollectionExtensions.cs # Dependency injection setup
└── Utilities/
    └── TextHelper.cs                  # Text manipulation helpers
```

## SOLID Principles Applied

### Single Responsibility Principle (SRP)
Each class has one reason to change:
- **Program**: CLI argument parsing and application bootstrapping
- **ConsensusOrchestrator**: Orchestrates the consensus building workflow
- **AgentService**: Manages AI agents and executes queries
- **PromptBuilder**: Constructs prompts for different scenarios
- **VotingService**: Handles voting logic
- **OutputService**: Manages file output operations
- **SimpleLogger**: Provides logging functionality

### Open/Closed Principle (OCP)
- Services are open for extension (via interfaces) but closed for modification
- New prompt strategies can be added by implementing `IPromptBuilder`
- New output formats can be added by implementing `IOutputService`

### Liskov Substitution Principle (LSP)
- All implementations can be substituted with their interfaces
- Any `IAgentService` implementation will work with `ConsensusOrchestrator`

### Interface Segregation Principle (ISP)
- Each interface is focused and minimal
- Clients only depend on the methods they actually use
- No "fat interfaces" with unused methods

### Dependency Inversion Principle (DIP)
- High-level `ConsensusOrchestrator` depends on abstractions (`IAgentService`, `IPromptBuilder`, etc.)
- Low-level implementations depend on abstractions
- Dependencies are injected via constructor injection

## Dependency Injection

The application uses Microsoft's `IServiceCollection` for dependency injection:

```csharp
services.AddConsensusServices(config);
```

This registers all services with appropriate lifetimes:
- **Singleton**: Configuration, Logger (shared across application)
- **Transient**: Services (new instance per resolution)

## Logging Format

The `SimpleLogger` uses a simplified one-line format:

```
[INFO][2025-10-03 14:30:45] Initializing 3 agent contexts
[WARN][2025-10-03 14:31:12] Model gpt-4 timed out after 90s - skipping
[ERROR][2025-10-03 14:32:01] Vote failed | Exception: Network timeout
```

Format: `[LEVEL][DATETIME] Message`

## Usage

The refactored code maintains the same external interface:

```bash
dotnet run prompt.txt models.txt
```

## Testing

The architecture makes unit testing straightforward:

```csharp
// Mock any service interface
var mockAgentService = new Mock<IAgentService>();
mockAgentService.Setup(x => x.QueryModelAsync(...)).ReturnsAsync("response");

// Inject mocks into orchestrator
var orchestrator = new ConsensusOrchestrator(config, logger, mockAgentService.Object, ...);
```

## Benefits

1. **Maintainability**: Each class is small and focused
2. **Testability**: Easy to mock dependencies and unit test
3. **Extensibility**: New features can be added without modifying existing code
4. **Readability**: Clear separation of concerns
5. **Reusability**: Services can be used independently or in different contexts
