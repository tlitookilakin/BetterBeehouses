using StardewValley;

namespace BetterBeehouses
{
	public interface IBetterBeehousesAPI
	{
		/// <summary>
		/// Gets if bees can work here.
		/// </summary>
		/// <param name="location">The location</param>
		/// <returns>True if bees can work in this location</returns>
		public bool GetEnabledHere(GameLocation location);
		/// <summary>
		/// Gets if bees can work here.
		/// </summary>
		/// <param name="location">The location</param>
		/// <param name="isWinter">If it is winter in this location</param>
		/// <returns>True if bees can work in this location</returns>
		public bool GetEnabledHere(GameLocation location, bool isWinter);
		/// <summary>
		/// Returns the number of days to produce honey
		/// </summary>
		/// <returns>True if bees can work in this location</returns>
		public int GetDaysToProduce();
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
	}
}
