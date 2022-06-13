using HarmonyLib;
using StardewModdingAPI;
using System.Collections.Generic;

namespace BetterBeehouses.integration
{
    class PFMPatch
    {
        private static bool isPatched = false;
        private static object BeehouseDef = null;
        private static IReflectedField<IDictionary<string, object>> configField = null;
        internal static bool Setup()
        {
            if (!ModEntry.helper.ModRegistry.IsLoaded("Digus.ProducerFrameworkMod"))
                return false;

            var target = AccessTools.TypeByName("ProducerFrameworkMod.Controllers.ProducerController").MethodNamed("AddConfigToRepository");

            if (configField is null)
                configField = ModEntry.helper.Reflection.GetField<IDictionary<string, object>>(
                    AccessTools.TypeByName("ProducerFrameworkMod.Controllers.ProducerController"),
                    "ConfigRepository");

            var configs = configField.GetValue();

            if (!isPatched && ModEntry.config.PatchPFM)
            {
                if (configs.TryGetValue("Bee House", out var c))
                    BeehouseDef = c;
                configs.Remove("Bee House");
                ModEntry.harmony.Patch(target, postfix: new(typeof(PFMPatch), "Postfix"));
                isPatched = true;
            } else if(isPatched && !ModEntry.config.PatchPFM){
                if (BeehouseDef is not null)
                    configs.Add("Bee House", BeehouseDef);
                ModEntry.harmony.Unpatch(target, HarmonyPatchType.Postfix, ModEntry.ModID);
                isPatched = false;
            }

            return true;
        }
        internal static void Postfix()
        {
            var configs = configField.GetValue();
            if (!configs.TryGetValue("Bee House", out var c))
                return;

            ModEntry.monitor.Log(ModEntry.helper.Translation.Get("general.removedPfmBeehouse"), LogLevel.Info);
            BeehouseDef = c;
            configs.Remove("Bee House");
        }
    }
}
