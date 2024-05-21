using StardewValley;
using StardewValley.Delegates;
using System.Reflection;
using static StardewValley.GameStateQuery;

namespace BetterBeehouses.framework
{
	public class GSQ
	{
		internal static void Register()
		{
			var methods = typeof(GSQ).GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);

			foreach (var method in methods)
				if (method.CreateDelegate<GameStateQueryDelegate>() is GameStateQueryDelegate query)
					GameStateQuery.Register(method.Name, query);
		}

		public static bool LOCATION_IS_GREENHOUSE(string[] query, GameStateQueryContext context)
		{
			var location = context.Location;
			if (!Helpers.TryGetLocationArg(query, 1, ref location, out var err))
				Helpers.ErrorResult(query, err);

			return location.IsGreenhouse;
		}
	}
}
