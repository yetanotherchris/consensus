---
name: consensus-review
description: Runs a multi-model panel review using the Consensus.Console application in this repository. Queries Grok, Gemini, Claude, and GPT in parallel, synthesizes their critiques via a judge model, and returns a single markdown report. Use when a user asks for a deep review, multi-perspective analysis, or panel evaluation of any document or code.
model: openrouter/anthropic/claude-opus-4.7
license: MIT
compatibility: opencode
---

# Consensus Panel Review

## Overview
This skill replaces the manual `@agent` orchestration pattern with a single CLI call to `Consensus.Console` — the multi-model orchestration application in this repository. The CLI queries all four panel models **in parallel**, applies retry and quorum logic, and hands the collected responses to a synthesis judge that produces a structured markdown report with consensus levels, confidence scores, and per-model breakdowns.

The OpenCode subagent pattern (calling `@grok-subagent`, `@gemini-subagent`, etc.) is documented below for reference — it works in OpenCode and GitHub Copilot — but `Consensus.Console` supersedes it by handling parallelism, timeouts, quorum enforcement, and synthesis automatically.

---

## Part 1 — OpenCode Subagent Configuration (Reference)

If you are using this skill in OpenCode, the four panel agents must be defined in your `opencode.jsonc`. This is the correct schema:

```jsonc
{
  "$schema": "https://opencode.ai/config.json",
  "shell": "pwsh",
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

Key schema points:
- The top-level key is `"agent"` (not `"agents"`)
- Each entry requires `"mode": "subagent"` to be callable as `@grok-subagent` etc.
- Permissions use `"read" | "bash" | "edit"` with `"allow" | "deny"` values (not `tools: { write, bash }`)
- The file is `opencode.jsonc` (supports comments)

> **Note:** `Consensus.Console` calls these same model APIs directly via OpenRouter and does not rely on the `opencode.jsonc` agent entries. The configuration above is only needed if you want to call `@grok-subagent` etc. manually in other OpenCode sessions.

---

## Part 2 — Execution via Consensus.Console

### Prerequisites
The following environment variables must be set:
- `CONSENSUS_API_ENDPOINT` — set to `https://openrouter.ai/api/v1`
- `CONSENSUS_API_KEY` — your OpenRouter API key

If either is missing, the CLI will exit with a `SettingsException`. Inform the user and stop.

### Step 1 — Identify the target
Determine what is being reviewed from the conversation. It may be:
- A file path the user referenced — read it with the Read tool
- Code or text pasted directly into the conversation
- A description of what to evaluate

If nothing is clear, ask: "What would you like the panel to review?"

### Step 2 — Write the prompt file
Write the review prompt to `/tmp/consensus-review-prompt.txt`. Substitute `{{TARGET}}` with the actual content:

```
Conduct an exhaustive, critical review of the following. Be specific, direct, and unsparing.

Your review must cover:
1. Correctness — are all claims, logic, and implementations accurate?
2. Architecture and structure — is it well-organised and maintainable?
3. Edge cases and failure modes — what can go wrong, and under what conditions?
4. Clarity — is meaning unambiguous? Are abstractions well-chosen?
5. Real-world applicability — does it hold up in practice? What would break first?
6. Concrete improvements — list specific, prioritised action items with rationale.

Do not summarise. Do not hedge. Give your full, expert critique.

---
{{TARGET}}
---
```

### Step 3 — Write the models file
Write the following to `/tmp/consensus-review-models.txt` (one per line, no extras):

```
anthropic/claude-sonnet-4.6
x-ai/grok-4.3
google/gemini-3.1-pro-preview
openai/gpt-5.5
```

The first entry (`anthropic/claude-sonnet-4.6`) is used as the synthesis judge — it receives all four model responses and writes the final report. The remaining three are the panel reviewers. All four also submit their own critiques.

### Step 4 — Run the CLI
Execute from the repository root:

```bash
CONSENSUS_API_ENDPOINT="https://openrouter.ai/api/v1" \
CONSENSUS_API_KEY="$CONSENSUS_API_KEY" \
dotnet run --project src/Consensus.Console -- \
  --prompt-file /tmp/consensus-review-prompt.txt \
  --models-file /tmp/consensus-review-models.txt \
  --output-filenames-id panel-review
```

This takes 30–120 seconds. The CLI requires a quorum of at least `max(3, models * 2/3)` successful responses before proceeding to synthesis — if too many models time out, it will exit with an error rather than produce a low-quality result.

If the command exits non-zero, report the error output verbatim. Common causes:
- Missing environment variables
- An unrecognised model ID in the models file
- Quorum not met (too many model timeouts)

### Step 5 — Present the report
On success, read `output/responses/consensus-panel-review.md` and output its full contents. Do not paraphrase, truncate, or reformat — the template produces correctly structured output.

Then clean up:
```bash
rm -f /tmp/consensus-review-prompt.txt /tmp/consensus-review-models.txt
```

---

## What the Report Contains
The markdown produced by `Consensus.Console` includes:

| Section | Content |
| :--- | :--- |
| **Consensus Level** | Strong / Moderate / Weak / Conflicted, plus a 0–100% confidence score |
| **Synthesized Answer** | The judge model's definitive synthesis across all panel responses |
| **Synthesis Reasoning** | How the judge weighted and reconciled conflicting views |
| **Points of Agreement** | Topics where all models converged |
| **Points of Disagreement** | Topics where models diverged, with each model's specific position |
| **Individual Model Responses** | Full critiques from each of the four models with per-model confidence scores |
