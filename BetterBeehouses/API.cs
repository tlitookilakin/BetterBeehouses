using StardewValley;

namespace BetterBeehouses
{
    public class API : IBetterBeehousesAPI
    {
        public int GetDaysToProduce()
        {
            return ModEntry.config.DaysToProduce;
        }
        public bool GetEnabledHere(GameLocation location)
        {
            return !ObjectPatch.CantProduceToday(location.GetSeasonForLocation() == "Winter", location);
        }
        public int GetSearchRadius()
        {
            return ModEntry.config.FlowerRange;
        }
        public float GetValueMultiplier()
        {
            return ModEntry.config.ValueMultiplier;
        }
    }
}
