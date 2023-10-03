using BetterBeehouses.integration;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterBeehouses.patches
{
	[HarmonyPatch(typeof(Utility))]
	class Utilities
	{
		private static Action<Crop, Vector2> tposf;

		internal static void Init()
		{
			tposf = typeof(Crop).FieldNamed("tilePosition").GetInstanceFieldSetter<Crop, Vector2>();
		}

		[HarmonyPatch("findCloseFlower", new Type[] { typeof(GameLocation), typeof(Vector2), typeof(int), typeof(Func<Crop, bool>) })]
		[HarmonyPrefix]
		[HarmonyPriority(Priority.High)]
		internal static bool preCheck(GameLocation location, Vector2 startTileLocation, int range, Func<Crop, bool> additional_check, ref Crop __result)
		{
			if (ModEntry.config.UseRandomFlower)
			{
				var items = GetAllNearFlowers(location, startTileLocation, range, additional_check).ToArray();
				if (items.Length > 0)
					__result = CropFromIndex(items[Game1.random.Next(items.Length)]);
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
		public static IEnumerable<KeyValuePair<Vector2, string>> GetAllNearFlowers(GameLocation loc, Vector2 tile, int range = -1, Func<Crop, bool> extraCheck = null)
		{
			var GiantCrops = new Dictionary<Vector2, string[]>();
			if (ModEntry.config.UseGiantCrops)
				foreach (var clump in loc.resourceClumps)
					if (clump is GiantCrop giant && GiantFlower(giant, out var harvest, loc))
						for (int x = 0; x < giant.width.Value; x++)
							for (int y = 0; y < giant.height.Value; y++)
								if (Math.Abs(giant.Tile.X + x - tile.X) + Math.Abs(giant.Tile.Y + y - tile.Y) <= range)
									GiantCrops.Add(new(giant.Tile.X + x, giant.Tile.Y + y), harvest);

			var wildflowers = WildFlowers.GetData(loc);
			Queue<Vector2> openList = new();
			HashSet<Vector2> closedList = new();
			openList.Enqueue(tile);
			for (int attempts = 0; range >= 0 || range < 0 && attempts <= 150; attempts++)
			{
				if (openList.Count <= 0)
					yield break;
				Vector2 currentTile = openList.Dequeue();
				if (GiantCrops.TryGetValue(currentTile, out var gc))
				{
					for (int i = 0; i < gc.Length; i++)
						yield return new(currentTile, gc[i]);
				}
				else if (wildflowers is not null && wildflowers.TryGetValue(currentTile, out var wilf))
				{
					yield return new(currentTile, wilf.indexOfHarvest.Value);
				}
				else if (loc.terrainFeatures.TryGetValue(currentTile, out var tf))
				{
					if (tf is HoeDirt dirt && IsGrown(dirt.crop, extraCheck) && IndexIsFlower(dirt.crop.indexOfHarvest.Value))
						yield return new(currentTile, dirt.crop.indexOfHarvest.Value);
					else if (tf is FruitTree tree && ModEntry.config.UseFruitTrees && tree.fruit.Count is > 0)
						foreach (var fruit in tree.fruit)
							if (ModEntry.config.UseAnyFruitTrees || IsFlower(fruit))
								yield return new(currentTile, fruit.QualifiedItemId);
				}
				else if (loc.objects.TryGetValue(currentTile, out StardewValley.Object obj))
				{
					if (obj is IndoorPot pot) //pot crop
					{
						if (Utils.GetProduceHere(loc, ModEntry.config.UsePottedFlowers))
						{
							if (ModEntry.config.UseForageFlowers && pot.heldObject.Value is not null) //forage in pot
							{
								var ho = pot.heldObject.Value;
								if (ho.CanBeGrabbed && IsFlower(ho))
									yield return new(currentTile, ho.QualifiedItemId);
							}
							Crop crop = pot.hoeDirt.Value?.crop;
							if (IsGrown(crop, extraCheck) && IndexIsFlower(crop.indexOfHarvest.Value) && (extraCheck is null || extraCheck(crop)))
								yield return new(currentTile, crop.indexOfHarvest.Value); //flower in pot
						}
					}
					else
					{
						if (ModEntry.config.UseForageFlowers && obj.CanBeGrabbed && IsFlower(obj))
							yield return new(currentTile, obj.QualifiedItemId);
						//non-pot forage
					}
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

		private static Crop CropFromIndex(KeyValuePair<Vector2, string> what)
		{
			if (what.Value == "")
				return null;
			Crop ret = new();
			ret.indexOfHarvest.Value = what.Value;
			tposf(ret, what.Key);
			return ret;
		}
	}
}
