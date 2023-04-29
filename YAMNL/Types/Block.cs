namespace YAMNL.Types
{
    public class BlockProperties
    {


        public BlockStateProperty[] Properties;
        public int State;

        public BlockProperties(BlockStateProperty[] properties, int defaultState)
        {
            Properties = properties;
            State = defaultState;
            Set(defaultState);
        }

        public BlockStateProperty? Get(string name)
        {
            return Properties.FirstOrDefault(p => p.Name == name);
        }

        public void Set(int data)
        {
            State = data;
            foreach (var property in Properties.Reverse())
            {
                property.SetValue(data % property.NumValues);
                data = data / property.NumValues;
            }
        }

        public BlockProperties Clone() => new BlockProperties(Properties.Clone() as BlockStateProperty[] ?? throw new Exception(), State);
    }

    public class BlockStateProperty
    {

        public enum BlockStatePropertyType
        {
            Enum,
            Bool,
            Int
        }

        public BlockStateProperty(string name, BlockStatePropertyType type, int numValues, string[]? values)
        {
            Name = name;
            Type = type;
            NumValues = numValues;
            AcceptedValues = values;
        }

        public string Name { get; set; }
        public BlockStatePropertyType Type { get; set; }
        public int State { get; set; }
        public int NumValues { get; set; }
        public string[]? AcceptedValues { get; set; }


        public void SetValue(int state)
        {
            if (state >= NumValues) throw new ArgumentOutOfRangeException();

            State = state;
        }

        public T GetValue<T>()
        {
            switch (Type)
            {
                case BlockStatePropertyType.Int:
                    if (typeof(T) != typeof(int)) throw new NotSupportedException();
                    return (T)(object)State;
                case BlockStatePropertyType.Bool:
                    if (typeof(T) != typeof(bool)) throw new NotSupportedException();
                    return (T)(object)!Convert.ToBoolean(State);
                case BlockStatePropertyType.Enum:
                    if (typeof(T) != typeof(string) || AcceptedValues == null) throw new NotSupportedException();
                    return (T)(object)AcceptedValues[State];
                default:
                    throw new NotImplementedException();

            }
        }
    }


    public class Block
    {

        public Block(int id, string name, string displayName, float hardness, float resistance, bool diggable, bool transparent, int filterLight, int emitLight, string boundingBox, int stackSize, string material, int defaultState, int minStateId, int maxStateId, int[]? harvestTools, BlockProperties properties)
        {
            Id = id;
            Name = name;
            DisplayName = displayName;
            Hardness = hardness;
            Resistance = resistance;
            Diggable = diggable;
            Transparent = transparent;
            FilterLight = filterLight;
            EmitLight = emitLight;
            BoundingBox = boundingBox;
            StackSize = stackSize;
            Material = material;
            DefaultState = defaultState;
            MinStateId = minStateId;
            MaxStateId = maxStateId;
            HarvestTools = harvestTools;
            Properties = properties;
        }

        public Block(int state, Position pos, int id, string name, string displayName, float hardness, float resistance, bool diggable, bool transparent, int filterLight, int emitLight, string boundingBox, int stackSize, string material, int defaultState, int minStateId, int maxStateId, int[]? harvestTools, BlockProperties properties)
            : this(id, name, displayName, hardness, resistance, diggable, transparent, filterLight, emitLight, boundingBox, stackSize, material, defaultState, minStateId, maxStateId, harvestTools, properties)
        {
            State = state;
            Position = pos;
            Properties.Set(Metadata);
        }

        public int Id { get; }
        public string Name { get; }
        public string DisplayName { get; }

        public float Hardness { get; }
        public float Resistance { get; }
        public bool Diggable { get; }
        public bool Transparent { get; }
        public int FilterLight { get; }
        public int EmitLight { get; }
        public string BoundingBox { get; }
        public int StackSize { get; }
        public string Material { get; }
        public int DefaultState { get; }
        public int MinStateId { get; }
        public int MaxStateId { get; }
        public int[]? HarvestTools { get; }
        public BlockProperties Properties { get; }

        public int? State { get; set; }
        public Position? Position { get; set; }
        public int Metadata => (int)State! - MinStateId;

        public override string ToString() => $"Block (Name={Name} Id={Id} StateId={State} Position={Position})";
    }
}
