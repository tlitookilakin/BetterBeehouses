using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;

namespace BetterBeehouses
{
	public interface IBetterBeehousesAPI
	{
		/// <summary>
		/// Return the distance bees will search for flowers
		/// </summary>
		/// <returns>Tile Radius</returns>
		public int GetSearchRadius();

		/// <summary>
		/// Returns the value multiplier for honey from bee houses
		/// </summary>
		/// <returns>Multiplier</returns>
		public float GetValueMultiplier();

		/// <summary>
		/// Finds nearby honey sources recognized by this mod
		/// </summary>
		/// <param name="where">The location to searh in</param>
		/// <param name="tile">The tile at the center of the search area</param>
		/// <param name="range">The range, in tiles, to search. If omitted, will use the configured range</param>
		/// <param name="predicate">A filter</param>
		/// <returns>A sequence of pairs- the key is the position it was found at, and the value is the ID of the item</returns>
		public IEnumerable<KeyValuePair<Vector2, string>> GetAllHoneySourcesInRange(GameLocation where, Vector2 tile, int range = -1, Func<Crop, bool> predicate = null);
	}
}
