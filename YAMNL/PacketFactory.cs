using fNbt;
using ICSharpCode.SharpZipLib.Zip.Compression;
using Logging.Net;
using YAMNL.Data;
using YAMNL.Handshaking.Clientbound;

namespace YAMNL;

public class PacketFactory
{
    private readonly Dictionary<ConnectionState, IPacketFactory> ClientboundPacketFactories;
    private readonly Dictionary<ConnectionState, IPacketFactory> ServerboundPacketFactories;
    private readonly MinecraftConnection Connection;
    private readonly bool IsServerConnection;

    public PacketFactory(MinecraftConnection minecraftConnection, bool isServerConnection = false)
    {
        Connection = minecraftConnection;

        IsServerConnection = isServerConnection;

        ClientboundPacketFactories = new Dictionary<ConnectionState, IPacketFactory>();
        ClientboundPacketFactories.Add(ConnectionState.Handshaking, new Handshaking.Clientbound.HandshakingPacketFactory());
        ClientboundPacketFactories.Add(ConnectionState.Status, new Status.Clientbound.StatusPacketFactory());
        ClientboundPacketFactories.Add(ConnectionState.Login, new Login.Clientbound.LoginPacketFactory());
        ClientboundPacketFactories.Add(ConnectionState.Play, new Play.Clientbound.PlayPacketFactory());


        ServerboundPacketFactories = new Dictionary<ConnectionState, IPacketFactory>();
        ServerboundPacketFactories.Add(ConnectionState.Handshaking,
            new Handshaking.Serverbound.HandshakingPacketFactory());
        ServerboundPacketFactories.Add(ConnectionState.Status,
            new Status.Serverbound.StatusPacketFactory());
        ServerboundPacketFactories.Add(ConnectionState.Login, new Login.Serverbound.LoginPacketFactory());
        ServerboundPacketFactories.Add(ConnectionState.Play, new Play.Serverbound.PlayPacketFactory());
    }

    public static PacketBuffer Decompress(byte[] buffer, int length)
    {
        if (length == 0) return new PacketBuffer(buffer);

        var inflater = new Inflater();

        inflater.SetInput(buffer);
        var abyte1 = new byte[length];
        inflater.Inflate(abyte1);
        inflater.Reset();
        return new PacketBuffer(abyte1);
    }

    public static PacketBuffer Compress(PacketBuffer input, int compressionThreshold)
    {
        var output = new PacketBuffer();
        if (input.Size < compressionThreshold)
        {
            output.WriteVarInt(0);
            output.WriteRaw(input.ToArray());
            return output;
        }

        var buffer = input.ToArray();
        output.WriteVarInt(buffer.Length);

        var deflater = new Deflater();
        deflater.SetInput(buffer);
        deflater.Finish();

        var deflateBuf = new byte[8192];
        while (!deflater.IsFinished)
        {
            var j = deflater.Deflate(deflateBuf);
            output.WriteRaw(deflateBuf, 0, j);
        }

        deflater.Reset();
        return output;
    }


    public IPacketPayload? BuildPacket(byte[] data, int uncompressedLength)
    {
        PacketBuffer packetBuffer;
        if (uncompressedLength > 0) packetBuffer = Decompress(data, uncompressedLength);
        else packetBuffer = new PacketBuffer(data);

        try
        {
            IPacket packet = null;

            if (IsServerConnection)
            {
                packet = ServerboundPacketFactories[Connection.ConnectionState].ReadPacket(packetBuffer);
            }
            else
            {
                packet = ClientboundPacketFactories[Connection.ConnectionState].ReadPacket(packetBuffer);
            }
            
            if (packetBuffer.ReadableBytes > 0)
                Logger.Debug(
                    $"PacketBuffer should be empty after reading ({packet.Name})"); //throw new Exception("PacketBuffer must be empty after reading");

            if (IsServerConnection)
            {
                return packet switch
                {
                    Packet chPacket => (IPacketPayload)chPacket.Params.Value!,
                    Status.Serverbound.Packet csPacket => (IPacketPayload)csPacket.Params.Value!,
                    Handshaking.Serverbound.Packet chPacket => (IPacketPayload)chPacket.Params.Value!,
                    Login.Serverbound.Packet clPacket => (IPacketPayload)clPacket.Params.Value!,
                    Play.Serverbound.Packet cpPacket => (IPacketPayload)cpPacket.Params.Value!,
                    _ => throw new Exception()
                };
            }
            else
            {
                return packet switch
                {
                    Packet chPacket => (IPacketPayload)chPacket.Params.Value!,
                    Status.Clientbound.Packet csPacket => (IPacketPayload)csPacket.Params.Value!,
                    Login.Clientbound.Packet clPacket => (IPacketPayload)clPacket.Params.Value!,
                    Play.Clientbound.Packet cpPacket => (IPacketPayload)cpPacket.Params.Value!,
                    _ => throw new Exception()
                };
            }
        }
        catch (NbtFormatException)
        {
            return null;
        }
        catch (Exception e)
        {
            Logger.Warn("Error reading packet!");
            Logger.Warn(e.ToString());
            return null;
        }
    }

    public PacketBuffer WritePacket(IPacketPayload packet)
    {
        try
        {
            var packetBuffer = new PacketBuffer();

            if (IsServerConnection)
            {
                ClientboundPacketFactories[Connection.ConnectionState].WritePacket(packetBuffer, packet);
            }
            else
            {
                ServerboundPacketFactories[Connection.ConnectionState].WritePacket(packetBuffer, packet);
            }

            if (Connection.CompressionThreshold > 0)
            {
                packetBuffer = Compress(packetBuffer, Connection.CompressionThreshold);
            }

            return packetBuffer;
        }
        catch (Exception ex)
        {
            Logger.Warn($"Error while writing packet of type {packet.GetType().FullName}: " + ex);
            throw new Exception($"Error while writing packet of type {packet.GetType().FullName}", ex);
        }
    }
}