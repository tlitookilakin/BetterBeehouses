using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using BetterBeehouses.integration;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;

namespace BetterBeehouses
{
    public class ModEntry : Mod
    {
        internal ITranslationHelper i18n => Helper.Translation;
        internal static IMonitor monitor;
        internal static IModHelper helper;
        internal static Harmony harmony;
        internal static string ModID;
        internal static Config config;
        internal static API api;

        public override void Entry(IModHelper helper)
        {
            Monitor.Log(helper.Translation.Get("general.startup"), LogLevel.Debug);

            monitor = Monitor;
            ModEntry.helper = Helper;
            harmony = new(ModManifest.UniqueID);
            ModID = ModManifest.UniqueID;
            config = helper.ReadConfig<Config>();
            api = new();
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }
        private void OnGameLaunched(object sender, GameLaunchedEventArgs ev)
        {
            harmony.PatchAll();
            bool modPatched = PFMPatch.setup();
            modPatched = AutomatePatch.Setup() || modPatched;
            if (modPatched)
                monitor.Log(helper.Translation.Get("general.patchedModsWarning"),LogLevel.Warn);
            config.RegisterModConfigMenu(ModManifest);
        }
        public override object GetApi()
        {
            return api;
        }
    }
}
