using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
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

        public override void Entry(IModHelper helper)
        {
            Monitor.Log(helper.Translation.Get("general.startup"), LogLevel.Debug);

            monitor = Monitor;
            ModEntry.helper = Helper;
            harmony = new(ModManifest.UniqueID);
            ModID = ModManifest.UniqueID;
            config = helper.ReadConfig<Config>();
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            harmony.PatchAll();
        }
        private void OnGameLaunched(object sender, GameLaunchedEventArgs ev)
        {
            config.RegisterModConfigMenu(ModManifest);
        }
    }
}
