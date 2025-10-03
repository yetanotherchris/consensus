# ConsensusAgent - Quick Reference

## Class Hierarchy

```
Program.cs (Entry Point)
    ↓ creates
ConsensusConfiguration
    ↓ uses DI
ServiceCollectionExtensions
    ↓ registers all services
    ├── SimpleLogger (Singleton)
    ├── IAgentService → AgentService (Transient)
    ├── IPromptBuilder → PromptBuilder (Transient)
    ├── IVotingService → VotingService (Transient)
    ├── IOutputService → OutputService (Transient)
    └── ConsensusOrchestrator (Transient)
```

## Orchestrator Dependencies

```
ConsensusOrchestrator
    ├── ConsensusConfiguration (config data)
    ├── SimpleLogger (logging)
    ├── IAgentService (AI queries)
    ├── IPromptBuilder (prompt construction)
    ├── IVotingService (voting logic)
    └── IOutputService (file operations)
```

## Flow

```
1. Program.Main()
   ├── Parse CLI args
   ├── Validate files
   ├── Read environment variables
   ├── Create ConsensusConfiguration
   └── Setup DI container

2. ConsensusOrchestrator.BuildConsensusAsync()
   ├── AgentService.InitializeAgentsAsync()
   └── For each round:
       ├── AgentService.QueryModelAsync() (for each model)
       └── VotingService.ConductVoteAsync()
           └── PromptBuilder.BuildVotingPrompt()

3. Generate Final Consensus
   ├── PromptBuilder.BuildFinalConsensusPrompt()
   └── AgentService.QueryModelOneOffAsync()

4. Save Results
   └── OutputService.SaveConsensusAsync()
```

## Logging Format

```
[INFO][2025-10-03 14:30:45] Message here
[WARN][2025-10-03 14:31:12] Warning message
[ERROR][2025-10-03 14:32:01] Error message | Exception: Details
```

## Key Files by Responsibility

| Responsibility | Files |
|---------------|-------|
| **Entry Point** | `Program.cs` |
| **Orchestration** | `ConsensusOrchestrator.cs` |
| **Configuration** | `Configuration/ConsensusConfiguration.cs` |
| **Logging** | `Logging/SimpleLogger.cs` |
| **AI Agents** | `Services/IAgentService.cs`, `Services/AgentService.cs` |
| **Prompts** | `Services/IPromptBuilder.cs`, `Services/PromptBuilder.cs` |
| **Voting** | `Services/IVotingService.cs`, `Services/VotingService.cs` |
| **File I/O** | `Services/IOutputService.cs`, `Services/OutputService.cs` |
| **DI Setup** | `DI/ServiceCollectionExtensions.cs` |
| **Utilities** | `Utilities/TextHelper.cs` |
