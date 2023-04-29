namespace YAMNL.Types
{
    public abstract class Effect
    {

        public Effect(int id, string name, string displayName, bool isGood)
        {
            Id = id;
            Name = name;
            DisplayName = displayName;
            IsGood = isGood;
        }

        public Effect(int amplifier, DateTime startTime, int duration, int id, string name, string displayName, bool isGood)
            : this(id, name, displayName, isGood)
        {
            Amplifier = amplifier;
            StartTime = startTime;
            Duration = duration;
        }

        public int Id { get; }
        public string Name { get; }
        public string DisplayName { get; }
        public bool IsGood { get; }

        public DateTime StartTime { get; set; }
        public int Amplifier { get; set; }
        public int Duration { get; set; }

        public override string ToString() => $"Effect (Name={Name} Id={Id} Amplifier={Amplifier} Duration={Duration})";
    }
}
