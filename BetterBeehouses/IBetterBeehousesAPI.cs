using StardewValley;

namespace BetterBeehouses
{
    public interface IBetterBeehousesAPI
    {
        /// <summary>
        /// Returns true if bee houses can work here, otherwise, false.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool GetEnabledHere(GameLocation location);
        /// <summary>
        /// Returns the number of days to produce honey.
        /// </summary>
        /// <returns></returns>
        public int GetDaysToProduce();
        /// <summary>
        /// Return the distance bees will search for flowers.
        /// </summary>
        /// <returns></returns>
        public int GetSearchRadius();
        /// <summary>
        /// Returns the value multiplier for honey from bee houses.
        /// </summary>
        /// <returns></returns>
        public float GetValueMultiplier();
    }
}
