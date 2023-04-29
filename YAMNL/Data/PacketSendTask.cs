namespace YAMNL.Data;

public struct PacketSendTask
{
    public CancellationToken? CancellationToken { get; set; }
    public IPacketPayload Packet { get; set; }
    public TaskCompletionSource SendingTsc { get; set; }

    public PacketSendTask(CancellationToken? cancellationToken, IPacketPayload packet, TaskCompletionSource tsc)
    {
        CancellationToken = cancellationToken;
        Packet = packet;
        SendingTsc = tsc;
    }
}