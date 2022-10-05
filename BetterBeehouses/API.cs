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
            return !ObjectPatch.CantProduceToday(location.GetSeasonForLocation() == "winter", location);
        }
        public bool GetEnabledHere(GameLocation location, bool isWinter)
        {
            return !ObjectPatch.CantProduceToday(isWinter, location);
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
