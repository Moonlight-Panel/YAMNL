using YAMNL.Types.Enums;

namespace YAMNL.Types
{
    public class Biome
    {

        public Biome(int id, string name, string displayName, int biomeCategory, float temperature, string precipitation, float depth, Dimension dimension, int color, float rainfall)
        {
            Id = id;
            Name = name;
            Category = biomeCategory;
            Temperature = temperature;
            Precipitation = precipitation;
            Depth = depth;
            Dimension = dimension;
            DisplayName = displayName;
            Color = color;
            Rainfall = rainfall;
        }

        public int Id { get; }
        public string Name { get; }
        public int Category { get; }
        public float Temperature { get; }
        public string Precipitation { get; }
        public float Depth { get; }
        public Dimension Dimension { get; }
        public string DisplayName { get; }
        public int Color { get; }
        public float Rainfall { get; }

        public override string ToString() => $"Biome (Name={Name} Id={Id})";
    }
}
