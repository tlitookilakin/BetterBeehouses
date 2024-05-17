using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Machines;
using System;
using System.Collections.Generic;

namespace BetterBeehouses.framework
{
	internal class MachineEditor
	{
		internal static void Edit(IAssetData asset)
		{
			if (asset.Data is not Dictionary<string, MachineData> data)
			{
				ModEntry.monitor.Log("Machine data was not in expected format and could not be edited!", LogLevel.Error);
				return;
			}

			int edited = 0;

			foreach((var key, var machine) in data)
			{
				if (
					ItemContextTagManager.HasBaseTag(key, "bee_house") &&
					(key is "(BC)10" || ModEntry.config.ModifyCustomBeehouses)
				)
				{
					var t = ItemContextTagManager.HasBaseTag(key, "bee_house");
					edited++;
					try
					{
						EditData(machine);
					}
					catch (AssetEditException ex)
					{
						ModEntry.monitor.Log($"Could not edit beehouse machine '{key}': {ex.Message}", LogLevel.Warn);
					}
				}
			}

			if (edited is 0)
				ModEntry.monitor.Log("No valid machine data found for any bee house!", LogLevel.Warn);
		}
		private static void EditData(MachineData data)
		{
			if (data.OutputRules.Count is 0)
				throw new AssetEditException("No output detected!");

			foreach (var rule in data.OutputRules)
				EditSeason(rule, data);

			EditSpeed(data);
		}
		private static void EditSeason(MachineOutputRule rule, MachineData data)
		{
			var produce = ModEntry.config.ProduceInWinter;
			var str = produce switch
			{
				Config.ProduceWhere.Always => "TRUE",
				Config.ProduceWhere.Indoors => $"ANY \"!LOCATION_SEASON Target Winter\" \"{GetIndoorsQuery()}\"",
				_ => null
			};
			if (str is null)
				return;

			if (rule.Triggers is null)
			{
				(rule.Triggers = new()).Add(
					new()
					{
						Id = "Default",
						Condition = str,
					}
				);
				ModEntry.monitor.Log("All triggers missing, adding default! Another mod is tampering with beehouse data.", LogLevel.Warn);
			}
			else
			{
				foreach (var trigger in rule.Triggers)
					trigger.Condition = trigger.Condition.Replace("!LOCATION_SEASON Target Winter", str, StringComparison.OrdinalIgnoreCase);
			}

			data.ClearContentsOvernightCondition = produce switch
			{
				Config.ProduceWhere.Always => "FALSE",
				Config.ProduceWhere.Indoors => data.ClearContentsOvernightCondition.ListAppend($" !{GetIndoorsQuery}"),
				_ => data.ClearContentsOvernightCondition
			};
		}
		private static string GetIndoorsQuery()
		{
			return ModEntry.config.UsableIn switch
			{
				Config.UsableOptions.Anywhere => "LOCATION_IS_INDOORS Target",
				Config.UsableOptions.Greenhouse => "LOCATION_IS_GREENHOUSE Target",
				_ => "TRUE"
			};
		}
		private static void EditSpeed(MachineData data)
		{
			foreach (var output in data.OutputRules)
				output.DaysUntilReady = Math.Min(output.DaysUntilReady, ModEntry.config.DaysToProduce);
		}
	}
}
