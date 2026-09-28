# AI2U — Offline Mod & Infinite Loading Fix · **v2.9**

A modification/fix for **AI2U: With You Til The End**.

The cracked build hangs on an **infinite loading screen** because the official Steam/PlayFab
servers are blocked. This fix removes that login entirely, builds a local save system, and
reroutes the NPC dialogue + voice to **your own LLM API and TTS provider**.

> Tested on the **Skidrow v0.7.12.2** build. Other versions are not guaranteed to work.
>
> **If you enjoy the game, please buy it — it is only $15.**

---

## 📥 Download

> Link in notepad : https://anotepad.com/notes/ngjhjg6s
>
> Full game + fix v2.9 — content all file to run.
> please read file **this file carefully** before playing.

---

# 🇬🇧 English

## What's new in v2.9

- **Redesigned Configurator** — darker, higher-contrast theme; dropdowns and input fields are
  now clearly readable.
- **English / Russian UI** — switch language from the top-right corner of the Configurator.
  (Only the interface is translated; config keys and values stay English.)
- **Fixed a decimal/locale bug** — on Windows set to a comma decimal separator
  (Russian, Vietnamese, German, French, …), values like `temperature`, `top_p`,
  `frequency_penalty`, `presence_penalty` used to silently become `0` (or ×100). Now they are
  read correctly, and the Configurator accepts both `,` and `.` and always writes a dot.
- **Fixed over-precise sampling values** in the request payload (e.g. `0.949999988079071` → `0.95`).
- **Kokoro offline TTS now bundled** — ships with its own portable Python environment, so players
  do **not** need a system-wide Python install. Separate fields for the Kokoro model and voices.
- Item purchasing, Dream OS level 1 passwords, Hub World chat, and chapter unlocks from v2.8 remain.

## Requirements

- Windows 10/11 (x64).
- The game (Skidrow v0.7.12.2).
- An internet connection for the LLM API (and the first Kokoro setup, if needed).
- **No Python installation required** — everything ships with the mod.

## Installation (from scratch)

1. **Extract** the full package into your game folder. You should end up with:

   ```
   AI2U - With you til the end.exe
   AI2U - With you til the end_Data/
   BepInEx/
   doorstop_config.ini
   winhttp.dll
   ```

   If `BepInEx/`, `doorstop_config.ini` or `winhttp.dll` are missing, re-extract — BepInEx is
   what injects the mod.

2. **Open the Configurator:** run `BepInEx\AI2U_Configurator.exe`.

3. **Enter your AI settings** (see *Configuration* below) and click **Save Configuration**.

4. **Play:** launch `AI2U - With you til the end.exe`.

> If the game does not start, run it as Administrator or temporarily disable Windows Defender.

## Configuration (Configurator v2.9)

Everything is set in one window: `BepInEx\AI2U_Configurator.exe`. It saves to
`BepInEx\config\AI2U_Config.json`.

> ⚠️ **Never share `AI2U_Config.json`** — it contains your API key.

Do the steps in order. Only **Step 2 (API)** is required; everything else already has good defaults.

### Step 1 — Open the Configurator

Double-click `BepInEx\AI2U_Configurator.exe`. No installation needed.

The window is one scrollable page with these sections, top to bottom:
**API Settings** → **TTS Settings (Voice)** → **AI Parameters** → **NPC Customization (Tags)** →
**System Prompt** → **Hub World System Prompt** → **Post-History Prompt** →
**Save / Reload / Reset** buttons → status bar (bottom-left).

### Step 2 — API Settings (required)

| Field | Example (OpenRouter) | Notes |
|-------|----------------------|-------|
| **Base URL** | `https://openrouter.ai/api/v1/chat/completions` | The full chat-completions endpoint. OpenAI: `https://api.openai.com/v1/chat/completions`. Local (Ollama / LM Studio): `http://127.0.0.1:PORT/v1/chat/completions`. |
| **API Key** | `sk-or-v1-...` | Your key. Leave empty **only** for a local server. |
| **Model** | `deepseek/deepseek-v4-flash` | Any model from the *Tested models* list. |

This is the only mandatory part. Without a valid key + model, the NPC won't reply.

### Step 3 — AI Parameters (keep the defaults)

| Field | Recommended |
|-------|-------------|
| Temperature | `1.1` |
| Top P | `0.95` |
| Top K | `0` |
| Max Tokens | `2050` |
| Frequency Penalty | `0.05` |
| Presence Penalty | `0.05` |

You can type `,` or `.` as the decimal separator — the Configurator always saves a dot.

### Step 4 — Pick a voice (TTS)

Tick **Enable Custom TTS**, then choose **one** of the paths below and configure only that one.

**A. Online — Azure** (best quality; free tier 500,000 chars/month)
1. **TTS Mode** = `Online TTS (API)`, **TTS Provider** = `Azure`.
2. Fill **TTS API Key** and **Azure Region** (e.g. `eastus`).
3. Leave **TTS Base URL** empty.
4. **TTS Model** = a voice name, e.g. `en-US-JaneNeural`.

**B. Online — OpenAI Compatible** (or any local TTS server)
1. **TTS Mode** = `Online TTS (API)`, **TTS Provider** = `OpenAI Compatible`.
2. **TTS Base URL** = your server root, e.g. `http://127.0.0.1:8880/v1` (empty = OpenAI itself).
3. **TTS API Key** = your key (may be empty for a local server).
4. **TTS Model** = voice / model name.

**C. Offline — Kokoro** (recommended; fully bundled, no internet at play time)
1. **TTS Mode** = `Offline TTS (Local)`, **Offline Provider** = `Kokoro`.
2. **Kokoro Model (.onnx)** and **Kokoro Voices (.bin)** are pre-filled to the bundled files in
   `BepInEx\kokoro\` — leave them as they are.
3. **Offline Voice Model** = a Kokoro voice, e.g. `af_jessica`, `af_bella`, `af_sarah`, `af_sky`.
4. Click **💾 Save Configuration**.
5. **Run `BepInEx\setup_kokoro_env.bat` ONCE — only if the `BepInEx\python` folder does not exist.**
   It downloads a portable Python and the Kokoro packages into `BepInEx\python` (needs internet,
   takes a few minutes). If `BepInEx\python` is already present, skip this step.

**D. Offline — Piper**
1. **TTS Mode** = `Offline TTS (Local)`, **Offline Provider** = `Piper`.
2. **Model File (.onnx)** = `BepInEx\piperweight\en_US-libritts-high.onnx`.
3. **Config File (.json)** = the matching `.json` next to it.

The game launches and stops the local Kokoro server (port `8880`) automatically.

### Step 5 — Character, prompts and tags

1. Pick a character in **Current Character** (Eddie, Elysia, Estelle, Eiona). Prompts and tags are
   stored **separately per character** — switching keeps each one's settings.
2. Edit **System Prompt** (main levels), **Hub World System Prompt** (Atrium) and
   **Post-History Prompt** (output-format reminder).
   > Keep the JSON-format instructions that are already in the prompt — the game needs valid JSON.
3. Under **NPC Customization (Tags)**, tick the **Personalities** and **Hobbies** you want injected.

### Step 6 — Save, then play

1. Click **💾 Save Configuration** — a green confirmation appears in the status bar.
2. Close the Configurator and launch `AI2U - With you til the end.exe`.
3. Optional: the **Language** dropdown (top-right) switches the interface between `English` and
   `Русский`. It only changes labels/notes, never the config keys or values.

### Config file reference

The GUI writes `BepInEx\config\AI2U_Config.json`. You can also edit it by hand:

```json
{
  "base_url": "https://openrouter.ai/api/v1/chat/completions",
  "api_key": "sk-or-v1-...",
  "model": "deepseek/deepseek-v4-flash",
  "temperature": 1.1,
  "top_p": 0.95,
  "top_k": 0,
  "max_tokens": 2050,
  "frequency_penalty": 0.05,
  "presence_penalty": 0.05,
  "tts_enable": true,
  "tts_mode": "Offline",
  "offline_tts_provider": "Kokoro",
  "offline_kokoro_model_path": "...\\BepInEx\\kokoro\\kokoro-v1.0.onnx",
  "offline_kokoro_voices_path": "...\\BepInEx\\kokoro\\voices-v1.0.bin",
  "eddie_offline_tts_model": "af_jessica",
  "ui_language": "en"
}
```

| Key | Meaning |
|-----|---------|
| `tts_mode` | `"Offline"` = Piper / Kokoro, `"Online"` = Azure / OpenAI Compatible. |
| `offline_tts_provider` | `"Piper"` or `"Kokoro"` (used when `tts_mode` = `Offline`). |
| `tts_provider` | `"Azure"` or `"OpenAI Compatible"` (used when `tts_mode` = `Online`). |
| `<char>_offline_tts_model` | Kokoro voice for that character (`eddie_`, `elysia_`, `estelle_`, `eiona_`). |
| `ui_language` | `"en"` or `"ru"` (Configurator interface language). |

## Tested LLM models

Tested through OpenRouter (`base_url` = `https://openrouter.ai/api/v1/chat/completions`):

| Model | Result |
|-------|--------|
| `dots-studio/dots-3-note-preview:free` | ✅ works |
| `nvidia/nemotron-3-ultra-550b-a55b` | ✅ works |
| `qwen/qwen3.8-flash` | ✅ works |
| `z-ai/glm-5.3-flash` | ✅ works |
| `deepseek/deepseek-v4-flash` | ✅ works |
| `deepseek/deepseek-v4.1-flash` | ❌ error |
| `deepseek/deepseek-v4-flash-0731` | ❌ error |

> The model must return **valid JSON** (the game parses the reply). Models that wrap the answer in
> markdown or extra text may fail — the mod strips ```` ```json ```` fences, but not arbitrary prose.

## Logs & troubleshooting

- Mod log: `BepInEx\UltimateFix_Debug.txt` (shows the exact request payload sent to the API).
- BepInEx log: `BepInEx\LogOutput.log`.
- NPC speaks nothing → check the TTS mode/provider and (for Kokoro) that `BepInEx\python` exists.
- No reply from the NPC → check the API key, credits, and internet connection.
- Game won't launch → run as Administrator or disable Windows Defender.

---

# 🇷🇺 Русский

Модификация/фикс для **AI2U: With You Til The End**.

Пиратская сборка зависает на **бесконечном экране загрузки**, потому что официальные серверы
Steam/PlayFab заблокированы. Этот фикс полностью убирает вход, создаёт локальную систему
сохранений и перенаправляет диалоги и озвучку NPC на **ваш собственный LLM API и TTS**.

> Протестировано на сборке **Skidrow v0.7.12.2**. На других версиях не гарантируется.
>
> **Если игра понравилась — купите её, она стоит всего $15.**

## Что нового в v2.9

- **Обновлённый Configurator** — более тёмная и контрастная тема; выпадающие списки и поля ввода
  теперь хорошо читаются.
- **Интерфейс на английском и русском** — переключение языка в правом верхнем углу Configurator.
  (Переведён только интерфейс; ключи и значения конфига остаются на английском.)
- **Исправлен баг с разделителем дробей** — в Windows с запятой в качестве десятичного
  разделителя (русский, вьетнамский, немецкий, французский…) значения `temperature`, `top_p`,
  `frequency_penalty`, `presence_penalty` молча становились `0` (или умножались на 100). Теперь
  они читаются корректно, а Configurator принимает и `,`, и `.` и всегда сохраняет точку.
- **Исправлены слишком длинные значения** параметров в запросе (например, `0.949999988079071` → `0.95`).
- **Kokoro (оффлайн TTS) теперь в комплекте** — со своим портативным Python, поэтому игрокам
  **не нужен** системный Python. Отдельные поля для модели и голосов Kokoro.
- Покупки предметов, пароли Dream OS уровня 1, чат в Hub World и разблокировка глав из v2.8 сохранены.

## Требования

- Windows 10/11 (x64).
- Игра (Skidrow v0.7.12.2).
- Интернет для LLM API (и для первой настройки Kokoro, если потребуется).
- **Python устанавливать не нужно** — всё идёт в комплекте с модом.

## Установка (с нуля)

1. **Распакуйте** полный архив в папку с игрой. В итоге должно быть:

   ```
   AI2U - With you til the end.exe
   AI2U - With you til the end_Data/
   BepInEx/
   doorstop_config.ini
   winhttp.dll
   ```

   Если нет `BepInEx/`, `doorstop_config.ini` или `winhttp.dll` — распакуйте заново: именно BepInEx
   подключает мод.

2. **Откройте Configurator:** запустите `BepInEx\AI2U_Configurator.exe`.

3. **Введите настройки ИИ** (см. *Настройка* ниже) и нажмите **Save Configuration**.

4. **Играйте:** запустите `AI2U - With you til the end.exe`.

> Если игра не запускается — запустите от имени администратора или временно отключите Windows Defender.

## Настройка (Configurator v2.9)

Всё настраивается в одном окне: `BepInEx\AI2U_Configurator.exe`. Сохраняется в
`BepInEx\config\AI2U_Config.json`.

> ⚠️ **Никому не передавайте `AI2U_Config.json`** — там ваш API-ключ.

Выполняйте шаги по порядку. Обязателен только **шаг 2 (API)** — у остального уже есть хорошие
значения по умолчанию.

### Шаг 1 — Откройте Configurator

Запустите `BepInEx\AI2U_Configurator.exe` (двойной клик). Устанавливать ничего не нужно.

Окно — одна прокручиваемая страница с разделами сверху вниз:
**API Settings** → **TTS Settings (Voice)** → **AI Parameters** → **NPC Customization (Tags)** →
**System Prompt** → **Hub World System Prompt** → **Post-History Prompt** →
кнопки **Save / Reload / Reset** → строка состояния (слева внизу).

### Шаг 2 — API Settings (обязательно)

| Поле | Пример (OpenRouter) | Примечание |
|------|---------------------|------------|
| **Base URL** | `https://openrouter.ai/api/v1/chat/completions` | Полный адрес chat completions. OpenAI: `https://api.openai.com/v1/chat/completions`. Локально (Ollama / LM Studio): `http://127.0.0.1:ПОРТ/v1/chat/completions`. |
| **API Key** | `sk-or-v1-...` | Ваш ключ. Пусто — **только** для локального сервера. |
| **Model** | `deepseek/deepseek-v4-flash` | Любая модель из списка *Проверенные модели*. |

Это единственная обязательная часть. Без рабочего ключа и модели NPC не будет отвечать.

### Шаг 3 — AI Parameters (оставьте значения по умолчанию)

| Поле | Рекомендуется |
|------|----------------|
| Temperature | `1.1` |
| Top P | `0.95` |
| Top K | `0` |
| Max Tokens | `2050` |
| Frequency Penalty | `0.05` |
| Presence Penalty | `0.05` |

Разделитель дробей можно вводить как `,`, так и `.` — Configurator всегда сохраняет точку.

### Шаг 4 — Выберите голос (TTS)

Поставьте галочку **Enable Custom TTS**, затем выберите **один** из вариантов ниже и настройте
только его.

**A. Онлайн — Azure** (лучшее качество; бесплатный тариф 500 000 символов/мес)
1. **TTS Mode** = `Online TTS (API)`, **TTS Provider** = `Azure`.
2. Заполните **TTS API Key** и **Azure Region** (например `eastus`).
3. **TTS Base URL** оставьте пустым.
4. **TTS Model** = имя голоса, например `en-US-JaneNeural`.

**B. Онлайн — OpenAI Compatible** (или свой локальный TTS-сервер)
1. **TTS Mode** = `Online TTS (API)`, **TTS Provider** = `OpenAI Compatible`.
2. **TTS Base URL** = корень сервера, например `http://127.0.0.1:8880/v1` (пусто = сам OpenAI).
3. **TTS API Key** = ключ (для локального сервера может быть пустым).
4. **TTS Model** = имя голоса / модели.

**C. Оффлайн — Kokoro** (рекомендуется; всё в комплекте, интернет при игре не нужен)
1. **TTS Mode** = `Offline TTS (Local)`, **Offline Provider** = `Kokoro`.
2. **Kokoro Model (.onnx)** и **Kokoro Voices (.bin)** уже указывают на файлы в `BepInEx\kokoro\`
   — не меняйте их.
3. **Offline Voice Model** = голос Kokoro, например `af_jessica`, `af_bella`, `af_sarah`, `af_sky`.
4. Нажмите **💾 Save Configuration**.
5. **Запустите `BepInEx\setup_kokoro_env.bat` ОДИН РАЗ — только если папки `BepInEx\python` нет.**
   Он скачает портативный Python и пакеты Kokoro в `BepInEx\python` (нужен интернет, несколько
   минут). Если `BepInEx\python` уже есть — пропустите этот шаг.

**D. Оффлайн — Piper**
1. **TTS Mode** = `Offline TTS (Local)`, **Offline Provider** = `Piper`.
2. **Model File (.onnx)** = `BepInEx\piperweight\en_US-libritts-high.onnx`.
3. **Config File (.json)** = соответствующий `.json` рядом.

Игра сама запускает и останавливает локальный сервер Kokoro (порт `8880`).

### Шаг 5 — Персонаж, промпты и теги

1. Выберите персонажа в **Current Character** (Eddie, Elysia, Estelle, Eiona). Промпты и теги
   хранятся **отдельно для каждого** — при переключении настройки сохраняются.
2. Отредактируйте **System Prompt** (основные уровни), **Hub World System Prompt** (Atrium) и
   **Post-History Prompt** (напоминание о формате ответа).
   > Оставьте инструкции по формату JSON, которые уже есть в промпте — игре нужен валидный JSON.
3. В **NPC Customization (Tags)** отметьте нужные **Personalities** и **Hobbies**.

### Шаг 6 — Сохранить и играть

1. Нажмите **💾 Save Configuration** — внизу появится зелёное подтверждение.
2. Закройте Configurator и запустите `AI2U - With you til the end.exe`.
3. При желании переключите язык интерфейса: список **Language** (справа вверху) — `English` /
   `Русский`. Меняется только интерфейс, не ключи и значения конфига.

### Справка по файлу конфига

GUI пишет `BepInEx\config\AI2U_Config.json`. Можно править вручную:

```json
{
  "base_url": "https://openrouter.ai/api/v1/chat/completions",
  "api_key": "sk-or-v1-...",
  "model": "deepseek/deepseek-v4-flash",
  "temperature": 1.1,
  "top_p": 0.95,
  "top_k": 0,
  "max_tokens": 2050,
  "frequency_penalty": 0.05,
  "presence_penalty": 0.05,
  "tts_enable": true,
  "tts_mode": "Offline",
  "offline_tts_provider": "Kokoro",
  "offline_kokoro_model_path": "...\\BepInEx\\kokoro\\kokoro-v1.0.onnx",
  "offline_kokoro_voices_path": "...\\BepInEx\\kokoro\\voices-v1.0.bin",
  "eiona_offline_tts_model": "af_jessica",
  "ui_language": "ru"
}
```

| Ключ | Значение |
|------|----------|
| `tts_mode` | `"Offline"` = Piper / Kokoro, `"Online"` = Azure / OpenAI Compatible. |
| `offline_tts_provider` | `"Piper"` или `"Kokoro"` (при `tts_mode` = `Offline`). |
| `tts_provider` | `"Azure"` или `"OpenAI Compatible"` (при `tts_mode` = `Online`). |
| `<char>_offline_tts_model` | Голос Kokoro для персонажа (`eddie_`, `elysia_`, `estelle_`, `eiona_`). |
| `ui_language` | `"en"` или `"ru"` (язык интерфейса Configurator). |

## Проверенные модели LLM

Проверено через OpenRouter (`base_url` = `https://openrouter.ai/api/v1/chat/completions`):

| Модель | Результат |
|--------|-----------|
| `dots-studio/dots-3-note-preview:free` | ✅ работает |
| `nvidia/nemotron-3-ultra-550b-a55b` | ✅ работает |
| `qwen/qwen3.8-flash` | ✅ работает |
| `z-ai/glm-5.3-flash` | ✅ работает |
| `deepseek/deepseek-v4-flash` | ✅ работает |
| `deepseek/deepseek-v4.1-flash` | ❌ ошибка |
| `deepseek/deepseek-v4-flash-0731` | ❌ ошибка |

> Модель обязана вернуть **валидный JSON** (игра парсит ответ). Модели, оборачивающие ответ в
> markdown или лишний текст, могут не работать — мод убирает блоки ```` ```json ````, но не
> произвольный текст.

## Логи и решение проблем

- Лог мода: `BepInEx\UltimateFix_Debug.txt` (показывает точный запрос к API).
- Лог BepInEx: `BepInEx\LogOutput.log`.
- NPC молчит → проверьте режим/провайдера TTS и (для Kokoro) наличие папки `BepInEx\python`.
- NPC не отвечает → проверьте API-ключ, баланс и интернет.
- Игра не запускается → запустите от администратора или отключите Windows Defender.

---

## Features / Возможности

| Feature | / | Возможность |
|---------|---|-------------|
| Offline play & auth bypass | / | Оффлайн-игра и обход авторизации |
| Custom LLM integration | / | Подключение своего LLM |
| Infinite currency & local shop save | / | Бесконечная валюта и локальные сохранения магазина |
| Unlocked NPCs (no Favor Meter) | / | Все NPC открыты (без Favor Meter) |
| Hidden chapters 2–4 unlocked | / | Скрытые главы 2–4 открыты |
| Local TTS: Piper / Kokoro | / | Локальный TTS: Piper / Kokoro |

---

**Credits**

Scripted by **huynhhoang04** (momadhuynh04). Bug reports: thanks to **Kora**.

Repo: https://github.com/momadhuynh04/AI2Uffline-ModFix_For_AI2U
