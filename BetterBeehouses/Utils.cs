using BetterBeehouses.integration;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Reflection;
using StardewValley.GameData;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace BetterBeehouses
{
	static class Utils
	{
		private static readonly MethodInfo addItemMethod = typeof(Utils).MethodNamed("AddItem");
		public static MethodInfo MethodNamed(this Type type, string name)
			=> AccessTools.Method(type, name);
		public static MethodInfo MethodNamed(this Type type, string name, Type[] args)
			=> AccessTools.Method(type, name, args);
		public static bool GetProduceHere(GameLocation loc, Config.ProduceWhere where)
			=> where is not Config.ProduceWhere.Never && (!loc.IsOutdoors || where is Config.ProduceWhere.Always);
		public static void AddDictionaryEntry(IAssetData asset, object key, string path)
		{
			Type T = asset.DataType;
			if (!T.IsGenericType || T.GetGenericTypeDefinition() != typeof(Dictionary<,>))
				return;

			Type[] types = T.GetGenericArguments();
			addItemMethod.MakeGenericMethod(types).Invoke(null, new object[] {asset, key, path});
		}
		public static void AddItem<k, v>(IAssetData asset, k key, string path)
		{
			var model = asset.AsDictionary<k, v>().Data;
			var entry = ModEntry.helper.ModContent.Load<v>($"assets/{path}");
			model.Add(key, entry);
		}
		internal static void AddQuickBool(this IGMCMAPI api, object inst, IManifest manifest, string prop)
		{
			var p = inst.GetType().GetProperty(prop);
			var cfname = prop.Decap();
			api.AddBoolOption(manifest,
				p.GetGetMethod().CreateDelegate<Func<bool>>(inst),
				p.GetSetMethod().CreateDelegate<Action<bool>>(inst),
				() => ModEntry.i18n.Get($"config.{cfname}.name"),
				() => ModEntry.i18n.Get($"config.{cfname}.desc")
			);
		}
		internal static void AddQuickFloat(this IGMCMAPI api, object inst, IManifest manifest, string prop, float? min = null, float? max = null, float? inc = null)
		{
			var p = inst.GetType().GetProperty(prop);
			var cfname = prop.Decap();
			api.AddNumberOption(manifest,
				p.GetGetMethod().CreateDelegate<Func<float>>(inst),
				p.GetSetMethod().CreateDelegate<Action<float>>(inst),
				() => ModEntry.i18n.Get($"config.{cfname}.name"),
				() => ModEntry.i18n.Get($"config.{cfname}.desc"),
				min, max, inc
			);
		}
		internal static void AddQuickInt(this IGMCMAPI api, object inst, IManifest manifest, string prop, int? min = null, int? max = null, int? inc = null)
		{
			var p = inst.GetType().GetProperty(prop);
			var cfname = prop.Decap();
			api.AddNumberOption(manifest,
				p.GetGetMethod().CreateDelegate<Func<int>>(inst),
				p.GetSetMethod().CreateDelegate<Action<int>>(inst),
				() => ModEntry.i18n.Get($"config.{cfname}.name"),
				() => ModEntry.i18n.Get($"config.{cfname}.desc"),
				min, max, inc
			);
		}
		internal static void AddQuickEnum<TE>(this IGMCMAPI api, object inst, IManifest manifest, string prop) where TE : Enum
		{
			var p = inst.GetType().GetProperty(prop);
			var cfname = prop.Decap();
			var tenum = typeof(TE);
			var tname = tenum.Name.Decap();
			api.AddTextOption(manifest,
				() => p.GetValue(inst).ToString(),
				(s) => p.SetValue(inst, (TE)Enum.Parse(tenum, s)),
				() => ModEntry.i18n.Get($"config.{cfname}.name"),
				() => ModEntry.i18n.Get($"config.{cfname}.desc"),
				Enum.GetNames(tenum),
				(s) => ModEntry.i18n.Get($"config.{tname}.{s}")
			);
		}
		internal static void AddQuickLink(this IGMCMAPI api, string id, IManifest manifest)
			=> api.AddPageLink(manifest, id, () => ModEntry.i18n.Get($"config.{id}.name"), () => ModEntry.i18n.Get($"config.{id}.desc"));
		internal static string Decap(this string src)
			=> src.Length > 0 ? char.ToLower(src[0]) + src[1..] : string.Empty;
		internal static float Next(this Random rand, float max)
			=> (float)rand.NextDouble() * max;

		internal static bool CheckItemDrop(this GenericSpawnItemDataWithCondition spawn, GameLocation location = null, Farmer who = null)
			=> ItemRegistry.Exists(spawn.ItemId) && GameStateQuery.CheckConditions(spawn.Condition, location, who);

		internal static string ListAppend(this string list, string item) 
		{
			var s = list.Trim();
			var i = item.Trim();
			if (i[^1] is ',')
				i = i[..^1];
			return s.Length is 0 ? item : s[^1] is ',' ? s + i : s + ',' + i;
		}

		internal static T SelectFrom<T>(this IList<T> items, Vector2 tile)
		{
			if (items.Count is 0)
				throw new ArgumentException("List is empty! Cannot select");

			try
			{
				var bytes = new uint[3];
				var posData = MemoryMarshal.Cast<Vector2, uint>(MemoryMarshal.CreateReadOnlySpan(ref tile, 1));
				posData.CopyTo(bytes);
				bytes[2] = (uint)Game1.Date.TotalDays;

				int index = (int)(RandomFrom(bytes) % (uint)items.Count);
				return items[index];
			}
			catch
			{
				return items[0];
			}
		}

		internal static uint RandomFrom(params uint[] data)
		{
			if (data.Length == 0)
				return 0;

			uint r = data[0];
			for (int i = 1; i < data.Length; i++)
			{
				uint x = data[i];
				uint t = x ^ (x << 11);

				r = r^(r >> 19) ^ t^(t >> 8);
			}

			return r;
		}
	}
}
