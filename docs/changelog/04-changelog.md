# Changelog 04 - ConsensusJob Execute Implementation

## Original Prompt

Can implement the API ConsensusJob Execute method

## Follow-up Questions and Answers

**Q1: Prompt Storage** - The current `ScheduleConsensusJobAsync` method doesn't store the prompt in the job data. How should we handle this?

**A1:** Modify `ScheduleConsensusJobAsync` to take the prompt as a parameter.

**Q2: Models Configuration** - The `GetConsensusAsync` method requires a `models` array parameter. How should we configure this?

**A2:** Hardcode the models for now as a string, using the models.txt file.

**Q3: Output Storage** - The consensus result needs to be saved. How should we handle this?

**A3:** The logic should already be done by the orchestrator - it saves to file right now using the run id you send it.

**Q4: Error Handling** - If the consensus process fails, how should we handle it?

**A4:** The orchestrator is quite error-safe, so it handles errors. The job should log the error and mark the job as finished, for now.

**Q5: Dependency Injection** - The ConsensusJob needs access to ConsensusOrchestrator and related services. How should we inject these?

**A5:** I think services.AddConsensus() already injects the dependencies.

## Changes Made

### 1. IJobScheduler.cs
- Modified `ScheduleConsensusJobAsync` signature to accept a `prompt` string parameter
- Updated XML documentation to include the new parameter

### 2. QuartzJobScheduler.cs
- Updated `ScheduleConsensusJobAsync` implementation to accept the `prompt` parameter
- Added prompt to the `JobDataMap` so it can be retrieved by the job

### 3. ConsensusController.cs
- Updated `StartJob` method to pass `request.Prompt` to `ScheduleConsensusJobAsync`

### 4. ConsensusJob.cs
- Added `ConsensusOrchestrator` dependency injection via constructor
- Hardcoded models array from models.txt:
  ```csharp
  "anthropic/claude-sonnet-4"
  "x-ai/grok-3"
  "qwen/qwen3-vl-235b-a22b-thinking"
  "alibaba/tongyi-deepresearch-30b-a3b"
  "google/gemini-2.5-pro"
  "openai/gpt-5"
  ```
- Implemented `Execute` method to:
  - Retrieve `runId` and `prompt` from job data map
  - Call `orchestrator.GetConsensusAsync(prompt, Models)`
  - Call `orchestrator.SaveConsensusAsync(result, runId)` to save outputs with the runId
  - Handle errors without rethrowing (logs error and marks job as finished)

### 5. ConsensusOrchestrator.cs
- Modified `SaveConsensusAsync` method to accept optional `id` parameter
- Passes the `id` through to both `MarkdownOutputService` and `HtmlOutputService`
- Maintains backward compatibility with existing Console application code

## How It Works

1. Client sends POST request to `/api/consensus/start` with a prompt
2. Controller generates a unique `runId` (GUID)
3. Controller schedules a Quartz job, passing both `runId` and `prompt`
4. Job is stored in Quartz's job data map with the prompt
5. After a 5-second delay, Quartz executes the `ConsensusJob.Execute` method
6. Job retrieves the prompt and runId from the job data
7. Job calls the orchestrator to build consensus across 6 hardcoded AI models
8. Orchestrator saves markdown and HTML outputs with filenames containing the runId
9. Files are saved to: `output/responses/consensus-{runId}.md` and `output/responses/output-{runId}.html`
10. Job marks itself as finished in the job data map

## Testing

Implementation was tested for compilation errors. All files compile successfully with no errors. The backward compatibility with the Console application was verified.
