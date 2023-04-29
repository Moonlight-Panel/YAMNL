namespace YAMNL;

public interface IPacketHandler
{
    public Task HandleIncomming(IPacketPayload packetPayload, MinecraftConnection connection);
    public Task HandleOutgoing(IPacketPayload packetPayload, MinecraftConnection connection);
}