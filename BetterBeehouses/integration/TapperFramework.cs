using BetterBeehouses.framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using StardewValley.GameData.WildTrees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBeehouses.integration
{
	internal class TapperFramework
	{
		public static void Init()
		{
			if (!ModEntry.helper.ModRegistry.IsLoaded("selph.CustomTapperFramework"))
				return;

			ModEntry.monitor.Log("Tapper framework detected, integrating...", LogLevel.Info);

			ModEntry.helper.Events.Content.AssetRequested += ModifyTapperData;
		}

		private static void ModifyTapperData(object sender, AssetRequestedEventArgs e)
		{
			if (!e.NameWithoutLocale.IsEquivalentTo("selph.CustomTapperFramework/Data"))
				return;

			e.Edit(static (asset) =>
			{
				var produce = ModEntry.config.ProduceInWinter;
				var str = produce switch
				{
					Config.ProduceWhere.Always => "TRUE",
					Config.ProduceWhere.Indoors => $"ANY \"!LOCATION_SEASON Target Winter\" \"{MachineEditor.GetIndoorsQuery()}\"",
					_ => null
				};

				if (str is null)
					return;

				var data = asset.Data as dynamic;

				foreach (var pair in data)
				{
					string key = pair.Key;
					var entry = pair.Value;

					if (key is not "(BC)10" && !(ModEntry.config.ModifyCustomBeehouses &&
					ItemContextTagManager.HasBaseTag(key, "bee_house")))
						continue;

					var rules = 
					(entry.FruitTreeOutputRules as IEnumerable<GenericSpawnItemDataWithCondition>)
					.Concat(entry.TreeOutputRules as IEnumerable<GenericSpawnItemDataWithCondition>);

					foreach (var rule in rules)
						rule.Condition = rule.Condition.Replace("!LOCATION_SEASON Target Winter", str, StringComparison.OrdinalIgnoreCase);
				}
			}, 
			AssetEditPriority.Late);
		}
	}
}
