using System.Net.Sockets;
using Logging.Net;
using YAMNL.Data;

namespace YAMNL;

public class MinecraftConnection
{
    private readonly TcpClient TcpClient;
    private readonly Queue<PacketSendTask> PacketQueue;
    private readonly PacketFactory PacketFactory;
    private readonly CancellationTokenSource CancellationTokenSource;

    private MinecraftStream Stream;
    private byte[] SharedSecret;
    private Task? StreamLoopTask;

    public int CompressionThreshold { get; set; } = -1;
    public IPacketHandler? PacketHandler { get; set; }
    public ConnectionState ConnectionState { get; set; }
    public CancellationToken CancellationToken { get; set; }
    public bool Connected => IsTcpConnected(TcpClient);
    public EventHandler OnTcpDisconnected { get; set; }

    public MinecraftConnection(TcpClient tcpClient, bool isServerConnection = false)
    {
        ConnectionState = ConnectionState.Handshaking;
        PacketQueue = new();
        
        CancellationTokenSource = new CancellationTokenSource();
        CancellationToken = CancellationTokenSource.Token;

        TcpClient = tcpClient;

        PacketFactory = new PacketFactory(this, isServerConnection);
        Stream = new MinecraftStream(TcpClient.GetStream());
        StreamLoopTask = Task.Run(async () =>
        {
            try
            {
                await StreamLoop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }, CancellationToken);
    }

    public void SetEncryptionKey(byte[] key)
    {
        SharedSecret = key;
    }

    public void EnableEncryption() => Stream!.EnableEncryption(SharedSecret);

    public void SetCompressionThreshold(int compressionThreshold)
    {
        CompressionThreshold = compressionThreshold;
    }

    public Task SendPacket(IPacketPayload packet, CancellationToken? cancellation = null)
    {
        if (packet == null) throw new ArgumentNullException();
        //Logger.Debug("Queueing packet: " + packet.GetType().Name);

        var sendTask = new PacketSendTask(cancellation, packet, new TaskCompletionSource());
        PacketQueue.Enqueue(sendTask);

        return sendTask.SendingTsc.Task;
    }

    private Task ReadPacket()
    {
        var length = Stream.ReadVarInt();
        var uncompressedLength = 0;

        if (CompressionThreshold > 0)
        {
            var r = 0;
            uncompressedLength = Stream.ReadVarInt(out r);
            length -= r;
        }

        var data = Stream.Read(length);

        var packet = PacketFactory.BuildPacket(data, uncompressedLength);
        if (packet != null)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                //Logger.Debug("Received packet: " + packet.GetType().Name); // Causes cpu usage spikes
                try
                {
                    if (PacketHandler != null)
                        PacketHandler.HandleIncomming(packet, this).Wait(CancellationToken);
                    //PacketReceived?.Invoke(this, packet);
                }
                catch (Exception e)
                {
                    Logger.Error("There occurred an error while handling the packet: \n" + e);
                }
            });
        }

        return Task.CompletedTask;
    }

    private async Task StreamLoop()
    {
        while (!CancellationToken.IsCancellationRequested)
        {
            if (!IsTcpConnected(TcpClient))
            {
                OnTcpDisconnected?.Invoke(this, null);
                break;
            }
            
            try
            {
                if (ConnectionState != ConnectionState.Play)
                {
                    if (TcpClient.Available > 0)
                    {
                        await ReadPacket();
                    }
                    
                    await Task.Delay(1, CancellationToken);
                }
                else
                {
                    while (TcpClient.Available > 0)
                    {
                        await ReadPacket();
                    }

                    await Task.Delay(1, CancellationToken);
                }


                if (PacketQueue.Count == 0) continue;
                // Writing
                var packetTask = PacketQueue.Dequeue();

                if (packetTask.CancellationToken.HasValue && packetTask.CancellationToken.Value.IsCancellationRequested)
                {
                    packetTask.SendingTsc.SetCanceled(packetTask.CancellationToken.Value);
                    continue;
                }

                if (packetTask.Packet == null) // https://github.com/psu-de/MineSharp/issues/8#issue-1315635361
                {
                    // for now just ignore the packet,
                    // since i have no idea why this happens
                    if (packetTask.SendingTsc != null)
                        packetTask.SendingTsc.TrySetCanceled();
                    continue;
                }

                var packetBuffer = PacketFactory.WritePacket(packetTask.Packet);

                Stream!.DispatchPacket(packetBuffer);

                packetTask.SendingTsc.TrySetResult();
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        if (PacketHandler != null)
                            PacketHandler.HandleOutgoing(packetTask.Packet, this).Wait(CancellationToken);
                        //PacketSent?.Invoke(this, packetTask.Packet);
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Error while handling sent event of packet: \n" + e);
                    }
                });
            }
            catch (Exception e)
            {
                Logger.Error("Error in readLoop: " + e);
            }
        }
    }

    private bool IsTcpConnected(TcpClient client)
    {
        try
        {
            if (client != null && client.Client != null && client.Client.Connected)
            {
                /* pear to the documentation on Poll:
                 * When passing SelectMode.SelectRead as a parameter to the Poll method it will return 
                 * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
                 * -or- true if data is available for reading; 
                 * -or- true if the connection has been closed, reset, or terminated; 
                 * otherwise, returns false
                 */

                // Detect if client disconnected
                if (client.Client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] buff = new byte[1];
                    if (client.Client.Receive(buff, SocketFlags.Peek) == 0)
                    {
                        // Client disconnected
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }
}