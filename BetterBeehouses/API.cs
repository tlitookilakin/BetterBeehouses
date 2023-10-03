using StardewValley;

namespace BetterBeehouses
{
	public class API : IBetterBeehousesAPI
	{
		// TODO: add get all near and get all rect
		public int GetSearchRadius()
			=> ModEntry.config.FlowerRange;
		public float GetValueMultiplier()
			=> ModEntry.config.ValueMultiplier;
	}
}
