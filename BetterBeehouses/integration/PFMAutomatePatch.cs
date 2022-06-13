using HarmonyLib;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            vanillaMachines ??= ModEntry.helper.Reflection.GetField<HashSet<string>>(
                AccessTools.TypeByName("Digus.PFMAutomate.AutomateOverrides"), "SupportedVanillaMachines");

            if (!isPatched && ModEntry.config.PatchPFM && ModEntry.config.PatchAutomate)
                vanillaMachines.GetValue().Remove(beehouseMachineName);
            else
                vanillaMachines.GetValue().Add(beehouseMachineName);

            return true;
        }
    }
}
