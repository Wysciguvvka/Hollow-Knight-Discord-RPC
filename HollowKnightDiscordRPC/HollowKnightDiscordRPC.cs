using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Discord;
using Modding;
using Modding.Menu;
using Modding.Menu.Config;
using UnityEngine;
using UnityEngine.UI;

namespace HollowKnightDiscordRPC {
    public class HollowKnightDiscordRPC : Mod, ITogglableMod, ICustomMenuMod, IGlobalSettings<RPCGlobalSettings> {
        public HollowKnightDiscordRPC() : base("Rich Presence") { }
        public Discord.Discord discord = null;
        public ActivityManager activityManager;
        public GameObject obj;
        public bool ToggleButtonInsideMenu => true;
        public Activity activity = new Activity();
        private Activity _activity;
        private int totalBossHp = 0;
        private int currentBossHp;
        private string currentBossName;
        private readonly DateTime startDate = DateTime.UtcNow;
        private DateTime elapsedTime = DateTime.UtcNow;
        private readonly List<GameObject> bosses = new List<GameObject>();
        private RPCGlobalSettings GlobalSettings = new RPCGlobalSettings();
        internal MenuScreen screen;
        public override string GetVersion() {
            return "1.5.8";
        }
        public void OnLoadGlobal(RPCGlobalSettings s) {
            GlobalSettings = s;
        }
        public RPCGlobalSettings OnSaveGlobal() {
            return GlobalSettings;
        }
        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates) {
            var dels = toggleDelegates.Value;
            void cancelAction(MenuSelectable _) {
                dels.ApplyChange();
                UIManager.instance.UIGoToDynamicMenu(modListMenu);
            }
            this.screen = new MenuBuilder(UIManager.instance.UICanvas.gameObject, "DiscordRPCMenu")
                .CreateTitle("Discord Rich Presence", MenuTitleStyle.vanillaStyle)
                .CreateContentPane(RectTransformData.FromSizeAndPos(
                    new RelVector2(new Vector2(1920f, 903f)),
                    new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -60f)
                    )
                ))
                .CreateControlPane(RectTransformData.FromSizeAndPos(
                    new RelVector2(new Vector2(1920f, 259f)),
                    new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -502f)
                    )
                ))
                .SetDefaultNavGraph(new GridNavGraph(1))
                .AddContent(
                     new NullContentLayout(),
                            cs => cs.AddScrollPaneContent(
                                new ScrollbarConfig {
                                    Navigation = new Navigation { mode = Navigation.Mode.Vertical },
                                    Position = new AnchoredPosition {
                                        ChildAnchor = new Vector2(0f, 1f),
                                        ParentAnchor = new Vector2(1f, 1f),
                                        Offset = new Vector2(-310f, 0f)
                                    },
                                },
                                new RelLength(1685f),
                                RegularGridLayout.CreateVerticalLayout(105f),
                    c => {
                        c.AddHorizontalOption(
                            "ToggleModOption",
                            new HorizontalOptionConfig {
                                Label = "Mod Enabled",
                                Options = new string[] { "Off", "On" },
                                ApplySetting = (_, i) => dels.SetModEnabled(i == 1),
                                RefreshSetting = (s, _) => s.optionList.SetOptionTo(dels.GetModEnabled() ? 1 : 0),
                                CancelAction = cancelAction
                            },
                            out var toggleModOption
                        ).AddHorizontalOption(
                            "ShowCurrentArea",
                            new HorizontalOptionConfig {
                                Label = "Show Current Area",
                                Options = new[] { "Off", "On" },
                                ApplySetting = (_, i) => { GlobalSettings.ShowCurrentArea = Convert.ToBoolean(i); },
                                RefreshSetting = (s, _) => s.optionList.SetOptionTo(Convert.ToInt32(GlobalSettings.ShowCurrentArea)),
                                CancelAction = cancelAction,
                                Style = HorizontalOptionStyle.VanillaStyle,
                                Description = new DescriptionInfo {
                                    Text = "Shows your current location"
                                }
                            }, out var showCurrentArea)
                        .AddHorizontalOption(
                            "ShowSmallImage",
                            new HorizontalOptionConfig {
                                Label = "Show Small Image",
                                Options = new[] { "Off", "On" },
                                ApplySetting = (_, i) => { GlobalSettings.ShowSmallImage = Convert.ToBoolean(i); },
                                RefreshSetting = (s, _) => s.optionList.SetOptionTo(Convert.ToInt32(GlobalSettings.ShowSmallImage)),
                                CancelAction = cancelAction,
                                Style = HorizontalOptionStyle.VanillaStyle,
                                Description = new DescriptionInfo {
                                    Text = "Shows small image (area). (Won't show when location is turned off)"
                                }
                            }, out var showSmallImage)
                        .AddHorizontalOption(
                            "ShowMode",
                            new HorizontalOptionConfig {
                                Label = "Show Game Mode",
                                Options = new[] { "Off", "On" },
                                ApplySetting = (_, i) => { GlobalSettings.ShowMode = Convert.ToBoolean(i); },
                                RefreshSetting = (s, _) => s.optionList.SetOptionTo(Convert.ToInt32(GlobalSettings.ShowMode)),
                                CancelAction = cancelAction,
                                Style = HorizontalOptionStyle.VanillaStyle,
                                Description = new DescriptionInfo {
                                    Text = "Shows your game mode"
                                }
                            }, out var showGameMode)
                        .AddHorizontalOption(
                            "ShowBossMode",
                            new HorizontalOptionConfig {
                                Label = "Show Boss Health",
                                Options = new[] { "Off", "At Area Status", "At Player Stats" },
                                ApplySetting = (_, i) => { GlobalSettings.ShowBossMode = i; },
                                RefreshSetting = (s, _) => s.optionList.SetOptionTo(GlobalSettings.ShowBossMode),
                                CancelAction = cancelAction,
                                Style = HorizontalOptionStyle.VanillaStyle,
                                Description = new DescriptionInfo {
                                    Text = "Shows Boss Health (and optional HoG mode) when in fight "
                                }
                            }, out var showBossMode)
                        .AddHorizontalOption(
                            "ShowTime",
                            new HorizontalOptionConfig {
                                Label = "Show Time Played",
                                Options = new[] { "None", "Per room", "Total" },
                                ApplySetting = (_, i) => { GlobalSettings.TimePlayedMode = i; },
                                RefreshSetting = (s, _) => s.optionList.SetOptionTo(GlobalSettings.TimePlayedMode),
                                CancelAction = cancelAction,
                                Style = HorizontalOptionStyle.VanillaStyle,
                                Description = new DescriptionInfo {
                                    Text = "Shows time played"
                                }
                            }, out var showTimePlayed)
                        .AddTextPanel(
                            "RichPresenceStats",
                            new RelVector2(new RelLength(1000), new RelLength(105f)),
                            new TextPanelConfig {
                                Text = "Rich Presence Stats",
                                Font = TextPanelConfig.TextFont.TrajanBold,
                                Size = 42,
                                Anchor = TextAnchor.MiddleCenter
                            }, out var statsText)
                        .AddHorizontalOption(
                            "StatsFirst",
                            new HorizontalOptionConfig {
                                Label = "First Position",
                                Options = new[] { "None", "Health", "Soul", "Geo", "Grubs", "Completion", "Delicate Flower", "Essence", "Hunter's Mark", "Overcharmed" },
                                ApplySetting = (_, i) => { GlobalSettings.PlayerStatus01 = i; },
                                RefreshSetting = (s, _) => s.optionList.SetOptionTo(GlobalSettings.PlayerStatus01),
                                CancelAction = cancelAction,
                                Style = HorizontalOptionStyle.VanillaStyle,
                                Description = new DescriptionInfo {
                                    Text = "Customize your Rich Presence stats"
                                }
                            }, out var playerStats01)
                        .AddHorizontalOption(
                            "StatsSecond",
                            new HorizontalOptionConfig {
                                Label = "Second Position",
                                Options = new[] { "None", "Health", "Soul", "Geo", "Grubs", "Completion", "Delicate Flower", "Essence", "Hunter's Mark", "Overcharmed" },
                                ApplySetting = (_, i) => { GlobalSettings.PlayerStatus02 = i; },
                                RefreshSetting = (s, _) => s.optionList.SetOptionTo(GlobalSettings.PlayerStatus02),
                                CancelAction = cancelAction,
                                Style = HorizontalOptionStyle.VanillaStyle,
                                Description = new DescriptionInfo {
                                    Text = "Customize your Rich Presence stats"
                                }
                            }, out var playerStats02)
                        .AddHorizontalOption(
                            "StatsThird",
                            new HorizontalOptionConfig {
                                Label = "Third Position",
                                Options = new[] { "None", "Health", "Soul", "Geo", "Grubs", "Completion", "Delicate Flower", "Essence", "Hunter's Mark", "Overcharmed" },
                                ApplySetting = (_, i) => { GlobalSettings.PlayerStatus03 = i; },
                                RefreshSetting = (s, _) => s.optionList.SetOptionTo(GlobalSettings.PlayerStatus03),
                                CancelAction = cancelAction,
                                Style = HorizontalOptionStyle.VanillaStyle,
                                Description = new DescriptionInfo {
                                    Text = "Customize your Rich Presence stats"
                                }
                            }, out var playerStats03)
                        .AddHorizontalOption(
                            "StatsFourth",
                            new HorizontalOptionConfig {
                                Label = "Fourth Position",
                                Options = new[] { "None", "Health", "Soul", "Geo", "Grubs", "Completion", "Delicate Flower", "Essence", "Hunter's Mark", "Overcharmed" },
                                ApplySetting = (_, i) => { GlobalSettings.PlayerStatus04 = i; },
                                RefreshSetting = (s, _) => s.optionList.SetOptionTo(GlobalSettings.PlayerStatus04),
                                CancelAction = cancelAction,
                                Style = HorizontalOptionStyle.VanillaStyle,
                                Description = new DescriptionInfo {
                                    Text = "Customize your Rich Presence stats"
                                }
                            }, out var playerStats04)
                        .AddTextPanel(
                            "Others",
                            new RelVector2(new RelLength(1000), new RelLength(105f)),
                            new TextPanelConfig {
                                Text = "Other Options",
                                Font = TextPanelConfig.TextFont.TrajanBold,
                                Size = 42,
                                Anchor = TextAnchor.MiddleCenter
                            }, out var otherOptions)
                        .AddHorizontalOption(
                            "ShowResting",
                            new HorizontalOptionConfig {
                                Label = "Show Player Resting",
                                Options = new[] { "Off", "On" },
                                ApplySetting = (_, i) => { GlobalSettings.ShowResting = Convert.ToBoolean(i); },
                                RefreshSetting = (s, _) => s.optionList.SetOptionTo(Convert.ToInt32(GlobalSettings.ShowResting)),
                                CancelAction = cancelAction,
                                Style = HorizontalOptionStyle.VanillaStyle,
                                Description = new DescriptionInfo {
                                    Text = "Shows whether The Knight is resting on a bench"
                                }
                            }, out var showResting)
                        .AddHorizontalOption(
                            "ShowPause",
                            new HorizontalOptionConfig {
                                Label = "Show Game Pause",
                                Options = new[] { "Off", "On" },
                                ApplySetting = (_, i) => { GlobalSettings.ShowPause = Convert.ToBoolean(i); },
                                RefreshSetting = (s, _) => s.optionList.SetOptionTo(Convert.ToInt32(GlobalSettings.ShowPause)),
                                CancelAction = cancelAction,
                                Style = HorizontalOptionStyle.VanillaStyle,
                                Description = new DescriptionInfo {
                                    Text = "Shows whether game is paused"
                                }
                            }, out var showPause)
                        .AddHorizontalOption(
                            "HideEverything",
                            new HorizontalOptionConfig {
                                Label = "Hide Everything",
                                Options = new[] { "Off", "On" },
                                ApplySetting = (_, i) => { GlobalSettings.HideEverything = Convert.ToBoolean(i); },
                                RefreshSetting = (s, _) => s.optionList.SetOptionTo(Convert.ToInt32(GlobalSettings.HideEverything)),
                                CancelAction = cancelAction,
                                Style = HorizontalOptionStyle.VanillaStyle,
                                Description = new DescriptionInfo {
                                    Text = "Hides In-Game stats and events"
                                }
                            }, out var hideEverything)
                        .AddMenuButton(
                            "LoadTriggersButton",
                            new MenuButtonConfig {
                                Label = "Reset to Defaults",
                                SubmitAction = _ => {
                                    GlobalSettings.ShowCurrentArea = true;
                                    GlobalSettings.ShowMode = true;
                                    GlobalSettings.ShowBossMode = 2;
                                    GlobalSettings.TimePlayedMode = 1;
                                    GlobalSettings.PlayerStatus01 = 1;
                                    GlobalSettings.PlayerStatus02 = 2;
                                    GlobalSettings.PlayerStatus03 = 3;
                                    GlobalSettings.PlayerStatus04 = 0;
                                    GlobalSettings.ShowResting = true;
                                    GlobalSettings.ShowPause = true;
                                    GlobalSettings.HideEverything = false;
                                    GlobalSettings.ShowSmallImage = true;
                                    showCurrentArea.menuSetting.RefreshValueFromGameSettings();
                                    showGameMode.menuSetting.RefreshValueFromGameSettings();
                                    showBossMode.menuSetting.RefreshValueFromGameSettings();
                                    showTimePlayed.menuSetting.RefreshValueFromGameSettings();
                                    playerStats01.menuSetting.RefreshValueFromGameSettings();
                                    playerStats02.menuSetting.RefreshValueFromGameSettings();
                                    playerStats03.menuSetting.RefreshValueFromGameSettings();
                                    playerStats04.menuSetting.RefreshValueFromGameSettings();
                                    showResting.menuSetting.RefreshValueFromGameSettings();
                                    showPause.menuSetting.RefreshValueFromGameSettings();
                                    showSmallImage.menuSetting.RefreshValueFromGameSettings();
                                    hideEverything.menuSetting.RefreshValueFromGameSettings();
                                },
                                CancelAction = cancelAction,
                                Style = MenuButtonStyle.VanillaStyle
                            }
                        );
                        showCurrentArea.menuSetting.RefreshValueFromGameSettings();
                        showGameMode.menuSetting.RefreshValueFromGameSettings();
                        showBossMode.menuSetting.RefreshValueFromGameSettings();
                        showTimePlayed.menuSetting.RefreshValueFromGameSettings();
                        playerStats01.menuSetting.RefreshValueFromGameSettings();
                        playerStats02.menuSetting.RefreshValueFromGameSettings();
                        playerStats03.menuSetting.RefreshValueFromGameSettings();
                        playerStats04.menuSetting.RefreshValueFromGameSettings();
                        showResting.menuSetting.RefreshValueFromGameSettings();
                        showPause.menuSetting.RefreshValueFromGameSettings();
                        showSmallImage.menuSetting.RefreshValueFromGameSettings();
                        hideEverything.menuSetting.RefreshValueFromGameSettings();
                        toggleModOption.GetComponent<MenuSetting>().RefreshValueFromGameSettings();
                    })
                )
                .AddControls(
                    new SingleContentLayout(new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -64f)
                    )),
                    c => c.AddMenuButton(
                        "BackButton",
                        new MenuButtonConfig {
                            Label = "Back",
                            CancelAction = cancelAction,
                            SubmitAction = cancelAction,
                            Style = MenuButtonStyle.VanillaStyle,
                            Proceed = true
                        },
                        out var backButton
                    )
                )
                .Build();
            return this.screen;
        }
        private string GetRPCPlayerStatus(int statID) {
            string combinedState = null;
            switch (statID) {
                case 0:
                    break;
                case 1:
                    combinedState = $"HP: {GameManager.instance.playerData.health}/{GameManager.instance.playerData.maxHealth}";
                    break;
                case 2:
                    combinedState = $"Soul: {GameManager.instance.playerData.MPCharge}";
                    break;
                case 3:
                    combinedState = $"Geo: {GameManager.instance.playerData.geo}";
                    break;
                case 4:
                    combinedState = $"Grubs: {GameManager.instance.playerData.grubsCollected}";
                    break;
                case 5:
                    if (GameManager.instance.playerData.unlockedCompletionRate) { combinedState = $"{GameManager.instance.playerData.completionPercentage}%"; }
                    break;
                case 6:
                    if (!GameManager.instance.playerData.xunFlowerBroken && GameManager.instance.playerData.hasXunFlower) { combinedState = $"Delicate Flower"; }
                    break;
                case 7:
                    combinedState = $"Essence: {GameManager.instance.playerData.dreamOrbs}";
                    break;
                case 8:
                    if (GameManager.instance.playerData.hasHuntersMark) { combinedState = $"Hunter's Mark"; }
                    break;
                case 9:
                    if (GameManager.instance.playerData.overcharmed) { combinedState = $"Overcharmed"; }
                    break;
                default:
                    break;
            }
            return !string.IsNullOrEmpty(combinedState) ? $"{combinedState} | " : null; // delim
        }
        private string GetRPCState() {
            string RPCState = null;
            RPCState += $"{GetRPCPlayerStatus(GlobalSettings.PlayerStatus01)}";
            RPCState += $"{GetRPCPlayerStatus(GlobalSettings.PlayerStatus02)}";
            RPCState += $"{GetRPCPlayerStatus(GlobalSettings.PlayerStatus03)}";
            RPCState += $"{GetRPCPlayerStatus(GlobalSettings.PlayerStatus04)}";
            return RPCState.Trim('|', ' ');
        }
        private void UpdatePlayerActivityData() {
            // todo: rewrite the code below
            if (HeroController.instance != null) {
                if (!GlobalSettings.HideEverything) {
                    string mode = "Hollow Knight";
                    string largeImage = "classic";
                    if (GlobalSettings.ShowMode) {
                        if (GameManager.instance.playerData.permadeathMode > 0) { mode += " (Steel Soul)"; largeImage = "steel"; }
                        else if (GameManager.instance.playerData.bossRushMode) { mode += " (Godseeker)"; largeImage = "godseeker"; }
                        else mode += " (Classic)";
                    }
                    string currentScene = "In Game ";
                    string action = null;
                    string gameState = null;
                    string areaName = null;
                    if (GlobalSettings.ShowCurrentArea) {
                        currentScene = GameManager.instance.GetSceneNameString();
                        areaName = SceneData.GetAreaName(currentScene);
                        if (!SceneData.IsInExcludedScenes(currentScene)) {
                            activity.Assets.SmallImage = SceneData.GetSceneImage(currentScene);
                            activity.Assets.SmallText = areaName;
                        }
                        if (!BossSequenceController.IsInSequence) { currentScene = SceneData.GetSceneArea(currentScene); }
                        else { currentScene = SceneData.GetPantheonName(PlayerData.instance.bossReturnEntryGate); }
                    }
                    if (!GlobalSettings.ShowSmallImage || !GlobalSettings.ShowCurrentArea) {
                        activity.Assets.SmallImage = null;
                        activity.Assets.SmallText = null;
                    }
                    if (GlobalSettings.ShowResting && GameManager.instance.playerData.atBench && !SceneData.IsInExcludedScenes(GameManager.instance.GetSceneNameString())) action = " (Resting)";
                    if (GameManager.instance.IsGamePaused() && GlobalSettings.ShowPause) gameState = " (Paused)";
                    activity.Details = $"{currentScene}{action}";
                    activity.State = GetRPCState();
                    activity.Assets.LargeText = $"{mode}{gameState}";
                    activity.Assets.LargeImage = largeImage;
                    activity.Timestamps = new ActivityTimestamps();
                    if (GlobalSettings.TimePlayedMode == 1 && !SceneData.IsInExcludedScenes(GameManager.instance.GetSceneNameString())) {
                        var elapsed = Math.Abs((elapsedTime - new DateTime(1970, 1, 1)).TotalSeconds);
                        activity.Timestamps.Start = (long)elapsed;
                    }
                    else if (GlobalSettings.TimePlayedMode == 2) {
                        var elapsed = Math.Abs((startDate - new DateTime(1970, 1, 1)).TotalSeconds);
                        activity.Timestamps.Start = (long)elapsed;
                    }                   
                    if (GlobalSettings.ShowBossMode > 0 && bosses.Count > 0) { // boss.count > 0 disables 0% hp status                                                                              
                        // boss scene                       
                        try {
                            int bossPercentHp = (100 * currentBossHp) / totalBossHp;
                            string bossStatus = null;
                            if (BossSceneController.Instance != null) {  // HoG
                                if (BossSceneController.Instance.BossLevel == 1) { bossStatus = $"(Ascended)"; }
                                else if (BossSceneController.Instance.BossLevel == 2) { bossStatus = "(Radiant)"; }
                            }
                            if (GlobalSettings.ShowBossMode == 1) { // at area
                                if (GlobalSettings.ShowCurrentArea && currentScene.EndsWith(currentBossName)) { bossStatus = $"{activity.Details}: {bossPercentHp}% {bossStatus}"; }
                                else { bossStatus = $"{activity.Details} - {currentBossName}: {bossPercentHp}% {bossStatus}"; }
                                activity.Details = bossStatus;
                            }
                            else if (GlobalSettings.ShowBossMode == 2) {
                                bossStatus = $"{currentBossName}: {bossPercentHp}% {bossStatus}";
                                activity.State = bossStatus;
                            }
                        }
                        catch { return; }
                    }
                }
                else {
                    activity.Details = "In Game";
                    activity.State = null;
                    activity.Assets.LargeText = "Hollow Knight";
                    activity.Assets.LargeImage = "classic";
                    activity.Assets.SmallImage = null;
                    activity.Timestamps = new ActivityTimestamps();
                }
            }
            else {
                activity.Details = "In Menu";
                activity.State = null;
                activity.Assets.LargeText = "Hollow Knight";
                activity.Assets.LargeImage = "classic";
                activity.Assets.SmallImage = null;
                activity.Timestamps = new ActivityTimestamps();
                if (!GlobalSettings.HideEverything) {
                    var elapsed = Math.Abs((startDate - new DateTime(1970, 1, 1)).TotalSeconds);
                    activity.Timestamps.Start = (long)elapsed;
                }
            }
            try {
                if (!activity.Equals(_activity)) {
                    _activity = activity;
                    activityManager.UpdateActivity(activity, (res) => { });
                }
            }
            catch (Exception) {
                discord = null;
            }
        }
        public void ExtractResourceToFile(string resourceName, string filename) {
            if (!File.Exists(filename)) {
                using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                using (FileStream fs = new FileStream(filename, FileMode.Create)) {
                    byte[] b = new byte[s.Length];
                    s.Read(b, 0, b.Length);
                    fs.Write(b, 0, b.Length);
                }
            }
        }
        public override void Initialize() {
            if (!Directory.Exists(".\\hollow_knight_Data\\Plugins")) Directory.CreateDirectory(".\\hollow_knight_Data\\Plugins");
            if (!Directory.Exists(".\\hollow_knight_Data\\Plugins\\x86_64")) Directory.CreateDirectory(".\\hollow_knight_Data\\Plugins\\x86_64");
            if (!Directory.Exists(".\\hollow_knight_Data\\Plugins\\x86")) Directory.CreateDirectory(".\\hollow_knight_Data\\Plugins\\x86");
            ExtractResourceToFile("HollowKnightDiscordRPC.Assets.Plugins.x86.discord_game_sdk.dll", ".\\hollow_knight_Data\\Plugins\\x86\\discord_game_sdk.dll");
            ExtractResourceToFile("HollowKnightDiscordRPC.Assets.Plugins.x86.discord_game_sdk.dll.lib", ".\\hollow_knight_Data\\Plugins\\x86\\discord_game_sdk.dll.lib");
            ExtractResourceToFile("HollowKnightDiscordRPC.Assets.Plugins.x86_64.discord_game_sdk.dll", ".\\hollow_knight_Data\\Plugins\\x86_64\\discord_game_sdk.dll");
            ExtractResourceToFile("HollowKnightDiscordRPC.Assets.Plugins.x86_64.discord_game_sdk.dll.lib", ".\\hollow_knight_Data\\Plugins\\x86_64\\discord_game_sdk.dll.lib");
            ExtractResourceToFile("HollowKnightDiscordRPC.Assets.Plugins.x86_64.discord_game_sdk.bundle", ".\\hollow_knight_Data\\Plugins\\x86_64\\discord_game_sdk.bundle");
            ExtractResourceToFile("HollowKnightDiscordRPC.Assets.Plugins.x86_64.discord_game_sdk.dylib", ".\\hollow_knight_Data\\Plugins\\x86_64\\discord_game_sdk.dylib");
            ExtractResourceToFile("HollowKnightDiscordRPC.Assets.Plugins.x86_64.discord_game_sdk.so", ".\\hollow_knight_Data\\Plugins\\x86_64\\discord_game_sdk.so");
            obj = new GameObject();
            obj.AddComponent<Updater>().RegisterMod(this);
            UnityEngine.Object.DontDestroyOnLoad(obj);
            InitDiscord();
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
            ModHooks.HeroUpdateHook += OnHeroUpdate;
            ModHooks.OnEnableEnemyHook += EnemyEnabled;
        }
        public void Update() {
            try {
                if (discord is null || activityManager is null) { InitDiscord(); return; }
                UpdatePlayerActivityData();
                discord.RunCallbacks();
            }
            catch (Exception e) {
                discord = null;
                if (e.Message == "NotRunning") InitDiscord();
            }
        }
        public void Unload() {
            GameObject.DestroyImmediate(obj);
            bosses.Clear();
            discord?.Dispose();
            discord = null;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnSceneChanged;
            ModHooks.HeroUpdateHook -= OnHeroUpdate;
            ModHooks.OnEnableEnemyHook -= EnemyEnabled;
        }
        private bool EnemyEnabled(GameObject enemy, bool isDead) {
            try {
                var healthManager = enemy.GetComponent<HealthManager>();
                if (healthManager == null || isDead) return isDead;
                if (SceneData.bossList.Any(enemy.name.Contains)) {
                    if (!bosses.Contains(enemy)) {
                        totalBossHp += healthManager.hp;
                        currentBossHp += healthManager.hp;
                        bosses.Add(enemy);
                        currentBossName = SceneData.GetBossName(enemy.name);
                    }
                }
                return isDead;
            }
            catch { return isDead; }
        }
        private void OnHeroUpdate() {
            if (bosses.Count > 0) {
                currentBossHp = 0;
                foreach (var boss in bosses) {
                    try {
                        var healthManager = boss.GetComponent<HealthManager>();
                        currentBossHp += Math.Max(0, healthManager.hp);
                        if (healthManager.hp <= 0) { bosses.Remove(boss); break; } // removes boss from list when dead
                    }
                    catch(Exception) { // thrown after boss death 
                        bosses.Remove(boss);
                        break; // continue causes "Collection was modified; enumeration operation may not execute" error.
                    }
                }
            }
            else {
                currentBossName = null;
                totalBossHp = 0;
                currentBossHp = 0;
            }

        }
        private void OnSceneChanged(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to) {
            elapsedTime = DateTime.UtcNow;
            UpdatePlayerActivityData();
        }
        private void InitDiscord() {
            try {
                discord?.Dispose();
                discord = new Discord.Discord(925354823304507433, (System.UInt64)CreateFlags.NoRequireDiscord);
                activityManager = discord.GetActivityManager();
                UpdatePlayerActivityData();
            }
            catch { return; } // not logging due to log spam
        }
    }
    public class Updater : MonoBehaviour {
        public HollowKnightDiscordRPC mod;
        public void RegisterMod(HollowKnightDiscordRPC mod) {
            this.mod = mod;
        }
        public void Update() {
            if (mod != null) {
                mod.Update();
            }
        }
    }
}

