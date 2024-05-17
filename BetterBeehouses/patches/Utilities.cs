using BetterBeehouses.framework;
using BetterBeehouses.integration;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterBeehouses.patches
{
	class Utilities
	{
		// TODO: bush bloom support
		// TODO: custom bush support

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
				var items = GetAllNearFlowers(location, startTileLocation, range, additional_check).ToArray();
				if (items.Length > 0)
					__result = CropFromIndex(items.SelectFrom(startTileLocation));
				else
					__result = null;
				return false;
			}
			else if (ModEntry.config.UseFruitTrees || ModEntry.config.UseGiantCrops ||
				ModEntry.config.UseForageFlowers || Utils.GetProduceHere(location, ModEntry.config.UsePottedFlowers))
			{
				__result = CropFromIndex(GetAllNearFlowers(location, startTileLocation, range, additional_check).FirstOrDefault());
				return false;
			}
			return true;
		}
		public static IEnumerable<FlowerData> GetAllNearFlowers(GameLocation loc, Vector2 tile, int range, Func<Crop, bool> extraCheck = null)
		{
			var GiantCrops = new Dictionary<Vector2, (string[] harvest, GiantCrop source)>();
			if (ModEntry.config.UseGiantCrops)
				foreach (var clump in loc.resourceClumps)
					if (clump is GiantCrop giant && GiantFlower(giant, out var harvest, loc))
						for (int x = 0; x < giant.width.Value; x++)
							for (int y = 0; y < giant.height.Value; y++)
								if (Math.Abs(giant.Tile.X + x - tile.X) + Math.Abs(giant.Tile.Y + y - tile.Y) <= range)
									GiantCrops.Add(new(giant.Tile.X + x, giant.Tile.Y + y), (harvest, giant));

			var wildflowers = WildFlowers.GetData(loc);
			Queue<Vector2> openList = new();
			HashSet<Vector2> closedList = new();
			openList.Enqueue(tile);
			for (int attempts = 0; range >= 0 || range < 0 && attempts <= 150; attempts++)
			{
				if (openList.Count <= 0)
					yield break;
				Vector2 currentTile = openList.Dequeue();

				// giant crops
				if (GiantCrops.TryGetValue(currentTile, out var gc))
				{
					for (int i = 0; i < gc.harvest.Length; i++)
						yield return new(gc.source, currentTile, gc.harvest[i]);
				}

				// wildflowers
				else if (wildflowers is not null && wildflowers.TryGetValue(currentTile, out var wilf))
				{
					yield return new(wilf);
				}

				// objects on floor
				else if (loc.objects.TryGetValue(currentTile, out StardewValley.Object obj))
				{
					// garden pot
					if (obj is IndoorPot pot)
					{
						if (Utils.GetProduceHere(loc, ModEntry.config.UsePottedFlowers))
						{
							// forage in pot
							if (ModEntry.config.UseForageFlowers && pot.heldObject.Value is not null) //forage in pot
							{
								var ho = pot.heldObject.Value;
								if (ho.CanBeGrabbed && IsFlower(ho))
									yield return new(currentTile, ho.QualifiedItemId, "Forage") { InPot = true };
							}

							// crop in pot
							Crop crop = pot.hoeDirt.Value?.crop;
							if (IsGrown(crop, extraCheck) && IndexIsFlower(crop.indexOfHarvest.Value) && (extraCheck is null || extraCheck(crop)))
								yield return new(crop) { InPot = true, Tile = currentTile, SourceTile = currentTile }; //flower in pot
						}
					}
					// forage on ground
					else
					{
						if (ModEntry.config.UseForageFlowers && obj.CanBeGrabbed && IsFlower(obj))
							yield return new(currentTile, obj.QualifiedItemId, "Forage");
					}
				}

				// trees & crops
				else if (loc.terrainFeatures.TryGetValue(currentTile, out var tf))
				{
					// crop
					if (tf is HoeDirt dirt && IsGrown(dirt.crop, extraCheck) && IndexIsFlower(dirt.crop.indexOfHarvest.Value))
						yield return new(dirt.crop);

					// tree
					else if (tf is FruitTree tree && ModEntry.config.UseFruitTrees && tree.fruit.Count is > 0)
						foreach (var fruit in tree.fruit)
							if (ModEntry.config.UseAnyFruitTrees || IsFlower(fruit))
								yield return new(currentTile, fruit.QualifiedItemId, "FruitTree");
				}

				foreach (Vector2 v in Utility.getAdjacentTileLocations(currentTile))
					if (!closedList.Contains(v) && !openList.Contains(v) && (range < 0 || Math.Abs(v.X - tile.X) + Math.Abs(v.Y - tile.Y) <= range))
						openList.Enqueue(v);
				closedList.Add(currentTile);
			}
		}
		private static bool IsGrown(Crop crop, Func<Crop, bool> extraCheck = null)
		{
			if (crop is not null && !crop.dead.Value &&
			crop.currentPhase.Value >= crop.phaseDays.Count - 1)
				if (extraCheck is null)
					return true;
				else
					return extraCheck(crop);
			return false;
		}
		private static bool IsFlower(Item item)
			=> item.Category is -80 || item.HasContextTag("honey_source");

		private static bool GiantFlower(GiantCrop giant, out string[] harvest, GameLocation location)
		{
			harvest = Array.Empty<string>();

			var data = giant.GetData();
			if (data is null || data.HarvestItems.Count is 0)
				return false;

			harvest = new string[data.HarvestItems.Count];
			int i = 0;
			foreach (var item in data.HarvestItems)
				if (IndexIsFlower(item.ItemId) && item.CheckItemDrop(location))
					harvest[i++] = item.ItemId;
			harvest = harvest[..i];
			return harvest.Length is not 0;
		}

		private static bool IndexIsFlower(string index)
		{
			if (!ItemRegistry.Exists(index))
				return false;
			if (ModEntry.config.AnythingHoney)
				return true;

			return ItemRegistry.GetData(index).Category == -80 || ItemContextTagManager.HasBaseTag(index, "honey_source");
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
