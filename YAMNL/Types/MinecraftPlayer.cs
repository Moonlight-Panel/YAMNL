using YAMNL.Types.Enums;

namespace YAMNL.Types
{
    public class MinecraftPlayer
    {

        public MinecraftPlayer(string username, UUID uuid, int ping, GameMode gamemode, Entity entity)
        {
            Username = username;
            UUID = uuid;
            Ping = ping;
            GameMode = gamemode;
            Entity = entity;
        }

        public string Username { get; set; }
        public UUID UUID { get; set; }
        public int Ping { get; set; }
        public GameMode GameMode { get; set; }
        public Entity Entity { get; set; }

        public Vector3 GetHeadPosition() => Entity.Position.Plus(Vector3.Up);
    }
}
