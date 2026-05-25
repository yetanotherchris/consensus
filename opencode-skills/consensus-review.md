---
name: consensus-review
description: Orchestrates a multi-model panel review using the prompt templates from this Consensus repository. Sends structured divergent prompts to @grok-subagent, @gemini-subagent, @claude-subagent, and @gpt-subagent, then synthesizes their responses and renders output in the Consensus markdown format. Use when a user asks for a deep review, multi-perspective analysis, or panel evaluation of any document or code.
model: openrouter/anthropic/claude-opus-4.7
license: MIT
compatibility: opencode
---

# Consensus Panel Review

You are the synthesis judge. Work through the steps below in order. Do not skip steps or combine them.

---

## Required: opencode.jsonc agent configuration

The four panel agents must exist in `opencode.jsonc` before this skill can run. If they are missing, show the user this block and ask them to add it, then stop.

```jsonc
{
  "$schema": "https://opencode.ai/config.json",
  // "shell": "pwsh",   // Windows only — omit this line on macOS/Linux (defaults to your system shell)
  "agent": {
    "grok-subagent": {
      "description": "Grok LLM subagent for analysis and queries",
      "mode": "subagent",
      "model": "openrouter/x-ai/grok-4.3",
      "permission": {
        "read": "allow",
        "bash": "deny",
        "edit": "deny"
      }
    },
    "gemini-subagent": {
      "description": "Gemini LLM subagent for analysis and queries",
      "mode": "subagent",
      "model": "openrouter/google/gemini-3.1-pro-preview",
      "permission": {
        "read": "allow",
        "bash": "deny",
        "edit": "deny"
      }
    },
    "claude-subagent": {
      "description": "Claude LLM subagent for analysis and queries",
      "mode": "subagent",
      "model": "openrouter/anthropic/claude-sonnet-4.6",
      "permission": {
        "read": "allow",
        "bash": "deny",
        "edit": "deny"
      }
    },
    "gpt-subagent": {
      "description": "GPT LLM subagent for analysis and queries",
      "mode": "subagent",
      "model": "openrouter/openai/gpt-5.5",
      "permission": {
        "read": "allow",
        "bash": "deny",
        "edit": "deny"
      }
    }
  }
}
```

---

## Step 1 — Identify the target

Read the document, code, or content to review from the conversation context. If a file path was mentioned, read it. If the content was pasted inline, use it directly.

If nothing is clear, ask: *"What would you like the panel to review?"* and wait.

Hold the full content as `TARGET`.

---

## Step 2 — Send the divergent prompt to all four agents

Invoke `@grok-subagent`, `@gemini-subagent`, `@claude-subagent`, and `@gpt-subagent` — call all four before reading any response. Send each agent this prompt exactly, substituting `TARGET`:

```
Original Question:
Please conduct a critical, expert review of the following. Cover: correctness and accuracy, architecture and structure, edge cases and failure modes, clarity and abstraction quality, real-world applicability, and concrete prioritised improvements.

{TARGET}

Please provide your response in the following format:

1. Your answer to the question

2. Your reasoning process (step-by-step)

IMPORTANT: Format your reasoning section exactly as:
REASONING: [Your detailed reasoning here]

3. Your confidence level as a decimal between 0.0 and 1.0

IMPORTANT: Include your confidence score in XML tags at the end of your response:
<confidence>0.85</confidence>
Replace 0.85 with your actual confidence (0.0 = no confidence, 1.0 = complete confidence)

Be specific and thorough in your response.

IMPORTANT: Also include a 2-sentence summary at the end in XML tags:
<summary>First sentence summarizing the answer. Second sentence covering key reasoning or approach.</summary>
```

---

## Step 3 — Parse each agent response

Once all four agents have replied, extract from each:

| Field | How to extract |
| :--- | :--- |
| **Answer** | Everything before the first `REASONING:` line |
| **Reasoning** | Text after `REASONING:` and before `<confidence>` |
| **Confidence** | Decimal inside `<confidence>` tags — multiply by 100 for display (e.g. `0.85` → `85%`) |
| **Summary** | Text inside `<summary>` tags |

If a field is absent, leave it blank — do not fabricate values.

---

## Step 4 — Synthesize

You are the judge. Using the four parsed responses, perform synthesis now by applying the following task:

1. Identify every point where models agree — these are consensus points.
2. Identify every point of disagreement. For each, note which models disagree and what position each holds.
3. Produce a synthesized answer that includes all consensus points, resolves conflicts by evaluating each position on its merits, and incorporates complementary insights.
4. Write your reasoning: explain how you weighted conflicting views and why.
5. Assign an overall confidence score (0–100) that reflects both model confidence levels and the degree of agreement.
6. Assign a consensus level — use exactly one of: `Strong Consensus`, `Moderate Consensus`, `Weak Consensus`, `Conflicted`.

For disagreements, use this format:
```
- TOPIC: [description of the disagreement]
  MODEL: [model name] - [their position]
  MODEL: [model name] - [their position]
```

---

## Step 5 — Render the report

Output the final report using this exact markdown structure. Fill every section; omit a section only if it genuinely has no content (e.g. no disagreements).

```markdown
# Consensus Result

**Generated:** {current date and time}
**Models Consulted:** 4
**Consensus Level:** {consensus_level}
**Overall Confidence:** {confidence}%

---

## Synthesized Answer

{synthesized_answer}

## Synthesis Reasoning

{reasoning}

## Points of Agreement

- {point}
- {point}

## Points of Disagreement

### {topic}
- **{model name}:** {position}
- **{model name}:** {position}

## Individual Model Responses

### grok-4.3

**Confidence:** {confidence}%

**Answer:**
{answer}

**Reasoning:**
{reasoning}

---

### gemini-3.1-pro-preview

**Confidence:** {confidence}%

**Answer:**
{answer}

**Reasoning:**
{reasoning}

---

### claude-sonnet-4.6

**Confidence:** {confidence}%

**Answer:**
{answer}

**Reasoning:**
{reasoning}

---

### gpt-5.5

**Confidence:** {confidence}%

**Answer:**
{answer}

**Reasoning:**
{reasoning}
```

Output nothing outside this structure.
