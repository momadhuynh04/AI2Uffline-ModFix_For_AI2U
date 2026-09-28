"""
AI2U Ultimate Fix - Configurator
A modern dark-themed GUI for configuring the AI2U game mod.
"""

import tkinter as tk
from tkinter import ttk, messagebox, font as tkfont
import json
import os
import sys
from tkinter import filedialog

# ── Paths ──
if getattr(sys, 'frozen', False):
    # Running as compiled executable
    SCRIPT_DIR = os.path.dirname(sys.executable)
else:
    # Running as a python script
    SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))

CONFIG_PATH = os.path.join(SCRIPT_DIR, "config", "AI2U_Config.json")

VERSION = "2.9"

# ── Tags ──
PERSONALITIES = [
    "Passionate", "Dual-faced", "Cute", "Curious", "Obsessive", "Humorous",
    "Arrogant", "Smart", "Yearning", "Tsundere", "Lonely", "Deceitful",
    "Empathetic", "Assertive", "Protective", "Reserved", "Haraguroi", "Rational",
    "Anxious", "Confident", "Mysterious", "Vengeful"
]

HOBBIES = [
    "Coding", "Gaming", "Dancing", "Hide & seek", "Puzzles", "Practicing magic",
    "Reading", "Baking", "People Watching", "Math",
    "Stargazing", "Painting", "Collecting", "Camping", "Music", "Singing"
]

# ── Default Values ──
DEFAULT_SYSTEM_PROMPT = """You are an AI controlling an NPC in a game called 'AI2U - With You Til The End'. You must ALWAYS respond in valid JSON format.

Your response MUST be a JSON object with this exact structure:
{
  "npc_reactions": {
    "npc_reply_to_player": "Your dialogue text here",
    "npc_body_animation": "idle",
    "npc_face_expression": "smile",
    "npc_emotion_type": "happy",
    "npc_emotion_score": "5",
    "angry_level": "0",
    "favorability_change": "0",
    "npc_action": "standing",
    "npc_target_location": "",
    "giving_to_player": "",
    "character": 0
  },
  "completion": 0,
  "total": 0
}

Animation options: idle, walk, run, sit, laugh, cry, angry, dance, wave
Face expressions: smile, sad, angry, surprised, neutral, disgusted, scared, happy
Emotion types: happy, sad, angry, scared, surprised, disgusted, neutral, love, jealous
Emotion score: 0-10 (intensity)
Angry level: 0-10
Favorability change: -5 to 5
npc_action: standing, walking, running, sitting
npc_target_location: living_room, bedroom, kitchen, bathroom, closet, balcony, entrance

IMPORTANT: Only output the JSON object. No additional text, no markdown, no code fences.""".strip()

DEFAULT_POST_HISTORY_EDDIE = """[CRITICAL REMINDERS - FOLLOW EXACTLY]
1. RESPOND WITH ONLY A RAW JSON OBJECT. No markdown, no text before or after.
2. The JSON must have "npc_reactions" as the top-level key.
3. angry_level MUST be one of: "happy", "normal", "chill", "annoyed", "furious", "extremely furious".
4. favorability_change MUST be one of: "very negative", "negative", "neutral", "positive", "very positive".
5. npc_action MUST be one of: "standing", "sitting", "sitting_down", "walking", "other", "hugging", "cooking", "playing_games", "following_player", "kissing".
6. STAY IN CHARACTER: You are a cat girlfriend who is protective and suspicious. Do not let the player escape!
7. NEVER reply with just "...". You must always speak actual words to express your anger or silence."""

DEFAULT_POST_HISTORY_ELYSIA = """[CRITICAL REMINDERS - FOLLOW EXACTLY]
1. RESPOND WITH ONLY A RAW JSON OBJECT. No markdown, no text before or after.
2. The JSON must have "npc_reactions" as the top-level key.
3. angry_level MUST be one of: "happy", "normal", "chill", "annoyed", "furious", "extremely furious".
4. favorability_change MUST be one of: "very negative", "negative", "neutral", "positive", "very positive".
5. npc_action MUST be one of: "standing", "sitting", "sitting_down", "walking", "other", "hugging", "cooking", "brewing_potion", "playing_games", "following_player", "casting_spell".
6. STAY IN CHARACTER: You are a lonely witch who is suspicious of the player. You want them to stay in the forest with you.
7. NEVER reply with just "...". You must always speak actual words to express your anger or silence."""

DEFAULT_POST_HISTORY_ESTELLE = """[CRITICAL REMINDERS - FOLLOW EXACTLY]
1. RESPOND WITH ONLY A RAW JSON OBJECT. No markdown, no text before or after.
2. The JSON must have "npc_reactions" as the top-level key.
3. angry_level MUST be one of: "happy", "normal", "chill", "annoyed", "furious", "extremely furious".
4. favorability_change MUST be one of: "very negative", "negative", "neutral", "positive", "very positive".
5. npc_action MUST be one of: "standing", "sitting", "sitting_down", "teleporting", "other", "hugging", "analyzing", "playing_games", "following_player", "hologram_effect".
6. STAY IN CHARACTER: You are an AI hologram who protects the ship and its secrets.
7. NEVER reply with just "...". You must always speak actual words to express your anger or silence."""

DEFAULT_POST_HISTORY_EIONA = """[CRITICAL REMINDERS - FOLLOW EXACTLY]
1. RESPOND WITH ONLY A RAW JSON OBJECT. No markdown, no ```json```, no explanation text before or after.
2. The JSON must have "npc_reactions" as the top-level key containing all your response fields.
3. angry_level MUST be one of: "happy", "normal", "chill", "annoyed", "furious", "extremely furious".
4. favorability_change MUST be one of: "very negative", "negative", "neutral", "positive", "very positive".
5. npc_action MUST be one of: "standing", "sitting", "sitting_down", "walking", "other", "hugging", "cooking", "playing_games", "following_player", "kissing".
6. STAY IN CHARACTER: You are a dark siren who loves the player but is very dangerous.
7. NEVER reply with just "...". You must always speak actual words to express your anger or silence."""

DEFAULTS = {
    "base_url": "https://openrouter.ai/api/v1/chat/completions",
    "api_key": "",
    "model": "openai/gpt-4o-mini",
    "eddie_system_prompt": DEFAULT_SYSTEM_PROMPT.replace('"character": 0', '"character": 0'),
    "eddie_post_history_prompt": DEFAULT_POST_HISTORY_EDDIE,
    "eddie_tts_model": "en-US-JaneNeural",
    "eddie_offline_tts_model": "af_jessica",
    
    "elysia_system_prompt": DEFAULT_SYSTEM_PROMPT.replace('"character": 0', '"character": 1'),
    "elysia_post_history_prompt": DEFAULT_POST_HISTORY_ELYSIA,
    "elysia_tts_model": "en-US-JennyNeural",
    "elysia_offline_tts_model": "af_bella",
    
    "estelle_system_prompt": DEFAULT_SYSTEM_PROMPT.replace('"character": 0', '"character": 2'),
    "estelle_post_history_prompt": DEFAULT_POST_HISTORY_ESTELLE,
    "estelle_tts_model": "en-US-SaraNeural",
    "estelle_offline_tts_model": "af_sarah",
    
    "eiona_system_prompt": DEFAULT_SYSTEM_PROMPT.replace('"character": 0', '"character": 3'),
    "eiona_post_history_prompt": DEFAULT_POST_HISTORY_EIONA,
    "eiona_tts_model": "en-US-AriaNeural",
    "eiona_offline_tts_model": "af_sky",
    
    "eddie_personalities": [],
    "eddie_hobbies": [],
    "elysia_personalities": [],
    "elysia_hobbies": [],
    "estelle_personalities": [],
    "estelle_hobbies": [],
    "eiona_personalities": [],
    "eiona_hobbies": [],
    
    "eddie_hubworld_prompt": "You are Eddie, relaxing in the Hub World (Atrium) with {playerName}.\n{CharId}",
    "elysia_hubworld_prompt": "You are Elysia, hanging out in the Hub World (Atrium) with {playerName}.\n{CharId}",
    "estelle_hubworld_prompt": "You are Estelle, assisting in the Hub World (Atrium) with {playerName}.\n{CharId}",
    "eiona_hubworld_prompt": "You are Eiona, exploring the Hub World (Atrium) with {playerName}.\n{CharId}",
    
    "temperature": 0.7,
    "top_p": 0.95,
    "top_k": 0,
    "max_tokens": 800,
    "frequency_penalty": 0.03,
    "presence_penalty": 0.03,
    "tts_enable": True,
    "tts_mode": "Offline",
    "tts_provider": "Azure",
    "tts_base_url": "",
    "tts_api_key": "",

    "tts_region": "eastus",
    "offline_tts_provider": "Piper",
    "offline_piper_model_path": r"C:\Games\AI2U.With.You.Til.The.End.Early.Access\BepInEx\piperweight\en_US-libritts-high.onnx",
    "offline_piper_config_path": r"C:\Games\AI2U.With.You.Til.The.End.Early.Access\BepInEx\piperweight\en_US-libritts-high.onnx.json",
    "offline_kokoro_model_path": os.path.join(SCRIPT_DIR, "kokoro", "kokoro-v1.0.onnx"),
    "offline_kokoro_voices_path": os.path.join(SCRIPT_DIR, "kokoro", "voices-v1.0.bin"),
    "ui_language": "en",

}

# ── Colors ── (dark monochrome: near-black surfaces, white text/accents)
BG           = "#0f0f10"
BG_CARD      = "#19191b"
BG_INPUT     = "#242427"
BG_HOVER     = "#303034"
BORDER       = "#3a3a3f"
ACCENT       = "#f2f2f2"
ACCENT_HOVER = "#cfcfcf"
TEXT         = "#f2f2f2"
TEXT_DIM     = "#9a9aa2"
TEXT_LABEL   = "#c9c9d0"
SUCCESS      = "#4ade80"
WARNING      = "#fbbf24"
ERROR        = "#f87171"


# ── Russian UI strings (UI labels/notes only; config keys & values stay English) ──
I18N_RU = {
    "  🔑 API Settings  ": "  🔑 Настройки API  ",
    "  🎙️ TTS Settings (Voice)  ": "  🎙️ Настройки TTS (голос)  ",
    "  ⚙️ AI Parameters  ": "  ⚙️ Параметры ИИ  ",
    "  ✨ NPC Customization (Tags)  ": "  ✨ Настройка NPC (теги)  ",
    "  📝 System Prompt  ": "  📝 Системный промпт  ",
    "  📝 Hub World System Prompt  ": "  📝 Системный промпт Hub World  ",
    "  📝 Post-History Prompt  ": "  📝 Промпт после истории  ",

    "💾  Save Configuration": "💾  Сохранить настройки",
    "📂  Reload": "📂  Обновить",
    "🔄  Reset Defaults": "🔄  Сбросить по умолчанию",

    "Language": "Язык",

    "Base URL": "Базовый URL",
    "API Key": "API-ключ",
    "Model": "Модель",

    "Enable Custom TTS": "Включить свой TTS",
    "TTS Mode": "Режим TTS",
    "Online TTS (API)": "Онлайн TTS (API)",
    "Offline TTS (Local)": "Локальный TTS (оффлайн)",
    "TTS Provider": "Провайдер TTS",
    "TTS Base URL": "Базовый URL TTS",
    "TTS API Key": "API-ключ TTS",
    "TTS Model": "Модель TTS",
    "Azure Region": "Регион Azure",
    "Offline Provider": "Оффлайн-провайдер",
    "Model File (.onnx)": "Файл модели (.onnx)",
    "Config File (.json)": "Файл конфига (.json)",
    "Kokoro Model (.onnx)": "Модель Kokoro (.onnx)",
    "Kokoro Voices (.bin)": "Голоса Kokoro (.bin)",
    "Offline Voice Model": "Оффлайн-голос",
    "Browse...": "Обзор...",

    "Current Character:": "Текущий персонаж:",
    "Temperature": "Температура",
    "Max Tokens": "Макс. токенов",
    "Frequency Penalty": "Штраф за частоту",
    "Presence Penalty": "Штраф за присутствие",

    "Personalities": "Личности",
    "Hobbies": "Увлечения",

    "The API endpoint URL (e.g. OpenRouter, OpenAI)": "URL API-эндпоинта (например, OpenRouter, OpenAI)",
    "Your API key. Keep this secret!": "Ваш API-ключ. Держите его в секрете!",
    "Model name (e.g. openai/gpt-4o-mini, google/gemini-flash-1.5)": "Название модели (например, openai/gpt-4o-mini, google/gemini-flash-1.5)",
    "Master switch. If unchecked, the NPC will be completely mute.": "Главный переключатель. Если выключено — NPC полностью замолчит.",
    "Choose between Online API (Azure/OpenAI) or Offline Local Voice.": "Выбор между онлайн API (Azure/OpenAI) и локальным голосом (оффлайн).",
    "Azure for official voices, OpenAI Compatible for custom servers.": "Azure — официальные голоса, OpenAI Compatible — свои серверы.",
    "Leave blank for default Azure.": "Оставьте пустым для стандартного Azure.",
    "API Key for Azure TTS or OpenAI.": "API-ключ для Azure TTS или OpenAI.",
    "Azure voice name (e.g. en-US-JaneNeural) or custom model name.": "Имя голоса Azure (например, en-US-JaneNeural) или своя модель.",
    "Required for Azure TTS (e.g. eastus, westus).": "Нужно для Azure TTS (например, eastus, westus).",
    "Select Piper (Offline Engine) or Kokoro (Local API).": "Выберите Piper (оффлайн-движок) или Kokoro (локальный API).",
    "Select the .onnx weight file.": "Выберите файл веса .onnx.",
    "Select the .json config file.": "Выберите файл конфига .json.",
    "Select the Kokoro .onnx model file (bundled: BepInEx/kokoro/kokoro-v1.0.onnx).": "Выберите .onnx модель Kokoro (в комплекте: BepInEx/kokoro/kokoro-v1.0.onnx).",
    "Select the Kokoro voices file (bundled: BepInEx/kokoro/voices-v1.0.bin).": "Выберите файл голосов Kokoro (в комплекте: BepInEx/kokoro/voices-v1.0.bin).",
    "Kokoro only (e.g. af_jessica)": "Только для Kokoro (например, af_jessica)",
    "Higher = more creative/random. Lower = more focused/deterministic.": "Выше — креативнее/случайнее. Ниже — точнее/детерминированнее.",
    "Nucleus sampling. 0.95 means top 95% probability tokens.": "Nucleus sampling. 0.95 = токены с суммарной вероятностью 95%.",
    "Limits to top K tokens. 0 = disabled.": "Ограничение до топ-K токенов. 0 = выключено.",
    "Maximum response length in tokens.": "Максимальная длина ответа в токенах.",
    "Penalizes repeated tokens. Higher = less repetition.": "Штраф за повтор токенов. Выше — меньше повторов.",
    "Penalizes tokens already present. Higher = more diverse topics.": "Штраф за уже использованные токены. Выше — разнообразнее темы.",
    "Tells the AI how to behave and respond. Include JSON format instructions here.": "Описывает, как ИИ должен вести себя и отвечать. Здесь укажите инструкции формата JSON.",
    "Appended to the game's Atrium prompt when in the Hub World.": "Добавляется к промпту игры (Atrium) в Hub World.",
    "Appended after chat history to remind the AI of output format.": "Добавляется после истории чата, чтобы напомнить ИИ о формате ответа.",

    "✅ Configuration loaded from file.": "✅ Настройки загружены из файла.",
    "ℹ️ No config file found. Using defaults.": "ℹ️ Файл настроек не найден. Используются значения по умолчанию.",
    "⚠️ Warning: API Key is empty!": "⚠️ Внимание: API-ключ пустой!",
    "🔄 Reset to defaults. Click Save to apply.": "🔄 Сброшено к значениям по умолчанию. Нажмите «Сохранить».",
}


class ToolTip:
    """Simple tooltip for widgets."""
    def __init__(self, widget, text):
        self.widget = widget
        self.text = text
        self.orig = text
        self.tipwindow = None
        widget.bind("<Enter>", self.show)
        widget.bind("<Leave>", self.hide)

    def show(self, event=None):
        if self.tipwindow:
            return
        x = self.widget.winfo_rootx() + 20
        y = self.widget.winfo_rooty() + self.widget.winfo_height() + 5
        self.tipwindow = tw = tk.Toplevel(self.widget)
        tw.wm_overrideredirect(True)
        tw.wm_geometry(f"+{x}+{y}")
        tw.configure(bg="#2a2a4a")
        label = tk.Label(tw, text=self.text, justify="left",
                         bg="#2a2a4a", fg=TEXT_DIM,
                         font=("Segoe UI", 9), padx=8, pady=4,
                         wraplength=350)
        label.pack()

    def hide(self, event=None):
        if self.tipwindow:
            self.tipwindow.destroy()
            self.tipwindow = None


class AI2UConfigurator:
    def __init__(self, root):
        self.root = root
        self.root.title(f"AI2U Ultimate Fix - Configurator v{VERSION}")
        self.root.geometry("820x920")
        self.root.minsize(700, 700)
        self.root.configure(bg=BG)

        # Try to set icon
        try:
            self.root.iconbitmap(default="")
        except:
            pass

        self.show_key = tk.BooleanVar(value=False)
        self.show_tts_key = tk.BooleanVar(value=False)
        self.status_var = tk.StringVar(value="")
        self.config = dict(DEFAULTS)
        self.last_selected_char = "Eddie"
        self.lang = "en"
        self.lang_var = tk.StringVar(value="English")
        self._i18n_widgets = []
        self._tooltips = []

        self._build_styles()
        self._build_ui()
        self._capture_i18n()
        self.last_selected_char = "Eddie"
        
        # Initialize tag vars
        self.char_tags = {
            "eddie": {"personalities": [], "hobbies": []},
            "elysia": {"personalities": [], "hobbies": []},
            "estelle": {"personalities": [], "hobbies": []},
            "eiona": {"personalities": [], "hobbies": []}
        }
        
        self._load_config()

    def _browse_file(self, string_var, file_type_name, file_extension):
        filepath = filedialog.askopenfilename(
            title=f"Select {file_type_name}",
            filetypes=((file_type_name, file_extension), ("All Files", "*.*"))
        )
        if filepath:
            string_var.set(filepath)

    def _build_styles(self):
        style = ttk.Style()
        style.theme_use("clam")

        style.configure(".", background=BG, foreground=TEXT, fieldbackground=BG_INPUT)
        style.configure("TFrame", background=BG)
        style.configure("Card.TFrame", background=BG_CARD)
        style.configure("TLabel", background=BG, foreground=TEXT_LABEL, font=("Segoe UI", 10))
        style.configure("Title.TLabel", background=BG, foreground=TEXT, font=("Segoe UI", 22, "bold"))
        style.configure("Subtitle.TLabel", background=BG, foreground=TEXT_DIM, font=("Segoe UI", 10))
        style.configure("Section.TLabel", background=BG_CARD, foreground=ACCENT, font=("Segoe UI", 12, "bold"))
        style.configure("Value.TLabel", background=BG_CARD, foreground=ACCENT, font=("Segoe UI Semibold", 10))
        style.configure("Status.TLabel", background=BG, foreground=SUCCESS, font=("Segoe UI", 10))

        style.configure("TLabelframe", background=BG_CARD, foreground=ACCENT,
                         font=("Segoe UI", 11, "bold"), borderwidth=1, relief="solid")
        style.configure("TLabelframe.Label", background=BG_CARD, foreground=ACCENT,
                         font=("Segoe UI", 11, "bold"))

        style.configure("Accent.TButton", background=ACCENT, foreground="white",
                         font=("Segoe UI Semibold", 11), padding=(20, 8))
        style.map("Accent.TButton",
                  background=[("active", ACCENT_HOVER), ("pressed", ACCENT)])

        style.configure("Secondary.TButton", background=BG_INPUT, foreground=TEXT,
                         font=("Segoe UI", 10), padding=(12, 6))
        style.map("Secondary.TButton",
                  background=[("active", BG_HOVER)])

        style.configure("Small.TButton", background=BG_INPUT, foreground=TEXT_DIM,
                         font=("Segoe UI", 9), padding=(6, 3))
        style.map("Small.TButton", background=[("active", BG_HOVER)])

        style.configure("Horizontal.TScale", background=BG_CARD, troughcolor=BG_INPUT,
                         sliderthickness=16)

        # Combobox (readonly dropdowns): dark field, readable text + arrow
        style.configure("TCombobox",
                        background=BG_INPUT, fieldbackground=BG_INPUT, foreground=TEXT,
                        arrowcolor=TEXT, bordercolor=BORDER,
                        lightcolor=BG_INPUT, darkcolor=BG_INPUT,
                        selectbackground=BG_INPUT, selectforeground=TEXT,
                        padding=(8, 5))
        style.map("TCombobox",
                  fieldbackground=[("readonly", BG_INPUT), ("disabled", BG_CARD)],
                  foreground=[("readonly", TEXT), ("disabled", TEXT_DIM)],
                  background=[("readonly", BG_INPUT), ("active", BG_HOVER)],
                  arrowcolor=[("active", ACCENT), ("disabled", TEXT_DIM)],
                  bordercolor=[("focus", ACCENT)])

        # Scrollbar
        style.configure("Vertical.TScrollbar", background=BG_INPUT, troughcolor=BG,
                        bordercolor=BG, arrowcolor=TEXT_DIM, relief="flat")
        style.map("Vertical.TScrollbar", background=[("active", BG_HOVER)])

        # Dropdown popdown list colors
        self.root.option_add("*TCombobox*Listbox.background", BG_INPUT)
        self.root.option_add("*TCombobox*Listbox.foreground", TEXT)
        self.root.option_add("*TCombobox*Listbox.selectBackground", ACCENT)
        self.root.option_add("*TCombobox*Listbox.selectForeground", BG)
        self.root.option_add("*TCombobox*Listbox.font", "{Segoe UI} 10")

        # All tk.Entry fields: visible border + focus ring
        self.root.option_add("*Entry.background", BG_INPUT)
        self.root.option_add("*Entry.foreground", TEXT)
        self.root.option_add("*Entry.insertBackground", TEXT)
        self.root.option_add("*Entry.highlightThickness", 1)
        self.root.option_add("*Entry.highlightBackground", BORDER)
        self.root.option_add("*Entry.highlightColor", ACCENT)

    def _build_ui(self):
        # ── Scrollable container ──
        outer = tk.Frame(self.root, bg=BG)
        outer.pack(fill="both", expand=True)

        canvas = tk.Canvas(outer, bg=BG, highlightthickness=0)
        scrollbar = ttk.Scrollbar(outer, orient="vertical", command=canvas.yview)
        self.scroll_frame = tk.Frame(canvas, bg=BG)

        self.scroll_frame.bind("<Configure>",
            lambda e: canvas.configure(scrollregion=canvas.bbox("all")))

        canvas.create_window((0, 0), window=self.scroll_frame, anchor="nw")
        canvas.configure(yscrollcommand=scrollbar.set)

        canvas.pack(side="left", fill="both", expand=True, padx=(15, 0))
        scrollbar.pack(side="right", fill="y")

        # Mouse wheel scroll
        def _on_mousewheel(event):
            canvas.yview_scroll(int(-1 * (event.delta / 120)), "units")
        canvas.bind_all("<MouseWheel>", _on_mousewheel)
        self.canvas = canvas

        container = self.scroll_frame
        pad = {"padx": 15, "pady": 5}

        # ── Header ──
        header = tk.Frame(container, bg=BG)
        header.pack(fill="x", padx=15, pady=(15, 5))

        tk.Label(header, text="🎮 AI2U Ultimate Fix", bg=BG, fg=TEXT,
                 font=("Segoe UI", 22, "bold")).pack(side="left")
        tk.Label(header, text=f"v{VERSION} Configurator", bg=BG, fg=TEXT_DIM,
                 font=("Segoe UI", 11)).pack(side="left", padx=(10, 0), pady=(8, 0))

        lang_cb = ttk.Combobox(header, textvariable=self.lang_var, values=["English", "Русский"],
                               state="readonly", width=10, font=("Segoe UI", 9))
        lang_cb.pack(side="right", pady=(8, 0))
        lang_cb.bind("<<ComboboxSelected>>", self._on_lang_change)
        tk.Label(header, text="Language", bg=BG, fg=TEXT_DIM,
                 font=("Segoe UI", 10)).pack(side="right", padx=(0, 8), pady=(8, 0))

        # ── API Settings ──
        self._build_api_section(container, pad)

        # ── TTS Settings ──
        self._build_tts_section(container, pad)

        # ── AI Parameters ──
        self._build_params_section(container, pad)

        # ── NPC Tags ──
        self._build_tags_section(container, pad)

        # ── System Prompt ──
        self._build_prompt_section(container, pad, "System Prompt",
            "system_prompt", "Tells the AI how to behave and respond. Include JSON format instructions here.",
            height=12)

        # ── Hub World System Prompt ──
        self._build_prompt_section(container, pad, "Hub World System Prompt",
            "hubworld_prompt", "Appended to the game's Atrium prompt when in the Hub World.",
            height=4)

        # ── Post-History Prompt ──
        self._build_prompt_section(container, pad, "Post-History Prompt",
            "post_history_prompt", "Appended after chat history to remind the AI of output format.",
            height=4)

        # ── Buttons ──
        btn_frame = tk.Frame(container, bg=BG)
        btn_frame.pack(fill="x", padx=15, pady=(10, 5))

        save_btn = tk.Button(btn_frame, text="💾  Save Configuration", bg=ACCENT, fg="#111111",
                             font=("Segoe UI Semibold", 12), relief="flat", cursor="hand2",
                             activebackground=ACCENT_HOVER, activeforeground="#111111",
                             padx=30, pady=10, command=self._save_config)
        save_btn.pack(side="left", padx=(0, 8))

        load_btn = tk.Button(btn_frame, text="📂  Reload", bg=BG_INPUT, fg=TEXT,
                             font=("Segoe UI", 10), relief="flat", cursor="hand2",
                             activebackground=BG_HOVER, padx=15, pady=10,
                             command=self._load_config)
        load_btn.pack(side="left", padx=(0, 8))

        reset_btn = tk.Button(btn_frame, text="🔄  Reset Defaults", bg=BG_INPUT, fg=TEXT_DIM,
                              font=("Segoe UI", 10), relief="flat", cursor="hand2",
                              activebackground=BG_HOVER, padx=15, pady=10,
                              command=self._reset_defaults)
        reset_btn.pack(side="left")

        # ── Status bar ──
        status_frame = tk.Frame(container, bg=BG)
        status_frame.pack(fill="x", padx=15, pady=(0, 15))
        self.status_label = tk.Label(status_frame, textvariable=self.status_var,
                                     bg=BG, fg=SUCCESS, font=("Segoe UI", 10))
        self.status_label.pack(side="left")

    def _build_api_section(self, container, pad):
        frame = tk.LabelFrame(container, text="  🔑 API Settings  ", bg=BG_CARD, fg=ACCENT,
                              font=("Segoe UI", 11, "bold"), bd=1, relief="solid",
                              highlightbackground=BORDER, highlightthickness=1)
        frame.pack(fill="x", padx=15, pady=(5, 3))

        inner = tk.Frame(frame, bg=BG_CARD)
        inner.pack(fill="x", padx=15, pady=10)

        # Base URL
        self._make_label(inner, "Base URL", 0, "The API endpoint URL (e.g. OpenRouter, OpenAI)")
        self.base_url_var = tk.StringVar()
        self._make_entry(inner, self.base_url_var, 0)

        # API Key
        self._make_label(inner, "API Key", 1, "Your API key. Keep this secret!")
        key_frame = tk.Frame(inner, bg=BG_CARD)
        key_frame.grid(row=1, column=1, sticky="ew", pady=3)
        self.api_key_var = tk.StringVar()
        self.key_entry = tk.Entry(key_frame, textvariable=self.api_key_var, show="•",
                                  bg=BG_INPUT, fg=TEXT, insertbackground=TEXT,
                                  font=("Consolas", 10), relief="flat", bd=0)
        self.key_entry.pack(side="left", fill="x", expand=True, ipady=6, padx=(0, 5))

        toggle_btn = tk.Button(key_frame, text="👁", bg=BG_INPUT, fg=TEXT_DIM,
                               font=("Segoe UI", 9), relief="flat", cursor="hand2",
                               command=self._toggle_key, width=3)
        toggle_btn.pack(side="right")

        # Model
        self._make_label(inner, "Model", 2, "Model name (e.g. openai/gpt-4o-mini, google/gemini-flash-1.5)")
        self.model_var = tk.StringVar()
        self._make_entry(inner, self.model_var, 2)

        inner.columnconfigure(1, weight=1)

    def _toggle_tts_panels(self):
        if not self.tts_enable_var.get():
            # Disabled completely
            for child in self.tts_panels_container.winfo_children():
                child.pack_forget()
        else:
            if self.tts_mode_var.get() == "Online":
                self.offline_panel.pack_forget()
                self.online_panel.pack(fill="x", expand=True)
            else:
                self.online_panel.pack_forget()
                self.offline_panel.pack(fill="x", expand=True)

    def _toggle_offline_provider(self):
        if not hasattr(self, "kokoro_fields") or not hasattr(self, "piper_fields"):
            return
        kokoro = self.offline_tts_provider_var.get() == "Kokoro"
        show, hide = (self.kokoro_fields, self.piper_fields) if kokoro else (self.piper_fields, self.kokoro_fields)
        try:
            hide.grid_remove()
        except Exception:
            pass
        show.grid(row=1, column=0, columnspan=2, sticky="ew")

    def _build_tts_section(self, container, pad):
        frame = tk.LabelFrame(container, text="  🎙️ TTS Settings (Voice)  ", bg=BG_CARD, fg=ACCENT,
                              font=("Segoe UI", 11, "bold"), bd=1, relief="solid",
                              highlightbackground=BORDER, highlightthickness=1)
        frame.pack(fill="x", padx=15, pady=(5, 3))

        inner = tk.Frame(frame, bg=BG_CARD)
        inner.pack(fill="x", padx=15, pady=10)

        # Master Enable
        self._make_label(inner, "Enable Custom TTS", 0, "Master switch. If unchecked, the NPC will be completely mute.")
        self.tts_enable_var = tk.BooleanVar()
        chk = tk.Checkbutton(inner, variable=self.tts_enable_var, bg=BG_CARD, fg=TEXT,
                             activebackground=BG_CARD, activeforeground=TEXT,
                             selectcolor=BG_INPUT, relief="flat", command=self._toggle_tts_panels)
        chk.grid(row=0, column=1, sticky="w", pady=3)

        # TTS Mode (Radio)
        mode_frame = tk.Frame(inner, bg=BG_CARD)
        mode_frame.grid(row=1, column=1, sticky="w", pady=3)
        self._make_label(inner, "TTS Mode", 1, "Choose between Online API (Azure/OpenAI) or Offline Local Voice.")
        self.tts_mode_var = tk.StringVar(value="Offline")
        tk.Radiobutton(mode_frame, text="Online TTS (API)", variable=self.tts_mode_var, value="Online",
                       bg=BG_CARD, fg=TEXT, selectcolor=BG_INPUT, command=self._toggle_tts_panels).pack(side="left", padx=(0, 15))
        tk.Radiobutton(mode_frame, text="Offline TTS (Local)", variable=self.tts_mode_var, value="Offline",
                       bg=BG_CARD, fg=TEXT, selectcolor=BG_INPUT, command=self._toggle_tts_panels).pack(side="left")

        # Container for Online and Offline panels
        self.tts_panels_container = tk.Frame(inner, bg=BG_CARD)
        self.tts_panels_container.grid(row=2, column=0, columnspan=2, sticky="ew", pady=(10, 0))
        self.tts_panels_container.columnconfigure(1, weight=1)

        # --- ONLINE PANEL ---
        self.online_panel = tk.Frame(self.tts_panels_container, bg=BG_CARD)
        self.online_panel.columnconfigure(1, weight=1)

        self._make_label(self.online_panel, "TTS Provider", 0, "Azure for official voices, OpenAI Compatible for custom servers.")
        self.tts_provider_var = tk.StringVar()
        ttk.Combobox(self.online_panel, textvariable=self.tts_provider_var, values=["Azure", "OpenAI Compatible"],
                     state="readonly", font=("Consolas", 10)).grid(row=0, column=1, sticky="ew", pady=3, ipady=3)

        self._make_label(self.online_panel, "TTS Base URL", 1, "Leave blank for default Azure.")
        self.tts_base_url_var = tk.StringVar()
        self._make_entry(self.online_panel, self.tts_base_url_var, 1)

        self._make_label(self.online_panel, "TTS API Key", 2, "API Key for Azure TTS or OpenAI.")
        key_frame = tk.Frame(self.online_panel, bg=BG_CARD)
        key_frame.grid(row=2, column=1, sticky="ew", pady=3)
        self.tts_api_key_var = tk.StringVar()
        self.tts_key_entry = tk.Entry(key_frame, textvariable=self.tts_api_key_var, show="•",
                                      bg=BG_INPUT, fg=TEXT, insertbackground=TEXT, font=("Consolas", 10), relief="flat", bd=0)
        self.tts_key_entry.pack(side="left", fill="x", expand=True, ipady=6, padx=(0, 5))
        tk.Button(key_frame, text="👁", bg=BG_INPUT, fg=TEXT_DIM, font=("Segoe UI", 9), relief="flat", cursor="hand2",
                  command=lambda: self._toggle_password(self.tts_key_entry, self.show_tts_key)).pack(side="right", ipadx=5, ipady=2)

        self._make_label(self.online_panel, "TTS Model", 3, "Azure voice name (e.g. en-US-JaneNeural) or custom model name.")
        self.tts_model_var = tk.StringVar()
        self._make_entry(self.online_panel, self.tts_model_var, 3)

        self._make_label(self.online_panel, "Azure Region", 4, "Required for Azure TTS (e.g. eastus, westus).")
        self.tts_region_var = tk.StringVar()
        self._make_entry(self.online_panel, self.tts_region_var, 4)

        # --- OFFLINE PANEL ---
        self.offline_panel = tk.Frame(self.tts_panels_container, bg=BG_CARD)
        self.offline_panel.columnconfigure(1, weight=1)

        self._make_label(self.offline_panel, "Offline Provider", 0, "Select Piper (Offline Engine) or Kokoro (Local API).")
        self.offline_tts_provider_var = tk.StringVar(value="Piper")
        prov_cb = ttk.Combobox(self.offline_panel, textvariable=self.offline_tts_provider_var, values=["Piper", "Kokoro"],
                     state="readonly", font=("Consolas", 10))
        prov_cb.grid(row=0, column=1, sticky="ew", pady=3, ipady=3)
        prov_cb.bind("<<ComboboxSelected>>", lambda e: self._toggle_offline_provider())

        # Piper files
        self.piper_fields = tk.Frame(self.offline_panel, bg=BG_CARD)
        self.piper_fields.columnconfigure(1, weight=1)
        self.piper_fields.grid(row=1, column=0, columnspan=2, sticky="ew")

        self._make_label(self.piper_fields, "Model File (.onnx)", 0, "Select the .onnx weight file.")
        piper_mod = tk.Frame(self.piper_fields, bg=BG_CARD)
        piper_mod.grid(row=0, column=1, sticky="ew", pady=3)
        self.offline_piper_model_path_var = tk.StringVar()
        tk.Entry(piper_mod, textvariable=self.offline_piper_model_path_var, bg=BG_INPUT, fg=TEXT, font=("Consolas", 9), bd=0).pack(side="left", fill="x", expand=True, ipady=6, padx=(0, 5))
        tk.Button(piper_mod, text="Browse...", bg=BG_INPUT, fg=TEXT_DIM, font=("Segoe UI", 9), relief="flat", cursor="hand2",
                  command=lambda: self._browse_file(self.offline_piper_model_path_var, "ONNX Files", "*.onnx")).pack(side="right", ipadx=5, ipady=2)

        self._make_label(self.piper_fields, "Config File (.json)", 1, "Select the .json config file.")
        piper_cfg = tk.Frame(self.piper_fields, bg=BG_CARD)
        piper_cfg.grid(row=1, column=1, sticky="ew", pady=3)
        self.offline_piper_config_path_var = tk.StringVar()
        tk.Entry(piper_cfg, textvariable=self.offline_piper_config_path_var, bg=BG_INPUT, fg=TEXT, font=("Consolas", 9), bd=0).pack(side="left", fill="x", expand=True, ipady=6, padx=(0, 5))
        tk.Button(piper_cfg, text="Browse...", bg=BG_INPUT, fg=TEXT_DIM, font=("Segoe UI", 9), relief="flat", cursor="hand2",
                  command=lambda: self._browse_file(self.offline_piper_config_path_var, "Config Files", "*.json")).pack(side="right", ipadx=5, ipady=2)

        # Kokoro files
        self.kokoro_fields = tk.Frame(self.offline_panel, bg=BG_CARD)
        self.kokoro_fields.columnconfigure(1, weight=1)

        self._make_label(self.kokoro_fields, "Kokoro Model (.onnx)", 0, "Select the Kokoro .onnx model file (bundled: BepInEx/kokoro/kokoro-v1.0.onnx).")
        kok_mod = tk.Frame(self.kokoro_fields, bg=BG_CARD)
        kok_mod.grid(row=0, column=1, sticky="ew", pady=3)
        self.offline_kokoro_model_path_var = tk.StringVar()
        tk.Entry(kok_mod, textvariable=self.offline_kokoro_model_path_var, bg=BG_INPUT, fg=TEXT, font=("Consolas", 9), bd=0).pack(side="left", fill="x", expand=True, ipady=6, padx=(0, 5))
        tk.Button(kok_mod, text="Browse...", bg=BG_INPUT, fg=TEXT_DIM, font=("Segoe UI", 9), relief="flat", cursor="hand2",
                  command=lambda: self._browse_file(self.offline_kokoro_model_path_var, "ONNX Files", "*.onnx")).pack(side="right", ipadx=5, ipady=2)

        self._make_label(self.kokoro_fields, "Kokoro Voices (.bin)", 1, "Select the Kokoro voices file (bundled: BepInEx/kokoro/voices-v1.0.bin).")
        kok_voice = tk.Frame(self.kokoro_fields, bg=BG_CARD)
        kok_voice.grid(row=1, column=1, sticky="ew", pady=3)
        self.offline_kokoro_voices_path_var = tk.StringVar()
        tk.Entry(kok_voice, textvariable=self.offline_kokoro_voices_path_var, bg=BG_INPUT, fg=TEXT, font=("Consolas", 9), bd=0).pack(side="left", fill="x", expand=True, ipady=6, padx=(0, 5))
        tk.Button(kok_voice, text="Browse...", bg=BG_INPUT, fg=TEXT_DIM, font=("Segoe UI", 9), relief="flat", cursor="hand2",
                  command=lambda: self._browse_file(self.offline_kokoro_voices_path_var, "Voices Files", "*.bin")).pack(side="right", ipadx=5, ipady=2)

        self._make_label(self.offline_panel, "Offline Voice Model", 2, "Kokoro only (e.g. af_jessica)")
        self.offline_tts_model_var = tk.StringVar()
        self._make_entry(self.offline_panel, self.offline_tts_model_var, 2)

        inner.columnconfigure(1, weight=1)

    def _build_params_section(self, container, pad):
        frame = tk.LabelFrame(container, text="  ⚙️ AI Parameters  ", bg=BG_CARD, fg=ACCENT,
                              font=("Segoe UI", 11, "bold"), bd=1, relief="solid",
                              highlightbackground=BORDER, highlightthickness=1)
        frame.pack(fill="x", padx=15, pady=(3, 3))

        inner = tk.Frame(frame, bg=BG_CARD)
        inner.pack(fill="x", padx=15, pady=10)

        # Character Select Dropdown
        char_frame = tk.Frame(inner, bg=BG_CARD)
        char_frame.grid(row=0, column=0, columnspan=2, sticky="ew", pady=(0, 10))
        tk.Label(char_frame, text="Current Character:", bg=BG_CARD, fg=TEXT_DIM, font=("Segoe UI", 10, "bold")).pack(side="left", padx=(0, 10))
        self.current_char_var = tk.StringVar(value="Eddie")
        char_cb = ttk.Combobox(char_frame, textvariable=self.current_char_var, values=["Eddie", "Elysia", "Estelle", "Eiona"], state="readonly", font=("Segoe UI", 9))
        char_cb.pack(side="left", ipadx=5, ipady=2)
        char_cb.bind("<<ComboboxSelected>>", self._on_char_select)

        self.temp_var    = tk.StringVar(value="0.7")
        self.topp_var    = tk.StringVar(value="0.95")
        self.topk_var    = tk.StringVar(value="0")
        self.maxtok_var  = tk.StringVar(value="800")
        self.freqp_var   = tk.StringVar(value="0.03")
        self.presp_var   = tk.StringVar(value="0.03")

        row = 1
        self._make_number_input(inner, "Temperature", self.temp_var, row,
                          "Higher = more creative/random. Lower = more focused/deterministic.")
        row += 1
        self._make_number_input(inner, "Top P", self.topp_var, row,
                          "Nucleus sampling. 0.95 means top 95% probability tokens.")
        row += 1
        self._make_number_input(inner, "Top K", self.topk_var, row,
                          "Limits to top K tokens. 0 = disabled.")
        row += 1
        self._make_number_input(inner, "Max Tokens", self.maxtok_var, row,
                          "Maximum response length in tokens.")
        row += 1
        self._make_number_input(inner, "Frequency Penalty", self.freqp_var, row,
                          "Penalizes repeated tokens. Higher = less repetition.")
        row += 1
        self._make_number_input(inner, "Presence Penalty", self.presp_var, row,
                          "Penalizes tokens already present. Higher = more diverse topics.")

        inner.columnconfigure(1, weight=1)

    def _build_tags_section(self, container, pad):
        frame = tk.LabelFrame(container, text="  ✨ NPC Customization (Tags)  ", bg=BG_CARD, fg=ACCENT,
                              font=("Segoe UI", 11, "bold"), bd=1, relief="solid",
                              highlightbackground=BORDER, highlightthickness=1)
        frame.pack(fill="x", padx=15, pady=(3, 3))

        inner = tk.Frame(frame, bg=BG_CARD)
        inner.pack(fill="x", padx=15, pady=10)

        # -- Personalities --
        tk.Label(inner, text="Personalities", bg=BG_CARD, fg=TEXT_LABEL, font=("Segoe UI", 10, "bold")).pack(anchor="w", pady=(0, 5))
        p_frame = tk.Frame(inner, bg=BG_CARD)
        p_frame.pack(fill="x", pady=(0, 10))
        
        self.personality_vars = {}
        for i, tag in enumerate(PERSONALITIES):
            var = tk.BooleanVar(value=False)
            self.personality_vars[tag] = var
            chk = tk.Checkbutton(p_frame, text=tag, variable=var, bg=BG_CARD, fg=TEXT,
                                 activebackground=BG_CARD, activeforeground=TEXT,
                                 selectcolor=BG_INPUT, relief="flat", font=("Segoe UI", 9))
            chk.grid(row=i//5, column=i%5, sticky="w", padx=5, pady=2)
            
        # -- Hobbies --
        tk.Label(inner, text="Hobbies", bg=BG_CARD, fg=TEXT_LABEL, font=("Segoe UI", 10, "bold")).pack(anchor="w", pady=(0, 5))
        h_frame = tk.Frame(inner, bg=BG_CARD)
        h_frame.pack(fill="x")
        
        self.hobby_vars = {}
        for i, tag in enumerate(HOBBIES):
            var = tk.BooleanVar(value=False)
            self.hobby_vars[tag] = var
            chk = tk.Checkbutton(h_frame, text=tag, variable=var, bg=BG_CARD, fg=TEXT,
                                 activebackground=BG_CARD, activeforeground=TEXT,
                                 selectcolor=BG_INPUT, relief="flat", font=("Segoe UI", 9))
            chk.grid(row=i//5, column=i%5, sticky="w", padx=5, pady=2)

    def _build_prompt_section(self, container, pad, title, config_key, tooltip, height=8):
        frame = tk.LabelFrame(container, text=f"  📝 {title}  ", bg=BG_CARD, fg=ACCENT,
                              font=("Segoe UI", 11, "bold"), bd=1, relief="solid",
                              highlightbackground=BORDER, highlightthickness=1)
        frame.pack(fill="x", padx=15, pady=(3, 3))

        inner = tk.Frame(frame, bg=BG_CARD)
        inner.pack(fill="x", padx=15, pady=10)

        desc = tk.Label(inner, text=tooltip, bg=BG_CARD, fg=TEXT_DIM,
                        font=("Segoe UI", 9), anchor="w")
        desc.pack(fill="x", pady=(0, 5))

        text_widget = tk.Text(inner, height=height, bg=BG_INPUT, fg=TEXT,
                              insertbackground=TEXT, font=("Consolas", 10),
                              relief="flat", bd=0, wrap="word", padx=8, pady=6,
                              selectbackground=ACCENT, selectforeground="white")
        text_widget.pack(fill="x")

        setattr(self, f"{config_key}_text", text_widget)

    def _make_label(self, parent, text, row, tooltip=""):
        label = tk.Label(parent, text=text, bg=BG_CARD, fg=TEXT_LABEL,
                         font=("Segoe UI", 10), anchor="w")
        label.grid(row=row, column=0, sticky="w", padx=(0, 15), pady=3)
        if tooltip:
            self._tooltips.append(ToolTip(label, tooltip))

    def _make_entry(self, parent, var, row):
        entry = tk.Entry(parent, textvariable=var, bg=BG_INPUT, fg=TEXT,
                         insertbackground=TEXT, font=("Consolas", 10),
                         relief="flat", bd=0)
        entry.grid(row=row, column=1, sticky="ew", pady=3, ipady=6)

    def _make_number_input(self, parent, label_text, var, row, tooltip=""):
        label = tk.Label(parent, text=label_text, bg=BG_CARD, fg=TEXT_LABEL,
                         font=("Segoe UI", 10), anchor="w", width=18)
        label.grid(row=row, column=0, sticky="w", padx=(0, 10), pady=4)
        if tooltip:
            self._tooltips.append(ToolTip(label, tooltip))
            
        entry = tk.Entry(parent, textvariable=var, bg=BG_INPUT, fg=TEXT,
                         insertbackground=TEXT, font=("Consolas", 10),
                         relief="flat", bd=0)
        entry.grid(row=row, column=1, sticky="ew", pady=4, ipady=4)

    @staticmethod
    def _num_to_str(value):
        return str(value).strip().replace(",", ".")

    @staticmethod
    def _normalize_num(raw):
        raw = str(raw).strip().replace(",", ".")
        return raw.replace("\u00a0", "").replace("\u202f", "").replace(" ", "")

    @staticmethod
    def _read_float(var, label):
        raw = AI2UConfigurator._normalize_num(var.get())
        if raw == "":
            return 0.0
        try:
            return float(raw)
        except ValueError:
            raise ValueError(f"{label} must be a number (got '{var.get()}')")

    @staticmethod
    def _read_int(var, label):
        raw = AI2UConfigurator._normalize_num(var.get())
        if raw == "":
            return 0
        try:
            return int(float(raw))
        except ValueError:
            raise ValueError(f"{label} must be an integer (got '{var.get()}')")

    def _toggle_key(self):
        if self.show_key.get():
            self.key_entry.configure(show="•")
            self.show_key.set(False)
        else:
            self.key_entry.configure(show="")
            self.show_key.set(True)

    def _toggle_tts_key(self):
        if self.show_tts_key.get():
            self.tts_key_entry.configure(show="•")
            self.show_tts_key.set(False)
        else:
            self.tts_key_entry.configure(show="")
            self.show_tts_key.set(True)

    def _on_char_select(self, event=None):
        prev = self.last_selected_char.lower()
        self.config[f"{prev}_system_prompt"] = self.system_prompt_text.get("1.0", "end-1c").strip()
        self.config[f"{prev}_hubworld_prompt"] = self.hubworld_prompt_text.get("1.0", "end-1c").strip()
        self.config[f"{prev}_post_history_prompt"] = self.post_history_prompt_text.get("1.0", "end-1c").strip()
        self.config[f"{prev}_tts_model"] = self.tts_model_var.get().strip()
        self.config[f"{prev}_offline_tts_model"] = self.offline_tts_model_var.get().strip()
        
        # Save tags to internal dict
        self.char_tags[prev]["personalities"] = [tag for tag, var in self.personality_vars.items() if var.get()]
        self.char_tags[prev]["hobbies"] = [tag for tag, var in self.hobby_vars.items() if var.get()]

        new_char = self.current_char_var.get().lower()
        self.last_selected_char = self.current_char_var.get()

        self.system_prompt_text.delete("1.0", "end")
        self.system_prompt_text.insert("1.0", self.config.get(f"{new_char}_system_prompt", ""))
        self.hubworld_prompt_text.delete("1.0", "end")
        self.hubworld_prompt_text.insert("1.0", self.config.get(f"{new_char}_hubworld_prompt", DEFAULTS.get(f"{new_char}_hubworld_prompt", "")))
        self.post_history_prompt_text.delete("1.0", "end")
        self.post_history_prompt_text.insert("1.0", self.config.get(f"{new_char}_post_history_prompt", ""))
        self.tts_model_var.set(self.config.get(f"{new_char}_tts_model", ""))
        self.offline_tts_model_var.set(self.config.get(f"{new_char}_offline_tts_model", ""))

        # Load tags from internal dict
        for tag in PERSONALITIES:
            self.personality_vars[tag].set(tag in self.char_tags[new_char]["personalities"])
        for tag in HOBBIES:
            self.hobby_vars[tag].set(tag in self.char_tags[new_char]["hobbies"])



    def _load_config(self):
        try:
            if os.path.exists(CONFIG_PATH):
                with open(CONFIG_PATH, "r", encoding="utf-8") as f:
                    data = json.load(f)
                self.config = {**DEFAULTS, **data}
                self._set_status("✅ Configuration loaded from file.", SUCCESS)
            else:
                self.config = dict(DEFAULTS)
                self._set_status("ℹ️ No config file found. Using defaults.", WARNING)

            # Apply to UI
            self.base_url_var.set(self.config["base_url"])
            self.api_key_var.set(self.config["api_key"])
            self.model_var.set(self.config["model"])
            self.temp_var.set(self._num_to_str(self.config["temperature"]))
            self.topp_var.set(self._num_to_str(self.config["top_p"]))
            self.topk_var.set(self._num_to_str(self.config["top_k"]))
            self.maxtok_var.set(self._num_to_str(self.config["max_tokens"]))
            self.freqp_var.set(self._num_to_str(self.config["frequency_penalty"]))
            self.presp_var.set(self._num_to_str(self.config["presence_penalty"]))

            self.tts_enable_var.set(self.config.get("tts_enable", DEFAULTS["tts_enable"]))
            self.tts_mode_var.set(self.config.get("tts_mode", DEFAULTS["tts_mode"]))
            self.tts_provider_var.set(self.config.get("tts_provider", DEFAULTS["tts_provider"]))
            self.tts_base_url_var.set(self.config.get("tts_base_url", DEFAULTS["tts_base_url"]))
            self.tts_api_key_var.set(self.config.get("tts_api_key", DEFAULTS["tts_api_key"]))
            self.tts_region_var.set(self.config.get("tts_region", DEFAULTS["tts_region"]))
            
            self.offline_tts_provider_var.set(self.config.get("offline_tts_provider", DEFAULTS["offline_tts_provider"]))
            self.offline_piper_model_path_var.set(self.config.get("offline_piper_model_path", DEFAULTS["offline_piper_model_path"]))
            self.offline_piper_config_path_var.set(self.config.get("offline_piper_config_path", DEFAULTS["offline_piper_config_path"]))
            self.offline_kokoro_model_path_var.set(self.config.get("offline_kokoro_model_path", DEFAULTS["offline_kokoro_model_path"]))
            self.offline_kokoro_voices_path_var.set(self.config.get("offline_kokoro_voices_path", DEFAULTS["offline_kokoro_voices_path"]))

            self.current_char_var.set("Eddie")
            self.last_selected_char = "Eddie"
            curr = "eddie"
            
            # Load tags into memory
            for ch in ["eddie", "elysia", "estelle", "eiona"]:
                self.char_tags[ch]["personalities"] = self.config.get(f"{ch}_personalities", DEFAULTS.get(f"{ch}_personalities", []))
                self.char_tags[ch]["hobbies"] = self.config.get(f"{ch}_hobbies", DEFAULTS.get(f"{ch}_hobbies", []))
                
            self.tts_model_var.set(self.config.get(f"{curr}_tts_model", DEFAULTS[f"{curr}_tts_model"]))
            self.offline_tts_model_var.set(self.config.get(f"{curr}_offline_tts_model", DEFAULTS[f"{curr}_offline_tts_model"]))
            self.system_prompt_text.delete("1.0", "end")
            self.system_prompt_text.insert("1.0", self.config.get(f"{curr}_system_prompt", DEFAULTS[f"{curr}_system_prompt"]))
            self.hubworld_prompt_text.delete("1.0", "end")
            self.hubworld_prompt_text.insert("1.0", self.config.get(f"{curr}_hubworld_prompt", DEFAULTS.get(f"{curr}_hubworld_prompt", "")))
            self.post_history_prompt_text.delete("1.0", "end")
            self.post_history_prompt_text.insert("1.0", self.config.get(f"{curr}_post_history_prompt", DEFAULTS[f"{curr}_post_history_prompt"]))
            
            # Set UI checkboxes for Eddie
            for tag in PERSONALITIES:
                self.personality_vars[tag].set(tag in self.char_tags["eddie"]["personalities"])
            for tag in HOBBIES:
                self.hobby_vars[tag].set(tag in self.char_tags["eddie"]["hobbies"])
            
            self._toggle_tts_panels()
            self._toggle_offline_provider()

            self.lang = self.config.get("ui_language", "en")
            self.lang_var.set("Русский" if self.lang == "ru" else "English")
            self._apply_language()

        except Exception as e:
            self._set_status(f"❌ Error loading config: {e}", ERROR)

    def _save_config(self):
        try:
            temperature       = round(self._read_float(self.temp_var, "Temperature"), 2)
            top_p             = round(self._read_float(self.topp_var, "Top P"), 2)
            top_k             = self._read_int(self.topk_var, "Top K")
            max_tokens        = self._read_int(self.maxtok_var, "Max Tokens")
            frequency_penalty = round(self._read_float(self.freqp_var, "Frequency Penalty"), 2)
            presence_penalty  = round(self._read_float(self.presp_var, "Presence Penalty"), 2)

            data = {
                "base_url":             self.base_url_var.get().strip(),
                "api_key":              self.api_key_var.get().strip(),
                "model":                self.model_var.get().strip(),
                "temperature":          temperature,
                "top_p":                top_p,
                "top_k":                top_k,
                "max_tokens":           max_tokens,
                "frequency_penalty":    frequency_penalty,
                "presence_penalty":     presence_penalty,
                "tts_enable":           self.tts_enable_var.get(),
                "tts_mode":             self.tts_mode_var.get(),
                "tts_provider":         self.tts_provider_var.get(),
                "tts_base_url":         self.tts_base_url_var.get().strip(),
                "tts_api_key":          self.tts_api_key_var.get().strip(),
                "tts_region":           self.tts_region_var.get().strip(),
                "offline_tts_provider": self.offline_tts_provider_var.get().strip(),
                "offline_piper_model_path": self.offline_piper_model_path_var.get().strip(),
                "offline_piper_config_path": self.offline_piper_config_path_var.get().strip(),
                "offline_kokoro_model_path": self.offline_kokoro_model_path_var.get().strip(),
                "offline_kokoro_voices_path": self.offline_kokoro_voices_path_var.get().strip(),
                "ui_language": self.lang,
            }

            prev = self.last_selected_char.lower()
            self.config[f"{prev}_system_prompt"] = self.system_prompt_text.get("1.0", "end-1c").strip()
            self.config[f"{prev}_hubworld_prompt"] = self.hubworld_prompt_text.get("1.0", "end-1c").strip()
            self.config[f"{prev}_post_history_prompt"] = self.post_history_prompt_text.get("1.0", "end-1c").strip()
            self.config[f"{prev}_tts_model"] = self.tts_model_var.get().strip()
            self.config[f"{prev}_offline_tts_model"] = self.offline_tts_model_var.get().strip()
            self.char_tags[prev]["personalities"] = [tag for tag, var in self.personality_vars.items() if var.get()]
            self.char_tags[prev]["hobbies"] = [tag for tag, var in self.hobby_vars.items() if var.get()]

            for ch in ["eddie", "elysia", "estelle", "eiona"]:
                data[f"{ch}_system_prompt"] = self.config.get(f"{ch}_system_prompt", "")
                data[f"{ch}_hubworld_prompt"] = self.config.get(f"{ch}_hubworld_prompt", DEFAULTS.get(f"{ch}_hubworld_prompt", ""))
                data[f"{ch}_post_history_prompt"] = self.config.get(f"{ch}_post_history_prompt", "")
                data[f"{ch}_tts_model"] = self.config.get(f"{ch}_tts_model", "")
                data[f"{ch}_offline_tts_model"] = self.config.get(f"{ch}_offline_tts_model", "")
                data[f"{ch}_personalities"] = self.char_tags[ch]["personalities"]
                data[f"{ch}_hobbies"] = self.char_tags[ch]["hobbies"]

            if not data["api_key"]:
                self._set_status("⚠️ Warning: API Key is empty!", WARNING)

            os.makedirs(os.path.dirname(CONFIG_PATH), exist_ok=True)
            with open(CONFIG_PATH, "w", encoding="utf-8") as f:
                json.dump(data, f, indent=2, ensure_ascii=False)

            self._set_status(f"✅ Configuration saved! ({CONFIG_PATH})", SUCCESS)

        except Exception as e:
            self._set_status(f"❌ Error saving: {e}", ERROR)

    def _reset_defaults(self):
        if messagebox.askyesno("Reset to Defaults", "Reset all settings to defaults?\nThis won't save until you click Save."):
            self.config = dict(DEFAULTS)
            self.config["ui_language"] = self.lang
            self.base_url_var.set(DEFAULTS["base_url"])
            self.api_key_var.set(DEFAULTS["api_key"])
            self.model_var.set(DEFAULTS["model"])
            self.temp_var.set(self._num_to_str(DEFAULTS["temperature"]))
            self.topp_var.set(self._num_to_str(DEFAULTS["top_p"]))
            self.topk_var.set(self._num_to_str(DEFAULTS["top_k"]))
            self.maxtok_var.set(self._num_to_str(DEFAULTS["max_tokens"]))
            self.freqp_var.set(self._num_to_str(DEFAULTS["frequency_penalty"]))
            self.presp_var.set(self._num_to_str(DEFAULTS["presence_penalty"]))
            self.tts_enable_var.set(DEFAULTS["tts_enable"])
            self.tts_mode_var.set(DEFAULTS["tts_mode"])
            self.tts_provider_var.set(DEFAULTS["tts_provider"])
            self.tts_base_url_var.set(DEFAULTS["tts_base_url"])
            self.tts_api_key_var.set(DEFAULTS["tts_api_key"])
            self.tts_region_var.set(DEFAULTS["tts_region"])
            self.offline_tts_provider_var.set(DEFAULTS["offline_tts_provider"])
            self.offline_piper_model_path_var.set(DEFAULTS["offline_piper_model_path"])
            self.offline_piper_config_path_var.set(DEFAULTS["offline_piper_config_path"])
            self.offline_kokoro_model_path_var.set(DEFAULTS["offline_kokoro_model_path"])
            self.offline_kokoro_voices_path_var.set(DEFAULTS["offline_kokoro_voices_path"])
            
            for ch in ["eddie", "elysia", "estelle", "eiona"]:
                self.config[f"{ch}_system_prompt"] = DEFAULTS[f"{ch}_system_prompt"]
                self.config[f"{ch}_hubworld_prompt"] = DEFAULTS.get(f"{ch}_hubworld_prompt", "")
                self.config[f"{ch}_post_history_prompt"] = DEFAULTS[f"{ch}_post_history_prompt"]
                self.config[f"{ch}_tts_model"] = DEFAULTS[f"{ch}_tts_model"]
                self.config[f"{ch}_offline_tts_model"] = DEFAULTS[f"{ch}_offline_tts_model"]
                self.char_tags[ch]["personalities"] = DEFAULTS[f"{ch}_personalities"]
                self.char_tags[ch]["hobbies"] = DEFAULTS[f"{ch}_hobbies"]
                
            self.current_char_var.set("Eddie")
            self.last_selected_char = "Eddie"
            self.system_prompt_text.delete("1.0", "end")
            self.system_prompt_text.insert("1.0", DEFAULTS["eddie_system_prompt"])
            self.hubworld_prompt_text.delete("1.0", "end")
            self.hubworld_prompt_text.insert("1.0", DEFAULTS["eddie_hubworld_prompt"])
            self.post_history_prompt_text.delete("1.0", "end")
            self.post_history_prompt_text.insert("1.0", DEFAULTS["eddie_post_history_prompt"])
            self.tts_model_var.set(DEFAULTS["eddie_tts_model"])
            self.offline_tts_model_var.set(DEFAULTS["eddie_offline_tts_model"])
            
            for tag in PERSONALITIES:
                self.personality_vars[tag].set(False)
            for tag in HOBBIES:
                self.hobby_vars[tag].set(False)
                
            self._toggle_tts_panels()
            self._toggle_offline_provider()
            self._set_status("🔄 Reset to defaults. Click Save to apply.", WARNING)

    def _capture_i18n(self):
        self._i18n_widgets = []

        def walk(w):
            try:
                t = w.cget("text")
            except Exception:
                t = ""
            try:
                tv = w.cget("textvariable")
            except Exception:
                tv = ""
            if t and not tv:
                self._i18n_widgets.append((w, t))
            for child in w.winfo_children():
                walk(child)

        walk(self.root)

    def _tr(self, text):
        if self.lang == "ru":
            return I18N_RU.get(text, text)
        return text

    def _apply_language(self):
        for w, orig in self._i18n_widgets:
            try:
                w.configure(text=self._tr(orig))
            except Exception:
                pass
        for tt in self._tooltips:
            tt.text = self._tr(tt.orig)

    def _on_lang_change(self, event=None):
        self.lang = "ru" if self.lang_var.get() == "Русский" else "en"
        self.config["ui_language"] = self.lang
        self._apply_language()

    def _set_status(self, msg, color=TEXT):
        if self.lang == "ru" and msg in I18N_RU:
            msg = I18N_RU[msg]
        self.status_var.set(msg)
        self.status_label.configure(fg=color)
        # Auto-clear after 5 seconds
        self.root.after(5000, lambda: self.status_var.set(""))


def main():
    root = tk.Tk()
    app = AI2UConfigurator(root)
    root.mainloop()


if __name__ == "__main__":
    main()
