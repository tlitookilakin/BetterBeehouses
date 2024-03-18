using BetterBeehouses.patches;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;

namespace BetterBeehouses
{
	public class API : IBetterBeehousesAPI
	{
		public IEnumerable<KeyValuePair<Vector2, string>> GetAllHoneySourcesInRange(GameLocation where, Vector2 tile, int range = -1, Func<Crop, bool> predicate = null)
		{
			if (range < 0)
				range = ModEntry.config.FlowerRange;
			return Utilities.GetAllNearFlowers(where, tile, range, predicate);
		}

		// TODO: add get all rect
		public int GetSearchRadius()
			=> ModEntry.config.FlowerRange;
		public float GetValueMultiplier()
			=> ModEntry.config.ValueMultiplier;
	}
}
