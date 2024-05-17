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
		public IEnumerable<KeyValuePair<Vector2, string>> GetAllHoneySourcesInRange(GameLocation where, Vector2 tile, int range = -1, Func<Crop, bool> predicate = null)
		{
			if (range < 0)
				range = ModEntry.config.FlowerRange;

			return FlowerFinder.GetAllNearFlowers(where, tile, range, predicate).Select(
				(f, i) => new KeyValuePair<Vector2, string>(f.Tile, f.ID)
			);
		}

		// TODO: add get all rect
		public int GetSearchRadius()
			=> ModEntry.config.FlowerRange;
		public float GetValueMultiplier()
			=> ModEntry.config.ValueMultiplier;
	}
}
