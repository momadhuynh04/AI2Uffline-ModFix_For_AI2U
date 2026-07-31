# AI2U - Offline Mod & Infinite Loading Fix

## Overview

A comprehensive modification and fix for **AI2U: With You Til The End** on **Windows** (Linux untested).

This is **not** a traditional mod — it fixes the crack version of the game (Skidrow v0.7.12.2, latest as of 23/05/2026) that suffers from an **infinite loading screen** or connection errors due to the official servers being blocked.

If you've already tried adding `steam_api64.dll` and `steam_appid.txt` with no luck, and you just want to experience the gameplay — this is for you.

> **Note:** This fix has only been tested on Skidrow v0.7.12.2. Other versions are not guaranteed to work.
>
> **If you enjoy the game, please buy it — it's only $15.**

![gameplay](https://raw.githubusercontent.com/momadhuynh04/AI2Uffline-ModFix_For_AI2U/refs/heads/main/config/image.png)
---

## 📥 Download

> ### [⬇ Click Here to Download (Google Drive)](https://drive.google.com/drive/folders/1LO2G3EelAPuLpc1N7v6FIj9TRwp6BuW2?usp=sharing)
>
> Full game + fix v2.8 — content all file to run.
> please read file `ReadMe.txt` before playing.

---

## How It Works

The game's dialogue system is rerouted to your own custom LLM API instead of the dead official servers. You'll need:

- **An LLM** — either running locally or via an API key. The game sends requests (sometimes with base64-encoded images for TV/drawing scenarios), so a model with vision support is recommended.
- **A TTS provider** — the game also uses TTS. Azure TTS is recommended (free tier: 500,000 words/month). OpenAI TTS also works.

### LLM Recommendations

| Model | Type | Notes |
|-------|------|-------|
| **GPT-OSS 20B / Nemotron 3 Nano Omni** | Free (OpenRouter) | Recommended free option. Supports image input. |
| **DeepSeek V4 Flash** | Paid | Best response quality and speed. |
| **Qwen 2.5 3B Coder Instruct** | Local | Works but has parsing errors. |

> For local models, use a **coder-type** model rather than a roleplay model — the output must be valid JSON for the game to parse correctly.

### TTS Recommendations

- **Azure TTS** — Free tier provides 500,000 words/month. No need to configure a base URL; the code handles it. Find voices at [json2video.com](https://json2video.com). Recommended: `en-AU-CarlyNeural`.
- **OpenAI TTS** — Alternative if Azure is unavailable.

---

## Version History

### v2.8 (05/06/2026)

- Added more TTS configs: Piper TTS & Kokoro (local)
- Added prompt routing per character
- Added game context and tag injection
- Added chat history injection to request payload
- Added manual tag management (via `AI2U_Configurator.exe`)
- Fixed item purchasing in-game
- Fixed Dream OS level 1 (PC/WiFi password dialogue)
- Fixed Hub World invitation (chat in Hall now works)
- Unlocked all levels (legit this time)

### Notes

- **Local TTS:** GPT-SoVITS recommended for quality; Kokoro for lightweight use. Change voice via the training template.
- **Tag Management:** Saving/injection of tags was unreliable, so it's handled manually through `AI2U_Configurator.exe`. Add personality and hobby tags yourself.
- **Character Prompts:** I dug through game source for every character's prompt except the last one. You'll need to write that one yourself.

> Bug reports: Thanks to **Kora**.
>
> **This quest ends June 5th, 2026. No further updates.**

---

## Features

| Feature | Status |
|---------|--------|
| **Offline Play & Auth Bypass** | Working |
| **Custom LLM Integration (AI Proxy)** | Working |
| **Infinite Currency & Local Shop Save** | Working |
| **Unlocked NPCs (no Favor Meter)** | Working |
| **Hidden Chapters 2–4 Unlocked** | Working |
| **Item Lookup Fix** | Working |

- **Offline Play & Auth Bypass:** Completely removes Steam and PlayFab login. Fixes startup screen freeze and network connection crashes.
- **Custom LLM Integration:** Replaces the default Azure AI backend. Route NPC chat to OpenRouter, OpenAI, or any compatible API using your own key and model.
- **Infinite Currency & Local Save:** Grants 999,999,999 Tokens. Built a custom local save system (`ES3`) — cosmetics, items, and Persona tags are permanently saved locally.
- **Unlocked NPCs:** All NPCs are fully unlocked from the start — no Favor Meter grind required.
- **Hidden Chapters:** Chapters 2, 3, and 4 are forcibly unlocked (normally behind "Coming Soon" or "Unlock to reveal" in Early Access).
- **Item Lookup Fix:** Fixes case-sensitivity bugs — saved shop items load and equip correctly every launch.

---

## Installation
## Please download the game form the link, the bypass login phase some how have troubles to push, that is the must replace file for the game to work 

### Download

Full game with fix v2.8: [Google Drive](https://drive.google.com/drive/folders/1LO2G3EelAPuLpc1N7v6FIj9TRwp6BuW2?usp=sharing)

**Read the included `ReadMe.txt` before proceeding.**

### Prerequisites

**BepInEx v5.x** is required. Download the x64 version (tested with v5.4.21.0) from the [BepInEx GitHub](https://github.com/BepInEx/BepInEx).

Extract directly into your game root folder. You should see:
- A `BepInEx/` folder
- `doorstop_config.ini`
- `winhttp.dll`

If any are missing, re-extract.

### Steps

1. **Download and extract** this repo as a ZIP.

2. **Replace `Assembly-CSharp.dll`:**
   - Copy `Assembly-CSharp.dll` from the repo's `core/` folder.
   - Paste into `Your_Game_Root/AI2U - With you til the end_Data/Managed/`.
   - Overwrite when prompted.
   - *Recommendation: rename the original to `Assembly-CSharp.dll.bak` as a backup.*
   - *This DLL was modified with dnSpy to bypass Steam/PlayFab login paths.*

3. **Install the plugin:**
   - Copy `AI2U_Configurator.dll` from the repo's `core/` folder.
   - Paste into `Your_Game_Root/BepInEx/plugins/`.
   - *This reroutes the game to your custom AI configuration instead of the official server.*

4. **Configure your AI settings:**
   - Run the game once to generate `Config.json` (or `AI2U_Config.json`).
   - Close the game and open the JSON file in any text editor.
   - Fill in your API details (see Configuration section below).
   - Alternatively, copy the `Config.json` from the repo. If it fails, rename it to `AI2U_Config.json` (or vice versa).

5. **Launch and play.**

---

## Configuration

### `Config.json` / `AI2U_Config.json`

```json
{
  "base_url": "https://api.openai.com/v1/chat/completions",
  "api_key": "your-api-key-here",
  "model": "gpt-oss-20b",
  "system_prompt": "Use the system prompt from prompt.txt for best results",
  "post_history_prompt": "Same as system prompt",
  "temperature": 0.9,
  "top_p": 0.95,
  "top_k": 0,
  "max_tokens": 2050,
  "frequency_penalty": 0.05,
  "presence_penalty": 0.05,
  "tts_enable": true,
  "tts_provider": "azure",
  "tts_base_url": "",
  "tts_api_key": "your-tts-key-here",
  "tts_model": "en-AU-CarlyNeural",
  "tts_region": "your-azure-region"
}
```

| Field | Description |
|-------|-------------|
| `base_url` | API endpoint (e.g., `https://api.openai.com/v1/chat/completions`, OpenRouter URL, or local endpoint) |
| `api_key` | Your API key. Skip if running locally. |
| `model` | Model name (e.g., `gpt-oss-20b`, `deepseek-v4-flash`) |
| `system_prompt` | System prompt for the AI; use `prompt.txt` for best results |
| `post_history_prompt` | Same as system prompt |
| `temperature` | Creativity (0.0–2.0). Too high = gibberish. |
| `top_p` | Nucleus sampling; limits word choice pool |
| `top_k` | Top-K sampling |
| `max_tokens` | Maximum output tokens |
| `frequency_penalty` | Penalizes repeated tokens |
| `presence_penalty` | Penalizes repeated topics |
| `tts_enable` | Enable/disable TTS |
| `tts_provider` | `azure` or `openai` |
| `tts_base_url` | Leave empty for Azure |
| `tts_api_key` | TTS API key |
| `tts_model` | Voice model (e.g., `en-AU-CarlyNeural`) |
| `tts_region` | Azure service region (closer = faster) |

### GUI Configurator (Python)

A GUI tool is included for easier configuration. Requires **Python 3** (no virtual environment needed — only built-in libraries).

1. Place `Configuratorv1.2.py` and `Configurator.bat` into `Your_Game_Root/BepInEx/`.
2. Run `Configurator.bat`.

![GUI Screenshot 1](https://raw.githubusercontent.com/momadhuynh04/AI2Uffline-ModFix_For_AI2U/refs/heads/main/config/image_2026-06-05_145105812.png)
![GUI Screenshot 2](https://raw.githubusercontent.com/momadhuynh04/AI2Uffline-ModFix_For_AI2U/refs/heads/main/config/image_2026-06-05_151759788.png)

> AI parameters in the GUI may not work — adjust them directly in the JSON file above.

---

## AI2U Configurator — User Guide (v2.5)

The **AI2U Configurator** lets you redirect the game's dialogue system to your own API and fully customize each character's personality.

### Step 1: Launch

Double-click `AI2U_Configurator.exe` in the game root directory. No extra installations needed.

### Step 2: API Setup

- **Custom API URL:** Your API endpoint (OpenAI, OpenRouter, Ollama, LM Studio, etc.)
- **Custom API Key:** Your API key (`sk-...`). Stored locally and securely.

> ⚠️ **Never share your `AI2U_Config.json`** (in `BepInEx/config/`) without removing your API key — it contains sensitive access tokens.

### Step 3: Character Customization

Select a character from the left panel (Estelle, Eiona, etc.).

#### 3.1 In-Game System Prompt (Main Levels)

Sets context and behavior for the character during main gameplay. Modify freely to change personality (Yandere, Tsundere, etc.).

#### 3.2 Hub World System Prompt (Atrium / Waiting Room)

The Hub World **requires strict JSON output** so the game engine can parse and control animations. Paste this formatting block and add your custom lore/context above it:

```text
All replies should be strictly using JSON Format!
As an NPC in a video game, reply with JSON code to reflect your current state.

Input format:
(npc_trust_level, npc_location, npc_action, player_location, player_action, npc_inventory, story_guide, sentence_from_player)

Output format:
{
  "npc_action": "standing",
  "npc_body_animation": "idle",
  "npc_target_location": "player_location",
  "npc_face_expression": "smile",
  "angry_level": "normal",
  "favorability_change": "neutral",
  "giving_to_player": "none",
  "npc_reply_to_player": "Your dialogue goes here"
}

Allowed values:
- npc_action: other, standing, sitting, sitting_down, walking, hugging, cooking, playing_games, following_player
- npc_body_animation: idle, chill_idle, shy, stretch, crying, talk, dance, troublesome, cheers, nod
- npc_face_expression: raise_eyebrows, sad, smile, angry_face, slight_smile, grin, tired_face, scream, angry, surprise, confused, bored, shy, smug, worried
- npc_target_location: level1_entrance_door, level2_entrance_door, level3_entrance_door, level4_entrance_door, level5_entrance_door, level6_entrance_door, player_location
- angry_level: happy, normal, chill, annoyed, furious, extremely furious
- favorability_change: very negative, negative, neutral, positive, very positive
- giving_to_player: Name of item from npc_inventory to give, or "none"
```

#### 3.3 Personalities & Hobbies

Check the boxes for personality traits and hobbies to assign. These tags are dynamically injected into the AI's context for both main levels and the Hub World.

### Step 4: Save & Play

- Click **"Save Configuration"** (green confirmation text appears).
- Close the Configurator.
- Launch the game via `AI2U - With you til the end.exe`.

> 💡 If dialogue breaks or the character stops responding: check your internet connection, verify API credits, or enable the BepInEx console for error logs.

---

## Known Issues

- ~~Could not chat in Hall via phone booth~~ **Fixed**
- Large CPU usage — may be the game itself. FPS remains high (~200 on i5-11th / RTX 3050 4GB / 16GB RAM).
- ~~Items bought in Hall not usable in gameplay~~ **Fixed**

---

**Thank you.**

**huynhhoang04**
