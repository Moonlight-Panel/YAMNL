using System.Net.Sockets;
using YAMNL.Encryption;

namespace YAMNL;

public class MinecraftStream : Stream
{
    private readonly NetworkStream _networkStream;

    private Stream _baseStream;
    private AesStream? _encryptionStream;

    public MinecraftStream(NetworkStream stream)
    {
        _networkStream = stream;
        _baseStream = _networkStream;
    }

    public override bool CanRead => _baseStream.CanRead;
    public override bool CanSeek => _baseStream.CanSeek;
    public override bool CanWrite => _baseStream.CanWrite;
    public override long Length => _baseStream.Length;

    public override long Position
    {
        get => _baseStream.Position;
        set => _baseStream.Position = value;
    }

    public bool IsAvailable => _networkStream.DataAvailable;

    public void EnableEncryption(byte[] sharedSecret)
    {
        var oldStream = _baseStream;
        _encryptionStream = new AesStream(oldStream, sharedSecret);
        _baseStream = _encryptionStream;
    }

    public override void Flush()
    {
        _baseStream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count) => _baseStream.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);

    public override void SetLength(long value) => _baseStream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => _baseStream.Write(buffer, offset, count);


    public byte[] Read(int length)
    {
        var buffer = new byte[length];
        _baseStream.Read(buffer, 0, length);
        return buffer;
    }

    public void Write(byte[] buffer)
    {
        _baseStream.Write(buffer, 0, buffer.Length);
    }

    public void DispatchPacket(PacketBuffer packetBuffer)
    {
        WriteVarInt((int)packetBuffer.Size);
        Write(packetBuffer.ToArray());
    }

    public void WriteVarInt(int value)
    {
        while (true)
        {
            if ((value & ~0x7F) == 0)
            {
                _baseStream.WriteByte((byte)value);
                return;
            }

            _baseStream.WriteByte((byte)(value & 0x7F | 0x80));
            value >>= 7;
        }
    }

    public int ReadVarInt(out int read)
    {
        var value = 0;
        var length = 0;
        byte currentByte;

        while (true)
        {
            currentByte = (byte)_baseStream.ReadByte();
            value |= (currentByte & 0x7F) << length * 7;

            length++;
            if (length > 5) throw new Exception("VarInt too big");

            if ((currentByte & 0x80) != 0x80)
            {
                break;
            }
        }

        read = length;
        return value;
    }

    public int ReadVarInt()
    {
        var value = 0;
        var length = 0;
        byte currentByte;

        while (true)
        {
            currentByte = (byte)_baseStream.ReadByte();
            value |= (currentByte & 0x7F) << length * 7;

            length++;
            if (length > 5) throw new Exception("VarInt too big");

            if ((currentByte & 0x80) != 0x80)
            {
                break;
            }
        }

        return value;
    }
}