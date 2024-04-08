using StardewModdingAPI;
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
			if (!data.TryGetValue("(BC)10", out var machine) || machine is null)
			{
				ModEntry.monitor.Log("Beehouse machine data is missing and could not be edited!", LogLevel.Warn);
				return;
			}
			try
			{
				EditData(machine);
			}
			catch (AssetEditException ex)
			{
				ModEntry.monitor.Log("Could not edit bee house: " + ex.Message, LogLevel.Warn);
			}
		}
		private static void EditData(MachineData data)
		{
			if (data.OutputRules.Count is 0)
				throw new AssetEditException("No output for beehouses detected!");

			var rule = FindByNameOrFirst(data.OutputRules, "Default") 
				?? throw new AssetEditException("No output for beehouses detected!");

			EditSeason(rule, data);
			EditSpeed(data);
		}
		private static void EditSeason(MachineOutputRule rule, MachineData data)
		{
			var produce = ModEntry.config.ProduceInWinter;
			var str = produce switch
			{
				Config.ProduceWhere.Always => "TRUE",
				Config.ProduceWhere.Indoors => "ANY \"!LOCATION_SEASON Target Winter\" \"LOCATION_IS_INDOORS Target\"",
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
				Config.ProduceWhere.Indoors => data.ClearContentsOvernightCondition.ListAppend("LOCATION_IS_INDOORS Target"),
				_ => data.ClearContentsOvernightCondition
			};
		}
		private static void EditSpeed(MachineData data)
		{
			foreach (var output in data.OutputRules)
				output.DaysUntilReady = Math.Min(output.DaysUntilReady, ModEntry.config.DaysToProduce);
		}
		private static MachineOutputRule FindByNameOrFirst(IList<MachineOutputRule> rules, string name)
		{
			MachineOutputRule first = null;
			foreach (var rule in rules)
				if (rule is null)
					continue;
				else if (rule.Id == name)
					return rule;
				else 
					first ??= rule;

			return first;
		}
	}
}
