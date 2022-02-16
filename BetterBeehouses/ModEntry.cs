﻿using HarmonyLib;
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
            harmony.PatchAll();
        }
        private void OnGameLaunched(object sender, GameLaunchedEventArgs ev)
        {
            config.RegisterModConfigMenu(ModManifest);
            if (PFMPatch.setup())
                monitor.Log(helper.Translation.Get("general.patchedModsWarning"),LogLevel.Warn);
        }
        public override object GetApi()
        {
            return api;
        }
    }
}
