using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;

namespace BetterBeehouses.integration
{
	internal class WildFlowers
	{
		internal const string FLAG = "aedenthorn.Wildflowers/wild";
		private static bool loaded = false;
		private static Dictionary<string, Dictionary<Vector2, Crop>> wild_data;
		internal static bool Setup()
		{
			if (!ModEntry.helper.ModRegistry.IsLoaded("aedenthorn.Wildflowers"))
				return false;

			loaded = true;
			wild_data = ModEntry.helper.Reflection.GetField<Dictionary<string, Dictionary<Vector2, Crop>>>(
				AccessTools.TypeByName("Wildflowers.ModEntry"), "cropDict").GetValue();

			ModEntry.monitor.Log("Wildflowers detected. Stripping flower patch.");
			var patch = AccessTools.TypeByName("Wildflowers.ModEntry+Utility_findCloseFlower_Patch")?.MethodNamed("Postfix");
			if (patch is not null)
			{
				ModEntry.harmony.Unpatch(typeof(Utility).MethodNamed(nameof(Utility.findCloseFlower),
					new[] { typeof(GameLocation), typeof(Vector2), typeof(int), typeof(Func<Crop, bool>) }),
					patch);
				return true;
			}
			ModEntry.monitor.Log("Could not find patch method; attempting broad strip.");
			ModEntry.harmony.Unpatch(typeof(Utility).MethodNamed(nameof(Utility.findCloseFlower),
					new[] { typeof(GameLocation), typeof(Vector2), typeof(int), typeof(Func<Crop, bool>) }),
					HarmonyPatchType.Postfix, "aedenthorn.Wildflowers");
			return true;
		}
		internal static Dictionary<Vector2, Crop> GetData(GameLocation where)
			=> (!loaded || wild_data is null || !wild_data.TryGetValue(where.Name, out var ret)) ? null : ret;
	}
}
