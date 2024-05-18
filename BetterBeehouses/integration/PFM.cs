using BetterBeehouses.framework;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BetterBeehouses.integration
{
	internal class PFM
	{
		public static void Patch(Harmony harmony)
		{
			if (!ModEntry.helper.ModRegistry.IsLoaded("Digus.ProducerFrameworkMod"))
				return;

			var controller = AccessTools.TypeByName("ProducerFrameworkMod.Controllers.ProducerController");

			var blacklist = controller?
				.GetField("UnsupportedMachines", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?
				.GetValue(null) as ICollection<string>;

			if (blacklist is null || controller is null)
			{
				ModEntry.monitor.Log("Could not alter PFM! Beehouses will be missing many or all features!", LogLevel.Error);
				return;
			}

			blacklist.Add("(BC)10");
			ModEntry.monitor.Log("Successfully removed beehouse control from PFM.", LogLevel.Info);

			try
			{
				DoPatches(harmony);
			}
			catch (Exception ex)
			{
				ModEntry.monitor.Log(ex.ToString(), LogLevel.Warn);
				ModEntry.monitor.Log("Advanced PFM integration failed. Things will still work, but custom beehouses won't be affected by Better Beehouses honey changes.", LogLevel.Warn);
			}

			ModEntry.monitor.Log("Sucessfully applied all PFM patches.", LogLevel.Info);
		}

		private static void DoPatches(Harmony harmony)
		{
			var controller = AccessTools.TypeByName("ProducerFrameworkMod.Controllers.ProducerRuleController");
			harmony.Patch(
				controller.GetMethod("SearchInput", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic),
				prefix: new(typeof(PFM), nameof(InputSearch))
			);
		}

		private static bool InputSearch(GameLocation location, Vector2 startTileLocation, dynamic inputSearchConfig, ref StardewValley.Object __result)
		{
			object input = inputSearchConfig.InputIdentifier;
			if (!(input is ICollection<string> inputList && inputList.Contains("-80")) && input is not "-80")
				return true;

			int range = inputSearchConfig.Range;

			if (ModEntry.config.UseRandomFlower)
			{
				var items = FlowerFinder.GetAllNearFlowers(location, startTileLocation, range)
					.Where(static f => !f.ID.StartsWith('(') || f.ID.StartsWith("(O)") || f.ID.StartsWith("(BC)"))
					.ToArray();

				if (items.Length > 0)
					__result = ItemRegistry.Create<StardewValley.Object>(items.SelectFrom(startTileLocation).ID);
				else
					__result = null;
				return false;
			}
			else if (ModEntry.config.UseFruitTrees || ModEntry.config.UseGiantCrops ||
				ModEntry.config.UseForageFlowers || Utils.GetProduceHere(location, ModEntry.config.UsePottedFlowers))
			{
				__result = ItemRegistry.Create<StardewValley.Object>(
					FlowerFinder.GetAllNearFlowers(location, startTileLocation, range)
					.Where(static f => !f.ID.StartsWith('(') || f.ID.StartsWith("(O)") || f.ID.StartsWith("(BC)"))
					.FirstOrDefault()
					.ID);

				return false;
			}
			return true;
		}
	}
}
