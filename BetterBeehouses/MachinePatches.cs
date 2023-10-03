using HarmonyLib;
using StardewValley;
using System;
using System.Linq;
using xTile.Tiles;
using SObject = StardewValley.Object;

namespace BetterBeehouses
{
	[HarmonyPatch]
	internal class MachinePatches
	{
		[HarmonyPatch(typeof(SObject), nameof(SObject.ShouldTimePassForMachine))]
		[HarmonyPostfix]
		internal static bool ShouldTimePass(bool result, SObject __instance)
			=> result || CanProduceHere(__instance.Location);

		[HarmonyPatch(typeof(MachineDataUtility), nameof(MachineDataUtility.GetOutputItem))]
		[HarmonyPostfix]
		internal static Item GetOutputItem(Item result, SObject machine, Farmer who)
		{
			// only modify if the machine is a bee house and the output is honey
			if (machine.QualifiedItemId is not "(BC)10" || result.QualifiedItemId is not "(O)340")
				return result;

			result.Quality = GetQuality(who, result.Quality);

			if (result is SObject obj)
				obj.Price = (int)(obj.Price * ModEntry.config.ValueMultiplier + .5f);

			var where = machine.Location;
			if (where is null)
				return result;

			if (ModEntry.config.UseFlowerBoost)
				result.Stack += Math.Max(UtilityPatch.GetAllNearFlowers(where, machine.TileLocation, ModEntry.config.FlowerRange).Count() - 1, 0)
					/ ModEntry.config.FlowersPerBoost;

			return result;
		}

		public static bool CanProduceHere(GameLocation loc)
			=> ModEntry.config.ProduceInWinter switch {
				Config.ProduceWhere.Always => CanProduceIn(loc),
				Config.ProduceWhere.Indoors => CanProduceIn(loc) && !loc.IsOutdoors,
				_ => CanProduceIn(loc) && (loc.GetLocationContext().SeasonOverride ?? Game1.season) is not Season.Winter
			};

		private static bool CanProduceIn(GameLocation loc)
			=> ModEntry.config.UsableIn switch { 
				Config.UsableOptions.Anywhere => true,
				Config.UsableOptions.Outdoors => loc.IsOutdoors,
				Config.UsableOptions.Greenhouse => loc.IsGreenhouse,
				_ => false
			};

		public static int GetQuality(Farmer who, int original)
		{
			//based on Crop.harvest()
			if (!ModEntry.config.UseQuality)
				return original;

			float boost = who is not null && who.eventsSeen.Contains("2120303") ? ModEntry.config.BearBoost : 1f;

			double chanceForGoldQuality = 0.2 * (who?.FarmingLevel ?? 0.0 / 10.0) + 0.2 * boost * ((who?.FarmingLevel ?? 0.0 + 2.0) / 12.0) + 0.01;
			double chanceForSilverQuality = Math.Min(0.75, chanceForGoldQuality * 2.0);
			return Math.Max(original,
				(Game1.random.NextDouble() < chanceForGoldQuality / 2.0) ? 4 :
				(Game1.random.NextDouble() < chanceForGoldQuality) ? 2 :
				(Game1.random.NextDouble() < chanceForSilverQuality) ? 1 : 0);
		}
	}
}
