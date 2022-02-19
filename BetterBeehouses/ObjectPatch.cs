using HarmonyLib;
using Netcode;
using StardewValley;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BetterBeehouses
{
    [HarmonyPatch(typeof(Object))]
    class ObjectPatch
    {
        private static ILHelper minutesElapsedPatch = new ILHelper("Object: Minutes Elapsed")
            .SkipTo(new CodeInstruction[] { 
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, typeof(Object).MethodNamed("get_name")),
                new(OpCodes.Ldstr, "Bee House"),
                new(OpCodes.Callvirt, typeof(string).MethodNamed("Equals", new[]{typeof(string)}))
            })
            .Skip(2)
            .Remove()
            .Add(new CodeInstruction[] { 
                new(OpCodes.Call,typeof(ObjectPatch).MethodNamed(nameof(CanProduceHere)))
            })
            .Finish();

        private static ILHelper dayUpdatePatch = new ILHelper("Object: Day Update")
            .SkipTo(new CodeInstruction[] {
                new(OpCodes.Callvirt, typeof(GameLocation).MethodNamed("GetSeasonForLocation")),
                new(OpCodes.Ldstr, "winter"),
                new(OpCodes.Callvirt, typeof(string).MethodNamed("Equals",new[]{typeof(string)}))
            })
            .Add(new CodeInstruction[]
            {
                new(OpCodes.Ldarg_1),
                new(OpCodes.Call, typeof(ObjectPatch).MethodNamed(nameof(CantProduceToday)))
            })
            .SkipTo(new CodeInstruction[] { 
                new(OpCodes.Ldfld,typeof(Object).FieldNamed("minutesUntilReady")),
                new(OpCodes.Ldsfld,typeof(Game1).FieldNamed("timeOfDay"))
            })
            .Transform(new CodeInstruction[]{
                new(OpCodes.Call,typeof(Utility).MethodNamed("CalculateMinutesUntilMorning",new[]{typeof(int),typeof(int)}))
            }, ChangeDays)
            .Finish();

        private static ILHelper dropDownPatch = new ILHelper("Object: Drop Down Action")
            .SkipTo(new CodeInstruction[]
            {
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, typeof(Object).MethodNamed("get_name")),
                new(OpCodes.Ldstr,"Bee House"),
                new(OpCodes.Callvirt, typeof(string).MethodNamed("Equals", new[]{typeof(string)}))
            })
            .SkipTo(new CodeInstruction[] {
                new(OpCodes.Ldfld,typeof(Object).FieldNamed("minutesUntilReady")),
                new(OpCodes.Ldsfld,typeof(Game1).FieldNamed("timeOfDay"))
            })
            .Transform(new CodeInstruction[]{
                new(OpCodes.Call,typeof(Utility).MethodNamed("CalculateMinutesUntilMorning",new[]{typeof(int),typeof(int)}))
            }, ChangeDays)
            .Finish();

        private static ILHelper checkForActionPatch = new ILHelper("Object: Check for Action")
            .SkipTo(new CodeInstruction[]
            {
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, typeof(Object).MethodNamed("get_name")),
                new(OpCodes.Ldstr,"Bee House"),
                new(OpCodes.Callvirt,typeof(string).MethodNamed("Equals",new[]{typeof(string)}))
            })
            .SkipTo(new CodeInstruction[]
            {
                new(OpCodes.Callvirt, typeof(Character).MethodNamed("get_currentLocation")),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, typeof(Object).FieldNamed("tileLocation"))
            })
            .Skip()
            .Remove()
            .Add(new CodeInstruction(OpCodes.Call,typeof(ObjectPatch).MethodNamed(nameof(GetSearchRange))))
            .SkipTo(new CodeInstruction[]
            {
                new(OpCodes.Ldstr," Honey")
            })
            .SkipTo(new CodeInstruction[]
            {
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Callvirt, typeof(string).MethodNamed("Split", new[]{typeof(char),typeof(System.StringSplitOptions)})),
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Ldelem_Ref),
                new(OpCodes.Call, typeof(System.Convert).MethodNamed("ToInt32", new[]{typeof(string)}))
            })
            .Skip(2)
            .Add(new CodeInstruction[]
            {
                new(OpCodes.Conv_R4),
                new(OpCodes.Call,typeof(ObjectPatch).MethodNamed(nameof(GetMultiplier))),
                new(OpCodes.Mul),
                new(OpCodes.Conv_I4)
            })
            .SkipTo(new CodeInstruction[]
            {
                new(OpCodes.Call,typeof(Game1).MethodNamed("get_currentLocation")),
                new(OpCodes.Call,typeof(Game1).MethodNamed("GetSeasonForLocation")),
                new(OpCodes.Ldstr, "winter"),
                new(OpCodes.Callvirt, typeof(string).MethodNamed("Equals",new[] { typeof(string) }))
            })
            .Add(new CodeInstruction[]
            {
                new(OpCodes.Call,typeof(Game1).MethodNamed("get_currentLocation")),
                new(OpCodes.Call,typeof(ObjectPatch).MethodNamed(nameof(CantProduceToday)))
            })
            .SkipTo(new CodeInstruction[]
            {
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, typeof(Object).MethodNamed("get_name")),
                new(OpCodes.Ldstr, "Bee House"),
                new(OpCodes.Callvirt, typeof(string).MethodNamed("Equals",new[] { typeof(string) }))
            })
            .SkipTo(new CodeInstruction[] {
                new(OpCodes.Ldstr, "winter"),
                new(OpCodes.Callvirt, typeof(string).MethodNamed("Equals",new[] { typeof(string) }))
            })
            .Add(new CodeInstruction[]
            {
                new(OpCodes.Ldarg_1),
                new(OpCodes.Call, typeof(ObjectPatch).MethodNamed(nameof(CantProduceToday)))
            })
            .SkipTo(new CodeInstruction[] {
                new(OpCodes.Ldfld,typeof(Object).FieldNamed("minutesUntilReady")),
                new(OpCodes.Ldsfld,typeof(Game1).FieldNamed("timeOfDay"))
            })
            .Transform(new CodeInstruction[]{
                new(OpCodes.Call,typeof(Utility).MethodNamed("CalculateMinutesUntilMorning",new[]{typeof(int),typeof(int)}))
            }, ChangeDays)
            .Finish();

        //---------

        [HarmonyPatch("minutesElapsed")]
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> minutesElapsed(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in minutesElapsedPatch.Run(instructions))
                yield return code;
        }

        [HarmonyPatch("DayUpdate")]
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> DayUpdate(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in dayUpdatePatch.Run(instructions))
                yield return code;
        }

        [HarmonyPatch("performDropDownAction")]
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> DropDown(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in dropDownPatch.Run(instructions))
                yield return code;
        }

        [HarmonyPatch("checkForAction")]
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> checkForAction(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in checkForActionPatch.Run(instructions))
                yield return code;
        }

        //--------

        internal static IEnumerable<CodeInstruction> ChangeDays(IList<CodeInstruction> codes)
        {
            var op = new CodeInstruction(OpCodes.Call, typeof(ObjectPatch).MethodNamed(nameof(GetProduceDays)));
            op.labels.AddRange(codes[0].labels);
            codes[0].labels.Clear();
            yield return op;
            foreach (var code in codes)
                yield return code;
        }
        public static bool CantProduceToday(bool isWinter, GameLocation loc)
        {
            return isWinter && !Utils.GetProduceHere(loc, ModEntry.config.ProduceInWinter);
        }
        public static int GetSearchRange()
        {
            return ModEntry.config.FlowerRange;
        }
        public static int GetProduceDays(int original)
        {
            return System.Math.Max(original * ModEntry.config.DaysToProduce / 4, 1);
        }
        public static bool CanProduceHere(GameLocation loc)
        {
            var where = ModEntry.config.UsableIn;
            return where is Config.UsableOptions.Anywhere || loc.IsOutdoors || loc.isGreenhouse && where is not Config.UsableOptions.Outdoors;
        }
        public static float GetMultiplier()
        {
            return ModEntry.config.ValueMultiplier;
        } 
    }
}
