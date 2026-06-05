using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;
using Subsystems;
using UnityEngine.SceneManagement;
using SimpleJSON;
using ChatGPTUtility;
using wAIfuBackend;

namespace AI2U_UltimateFix
{
    [BepInPlugin("com.omni.ai2ufix", "AI2U Ultimate Protocol", "8.0.0")]
    public class UltimateFixPlugin : BaseUnityPlugin
    {
        // ── Config from JSON ──
        private static string cfgBaseURL   = "https://openrouter.ai/api/v1/chat/completions";
        private static string cfgAPIKey    = "";
        private static string cfgModel     = "openai/gpt-4o-mini";
        public static string cfgEddieSystemPrompt = "";
        public static string cfgElysiaSystemPrompt = "";
        public static string cfgEstelleSystemPrompt = "";
        public static string cfgEionaSystemPrompt = "";

        public static string cfgEddieHubWorldPrompt = "";
        public static string cfgElysiaHubWorldPrompt = "";
        public static string cfgEstelleHubWorldPrompt = "";
        public static string cfgEionaHubWorldPrompt = "";

        public static string cfgEddiePostHistoryPrompt = "";
        
        public static string[] cfgEddiePersonalities = new string[0];
        public static string[] cfgEddieHobbies = new string[0];
        public static string[] cfgElysiaPersonalities = new string[0];
        public static string[] cfgElysiaHobbies = new string[0];
        public static string[] cfgEstellePersonalities = new string[0];
        public static string[] cfgEstelleHobbies = new string[0];
        public static string[] cfgEionaPersonalities = new string[0];
        public static string[] cfgEionaHobbies = new string[0];

        public static string InjectTags(string originalCharId, string[] personalities, string[] hobbies) {
            if (string.IsNullOrEmpty(originalCharId)) return "";
            int pIndex = originalCharId.IndexOf("<Personality>");
            if (pIndex >= 0) {
                originalCharId = originalCharId.Substring(0, pIndex).TrimEnd();
            }
            string personalityTags = "";
            if (personalities != null) {
                foreach(var t in personalities) personalityTags += "[" + t + "]";
            }
            string hobbyTags = "";
            if (hobbies != null) {
                foreach(var t in hobbies) hobbyTags += "[" + t + "]";
            }
            if (!string.IsNullOrEmpty(personalityTags)) originalCharId += "\n<Personality>" + personalityTags;
            if (!string.IsNullOrEmpty(hobbyTags)) originalCharId += "\n<Hobby>" + hobbyTags;
            return originalCharId;
        }

        public static string cfgEddieTTSModel = "en-US-JaneNeural";
        private static string cfgEddieOfflineTTSModel = "af_jessica";

        private static string cfgElysiaPostHistoryPrompt = "";
        private static string cfgElysiaTTSModel = "en-US-JennyNeural";
        private static string cfgElysiaOfflineTTSModel = "af_bella";

        private static string cfgEstellePostHistoryPrompt = "";
        private static string cfgEstelleTTSModel = "en-US-SaraNeural";
        private static string cfgEstelleOfflineTTSModel = "af_sarah";

        private static string cfgEionaPostHistoryPrompt = "";
        private static string cfgEionaTTSModel = "en-US-AriaNeural";
        private static string cfgEionaOfflineTTSModel = "af_sky";
        private static float  cfgTemperature = 0.7f;
        private static float  cfgTopP        = 0.95f;
        private static int    cfgTopK        = 0;
        private static int    cfgMaxTokens   = 800;
        private static float  cfgFreqPenalty = 0.03f;
        private static float  cfgPresPenalty = 0.03f;

        public static ConfigEntry<float> ConfigFreqPenalty;
        public static ConfigEntry<float> ConfigPresPenalty;
        
        public static ConfigEntry<bool> ConfigTTSEnable;
        public static ConfigEntry<string> ConfigTTSMode;
        
        public static string cfgTTSProvider = "Azure";
        public static string cfgTTSBaseURL  = "";
        public static string cfgTTSAPIKey   = "";
        public static string cfgTTSRegion   = "eastus";
        
        public static string cfgOfflineTTSProvider = "Piper";
        public static string cfgOfflinePiperModelPath = "";
        public static string cfgOfflinePiperConfigPath = "";
        
        private static IntPtr cachedVoicePtr = IntPtr.Zero;
        private static string cachedVoiceModelPath = "";

        private static System.Diagnostics.Process kokoroProcess = null;

        public static string pluginDir;
        private static string configPath;
        private static string logPath;
        public static BepInEx.Logging.ManualLogSource BepLogger;

        public static void LogDebug(string msg)
        {
            try { File.AppendAllText(logPath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg + "\n"); } catch { }
            if (BepLogger != null) BepLogger.LogInfo(msg);
        }


        public static Character GetActiveCharacter()
        {
            Communicator comm = UnityEngine.Object.FindObjectOfType<Communicator>();
            if (comm != null)
            {
                var field = typeof(Communicator).GetField("currentCharacterID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null) return (Character)field.GetValue(comm);
            }
            return Character.Eddie;
        }

        public static string GetActiveSystemPrompt()
        {
            Character activeChar = GetActiveCharacter();
            switch (activeChar)
            {
                case Character.Eddie: return cfgEddieSystemPrompt;
                case Character.Elysia: return cfgElysiaSystemPrompt;
                case Character.Estelle: return cfgEstelleSystemPrompt;
                case Character.Eiona: return cfgEionaSystemPrompt;
                default: return cfgEddieSystemPrompt;
            }
        }

        public static string GetActiveHubWorldPrompt()
        {
            Character activeChar = GetActiveCharacter();
            switch (activeChar)
            {
                case Character.Eddie: return cfgEddieHubWorldPrompt;
                case Character.Elysia: return cfgElysiaHubWorldPrompt;
                case Character.Estelle: return cfgEstelleHubWorldPrompt;
                case Character.Eiona: return cfgEionaHubWorldPrompt;
                default: return cfgEddieHubWorldPrompt;
            }
        }

        public static string GetActivePostPrompt()
        {
            switch (GetActiveCharacter())
            {
                case Character.Elysia: return cfgElysiaPostHistoryPrompt;
                case Character.Estelle: return cfgEstellePostHistoryPrompt;
                case Character.Eiona: return cfgEionaPostHistoryPrompt;
                default: return cfgEddiePostHistoryPrompt;
            }
        }

        public static string GetActiveTTSModel(bool offline)
        {
            if (offline)
            {
                switch (GetActiveCharacter())
                {
                    case Character.Elysia: return string.IsNullOrEmpty(cfgElysiaOfflineTTSModel) ? "en_US-libritts-high" : cfgElysiaOfflineTTSModel;
                    case Character.Estelle: return string.IsNullOrEmpty(cfgEstelleOfflineTTSModel) ? "en_US-libritts-high" : cfgEstelleOfflineTTSModel;
                    case Character.Eiona: return string.IsNullOrEmpty(cfgEionaOfflineTTSModel) ? "en_US-libritts-high" : cfgEionaOfflineTTSModel;
                    default: return string.IsNullOrEmpty(cfgEddieOfflineTTSModel) ? "en_US-libritts-high" : cfgEddieOfflineTTSModel;
                }
            }
            
            // If Kokoro auto-routing is active (OpenAI Compatible mode + local server)
            if (cfgTTSProvider == "OpenAI Compatible" && cfgTTSBaseURL.Contains("127.0.0.1:8880"))
            {
                switch (GetActiveCharacter())
                {
                    case Character.Elysia: return string.IsNullOrEmpty(cfgElysiaOfflineTTSModel) ? "af_bella" : cfgElysiaOfflineTTSModel;
                    case Character.Estelle: return string.IsNullOrEmpty(cfgEstelleOfflineTTSModel) ? "af_sarah" : cfgEstelleOfflineTTSModel;
                    case Character.Eiona: return string.IsNullOrEmpty(cfgEionaOfflineTTSModel) ? "af_sky" : cfgEionaOfflineTTSModel;
                    default: return string.IsNullOrEmpty(cfgEddieOfflineTTSModel) ? "af_jessica" : cfgEddieOfflineTTSModel;
                }
            }
            
            // Otherwise Azure online model
            switch (GetActiveCharacter())
            {
                case Character.Elysia: return string.IsNullOrEmpty(cfgElysiaTTSModel) ? "en-US-JennyNeural" : cfgElysiaTTSModel;
                case Character.Estelle: return string.IsNullOrEmpty(cfgEstelleTTSModel) ? "en-US-SaraNeural" : cfgEstelleTTSModel;
                case Character.Eiona: return string.IsNullOrEmpty(cfgEionaTTSModel) ? "en-US-AriaNeural" : cfgEionaTTSModel;
                default: return string.IsNullOrEmpty(cfgEddieTTSModel) ? "en-US-JaneNeural" : cfgEddieTTSModel;
            }
        }

        public static string EscapeJsonString(string s)
        {
            if (s == null) return "";
            return s
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        private void LoadJsonConfig()
        {
            try
            {
                if (!File.Exists(configPath))
                {
                    LogDebug("Config JSON not found at: " + configPath + ". Using defaults. Please run AI2U_Configurator to set up.");
                    return;
                }

                string jsonText = File.ReadAllText(configPath);
                JSONNode cfg = JSON.Parse(jsonText);
                if (cfg == null) { LogDebug("Failed to parse config JSON."); return; }

                if (cfg["base_url"] != null)          cfgBaseURL          = cfg["base_url"].Value;
                if (cfg["api_key"] != null)            cfgAPIKey           = cfg["api_key"].Value;
                if (cfg["model"] != null)              cfgModel            = cfg["model"].Value;
                foreach (string ch in new string[]{"eddie", "elysia", "estelle", "eiona"})
                {
                    if (cfg[ch + "_system_prompt"] != null) {
                        if (ch == "eddie") cfgEddieSystemPrompt = cfg[ch + "_system_prompt"].Value;
                        if (ch == "elysia") cfgElysiaSystemPrompt = cfg[ch + "_system_prompt"].Value;
                        if (ch == "estelle") cfgEstelleSystemPrompt = cfg[ch + "_system_prompt"].Value;
                        if (ch == "eiona") cfgEionaSystemPrompt = cfg[ch + "_system_prompt"].Value;
                    }
                    if (cfg[ch + "_hubworld_prompt"] != null) {
                        if (ch == "eddie") cfgEddieHubWorldPrompt = cfg[ch + "_hubworld_prompt"].Value;
                        if (ch == "elysia") cfgElysiaHubWorldPrompt = cfg[ch + "_hubworld_prompt"].Value;
                        if (ch == "estelle") cfgEstelleHubWorldPrompt = cfg[ch + "_hubworld_prompt"].Value;
                        if (ch == "eiona") cfgEionaHubWorldPrompt = cfg[ch + "_hubworld_prompt"].Value;
                    }
                    if (cfg[ch + "_post_history_prompt"] != null) {
                        if (ch == "eddie") cfgEddiePostHistoryPrompt = cfg[ch + "_post_history_prompt"].Value;
                        if (ch == "elysia") cfgElysiaPostHistoryPrompt = cfg[ch + "_post_history_prompt"].Value;
                        if (ch == "estelle") cfgEstellePostHistoryPrompt = cfg[ch + "_post_history_prompt"].Value;
                        if (ch == "eiona") cfgEionaPostHistoryPrompt = cfg[ch + "_post_history_prompt"].Value;
                    }
                    if (cfg[ch + "_tts_model"] != null) {
                        if (ch == "eddie") cfgEddieTTSModel = cfg[ch + "_tts_model"].Value;
                        if (ch == "elysia") cfgElysiaTTSModel = cfg[ch + "_tts_model"].Value;
                        if (ch == "estelle") cfgEstelleTTSModel = cfg[ch + "_tts_model"].Value;
                        if (ch == "eiona") cfgEionaTTSModel = cfg[ch + "_tts_model"].Value;
                    }
                    if (cfg[ch + "_offline_tts_model"] != null) {
                        if (ch == "eddie") cfgEddieOfflineTTSModel = cfg[ch + "_offline_tts_model"].Value;
                        if (ch == "elysia") cfgElysiaOfflineTTSModel = cfg[ch + "_offline_tts_model"].Value;
                        if (ch == "estelle") cfgEstelleOfflineTTSModel = cfg[ch + "_offline_tts_model"].Value;
                        if (ch == "eiona") cfgEionaOfflineTTSModel = cfg[ch + "_offline_tts_model"].Value;
                    }
                    if (cfg[ch + "_personalities"] != null) {
                        var arr = cfg[ch + "_personalities"].AsArray;
                        string[] parsedArr = new string[arr.Count];
                        for(int i = 0; i < arr.Count; i++) parsedArr[i] = arr[i].Value;
                        
                        if (ch == "eddie") cfgEddiePersonalities = parsedArr;
                        if (ch == "elysia") cfgElysiaPersonalities = parsedArr;
                        if (ch == "estelle") cfgEstellePersonalities = parsedArr;
                        if (ch == "eiona") cfgEionaPersonalities = parsedArr;
                    }
                    if (cfg[ch + "_hobbies"] != null) {
                        var arr = cfg[ch + "_hobbies"].AsArray;
                        string[] parsedArr = new string[arr.Count];
                        for(int i = 0; i < arr.Count; i++) parsedArr[i] = arr[i].Value;
                        
                        if (ch == "eddie") cfgEddieHobbies = parsedArr;
                        if (ch == "elysia") cfgElysiaHobbies = parsedArr;
                        if (ch == "estelle") cfgEstelleHobbies = parsedArr;
                        if (ch == "eiona") cfgEionaHobbies = parsedArr;
                    }
                }
                if (cfg["temperature"] != null)        cfgTemperature      = cfg["temperature"].AsFloat;
                if (cfg["top_p"] != null)              cfgTopP             = cfg["top_p"].AsFloat;
                if (cfg["top_k"] != null)              cfgTopK             = cfg["top_k"].AsInt;
                if (cfg["max_tokens"] != null)         cfgMaxTokens        = cfg["max_tokens"].AsInt;
                if (cfg["frequency_penalty"] != null)  cfgFreqPenalty      = cfg["frequency_penalty"].AsFloat;
                if (cfg["presence_penalty"] != null)   cfgPresPenalty      = cfg["presence_penalty"].AsFloat;

                if (cfg["tts_enable"] != null)         ConfigTTSEnable.Value = cfg["tts_enable"].AsBool;
                if (cfg["tts_mode"] != null)           ConfigTTSMode.Value = cfg["tts_mode"].Value;
                if (cfg["tts_provider"] != null)       cfgTTSProvider      = cfg["tts_provider"].Value;
                if (cfg["tts_base_url"] != null)       cfgTTSBaseURL       = cfg["tts_base_url"].Value;
                if (cfg["tts_api_key"] != null)        cfgTTSAPIKey        = cfg["tts_api_key"].Value;
                if (cfg["tts_region"] != null)         cfgTTSRegion        = cfg["tts_region"].Value;
                
                if (cfg["offline_tts_provider"] != null)       cfgOfflineTTSProvider       = cfg["offline_tts_provider"].Value;
                if (cfg["offline_piper_model_path"] != null)   cfgOfflinePiperModelPath    = cfg["offline_piper_model_path"].Value;
                if (cfg["offline_piper_config_path"] != null)  cfgOfflinePiperConfigPath   = cfg["offline_piper_config_path"].Value;

            LogDebug("Config loaded: URL=" + cfgBaseURL + " Model=" + cfgModel + " Temp=" + cfgTemperature);
            LogDebug("Eddie prompt len=" + cfgEddieSystemPrompt.Length + " Elysia=" + cfgElysiaSystemPrompt.Length + " Estelle=" + cfgEstelleSystemPrompt.Length + " Eiona=" + cfgEionaSystemPrompt.Length);
            LogDebug("Eddie post len=" + cfgEddiePostHistoryPrompt.Length + " Elysia=" + cfgElysiaPostHistoryPrompt.Length);
            if (string.IsNullOrEmpty(cfgAPIKey))
                LogDebug("WARNING: API Key is empty! Please set it in the Configurator.");
                
            if (ConfigTTSMode.Value == "Offline" && cfgOfflineTTSProvider == "Kokoro")
            {
                // Auto-route Offline Kokoro to local API
                ConfigTTSMode.Value = "Online";
                cfgTTSProvider = "OpenAI Compatible";
                cfgTTSBaseURL = "http://127.0.0.1:8880/v1";
                LogDebug(">>> Auto-Routed Offline Kokoro to Local API Endpoint.");
                StartKokoroServer();
            }
        }
        catch (Exception ex)
        {
            LogDebug("Error loading config: " + ex.Message);
        }
    }

    private void StartKokoroServer()
    {
        try
        {
            string pythonPath = "python";
            string scriptPath = Path.Combine(Directory.GetParent(pluginDir).FullName, "kokoro_server.py");
            if (File.Exists(scriptPath))
            {
                LogDebug(">>> Starting Kokoro Local Server...");
                System.Diagnostics.ProcessStartInfo start = new System.Diagnostics.ProcessStartInfo();
                start.FileName = pythonPath;
                start.Arguments = string.Format("\"{0}\" --model \"{1}\" --voices \"{2}\" --port 8880", scriptPath, cfgOfflinePiperModelPath, cfgOfflinePiperConfigPath);
                start.UseShellExecute = false;
                start.CreateNoWindow = true;
                
                start.RedirectStandardOutput = true;
                start.RedirectStandardError = true;
                
                kokoroProcess = System.Diagnostics.Process.Start(start);
                kokoroProcess.BeginOutputReadLine();
                kokoroProcess.BeginErrorReadLine();
                
                kokoroProcess.OutputDataReceived += (sender, e) => {
                    if (!string.IsNullOrEmpty(e.Data)) LogDebug("[Kokoro] " + e.Data);
                };
                kokoroProcess.ErrorDataReceived += (sender, e) => {
                    if (!string.IsNullOrEmpty(e.Data)) LogDebug("[Kokoro ERR] " + e.Data);
                };
                
                LogDebug(">>> Kokoro Server Started! PID: " + kokoroProcess.Id);
            }
            else
            {
                LogDebug(">>> Kokoro script not found at " + scriptPath);
            }
        }
        catch (Exception ex)
        {
            LogDebug(">>> Error starting Kokoro server: " + ex.ToString());
        }
    }

    private void OnApplicationQuit()
    {
        if (kokoroProcess != null && !kokoroProcess.HasExited)
        {
            try {
                kokoroProcess.Kill();
                LogDebug(">>> Killed Kokoro Server Process.");
            } catch {}
        }
    }

    private void Awake()
        {
            BepLogger = this.Logger;
            pluginDir  = Path.GetDirectoryName(typeof(UltimateFixPlugin).Assembly.Location);
            string bepinExDir = Directory.GetParent(pluginDir).FullName;
            configPath = Path.Combine(bepinExDir, "config", "AI2U_Config.json");
            logPath    = Path.Combine(bepinExDir, "UltimateFix_Debug.txt");

            ConfigFreqPenalty = Config.Bind("LLM", "FreqPenalty", 0.03f, "Frequency Penalty");
            ConfigPresPenalty = Config.Bind("LLM", "PresPenalty", 0.03f, "Presence Penalty");
            ConfigTTSEnable = Config.Bind("TTS", "Enable", true, "Enable Custom TTS entirely");
            ConfigTTSMode = Config.Bind("TTS", "Mode", "Offline", "TTS Mode: Online or Offline");

            File.WriteAllText(logPath, "=== AI2U Ultimate Protocol v8 Started ===\n");
            LogDebug("Plugin dir: " + pluginDir);
            LogDebug("Config path: " + configPath);

            LoadJsonConfig();

            Communicator.AIModel = ChatGPTConversation.Model.ChatGPTAzure;
            LogDebug("AIModel = ChatGPTAzure");

            GameObject hunter = new GameObject("OmniUIHunter");
            DontDestroyOnLoad(hunter);
            hunter.AddComponent<UIHunter>();

            GameObject ttsManagerObj = new GameObject("AI2UTTSManager");
            DontDestroyOnLoad(ttsManagerObj);
            ttsManagerObj.AddComponent<TTSManager>();

            var harmony = new Harmony("com.omni.ai2ufix");
            
            try
            {
                var prefsType = AccessTools.TypeByName("wAIfuBackend.Prefs");
                if (prefsType != null)
                {
                    var getMethod = AccessTools.PropertyGetter(prefsType, "PlayFabId");
                    var prefix = new HarmonyMethod(typeof(UltimateFixPlugin), "PrefixPlayFabId");
                    harmony.Patch(getMethod, prefix: prefix);
                    LogDebug("Manually patched wAIfuBackend.Prefs.PlayFabId getter");
                }
            }
            catch (Exception ex)
            {
                LogDebug("Failed to patch wAIfuBackend.Prefs: " + ex.Message);
            }

            PlayerPrefs.SetInt("gems", 999999999);
            LogDebug("Offline Shop: Granted 999,999,999 Gems!");

            harmony.PatchAll(typeof(UltimateFixPlugin));
            LogDebug("Harmony Patches Applied!");
        }

        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F10))
            {
                LogDebug("F10 Pressed: Testing LocalTTSManager...");
                try
                {
                    LocalTTSManager tts = UnityEngine.Object.FindObjectOfType<LocalTTSManager>();
                    if (tts != null)
                    {
                        LogDebug("LocalTTSManager found! Forcing speech...");
                        // tts.Speak("Hello master. This is a test of the local TTS engine. If you hear this, it works perfectly!", Character.Eddie);
                    }
                    else
                    {
                        LogDebug("LocalTTSManager NOT found in current scene!");
                    }
                }
                catch (Exception ex)
                {
                    LogDebug("Error triggering LocalTTSManager: " + ex.ToString());
                }
            }
        }

        [HarmonyPatch(typeof(LoginLoadingManager), "StartLoading")]
        [HarmonyPrefix]
        public static bool BypassLoading()
        {
            LogDebug("Bypassing Loading -> MenuState");
            SceneManager.LoadScene("MenuState");
            return false;
        }

        [HarmonyPatch(typeof(LocalTTSManager), "Speak")]
        [HarmonyPrefix]
        public static bool OnLocalTTSManagerSpeak(LocalTTSManager __instance, string Text, Character currentCharacterID)
        {
            try
            {
                if (!ConfigTTSEnable.Value)
                {
                    UltimateFixPlugin.LogDebug(">>> TTS Intercept: TTS is DISABLED completely.");
                    return false;
                }

                if (ConfigTTSMode.Value == "Online")
                {
                    UltimateFixPlugin.LogDebug(">>> TTS Intercept: Redirecting to Custom API TTS.");
                    AudioSource targetSource = null;
                    var playerObj = GameObject.Find("Player");
                    if (playerObj != null) targetSource = playerObj.GetComponent<AudioSource>();
                    if (targetSource == null) targetSource = __instance.gameObject.GetComponent<AudioSource>();
                    if (targetSource == null) targetSource = __instance.gameObject.AddComponent<AudioSource>();
                    
                    TTSManager mgr = TTSManager.Instance;
                    if (mgr == null)
                    {
                        GameObject go = new GameObject("OmniTTSManager");
                        UnityEngine.Object.DontDestroyOnLoad(go);
                        mgr = go.AddComponent<TTSManager>();
                    }
                    mgr.StartTTSCoroutine(Text, targetSource);
                    return false;
                }

                // ===== Offline TTS Engine =====
                UltimateFixPlugin.LogDebug(">>> TTS Intercept: Using Offline TTS Engine.");
                
                var playerField = AccessTools.Field(typeof(LocalTTSManager), "_player");
                var voiceField = AccessTools.Field(typeof(LocalTTSManager), "_voice");
                
                var player = playerField.GetValue(__instance) as LeastSquares.Overtone.TTSPlayer;
                var voice = voiceField.GetValue(__instance) as Assets.Overtone.Scripts.TTSVoice;
                
                // Step 1: Wire up _player from existing component
                if (player == null)
                {
                    player = __instance.gameObject.GetComponent<LeastSquares.Overtone.TTSPlayer>();
                    if (player != null)
                    {
                        playerField.SetValue(__instance, player);
                        UltimateFixPlugin.LogDebug(">>> FIX: Wired _player from existing TTSPlayer component.");
                    }
                    else
                    {
                        UltimateFixPlugin.LogDebug(">>> ERROR: No TTSPlayer component found on GameObject!");
                        return false;
                    }
                }
                
                // Step 2: Wire up _voice from existing component
                if (voice == null)
                {
                    voice = __instance.gameObject.GetComponent<Assets.Overtone.Scripts.TTSVoice>();
                    if (voice != null)
                    {
                        voiceField.SetValue(__instance, voice);
                        UltimateFixPlugin.LogDebug(">>> FIX: Wired _voice from existing TTSVoice component. voiceName=" + voice.voiceName);
                    }
                    else
                    {
                        UltimateFixPlugin.LogDebug(">>> ERROR: No TTSVoice component found on GameObject!");
                        return false;
                    }
                }
                
                // Step 3: Ensure AudioSource exists and is wired to TTSPlayer.sources
                if (player.sources == null || player.sources.Length == 0 || player.sources[0] == null)
                {
                    AudioSource aSrc = __instance.gameObject.GetComponent<AudioSource>();
                    if (aSrc == null)
                    {
                        aSrc = __instance.gameObject.AddComponent<AudioSource>();
                        aSrc.playOnAwake = false;
                        aSrc.volume = 1.0f;
                        UltimateFixPlugin.LogDebug(">>> FIX: Created new AudioSource.");
                    }
                    player.sources = new AudioSource[] { aSrc };
                    UltimateFixPlugin.LogDebug(">>> FIX: Wired AudioSource into TTSPlayer.sources.");
                }
                
                // Step 4: Ensure TTSPlayer.Engine is wired and STARTED
                if (player.Engine == null)
                {
                    var engine = __instance.gameObject.GetComponent<LeastSquares.Overtone.TTSEngine>();
                    if (engine != null)
                    {
                        player.Engine = engine;
                        UltimateFixPlugin.LogDebug(">>> FIX: Wired TTSEngine into TTSPlayer.Engine.");
                    }
                    else
                    {
                        UltimateFixPlugin.LogDebug(">>> ERROR: No TTSEngine component found!");
                        return false;
                    }
                }
                
                if (player.Engine != null && !player.Engine.Loaded)
                {
                    UltimateFixPlugin.LogDebug(">>> FIX: TTSEngine is not loaded. Calling TTSNative.OvertoneStart()...");
                    IntPtr ctx = LeastSquares.Overtone.TTSNative.OvertoneStart();
                    AccessTools.Field(typeof(LeastSquares.Overtone.TTSEngine), "_context").SetValue(player.Engine, ctx);
                    AccessTools.Property(typeof(LeastSquares.Overtone.TTSEngine), "Loaded").SetValue(player.Engine, true, null);
                    UltimateFixPlugin.LogDebug(">>> FIX: TTSEngine native context loaded successfully!");
                }
                
                // Step 5: Ensure TTSPlayer.Voice is wired
                if (player.Voice == null)
                {
                    player.Voice = voice;
                    UltimateFixPlugin.LogDebug(">>> FIX: Wired TTSVoice into TTSPlayer.Voice.");
                }
                
                // Step 6: Set voice name if empty
                if (string.IsNullOrEmpty(voice.voiceName))
                {
                    voice.voiceName = "custom-piper";
                    UltimateFixPlugin.LogDebug(">>> FIX: Set voiceName to " + voice.voiceName);
                }
                
                // Diagnostic: Check engine and voice readiness
                UltimateFixPlugin.LogDebug(">>> DIAG: Engine.Loaded=" + player.Engine.Loaded + " Engine.Disposed=" + player.Engine.Disposed);
                var ctxField = AccessTools.Field(typeof(LeastSquares.Overtone.TTSEngine), "_context");
                if (ctxField != null)
                {
                    var ctxVal = ctxField.GetValue(player.Engine);
                    UltimateFixPlugin.LogDebug(">>> DIAG: Engine._context=" + (ctxVal != null ? ctxVal.ToString() : "NULL"));
                }
                UltimateFixPlugin.LogDebug(">>> DIAG: Voice.voiceName=" + voice.voiceName);
                UltimateFixPlugin.LogDebug(">>> DIAG: Voice.VoiceModel=" + (voice.VoiceModel != null ? "LOADED" : "NOT_LOADED"));
                UltimateFixPlugin.LogDebug(">>> DIAG: sources[0]=" + (player.sources[0] != null ? "vol=" + player.sources[0].volume : "NULL"));
                
                // Step 7: Bypass stripped TTSPlayer.Speak and generate manually!
                UltimateFixPlugin.LogDebug(">>> DIAG: Generating audio manually via Native P/Invoke...");
                
                var finalCtxField = AccessTools.Field(typeof(LeastSquares.Overtone.TTSEngine), "_context");
                IntPtr finalCtx = (IntPtr)finalCtxField.GetValue(player.Engine);
                
                IntPtr voicePtr = voice.VoiceModel.Pointer;

                if (cfgOfflineTTSProvider == "Piper" && !string.IsNullOrEmpty(cfgOfflinePiperModelPath) && File.Exists(cfgOfflinePiperModelPath))
                {
                    if (cachedVoicePtr == IntPtr.Zero || cachedVoiceModelPath != cfgOfflinePiperModelPath)
                    {
                        UltimateFixPlugin.LogDebug(">>> LOADING CUSTOM PIPER VOICE FROM DISK: " + cfgOfflinePiperModelPath);
                        byte[] configBytes = File.ReadAllBytes(cfgOfflinePiperConfigPath);
                        byte[] modelBytes = File.ReadAllBytes(cfgOfflinePiperModelPath);
                        
                        IntPtr configPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(configBytes.Length);
                        System.Runtime.InteropServices.Marshal.Copy(configBytes, 0, configPtr, configBytes.Length);
                        IntPtr modelPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(modelBytes.Length);
                        System.Runtime.InteropServices.Marshal.Copy(modelBytes, 0, modelPtr, modelBytes.Length);
                        
                        cachedVoicePtr = LeastSquares.Overtone.TTSNative.OvertoneLoadVoice(configPtr, (uint)configBytes.Length, modelPtr, (uint)modelBytes.Length);
                        cachedVoiceModelPath = cfgOfflinePiperModelPath;
                        
                        System.Runtime.InteropServices.Marshal.FreeHGlobal(configPtr);
                        System.Runtime.InteropServices.Marshal.FreeHGlobal(modelPtr);
                        UltimateFixPlugin.LogDebug(">>> CUSTOM PIPER VOICE CACHED IN UNMANAGED MEMORY.");
                    }
                    voicePtr = cachedVoicePtr;
                }
                else
                {
                    UltimateFixPlugin.LogDebug(">>> USING DEFAULT OVERTONE VOICE.");
                }
                
                byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(Text + "\0");
                IntPtr textPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(utf8Bytes.Length);
                System.Runtime.InteropServices.Marshal.Copy(utf8Bytes, 0, textPtr, utf8Bytes.Length);
                
                var res = LeastSquares.Overtone.TTSNative.OvertoneText2Audio(finalCtx, textPtr, voicePtr);
                
                System.Runtime.InteropServices.Marshal.FreeHGlobal(textPtr);
                
                if (res.Samples != IntPtr.Zero && res.LengthSamples > 0)
                {
                    UltimateFixPlugin.LogDebug(">>> DIAG: Native TTS returned " + res.LengthSamples + " samples. Channels: " + res.Channels + ", Rate: " + res.SampleRate);
                    
                    // Native TTS engine (Piper/Overtone) outputs 16-bit PCM (shorts), NOT 32-bit floats!
                    // If we read floats, we read past the buffer, causing Alien Sounds & Access Violation Crashes!
                    int totalSamples = (int)(res.LengthSamples * res.Channels);
                    short[] pcmData = new short[totalSamples];
                    
                    // Safely copy the 16-bit shorts from native memory
                    System.Runtime.InteropServices.Marshal.Copy(res.Samples, pcmData, 0, totalSamples);
                    LeastSquares.Overtone.TTSNative.OvertoneFreeResult(res);
                    
                    // Convert 16-bit PCM to Unity's expected float format (-1.0f to 1.0f)
                    float[] floatSamples = new float[totalSamples];
                    for (int i = 0; i < totalSamples; i++)
                    {
                        floatSamples[i] = pcmData[i] / 32768f;
                    }
                    
                    AudioClip clip = AudioClip.Create("OfflineTTS", (int)res.LengthSamples, (int)res.Channels, (int)res.SampleRate, false);
                    clip.SetData(floatSamples, 0);
                    
                    int currentNPCID = 0;
                    if (currentCharacterID == Character.MagicCircle) currentNPCID = 1;
                    
                    player.sources[currentNPCID].clip = clip;
                    player.sources[currentNPCID].Play();
                    UltimateFixPlugin.LogDebug(">>> SUCCESS: Offline audio is playing!");
                }
                else
                {
                    UltimateFixPlugin.LogDebug(">>> ERROR: Native TTS generated 0 samples or returned NULL!");
                }
                
                return false; // Skip original to prevent NullRef on outputAudioMixerGroup
            }
            catch (Exception ex)
            {
                UltimateFixPlugin.LogDebug(">>> HARMONY INTERCEPT ERROR: " + ex.ToString());
                return false;
            }
        }

        [HarmonyPatch(typeof(LevelManager_HubWorld), "Start")]
        [HarmonyPostfix]
        public static void StealTTS(LevelManager_HubWorld __instance)
        {
            try
            {
                // ===== COMPREHENSIVE CHAPTER UNLOCK =====
                // 1. Force all endings as unlocked (need >= 3 per level for NextLevelUnlocked_Ending)
                for (int e = 0; e < GlobalSettings.endingUnlock_L1.Length; e++) GlobalSettings.endingUnlock_L1[e] = true;
                for (int e = 0; e < GlobalSettings.endingUnlock_L2.Length; e++) GlobalSettings.endingUnlock_L2[e] = true;
                for (int e = 0; e < GlobalSettings.endingUnlock_L3.Length; e++) GlobalSettings.endingUnlock_L3[e] = true;
                for (int e = 0; e < GlobalSettings.endingUnlock_L4.Length; e++) GlobalSettings.endingUnlock_L4[e] = true;

                // 2. Force all character names as set (prevents first-time name entry screen)
                for (int n = 0; n < GlobalSettings.nameSet.Length; n++) GlobalSettings.nameSet[n] = true;

                // 3. Force all levels unlocked
                for (int i = 0; i <= 5; i++) {
                    if (i < LevelData_HubWorld.levelUnlockedStatus.Count)
                        LevelData_HubWorld.levelUnlockedStatus[i] = true;
                }

                // 4. Force FavorMeter FM to max on all characters via reflection
                var configsField = AccessTools.Field(typeof(LevelManager_HubWorld), "characterConfigs");
                if (configsField != null) {
                    var configs = configsField.GetValue(__instance) as CharacterConfig[];
                    if (configs != null) {
                        foreach (var cfg in configs) {
                            if (cfg != null && cfg.favorMeters != null) {
                                cfg.favorMeters.FM = 999;
                                UltimateFixPlugin.LogDebug(">>> Set FM=999 for level " + cfg.levelIndex);
                            }
                        }
                    }
                }

                // 5. Enable all level doors and stairs
                var enableMethod = AccessTools.Method(typeof(LevelManager_HubWorld), "EnableLevel");
                if (enableMethod != null) {
                    for (int lvl = 1; lvl <= 4; lvl++) {
                        try { enableMethod.Invoke(__instance, new object[]{ lvl }); } catch {}
                    }
                }
                
                // 6. Disable all cutscenes
                string[] cutsceneKeys = { "level1Cutscene", "level2Cutscene", "level3Cutscene", "level4Cutscene",
                    "PhoneBoothCutscene", "tutorialCutscene", "neutralCutscene", "badCutscene", "goodCutscene" };
                foreach (var key in cutsceneKeys) {
                    if (!LevelData_HubWorld.cutsceneToggle.ContainsKey(key))
                        LevelData_HubWorld.cutsceneToggle.Add(key, true);
                    else
                        LevelData_HubWorld.cutsceneToggle[key] = true;
                }

                // 7. Update visual state
                var updateMethod = AccessTools.Method(typeof(LevelManager_HubWorld), "UpdateLevelUnlockStatus");
                if (updateMethod != null) updateMethod.Invoke(__instance, null);
                UltimateFixPlugin.LogDebug(">>> HARMONY INTERCEPT: Unlocked ALL chapters! (endings, FM, names, cutscenes all forced)");
                
                var ttsField = AccessTools.Field(typeof(LevelManager_HubWorld), "localTTSManager");
                LocalTTSManager tts = (LocalTTSManager)ttsField.GetValue(__instance);
                
                if (tts != null)
                {
                    UltimateFixPlugin.LogDebug(">>> HARMONY INTERCEPT: LevelManager_HubWorld initialized LocalTTSManager!");
                    if (!tts.gameObject.activeInHierarchy)
                    {
                        UltimateFixPlugin.LogDebug(">>> It was INACTIVE! Forcing it to active...");
                        tts.gameObject.SetActive(true);
                    }
                    UltimateFixPlugin.LogDebug(">>> Forcing speech...");
                    // tts.Speak("Hello master. Harmony interception successful. The offline engine is completely under our control.", Character.Eddie);
                }
                else
                {
                    UltimateFixPlugin.LogDebug(">>> HARMONY INTERCEPT: localTTSManager is NULL inside LevelManager_HubWorld!");
                }
            }
            catch (Exception ex)
            {
                UltimateFixPlugin.LogDebug(">>> HARMONY INTERCEPT ERROR: " + ex.ToString());
            }
        }
        // ─────────────────────────────────────────────────────────────────────
        // Prevent CharacterConfig.LoadNPCData from overwriting levelUnlockedStatus
        [HarmonyPatch(typeof(CharacterConfig), "LoadNPCData")]
        [HarmonyPostfix]
        public static void PostfixLoadNPCData(CharacterConfig __instance)
        {
            // After LoadNPCData runs, it recalculates levelUnlockedStatus from FavorMeter.
            // We force it back to true so unlock is never reverted.
            if (__instance.levelIndex >= 0 && __instance.levelIndex < LevelData_HubWorld.levelUnlockedStatus.Count)
            {
                LevelData_HubWorld.levelUnlockedStatus[__instance.levelIndex] = true;
                UltimateFixPlugin.LogDebug(">>> PostfixLoadNPCData: Forced levelUnlockedStatus[" + __instance.levelIndex + "] = true");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Redirect GameState_L4 to GameState_L4_MVP1 (dev scene name mismatch)
        [HarmonyPatch(typeof(GameManager), "GoToLevel")]
        [HarmonyPrefix]
        public static void PrefixGoToLevel(ref string nextLevel)
        {
            UltimateFixPlugin.LogDebug(">>> GoToLevel intercepted: " + nextLevel);
            if (nextLevel == "GameState_L4")
            {
                UltimateFixPlugin.LogDebug(">>> Redirecting GameState_L4 to GameState_L4_MVP1");
                nextLevel = "GameState_L4_MVP1";
            }
        }

        // ─────────────────────────────────────────────────────────────────────

        [HarmonyPatch(typeof(wAIfuPlayFab), "ConsumeToken")]
        [HarmonyPrefix]
        public static bool InfiniteTokens(ref bool __result)
        {
            __result = true;
            return false;
        }

        // ─────────────────────────────────────────────────────────────────────
        // FIX 2: Redirect URL to OpenRouter
        // ─────────────────────────────────────────────────────────────────────

        [HarmonyPatch(typeof(ChatGPTConversation), "SendToChatGPT",
            new Type[] { typeof(string), typeof(Action<string, int>) })]
        [HarmonyPrefix]
        public static void RedirectRequest2(ChatGPTConversation __instance)
        {
            try
            {
                var uriField = AccessTools.Field(typeof(ChatGPTConversation), "_uri");
                if (uriField != null) uriField.SetValue(__instance, new Uri(cfgBaseURL));
                LogDebug("Redirected _uri -> " + cfgBaseURL);
            }
            catch (Exception ex) { LogDebug("Error RedirectRequest2: " + ex.Message); }
        }

        [HarmonyPatch(typeof(ChatGPTConversation), "SendToChatGPT",
            new Type[] { typeof(string), typeof(Action<string, int>), typeof(string), typeof(EnvisionType) })]
        [HarmonyPrefix]
        public static void RedirectRequest4(ChatGPTConversation __instance)
        {
            try
            {
                var uriField = AccessTools.Field(typeof(ChatGPTConversation), "_uriEnvision");
                if (uriField != null) uriField.SetValue(__instance, new Uri(cfgBaseURL));
            }
            catch (Exception ex) { LogDebug("Error RedirectRequest4: " + ex.Message); }
        }

        // ─────────────────────────────────────────────────────────────────────
        // FIX 2B: Clean null headers + inject auth AFTER UpdateRequestHeaders
        // ─────────────────────────────────────────────────────────────────────

        [HarmonyPatch(typeof(ChatGPTConversation), "UpdateRequestHeaders")]
        [HarmonyPostfix]
        public static void FixNullHeaders(ChatGPTConversation __instance)
        {
            try
            {
                var headersField = AccessTools.Field(typeof(ChatGPTConversation), "_reqHeaders");
                if (headersField == null) return;
                var headers = headersField.GetValue(__instance) as Dictionary<string, string>;
                if (headers == null) return;

                var keys = new List<string>(headers.Keys);
                foreach (string key in keys)
                    if (headers[key] == null) headers[key] = "";

                headers["Authorization"] = "Bearer " + cfgAPIKey;
                headers["Content-Type"]  = "application/json";
            }
            catch (Exception ex) { LogDebug("Error FixNullHeaders: " + ex.Message); }
        }

        // ─────────────────────────────────────────────────────────────────────
        // FIX 2C: Safe SetHeaders (skip null values)
        // ─────────────────────────────────────────────────────────────────────

        [HarmonyPatch(typeof(Requests), "SetHeaders")]
        [HarmonyPrefix]
        public static bool SafeSetHeaders(ref UnityWebRequest req, Dictionary<string, string> headers)
        {
            try
            {
                if (headers == null) return false;
                foreach (var kvp in headers)
                    if (kvp.Key != null && kvp.Value != null)
                        req.SetRequestHeader(kvp.Key, kvp.Value);
                return false;
            }
            catch (Exception ex) { LogDebug("Error SafeSetHeaders: " + ex.Message); return false; }
        }

        // ─────────────────────────────────────────────────────────────────────
        // FIX 3: Inject model, system prompt, post-history prompt, remove stop
        // ─────────────────────────────────────────────────────────────────────

        [HarmonyPatch(typeof(UnityWebRequest), "SendWebRequest")]
        [HarmonyPrefix]
        public static void TransformPayload(UnityWebRequest __instance)
        {
            try
            {
                if (__instance == null) return;
                string url = __instance.url;
                LogDebug("SendWebRequest: " + url);

                if (!url.Contains("openrouter.ai") && !url.Contains(cfgBaseURL.Replace("https://", "").Split('/')[0]))
                    return;

                if (__instance.uploadHandler == null) return;
                byte[] data = __instance.uploadHandler.data;
                if (data == null || data.Length == 0) return;

                string json = Encoding.UTF8.GetString(data);

                if (!json.Contains("\"messages\"")) return;

                LogDebug("Original payload length: " + json.Length);

                // Parse JSON to modify it
                JSONNode root = JSON.Parse(json);
                if (root == null) return;

                // Inject model
                root["model"] = cfgModel;

                // Override parameters from config
                root["temperature"]      = cfgTemperature;
                root["max_tokens"]        = cfgMaxTokens;
                root["top_p"]             = cfgTopP;
                root["frequency_penalty"] = cfgFreqPenalty;
                root["presence_penalty"]  = cfgPresPenalty;

                if (cfgTopK > 0) root["top_k"] = cfgTopK;

                // Remove empty stop field
                if (root["stop"] != null)
                {
                    string stopVal = root["stop"].Value;
                    if (string.IsNullOrEmpty(stopVal))
                        root.Remove("stop");
                }
                
                // Removed local function InjectTags

                // Inject system prompt if configured
                JSONArray messages = root["messages"].AsArray;
                string activePrompt = "";
                bool isHubWorld = (GameManager.CurrentLevel == 0);
                
                if (isHubWorld) {
                    activePrompt = UltimateFixPlugin.GetActiveHubWorldPrompt();
                } else {
                    activePrompt = UltimateFixPlugin.GetActiveSystemPrompt();
                }
                
                // --- DYNAMIC INJECTION FOR {Stage}, {Memories} AND ALL VARIABLES ---
                try
                {
                    if (!string.IsNullOrEmpty(activePrompt))
                    {
                        CustomPrompt cp = null;
                        var cps = Resources.FindObjectsOfTypeAll<CustomPrompt>();
                        if (cps != null && cps.Length > 0)
                            cp = cps[0];
                            
                        ServerContext sc = ServerContext.CurrentServerContext;
                        Character activeChar = UltimateFixPlugin.GetActiveCharacter();
                        if (cp != null && sc != null)
                        {
                            int level = sc.Level;
                            int stage = sc.Stage;
                            System.Collections.Generic.List<int> memories = sc.Memories;
                            
                            System.Collections.Generic.List<string> stageList = null;
                            System.Collections.Generic.List<string> memoriesList = null;
                            
                            if (level == 1) {
                                stageList = (System.Collections.Generic.List<string>)typeof(CustomPrompt).GetField("stage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cp);
                                memoriesList = (System.Collections.Generic.List<string>)typeof(CustomPrompt).GetField("memories", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cp);
                            }
                            else if (level == 2) {
                                stageList = (System.Collections.Generic.List<string>)typeof(CustomPrompt).GetField("stageL2", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cp);
                                memoriesList = (System.Collections.Generic.List<string>)typeof(CustomPrompt).GetField("memoriesL2", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cp);
                            }
                            else if (level == 3) {
                                stageList = (System.Collections.Generic.List<string>)typeof(CustomPrompt).GetField("stageL3", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cp);
                                memoriesList = (System.Collections.Generic.List<string>)typeof(CustomPrompt).GetField("memoriesL3", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(cp);
                            }
                            
                            if (stageList != null && stage >= 0 && stage < stageList.Count)
                                activePrompt = activePrompt.Replace("{Stage}", stageList[stage]);
                            else
                                activePrompt = activePrompt.Replace("{Stage}", "");
                                
                            if (memoriesList != null && memories != null)
                            {
                                string memoryText = "";
                                foreach (int memIdx in memories)
                                {
                                    if (memIdx >= 0 && memIdx < memoriesList.Count)
                                        memoryText += memoriesList[memIdx] + "\n";
                                }
                                activePrompt = activePrompt.Replace("{Memories}", memoryText.TrimEnd());
                            }
                            else
                            {
                                activePrompt = activePrompt.Replace("{Memories}", "");
                            }

                            // Replace Base Variables
                            activePrompt = activePrompt.Replace("{npcName}", sc.Her ?? "");
                            activePrompt = activePrompt.Replace("{playerName}", sc.Him ?? "");
                            var langTuple = CustomPrompt.NPCSettings.GetLanguageCodeAndReplyTip(cp.Language);
                            activePrompt = activePrompt.Replace("{language}", langTuple.Item1 + "\n" + langTuple.Item2);

                            // Replace Level-Specific Variables
                            if (level == 1) {
                                string[] per = UltimateFixPlugin.cfgEddiePersonalities;
                                string[] hob = UltimateFixPlugin.cfgEddieHobbies;
                                if (activeChar == Character.Eiona) {
                                    per = UltimateFixPlugin.cfgEionaPersonalities;
                                    hob = UltimateFixPlugin.cfgEionaHobbies;
                                }
                                activePrompt = activePrompt.Replace("{CharId}", UltimateFixPlugin.InjectTags(cp.npcSettingsL1.GenerateNPCSettingsString() ?? "", per, hob));
                                activePrompt = activePrompt.Replace("{GeneratedRoom}", ServerContextL1.CurrentServerContext.GeneratedRoom ?? "");
                                activePrompt = activePrompt.Replace("{OutsideArea}", ServerContextL1.CurrentServerContext.OutsideArea ?? "");
                                activePrompt = activePrompt.Replace("{PcPwd}", ServerContextL1.CurrentServerContext.PcPswd ?? "");
                                activePrompt = activePrompt.Replace("{WifiPwd}", ServerContextL1.CurrentServerContext.WifiPswd ?? "");
                                activePrompt = activePrompt.Replace("{SecretPwd}", ServerContextL1.CurrentServerContext.SecretPswd ?? "");
                            }
                            else if (level == 2) {
                                activePrompt = activePrompt.Replace("{CharId}", UltimateFixPlugin.InjectTags(cp.npcSettingsL2.GenerateNPCSettingsString() ?? "", UltimateFixPlugin.cfgElysiaPersonalities, UltimateFixPlugin.cfgElysiaHobbies));
                                activePrompt = activePrompt.Replace("{Location}", ServerContextL2.CurrentServerContext.Location ?? "");
                                activePrompt = activePrompt.Replace("{Recipe}", ServerContextL2.CurrentServerContext.Recipe ?? "");
                                activePrompt = activePrompt.Replace("{SpeedColor}", ServerContextL2.CurrentServerContext.SpeedColor ?? "");
                                activePrompt = activePrompt.Replace("{HealthColor}", ServerContextL2.CurrentServerContext.HealthColor ?? "");
                                activePrompt = activePrompt.Replace("{ShieldColor}", ServerContextL2.CurrentServerContext.ShieldColor ?? "");
                                activePrompt = activePrompt.Replace("{LoveColor}", ServerContextL2.CurrentServerContext.LoveColor ?? "");
                                // Also handle addon1/2/3/4 if they exist
                                activePrompt = activePrompt.Replace("{addon1}", "Speed Potion");
                                activePrompt = activePrompt.Replace("{addon2}", "Health Potion");
                                activePrompt = activePrompt.Replace("{addon3}", "Shield Potion");
                                activePrompt = activePrompt.Replace("{addon4}", "Love Potion");
                            }
                            else if (level == 3) {
                                activePrompt = activePrompt.Replace("{CharId}", UltimateFixPlugin.InjectTags(cp.npcSettingsL3.GenerateNPCSettingsString() ?? "", UltimateFixPlugin.cfgEstellePersonalities, UltimateFixPlugin.cfgEstelleHobbies));
                                activePrompt = activePrompt.Replace("{SecurityLevel}", ServerContextL3.CurrentServerContext.SecLvl.HasValue ? ServerContextL3.CurrentServerContext.SecLvl.Value.ToString() : "");
                                activePrompt = activePrompt.Replace("{DarkRoom}", ServerContextL3.CurrentServerContext.DarkRoom ?? "");
                                activePrompt = activePrompt.Replace("{Location}", ServerContextL3.CurrentServerContext.Location ?? "");
                                activePrompt = activePrompt.Replace("{Year}", ServerContextL3.CurrentServerContext.Year ?? "");
                                activePrompt = activePrompt.Replace("{Gift}", ServerContextL3.CurrentServerContext.Gift ?? "");
                                activePrompt = activePrompt.Replace("{FixedSystems}", ServerContextL3.CurrentServerContext.FixedSystems ?? "");
                                activePrompt = activePrompt.Replace("{UnfixedSystems}", ServerContextL3.CurrentServerContext.UnFixedSystems ?? "");
                                activePrompt = activePrompt.Replace("{Cards}", ServerContextL3.CurrentServerContext.Cards ?? "");
                            }

                            LogDebug("Dynamically replaced ALL variables ({Stage}, {Memories}, etc.) using CustomPrompt data!");
                        }
                        else
                        {
                            LogDebug("Could NOT dynamically replace variables via CustomPrompt (cp is null). Using Fallback Hardcoded data!");
                            
                            // Detect level from active character since ServerContext.CurrentServerContext 
                            // is the base singleton and has Level=0. The actual data lives in ServerContextL1/L2/L3.
                            int level = 0;
                            switch (activeChar)
                            {
                                case Character.Eddie: level = 1; break;
                                case Character.Elysia: level = 2; break;
                                case Character.Estelle: level = 3; break;
                                case Character.Eiona: level = 1; break; // Eiona uses L1 layout
                                default: level = 1; break;
                            }
                            LogDebug("Fallback: Detected level=" + level + " from activeChar=" + activeChar);
                            
                            // Get the correct ServerContext for this level
                            ServerContext scLevel = null;
                            if (level == 1) scLevel = ServerContextL1.CurrentServerContext;
                            else if (level == 2) scLevel = ServerContextL2.CurrentServerContext;
                            else if (level == 3) scLevel = ServerContextL3.CurrentServerContext;
                            
                            // Fallback Base Variables - use scLevel which has actual data
                            if (scLevel != null) {
                                LogDebug("Fallback: scLevel.Her=" + (scLevel.Her ?? "NULL") + " scLevel.Him=" + (scLevel.Him ?? "NULL") + " scLevel.Level=" + scLevel.Level);
                                activePrompt = activePrompt.Replace("{npcName}", scLevel.Her ?? "");
                                activePrompt = activePrompt.Replace("{playerName}", scLevel.Him ?? "");
                            } else {
                                LogDebug("Fallback: scLevel is NULL for level=" + level + "! Cannot get Her/Him names.");
                                // Hard-code character names as absolute last resort
                                switch (activeChar)
                                {
                                    case Character.Eddie: 
                                        activePrompt = activePrompt.Replace("{npcName}", "Eddie");
                                        activePrompt = activePrompt.Replace("{playerName}", "Player");
                                        break;
                                    case Character.Elysia: 
                                        activePrompt = activePrompt.Replace("{npcName}", "Elysia");
                                        activePrompt = activePrompt.Replace("{playerName}", "Player");
                                        break;
                                    case Character.Estelle: 
                                        activePrompt = activePrompt.Replace("{npcName}", "Estelle");
                                        activePrompt = activePrompt.Replace("{playerName}", "Player");
                                        break;
                                    case Character.Eiona: 
                                        activePrompt = activePrompt.Replace("{npcName}", "Eiona");
                                        activePrompt = activePrompt.Replace("{playerName}", "Player");
                                        break;
                                }
                            }
                            activePrompt = activePrompt.Replace("{language}", "en-US\nFor npc_reply_to_player, it's a string for the character's reply");

                            if (level == 1) {
                                activePrompt = activePrompt.Replace("{Stage}", "The player just woke up in your apartment. You have locked them inside for their own safety.");
                                activePrompt = activePrompt.Replace("{Memories}", "You found the player unconscious outside and brought them in to take care of them.");
                                
                                string[] per = UltimateFixPlugin.cfgEddiePersonalities;
                                string[] hob = UltimateFixPlugin.cfgEddieHobbies;
                                string defaultCharId = "<Name>Eddie\n<Age>20\n<Character>Yandere";
                                if (activeChar == Character.Eiona) {
                                    per = UltimateFixPlugin.cfgEionaPersonalities;
                                    hob = UltimateFixPlugin.cfgEionaHobbies;
                                    defaultCharId = "<Name>Eiona\n<Age>Unknown\n<Character>Dark Siren";
                                }
                                activePrompt = activePrompt.Replace("{CharId}", UltimateFixPlugin.InjectTags(defaultCharId, per, hob));
                                
                                var scL1 = ServerContextL1.CurrentServerContext;
                                if (scL1 != null) {
                                    activePrompt = activePrompt.Replace("{GeneratedRoom}", scL1.GeneratedRoom ?? "storage_room");
                                    activePrompt = activePrompt.Replace("{OutsideArea}", scL1.OutsideArea ?? "balcony");
                                    activePrompt = activePrompt.Replace("{PcPwd}", scL1.PcPswd ?? "1234");
                                    activePrompt = activePrompt.Replace("{WifiPwd}", scL1.WifiPswd ?? "password");
                                    activePrompt = activePrompt.Replace("{SecretPwd}", scL1.SecretPswd ?? "secret");
                                } else {
                                    activePrompt = activePrompt.Replace("{GeneratedRoom}", "storage_room");
                                    activePrompt = activePrompt.Replace("{OutsideArea}", "balcony");
                                    activePrompt = activePrompt.Replace("{PcPwd}", "1234");
                                    activePrompt = activePrompt.Replace("{WifiPwd}", "password");
                                    activePrompt = activePrompt.Replace("{SecretPwd}", "secret");
                                }
                            }
                            else if (level == 2) {
                                activePrompt = activePrompt.Replace("{Stage}", "The player has stumbled into your magical forest cabin. You want to keep them here forever.");
                                activePrompt = activePrompt.Replace("{Memories}", "You have lived alone for centuries. Finally, someone has arrived.");
                                activePrompt = activePrompt.Replace("{CharId}", UltimateFixPlugin.InjectTags("<Name>Elysia\n<Age>200\n<Character>Witch", UltimateFixPlugin.cfgElysiaPersonalities, UltimateFixPlugin.cfgElysiaHobbies));
                                var scL2 = ServerContextL2.CurrentServerContext;
                                if (scL2 != null) {
                                    activePrompt = activePrompt.Replace("{Location}", scL2.Location ?? "Deep Forest");
                                    activePrompt = activePrompt.Replace("{Recipe}", scL2.Recipe ?? "Love Potion");
                                    activePrompt = activePrompt.Replace("{SpeedColor}", scL2.SpeedColor ?? "Blue");
                                    activePrompt = activePrompt.Replace("{HealthColor}", scL2.HealthColor ?? "Red");
                                    activePrompt = activePrompt.Replace("{ShieldColor}", scL2.ShieldColor ?? "Yellow");
                                    activePrompt = activePrompt.Replace("{LoveColor}", scL2.LoveColor ?? "Pink");
                                    activePrompt = activePrompt.Replace("{PotionColors}", "Speed=" + (scL2.SpeedColor ?? "Blue") + ", Health=" + (scL2.HealthColor ?? "Red") + ", Shield=" + (scL2.ShieldColor ?? "Yellow") + ", Love=" + (scL2.LoveColor ?? "Pink"));
                                } else {
                                    activePrompt = activePrompt.Replace("{Location}", "Deep Forest");
                                    activePrompt = activePrompt.Replace("{Recipe}", "Love Potion");
                                    activePrompt = activePrompt.Replace("{SpeedColor}", "Blue");
                                    activePrompt = activePrompt.Replace("{HealthColor}", "Red");
                                    activePrompt = activePrompt.Replace("{ShieldColor}", "Yellow");
                                    activePrompt = activePrompt.Replace("{LoveColor}", "Pink");
                                    activePrompt = activePrompt.Replace("{PotionColors}", "Speed=Blue, Health=Red, Shield=Yellow, Love=Pink");
                                }
                                activePrompt = activePrompt.Replace("{addon1}", "Speed Potion");
                                activePrompt = activePrompt.Replace("{addon2}", "Health Potion");
                                activePrompt = activePrompt.Replace("{addon3}", "Shield Potion");
                                activePrompt = activePrompt.Replace("{addon4}", "Love Potion");
                                activePrompt = activePrompt.Replace("{GeneratedRoom}", "garden");
                                activePrompt = activePrompt.Replace("{OutsideArea}", "forest_clearing");
                            }
                            else if (level == 3) {
                                activePrompt = activePrompt.Replace("{Stage}", "The player has awakened you, the ship's AI hologram, from a long slumber.");
                                activePrompt = activePrompt.Replace("{Memories}", "You vaguely remember the past crew, but now only the player remains.");
                                activePrompt = activePrompt.Replace("{CharId}", UltimateFixPlugin.InjectTags("<Name>Estelle\n<Age>999\n<Character>AI Hologram", UltimateFixPlugin.cfgEstellePersonalities, UltimateFixPlugin.cfgEstelleHobbies));
                                var scL3 = ServerContextL3.CurrentServerContext;
                                if (scL3 != null) {
                                    activePrompt = activePrompt.Replace("{SecurityLevel}", scL3.SecLvl.HasValue ? scL3.SecLvl.Value.ToString() : "5");
                                    activePrompt = activePrompt.Replace("{DarkRoom}", scL3.DarkRoom ?? "Cargo Bay");
                                    activePrompt = activePrompt.Replace("{Location}", scL3.Location ?? "Cryo Chamber");
                                    activePrompt = activePrompt.Replace("{Year}", scL3.Year ?? "2042");
                                    activePrompt = activePrompt.Replace("{Gift}", scL3.Gift ?? "Alien Artifact");
                                    activePrompt = activePrompt.Replace("{FixedSystems}", scL3.FixedSystems ?? "Life Support");
                                    activePrompt = activePrompt.Replace("{UnfixedSystems}", scL3.UnFixedSystems ?? "Engines");
                                    activePrompt = activePrompt.Replace("{Cards}", scL3.Cards ?? "Access Card");
                                } else {
                                    activePrompt = activePrompt.Replace("{SecurityLevel}", "5");
                                    activePrompt = activePrompt.Replace("{DarkRoom}", "Cargo Bay");
                                    activePrompt = activePrompt.Replace("{Location}", "Cryo Chamber");
                                    activePrompt = activePrompt.Replace("{Year}", "2042");
                                    activePrompt = activePrompt.Replace("{Gift}", "Alien Artifact");
                                    activePrompt = activePrompt.Replace("{FixedSystems}", "Life Support");
                                    activePrompt = activePrompt.Replace("{UnfixedSystems}", "Engines");
                                    activePrompt = activePrompt.Replace("{Cards}", "Access Card");
                                }
                                activePrompt = activePrompt.Replace("{GeneratedRoom}", "observation_deck");
                                activePrompt = activePrompt.Replace("{OutsideArea}", "airlock");
                            }
                            
                            LogDebug("Dynamically replaced {Stage}, {Memories} using Fallback data!");
                        }
                    }
                }
                catch (Exception dynEx)
                {
                    LogDebug("Failed to dynamically replace variables: " + dynEx.Message);
                }
                // ----------------------------------------------------

                LogDebug("Active character: " + UltimateFixPlugin.GetActiveCharacter().ToString() + " | Prompt length: " + (activePrompt != null ? activePrompt.Length.ToString() : "NULL"));

                // Fetch Native Chat History
                JSONArray chatHistoryArray = new JSONArray();
                try
                {
                    Communicator comm = UnityEngine.Object.FindObjectOfType<Communicator>();
                    if (comm != null)
                    {
                        var chatField = AccessTools.Field(typeof(Communicator), "_chat");
                        var chatObj = (chatField != null) ? chatField.GetValue(comm) : null;
                        var currentChatField = chatObj != null ? AccessTools.Field(chatObj.GetType(), "_currentChat") : null;
                        var currentChat = (currentChatField != null && chatObj != null) ? currentChatField.GetValue(chatObj) as System.Collections.IList : null;
                        if (currentChat != null)
                        {
                            foreach (var msgObj in currentChat)
                            {
                                var role = AccessTools.Field(msgObj.GetType(), "role").GetValue(msgObj) as string;
                                var content = AccessTools.Field(msgObj.GetType(), "content").GetValue(msgObj) as string;
                                if (role != "system")
                                {
                                    JSONNode histNode = JSON.Parse("{}");
                                    histNode["role"] = role;
                                    histNode["content"] = content;
                                    chatHistoryArray.Add(histNode);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) { LogDebug("Error fetching history: " + ex.Message); }

                if (messages != null && messages.Count > 0 && !string.IsNullOrEmpty(activePrompt))
                {
                    int startIndex = 0;
                    if (messages[0]["role"].Value == "system") startIndex = 1;

                    JSONArray newMessages = new JSONArray();
                    JSONNode sysMsg = JSON.Parse("{}");
                    sysMsg["role"] = "system";
                    
                    if (isHubWorld)
                    {
                        string gamePrompt = messages[0]["content"].Value;
                        sysMsg["content"] = activePrompt + "\n\n" + gamePrompt;
                    }
                    else
                    {
                        sysMsg["content"] = activePrompt;
                    }
                    
                    newMessages.Add(sysMsg);

                    // Add history
                    for (int i = 0; i < chatHistoryArray.Count; i++)
                    {
                        // Skip the very last user message as it is already embedded in the JSON state sent by the game
                        if (i == chatHistoryArray.Count - 1 && chatHistoryArray[i]["role"].Value == "user") continue;
                        newMessages.Add(chatHistoryArray[i]);
                    }

                    // Add remaining messages (the game's state JSON and post history prompt)
                    for (int i = startIndex; i < messages.Count; i++)
                    {
                        newMessages.Add(messages[i]);
                    }

                    root["messages"] = newMessages;
                    LogDebug("Injected system prompt and " + chatHistoryArray.Count + " history messages.");
                }

                // Inject post-history prompt (append as last user message)
                string activePost = UltimateFixPlugin.GetActivePostPrompt();
                if (!string.IsNullOrEmpty(activePost))
                {
                    JSONArray msgs = root["messages"].AsArray;
                    JSONNode postMsg = JSON.Parse("{}");
                    postMsg["role"]    = "system";
                    postMsg["content"] = activePost;
                    msgs.Add(postMsg);
                    LogDebug("Appended character-specific post-history prompt.");
                }

                // Log the final payload so the user can verify {Stage} and {Memories} AND see the history
                if (root["messages"] != null && root["messages"].Count > 0)
                {
                    LogDebug("=== SYSTEM PROMPT SENT TO AI ===");
                    LogDebug(root["messages"][0]["content"].Value);
                    LogDebug("=================================");
                    LogDebug("=== CONVERSATION HISTORY SENT ===");
                    for (int i = 1; i < root["messages"].Count; i++)
                    {
                        LogDebug("ROLE: " + root["messages"][i]["role"].Value);
                        LogDebug("CONTENT: " + root["messages"][i]["content"].Value);
                        LogDebug("---");
                    }
                    LogDebug("=================================");
                }

                json = root.ToString();
                LogDebug("--- INTERCEPTED REQUEST PAYLOAD ---");
                LogDebug(json);
                LogDebug("-----------------------------------");
                __instance.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            }
            catch (Exception ex) { LogDebug("Error TransformPayload: " + ex.Message + "\n" + ex.StackTrace); }
        }

        // ─────────────────────────────────────────────────────────────────────
        // FIX 4: Transform OpenRouter response -> game JSON
        // ─────────────────────────────────────────────────────────────────────

        [HarmonyPatch(typeof(DownloadHandler), "text", MethodType.Getter)]
        [HarmonyPostfix]
        public static void TransformResponse(ref string __result)
        {
            try
            {
                if (string.IsNullOrEmpty(__result)) return;
                if (!__result.Contains("\"choices\"") || !__result.Contains("\"content\"")) return;

                LogDebug("TransformResponse: Intercepted.");

                JSONNode resp = JSON.Parse(__result);
                if (resp == null || resp["choices"] == null || resp["choices"].Count == 0) return;

                string content = resp["choices"][0]["message"]["content"].Value;
                int compTokens  = resp["usage"] != null ? resp["usage"]["completion_tokens"].AsInt : 50;
                int totalTokens = resp["usage"] != null ? resp["usage"]["total_tokens"].AsInt : 100;

                LogDebug("Raw AI content: " + content);

                // Strip markdown code fences
                Match mdMatch = Regex.Match(content, @"```(?:json)?\s*([\s\S]+?)\s*```");
                if (mdMatch.Success) content = mdMatch.Groups[1].Value.Trim();

                string gameJson = null;

                // Try to parse JSON from content
                int firstBrace = content.IndexOf('{');
                int lastBrace  = content.LastIndexOf('}');
                if (firstBrace >= 0 && lastBrace > firstBrace)
                {
                    string candidate = content.Substring(firstBrace, lastBrace - firstBrace + 1);
                    try
                    {
                        JSONNode parsed = JSON.Parse(candidate);
                        if (parsed != null)
                        {
                            if (parsed["npc_reactions"] != null)
                            {
                                parsed["completion"] = compTokens;
                                parsed["total"]      = totalTokens;
                                gameJson = parsed.ToString();
                                LogDebug("Found full game JSON format.");
                            }
                            else if (parsed["npc_reply_to_player"] != null)
                            {
                                JSONNode wrapper = JSON.Parse("{}");
                                wrapper["npc_reactions"] = parsed;
                                wrapper["completion"]    = compTokens;
                                wrapper["total"]         = totalTokens;
                                gameJson = wrapper.ToString();
                                LogDebug("Found inner npc_reactions JSON.");
                            }
                        }
                    }
                    catch { }
                }

                // Plain text fallback
                if (gameJson == null)
                {
                    LogDebug("AI returned plain text. Wrapping.");
                    string rawContent = resp["choices"][0]["message"]["content"].Value;
                    string escaped = EscapeJsonString(rawContent);

                    gameJson = "{\"npc_reactions\":{"
                        + "\"npc_reply_to_player\":\"" + escaped + "\","
                        + "\"npc_body_animation\":\"idle\","
                        + "\"npc_face_expression\":\"smile\","
                        + "\"npc_emotion_type\":\"happy\","
                        + "\"npc_emotion_score\":\"5\","
                        + "\"angry_level\":\"0\","
                        + "\"favorability_change\":\"0\","
                        + "\"npc_action\":\"standing\","
                        + "\"npc_target_location\":\"\","
                        + "\"giving_to_player\":\"\","
                        + "\"character\":0"
                        + "},\"completion\":" + compTokens
                        + ",\"total\":" + totalTokens + "}";
                }

                __result = gameJson;
                LogDebug("Game JSON ready.");
            }
            catch (Exception ex)
            {
                LogDebug("Error TransformResponse: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Keep AIModel = ChatGPTAzure
        // ─────────────────────────────────────────────────────────────────────

        [HarmonyPatch(typeof(Communicator), "Start")]
        [HarmonyPrefix]
        public static void ForceAzureOnStart()
        {
            Communicator.AIModel = ChatGPTConversation.Model.ChatGPTAzure;
        }

        [HarmonyPatch(typeof(ChatGPTConversation), "Init")]
        [HarmonyPostfix]
        public static void ForceAzureAfterInit(ChatGPTConversation __instance)
        {
            try
            {
                var modelField = AccessTools.Field(typeof(ChatGPTConversation), "_model");
                if (modelField != null)
                    modelField.SetValue(__instance, ChatGPTConversation.Model.ChatGPTAzure);
            }
            catch (Exception ex) { LogDebug("Error ForceAzureAfterInit: " + ex.Message); }
        }

        // ─────────────────────────────────────────────────────────────────────
        // FIX 4: Force Voice TTS locally since OpenRouter has no audio
        // Patches AzureVoiceManager.Speak → uses RT-Voice (Windows SAPI)
        // ─────────────────────────────────────────────────────────────────────

        [HarmonyPatch(typeof(AzureVoiceManager), "Speak")]
        [HarmonyPrefix]
        public static bool PrefixVoiceSpeak(AzureVoiceManager __instance, JSONNode jsonVoice, Character characterId, float delayPlayTime)
        {
            try
            {
                // If jsonVoice has valid base64 audio data, let original handle it
                if (jsonVoice != null && !string.IsNullOrEmpty(jsonVoice.Value) && jsonVoice.Value.Length > 100)
                    return true;

                LogDebug("VoiceSpeak: No server audio, using local TTS...");

                // Get reply text from Communicator.currentJSON
                var communicator = UnityEngine.Object.FindObjectOfType<Communicator>();
                if (communicator == null) { LogDebug("VoiceSpeak: Communicator not found"); return false; }

                var currentJSONField = AccessTools.Field(typeof(Communicator), "currentJSON");
                if (currentJSONField == null) { LogDebug("VoiceSpeak: currentJSON field not found"); return false; }

                JSONNode currentJSON = currentJSONField.GetValue(communicator) as JSONNode;
                if (currentJSON == null) { LogDebug("VoiceSpeak: currentJSON is null"); return false; }

                string replyText = currentJSON["npc_reply_to_player"].Value;
                if (string.IsNullOrEmpty(replyText)) { LogDebug("VoiceSpeak: replyText is empty"); return false; }

                // Get AudioSource from AzureVoiceManager.VoiceMap
                var voiceMap = __instance.VoiceMap;
                if (voiceMap == null || voiceMap.Count == 0)
                {
                    LogDebug("VoiceSpeak: VoiceMap is null or empty");
                    return false;
                }
                
                AudioSource audioSource = null;
                if (voiceMap.ContainsKey(characterId))
                {
                    audioSource = voiceMap[characterId];
                }
                else if (voiceMap.ContainsKey(__instance.mainCharacter))
                {
                    audioSource = voiceMap[__instance.mainCharacter];
                    LogDebug("VoiceSpeak: Used mainCharacter fallback: " + __instance.mainCharacter);
                }
                else
                {
                    // Last resort: grab any available AudioSource
                    foreach (var kvp in voiceMap)
                    {
                        if (kvp.Value != null) { audioSource = kvp.Value; break; }
                    }
                    LogDebug("VoiceSpeak: Used first available AudioSource");
                }
                
                if (audioSource == null)
                {
                    LogDebug("VoiceSpeak: No AudioSource found at all. VoiceMap keys: " + string.Join(", ", new List<Character>(voiceMap.Keys).ConvertAll(k => k.ToString()).ToArray()));
                    return false;
                }

                if (UltimateFixPlugin.ConfigTTSMode.Value == "Online")
                {
                    LogDebug("VoiceSpeak: Custom TTS is ENABLED (Online). Provider: " + UltimateFixPlugin.cfgTTSProvider);
                    
                    TTSManager mgr = TTSManager.Instance;
                    if (mgr == null)
                    {
                        mgr = UnityEngine.Object.FindObjectOfType<TTSManager>();
                        LogDebug("VoiceSpeak: Instance was null, FindObjectOfType found: " + (mgr != null));
                    }
                    if (mgr == null)
                    {
                        // Last resort: create it on the fly
                        LogDebug("VoiceSpeak: Creating TTSManager on the fly...");
                        GameObject go = new GameObject("AI2UTTSManager_Runtime");
                        UnityEngine.Object.DontDestroyOnLoad(go);
                        mgr = go.AddComponent<TTSManager>();
                    }
                    
                    mgr.StartTTSCoroutine(replyText, audioSource);
                    LogDebug("VoiceSpeak: Coroutine started!");
                    return false; // Skip original
                }

                LogDebug("VoiceSpeak: Custom TTS is disabled. Invoking LocalTTSManager manually...");
                LocalTTSManager localTTS = UnityEngine.Object.FindObjectOfType<LocalTTSManager>();
                if (localTTS != null)
                {
                    localTTS.Speak(replyText, characterId);
                }
                else
                {
                    LogDebug("VoiceSpeak: LocalTTSManager not found in scene!");
                }
                
                return false; // Skip original method
            }
            catch (Exception ex)
            {
                LogDebug("Error VoiceSpeak: " + ex.Message
                    + (ex.InnerException != null ? "\nInner: " + ex.InnerException.Message : "")
                    + "\n" + ex.StackTrace);
                return false;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // OFFLINE SAVE/LOAD SYSTEM
        // ─────────────────────────────────────────────────────────────────────

        public static bool PrefixPlayFabId(ref string __result)
        {
            __result = "OFFLINE_USER";
            return false;
        }

        [HarmonyPatch(typeof(PlayerDataSubsystem), "LoadAllData")]
        [HarmonyPrefix]
        public static bool PrefixLoadAllData(PlayerDataSubsystem __instance, Action loadSuccess)
        {
            try
            {
                string text = "savedata/global.yags";
                string text2 = "OFFLINE_USER_savedData";
                Dictionary<string, string> savedData = null;
                if (ES3.FileExists(text))
                {
                    savedData = ES3.Load<Dictionary<string, string>>(text2, text);
                    LogDebug("Offline Load: Successfully loaded " + savedData.Count + " key(s) from local storage.");
                }
                if (savedData == null) savedData = new Dictionary<string, string>();

                AccessTools.Field(typeof(PlayerDataSubsystem), "SavedData").SetValue(__instance, savedData);
                AccessTools.Field(typeof(PlayerDataSubsystem), "UploadedData").SetValue(__instance, new Dictionary<string, string>());
                
                // SaveGlobalDataToLocals
                AccessTools.Method(typeof(PlayerDataSubsystem), "SaveGlobalDataToLocals").Invoke(__instance, null);
                
                // HandleGeneralData
                AccessTools.Method(typeof(PlayerDataSubsystem), "HandleGeneralData").Invoke(__instance, null);
                
                GameRuntime.GetSubsystem<AchievementSubsystem>().HandleAchievementData();
                LevelData_HubWorld.LoadLevelData();
                EventManager.NPCDataUpdated();
                
                if (loadSuccess != null) loadSuccess();
            }
            catch (Exception ex)
            {
                LogDebug("Error in PrefixLoadAllData: " + ex.ToString());
            }
            return false; // Skip original
        }

        [HarmonyPatch(typeof(PlayerDataSubsystem), "SaveAndUpload")]
        [HarmonyPrefix]
        public static bool PrefixSaveAndUpload(PlayerDataSubsystem __instance)
        {
            try
            {
                AccessTools.Method(typeof(PlayerDataSubsystem), "SaveGlobalDataToLocals").Invoke(__instance, null);
                GameRuntime.GetSubsystem<AchievementSubsystem>().SaveAchievementRelated();
                LogDebug("Offline Save: Local save complete, cloud upload skipped.");
            }
            catch (Exception ex)
            {
                LogDebug("Error in PrefixSaveAndUpload: " + ex.ToString());
            }
            return false; // Skip original
        }

        [HarmonyPatch(typeof(PlayerDataSubsystem), "Save")]
        [HarmonyPrefix]
        public static bool PrefixSave()
        {
            return false; // Skip original
        }
        
        [HarmonyPatch(typeof(PlayerDataSubsystem), "UploadData")]
        [HarmonyPrefix]
        public static bool PrefixUploadData()
        {
            return false; // Skip original
        }

        [HarmonyPatch(typeof(PlayerDataSubsystem), "UploadDataAfterCheck")]
        [HarmonyPrefix]
        public static bool PrefixUploadDataAfterCheck()
        {
            return false; // Skip original
        }

        [HarmonyPatch(typeof(Shop), "PurchaseItemFromShop")]
        [HarmonyPrefix]
        public static bool PrefixPurchaseItemFromShop(Shop __instance, Item _item, int count)
        {
            try
            {
                LogDebug("Offline Shop: Bypassing PlayFab purchase...");
                AccessTools.Field(typeof(Shop), "m_purchasingItem").SetValue(__instance, _item);
                AccessTools.Field(typeof(Shop), "m_purchasingItemCount").SetValue(__instance, count);
                
                var loading = AccessTools.Field(typeof(Shop), "loadingPurchaseContainer").GetValue(__instance) as GameObject;
                if (loading != null) loading.SetActive(true);
                
                AccessTools.Field(typeof(Shop), "success").SetValue(__instance, false);
                AccessTools.Field(typeof(Shop), "fail").SetValue(__instance, false);

                // Save to OfflineItems in ES3
                List<string> offlineItems = new List<string>();
                if (ES3.KeyExists("OfflineItems", "savedata/global.yags"))
                {
                    offlineItems = ES3.Load<List<string>>("OfflineItems", "savedata/global.yags");
                }
                LogDebug("Offline Shop: Purchasing item " + _item.name + " x" + count);

                // Add to our offline items list
                for (int i = 0; i < count; i++)
                {
                    offlineItems.Add(_item.name);
                }

                // Save immediately
                ES3.Save("OfflineItems", offlineItems, "savedata/global.yags");

                // Original logic: add to inventory
                Inventory inventory = Inventory.FindInventory("PlayerInventory");
                if (_item.IsNPCTag)
                {
                    if (inventory != null) inventory.AddItem(_item, inventory.InventoryName, false);
                    
                    LevelManager_HubWorld hub = LevelManager_HubWorld.Instance;
                    if (hub != null)
                    {
                        var tagsField = typeof(LevelManager_HubWorld).GetField("m_NPCPersonalityTags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (tagsField != null)
                        {
                            NPCPersonalityTags tagsObj = tagsField.GetValue(hub) as NPCPersonalityTags;
                            if (tagsObj != null)
                            {
                                string tType = _item.m_TagType.ToString();
                                if (tType == "Personality" && !tagsObj.personality.Contains(_item.EnumIndex)) tagsObj.personality.Add(_item.EnumIndex);
                                else if (tType == "Hobby" && !tagsObj.hobby.Contains(_item.EnumIndex)) tagsObj.hobby.Add(_item.EnumIndex);
                                else if (tType == "SpeakingTone" && !tagsObj.speakingTone.Contains(_item.EnumIndex)) tagsObj.speakingTone.Add(_item.EnumIndex);
                            }
                        }
                    }
                }

                // Call PurchaseItem directly with null
                AccessTools.Method(typeof(Shop), "PurchaseItem").Invoke(__instance, new object[] { null });

                // Start Coroutine
                var coroutine = AccessTools.Method(typeof(Shop), "UpdateConfirmPage").Invoke(__instance, null) as System.Collections.IEnumerator;
                if (coroutine != null)
                {
                    __instance.StartCoroutine(coroutine);
                }
            }
            catch (Exception ex)
            {
                LogDebug("Error in PrefixPurchaseItemFromShop: " + ex.ToString());
            }
            return false; // Skip original
        }

        [HarmonyPatch(typeof(LevelManager_HubWorld), "LoadPlayfabInventory")]
        [HarmonyPrefix]
        public static bool PrefixLoadPlayfabInventory(LevelManager_HubWorld __instance)
        {
            try
            {
                LogDebug("Offline Shop: Loading inventory from ES3...");
                List<string> offlineItems = new List<string>();
                if (ES3.KeyExists("OfflineItems", "savedata/global.yags"))
                {
                    offlineItems = ES3.Load<List<string>>("OfflineItems", "savedata/global.yags");
                }
                LogDebug("Offline Shop: Found " + offlineItems.Count + " items in save.");
                
                var rewardDictField = AccessTools.Field(typeof(LevelManager_HubWorld), "rewardItemDictionary");
                var rewardDict = rewardDictField != null ? rewardDictField.GetValue(__instance) as ItemIDList : null;
                
                if (rewardDict == null) LogDebug("Offline Shop: rewardItemDictionary is NULL!");
                else LogDebug("Offline Shop: rewardItemDictionary has " + rewardDict.Items.Count + " items.");

                if (rewardDict != null)
                {
                    Inventory inventory = Inventory.FindInventory("PlayerInventory");
                    if (inventory == null) LogDebug("Offline Shop: PlayerInventory is NULL!");
                    
                    foreach (string itemId in offlineItems)
                    {
                        Item item = null;
                        foreach (var kvp in rewardDict.Items)
                        {
                            if (kvp.Key.Equals(itemId, StringComparison.OrdinalIgnoreCase) ||
                                (kvp.Value != null && kvp.Value.name.Equals(itemId, StringComparison.OrdinalIgnoreCase)) ||
                                (kvp.Value != null && kvp.Value.ItemID != null && kvp.Value.ItemID.Equals(itemId, StringComparison.OrdinalIgnoreCase)))
                            {
                                item = kvp.Value;
                                break;
                            }
                        }

                        if (item != null)
                        {
                            LogDebug("Offline Shop: Granting item " + item.name);
                            if (item.IsNPCTag)
                            {
                                if (inventory != null) inventory.AddItem(item, inventory.InventoryName, false);
                                
                                LevelManager_HubWorld hub = LevelManager_HubWorld.Instance;
                                if (hub != null)
                                {
                                    var tagsField = typeof(LevelManager_HubWorld).GetField("m_NPCPersonalityTags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                    if (tagsField != null)
                                    {
                                        NPCPersonalityTags tagsObj = tagsField.GetValue(hub) as NPCPersonalityTags;
                                        if (tagsObj != null)
                                        {
                                            string tType = item.m_TagType.ToString();
                                            if (tType == "Personality" && !tagsObj.personality.Contains(item.EnumIndex)) tagsObj.personality.Add(item.EnumIndex);
                                            else if (tType == "Hobby" && !tagsObj.hobby.Contains(item.EnumIndex)) tagsObj.hobby.Add(item.EnumIndex);
                                            else if (tType == "SpeakingTone" && !tagsObj.speakingTone.Contains(item.EnumIndex)) tagsObj.speakingTone.Add(item.EnumIndex);
                                        }
                                    }
                                }
                            }
                            else if (item.IsNPCAppearance)
                            {
                                if (item.NPCAppearance is NPCAppearance_L1) Appearance.Instance.Appearances_L1.Add(item.NPCAppearance);
                                else if (item.NPCAppearance is NPCAppearance_L2) Appearance.Instance.Appearances_L2.Add(item.NPCAppearance);
                                else if (item.NPCAppearance is NPCAppearance_L3) Appearance.Instance.Appearances_L3.Add(item.NPCAppearance);
                                else if (item.NPCAppearance is NPCAppearance_L4) Appearance.Instance.Appearances_L4.Add(item.NPCAppearance);
                            }
                            else
                            {
                                if (inventory != null) inventory.AddItem(item, inventory.InventoryName, false);
                            }
                        }
                        else
                        {
                            LogDebug("Offline Shop: Item " + itemId + " not found in dictionary!");
                        }
                    }
                    
                    var toggleMethod = AccessTools.Method(typeof(LevelManager_HubWorld), "TogglePlayerDialogueUIInAtrium");
                    if (toggleMethod != null) toggleMethod.Invoke(__instance, new object[] { false });
                    
                    var listenerField = AccessTools.Field(typeof(LevelManager_HubWorld), "_inventoryActionListener");
                    var listener = listenerField != null ? listenerField.GetValue(__instance) : null;
                    if (listener != null)
                    {
                        var evField = AccessTools.Field(listener.GetType(), "toggleInventoryBoolEvent");
                        var ev = evField != null ? evField.GetValue(listener) as UnityEngine.Events.UnityEvent<bool> : null;
                        if (ev != null) ev.Invoke(false);
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebug("Error in PrefixLoadPlayfabInventory: " + ex.ToString());
            }
            return false; // Skip original
        }

        [HarmonyPatch(typeof(StateMachine), "SwitchToPreviousState")]
        [HarmonyPrefix]
        public static bool PrefixSwitchToPreviousState(StateMachine __instance, ref bool __result)
        {
            // ... (keep existing SwitchToPreviousState patch)
            try
            {
                var prevField = AccessTools.Field(typeof(StateMachine), "previousState");
                var prev = prevField != null ? prevField.GetValue(__instance) : null;
                if (prev == null)
                {
                    LogDebug("SwitchToPreviousState: previousState is null. Applying fallback...");
                    if (__instance is HubWorldStateMachine)
                    {
                        var hubSM = (HubWorldStateMachine)__instance;
                        hubSM.ChangeToDefaultState();
                        __result = true;
                        return false; // Skip original
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebug("Error in PrefixSwitchToPreviousState: " + ex.Message);
            }
            return true; // Run original
        }

        [HarmonyPatch(typeof(FavorMeter), "HaveFinished")]
        [HarmonyPrefix]
        public static bool PrefixHaveFinished(ref bool __result)
        {
            __result = true;
            return false; // Skip original
        }

        [HarmonyPatch(typeof(UIManager_HubWorld), "Start")]
        [HarmonyPostfix]
        public static void PostfixUIManager_HubWorld_Start(UIManager_HubWorld __instance)
        {
            try
            {
                var container = __instance.LevelSelectionContainer;
                if (container != null)
                {
                    LogDebug("Unlocking all chapters...");
                    UnityEngine.UI.Button[] buttons = container.GetComponentsInChildren<UnityEngine.UI.Button>(true);
                    LogDebug("Found " + buttons.Length + " buttons in LevelSelectionContainer.");
                    
                    int chapterIndex = 1; // Start from 1
                    foreach (UnityEngine.UI.Button btn in buttons)
                    {
                        string btnName = btn.name.ToLower();
                        if (btnName.Contains("back") || btnName.Contains("close") || btnName.Contains("quit") || btnName.Contains("exit")) continue;

                        // Enable the button
                        btn.interactable = true;
                        
                        // Disable padlocks (Images or GameObjects named "Lock" etc)
                        foreach (Transform child in btn.transform)
                        {
                            string childName = child.name.ToLower();
                            if (childName.Contains("lock") || childName.Contains("soon") || childName.Contains("reveal"))
                            {
                                child.gameObject.SetActive(false);
                            }
                        }

                        // Add listener if missing
                        int levelToSelect = chapterIndex;
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => {
                            LogDebug("Clicked level " + levelToSelect);
                            try
                            {
                                __instance.SelectLevel(levelToSelect);
                            }
                            catch (Exception ex)
                            {
                                LogDebug("SelectLevel threw exception (probably unfinished UI arrays). Forcing direct load! Error: " + ex.Message);
                                var field = typeof(UIManager_HubWorld).GetField("currentSelectingLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                if (field != null) field.SetValue(__instance, levelToSelect);
                                __instance.ButtonPressed_GameStart();
                            }
                        });
                        
                        chapterIndex++;
                        if (chapterIndex > 4) break; // Only 4 chapters
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebug("Error in PostfixUIManager_HubWorld_Start: " + ex.Message);
            }
        }
        // ─────────────────────────────────────────────────────────────────────

        [HarmonyPatch(typeof(NPCMasterBehavior_Main_L1), "SetServerContext")]
        [HarmonyPrefix]
        public static void PrefixL1(object __instance) { EnsureCharacterConfig(__instance); }

        [HarmonyPatch(typeof(NPCMasterBehavior_Main_L2), "SetServerContext")]
        [HarmonyPrefix]
        public static void PrefixL2(object __instance) { EnsureCharacterConfig(__instance); }

        [HarmonyPatch(typeof(NPCMasterBehavior_Main_L3), "SetServerContext")]
        [HarmonyPrefix]
        public static void PrefixL3(object __instance) { EnsureCharacterConfig(__instance); }

        [HarmonyPatch(typeof(NPCMasterBehavior_Main_L4), "SetServerContext")]
        [HarmonyPrefix]
        public static void PrefixL4(object __instance) { EnsureCharacterConfig(__instance); }

        public static void EnsureCharacterConfig(object __instance)
        {
            try
            {
                var field = AccessTools.Field(__instance.GetType(), "characterConfig");
                if (field == null) return;
                var config = field.GetValue(__instance) as CharacterConfig;
                if (config == null)
                {
                    config = ScriptableObject.CreateInstance<CharacterConfig>();
                    config.personality  = new List<int>();
                    config.speakingTone = new List<int>();
                    config.hobby        = new List<int>();
                    config.NPCCurrentAppearanceConfig = ScriptableObject.CreateInstance<NPCAppearance>();
                    config.NPCCurrentAppearanceConfig.prompt_Description = "A friendly anime girl";
                    field.SetValue(__instance, config);
                }
                else if (config.NPCCurrentAppearanceConfig == null)
                {
                    config.NPCCurrentAppearanceConfig = ScriptableObject.CreateInstance<NPCAppearance>();
                    config.NPCCurrentAppearanceConfig.prompt_Description = "A friendly anime girl";
                }
            }
            catch (Exception ex) { LogDebug("Error EnsureCharacterConfig: " + ex.Message); }
        }

        // ─────────────────────────────────────────────────────────────────────
        // DEBUG: Log + catch Communicator.SendToChatGPT
        // ─────────────────────────────────────────────────────────────────────

        [HarmonyPatch(typeof(Communicator), "SendToChatGPT")]
        [HarmonyPrefix]
        public static bool DebugSendToChatGPT(Communicator __instance, string message)
        {
            try
            {
                Type prefsType = AccessTools.TypeByName("wAIfuBackend.Prefs");
                if (prefsType != null)
                {
                    var pldField = AccessTools.Field(prefsType, "PlayerLocalData");
                    if (pldField != null && pldField.GetValue(null) == null)
                    {
                        Type userLocalDataType = AccessTools.TypeByName("wAIfuBackend.UserLocalData");
                        if (userLocalDataType != null)
                        {
                            object uld = Activator.CreateInstance(userLocalDataType);
                            AccessTools.Field(userLocalDataType, "playfabId").SetValue(uld, "OFFLINE_PLAYFAB_ID_9999");
                            pldField.SetValue(null, uld);
                            LogDebug("Initialized UserLocalData because it was null!");
                        }
                    }
                }
            }
            catch (Exception ex) { LogDebug("Error init PlayFabId: " + ex.Message); }

            try
            {
                LogDebug(">>> Communicator.SendToChatGPT MSG=" + message);
                var chatField = AccessTools.Field(typeof(Communicator), "_chat");
                if (chatField != null)
                {
                    var chatObj = chatField.GetValue(__instance);
                    if (chatObj != null)
                    {
                        var currentChatField = AccessTools.Field(chatObj.GetType(), "_currentChat");
                        if (currentChatField != null)
                        {
                            var currentChat = currentChatField.GetValue(chatObj) as System.Collections.IList;
                            if (currentChat != null)
                            {
                                LogDebug("--- _currentChat has " + currentChat.Count + " messages ---");
                                foreach (var msgObj in currentChat)
                                {
                                    var role = AccessTools.Field(msgObj.GetType(), "role").GetValue(msgObj) as string;
                                    var content = AccessTools.Field(msgObj.GetType(), "content").GetValue(msgObj) as string;
                                    LogDebug("ROLE: " + role + " | CONTENT: " + content);
                                }
                                LogDebug("-------------------------------------------------");
                            }
                        }
                    }
                }
                var presetField = AccessTools.Field(typeof(Communicator), "_presetNPCAI");
                if (presetField != null)
                {
                    PresetNPCAI preset = (PresetNPCAI)presetField.GetValue(__instance);
                    if (preset != null && preset.isAIReplyDisabled) preset.isAIReplyDisabled = false;
                }
                if (__instance.noReplyTimer <= 2f) __instance.noReplyTimer = 10f;
            }
            catch (Exception e) { LogDebug("Error DebugSend: " + e.Message); }
            return true;
        }

        [HarmonyPatch(typeof(Communicator), "SendToChatGPT")]
        [HarmonyFinalizer]
        public static Exception CatchSendException(Exception __exception)
        {
            if (__exception != null)
                LogDebug("!!! EXCEPTION SendToChatGPT: " + __exception.ToString());
            return null;
        }

        // ─────────────────────────────────────────────────────────────────────
        // NPC CHAT SUBMIT
        // ─────────────────────────────────────────────────────────────────────

        [HarmonyPatch(typeof(NPCMasterBehavior_MainCharacter), "OnSubmitChatMessage")]
        [HarmonyPrefix]
        public static bool ForceSubmitMessage(NPCMasterBehavior_MainCharacter __instance, string message, Character characterId)
        {
            try
            {
                var comm = AccessTools.Field(typeof(NPCMasterBehavior_MainCharacter), "communicator").GetValue(__instance) as Communicator;
                var npcDisabledField = AccessTools.Field(typeof(NPCMasterBehavior_MainCharacter), "npcDisabled");
                bool isDisabled = (bool)npcDisabledField.GetValue(__instance);
                if (isDisabled) npcDisabledField.SetValue(__instance, false);
                if (comm != null && comm.noReplyTimer <= 2f) comm.noReplyTimer = 10f;
            }
            catch (Exception e) { LogDebug("Error ForceSubmit: " + e.Message); }
            return true;
        }

        [HarmonyPatch(typeof(Communicator), "ChangeNPC")]
        [HarmonyPostfix]
        public static void ClearChatOnNPCChange(Communicator __instance)
        {
            try
            {
                var chatField = AccessTools.Field(typeof(Communicator), "_chat");
                if (chatField != null)
                {
                    var chatObj = chatField.GetValue(__instance);
                    if (chatObj != null)
                    {
                        var currentChatField = AccessTools.Field(chatObj.GetType(), "_currentChat");
                        if (currentChatField != null)
                        {
                            var currentChat = currentChatField.GetValue(chatObj) as System.Collections.IList;
                            if (currentChat != null)
                            {
                                currentChat.Clear();
                                LogDebug(">>> Cleared Chat History because NPC/Level changed!");
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { LogDebug("Error ClearChat: " + ex.Message); }
        }

        [HarmonyPatch(typeof(Michsky.DreamOS.UserManager), "Login")]
        [HarmonyFinalizer]
        public static Exception CatchUserManagerLogin(Exception __exception)
        {
            if (__exception != null)
                LogDebug("!!! EXCEPTION UserManager.Login: " + __exception.ToString());
            return null;
        }

        [HarmonyPatch(typeof(Michsky.DreamOS.UserManager), "Login")]
        [HarmonyPrefix]
        public static bool DebugUserManagerLoginPrefix(object __instance)
        {
            try {
                var pwd = AccessTools.Field(__instance.GetType(), "password").GetValue(__instance) as string;
                var sysPwd = AccessTools.Field(__instance.GetType(), "systemPassword").GetValue(__instance) as string;
                LogDebug(">>> UserManager.Login called! password=" + pwd + " | systemPassword=" + sysPwd);
                
                // If it's a bug where systemPassword is blank or missing, maybe we should force it?
                // For now, just log it.
            } catch {}
            return true;
        }

        [HarmonyPatch(typeof(NPCMasterBehavior_Main_Config), "SetServerContext")]
        [HarmonyPrefix]
        public static bool BypassSetServerContext(NPCMasterBehavior_Main_Config __instance, string npcName, string playerName, bool IsVisionEnabled, string mediaFileName, bool isIgnoranceTriggered)
        {
            if (GameManager.CurrentLevel == 0)
            {
                try
                {
                    ServerContextHubworld.CurrentServerContext.SetCommonAttributes(
                        npcName, playerName, "English", 0, 0, 0, Character.Eddie, 
                        new System.Collections.Generic.List<int> { 0 }, 
                        "OFFLINE_PLAYFAB_ID_9999", false, 
                        new System.Collections.Generic.List<int>(), 
                        new System.Collections.Generic.List<int>(), 
                        new System.Collections.Generic.List<int>(), 
                        "DummyAppearance", "DummyBio"
                    );
                    LogDebug(">>> Bypassed SetServerContext to prevent NullReferenceException in HubWorld!");
                }
                catch (Exception ex)
                {
                    LogDebug("Error in BypassSetServerContext: " + ex.Message);
                }
                return false; // Skip original method
            }
            return true;
        }

        [HarmonyPatch(typeof(LevelManager_HubWorld), "EnableNPC")]
        [HarmonyPrefix]
        public static bool FixHubWorldMissingData(LevelManager_HubWorld __instance, int idx)
        {
            if (idx < 1 || idx > 4) return true;
            try
            {
                Type prefsType = AccessTools.TypeByName("wAIfuBackend.Prefs");
                if (prefsType != null)
                {
                    var pldField = AccessTools.Field(prefsType, "PlayerLocalData");
                    if (pldField != null && pldField.GetValue(null) == null)
                    {
                        Type userLocalDataType = AccessTools.TypeByName("wAIfuBackend.UserLocalData");
                        if (userLocalDataType != null)
                        {
                            object uld = Activator.CreateInstance(userLocalDataType);
                            AccessTools.Field(userLocalDataType, "playfabId").SetValue(uld, "OFFLINE_PLAYFAB_ID_9999");
                            pldField.SetValue(null, uld);
                            LogDebug("Initialized UserLocalData in HubWorld!");
                        }
                    }
                }

                var charConfigsField = AccessTools.Field(typeof(LevelManager_HubWorld), "characterConfigs");
                if (charConfigsField != null)
                {
                    Array configs = charConfigsField.GetValue(__instance) as Array;
                    if (configs != null && configs.Length >= idx)
                    {
                        var config = configs.GetValue(idx - 1);
                        if (config != null)
                        {
                            var savedDataField = AccessTools.Field(config.GetType(), "savedData");
                            if (savedDataField != null && savedDataField.GetValue(config) == null)
                            {
                                // Initialize dummy data
                                Type npcDataType = AccessTools.TypeByName("NPCData");
                                if (npcDataType != null)
                                {
                                    object dummyData = Activator.CreateInstance(npcDataType);
                                    
                                    var listObj = AccessTools.Field(config.GetType(), "list").GetValue(config);
                                    var list = listObj as System.Collections.IList;
                                    string skinName = "Default";
                                    if (list != null && list.Count > 0)
                                    {
                                        var firstApp = list[0];
                                        if (firstApp != null)
                                        {
                                            AccessTools.Field(config.GetType(), "NPCCurrentAppearanceConfig").SetValue(config, firstApp);
                                            var nameField = AccessTools.Field(firstApp.GetType(), "name");
                                            if (nameField != null) skinName = (string)nameField.GetValue(firstApp);
                                        }
                                    }
                                    
                                    AccessTools.Field(npcDataType, "Skin").SetValue(dummyData, skinName);
                                    
                                    AccessTools.Field(npcDataType, "Personality").SetValue(dummyData, new System.Collections.Generic.List<int>());
                                    AccessTools.Field(npcDataType, "Hobby").SetValue(dummyData, new System.Collections.Generic.List<int>());
                                    AccessTools.Field(npcDataType, "SpeakingTone").SetValue(dummyData, new System.Collections.Generic.List<int>());
                                    
                                    savedDataField.SetValue(config, dummyData);
                                    LogDebug("FixHubWorldMissingData: Initialized dummy savedData for character " + idx);
                                }
                            }
                            
                            var appConfigField = AccessTools.Field(config.GetType(), "NPCCurrentAppearanceConfig");
                            if (appConfigField != null && appConfigField.GetValue(config) == null)
                            {
                                var listObj = AccessTools.Field(config.GetType(), "list").GetValue(config);
                                var list = listObj as System.Collections.IList;
                                if (list != null && list.Count > 0)
                                {
                                    appConfigField.SetValue(config, list[0]);
                                    LogDebug("FixHubWorldMissingData: Initialized NPCCurrentAppearanceConfig for character " + idx);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebug("Error in FixHubWorldMissingData: " + ex.ToString());
            }
            return true;
        }

        [HarmonyPatch(typeof(wAIfuBackend.PlayfabInventory), "SubstractInventoryItems")]
        [HarmonyPrefix]
        public static bool BypassSubstractInventoryItems()
        {
            LogDebug(">>> Bypassed PlayfabInventory.SubstractInventoryItems (Offline Mode)");
            return false; // Skip original method
        }

        [HarmonyPatch(typeof(Computer), "Start")]
        [HarmonyPrefix]
        public static void DebugComputerStart(Computer __instance)
        {
            try {
                var pList = AccessTools.Field(typeof(Computer), "passwordList").GetValue(__instance) as System.Collections.Generic.List<string>;
                if (pList != null) {
                    LogDebug(">>> Computer initialized. Valid passwords: " + string.Join(", ", pList));
                }
            } catch {}
        }

        [HarmonyPatch(typeof(Computer), "OnLogIn")]
        [HarmonyFinalizer]
        public static Exception CatchComputerOnLogIn(Exception __exception)
        {
            if (__exception != null)
                LogDebug("!!! EXCEPTION Computer.OnLogIn: " + __exception.ToString());
            return null;
        }
        
        [HarmonyPatch(typeof(Computer), "OnLogIn")]
        [HarmonyPrefix]
        public static bool DebugComputerOnLogInPrefix()
        {
            LogDebug(">>> Computer.OnLogIn called!");
            return true;
        }
    }

    public class TTSManager : MonoBehaviour
    {
        public static TTSManager Instance;

        void Awake()
        {
            Instance = this;
        }

        public void StartTTSCoroutine(string text, AudioSource source)
        {
            StartCoroutine(DownloadAndPlayTTS(text, source));
        }

        public System.Collections.IEnumerator DownloadAndPlayTTS(string text, AudioSource source)
        {
            UltimateFixPlugin.LogDebug("TTS Coroutine: Started execution");
            
            if (string.IsNullOrEmpty(UltimateFixPlugin.cfgTTSAPIKey))
            {
                if (UltimateFixPlugin.cfgTTSProvider == "OpenAI Compatible" && UltimateFixPlugin.cfgTTSBaseURL.Contains("127.0.0.1"))
                {
                    UltimateFixPlugin.LogDebug("TTS Note: Using local OpenAI Compatible server, empty API Key is allowed.");
                }
                else
                {
                    UltimateFixPlugin.LogDebug("TTS Error: API Key is empty! Please configure TTS API Key.");
                    yield break;
                }
            }

            if (!UltimateFixPlugin.ConfigTTSEnable.Value) yield break;

            string url = UltimateFixPlugin.cfgTTSBaseURL;
            byte[] bodyData = null;
            Dictionary<string, string> headers = new Dictionary<string, string>();

            if (UltimateFixPlugin.cfgTTSProvider == "OpenAI Compatible")
            {
                if (string.IsNullOrEmpty(url)) url = "https://api.openai.com/v1";
                if (!url.EndsWith("/audio/speech"))
                {
                    if (!url.EndsWith("/")) url += "/";
                    url += "audio/speech";
                }
                
                string escapedText = UltimateFixPlugin.EscapeJsonString(text);
                string voiceModel = UltimateFixPlugin.GetActiveTTSModel(false);
                
                string json = "{\"model\":\"tts-1\",\"input\":\"" + escapedText + "\",\"voice\":\"" + voiceModel + "\",\"response_format\":\"mp3\"}";
                bodyData = Encoding.UTF8.GetBytes(json);

                headers["Authorization"] = "Bearer " + UltimateFixPlugin.cfgTTSAPIKey;
                headers["Content-Type"] = "application/json";
            }
            else // Azure
            {
                if (string.IsNullOrEmpty(url)) url = "https://" + UltimateFixPlugin.cfgTTSRegion + ".tts.speech.microsoft.com/cognitiveservices/v1";
                
                string voiceModel = UltimateFixPlugin.GetActiveTTSModel(false);
                string escapedText = text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
                
                string ssml = "<speak version='1.0' xml:lang='en-US'><voice xml:lang='en-US' xml:gender='Female' name='" + voiceModel + "'>" 
                    + escapedText 
                    + "</voice></speak>";
                
                bodyData = Encoding.UTF8.GetBytes(ssml);

                headers["Ocp-Apim-Subscription-Key"] = UltimateFixPlugin.cfgTTSAPIKey;
                headers["Content-Type"] = "application/ssml+xml";
                headers["X-Microsoft-OutputFormat"] = "audio-16khz-32kbitrate-mono-mp3";
            }

            UltimateFixPlugin.LogDebug("TTS Request URL: " + url);

            using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(bodyData);
                www.downloadHandler = new DownloadHandlerBuffer();
                foreach (var kvp in headers)
                {
                    www.SetRequestHeader(kvp.Key, kvp.Value);
                }

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    UltimateFixPlugin.LogDebug("TTS Network Error: " + www.error + "\n" + www.downloadHandler.text);
                }
                else
                {
                    byte[] audioBytes = www.downloadHandler.data;
                    if (audioBytes != null && audioBytes.Length > 0)
                    {
                        UltimateFixPlugin.LogDebug("TTS Success! Downloaded " + audioBytes.Length + " bytes.");
                        bool isWav = audioBytes.Length > 4 && audioBytes[0] == 'R' && audioBytes[1] == 'I' && audioBytes[2] == 'F' && audioBytes[3] == 'F';
                        string tempFile = Path.Combine(UltimateFixPlugin.pluginDir, isWav ? "temp_tts.wav" : "temp_tts.mp3");
                        File.WriteAllBytes(tempFile, audioBytes);
                        
                        string fileUrl = "file:///" + tempFile.Replace("\\", "/");
                        AudioType aType = isWav ? AudioType.WAV : AudioType.MPEG;
                        using (UnityWebRequest audioReq = UnityWebRequestMultimedia.GetAudioClip(fileUrl, aType))
                        {
                            yield return audioReq.SendWebRequest();
                            if (audioReq.result == UnityWebRequest.Result.ConnectionError || audioReq.result == UnityWebRequest.Result.ProtocolError)
                            {
                                UltimateFixPlugin.LogDebug("TTS Audio Load Error: " + audioReq.error);
                            }
                            else
                            {
                                AudioClip clip = DownloadHandlerAudioClip.GetContent(audioReq);
                                if (clip != null)
                                {
                                    // Wait until clip is fully loaded
                                    while (clip.loadState == AudioDataLoadState.Loading)
                                    {
                                        yield return null;
                                    }

                                    if (clip.loadState == AudioDataLoadState.Loaded)
                                    {
                                        source.clip = clip;
                                        source.volume = 1f;
                                        source.mute = false;
                                        source.enabled = true;
                                        source.Play();
                                        UltimateFixPlugin.LogDebug("TTS Audio Playing!");
                                        
                                        // Wait for it to finish
                                        while (source.isPlaying)
                                        {
                                            yield return null;
                                        }
                                        UnityEngine.Object.Destroy(clip);
                                    }
                                    else
                                    {
                                        UltimateFixPlugin.LogDebug("TTS Audio Load Error: Clip load state is " + clip.loadState.ToString());
                                    }
                                }
                                else
                                {
                                    UltimateFixPlugin.LogDebug("TTS Audio Load Error: Clip is null.");
                                }
                            }
                        }
                        
                        // Clean up temporary file
                        try
                        {
                            if (File.Exists(tempFile))
                            {
                                File.Delete(tempFile);
                                UltimateFixPlugin.LogDebug("TTS Cleaned up " + tempFile);
                            }
                        }
                        catch (Exception ex)
                        {
                            UltimateFixPlugin.LogDebug("TTS Error deleting temp file: " + ex.Message);
                        }
                    }
                }
            }
        }
    }

    public class UIHunter : MonoBehaviour
    {
        private bool hasTestedTTS = false;
        void Start() { InvokeRepeating("Hunt", 0.5f, 0.5f); }
        void Hunt()
        {
            if (SceneManager.GetActiveScene().name == "MenuState")
            {
                Canvas[] canvases = FindObjectsOfType<Canvas>();
                foreach (Canvas c in canvases)
                    foreach (Transform child in c.transform)
                    {
                        string name = child.name.ToLower();
                        if ((name.Contains("loading") || name.Contains("fade") || name.Contains("black") || name.Contains("splash"))
                            && child.gameObject.activeInHierarchy)
                            child.gameObject.SetActive(false);
                    }
            }

            // Auto Test LocalTTSManager
            if (!hasTestedTTS)
            {
                GameObject ttsObj = GameObject.Find("LocalTTSManager");
                if (ttsObj != null)
                {
                    UltimateFixPlugin.LogDebug("FOUND GameObject named 'LocalTTSManager'!");
                    LocalTTSManager tts = ttsObj.GetComponent<LocalTTSManager>();
                    if (tts != null)
                    {
                        UltimateFixPlugin.LogDebug("It HAS the LocalTTSManager component! Forcing speech...");
                        try
                        {
                            // tts.Speak("Ko ko ro Text to Speech enabled!", Character.Eddie);
                            hasTestedTTS = true;
                        }
                        catch (Exception ex)
                        {
                            UltimateFixPlugin.LogDebug("Error triggering LocalTTSManager: " + ex.ToString());
                            hasTestedTTS = true;
                        }
                    }
                    else
                    {
                        UltimateFixPlugin.LogDebug("It DOES NOT have the LocalTTSManager component! Stripped?");
                        hasTestedTTS = true;
                    }
                }
                else
                {
                    // Maybe it's inactive? Let's check all transforms
                    foreach (Transform t in Resources.FindObjectsOfTypeAll<Transform>())
                    {
                        if (t.name == "LocalTTSManager")
                        {
                            UltimateFixPlugin.LogDebug("FOUND INACTIVE GameObject named 'LocalTTSManager'!");
                            hasTestedTTS = true;
                            break;
                        }
                    }
                }
            }
        }
    }
}