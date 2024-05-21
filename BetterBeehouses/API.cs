using BetterBeehouses.framework;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterBeehouses
{
	public class API : IBetterBeehousesAPI
	{
		public IEnumerable<object> GetAllHoneySources(GameLocation where, IEnumerable<Vector2> tiles)
		{
			return FlowerFinder.GetAllNearFlowers(where, tiles) as IEnumerable<object>;
		}

		public IEnumerable<KeyValuePair<Vector2, string>> GetAllHoneySourcesInRange(GameLocation where, Vector2 tile, int range = -1, Func<Crop, bool> predicate = null)
		{
			if (range < 0)
				range = Config.config.FlowerRange;

			return FlowerFinder.GetAllNearFlowers(where, FlowerFinder.DefaultSearch(tile, range), predicate).Select(
				(f, i) => new KeyValuePair<Vector2, string>(f.Tile, f.ID)
			);
		}

		public int GetSearchRadius()
			=> Config.config.FlowerRange;

		public float GetValueMultiplier()
			=> Config.config.ValueMultiplier;
	}
}
