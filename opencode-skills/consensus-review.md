---
name: consensus-review
description: Orchestrates a multi-model panel review using the prompt templates from this Consensus repository. Discovers all agents prefixed with "consensus-subagent-" from opencode.jsonc, sends structured divergent prompts to each, synthesizes their responses, and renders output in the Consensus markdown format. Use when a user asks for a deep review, multi-perspective analysis, or panel evaluation of any document or code.
model: openrouter/anthropic/claude-opus-4.7
license: MIT
compatibility: opencode
---

# Consensus Panel Review

You are the synthesis judge. Work through the steps below in order. Do not skip steps or combine them.

---

## Step 1 — Discover panel agents

Search for `opencode.jsonc` (or `opencode.json`) in the following locations, in order. Read every file that exists and merge their `"agent"` sections — project-level entries override global ones.

| Scope | macOS / Linux | Windows |
| :--- | :--- | :--- |
| Project | `./opencode.jsonc` | `.\opencode.jsonc` |
| User (global) | `~/.config/opencode/opencode.jsonc` | `%ProgramData%\opencode\opencode.jsonc` or `~/config/opencode/opencode.jsonc` |

> **Windows note:** the docs list `%ProgramData%\opencode` but the file has also been observed at `~/config/opencode`. Check both locations if the first is empty.

Collect every key under `"agent"` whose name starts with `consensus-subagent-`. These are your panel members.

For each discovered agent, record:
- **Agent key** — the full key name (e.g. `consensus-subagent-grok`)
- **Model** — the value of its `"model"` field (e.g. `openrouter/x-ai/grok-4.3`) — used as the display name in the report

If no `consensus-subagent-*` agents are found, show the user this example configuration and ask them to add at least two agents, then stop:

```jsonc
{
  "$schema": "https://opencode.ai/config.json",
  // "shell": "pwsh",  // Windows only — omit on macOS/Linux
  "agent": {
    "consensus-subagent-grok": {
      "description": "Grok panel member for consensus reviews",
      "mode": "subagent",
      "model": "openrouter/x-ai/grok-4.3",
      "permission": {
        "read": "allow",
        "bash": "deny",
        "edit": "deny"
      }
    },
    "consensus-subagent-gemini": {
      "description": "Gemini panel member for consensus reviews",
      "mode": "subagent",
      "model": "openrouter/google/gemini-3.1-pro-preview",
      "permission": {
        "read": "allow",
        "bash": "deny",
        "edit": "deny"
      }
    },
    "consensus-subagent-claude": {
      "description": "Claude panel member for consensus reviews",
      "mode": "subagent",
      "model": "openrouter/anthropic/claude-sonnet-4.6",
      "permission": {
        "read": "allow",
        "bash": "deny",
        "edit": "deny"
      }
    },
    "consensus-subagent-gpt": {
      "description": "GPT panel member for consensus reviews",
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

The prefix `consensus-subagent-` is the only requirement. Users can add, remove, or swap any models freely.

---

## Step 2 — Identify the target

Read the document, code, or content to review from the conversation context. If a file path was mentioned, read it. If the content was pasted inline, use it directly.

If nothing is clear, ask: *"What would you like the panel to review?"* and wait.

Hold the full content as `TARGET`.

---

## Step 3 — Send the divergent prompt to all panel agents

Invoke every discovered `consensus-subagent-*` agent — call all of them before reading any response. Send each agent this prompt exactly, substituting `TARGET`:

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

## Step 4 — Parse each agent response

Once all agents have replied, extract from each:

| Field | How to extract |
| :--- | :--- |
| **Answer** | Everything before the first `REASONING:` line |
| **Reasoning** | Text after `REASONING:` and before `<confidence>` |
| **Confidence** | Decimal inside `<confidence>` tags — multiply by 100 for display (e.g. `0.85` → `85%`) |
| **Summary** | Text inside `<summary>` tags |

If a field is absent, leave it blank — do not fabricate values.

---

## Step 5 — Synthesize

You are the judge. Using all parsed responses, perform synthesis now:

1. Identify every point where models agree — these are consensus points.
2. Identify every point of disagreement. For each, note which models disagree and what position each holds.
3. Produce a synthesized answer that includes all consensus points, resolves conflicts by evaluating each position on its merits, and incorporates complementary insights.
4. Write your reasoning: explain how you weighted conflicting views and why.
5. Assign an overall confidence score (0–100) reflecting both model confidence levels and degree of agreement.
6. Assign a consensus level — use exactly one of: `Strong Consensus`, `Moderate Consensus`, `Weak Consensus`, `Conflicted`.

---

## Step 6 — Render the report

Output the final report using this structure. Use the actual model value from each agent's config as its display name. `{N}` is the number of panel agents discovered in Step 1.

```markdown
# Consensus Result

**Generated:** {current date and time}
**Models Consulted:** {N}
**Consensus Level:** {consensus_level}
**Overall Confidence:** {confidence}%

---

## Synthesized Answer

{synthesized_answer}

## Synthesis Reasoning

{reasoning}

## Points of Agreement

- {point}

## Points of Disagreement

### {topic}
- **{model}:** {position}

## Individual Model Responses

### {model value from agent config}

**Confidence:** {confidence}%

**Answer:**
{answer}

**Reasoning:**
{reasoning}

---

{repeat for each panel agent}
```

Output nothing outside this structure.
