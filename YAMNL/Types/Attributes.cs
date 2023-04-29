﻿namespace YAMNL.Types
{
    public class Attribute
    {

        public Attribute(string key, double @base, List<Modifier> modifiers)
        {
            Key = key;
            Base = @base;
            Modifiers = modifiers.ToDictionary(x => x.UUID);
        }
        public string Key { get; set; }
        public double Base { get; set; }
        public Dictionary<UUID, Modifier> Modifiers { get; set; }
        public double Value =>
            Modifiers.GroupBy(m => m.Value.Operation)
                .OrderBy(x => x.Key)
                .Aggregate(Base, (x, t) =>
                {
                    var op = t.Key;
                    var modifiers = t.Select(x => x.Value);
                    return op switch
                    {
                        ModifierOp.Add => modifiers.Aggregate(x, (y, t) => y += t.Amount),
                        ModifierOp.MultiplyBase => x * (1 + modifiers.Select(x => x.Amount).Sum()),
                        ModifierOp.Multiply => modifiers.Aggregate(x, (y, t) => y *= 1 + t.Amount),
                        _ => throw new NotSupportedException($"Modifier operation {op} not supported")
                    };
                });
    }

    public enum ModifierOp
    {
        Add = 0,
        MultiplyBase = 1,
        Multiply = 2
    }

    public struct Modifier
    {
        public UUID UUID { get; set; }
        public double Amount { get; set; }
        public ModifierOp Operation { get; set; }

        public Modifier(UUID uuid, double amount, ModifierOp operation)
        {
            UUID = uuid;
            Amount = amount;
            Operation = operation;
        }
    }
}
