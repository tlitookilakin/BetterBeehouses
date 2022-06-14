using HarmonyLib;
using StardewModdingAPI;
using System;
using System.Collections.Generic;

namespace BetterBeehouses.integration
{
    internal class PFMAutomatePatch
    {
        private static bool isPatched = false;
        private const string beehouseMachineName = "Pathoschild.Stardew.Automate.Framework.Machines.Objects.BeeHouseMachine";
        private static IReflectedField<HashSet<string>> vanillaMachines = null;
        internal static bool Setup()
        {
            if (!ModEntry.helper.ModRegistry.IsLoaded("Digus.PFMAutomate"))
                return false;

            ModEntry.monitor.Log($"PFMAutomate integration {(isPatched ? "Disabling" : "Enabling")}.", LogLevel.Trace);

            vanillaMachines ??= ModEntry.helper.Reflection.GetField<HashSet<string>>(
                AccessTools.TypeByName("PFMAutomate.AutomateOverrides"), "SupportedVanillaMachines");

            if (!isPatched && ModEntry.config.PatchPFM && ModEntry.config.PatchAutomate)
            {
                vanillaMachines.GetValue().Remove(beehouseMachineName);
                isPatched = true;
            }
            else
            {
                vanillaMachines.GetValue().Add(beehouseMachineName);
                isPatched = false;
            }

            return true;
        }
    }
}
