using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using BetterBeehouses.integration;
using System;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData.Objects;
using BetterBeehouses.patches;
using BetterBeehouses.framework;

namespace BetterBeehouses
{
	public class ModEntry : Mod
	{
		internal static ITranslationHelper i18n;
		internal static IMonitor monitor;
		internal static IModHelper helper;
		internal static Harmony harmony;
		internal static string ModID;
		internal static Config config;
		internal static API api;
		internal static IAeroCoreAPI AeroCore;
		internal static Texture2D BeeTex => beeTex ??= helper.GameContent.Load<Texture2D>("Mods/BetterBeehouses/Bees");
		private static Texture2D beeTex;

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
			helper.Events.Content.AssetRequested += AssetRequested;
			i18n = helper.Translation;
		}
		private void OnGameLaunched(object sender, GameLaunchedEventArgs ev)
		{
			Utilities.Init();
			monitor.Log(helper.Translation.Get("general.patchedModsWarning"), LogLevel.Trace);
			if (helper.ModRegistry.IsLoaded("tlitookilakin.AeroCore") &&
				helper.ModRegistry.Get("tlitookilakin.AeroCore").Manifest.Version.IsNewerThan("0.9.4"))
				AeroCore = helper.ModRegistry.GetApi<IAeroCoreAPI>("tlitookilakin.AeroCore");
			BeeManager.Init();
			harmony.PatchAll();
			config.Patch();
			WildFlowers.Setup();
			config.RegisterModConfigMenu(ModManifest);
		}
		public override object GetApi()
			=> api;
		private void AssetRequested(object _, AssetRequestedEventArgs ev)
		{
			if (config.Particles && AeroCore is null && ev.NameWithoutLocale.IsEquivalentTo("Mods/aedenthorn.ParticleEffects/dict"))
				ev.Edit(data => Utils.AddDictionaryEntry(data, "tlitookilakin.BetterBeehouses.Bees", "beeParticle.json"));
			else if (ev.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
				ev.Edit(AddTags);
			else if (ev.NameWithoutLocale.IsEquivalentTo("Mods/BetterBeehouses/Bees"))
				ev.LoadFromModFile<Texture2D>("assets/bees.png", AssetLoadPriority.Medium);
			else if (ev.NameWithoutLocale.IsEquivalentTo("Data/Machines"))
				ev.Edit(MachineEditor.Edit, AssetEditPriority.Late);
		}
		private void AssetInvalidated(object _, AssetsInvalidatedEventArgs ev)
		{
			foreach (var name in ev.NamesWithoutLocale)
				if (name.IsEquivalentTo("Mods/BetterBeehouses/Bees"))
					beeTex = null;
		}
		private static void AddTags(IAssetData asset)
		{
			var data = asset.AsDictionary<string, ObjectData>().Data;
			data["18"].ContextTags.Add("honey_source");
			data["22"].ContextTags.Add("honey_source");
			data["418"].ContextTags.Add("honey_source");
			data["402"].ContextTags.Add("honey_source");
		}
	}
}
