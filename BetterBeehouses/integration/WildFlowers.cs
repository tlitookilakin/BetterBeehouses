using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace BetterBeehouses.integration
{
	internal class WildFlowers
	{
		internal static bool loaded = false;
		internal static Func<Grass, Crop> GetWildFlower;

		internal static bool Setup()
		{
			if (!ModEntry.helper.ModRegistry.IsLoaded("jpp.WildFlowersReimagined"))
				return false;

			var flowerData = AccessTools.TypeByName("WildFlowersReimagined.FlowerGrass");
			if (flowerData is null)
				return false;

			try
			{
				GetWildFlower ??= BuildFlowerGetter(flowerData);
				loaded = true;
			} 
			catch (Exception ex)
			{
				ModEntry.monitor.Log($"Failed to generate wildflowers getter: {ex}", LogLevel.Warn);
			}

			return true;
		}

		private static Func<Grass, Crop> BuildFlowerGetter(Type dataType)
		{
			// (Grass grass) => (grass as FlowerGrass)?.Crop;

			const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
			var method = new DynamicMethod("GetWildFlower", typeof(Crop), [typeof(Grass)], true);
			var il = method.GetILGenerator();
			var skip = il.DefineLabel();

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Isinst, dataType);
			il.Emit(OpCodes.Dup);
			il.Emit(OpCodes.Brfalse, skip);
			il.Emit(OpCodes.Callvirt, dataType.GetProperty("Crop", flags).GetMethod);
			il.MarkLabel(skip);
			il.Emit(OpCodes.Ret);

			return method.CreateDelegate<Func<Grass, Crop>>();
		}
	}
}
