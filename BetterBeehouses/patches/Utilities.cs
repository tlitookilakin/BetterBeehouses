using BetterBeehouses.framework;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Linq;

namespace BetterBeehouses.patches
{
	class Utilities
	{
		internal static void Init()
		{
			ModEntry.harmony.Patch(
				typeof(Utility).GetMethod(nameof(Utility.findCloseFlower),
				new[] { typeof(GameLocation), typeof(Vector2), typeof(int), typeof(Func<Crop, bool>) }),
				prefix: new(typeof(Utilities), nameof(preCheck))
			);
		}

		internal static bool preCheck(GameLocation location, Vector2 startTileLocation, int range, Func<Crop, bool> additional_check, ref Crop __result)
		{
			if (ModEntry.config.UseRandomFlower)
			{
				var items = FlowerFinder.GetAllNearFlowers(location, startTileLocation, range, additional_check).ToArray();
				if (items.Length > 0)
					__result = CropFromIndex(items.SelectFrom(startTileLocation));
				else
					__result = null;
				return false;
			}
			else if (ModEntry.config.UseFruitTrees || ModEntry.config.UseGiantCrops ||
				ModEntry.config.UseForageFlowers || Utils.GetProduceHere(location, ModEntry.config.UsePottedFlowers))
			{
				__result = CropFromIndex(FlowerFinder.GetAllNearFlowers(location, startTileLocation, range, additional_check).FirstOrDefault());
				return false;
			}
			return true;
		}

		private static Crop CropFromIndex(FlowerData what)
		{
			if (what.crop is not null)
			{
				if (what.InPot)
				{
					what.crop.tilePosition = what.SourceTile;
					what.crop.modData["tlitookilakin.BetterBeehouses.FromPot"] = "T";
				}

				return what.crop;
			}

			if (what.ID == "")
				return null;

			Crop ret = new();
			ret.indexOfHarvest.Value = what.ID;
			ret.tilePosition = what.SourceTile;
			ret.modData["tlitookilakin.BetterBeehouses.SourceType"] = what.type;
			if (what.InPot)
				ret.modData["tlitookilakin.BetterBeehouses.FromPot"] = "T";

			return ret;
		}
	}
}
