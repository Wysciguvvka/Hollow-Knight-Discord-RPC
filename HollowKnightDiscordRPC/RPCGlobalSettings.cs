namespace HollowKnightDiscordRPC {
    public class RPCGlobalSettings {
        public bool ShowCurrentArea = true;
        public bool ShowMode = true;
        public int TimePlayedMode = 1; // 0 - hide 1 - partial 2 - total
        public int PlayerStatus01 = 1; // health
        public int PlayerStatus02 = 2; // ???
        public int PlayerStatus03 = 3; // ????
        public int PlayerStatus04 = 0; // none
        public bool ShowResting = true;
        public bool ShowPause = true;
        public bool HideEverything = false;
        /*
        // description in settings duh
        public string[] names = {
            "setting1",
            "setting2",
            "setting3",
            "setting4",
            "setting5",
        };
        public Dictionary<string, string[]> Values = new Dictionary<string, string[]>() {
            {"setting1", new string[] {"value1", "value2", "value3"} },
            {"setting2", new string[] {"value1", "value2", "value3"} },
            {"setting3", new string[] {"value1", "value2", "value3"} },
            {"setting4", new string[] {"value1", "value2", "value3"} },
            {"setting5", new string[] {"value1", "value2", "value3"} },
        };
        public Dictionary<string, string> Descriptions = new Dictionary<string, string>() {
            {"setting1", "Desc1" },
            {"setting2", "Desc2" },
            {"setting3", "Desc3" },
            {"setting4", "Desc4" },
            {"setting5", "Desc5" },
        };
        public Dictionary<string, int> IntValues = new Dictionary<string, int>() {
            {"setting1", 0 },
            {"setting2", 1 },
            {"setting3", 2 },
            {"setting4", 2 },
            {"setting5", 2 },
        };
        */
    }
}
