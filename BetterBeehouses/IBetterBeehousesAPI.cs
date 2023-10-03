using StardewValley;

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
	}
}
