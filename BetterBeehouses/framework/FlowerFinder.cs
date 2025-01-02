using BetterBeehouses.integration;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;

namespace BetterBeehouses.framework
{
	public static class FlowerFinder
	{
		public static IEnumerable<FlowerData> GetAllNearFlowers(GameLocation loc, IEnumerable<Vector2> tiles, Func<Crop, bool> extraCheck = null)
		{
			var GiantCrops = new Dictionary<Vector2, (string[] harvest, GiantCrop source)>();
			if (Config.config.UseGiantCrops)
				foreach (var clump in loc.resourceClumps)
					if (clump is GiantCrop giant && GiantFlower(giant, out var harvest, loc))
						for (int x = 0; x < giant.width.Value; x++)
							for (int y = 0; y < giant.height.Value; y++)
									GiantCrops.TryAdd(new(giant.Tile.X + x, giant.Tile.Y + y), (harvest, giant));

			foreach (var currentTile in tiles)
			{
				// giant crops
				if (GiantCrops.TryGetValue(currentTile, out var gc))
				{
					for (int i = 0; i < gc.harvest.Length; i++)
						yield return new(gc.source, currentTile, gc.harvest[i]);
				}

				// objects on floor
				else if (loc.objects.TryGetValue(currentTile, out StardewValley.Object obj))
				{
					// garden pot
					if (obj is IndoorPot pot)
					{
						if (Utils.GetProduceHere(loc, Config.config.UsePottedFlowers))
						{
							// forage in pot
							if (Config.config.UseForageFlowers && pot.heldObject.Value is not null) //forage in pot
							{
								var ho = pot.heldObject.Value;
								if (ho.CanBeGrabbed && IsFlower(ho))
									yield return new(currentTile, ho.QualifiedItemId, "Forage", true);
							}

							// crop in pot
							Crop crop = pot.hoeDirt.Value?.crop;
							if (IsGrown(crop, extraCheck) && IndexIsFlower(crop.indexOfHarvest.Value) && (extraCheck is null || extraCheck(crop)))
								yield return new(crop, currentTile); //flower in pot

							// bush in pot
							if (Config.config.UseBushes && pot.bush.Value is Bush bush && bush.readyForHarvest())
							{
								var shake = bush.GetShakeOffItem();
								if (IndexIsFlower(shake))
									yield return new(currentTile, shake, "Bush", true);
							}
						}
					}
					// forage on ground
					else
					{
						if (Config.config.UseForageFlowers && obj.CanBeGrabbed && IsFlower(obj))
							yield return new(currentTile, obj.QualifiedItemId, "Forage", false);
					}
				}

				// trees & crops
				else if (loc.terrainFeatures.TryGetValue(currentTile, out var tf))
				{
					// crop
					if (tf is HoeDirt dirt && IsGrown(dirt.crop, extraCheck) && IndexIsFlower(dirt.crop.indexOfHarvest.Value))
						yield return new(dirt.crop);

					// wildlfower grass
					else if (tf is Grass grass && WildFlowers.loaded && WildFlowers.GetWildFlower(grass) is Crop crop && IsGrown(crop, extraCheck))
						yield return new(crop);

					// tree
					else if (tf is FruitTree tree && Config.config.UseFruitTrees && tree.fruit.Count is > 0)
						foreach (var fruit in tree.fruit)
							if (Config.config.UseAnyFruitTrees || IsFlower(fruit))
								yield return new(currentTile, fruit.QualifiedItemId, "FruitTree", false);
				}

				// bushes
				if (Config.config.UseBushes)
				{
					for (int i = 0; i < 3; i++)
					{
						Vector2 targ = new(currentTile.X - i, currentTile.Y);
						if (loc.terrainFeatures.TryGetValue(targ, out var tf) && tf is Bush bush &&
							bush.getBoundingBox().Width > i && bush.readyForHarvest())
						{
							var item = bush.GetShakeOffItem();
							if (IndexIsFlower(item))
								yield return new(item, currentTile, targ, "Bush", false);
						}
					}
				}
			}
		}

		public static IEnumerable<Vector2> DefaultSearch(Vector2 source, int range = -1)
		{
			if (range < 0)
				range = Config.config.FlowerRange;

			Queue<Vector2> openList = new();
			HashSet<Vector2> closedList = new();
			openList.Enqueue(source);

			while(openList.TryDequeue(out var tile))
			{
				yield return tile;

				foreach (Vector2 v in Utility.getAdjacentTileLocations(tile))
					if (!closedList.Contains(v) && !openList.Contains(v) && (range < 0 || Math.Abs(v.X - source.X) + Math.Abs(v.Y - source.Y) <= range))
						openList.Enqueue(v);
				closedList.Add(tile);
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
			=> 
				!(item.HasTypeBigCraftable() || item.Category == -999) && 
				(Config.config.AnythingHoney || item.Category is -80 || item.HasContextTag("honey_source"));

		private static bool GiantFlower(GiantCrop giant, out string[] harvest, GameLocation location)
		{
			harvest = [];

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
			if (Config.config.AnythingHoney)
				return true;

			var data = ItemRegistry.GetData(index);

			return 
				!(data.HasTypeBigCraftable() || data.Category == -999) && 
				(data.Category == -80 || ItemContextTagManager.HasBaseTag(index, "honey_source"));
		}
	}
}
