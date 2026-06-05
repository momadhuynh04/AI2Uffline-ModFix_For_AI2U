# AI2U - Offline & Unlocked Fix Infinite Loading Screen

## 📖 Introduction **READ THIS**
A comprehensive modification and fix for **AI2U** in **Window** i havnt try it in linix yet. This is not a **mod** but a fix if you have the crack version of the game(from skidrow version 0.7.12.2 lastest on their wedsite(23/5/2026) ) and suffering **INFINATE LOADING SCREEN** or some connect error cause their server block it(i guess) and you have try to add **steam_api64.dll and steam_appid.txt** but it still fuck and you dont really give a shit about item and other stupid stuff (except skin this i fix and other stuff) and you really want to try out the gameplay what it could do then try this **remenber i only try to use this on skidrow version 0.7.12.2** other version i will try out in the future **or not** i dont know **IF YOU REALLY WANT TO EXPERIENCES THE GAME JUST BUY IT BUDDY IT ONLY 15 DOLLAR**. Well in this fix you need to use a LLM model could be runing local or use **API KEY** , i have try qwen2.5 3b coder instruct(it such and have alot of pasing error) run local , Gpt oss 20b 120b nemotron 3 nano omni(**THIS I RECOMMEND IT FREE IN OPENROUTER AND ITS THE ONLY FREE MODEL THAT SUPPORT IMAGE THE GAME MAY SEND A REQUEST WITH IMAGE(ENCODE BASE64) IN SOME SITUATION LIKE WATCHING TV OR DRAWING**) , Deepseek v4 flash(it paid but best response so far ive try, way faster than the free model) i use apikey from **OPENROUTER** havnt try other provide4r but basicly same shit, if you insist running **LOCALLY** you gonna need a coder type mode rather than a roleplay cause it need to responce as json so the game could parse it correctlly. And then the TTS model basiclly the game also use the same method as i use SO **BEST THAT YOU HAVE AN AZURE APIKEY FOR IT** if not Open ai is cool, for azure tts you could create is the free plan provide 500,000 wword a month enough to goon gang and if you using sinh azure you dont have to put the base url the code provide it and the speach model dig in to find what voice that you like(for my en-AU-CarlyNeural) go to json2video.com to find.

### If you dislike the voice from azure and you too broke for elevenlad you could try my fine tune weight for Gpt-SoVITS
*It locate in the full gamelink on the installation guide below*
*I also add the template locate in folder tts/ of this repo, if you have a potato pc and could not finetune it, its a template for you to fine tune your own voice in clound like Model or gg Colad*
*The missing part is that Gpt-SoVITS does not work well for game like this, i planning to scripts a neu api version for the this game soon and there will be a full instruction for that here*

# Update Version v2.8(05/06/2026)
**- Add more tts config include piper tts and kokoro run locally**
**- Add prompt routing for character**
**- Add game context and tag injection**
**- Add chat history injection to request payload**
**- Add tag managemant manually(need to adjust in AI2U_Confihurator.exe)**
**- Fix add item to ingame**
**- Fix Dream OS problem(level 1 computer you have to beg the ai the pc and wifi password)**
**- Fix HubWorld invitation problem(you could chat in Hall now)**
**- Open all level(this time is legit)**

## Note
**- For local tts recomand use gpt-sovits but for light weight use kokoro, if you want to chage the voice use the train template**
**- For tag managemant, since ive try many way to fix the saving and injection by it return a mess every time so i plug it in the Confihurator.exe and you will have to adjust it you own add tag for personality and hobby**
**- I spent a morning digging the prompt in src for every character but the last one i could not find it, and im too lazy to actual play the hold level to write a prompt for it so create you own**

Thank to Kora for bug report
**This quest end in June 5th 2026**
**No more farther update!!**

## ✨ Features & What It Solves(**THIS PART IS AI GENERATE SOME CORRECT SOME NOT READ THE INTRO**)
- **Offline Play & Authentication Bypass(Working):** Completely removes Steam and PlayFab login requirements. Solves the issue of getting stuck on the startup screen or crashing due to network connection failures.
- **Custom LLM Integration (AI Proxy)(Working):** Replaces the game's default Azure AI backend. You can now route the NPC chat to external providers (such as OpenRouter) using your own API key and chosen model (e.g., GPT-4, Claude, etc.) for a smarter, filter-free conversation experience.
- **Infinite Currency & Offline Shop Save(Working):** Grants 999,999,999 Tokens. Because PlayFab is bypassed, we built a custom local save system (`ES3`) for the shop. Any cosmetics, items, or Persona tags you buy are permanently saved locally on your machine and never lost.
- **Unlocked NPCs & Restrictions Removed:** Bypasses the "Favor Meter" requirements. All NPCs are fully unlocked and ready to be customized right from the start.
- **Hidden Chapters Unlocked(Working):** Forcibly unlocks Chapters 2, 3, and 4 (which are normally locked with a "Coming Soon" or "Unlock to reveal" label in Early Access). Bypasses broken/unfinished UI logic to let you load directly into these maps.
- **Robust Item Lookup(Working):** Fixes the game's internal case-sensitivity bugs, ensuring your saved shop items (Persona tags, cosmetics) are flawlessly loaded and equipped every time you launch the game.

## ⚙️ Installation Guide

You could download full version off the game from [FullGameWithFix](https://drive.google.com/drive/folders/1LO2G3EelAPuLpc1N7v6FIj9TRwp6BuW2?usp=sharing) the ***version 2.8***, and make sure that you read the ReadMe.txt

### Prerequisites
- **BepInEx (v5.x)** must be installed in your game's root directory. So you in BepIndex (**THIS SHIT IS NEEDED**) for this go to there githud download the zip file x64 i use the version x64 5.4.21.0 and you would want to extract it directly in to the game root folder then you will see a folder name BepInEx and two file **doorstop_config.ini and winhttp.dll**, if missing any do it again  

### Steps
1. Download the this repo as a zip file and extract it
2. OK in the repo folder we have a folder name core right that the main shit both the dll file in there need to be exactly where it need to be first is **Assembly-CSharp.dll** copy it and paste into **Your_Game_Root_Folder/AI2U - With you til the end_Data/Managed/** if it pop up a window that said replace this file then Ok cause i use dnSpy to adjust and re compile this dll to bypass mostly the login path include Steam and PlayFab, recomend to rename the og Assembly-CSharp.dll file to Assembly-CSharp.dll.bak in case of you my file fuck
3. Next is the AI2U_Configurator.dll copy and paste into **Your_Game_Root_Folder/BepInEx/plugins/** this is the fix for rerouting your game as you AI config instance of the game server

4. Then run the game for the first time to create the Config.json file or AI2U_Config.json then you could stop the game and open the json on notepad vscode or whaterver then you could put your **AI config in here** or just copy and paste the Config.json file on the repo you want it dosent effect shit but if it fail rename it to AI2U_Config.json (or the opposite way)
```json
   {
      // this is old config 
      "base_url": "base url like https://api.openai.com/v1/chat/completions search it on gg",
      "api_key": "you key skip if running model locally ",
      "model": "model like gpt-oss-20b, deepseek-v4-flash",
      "system_prompt": "you prompt use the sys prompt in prompt.txt for best response",
      "post_history_prompt": "same as sys prompt",
      "temperature": 0.9, // how creative the ai could put out potential words too high alien langue
      "top_p": 0.95, // limit word choice
      "top_k": 0, //nucleus sampling
      "max_tokens": 2050, // token out put
      "frequency_penalty": 0.05,
      "presence_penalty": 0.05,
      "tts_enable": true,
      "tts_provider": "azure, openai",
      "tts_base_url": " could be empty if azure",
      "tts_api_key": "your key",
      "tts_model": "speech model like en-AU-CarlyNeural",
      "tts_region": "your azure service region close to you equal faster response"
    }
```

5. open and play gang

### Note: 
I also build an gui so that you could adjust AI config in here but you will need python 3 to run, i only use build in library so you properly dont need a vitual enviroment to run (if you want to run it in a vitual enviroment then it fine) , you will need to put 2 file Configuratorv1.2.py and Configurator.bat in this specific path **Your_Game_Root_Folder/BepInEx/** , then run the **Configurator.bat** and it will be looking like this :

![Gui](https://raw.githubusercontent.com/momadhuynh04/AI2Uffline-ModFix_For_AI2U/refs/heads/main/config/image_2026-06-05_145105812.png)
![Gui](https://raw.githubusercontent.com/momadhuynh04/AI2Uffline-ModFix_For_AI2U/refs/heads/main/config/image_2026-06-05_151759788.png)

In the Gui the ai parameter is not working at the moment i write this so you could adjust in the json file above(It work now)

## AI2U Configurator Mod User Guide for v2.5(Update 05/06/2026)

Welcome to the **AI2U Configurator** - the essential tool to revive *AI2U: With You Til The End*. Since the official servers have been shut down, this mod allows you to redirect the entire dialogue system to your own custom API (such as OpenAI, OpenRouter, or OpenAI-compatible local endpoints like Ollama or LM Studio), while giving you complete freedom to "shape" your waifu's personality!

---

## Step 1: Launch the Configurator
After extracting the Mod, locate and double-click `AI2U_Configurator.exe` in your root game directory. The user-friendly interface will open instantly without any extra installations.

## Step 2: API Setup (Crucial)
The game needs a new "brain" to replace the dead server. At the top of the interface:
- **Custom API URL:** Enter your API endpoint. The default is `https://api.openai.com/v1/chat/completions`. If you're using OpenRouter or a Local AI, replace this with the respective URL.
- **Custom API Key:** Paste your API Key here (e.g., `sk-...`). This key is stored securely and locally on your PC.

> [!WARNING]
> Never share your `AI2U_Config.json` file (located in the `BepInEx/config` folder) with anyone unless you have wiped your API Key from it, as it contains your personal, sensitive access token!

## Step 3: Waifu Customization
The left column displays a list of available characters (Estelle, Eiona, etc.). Select the character you want to configure:

### 1. In-Game System Prompt (Main Levels)
This sets the context and behavioral guidelines for the character during the main gameplay levels. You can keep the default prompt or completely rewrite it to make the character act differently (e.g., Yandere, Tsundere).

### 2. Hub World System Prompt (Atrium / Waiting Room)
This prompt is exclusively for the Hub World. Because the Hub World strictly requires a JSON format output so the game engine can parse and control character animations, you **MUST** paste the exact JSON formatting instructions below into this text box (you may append any custom lore/context above it, but the structural instructions must remain intact):

```text
All replies should be strictly using JSON Format!
As an NPC in a video game, reply with JSON code to reflect your current state. The input format from the game will be like (npc_trust_level, npc_location, npc_action, player_location, player_action, npc_inventory, story_guide, sentence_from_player).
The output format MUST be exactly like this:
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
- npc_action: (other, standing, sitting, sitting_down, walking, hugging, cooking, playing_games, following_player)
- npc_body_animation: (idle, chill_idle, shy, stretch, crying, talk, dance, troublesome, cheers, nod)
- npc_face_expression: (raise_eyebrows, sad, smile, angry_face, slight_smile, grin, tired_face, scream, angry, surprise, confused, bored, shy, smug, worried)
- npc_target_location: (level1_entrance_door, level2_entrance_door, level3_entrance_door, level4_entrance_door, level5_entrance_door, level6_entrance_door, player_location)
- angry_level: (happy, normal, chill, annoyed, furious, extremely furious)
- favorability_change: (very negative, negative, neutral, positive, very positive)
- giving_to_player: Name of item from npc_inventory to give, or "none".
```

### 3. Personalities & Hobbies
Check the boxes for the specific personality traits and hobbies you want to assign to the character. These tags will be dynamically injected into the AI's "brain" during both the main levels and the Hub World, giving your waifu much more depth and consistency.

## Step 4: Save & Play
- Click the **"Save Configuration"** button at the bottom right to apply all your settings. A green text "Configuration saved successfully!" will confirm it.
- Close the Configurator and launch the game via `AI2U - With you til the end.exe`.
- Enjoy the game powered by your very own custom AI server!

> [!TIP]
> If the game's dialogue breaks, or the character stops responding entirely, check your internet connection, verify your API billing/credits, or toggle the BepInEx console terminal to look for detailed error logs.


## ⚠️ Known Issues
*(Maintainer notes: Please list any known bugs, missing features, or ongoing issues here)*
- [ could not chat in hall though calling char from phonebooth ] Fixed
- [ massive cpu usage this i dont know because i dont have the og game could be the game are heavy it self but the fps are high like mostly 200 my system laptop i511th 30504g 16ram ] dont give a shit
- [ could not use item that just bought in hall in gameplay ] Fixed

**I fix it in the future i hope so**
**Thanh you**
**huynhoang04**
