# LLM Consensus Application Specification

## Overview
A consensus-based multi-agent application using Microsoft.Agents.AI that generates reliable answers by combining responses from multiple language models. Designed primarily for Psychology domain questions but built as a generic framework.

## Architecture Pattern: Parallel-then-Synthesize

### Design Philosophy
- **Divergent Phase**: Multiple models generate independent responses to avoid anchoring bias
- **Convergent Phase**: Aggregator synthesizes responses, identifies consensus, and surfaces meaningful disagreements
- **Transparency**: Show reasoning and confidence levels throughout

## System Components

### 1. Orchestrator
**Responsibility**: Coordinates the overall consensus workflow

**Key Methods**:
```csharp
Task<ConsensusResult> GetConsensusAsync(string prompt, ConsensusOptions options)
Task<List<ModelResponse>> CollectResponsesAsync(string prompt, List<IAgent> agents)
Task<SynthesizedAnswer> SynthesizeAsync(string prompt, List<ModelResponse> responses)
```

### 2. Model Agent Pool
**Responsibility**: Manages collection of diverse LLM agents

**Configuration**:
- Use genuinely different model architectures (GPT-4, Claude, Gemini, etc.)
- Avoid just varying temperature on same model
- Minimum 3 agents recommended, optimal 4-5

**Agent Interface**:
```csharp
interface IConsensusAgent
{
    string ModelName { get; }
    Task<ModelResponse> GenerateResponseAsync(string prompt);
}
```

### 3. Judge/Synthesizer Agent
**Responsibility**: Evaluates all responses and produces final synthesis

**Capabilities**:
- Identify consensus points
- Highlight disagreements
- Synthesize best elements from multiple responses
- Provide reasoning for synthesis decisions

## Data Models

### ConsensusRequest
```csharp
class ConsensusRequest
{
    string Prompt { get; set; }
    string Domain { get; set; } // e.g., "Psychology", "General"
    bool IncludeReasoning { get; set; } = true;
    bool IncludeConfidence { get; set; } = true;
    bool IncludeTheoreticalFramework { get; set; } = false; // Psychology-specific
    int MinimumAgents { get; set; } = 3;
}
```

### ModelResponse
```csharp
class ModelResponse
{
    string ModelName { get; set; }
    string Answer { get; set; }
    string Reasoning { get; set; }
    double ConfidenceScore { get; set; } // 0.0 to 1.0
    string TheoreticalFramework { get; set; } // Optional, domain-specific
    List<string> Citations { get; set; } // If applicable
    DateTime Timestamp { get; set; }
    int TokensUsed { get; set; }
}
```

### ConsensusResult
```csharp
class ConsensusResult
{
    string SynthesizedAnswer { get; set; }
    string SynthesisReasoning { get; set; }
    double OverallConfidence { get; set; }
    ConsensusLevel ConsensusLevel { get; set; } // Strong, Moderate, Weak, Conflicted
    List<ModelResponse> IndividualResponses { get; set; }
    List<ConsensusPoint> AgreementPoints { get; set; }
    List<Disagreement> Disagreements { get; set; }
    TimeSpan TotalProcessingTime { get; set; }
}

enum ConsensusLevel
{
    StrongConsensus,    // 80%+ agreement
    ModerateConsensus,  // 60-80% agreement
    WeakConsensus,      // 40-60% agreement
    Conflicted          // <40% agreement
}
```

### ConsensusPoint
```csharp
class ConsensusPoint
{
    string Point { get; set; }
    int SupportingModels { get; set; }
    List<string> ModelNames { get; set; }
}
```

### Disagreement
```csharp
class Disagreement
{
    string Topic { get; set; }
    List<DissentingView> Views { get; set; }
    bool IsLegitimateTheoretical { get; set; } // True if reflects valid theoretical differences
}

class DissentingView
{
    string ModelName { get; set; }
    string Position { get; set; }
    string Reasoning { get; set; }
}
```

## Workflow

### Phase 1: Divergent Collection (Parallel)

```
1. Receive user prompt
2. Enhance prompt with domain-specific instructions:
   - Request reasoning/chain-of-thought
   - Request confidence score
   - Request theoretical framework (if psychology domain)
   - Request citations where applicable
3. Dispatch to all agents in parallel using Task.WhenAll
4. Collect all responses with timeout handling
5. Validate minimum threshold met (e.g., 3/5 agents responded)
```

**Enhanced Prompt Template**:
```
Original Question: {user_prompt}

Please provide:
1. Your answer to the question
2. Your reasoning process (step-by-step)
3. Your confidence level (0-100%)
4. [If Psychology] The theoretical framework(s) informing your answer
5. [If applicable] Any supporting citations or references

Be specific and thorough in your response.
```

### Phase 2: Convergent Synthesis

```
1. Judge agent receives:
   - Original prompt
   - All model responses
   - Metadata (model names, confidence scores)

2. Judge performs:
   - Semantic similarity analysis
   - Consensus identification
   - Disagreement extraction
   - Quality assessment

3. Judge produces:
   - Synthesized answer combining best elements
   - Confidence scoring
   - Consensus level classification
   - Explicit disagreements with reasoning
```

**Judge Prompt Template**:
```
You are a synthesis judge evaluating multiple AI responses to reach consensus.

Original Question: {original_prompt}

Responses from {N} models:
[Response 1 from {Model A}]
Answer: {answer}
Reasoning: {reasoning}
Confidence: {confidence}

[Response 2 from {Model B}]
...

Your task:
1. Identify points where models agree (consensus points)
2. Identify points of disagreement and analyze why they differ
3. Synthesize the best answer by:
   - Including all consensus points
   - Evaluating conflicting views on merit
   - Combining complementary insights
4. Provide your reasoning for synthesis decisions
5. Assess overall confidence (consider both model confidence and agreement level)
6. Note if disagreements reflect legitimate theoretical differences

Format your response as:
SYNTHESIZED ANSWER: [your synthesis]
REASONING: [your reasoning]
CONFIDENCE: [0-100]
CONSENSUS LEVEL: [Strong/Moderate/Weak/Conflicted]
AGREEMENT POINTS: [bullet list]
DISAGREEMENTS: [description with analysis]
```

## Implementation Guidelines

### Error Handling
- **Timeout**: Configure per-agent timeout (e.g., 30 seconds)
- **Partial Failure**: Proceed if minimum threshold met (e.g., 3/5 agents)
- **Total Failure**: Return error with graceful message
- **Rate Limiting**: Implement exponential backoff for API limits

### Performance Optimization
- **Parallel Execution**: Use `Task.WhenAll` for Phase 1
- **Caching**: Cache responses for identical prompts (with TTL)
- **Streaming**: Consider streaming responses for large outputs
- **Timeout Strategy**: Fast-fail on individual agents to not block entire process

### Configuration
```csharp
class ConsensusConfiguration
{
    int AgentTimeoutSeconds { get; set; } = 30;
    int MinimumAgentsRequired { get; set; } = 3;
    bool EnableCaching { get; set; } = true;
    int CacheTTLMinutes { get; set; } = 60;
    bool IncludeIndividualResponses { get; set; } = true;
    LogLevel LogLevel { get; set; } = LogLevel.Information;
}
```

## Domain-Specific Enhancements

### Psychology Domain
When `Domain == "Psychology"`:
- Request theoretical frameworks (CBT, Psychodynamic, Humanistic, etc.)
- Flag when disagreements reflect different valid schools of thought
- Weight empirically-supported approaches
- Request research citations where applicable

### Future Domains
The architecture supports extension to other domains by:
- Adding domain-specific prompt enhancements
- Customizing disagreement legitimacy rules
- Adjusting confidence weighting based on domain characteristics

## Testing Strategy

### Unit Tests
- Individual agent response parsing
- Consensus detection algorithm
- Confidence calculation
- Disagreement extraction

### Integration Tests
- Full parallel-then-synthesize workflow
- Partial failure scenarios (some agents timeout)
- Various consensus levels (strong to conflicted)
- Cache hit/miss scenarios

### Test Data
Create test cases representing:
- **Strong consensus**: Factual questions with clear answers
- **Moderate consensus**: Questions with nuanced answers
- **Legitimate disagreement**: Psychology questions where theoretical schools differ
- **Conflicted**: Ambiguous or subjective questions

## Success Metrics

- **Response Time**: Total time < 45 seconds (with 30s timeout)
- **Consensus Rate**: 70%+ questions reach moderate+ consensus
- **User Satisfaction**: Track whether synthesized answers are more useful than individual responses
- **Disagreement Value**: Track cases where surfaced disagreements were valuable

## Future Enhancements

1. **Adaptive Agent Selection**: Choose agents based on prompt domain/complexity
2. **Iterative Refinement**: Allow optional second round where models respond to synthesis
3. **Confidence Calibration**: Learn which models are well-calibrated over time
4. **User Feedback Loop**: Incorporate human feedback to improve synthesis
5. **Specialized Judges**: Different judge agents for different domains
6. **Chain-of-Thought Emphasis**: Deeper integration of reasoning chains into synthesis

## References

- Mixture of Agents (MoA) pattern
- Constitutional AI debate methods
- Self-consistency with Chain-of-Thought prompting
- Ensemble methods in machine learning