using System;
using System.Collections.Generic;
using Discord;
using Modding;
using Modding.Menu;
using Modding.Menu.Config;
using UnityEngine;
using UnityEngine.UI;

namespace HollowKnightDiscordRPC {
    // todo: add regions
    public class HollowKnightDiscordRPC : Mod, ITogglableMod, ICustomMenuMod, IGlobalSettings<RPCGlobalSettings> {
        public HollowKnightDiscordRPC() : base("Rich Presence") { }
        public Discord.Discord discord = null;
        public ActivityManager activityManager;
        public Activity activity = new Activity();
        public GameObject obj;
        public bool ToggleButtonInsideMenu => true;
        private readonly DateTime startDate = DateTime.UtcNow;
        private DateTime elapsedTime = DateTime.UtcNow;
        private bool gamePaused;
        private RPCGlobalSettings GlobalSettings = new RPCGlobalSettings();
        internal MenuScreen screen;
        public static Dictionary<string, int> playerDictData = new Dictionary<string, int>() {
            { "geo" , 0 },
            { "soul" , 0 },
            { "health" , 0 },
            { "maxhealth" , 0 },
            { "grubs" , 0 },
            { "essence" , 0 },
        };
        public override string GetVersion() {
            return "1.5.3";
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
                                new RelLength(1365f),
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
                        .AddMenuButton(
                            "LoadTriggersButton",
                            new MenuButtonConfig {
                                Label = "Reset to Defaults",
                                SubmitAction = _ => {
                                    GlobalSettings.ShowCurrentArea = true;
                                    GlobalSettings.ShowMode = true;
                                    GlobalSettings.TimePlayedMode = 1;
                                    GlobalSettings.PlayerStatus01 = 1;
                                    GlobalSettings.PlayerStatus02 = 2;
                                    GlobalSettings.PlayerStatus03 = 3;
                                    GlobalSettings.PlayerStatus04 = 0;
                                    GlobalSettings.ShowResting = true;
                                    GlobalSettings.ShowPause = true;
                                    GlobalSettings.HideEverything = false;
                                    showCurrentArea.menuSetting.RefreshValueFromGameSettings();
                                    showGameMode.menuSetting.RefreshValueFromGameSettings();
                                    showTimePlayed.menuSetting.RefreshValueFromGameSettings();
                                    playerStats01.menuSetting.RefreshValueFromGameSettings();
                                    playerStats02.menuSetting.RefreshValueFromGameSettings();
                                    playerStats03.menuSetting.RefreshValueFromGameSettings();
                                    playerStats04.menuSetting.RefreshValueFromGameSettings();
                                    showResting.menuSetting.RefreshValueFromGameSettings();
                                    showPause.menuSetting.RefreshValueFromGameSettings();
                                },
                                CancelAction = cancelAction,
                                Style = MenuButtonStyle.VanillaStyle
                            }
                        );
                        showCurrentArea.menuSetting.RefreshValueFromGameSettings();
                        showGameMode.menuSetting.RefreshValueFromGameSettings();
                        showTimePlayed.menuSetting.RefreshValueFromGameSettings();
                        playerStats01.menuSetting.RefreshValueFromGameSettings();
                        playerStats02.menuSetting.RefreshValueFromGameSettings();
                        playerStats03.menuSetting.RefreshValueFromGameSettings();
                        playerStats04.menuSetting.RefreshValueFromGameSettings();
                        showResting.menuSetting.RefreshValueFromGameSettings();
                        showPause.menuSetting.RefreshValueFromGameSettings();

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
                    combinedState = $"HP: {playerDictData["health"]}/{playerDictData["maxhealth"]}";
                    break;
                case 2:
                    combinedState = $"Soul: {playerDictData["soul"]}";
                    break;
                case 3:
                    combinedState = $"Geo: {playerDictData["geo"]}";
                    break;
                case 4:
                    combinedState = $"Grubs: {playerDictData["grubs"]}";
                    break;
                case 5:
                    if (GameManager.instance.playerData.unlockedCompletionRate) { combinedState = $"{playerDictData["completion"]}%"; }
                    break;
                case 6:
                    if (!GameManager.instance.playerData.xunFlowerBroken && GameManager.instance.playerData.hasXunFlower) { combinedState = $"Delicate Flower"; }
                    break;
                case 7:
                    combinedState = $"Essence: {playerDictData["essence"]}";
                    break;
                case 8:
                    if (GameManager.instance.playerData.overcharmed) { combinedState = $"Hunter's Mark"; }
                    break;
                case 9:
                    if (GameManager.instance.playerData.hasHuntersMark) { combinedState = $"Overcharmed"; }
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
            if (HeroController.instance != null) {
                // todo: pantheon bindings, boss info
                // CheckGGBossLevel?
                // todo: find a better way to detect game mode
                if (!GlobalSettings.HideEverything) {
                    string mode = "Hollow Knight";
                    string largeImage = "classic";
                    if (GlobalSettings.ShowMode) {
                        if (GameManager.instance.playerData.permadeathMode > 0) { mode += " (Steel Soul)"; largeImage = "steel"; }
                        else if (GameManager.instance.playerData.bossRushMode) { mode += " (Godseeker)"; largeImage = "godseeker"; }
                        else mode += " (Classic)";
                    }
                    string currentScene = "In Game";
                    string action = null;
                    string gameState = null;
                    if (GlobalSettings.ShowCurrentArea) {
                        currentScene = GameManager.instance.GetSceneNameString();
                        string areaName = SceneData.GetAreaName(currentScene);
                        if (!SceneData.IsInExcludedScenes(currentScene)) {
                            activity.Assets.SmallImage = SceneData.GetSceneImage(currentScene);
                            activity.Assets.SmallText = areaName;
                        }
                        currentScene = SceneData.GetSceneArea(currentScene);
                    }
                    else {
                        activity.Assets.SmallImage = null;
                        activity.Assets.SmallText = null;
                    }
                    if (GlobalSettings.ShowResting && GameManager.instance.playerData.atBench && !SceneData.IsInExcludedScenes(GameManager.instance.GetSceneNameString())) action = " (Resting)";
                    if (GameManager.instance.IsGamePaused() && GlobalSettings.ShowPause) gameState = " (Paused)";
                    activity.Details = $"{currentScene}{action}";
                    activity.State = GetRPCState();
                    activity.Assets.LargeText = $"{mode}{gameState}";
                    activity.Assets.LargeImage = largeImage; // todo: change
                    activity.Timestamps = new ActivityTimestamps();
                    if (GlobalSettings.TimePlayedMode == 1 && !SceneData.IsInExcludedScenes(GameManager.instance.GetSceneNameString())) {
                        var elapsed = Math.Abs((elapsedTime - new DateTime(1970, 1, 1)).TotalSeconds);
                        activity.Timestamps.Start = (long)elapsed;
                    }
                    else if (GlobalSettings.TimePlayedMode == 2) {
                        var elapsed = Math.Abs((startDate - new DateTime(1970, 1, 1)).TotalSeconds);
                        activity.Timestamps.Start = (long)elapsed;
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
                activityManager.UpdateActivity(activity, (res) => { });
            }
            catch (Exception) {
                discord = null;
            }
        }

        public override void Initialize() {
            obj = new GameObject();
            obj.AddComponent<Updater>().RegisterMod(this);
            UnityEngine.Object.DontDestroyOnLoad(obj);
            // UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneLoaded;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
            ModHooks.SetPlayerBoolHook += OnSetPlayerBool;
            ModHooks.SoulGainHook += GainSoul;
            ModHooks.SetPlayerIntHook += PlayerIntSet;
            ModHooks.HeroUpdateHook += OnHeroUpdate;
        }

        public void Update() {
            try {
                if (discord is null || activityManager is null) { InitDiscord(); return; }
                discord.RunCallbacks();
            }
            catch (Exception e) {
                discord = null;
                if (e.Message == "NotRunning") InitDiscord();
            }
        }
        public void Unload() {
            discord?.Dispose();
            GameObject.DestroyImmediate(obj);
            discord = null;
        }
        private void OnHeroUpdate() {
            try {
                // making sure that stats will be up to date at launch
                playerDictData["geo"] = GameManager.instance.playerData.geo;
                // playerDictData["soul"] = GameManager.instance.playerData.MPCharge; // updating soul here is not necessary
                playerDictData["health"] = GameManager.instance.playerData.health;
                playerDictData["maxhealth"] = GameManager.instance.playerData.maxHealth;
                playerDictData["grubs"] = GameManager.instance.playerData.grubsCollected;
                playerDictData["completion"] = (int)GameManager.instance.playerData.completionPercentage;
                playerDictData["essence"] = GameManager.instance.playerData.dreamOrbs;
                // pause toggle
                if (GameManager.instance.IsGamePaused() && !gamePaused) { gamePaused = true; UpdatePlayerActivityData(); }
                if (!GameManager.instance.IsGamePaused() && gamePaused) { gamePaused = false; UpdatePlayerActivityData(); }
            }
            catch { } // ???
        }
        private int PlayerIntSet(string target, int val) {
            // update presence only when needed
            if (target == "geo") { playerDictData["geo"] = val; UpdatePlayerActivityData(); }
            if (target == "MPCharge") { playerDictData["soul"] = val; UpdatePlayerActivityData(); }
            if (target == "health") { playerDictData["health"] = val; UpdatePlayerActivityData(); }
            if (target == "maxHealth") { playerDictData["maxhealth"] = val; UpdatePlayerActivityData(); } //health regens when acquired new mask (afaik) so probably not needed - chceck pls?
            if (target == "grubsCollected") { playerDictData["grubs"] = val; UpdatePlayerActivityData(); }
            return val;
        }
        private int GainSoul(int amount) {
            //UpdatePlayerActivityData();
            return amount;
        }
        private bool OnSetPlayerBool(string target, bool val) {
            if (target == "atBench") UpdatePlayerActivityData();
            if (target == "disablePause") UpdatePlayerActivityData();
            return val;
        }
        /*
        private void SceneLoaded(UnityEngine.SceneManagement.Scene targetArea, UnityEngine.SceneManagement.LoadSceneMode loadMode) {
            elapsedTime = DateTime.UtcNow; // move to OnSceneChanged?
            // UpdatePlayerActivityData();
        }
        */
        private void OnSceneChanged(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to) {
            elapsedTime = DateTime.UtcNow;
            UpdatePlayerActivityData();
        }
        private void InitDiscord() {
            discord?.Dispose();
            discord = new Discord.Discord(925354823304507433, (System.UInt64)CreateFlags.NoRequireDiscord);
            activityManager = discord.GetActivityManager();
            UpdatePlayerActivityData();
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

