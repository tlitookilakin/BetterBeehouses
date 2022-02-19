using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BetterBeehouses.integration
{
    class AutomatePatch
    {
        private static ILHelper getStatePatch;
        private static ILHelper getOutputPatch;
        private static ILHelper resetPatch;
        public static bool Setup()
        {
            if (!ModEntry.helper.ModRegistry.IsLoaded("Pathoschild.Automate"))
                return false;

            ModEntry.monitor.Log(ModEntry.helper.Translation.Get("general.automateWarning"), LogLevel.Warn);

            var targetClass = AccessTools.TypeByName("Pathoschild.Stardew.Automate.Framework.Machines.Objects.BeeHouseMachine");
            getStatePatch = statePatch();
            getOutputPatch = outputPatch();
            resetPatch = getResetPatch();

            ModEntry.harmony.Patch(targetClass.MethodNamed("GetState"),transpiler: new(typeof(AutomatePatch),"PatchState"));
            ModEntry.harmony.Patch(targetClass.MethodNamed("GetOutput"), transpiler: new(typeof(AutomatePatch), "PatchOutput"));
            ModEntry.harmony.Patch(targetClass.MethodNamed("Reset"), transpiler: new(typeof(AutomatePatch), "PatchReset"));

            return true;
        }

        public static IEnumerable<CodeInstruction> PatchState(IEnumerable<CodeInstruction> instructions)
        {
            return getStatePatch.Run(instructions);
        }
        public static IEnumerable<CodeInstruction> PatchOutput(IEnumerable<CodeInstruction> instructions)
        {
            return getOutputPatch.Run(instructions);
        }
        public static IEnumerable<CodeInstruction> PatchReset(IEnumerable<CodeInstruction> instructions)
        {
            return resetPatch.Run(instructions);
        }

        private static ILHelper statePatch()
        {
            return new ILHelper("Automate:GetState")
                .Remove(new CodeInstruction[]
                {
                    new(OpCodes.Callvirt,typeof(GameLocation).MethodNamed("GetSeasonForLocation")),
                    new(OpCodes.Ldstr,"winter")
                })
                .Remove()
                .Add(new CodeInstruction[]
                {
                    new(OpCodes.Call,typeof(AutomatePatch).MethodNamed("CantWorkHere"))
                })
                .Finish();
        }
        private static ILHelper outputPatch()
        {
            return new ILHelper("Automate:GetOutput")
                .SkipTo(new CodeInstruction[]
                {
                    new(OpCodes.Dup),
                    new(OpCodes.Ldloc_0),
                    new(OpCodes.Callvirt,typeof(Object).MethodNamed("get_Price"))
                })
                .Skip(2)
                .Add(new CodeInstruction[] {
                    new(OpCodes.Conv_R4),
                    new(OpCodes.Call,typeof(ObjectPatch).MethodNamed("GetMultiplier")),
                    new(OpCodes.Mul),
                    new(OpCodes.Conv_I4)
                })
                .Finish();
        }
        private static ILHelper getResetPatch()
        {
            return new ILHelper("Automate:Reset")
                .SkipTo(new CodeInstruction[]
                {
                    new(OpCodes.Ldloc_0),
                    new(OpCodes.Ldsfld,typeof(Game1).FieldNamed("timeOfDay"))
                })
                .Transform(new CodeInstruction[]{
                    new(OpCodes.Call,typeof(Utility).MethodNamed("CalculateMinutesUntilMorning",new[]{typeof(int),typeof(int)}))
                }, ObjectPatch.ChangeDays)
                .Finish();
        }
        public static bool CantWorkHere(GameLocation loc)
        {
            return ObjectPatch.CantProduceToday(loc.GetSeasonForLocation() == "winter", loc);
        }
    }
}
