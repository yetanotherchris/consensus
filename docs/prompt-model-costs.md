# Example prompt

This prompt should have a disagreement (by Microsoft Phi) with the top 5 list, where it thinks Paracetamol is in the list.

```
What are the top 5 prescribed drugs in the UK? 
Factor in the typical length of time a single prescription lasts,  
e.g. 2 weeks supply. Factor this in with the number of prescriptions given.

Include information about drugs prescribed for mental health, and format using a numbered list.
```

**Another example**

```
Find research for links between paracetamol (acetaminophen) usage during pregnancy and autism. Decide which viewpoint has stronger evidence
```

# Model costs
## Cheap models

```
"anthropic/claude-haiku-4.5",
"microsoft/phi-4",
"x-ai/grok-3-mini",
"google/gemini-2.5-flash",
"deepseek/deepseek-chat-v3.1",
"qwen/qwen-2.5-72b-instruct"
```  

| Model | Input Cost | Output Cost | Total Cost |
|-------|-----------|-------------|------------|
| **Claude Haiku 4.5** | $0.00007 | $0.01000 | **$0.0101** |
| **Phi-4** | $0.000005 | $0.00028 | **$0.0003** |
| **Grok 3 Mini** | $0.00002 | $0.00100 | **$0.0010** |
| **Gemini 2.5 Flash** | $0.00002 | $0.00500 | **$0.0050** |
| **DeepSeek V3.1** | $0.00003 | $0.00178 | **$0.0018** |
| **Qwen 2.5 72B** | $0.00005 | $0.00250 | **$0.0026** |
| **TOTAL (All 6 Models)** | **$0.00019** | **$0.02056** | **$0.0208** |

### **Summary:**
- **Cost per query (all 6 models):** ~$0.021 (2.1 cents)
- **Cost for 100 queries:** ~$2.10
- **Cost for 1,000 queries:** ~$21.00
- **Cost for 10,000 queries:** ~$210.00

## Premium models

Here's the cost breakdown for these premium models:

```
"anthropic/claude-sonnet-4.5",
"x-ai/grok-4",
"deepseek/deepseek-chat-v3.1",
"microsoft/phi-4",
"google/gemini-2.5-pro",
"openai/gpt-5"
```

| Model | Input Cost | Output Cost | Total Cost |
|-------|-----------|-------------|------------|
| **Claude Sonnet 4.5** | $0.00021 | $0.03000 | **$0.0302** |
| **Grok 4** | $0.00021 | $0.03000 | **$0.0302** |
| **DeepSeek V3.1** | $0.00003 | $0.00178 | **$0.0018** |
| **Phi-4** | $0.000005 | $0.00028 | **$0.0003** |
| **Gemini 2.5 Pro** | $0.00009 | $0.02000 | **$0.0201** |
| **OpenAI GPT-5** | $0.00009 | $0.02000 | **$0.0201** |
| **TOTAL (All 6 Models)** | **$0.00063** | **$0.10206** | **$0.1027** |

### **Summary:**
- **Cost per query (all 6 models):** ~$0.10 (10 cents)
- **Cost for 100 queries:** ~$10.30
- **Cost for 1,000 queries:** ~$103.00
- **Cost for 10,000 queries:** ~$1,030.00
