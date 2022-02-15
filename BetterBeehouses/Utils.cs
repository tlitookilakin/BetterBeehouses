using HarmonyLib;
using StardewValley;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace BetterBeehouses
{
    static class Utils
    {
        public static MethodInfo MethodNamed(this Type type, string name)
        {
            return AccessTools.Method(type, name);
        }
        public static MethodInfo MethodNamed(this Type type, string name, Type[] args)
        {
            return AccessTools.Method(type, name, args);
        }
        public static FieldInfo FieldNamed(this Type type, string name)
        {
            return AccessTools.Field(type, name);
        }
        public static CodeInstruction WithLabels(this CodeInstruction code, params Label[] labels)
        {
            foreach (Label label in labels)
                code.labels.Add(label);

            return code;
        }
        public static bool GetProduceHere(GameLocation loc, Config.ProduceWhere where)
        {
            return where is not Config.ProduceWhere.Never && (!loc.isOutdoors || where is Config.ProduceWhere.Always);
        }
        public static Crop CropFromObj(StardewValley.Object obj)
        {
            Crop ret = new();
            ret.indexOfHarvest.Value = obj.parentSheetIndex;
            return ret;
        }
    }
}
