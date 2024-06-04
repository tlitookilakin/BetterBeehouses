using BetterBeehouses.framework;
using HarmonyLib;
using StardewValley;
using System;
using System.Linq;
using SObject = StardewValley.Object;

namespace BetterBeehouses.patches
{
	internal class Machines
	{
		public static void Patch(Harmony harmony)
		{
			harmony.Patch(
				typeof(MachineDataUtility).GetMethod(nameof(MachineDataUtility.GetOutputItem)),
				postfix: new(typeof(Machines), nameof(GetOutputItem))
			);

			harmony.Patch(
				typeof(SObject).GetMethod(nameof(SObject.ShouldTimePassForMachine)),
				postfix: new(typeof(Machines), nameof(ShouldBeehouseRun))
			);
		}

		private static bool ShouldBeehouseRun(bool runByDefault, SObject __instance)
		{
			// already running; let it fly!
			if (runByDefault)
				return true;

			// not running, and not a beehouse
			if (!__instance.IsBeeHouse())
				return false;

			var where = __instance.Location;
			if (where is null)
				return false;

			return
				(where.GetSeason() is not Season.Winter || Config.config.ProduceInWinter switch
				{
					Config.ProduceWhere.Never => false,
					Config.ProduceWhere.Always => true,
					Config.ProduceWhere.Indoors => !where.IsOutdoors,
					_ => false
				})
				&& Config.config.UsableIn switch
				{
					Config.UsableOptions.Anywhere => true,
					Config.UsableOptions.Greenhouse => where.IsGreenhouse,
					Config.UsableOptions.Outdoors => where.IsOutdoors,
					_ => false
				};
		}

		private static Item GetOutputItem(Item result, SObject machine, Farmer who)
		{
			// only modify if the machine is a bee house and the output is honey
			if (!machine.IsBeeHouse() || result.QualifiedItemId is not "(O)340")
				return result;

			result.Quality = GetQuality(who, result.Quality);

			if (result is SObject obj)
				obj.Price = (int)(obj.Price * Config.config.ValueMultiplier + .5f);

			var where = machine.Location;
			if (where is null)
				return result;

			if (Config.config.UseFlowerBoost)
				result.Stack = 
					result.Stack * 
					Math.Max(FlowerFinder.GetAllNearFlowers(where, FlowerFinder.DefaultSearch(machine.TileLocation)).Count(), 1) 
					/ Config.config.FlowersPerBoost;

			return result;
		}

		public static int GetQuality(Farmer who, int original)
		{
			//based on Crop.harvest()
			if (!Config.config.UseQuality)
				return original;

			float boost = who is not null && who.eventsSeen.Contains("2120303") ? Config.config.BearBoost : 1f;

			double chanceForGoldQuality = 0.2 * (who?.FarmingLevel ?? 0.0 / 10.0) + 0.2 * boost * ((who?.FarmingLevel ?? 0.0 + 2.0) / 12.0) + 0.01;
			double chanceForSilverQuality = Math.Min(0.75, chanceForGoldQuality * 2.0);
			return Math.Max(original,
				Game1.random.NextDouble() < chanceForGoldQuality / 2.0 ? 4 :
				Game1.random.NextDouble() < chanceForGoldQuality ? 2 :
				Game1.random.NextDouble() < chanceForSilverQuality ? 1 : 0);
		}
	}
}
