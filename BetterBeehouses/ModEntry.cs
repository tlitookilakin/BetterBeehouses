using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using BetterBeehouses.integration;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;

namespace BetterBeehouses
{
    public class ModEntry : Mod, IAssetEditor, IAssetLoader
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
            monitor.Log(helper.Translation.Get("general.patchedModsWarning"), LogLevel.Trace);
            if (helper.ModRegistry.IsLoaded("Pathoschild.Automate") && !config.PatchAutomate)
                monitor.Log(i18n.Get("general.automatePatchDisabled"), LogLevel.Info);
            if (helper.ModRegistry.IsLoaded("Digus.ProducerFrameworkMod") && !config.PatchPFM)
                monitor.Log(i18n.Get("general.pfmPatchDisabled"), LogLevel.Info);
            harmony.PatchAll();
            PFMPatch.Setup();
            AutomatePatch.Setup();
            config.RegisterModConfigMenu(ModManifest);
        }
        public override object GetApi()
        {
            return api;
        }
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Mods/aedenthorn.ParticleEffects/dict") && config.Particles;
        }
        public void Edit<T>(IAssetData asset)
        {
            Utils.AddDictionaryEntry<T>(asset, "tlitookilakin.BetterBeehouses.Bees", "beeParticle.json");
        }
        public bool CanLoad<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Mods/BetterBeehouses/Bees");
        }
        public T Load<T>(IAssetInfo asset)
        {
            return helper.Content.Load<T>("assets/bees.png");
        }
    }
}
