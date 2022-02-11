using HarmonyLib;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BetterBeehouses
{
    class ILHelper
    {
        public readonly List<LocalBuilder> boxes;
        public readonly string name;
        public ILGenerator Generator
        {
            get { return currentGenerator; }
        }

        private enum ActionType { None, SkipTo, Add, AddF, Remove, RemoveTo, RemoveAt, RemoveL, Finish, Transform };
        public delegate IEnumerable<CodeInstruction> Transformer(IList<CodeInstruction> instructions);

        private readonly List<(ActionType action, object arg)> actionQueue = new();
        private IEnumerable<CodeInstruction> instructions;
        private IEnumerator<CodeInstruction> cursor;
        private int actionIndex = 0;
        private bool hasErrored = false;
        private ILGenerator currentGenerator = null;

        public ILHelper(string Name)
        {
            name = Name;
            boxes = new();
        }
        public ILHelper Skip(int count = 1)
        {
            actionQueue.Add((ActionType.None, count));
            return this;
        }
        public ILHelper SkipTo(IList<CodeInstruction> markers)
        {
            actionQueue.Add((ActionType.SkipTo, markers));
            return this;
        }
        public ILHelper Add(IList<CodeInstruction> codes)
        {
            actionQueue.Add((ActionType.Add, codes));
            return this;
        }
        public ILHelper Add(CodeInstruction code)
        {
            return Add(new CodeInstruction[] { code });
        }
        public ILHelper Add(Func<IList<LocalBuilder>, IEnumerable<CodeInstruction>> func)
        {
            actionQueue.Add((ActionType.AddF, func));
            return this;
        }
        public ILHelper Remove(int count = 1)
        {
            actionQueue.Add((ActionType.Remove, count));
            return this;
        }
        public ILHelper Remove(IList<CodeInstruction> markers)
        {
            actionQueue.Add((ActionType.RemoveL, markers));
            return this;
        }
        public ILHelper RemoveTo(IList<CodeInstruction> markers)
        {
            actionQueue.Add((ActionType.RemoveTo, markers));
            return this;
        }
        public ILHelper RemoveAt(IList<CodeInstruction> markers)
        {
            actionQueue.Add((ActionType.RemoveAt, markers));
            return this;
        }
        public ILHelper Finish()
        {
            actionQueue.Add((ActionType.Finish, null));
            return this;
        }
        public ILHelper Transform(IList<CodeInstruction> markers, Transformer transformer)
        {
            actionQueue.Add((ActionType.Transform, (markers, transformer)));
            return this;
        }
        public void Reset()
        {
            actionQueue.Clear();
            boxes.Clear();
        }
        public IEnumerable<CodeInstruction> Run(IEnumerable<CodeInstruction> Instructions, ILGenerator generator = null)
        {
            ModEntry.monitor.Log("Now applying patch '" + name + "'...", LogLevel.Debug);
            currentGenerator = generator;
            instructions = Instructions;
            cursor = instructions.GetEnumerator();
            actionIndex = 0;
            hasErrored = false;
            foreach (var item in actionQueue)
            {
                int c = 0;
                int count = 0;
                switch (item.action)
                {
                    case ActionType.None:
                        count = (int)item.arg;
                        while (c < count && cursor.MoveNext())
                        {
                            yield return cursor.Current;
                            c++;
                        }
                        break;
                    case ActionType.SkipTo:
                        foreach (var code in skipTo((IList<CodeInstruction>)item.arg))
                            yield return code;
                        break;
                    case ActionType.Add:
                        foreach (var code in (IList<CodeInstruction>)item.arg)
                            yield return code;
                        break;
                    case ActionType.AddF:
                        foreach (var code in ((Func<IList<LocalBuilder>, IEnumerable<CodeInstruction>>)item.arg)(boxes))
                            yield return code;
                        break;
                    case ActionType.Remove:
                        count = (int)item.arg;
                        while (c < count && cursor.MoveNext())
                            c++;
                        break;
                    case ActionType.RemoveTo:
                        foreach (var code in removeTo((IList<CodeInstruction>)item.arg))
                            yield return code;
                        break;
                    case ActionType.RemoveAt:
                        foreach (var code in removeAt((IList<CodeInstruction>)item.arg))
                            yield return code;
                        break;
                    case ActionType.RemoveL:
                        foreach (var code in removeChunk((IList<CodeInstruction>)item.arg))
                            yield return code;
                        break;
                    case ActionType.Finish:
                        while (cursor.MoveNext())
                            yield return cursor.Current;
                        break;
                    case ActionType.Transform:
                        (var markers, var transformer) = (ValueTuple<IList<CodeInstruction>, Transformer>)item.arg;
                        foreach (var code in transform(markers, transformer))
                            yield return code;
                        break;
                }

                if (hasErrored)
                    break;
                actionIndex++;
            }
            currentGenerator = null;
            if (hasErrored)
                ModEntry.monitor.Log("Failed to correctly apply patch '" + name + "'! May cause problems!", LogLevel.Error);
            else
                ModEntry.monitor.Log("Successfully applied patch '" + name + "'.", LogLevel.Debug);
        }
        private IEnumerable<CodeInstruction> transform(IList<CodeInstruction> Anchors, Transformer transformer)
        {
            int marker = 0;
            List<CodeInstruction> saved = new();
            while (cursor.MoveNext())
            {
                var s = Anchors[marker];
                var code = cursor.Current;
                if (code.opcode == s.opcode && CompareOperands(code.operand, s.operand))
                {
                    marker++;
                    saved.Add(code);
                }
                else
                {
                    foreach (var item in saved)
                        yield return item;
                    yield return code;
                    saved.Clear();
                    marker = 0;
                }
                if (marker >= Anchors.Count)
                {
                    ModEntry.monitor.Log("Found markers for '" + name + "':" + actionIndex.ToString(), LogLevel.Trace);
                    foreach (var item in transformer(saved))
                        yield return item;
                    yield break;
                }
            }
            hasErrored = true;
            ModEntry.monitor.Log("Failed to apply patch component '" + name + "':" + actionIndex.ToString() + " ; Marker instructions not found!", LogLevel.Error);
        }
        private IEnumerable<CodeInstruction> skipTo(IList<CodeInstruction> Anchors)
        {
            int marker = 0;
            while (cursor.MoveNext())
            {
                var s = Anchors[marker];
                var code = cursor.Current;
                if (code.opcode == s.opcode && CompareOperands(code.operand, s.operand))
                {
                    marker++;
                    if (code.operand is LocalBuilder b)
                        boxes.Add(b);
                }
                else
                {
                    boxes.Clear();
                    marker = 0;
                }
                yield return code;

                if (marker >= Anchors.Count)
                {
                    ModEntry.monitor.Log("Found markers for '" + name + "':" + actionIndex.ToString(), LogLevel.Trace);
                    yield break;
                }
            }
            hasErrored = true;
            ModEntry.monitor.Log("Failed to apply patch component '" + name + "':" + actionIndex.ToString() + " ; Marker instructions not found!", LogLevel.Error);
        }
        private IEnumerable<CodeInstruction> removeTo(IList<CodeInstruction> Anchors)
        {
            int marker = 0;
            List<CodeInstruction> saved = new();
            while (cursor.MoveNext())
            {
                var s = Anchors[marker];
                var code = cursor.Current;
                if (code.opcode == s.opcode && CompareOperands(code.operand, s.operand))
                {
                    marker++;
                    saved.Add(code);
                    if (code.operand is LocalBuilder b)
                        boxes.Add(b);
                }
                else
                {
                    boxes.Clear();
                    saved.Clear();
                    marker = 0;
                }
                if (marker >= Anchors.Count)
                {
                    foreach (var inst in saved)
                    {
                        yield return inst;
                    }
                    ModEntry.monitor.Log("Found markers for '" + name + "':" + actionIndex.ToString(), LogLevel.Trace);
                    yield break;
                }
            }
            hasErrored = true;
            ModEntry.monitor.Log("Failed to apply patch component '" + name + "':" + actionIndex.ToString() + " ; Marker instructions not found!", LogLevel.Error);
        }
        private IEnumerable<CodeInstruction> removeAt(IList<CodeInstruction> Anchors)
        {
            int marker = 0;
            while (cursor.MoveNext())
            {
                var s = Anchors[marker];
                var code = cursor.Current;
                if (code.opcode == s.opcode && CompareOperands(code.operand, s.operand))
                {
                    marker++;
                    if (code.operand is LocalBuilder b)
                        boxes.Add(b);
                }
                else
                {
                    boxes.Clear();
                    marker = 0;
                }
                if (marker >= Anchors.Count)
                {
                    ModEntry.monitor.Log("Found markers for '" + name + "':" + actionIndex.ToString(), LogLevel.Trace);
                    yield break;
                }
            }
            hasErrored = true;
            ModEntry.monitor.Log("Failed to apply patch component '" + name + "':" + actionIndex.ToString() + " ; Marker instructions not found!", LogLevel.Error);
        }
        private IEnumerable<CodeInstruction> removeChunk(IList<CodeInstruction> Anchors)
        {
            int marker = 0;
            List<CodeInstruction> saved = new();
            while (cursor.MoveNext())
            {
                var s = Anchors[marker];
                var code = cursor.Current;
                if (code.opcode == s.opcode && CompareOperands(code.operand, s.operand))
                {
                    marker++;
                    saved.Add(code);
                    if (code.operand is LocalBuilder b)
                        boxes.Add(b);
                }
                else
                {
                    boxes.Clear();
                    marker = 0;
                    foreach (var inst in saved)
                        yield return inst;
                    saved.Clear();
                    yield return code;
                }
                if (marker >= Anchors.Count)
                {
                    ModEntry.monitor.Log("Found markers for '" + name + "':" + actionIndex.ToString(), LogLevel.Trace);
                    yield break;
                }
            }
            hasErrored = true;
            ModEntry.monitor.Log("Failed to apply patch component '" + name + "':" + actionIndex.ToString() + " ; Marker instructions not found!", LogLevel.Error);
        }
        public Label? FindAddress(CodeInstruction[] Anchors)
        {
            int marker = 0;
            foreach (var code in instructions)
            {
                var s = Anchors[marker];
                if (code.opcode == s.opcode && CompareOperands(code.operand, s.operand))
                {
                    marker++;
                }
                else
                {
                    marker = 0;
                }
                if (marker >= Anchors.Length)
                {
                    return (code.labels.Count > 0) ? code.labels[0] : null;
                }
            }
            return null;
        }
        public static bool CompareOperands(object op1, object op2)
        {
            if (op1 == null || op1.Equals(op2))
                return true;

            if (op1 is LocalBuilder oper1 && op2 is ValueTuple<int, Type> oper2)
            {
                return (oper2.Item1 < 0 || oper1.LocalIndex == oper2.Item1) && (oper2.Item2 == null || oper1.LocalType == oper2.Item2);
            }
            return false;
        }
    }
}
