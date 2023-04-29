//////////////////////////////////////////////////////////////
//   Generated Protocol Data for Minecraft Version 1.19.2   //
//////////////////////////////////////////////////////////////
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fNbt;
using YAMNL.Types;

namespace YAMNL
{
    public partial class PacketBuffer {

        protected MemoryStream _buffer;

        public long Size => _buffer.Length;
        public long ReadableBytes => _buffer.Length - _buffer.Position;
        public long Position => _buffer.Position;


        public PacketBuffer() {
            _buffer = new MemoryStream();
        }

        public PacketBuffer(byte[] buffer) {
            _buffer = new MemoryStream(buffer);
        }

        public byte[] ToArray() {
            return _buffer.ToArray();
        }

        public string HexDump() {
            return string.Join(" ", ToArray().Select(x => x.ToString("X2")));
        }
        

        public byte[] ReadRaw(int length) {
            byte[] buffer = new byte[length];
            _buffer.Read(buffer, 0, length);
            return buffer;
        }

        public void WriteRaw(byte[] data, int offset, int length) {
            _buffer.Write(data, offset, length);
        }

        public void WriteRaw(byte[] data) {
            _buffer.Write(data, 0, data.Length);
        }
    }

    public interface IPacketPayload {
        
    }

    public interface IPacket {
		public string Name { get; set; }
    }

    public interface IPacketFactory {
        public IPacket ReadPacket(PacketBuffer buffer);
        public void WritePacket(PacketBuffer buffer, IPacketPayload packet);
    }

    public class VarInt {

        public bool IsLong => (Value & (long)-4294967296) != 0;

        public long Value;

        public VarInt(long value) {
            Value = value;
        }

        public static VarInt Read(PacketBuffer buffer) {
            long value = 0;
            int shift = 0;

            while (true) {
                byte b = buffer.ReadU8();
                value |= ((b & (long)0x7f) << shift); // Add the bits to our number, except MSB
                if ((b & 0x80) == 0x00)  // If the MSB is not set, we return the number
                    break;

                shift += 7; // we only have 7 bits, MSB being the return-trigger
                if (shift >= 64) throw new Exception("varint is too big"); // Make sure our shift don't overflow.
            }

            return new VarInt(value);
        }

        public void Write(PacketBuffer buffer) {
            while ((Value & ~0x7F) != 0x00) {
                buffer.WriteU8((byte)((Value & 0xFF) | 0x80));
                Value >>= 7;
            }
            buffer.WriteU8((byte)Value);
        }

        public static implicit operator int(VarInt value) => (int)value.Value;
        public static implicit operator VarInt(int value) => new VarInt(value);

        public override string ToString() {
            return Value.ToString();
        }
    }
}
namespace YAMNL
{
    public partial class PacketBuffer {
		#region Reading

		public VarInt ReadVarInt() {
    long value = 0;
    int shift = 0;

    while (true) {
        byte b = ReadU8();
        value |= ((b & (long)0x7f) << shift); // Add the bits to our number, except MSB
        if ((b & 0x80) == 0x00)  // If the MSB is not set, we return the number
            break;

        shift += 7; // we only have 7 bits, MSB being the return-trigger
        if (shift >= 64) throw new Exception("varint is too big"); // Make sure our shift don't overflow.
    }

    return new VarInt(value);
}
		public string ReadPString(Func<PacketBuffer, VarInt> lengthReader, Encoding? encoding = null) {
    byte[] data = ReadRaw(lengthReader(this));
    return (encoding ?? Encoding.UTF8).GetString(data);
}
		public byte[] ReadBuffer(int count) {
    return ReadArray<byte>(count, (PacketBuffer buffer) => buffer.ReadU8());
}
		public byte ReadU8(){
    byte[] raw = ReadRaw(sizeof(byte));
    if (BitConverter.IsLittleEndian) Array.Reverse(raw);
    
    return (byte)raw[0];
    
}
		public ushort ReadU16(){
    byte[] raw = ReadRaw(sizeof(ushort));
    if (BitConverter.IsLittleEndian) Array.Reverse(raw);
    
    return BitConverter.ToUInt16(raw);
    
}
		public uint ReadU32(){
    byte[] raw = ReadRaw(sizeof(uint));
    if (BitConverter.IsLittleEndian) Array.Reverse(raw);
    
    return BitConverter.ToUInt32(raw);
    
}
		public ulong ReadU64(){
    byte[] raw = ReadRaw(sizeof(ulong));
    if (BitConverter.IsLittleEndian) Array.Reverse(raw);
    
    return BitConverter.ToUInt64(raw);
    
}
		public sbyte ReadI8(){
    byte[] raw = ReadRaw(sizeof(sbyte));
    if (BitConverter.IsLittleEndian) Array.Reverse(raw);
    
    return (sbyte)raw[0];
    
}
		public short ReadI16(){
    byte[] raw = ReadRaw(sizeof(short));
    if (BitConverter.IsLittleEndian) Array.Reverse(raw);
    
    return BitConverter.ToInt16(raw);
    
}
		public int ReadI32(){
    byte[] raw = ReadRaw(sizeof(int));
    if (BitConverter.IsLittleEndian) Array.Reverse(raw);
    
    return BitConverter.ToInt32(raw);
    
}
		public long ReadI64(){
    byte[] raw = ReadRaw(sizeof(long));
    if (BitConverter.IsLittleEndian) Array.Reverse(raw);
    
    return BitConverter.ToInt64(raw);
    
}
		public bool ReadBool(){
    byte[] raw = ReadRaw(sizeof(bool));
    if (BitConverter.IsLittleEndian) Array.Reverse(raw);
    
    return BitConverter.ToBoolean(raw);
    
}
		public float ReadF32(){
    byte[] raw = ReadRaw(sizeof(float));
    if (BitConverter.IsLittleEndian) Array.Reverse(raw);
    
    return BitConverter.ToSingle(raw);
    
}
		public double ReadF64(){
    byte[] raw = ReadRaw(sizeof(double));
    if (BitConverter.IsLittleEndian) Array.Reverse(raw);
    
    return BitConverter.ToDouble(raw);
    
}
		public UUID ReadUUID() {
    long l1 = ReadI64();
    long l2 = ReadI64();
    return new UUID(l1, l2);
}
		public T? ReadOption<T>(Func<PacketBuffer, T> reader) { 
    bool present = ReadBool();
    if (!present) return default(T);
    return reader(this);
}
		public T[] ReadEntityMetadataLoop<T>(int endVal, Func<PacketBuffer, T> reader) {
    List<T> data = new List<T>();
    
    while (true) {
        if (ReadU8() == endVal) {
            return data.ToArray();
        } else {
            _buffer.Position -= 1;
        }
        data.Add(reader(this));
    }
}
		public T[] ReadTopBitSetTerminatedArray<T>(Func<PacketBuffer, T> reader) {
    List<T> data = new List<T>();
    
    while (true) {
        var next = ReadU8();
        var clone = new MemoryStream();
        clone.WriteByte((byte)(next & 127));
        _buffer.CopyTo(clone);
        _buffer = new MemoryStream(clone.GetBuffer());
        data.Add(reader(this));
        if ((next & 128) == 0) {
            return data.ToArray();
        }
    }
}
		public object? ReadVoid() {
    return null;
}
		public T[] ReadArray<T>(int length, Func<PacketBuffer, T> reader) {
    T[] array = new T[length];
    for (int i = 0; i < length; i++)
        array[i] = reader(this);
    return array;
}
		public byte[] ReadRestBuffer() {
    return ReadRaw((int)ReadableBytes);
}
		public NbtCompound ReadNbt() {
    NbtTagType t = (NbtTagType)ReadU8();
    if (t != NbtTagType.Compound) return null;
    _buffer.Position--;

    NbtFile file = new NbtFile() {BigEndian = true};

    file.LoadFromStream(_buffer, NbtCompression.None);

    return (NbtCompound)file.RootTag;
}

		public NbtCompound? ReadOptionalNbt() {
    NbtTagType t = (NbtTagType)ReadU8();
    if (t != NbtTagType.Compound) return null;
    _buffer.Position--;

    NbtFile file = new NbtFile() {BigEndian = true};

    file.LoadFromStream(_buffer, NbtCompression.None);

    return (NbtCompound)file.RootTag;
}


		#endregion
		#region Writing

		public void WriteVarInt(VarInt value) {
    var Value = value.Value;
    while ((Value & ~0x7F) != 0x00) {
        WriteU8((byte)((Value & 0xFF) | 0x80));
        Value >>= 7;
    }
    WriteU8((byte)Value);
}
		public void WritePString(string value, Action<PacketBuffer, VarInt> lengthEncoder, Encoding? encoding = null) {
    byte[] data = (encoding ?? Encoding.UTF8).GetBytes(value);
    lengthEncoder(this, data.Length);
    WriteRaw(data);
}
		
public void WriteBuffer(byte[] array, Action<PacketBuffer, byte> lengthEncoder) {
    lengthEncoder(this, (byte)array.Length);
    EncodeArray<byte>(array, (buffer, x) => buffer.WriteU8(x));
}

public void WriteBuffer(byte[] array, Action<PacketBuffer, VarInt> lengthEncoder) {
    lengthEncoder(this, array.Length);
    EncodeArray<byte>(array, (buffer, x) => buffer.WriteU8(x));
}
		public void WriteU8(byte value) {
    _buffer.WriteByte((byte)value);
}
		public void WriteU16(ushort value) {
    byte[] bytes = BitConverter.GetBytes(value);
    if (BitConverter.IsLittleEndian) Array.Reverse(bytes);

    WriteRaw(bytes);
}
		public void WriteU32(uint value) {
    byte[] bytes = BitConverter.GetBytes(value);
    if (BitConverter.IsLittleEndian) Array.Reverse(bytes);

    WriteRaw(bytes);
}
		public void WriteU64(ulong value) {
    byte[] bytes = BitConverter.GetBytes(value);
    if (BitConverter.IsLittleEndian) Array.Reverse(bytes);

    WriteRaw(bytes);
}
		public void WriteI8(sbyte value) {
    _buffer.WriteByte((byte)value);
}
		public void WriteI16(short value) {
    byte[] bytes = BitConverter.GetBytes(value);
    if (BitConverter.IsLittleEndian) Array.Reverse(bytes);

    WriteRaw(bytes);
}
		public void WriteI32(int value) {
    byte[] bytes = BitConverter.GetBytes(value);
    if (BitConverter.IsLittleEndian) Array.Reverse(bytes);

    WriteRaw(bytes);
}
		public void WriteI64(long value) {
    byte[] bytes = BitConverter.GetBytes(value);
    if (BitConverter.IsLittleEndian) Array.Reverse(bytes);

    WriteRaw(bytes);
}
		public void WriteBool(bool value) {
    byte[] bytes = BitConverter.GetBytes(value);
    if (BitConverter.IsLittleEndian) Array.Reverse(bytes);

    WriteRaw(bytes);
}
		public void WriteF32(float value) {
    byte[] bytes = BitConverter.GetBytes(value);
    if (BitConverter.IsLittleEndian) Array.Reverse(bytes);

    WriteRaw(bytes);
}
		public void WriteF64(double value) {
    byte[] bytes = BitConverter.GetBytes(value);
    if (BitConverter.IsLittleEndian) Array.Reverse(bytes);

    WriteRaw(bytes);
}
		public void WriteUUID(UUID value) {
    WriteI64(value.MostSignificantBits);
    WriteI64(value.LeastSignificantBits);
}
		public void WriteOption<T>(T value, Action<PacketBuffer, T> encoder) where T : class {
    if (value == null) {
        WriteBool(false);
        return;
    }
    WriteBool(true);
    encoder(this, value);
}
public void WriteOption<T>(Nullable<T> value, Action<PacketBuffer, T> encoder) where T: struct {
	if (value == null) {
		WriteBool(false);
		return;
	}
	WriteBool(true);
	encoder(this, value.Value);
}
		public void WriteEntityMetadataLoop<T>(T[] values, int endVal, Action<PacketBuffer, T> encoder) { 
    for (byte b = 0; b < values.Length; b++) {
        WriteU8(b);
        encoder(this, values[b]);
    }
    WriteU8(0xFF);
}
		public void WriteTopBitSetTerminatedArray<T>(T[] values, Action<PacketBuffer, T> encoder) { 
    for (int i = 0; i < values.Length; i++) {
        long pos = _buffer.Position;
        encoder(this, values[i]);

        if (i == values.Length - 1) {
            List<byte> buf = new List<byte>();
            var clone = new MemoryStream(_buffer.ToArray());
            
            byte[] b1 = new byte[(int)pos];
            clone.Read(b1, 0, (int)pos);
            buf.AddRange(b1);
            byte b = (byte)clone.ReadByte();
            buf.Add((byte)(b | 128));
            _buffer = new MemoryStream();
            _buffer.Write(buf.ToArray(), 0, buf.Count);
            clone.CopyTo(_buffer);
        }
    }
}
		public void WriteVoid(object? value) { }
		private void EncodeArray<T>(T[] array, Action<PacketBuffer, T> encoder) {
    for (int i = 0; i < array.Length; i++)
        encoder(this, array[i]);
}

public void WriteArray<T>(T[] array, Action<PacketBuffer, T> encoder, Action<PacketBuffer, byte> lengthEncoder) {
    lengthEncoder(this, (byte)array.Length);
    EncodeArray<T>(array, encoder);
}

public void WriteArray<T>(T[] array, Action<PacketBuffer, T> encoder, Action<PacketBuffer, VarInt> lengthEncoder) {
    lengthEncoder(this, (VarInt)array.Length);
    EncodeArray<T>(array, encoder);
}
		public void WriteRestBuffer(byte[] value) {
    WriteRaw(value);
}
		public void WriteNbt(NbtCompound value) {
    if (value == null) {
        WriteU8(0);
        return;
    }

    NbtFile f = new NbtFile(value) { BigEndian = true };
    f.SaveToStream(_buffer, NbtCompression.None);
}
		public void WriteOptionalNbt(NbtCompound? value) {
    if (value == null) {
        WriteU8(0);
        return;
    }

    NbtFile f = new NbtFile(value) { BigEndian = true };
    f.SaveToStream(_buffer, NbtCompression.None);
}

		#endregion
	}
}
namespace YAMNL
{
    public class Slot {
		public class AnonSwitch {
			public class AnonSwitchStatetrueContainer {
				public VarInt ItemId { get; set; }
				public sbyte ItemCount { get; set; }
				public NbtCompound? NbtData { get; set; }
				public AnonSwitchStatetrueContainer(VarInt @itemId, sbyte @itemCount, NbtCompound? @nbtData) {
					ItemId = @itemId;
					ItemCount = @itemCount;
					NbtData = @nbtData;
				}
				public void Write(PacketBuffer buffer ) {
					((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, ItemId);
					((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, ItemCount);
					((Action<PacketBuffer, NbtCompound?>)((buffer, value) => buffer.WriteOptionalNbt(value)))(buffer, NbtData);
				}
				public static AnonSwitchStatetrueContainer Read(PacketBuffer buffer ) {
					VarInt @itemId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
					sbyte @itemCount = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
					NbtCompound? @nbtData = ((Func<PacketBuffer, NbtCompound?>)((buffer) => buffer.ReadOptionalNbt()))(buffer);
					return new AnonSwitchStatetrueContainer(@itemId, @itemCount, @nbtData);
				}
			}
			public object? Value { get; set; }
			public AnonSwitch(object? value) {
				Value = value;
			}
			public void Write(PacketBuffer buffer, bool state) {
				switch (state) {
					case false: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
					case true: ((Action<PacketBuffer, AnonSwitchStatetrueContainer>)((buffer, value) => value.Write(buffer )))(buffer, (AnonSwitchStatetrueContainer)this); break;
					default: throw new Exception($"Invalid value: '{state}'");
				}
			}
			public static AnonSwitch Read(PacketBuffer buffer, bool state) {
				object? value = state switch {
					false => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
					true => ((Func<PacketBuffer, AnonSwitchStatetrueContainer>)((buffer) => Mine.Net.Slot.AnonSwitch.AnonSwitchStatetrueContainer.Read(buffer )))(buffer),
				};
				return new AnonSwitch(value);
			}
			public static implicit operator AnonSwitchStatetrueContainer?(AnonSwitch value) => (AnonSwitchStatetrueContainer?)value.Value;
			public static implicit operator AnonSwitch?(AnonSwitchStatetrueContainer? value) => new AnonSwitch(value);
		}
		public bool Present { get; set; }
		public AnonSwitch Anon { get; set; }
		public Slot(bool @present, AnonSwitch @anon) {
			Present = @present;
			Anon = @anon;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, Present);
			((Action<PacketBuffer, AnonSwitch>)((buffer, value) => value.Write(buffer, Present)))(buffer, Anon);
		}
		public static Slot Read(PacketBuffer buffer ) {
			bool @present = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			AnonSwitch @anon = ((Func<PacketBuffer, AnonSwitch>)((buffer) => AnonSwitch.Read(buffer, @present)))(buffer);
			return new Slot(@present, @anon);
		}
	}
	public class Particle {
		public VarInt ParticleId { get; set; }
		public ParticleDataSwitch Data { get; set; }
		public Particle(VarInt @particleId, ParticleDataSwitch @data) {
			ParticleId = @particleId;
			Data = @data;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, ParticleId);
			((Action<PacketBuffer, ParticleDataSwitch>)((buffer, value) => value.Write(buffer, ParticleId)))(buffer, Data);
		}
		public static Particle Read(PacketBuffer buffer ) {
			VarInt @particleId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			ParticleDataSwitch @data = ((Func<PacketBuffer, ParticleDataSwitch>)((buffer) => ParticleDataSwitch.Read(buffer, @particleId)))(buffer);
			return new Particle(@particleId, @data);
		}
	}
	public class ParticleDataSwitch {
		public class ParticleDataSwitchState2 {
			public VarInt BlockState { get; set; }
			public ParticleDataSwitchState2(VarInt @blockState) {
				BlockState = @blockState;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, BlockState);
			}
			public static ParticleDataSwitchState2 Read(PacketBuffer buffer ) {
				VarInt @blockState = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
				return new ParticleDataSwitchState2(@blockState);
			}
		}
		public class ParticleDataSwitchState3 {
			public VarInt BlockState { get; set; }
			public ParticleDataSwitchState3(VarInt @blockState) {
				BlockState = @blockState;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, BlockState);
			}
			public static ParticleDataSwitchState3 Read(PacketBuffer buffer ) {
				VarInt @blockState = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
				return new ParticleDataSwitchState3(@blockState);
			}
		}
		public class ParticleDataSwitchState14 {
			public float Red { get; set; }
			public float Green { get; set; }
			public float Blue { get; set; }
			public float Scale { get; set; }
			public ParticleDataSwitchState14(float @red, float @green, float @blue, float @scale) {
				Red = @red;
				Green = @green;
				Blue = @blue;
				Scale = @scale;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, Red);
				((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, Green);
				((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, Blue);
				((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, Scale);
			}
			public static ParticleDataSwitchState14 Read(PacketBuffer buffer ) {
				float @red = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
				float @green = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
				float @blue = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
				float @scale = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
				return new ParticleDataSwitchState14(@red, @green, @blue, @scale);
			}
		}
		public class ParticleDataSwitchState15 {
			public float FromRed { get; set; }
			public float FromGreen { get; set; }
			public float FromBlue { get; set; }
			public float Scale { get; set; }
			public float ToRed { get; set; }
			public float ToGreen { get; set; }
			public float ToBlue { get; set; }
			public ParticleDataSwitchState15(float @fromRed, float @fromGreen, float @fromBlue, float @scale, float @toRed, float @toGreen, float @toBlue) {
				FromRed = @fromRed;
				FromGreen = @fromGreen;
				FromBlue = @fromBlue;
				Scale = @scale;
				ToRed = @toRed;
				ToGreen = @toGreen;
				ToBlue = @toBlue;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, FromRed);
				((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, FromGreen);
				((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, FromBlue);
				((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, Scale);
				((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, ToRed);
				((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, ToGreen);
				((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, ToBlue);
			}
			public static ParticleDataSwitchState15 Read(PacketBuffer buffer ) {
				float @fromRed = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
				float @fromGreen = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
				float @fromBlue = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
				float @scale = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
				float @toRed = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
				float @toGreen = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
				float @toBlue = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
				return new ParticleDataSwitchState15(@fromRed, @fromGreen, @fromBlue, @scale, @toRed, @toGreen, @toBlue);
			}
		}
		public class ParticleDataSwitchState24 {
			public VarInt BlockState { get; set; }
			public ParticleDataSwitchState24(VarInt @blockState) {
				BlockState = @blockState;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, BlockState);
			}
			public static ParticleDataSwitchState24 Read(PacketBuffer buffer ) {
				VarInt @blockState = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
				return new ParticleDataSwitchState24(@blockState);
			}
		}
		public class ParticleDataSwitchState35 {
			public Slot Item { get; set; }
			public ParticleDataSwitchState35(Slot @item) {
				Item = @item;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer )))(buffer, Item);
			}
			public static ParticleDataSwitchState35 Read(PacketBuffer buffer ) {
				Slot @item = ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer )))(buffer);
				return new ParticleDataSwitchState35(@item);
			}
		}
		public class ParticleDataSwitchState36 {
			public class DestinationSwitch {
				public object? Value { get; set; }
				public DestinationSwitch(object? value) {
					Value = value;
				}
				public void Write(PacketBuffer buffer, string state) {
					switch (state) {
						case "minecraft:block": ((Action<PacketBuffer, PositionBitfield>)((buffer, value) => ((Action<PacketBuffer, ulong>)((buffer, value) => buffer.WriteU64(value)))(buffer, value.Value)))(buffer, (PositionBitfield)this); break;
						case "minecraft:entity": ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
						default: throw new Exception($"Invalid value: '{state}'");
					}
				}
				public static DestinationSwitch Read(PacketBuffer buffer, string state) {
					object? value = state switch {
						"minecraft:block" => ((Func<PacketBuffer, PositionBitfield>)((buffer) => new PositionBitfield(((Func<PacketBuffer, ulong>)((buffer) => buffer.ReadU64()))(buffer))))(buffer),
						"minecraft:entity" => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
						 _ => throw new Exception($"Invalid value: '{state}'")
					};
					return new DestinationSwitch(value);
				}
				public static implicit operator PositionBitfield?(DestinationSwitch value) => (PositionBitfield?)value.Value;
				public static implicit operator VarInt?(DestinationSwitch value) => (VarInt?)value.Value;
				public static implicit operator DestinationSwitch?(PositionBitfield? value) => new DestinationSwitch(value);
				public static implicit operator DestinationSwitch?(VarInt? value) => new DestinationSwitch(value);
			}
			public PositionBitfield Origin { get; set; }
			public string PositionType { get; set; }
			public DestinationSwitch Destination { get; set; }
			public VarInt Ticks { get; set; }
			public ParticleDataSwitchState36(PositionBitfield @origin, string @positionType, DestinationSwitch @destination, VarInt @ticks) {
				Origin = @origin;
				PositionType = @positionType;
				Destination = @destination;
				Ticks = @ticks;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, PositionBitfield>)((buffer, value) => ((Action<PacketBuffer, ulong>)((buffer, value) => buffer.WriteU64(value)))(buffer, value.Value)))(buffer, Origin);
				((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, PositionType);
				((Action<PacketBuffer, DestinationSwitch>)((buffer, value) => value.Write(buffer, PositionType)))(buffer, Destination);
				((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Ticks);
			}
			public static ParticleDataSwitchState36 Read(PacketBuffer buffer ) {
				PositionBitfield @origin = ((Func<PacketBuffer, PositionBitfield>)((buffer) => new PositionBitfield(((Func<PacketBuffer, ulong>)((buffer) => buffer.ReadU64()))(buffer))))(buffer);
				string @positionType = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
				DestinationSwitch @destination = ((Func<PacketBuffer, DestinationSwitch>)((buffer) => DestinationSwitch.Read(buffer, @positionType)))(buffer);
				VarInt @ticks = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
				return new ParticleDataSwitchState36(@origin, @positionType, @destination, @ticks);
			}
		}
		public object? Value { get; set; }
		public ParticleDataSwitch(object? value) {
			Value = value;
		}
		public void Write(PacketBuffer buffer, int state) {
			switch (state) {
				case 2: ((Action<PacketBuffer, ParticleDataSwitchState2>)((buffer, value) => value.Write(buffer )))(buffer, (ParticleDataSwitchState2)this); break;
				case 3: ((Action<PacketBuffer, ParticleDataSwitchState3>)((buffer, value) => value.Write(buffer )))(buffer, (ParticleDataSwitchState3)this); break;
				case 14: ((Action<PacketBuffer, ParticleDataSwitchState14>)((buffer, value) => value.Write(buffer )))(buffer, (ParticleDataSwitchState14)this); break;
				case 15: ((Action<PacketBuffer, ParticleDataSwitchState15>)((buffer, value) => value.Write(buffer )))(buffer, (ParticleDataSwitchState15)this); break;
				case 24: ((Action<PacketBuffer, ParticleDataSwitchState24>)((buffer, value) => value.Write(buffer )))(buffer, (ParticleDataSwitchState24)this); break;
				case 35: ((Action<PacketBuffer, ParticleDataSwitchState35>)((buffer, value) => value.Write(buffer )))(buffer, (ParticleDataSwitchState35)this); break;
				case 36: ((Action<PacketBuffer, ParticleDataSwitchState36>)((buffer, value) => value.Write(buffer )))(buffer, (ParticleDataSwitchState36)this); break;
				default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
			}
		}
		public static ParticleDataSwitch Read(PacketBuffer buffer, int state) {
			object? value = state switch {
				2 => ((Func<PacketBuffer, ParticleDataSwitchState2>)((buffer) => Mine.Net.ParticleDataSwitch.ParticleDataSwitchState2.Read(buffer )))(buffer),
				3 => ((Func<PacketBuffer, ParticleDataSwitchState3>)((buffer) => Mine.Net.ParticleDataSwitch.ParticleDataSwitchState3.Read(buffer )))(buffer),
				14 => ((Func<PacketBuffer, ParticleDataSwitchState14>)((buffer) => Mine.Net.ParticleDataSwitch.ParticleDataSwitchState14.Read(buffer )))(buffer),
				15 => ((Func<PacketBuffer, ParticleDataSwitchState15>)((buffer) => Mine.Net.ParticleDataSwitch.ParticleDataSwitchState15.Read(buffer )))(buffer),
				24 => ((Func<PacketBuffer, ParticleDataSwitchState24>)((buffer) => Mine.Net.ParticleDataSwitch.ParticleDataSwitchState24.Read(buffer )))(buffer),
				35 => ((Func<PacketBuffer, ParticleDataSwitchState35>)((buffer) => Mine.Net.ParticleDataSwitch.ParticleDataSwitchState35.Read(buffer )))(buffer),
				36 => ((Func<PacketBuffer, ParticleDataSwitchState36>)((buffer) => Mine.Net.ParticleDataSwitch.ParticleDataSwitchState36.Read(buffer )))(buffer),
				_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
			};
			return new ParticleDataSwitch(value);
		}
		public static implicit operator ParticleDataSwitchState2?(ParticleDataSwitch value) => (ParticleDataSwitchState2?)value.Value;
		public static implicit operator ParticleDataSwitchState3?(ParticleDataSwitch value) => (ParticleDataSwitchState3?)value.Value;
		public static implicit operator ParticleDataSwitchState14?(ParticleDataSwitch value) => (ParticleDataSwitchState14?)value.Value;
		public static implicit operator ParticleDataSwitchState15?(ParticleDataSwitch value) => (ParticleDataSwitchState15?)value.Value;
		public static implicit operator ParticleDataSwitchState24?(ParticleDataSwitch value) => (ParticleDataSwitchState24?)value.Value;
		public static implicit operator ParticleDataSwitchState35?(ParticleDataSwitch value) => (ParticleDataSwitchState35?)value.Value;
		public static implicit operator ParticleDataSwitchState36?(ParticleDataSwitch value) => (ParticleDataSwitchState36?)value.Value;
		public static implicit operator ParticleDataSwitch?(ParticleDataSwitchState2? value) => new ParticleDataSwitch(value);
		public static implicit operator ParticleDataSwitch?(ParticleDataSwitchState3? value) => new ParticleDataSwitch(value);
		public static implicit operator ParticleDataSwitch?(ParticleDataSwitchState14? value) => new ParticleDataSwitch(value);
		public static implicit operator ParticleDataSwitch?(ParticleDataSwitchState15? value) => new ParticleDataSwitch(value);
		public static implicit operator ParticleDataSwitch?(ParticleDataSwitchState24? value) => new ParticleDataSwitch(value);
		public static implicit operator ParticleDataSwitch?(ParticleDataSwitchState35? value) => new ParticleDataSwitch(value);
		public static implicit operator ParticleDataSwitch?(ParticleDataSwitchState36? value) => new ParticleDataSwitch(value);
	}
	public class PositionBitfield {
		public ulong Value { get; set; }
		public PositionBitfield(ulong value) {
			Value = value;
		}
		public int X { 
    get { 
        return (int)(((int)Value! >> 38 & (67108863)));
    }
	set { 
        var val = value << 38; 
        var inv = ~val; var x = (int)Value! & (int)inv; 
        Value = (ulong)((uint)x | (uint)val); 
    }
}
		public int Z { 
    get { 
        return (int)(((int)Value! >> 12 & (67108863)));
    }
	set { 
        var val = value << 12; 
        var inv = ~val; var x = (int)Value! & (int)inv; 
        Value = (ulong)((uint)x | (uint)val); 
    }
}
		public short Y { 
    get { 
        return (short)(((short)Value! >> 0 & (4095)));
    }
	set { 
        var val = value << 0; 
        var inv = ~val; var x = (short)Value! & (short)inv; 
        Value = (ulong)((ushort)x | (ushort)val); 
    }
}
	}
	public class PreviousMessagesElement {
		public UUID MessageSender { get; set; }
		public byte[] MessageSignature { get; set; }
		public PreviousMessagesElement(UUID @messageSender, byte[] @messageSignature) {
			MessageSender = @messageSender;
			MessageSignature = @messageSignature;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, UUID>)((buffer, value) => buffer.WriteUUID(value)))(buffer, MessageSender);
			((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, MessageSignature);
		}
		public static PreviousMessagesElement Read(PacketBuffer buffer ) {
			UUID @messageSender = ((Func<PacketBuffer, UUID>)((buffer) => buffer.ReadUUID()))(buffer);
			byte[] @messageSignature = ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8())))))(buffer);
			return new PreviousMessagesElement(@messageSender, @messageSignature);
		}
	}
	public class EntityMetadataItemSwitch {
		public class EntityMetadataItemSwitchState8 {
			public float Pitch { get; set; }
			public float Yaw { get; set; }
			public float Roll { get; set; }
			public EntityMetadataItemSwitchState8(float @pitch, float @yaw, float @roll) {
				Pitch = @pitch;
				Yaw = @yaw;
				Roll = @roll;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, Pitch);
				((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, Yaw);
				((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, Roll);
			}
			public static EntityMetadataItemSwitchState8 Read(PacketBuffer buffer ) {
				float @pitch = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
				float @yaw = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
				float @roll = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
				return new EntityMetadataItemSwitchState8(@pitch, @yaw, @roll);
			}
		}
		public class EntityMetadataItemSwitchState16 {
			public VarInt VillagerType { get; set; }
			public VarInt VillagerProfession { get; set; }
			public VarInt Level { get; set; }
			public EntityMetadataItemSwitchState16(VarInt @villagerType, VarInt @villagerProfession, VarInt @level) {
				VillagerType = @villagerType;
				VillagerProfession = @villagerProfession;
				Level = @level;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, VillagerType);
				((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, VillagerProfession);
				((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Level);
			}
			public static EntityMetadataItemSwitchState16 Read(PacketBuffer buffer ) {
				VarInt @villagerType = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
				VarInt @villagerProfession = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
				VarInt @level = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
				return new EntityMetadataItemSwitchState16(@villagerType, @villagerProfession, @level);
			}
		}
		public object? Value { get; set; }
		public EntityMetadataItemSwitch(object? value) {
			Value = value;
		}
		public void Write(PacketBuffer buffer, VarInt state) {
			switch (state) {
				case 0: ((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, (sbyte)this); break;
				case 1: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
				case 2: ((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, (float)this); break;
				case 3: ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string)this); break;
				case 4: ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string)this); break;
				case 5: ((Action<PacketBuffer, string?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))))))(buffer, (string?)this); break;
				case 6: ((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer )))(buffer, (Slot)this); break;
				case 7: ((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, (bool)this); break;
				case 8: ((Action<PacketBuffer, EntityMetadataItemSwitchState8>)((buffer, value) => value.Write(buffer )))(buffer, (EntityMetadataItemSwitchState8)this); break;
				case 9: ((Action<PacketBuffer, PositionBitfield>)((buffer, value) => ((Action<PacketBuffer, ulong>)((buffer, value) => buffer.WriteU64(value)))(buffer, value.Value)))(buffer, (PositionBitfield)this); break;
				case 10: ((Action<PacketBuffer, PositionBitfield?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, PositionBitfield>)((buffer, value) => ((Action<PacketBuffer, ulong>)((buffer, value) => buffer.WriteU64(value)))(buffer, value.Value))))))(buffer, (PositionBitfield?)this); break;
				case 11: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
				case 12: ((Action<PacketBuffer, UUID?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, UUID>)((buffer, value) => buffer.WriteUUID(value))))))(buffer, (UUID?)this); break;
				case 13: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
				case 14: ((Action<PacketBuffer, NbtCompound>)((buffer, value) => buffer.WriteNbt(value)))(buffer, (NbtCompound)this); break;
				case 15: ((Action<PacketBuffer, Particle>)((buffer, value) => value.Write(buffer )))(buffer, (Particle)this); break;
				case 16: ((Action<PacketBuffer, EntityMetadataItemSwitchState16>)((buffer, value) => value.Write(buffer )))(buffer, (EntityMetadataItemSwitchState16)this); break;
				case 17: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
				case 18: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
				case 19: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
				case 20: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
				case 21: ((Action<PacketBuffer, string?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))))))(buffer, (string?)this); break;
				case 22: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
				default: throw new Exception($"Invalid value: '{state}'");
			}
		}
		public static EntityMetadataItemSwitch Read(PacketBuffer buffer, VarInt state) {
			object? value = state.Value switch {
				0 => ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer),
				1 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
				2 => ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer),
				3 => ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer),
				4 => ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer),
				5 => ((Func<PacketBuffer, string?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer),
				6 => ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer )))(buffer),
				7 => ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer),
				8 => ((Func<PacketBuffer, EntityMetadataItemSwitchState8>)((buffer) => Mine.Net.EntityMetadataItemSwitch.EntityMetadataItemSwitchState8.Read(buffer )))(buffer),
				9 => ((Func<PacketBuffer, PositionBitfield>)((buffer) => new PositionBitfield(((Func<PacketBuffer, ulong>)((buffer) => buffer.ReadU64()))(buffer))))(buffer),
				10 => ((Func<PacketBuffer, PositionBitfield?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, PositionBitfield>)((buffer) => new PositionBitfield(((Func<PacketBuffer, ulong>)((buffer) => buffer.ReadU64()))(buffer)))))))(buffer),
				11 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
				12 => ((Func<PacketBuffer, UUID?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, UUID>)((buffer) => buffer.ReadUUID())))))(buffer),
				13 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
				14 => ((Func<PacketBuffer, NbtCompound>)((buffer) => buffer.ReadNbt()))(buffer),
				15 => ((Func<PacketBuffer, Particle>)((buffer) => Mine.Net.Particle.Read(buffer )))(buffer),
				16 => ((Func<PacketBuffer, EntityMetadataItemSwitchState16>)((buffer) => Mine.Net.EntityMetadataItemSwitch.EntityMetadataItemSwitchState16.Read(buffer )))(buffer),
				17 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
				18 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
				19 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
				20 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
				21 => ((Func<PacketBuffer, string?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer),
				22 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
				 _ => throw new Exception($"Invalid value: '{state}'")
			};
			return new EntityMetadataItemSwitch(value);
		}
		public static implicit operator sbyte?(EntityMetadataItemSwitch value) => (sbyte?)value.Value;
		public static implicit operator VarInt?(EntityMetadataItemSwitch value) => (VarInt?)value.Value;
		public static implicit operator float?(EntityMetadataItemSwitch value) => (float?)value.Value;
		public static implicit operator string?(EntityMetadataItemSwitch value) => (string?)value.Value;
		public static implicit operator Slot?(EntityMetadataItemSwitch value) => (Slot?)value.Value;
		public static implicit operator bool?(EntityMetadataItemSwitch value) => (bool?)value.Value;
		public static implicit operator EntityMetadataItemSwitchState8?(EntityMetadataItemSwitch value) => (EntityMetadataItemSwitchState8?)value.Value;
		public static implicit operator PositionBitfield?(EntityMetadataItemSwitch value) => (PositionBitfield?)value.Value;
		public static implicit operator UUID?(EntityMetadataItemSwitch value) => (UUID?)value.Value;
		public static implicit operator NbtCompound?(EntityMetadataItemSwitch value) => (NbtCompound?)value.Value;
		public static implicit operator Particle?(EntityMetadataItemSwitch value) => (Particle?)value.Value;
		public static implicit operator EntityMetadataItemSwitchState16?(EntityMetadataItemSwitch value) => (EntityMetadataItemSwitchState16?)value.Value;
		public static implicit operator EntityMetadataItemSwitch?(sbyte? value) => new EntityMetadataItemSwitch(value);
		public static implicit operator EntityMetadataItemSwitch?(VarInt? value) => new EntityMetadataItemSwitch(value);
		public static implicit operator EntityMetadataItemSwitch?(float? value) => new EntityMetadataItemSwitch(value);
		public static implicit operator EntityMetadataItemSwitch?(string? value) => new EntityMetadataItemSwitch(value);
		public static implicit operator EntityMetadataItemSwitch?(Slot? value) => new EntityMetadataItemSwitch(value);
		public static implicit operator EntityMetadataItemSwitch?(bool? value) => new EntityMetadataItemSwitch(value);
		public static implicit operator EntityMetadataItemSwitch?(EntityMetadataItemSwitchState8? value) => new EntityMetadataItemSwitch(value);
		public static implicit operator EntityMetadataItemSwitch?(PositionBitfield? value) => new EntityMetadataItemSwitch(value);
		public static implicit operator EntityMetadataItemSwitch?(UUID? value) => new EntityMetadataItemSwitch(value);
		public static implicit operator EntityMetadataItemSwitch?(NbtCompound? value) => new EntityMetadataItemSwitch(value);
		public static implicit operator EntityMetadataItemSwitch?(Particle? value) => new EntityMetadataItemSwitch(value);
		public static implicit operator EntityMetadataItemSwitch?(EntityMetadataItemSwitchState16? value) => new EntityMetadataItemSwitch(value);
	}
	public class EntityMetadataLoopElement {
		public class AnonContainer {
			public byte Key { get; set; }
			public VarInt Type { get; set; }
			public AnonContainer(byte @key, VarInt @type) {
				Key = @key;
				Type = @type;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, Key);
				((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Type);
			}
			public static AnonContainer Read(PacketBuffer buffer ) {
				byte @key = ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer);
				VarInt @type = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
				return new AnonContainer(@key, @type);
			}
		}
		public AnonContainer Anon { get; set; }
		public EntityMetadataItemSwitch Value { get; set; }
		public byte Key { get { return Anon.Key; } set { Anon.Key = value; } }
		public VarInt Type { get { return Anon.Type; } set { Anon.Type = value; } }
		public EntityMetadataLoopElement(AnonContainer @anon, EntityMetadataItemSwitch @value) {
			Anon = @anon;
			Value = @value;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, AnonContainer>)((buffer, value) => value.Write(buffer )))(buffer, Anon);
			((Action<PacketBuffer, EntityMetadataItemSwitch>)((buffer, value) => value.Write(buffer, Type)))(buffer, Value);
		}
		public static EntityMetadataLoopElement Read(PacketBuffer buffer ) {
			AnonContainer @anon = ((Func<PacketBuffer, AnonContainer>)((buffer) => Mine.Net.EntityMetadataLoopElement.AnonContainer.Read(buffer )))(buffer);
			byte @key = @anon.Key;
			VarInt @type = @anon.Type;
			EntityMetadataItemSwitch @value = ((Func<PacketBuffer, EntityMetadataItemSwitch>)((buffer) => EntityMetadataItemSwitch.Read(buffer, @type)))(buffer);
			return new EntityMetadataLoopElement(@anon, @value);
		}
	}
	public class MinecraftSmeltingFormat {
		public string Group { get; set; }
		public Slot[] Ingredient { get; set; }
		public Slot Result { get; set; }
		public float Experience { get; set; }
		public VarInt CookTime { get; set; }
		public MinecraftSmeltingFormat(string @group, Slot[] @ingredient, Slot @result, float @experience, VarInt @cookTime) {
			Group = @group;
			Ingredient = @ingredient;
			Result = @result;
			Experience = @experience;
			CookTime = @cookTime;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Group);
			((Action<PacketBuffer, Slot[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Ingredient);
			((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer )))(buffer, Result);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, Experience);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, CookTime);
		}
		public static MinecraftSmeltingFormat Read(PacketBuffer buffer ) {
			string @group = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			Slot[] @ingredient = ((Func<PacketBuffer, Slot[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer ))))))(buffer);
			Slot @result = ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer )))(buffer);
			float @experience = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			VarInt @cookTime = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new MinecraftSmeltingFormat(@group, @ingredient, @result, @experience, @cookTime);
		}
	}
	public class TagsElement {
		public string TagName { get; set; }
		public VarInt[] Entries { get; set; }
		public TagsElement(string @tagName, VarInt[] @entries) {
			TagName = @tagName;
			Entries = @entries;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, TagName);
			((Action<PacketBuffer, VarInt[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Entries);
		}
		public static TagsElement Read(PacketBuffer buffer ) {
			string @tagName = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			VarInt[] @entries = ((Func<PacketBuffer, VarInt[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			return new TagsElement(@tagName, @entries);
		}
	}
	public class ChunkBlockEntity {
		public class AnonBitfield {
			public byte Value { get; set; }
			public AnonBitfield(byte value) {
				Value = value;
			}
			public byte X { 
    get { 
        return (byte)(((byte)Value! >> 4 & (15)));
    }
	set { 
        var val = value << 4; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
			public byte Z { 
    get { 
        return (byte)(((byte)Value! >> 0 & (15)));
    }
	set { 
        var val = value << 0; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
		}
		public AnonBitfield Anon { get; set; }
		public short Y { get; set; }
		public VarInt Type { get; set; }
		public NbtCompound? NbtData { get; set; }
		public byte X { get { return Anon.X; } set { Anon.X = value; } }
		public byte Z { get { return Anon.Z; } set { Anon.Z = value; } }
		public ChunkBlockEntity(AnonBitfield @anon, short @y, VarInt @type, NbtCompound? @nbtData) {
			Anon = @anon;
			Y = @y;
			Type = @type;
			NbtData = @nbtData;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, AnonBitfield>)((buffer, value) => ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, value.Value)))(buffer, Anon);
			((Action<PacketBuffer, short>)((buffer, value) => buffer.WriteI16(value)))(buffer, Y);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Type);
			((Action<PacketBuffer, NbtCompound?>)((buffer, value) => buffer.WriteOptionalNbt(value)))(buffer, NbtData);
		}
		public static ChunkBlockEntity Read(PacketBuffer buffer ) {
			AnonBitfield @anon = ((Func<PacketBuffer, AnonBitfield>)((buffer) => new AnonBitfield(((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer))))(buffer);
			byte @x = @anon.X;
			byte @z = @anon.Z;
			short @y = ((Func<PacketBuffer, short>)((buffer) => buffer.ReadI16()))(buffer);
			VarInt @type = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			NbtCompound? @nbtData = ((Func<PacketBuffer, NbtCompound?>)((buffer) => buffer.ReadOptionalNbt()))(buffer);
			return new ChunkBlockEntity(@anon, @y, @type, @nbtData);
		}
	}
	public class CommandNode {
		public class FlagsBitfield {
			public byte Value { get; set; }
			public FlagsBitfield(byte value) {
				Value = value;
			}
			public byte Unused { 
    get { 
        return (byte)(((byte)Value! >> 5 & (7)));
    }
	set { 
        var val = value << 5; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
			public byte HasCustomSuggestions { 
    get { 
        return (byte)(((byte)Value! >> 4 & (1)));
    }
	set { 
        var val = value << 4; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
			public byte HasRedirectNode { 
    get { 
        return (byte)(((byte)Value! >> 3 & (1)));
    }
	set { 
        var val = value << 3; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
			public byte HasCommand { 
    get { 
        return (byte)(((byte)Value! >> 2 & (1)));
    }
	set { 
        var val = value << 2; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
			public byte CommandNodeType { 
    get { 
        return (byte)(((byte)Value! >> 0 & (3)));
    }
	set { 
        var val = value << 0; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
		}
		public class RedirectNodeSwitch {
			public object? Value { get; set; }
			public RedirectNodeSwitch(object? value) {
				Value = value;
			}
			public void Write(PacketBuffer buffer, byte state) {
				switch (state) {
					case 1: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static RedirectNodeSwitch Read(PacketBuffer buffer, byte state) {
				object? value = state switch {
					1 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new RedirectNodeSwitch(value);
			}
			public static implicit operator VarInt?(RedirectNodeSwitch value) => (VarInt?)value.Value;
			public static implicit operator RedirectNodeSwitch?(VarInt? value) => new RedirectNodeSwitch(value);
		}
		public class ExtraNodeDataSwitch {
			public class ExtraNodeDataSwitchState1Container {
				public string Name { get; set; }
				public ExtraNodeDataSwitchState1Container(string @name) {
					Name = @name;
				}
				public void Write(PacketBuffer buffer ) {
					((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Name);
				}
				public static ExtraNodeDataSwitchState1Container Read(PacketBuffer buffer ) {
					string @name = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
					return new ExtraNodeDataSwitchState1Container(@name);
				}
			}
			public class ExtraNodeDataSwitchState2Container {
				public class PropertiesSwitch {
					public class PropertiesSwitchStatebrigadierFloatContainer {
						public class FlagsBitfield {
							public byte Value { get; set; }
							public FlagsBitfield(byte value) {
								Value = value;
							}
							public byte Unused { 
    get { 
        return (byte)(((byte)Value! >> 2 & (63)));
    }
	set { 
        var val = value << 2; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
							public byte MaxPresent { 
    get { 
        return (byte)(((byte)Value! >> 1 & (1)));
    }
	set { 
        var val = value << 1; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
							public byte MinPresent { 
    get { 
        return (byte)(((byte)Value! >> 0 & (1)));
    }
	set { 
        var val = value << 0; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
						}
						public class MinSwitch {
							public object? Value { get; set; }
							public MinSwitch(object? value) {
								Value = value;
							}
							public void Write(PacketBuffer buffer, byte state) {
								switch (state) {
									case 1: ((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, (float)this); break;
									default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
								}
							}
							public static MinSwitch Read(PacketBuffer buffer, byte state) {
								object? value = state switch {
									1 => ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer),
									_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
								};
								return new MinSwitch(value);
							}
							public static implicit operator float?(MinSwitch value) => (float?)value.Value;
							public static implicit operator MinSwitch?(float? value) => new MinSwitch(value);
						}
						public class MaxSwitch {
							public object? Value { get; set; }
							public MaxSwitch(object? value) {
								Value = value;
							}
							public void Write(PacketBuffer buffer, byte state) {
								switch (state) {
									case 1: ((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, (float)this); break;
									default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
								}
							}
							public static MaxSwitch Read(PacketBuffer buffer, byte state) {
								object? value = state switch {
									1 => ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer),
									_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
								};
								return new MaxSwitch(value);
							}
							public static implicit operator float?(MaxSwitch value) => (float?)value.Value;
							public static implicit operator MaxSwitch?(float? value) => new MaxSwitch(value);
						}
						public FlagsBitfield Flags { get; set; }
						public MinSwitch Min { get; set; }
						public MaxSwitch Max { get; set; }
						public PropertiesSwitchStatebrigadierFloatContainer(FlagsBitfield @flags, MinSwitch @min, MaxSwitch @max) {
							Flags = @flags;
							Min = @min;
							Max = @max;
						}
						public void Write(PacketBuffer buffer ) {
							((Action<PacketBuffer, FlagsBitfield>)((buffer, value) => ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, value.Value)))(buffer, Flags);
							((Action<PacketBuffer, MinSwitch>)((buffer, value) => value.Write(buffer, Flags.MinPresent)))(buffer, Min);
							((Action<PacketBuffer, MaxSwitch>)((buffer, value) => value.Write(buffer, Flags.MaxPresent)))(buffer, Max);
						}
						public static PropertiesSwitchStatebrigadierFloatContainer Read(PacketBuffer buffer ) {
							FlagsBitfield @flags = ((Func<PacketBuffer, FlagsBitfield>)((buffer) => new FlagsBitfield(((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer))))(buffer);
							MinSwitch @min = ((Func<PacketBuffer, MinSwitch>)((buffer) => MinSwitch.Read(buffer, @flags.MinPresent)))(buffer);
							MaxSwitch @max = ((Func<PacketBuffer, MaxSwitch>)((buffer) => MaxSwitch.Read(buffer, @flags.MaxPresent)))(buffer);
							return new PropertiesSwitchStatebrigadierFloatContainer(@flags, @min, @max);
						}
					}
					public class PropertiesSwitchStatebrigadierDoubleContainer {
						public class FlagsBitfield {
							public byte Value { get; set; }
							public FlagsBitfield(byte value) {
								Value = value;
							}
							public byte Unused { 
    get { 
        return (byte)(((byte)Value! >> 2 & (63)));
    }
	set { 
        var val = value << 2; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
							public byte MaxPresent { 
    get { 
        return (byte)(((byte)Value! >> 1 & (1)));
    }
	set { 
        var val = value << 1; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
							public byte MinPresent { 
    get { 
        return (byte)(((byte)Value! >> 0 & (1)));
    }
	set { 
        var val = value << 0; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
						}
						public class MinSwitch {
							public object? Value { get; set; }
							public MinSwitch(object? value) {
								Value = value;
							}
							public void Write(PacketBuffer buffer, byte state) {
								switch (state) {
									case 1: ((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, (double)this); break;
									default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
								}
							}
							public static MinSwitch Read(PacketBuffer buffer, byte state) {
								object? value = state switch {
									1 => ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer),
									_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
								};
								return new MinSwitch(value);
							}
							public static implicit operator double?(MinSwitch value) => (double?)value.Value;
							public static implicit operator MinSwitch?(double? value) => new MinSwitch(value);
						}
						public class MaxSwitch {
							public object? Value { get; set; }
							public MaxSwitch(object? value) {
								Value = value;
							}
							public void Write(PacketBuffer buffer, byte state) {
								switch (state) {
									case 1: ((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, (double)this); break;
									default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
								}
							}
							public static MaxSwitch Read(PacketBuffer buffer, byte state) {
								object? value = state switch {
									1 => ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer),
									_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
								};
								return new MaxSwitch(value);
							}
							public static implicit operator double?(MaxSwitch value) => (double?)value.Value;
							public static implicit operator MaxSwitch?(double? value) => new MaxSwitch(value);
						}
						public FlagsBitfield Flags { get; set; }
						public MinSwitch Min { get; set; }
						public MaxSwitch Max { get; set; }
						public PropertiesSwitchStatebrigadierDoubleContainer(FlagsBitfield @flags, MinSwitch @min, MaxSwitch @max) {
							Flags = @flags;
							Min = @min;
							Max = @max;
						}
						public void Write(PacketBuffer buffer ) {
							((Action<PacketBuffer, FlagsBitfield>)((buffer, value) => ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, value.Value)))(buffer, Flags);
							((Action<PacketBuffer, MinSwitch>)((buffer, value) => value.Write(buffer, Flags.MinPresent)))(buffer, Min);
							((Action<PacketBuffer, MaxSwitch>)((buffer, value) => value.Write(buffer, Flags.MaxPresent)))(buffer, Max);
						}
						public static PropertiesSwitchStatebrigadierDoubleContainer Read(PacketBuffer buffer ) {
							FlagsBitfield @flags = ((Func<PacketBuffer, FlagsBitfield>)((buffer) => new FlagsBitfield(((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer))))(buffer);
							MinSwitch @min = ((Func<PacketBuffer, MinSwitch>)((buffer) => MinSwitch.Read(buffer, @flags.MinPresent)))(buffer);
							MaxSwitch @max = ((Func<PacketBuffer, MaxSwitch>)((buffer) => MaxSwitch.Read(buffer, @flags.MaxPresent)))(buffer);
							return new PropertiesSwitchStatebrigadierDoubleContainer(@flags, @min, @max);
						}
					}
					public class PropertiesSwitchStatebrigadierIntegerContainer {
						public class FlagsBitfield {
							public byte Value { get; set; }
							public FlagsBitfield(byte value) {
								Value = value;
							}
							public byte Unused { 
    get { 
        return (byte)(((byte)Value! >> 2 & (63)));
    }
	set { 
        var val = value << 2; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
							public byte MaxPresent { 
    get { 
        return (byte)(((byte)Value! >> 1 & (1)));
    }
	set { 
        var val = value << 1; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
							public byte MinPresent { 
    get { 
        return (byte)(((byte)Value! >> 0 & (1)));
    }
	set { 
        var val = value << 0; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
						}
						public class MinSwitch {
							public object? Value { get; set; }
							public MinSwitch(object? value) {
								Value = value;
							}
							public void Write(PacketBuffer buffer, byte state) {
								switch (state) {
									case 1: ((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, (int)this); break;
									default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
								}
							}
							public static MinSwitch Read(PacketBuffer buffer, byte state) {
								object? value = state switch {
									1 => ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer),
									_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
								};
								return new MinSwitch(value);
							}
							public static implicit operator int?(MinSwitch value) => (int?)value.Value;
							public static implicit operator MinSwitch?(int? value) => new MinSwitch(value);
						}
						public class MaxSwitch {
							public object? Value { get; set; }
							public MaxSwitch(object? value) {
								Value = value;
							}
							public void Write(PacketBuffer buffer, byte state) {
								switch (state) {
									case 1: ((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, (int)this); break;
									default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
								}
							}
							public static MaxSwitch Read(PacketBuffer buffer, byte state) {
								object? value = state switch {
									1 => ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer),
									_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
								};
								return new MaxSwitch(value);
							}
							public static implicit operator int?(MaxSwitch value) => (int?)value.Value;
							public static implicit operator MaxSwitch?(int? value) => new MaxSwitch(value);
						}
						public FlagsBitfield Flags { get; set; }
						public MinSwitch Min { get; set; }
						public MaxSwitch Max { get; set; }
						public PropertiesSwitchStatebrigadierIntegerContainer(FlagsBitfield @flags, MinSwitch @min, MaxSwitch @max) {
							Flags = @flags;
							Min = @min;
							Max = @max;
						}
						public void Write(PacketBuffer buffer ) {
							((Action<PacketBuffer, FlagsBitfield>)((buffer, value) => ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, value.Value)))(buffer, Flags);
							((Action<PacketBuffer, MinSwitch>)((buffer, value) => value.Write(buffer, Flags.MinPresent)))(buffer, Min);
							((Action<PacketBuffer, MaxSwitch>)((buffer, value) => value.Write(buffer, Flags.MaxPresent)))(buffer, Max);
						}
						public static PropertiesSwitchStatebrigadierIntegerContainer Read(PacketBuffer buffer ) {
							FlagsBitfield @flags = ((Func<PacketBuffer, FlagsBitfield>)((buffer) => new FlagsBitfield(((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer))))(buffer);
							MinSwitch @min = ((Func<PacketBuffer, MinSwitch>)((buffer) => MinSwitch.Read(buffer, @flags.MinPresent)))(buffer);
							MaxSwitch @max = ((Func<PacketBuffer, MaxSwitch>)((buffer) => MaxSwitch.Read(buffer, @flags.MaxPresent)))(buffer);
							return new PropertiesSwitchStatebrigadierIntegerContainer(@flags, @min, @max);
						}
					}
					public class PropertiesSwitchStatebrigadierLongContainer {
						public class FlagsBitfield {
							public byte Value { get; set; }
							public FlagsBitfield(byte value) {
								Value = value;
							}
							public byte Unused { 
    get { 
        return (byte)(((byte)Value! >> 2 & (63)));
    }
	set { 
        var val = value << 2; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
							public byte MaxPresent { 
    get { 
        return (byte)(((byte)Value! >> 1 & (1)));
    }
	set { 
        var val = value << 1; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
							public byte MinPresent { 
    get { 
        return (byte)(((byte)Value! >> 0 & (1)));
    }
	set { 
        var val = value << 0; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
						}
						public class MinSwitch {
							public object? Value { get; set; }
							public MinSwitch(object? value) {
								Value = value;
							}
							public void Write(PacketBuffer buffer, byte state) {
								switch (state) {
									case 1: ((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value)))(buffer, (long)this); break;
									default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
								}
							}
							public static MinSwitch Read(PacketBuffer buffer, byte state) {
								object? value = state switch {
									1 => ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64()))(buffer),
									_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
								};
								return new MinSwitch(value);
							}
							public static implicit operator long?(MinSwitch value) => (long?)value.Value;
							public static implicit operator MinSwitch?(long? value) => new MinSwitch(value);
						}
						public class MaxSwitch {
							public object? Value { get; set; }
							public MaxSwitch(object? value) {
								Value = value;
							}
							public void Write(PacketBuffer buffer, byte state) {
								switch (state) {
									case 1: ((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value)))(buffer, (long)this); break;
									default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
								}
							}
							public static MaxSwitch Read(PacketBuffer buffer, byte state) {
								object? value = state switch {
									1 => ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64()))(buffer),
									_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
								};
								return new MaxSwitch(value);
							}
							public static implicit operator long?(MaxSwitch value) => (long?)value.Value;
							public static implicit operator MaxSwitch?(long? value) => new MaxSwitch(value);
						}
						public FlagsBitfield Flags { get; set; }
						public MinSwitch Min { get; set; }
						public MaxSwitch Max { get; set; }
						public PropertiesSwitchStatebrigadierLongContainer(FlagsBitfield @flags, MinSwitch @min, MaxSwitch @max) {
							Flags = @flags;
							Min = @min;
							Max = @max;
						}
						public void Write(PacketBuffer buffer ) {
							((Action<PacketBuffer, FlagsBitfield>)((buffer, value) => ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, value.Value)))(buffer, Flags);
							((Action<PacketBuffer, MinSwitch>)((buffer, value) => value.Write(buffer, Flags.MinPresent)))(buffer, Min);
							((Action<PacketBuffer, MaxSwitch>)((buffer, value) => value.Write(buffer, Flags.MaxPresent)))(buffer, Max);
						}
						public static PropertiesSwitchStatebrigadierLongContainer Read(PacketBuffer buffer ) {
							FlagsBitfield @flags = ((Func<PacketBuffer, FlagsBitfield>)((buffer) => new FlagsBitfield(((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer))))(buffer);
							MinSwitch @min = ((Func<PacketBuffer, MinSwitch>)((buffer) => MinSwitch.Read(buffer, @flags.MinPresent)))(buffer);
							MaxSwitch @max = ((Func<PacketBuffer, MaxSwitch>)((buffer) => MaxSwitch.Read(buffer, @flags.MaxPresent)))(buffer);
							return new PropertiesSwitchStatebrigadierLongContainer(@flags, @min, @max);
						}
					}
					public class PropertiesSwitchStateminecraftEntityBitfield {
						public byte Value { get; set; }
						public PropertiesSwitchStateminecraftEntityBitfield(byte value) {
							Value = value;
						}
						public byte Unused { 
    get { 
        return (byte)(((byte)Value! >> 2 & (63)));
    }
	set { 
        var val = value << 2; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
						public byte OnlyAllowPlayers { 
    get { 
        return (byte)(((byte)Value! >> 1 & (1)));
    }
	set { 
        var val = value << 1; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
						public byte OnlyAllowEntities { 
    get { 
        return (byte)(((byte)Value! >> 0 & (1)));
    }
	set { 
        var val = value << 0; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
					}
					public class PropertiesswitchstateminecraftScoreHolderBitfield {
						public byte Value { get; set; }
						public PropertiesswitchstateminecraftScoreHolderBitfield(byte value) {
							Value = value;
						}
						public byte Unused { 
    get { 
        return (byte)(((byte)Value! >> 1 & (127)));
    }
	set { 
        var val = value << 1; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
						public byte AllowMultiple { 
    get { 
        return (byte)(((byte)Value! >> 0 & (1)));
    }
	set { 
        var val = value << 0; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (byte)((byte)x | (byte)val); 
    }
}
					}
					public class PropertiesswitchstateminecraftResourceOrTagContainer {
						public string Registry { get; set; }
						public PropertiesswitchstateminecraftResourceOrTagContainer(string @registry) {
							Registry = @registry;
						}
						public void Write(PacketBuffer buffer ) {
							((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Registry);
						}
						public static PropertiesswitchstateminecraftResourceOrTagContainer Read(PacketBuffer buffer ) {
							string @registry = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
							return new PropertiesswitchstateminecraftResourceOrTagContainer(@registry);
						}
					}
					public class PropertiesSwitchStateminecraftResourceContainer {
						public string Registry { get; set; }
						public PropertiesSwitchStateminecraftResourceContainer(string @registry) {
							Registry = @registry;
						}
						public void Write(PacketBuffer buffer ) {
							((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Registry);
						}
						public static PropertiesSwitchStateminecraftResourceContainer Read(PacketBuffer buffer ) {
							string @registry = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
							return new PropertiesSwitchStateminecraftResourceContainer(@registry);
						}
					}
					public object? Value { get; set; }
					public PropertiesSwitch(object? value) {
						Value = value;
					}
					public void Write(PacketBuffer buffer, string state) {
						switch (state) {
							case "brigadier:bool": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "brigadier:float": ((Action<PacketBuffer, PropertiesSwitchStatebrigadierFloatContainer>)((buffer, value) => value.Write(buffer )))(buffer, (PropertiesSwitchStatebrigadierFloatContainer)this); break;
							case "brigadier:double": ((Action<PacketBuffer, PropertiesSwitchStatebrigadierDoubleContainer>)((buffer, value) => value.Write(buffer )))(buffer, (PropertiesSwitchStatebrigadierDoubleContainer)this); break;
							case "brigadier:integer": ((Action<PacketBuffer, PropertiesSwitchStatebrigadierIntegerContainer>)((buffer, value) => value.Write(buffer )))(buffer, (PropertiesSwitchStatebrigadierIntegerContainer)this); break;
							case "brigadier:long": ((Action<PacketBuffer, PropertiesSwitchStatebrigadierLongContainer>)((buffer, value) => value.Write(buffer )))(buffer, (PropertiesSwitchStatebrigadierLongContainer)this); break;
							case "brigadier:string": ((Action<PacketBuffer, string>)((buffer, value) => ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, value switch { "SINGLE_WORD" => 0, "QUOTABLE_PHRASE" => 1, "GREEDY_PHRASE" => 2, _ => throw new Exception($"Value '{value}' not supported.") })))(buffer, (string)this); break;
							case "minecraft:entity": ((Action<PacketBuffer, PropertiesSwitchStateminecraftEntityBitfield>)((buffer, value) => ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, value.Value)))(buffer, (PropertiesSwitchStateminecraftEntityBitfield)this); break;
							case "minecraft:game_profile": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:block_pos": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:column_pos": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:vec3": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:vec2": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:block_state": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:block_predicate": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:item_stack": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:item_predicate": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:color": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:component": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:message": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:nbt": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:nbt_path": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:objective": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:objective_criteria": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:operation": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:particle": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:angle": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:rotation": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:scoreboard_slot": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:score_holder": ((Action<PacketBuffer, PropertiesswitchstateminecraftScoreHolderBitfield>)((buffer, value) => ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, value.Value)))(buffer, (PropertiesswitchstateminecraftScoreHolderBitfield)this); break;
							case "minecraft:swizzle": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:team": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:item_slot": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:resource_location": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:mob_effect": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:function": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:entity_anchor": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:int_range": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:float_range": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:item_enchantment": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:entity_summon": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:dimension": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:time": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:resource_or_tag": ((Action<PacketBuffer, PropertiesswitchstateminecraftResourceOrTagContainer>)((buffer, value) => value.Write(buffer )))(buffer, (PropertiesswitchstateminecraftResourceOrTagContainer)this); break;
							case "minecraft:resource": ((Action<PacketBuffer, PropertiesSwitchStateminecraftResourceContainer>)((buffer, value) => value.Write(buffer )))(buffer, (PropertiesSwitchStateminecraftResourceContainer)this); break;
							case "template_mirror": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "template_rotation": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							case "minecraft:uuid": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
							default: throw new Exception($"Invalid value: '{state}'");
						}
					}
					public static PropertiesSwitch Read(PacketBuffer buffer, string state) {
						object? value = state switch {
							"brigadier:bool" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"brigadier:float" => ((Func<PacketBuffer, PropertiesSwitchStatebrigadierFloatContainer>)((buffer) => Mine.Net.CommandNode.ExtraNodeDataSwitch.ExtraNodeDataSwitchState2Container.PropertiesSwitch.PropertiesSwitchStatebrigadierFloatContainer.Read(buffer )))(buffer),
							"brigadier:double" => ((Func<PacketBuffer, PropertiesSwitchStatebrigadierDoubleContainer>)((buffer) => Mine.Net.CommandNode.ExtraNodeDataSwitch.ExtraNodeDataSwitchState2Container.PropertiesSwitch.PropertiesSwitchStatebrigadierDoubleContainer.Read(buffer )))(buffer),
							"brigadier:integer" => ((Func<PacketBuffer, PropertiesSwitchStatebrigadierIntegerContainer>)((buffer) => Mine.Net.CommandNode.ExtraNodeDataSwitch.ExtraNodeDataSwitchState2Container.PropertiesSwitch.PropertiesSwitchStatebrigadierIntegerContainer.Read(buffer )))(buffer),
							"brigadier:long" => ((Func<PacketBuffer, PropertiesSwitchStatebrigadierLongContainer>)((buffer) => Mine.Net.CommandNode.ExtraNodeDataSwitch.ExtraNodeDataSwitchState2Container.PropertiesSwitch.PropertiesSwitchStatebrigadierLongContainer.Read(buffer )))(buffer),
							"brigadier:string" => ((Func<PacketBuffer, string>)((buffer) => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer).Value switch { 0 => "SINGLE_WORD", 1 => "QUOTABLE_PHRASE", 2 => "GREEDY_PHRASE", _ => throw new Exception() }))(buffer),
							"minecraft:entity" => ((Func<PacketBuffer, PropertiesSwitchStateminecraftEntityBitfield>)((buffer) => new PropertiesSwitchStateminecraftEntityBitfield(((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer))))(buffer),
							"minecraft:game_profile" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:block_pos" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:column_pos" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:vec3" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:vec2" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:block_state" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:block_predicate" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:item_stack" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:item_predicate" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:color" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:component" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:message" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:nbt" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:nbt_path" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:objective" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:objective_criteria" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:operation" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:particle" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:angle" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:rotation" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:scoreboard_slot" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:score_holder" => ((Func<PacketBuffer, PropertiesswitchstateminecraftScoreHolderBitfield>)((buffer) => new PropertiesswitchstateminecraftScoreHolderBitfield(((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer))))(buffer),
							"minecraft:swizzle" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:team" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:item_slot" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:resource_location" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:mob_effect" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:function" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:entity_anchor" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:int_range" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:float_range" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:item_enchantment" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:entity_summon" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:dimension" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:time" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:resource_or_tag" => ((Func<PacketBuffer, PropertiesswitchstateminecraftResourceOrTagContainer>)((buffer) => Mine.Net.CommandNode.ExtraNodeDataSwitch.ExtraNodeDataSwitchState2Container.PropertiesSwitch.PropertiesswitchstateminecraftResourceOrTagContainer.Read(buffer )))(buffer),
							"minecraft:resource" => ((Func<PacketBuffer, PropertiesSwitchStateminecraftResourceContainer>)((buffer) => Mine.Net.CommandNode.ExtraNodeDataSwitch.ExtraNodeDataSwitchState2Container.PropertiesSwitch.PropertiesSwitchStateminecraftResourceContainer.Read(buffer )))(buffer),
							"template_mirror" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"template_rotation" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							"minecraft:uuid" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
							 _ => throw new Exception($"Invalid value: '{state}'")
						};
						return new PropertiesSwitch(value);
					}
					public static implicit operator PropertiesSwitchStatebrigadierFloatContainer?(PropertiesSwitch value) => (PropertiesSwitchStatebrigadierFloatContainer?)value.Value;
					public static implicit operator PropertiesSwitchStatebrigadierDoubleContainer?(PropertiesSwitch value) => (PropertiesSwitchStatebrigadierDoubleContainer?)value.Value;
					public static implicit operator PropertiesSwitchStatebrigadierIntegerContainer?(PropertiesSwitch value) => (PropertiesSwitchStatebrigadierIntegerContainer?)value.Value;
					public static implicit operator PropertiesSwitchStatebrigadierLongContainer?(PropertiesSwitch value) => (PropertiesSwitchStatebrigadierLongContainer?)value.Value;
					public static implicit operator string?(PropertiesSwitch value) => (string?)value.Value;
					public static implicit operator PropertiesSwitchStateminecraftEntityBitfield?(PropertiesSwitch value) => (PropertiesSwitchStateminecraftEntityBitfield?)value.Value;
					public static implicit operator PropertiesswitchstateminecraftScoreHolderBitfield?(PropertiesSwitch value) => (PropertiesswitchstateminecraftScoreHolderBitfield?)value.Value;
					public static implicit operator PropertiesswitchstateminecraftResourceOrTagContainer?(PropertiesSwitch value) => (PropertiesswitchstateminecraftResourceOrTagContainer?)value.Value;
					public static implicit operator PropertiesSwitchStateminecraftResourceContainer?(PropertiesSwitch value) => (PropertiesSwitchStateminecraftResourceContainer?)value.Value;
					public static implicit operator PropertiesSwitch?(PropertiesSwitchStatebrigadierFloatContainer? value) => new PropertiesSwitch(value);
					public static implicit operator PropertiesSwitch?(PropertiesSwitchStatebrigadierDoubleContainer? value) => new PropertiesSwitch(value);
					public static implicit operator PropertiesSwitch?(PropertiesSwitchStatebrigadierIntegerContainer? value) => new PropertiesSwitch(value);
					public static implicit operator PropertiesSwitch?(PropertiesSwitchStatebrigadierLongContainer? value) => new PropertiesSwitch(value);
					public static implicit operator PropertiesSwitch?(string? value) => new PropertiesSwitch(value);
					public static implicit operator PropertiesSwitch?(PropertiesSwitchStateminecraftEntityBitfield? value) => new PropertiesSwitch(value);
					public static implicit operator PropertiesSwitch?(PropertiesswitchstateminecraftScoreHolderBitfield? value) => new PropertiesSwitch(value);
					public static implicit operator PropertiesSwitch?(PropertiesswitchstateminecraftResourceOrTagContainer? value) => new PropertiesSwitch(value);
					public static implicit operator PropertiesSwitch?(PropertiesSwitchStateminecraftResourceContainer? value) => new PropertiesSwitch(value);
				}
				public class SuggestionTypeSwitch {
					public object? Value { get; set; }
					public SuggestionTypeSwitch(object? value) {
						Value = value;
					}
					public void Write(PacketBuffer buffer, byte state, FlagsBitfield @flags) {
						switch (state) {
							case 1: ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string)this); break;
							default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
						}
					}
					public static SuggestionTypeSwitch Read(PacketBuffer buffer, byte state, FlagsBitfield @flags) {
						object? value = state switch {
							1 => ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer),
							_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
						};
						return new SuggestionTypeSwitch(value);
					}
					public static implicit operator string?(SuggestionTypeSwitch value) => (string?)value.Value;
					public static implicit operator SuggestionTypeSwitch?(string? value) => new SuggestionTypeSwitch(value);
				}
				public string Name { get; set; }
				public string Parser { get; set; }
				public PropertiesSwitch Properties { get; set; }
				public SuggestionTypeSwitch SuggestionType { get; set; }
				public ExtraNodeDataSwitchState2Container(string @name, string @parser, PropertiesSwitch @properties, SuggestionTypeSwitch @suggestionType) {
					Name = @name;
					Parser = @parser;
					Properties = @properties;
					SuggestionType = @suggestionType;
				}
				public void Write(PacketBuffer buffer , FlagsBitfield @flags) {
					((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Name);
					((Action<PacketBuffer, string>)((buffer, value) => ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, value switch { "brigadier:bool" => 0, "brigadier:float" => 1, "brigadier:double" => 2, "brigadier:integer" => 3, "brigadier:long" => 4, "brigadier:string" => 5, "minecraft:entity" => 6, "minecraft:game_profile" => 7, "minecraft:block_pos" => 8, "minecraft:column_pos" => 9, "minecraft:vec3" => 10, "minecraft:vec2" => 11, "minecraft:block_state" => 12, "minecraft:block_predicate" => 13, "minecraft:item_stack" => 14, "minecraft:item_predicate" => 15, "minecraft:color" => 16, "minecraft:component" => 17, "minecraft:message" => 18, "minecraft:nbt" => 19, "minecraft:nbt_tag" => 20, "minecraft:nbt_path" => 21, "minecraft:objective" => 22, "minecraft:objective_criteria" => 23, "minecraft:operation" => 24, "minecraft:particle" => 25, "minecraft:angle" => 26, "minecraft:rotation" => 27, "minecraft:scoreboard_slot" => 28, "minecraft:score_holder" => 29, "minecraft:swizzle" => 30, "minecraft:team" => 31, "minecraft:item_slot" => 32, "minecraft:resource_location" => 33, "minecraft:mob_effect" => 34, "minecraft:function" => 35, "minecraft:entity_anchor" => 36, "minecraft:int_range" => 37, "minecraft:float_range" => 38, "minecraft:item_enchantment" => 39, "minecraft:entity_summon" => 40, "minecraft:dimension" => 41, "minecraft:time" => 42, "minecraft:resource_or_tag" => 43, "minecraft:resource" => 44, "template_mirror" => 45, "template_rotation" => 46, "minecraft:uuid" => 47, _ => throw new Exception($"Value '{value}' not supported.") })))(buffer, Parser);
					((Action<PacketBuffer, PropertiesSwitch>)((buffer, value) => value.Write(buffer, Parser)))(buffer, Properties);
					((Action<PacketBuffer, SuggestionTypeSwitch>)((buffer, value) => value.Write(buffer, @flags.HasCustomSuggestions, @flags)))(buffer, SuggestionType);
				}
				public static ExtraNodeDataSwitchState2Container Read(PacketBuffer buffer , FlagsBitfield @flags) {
					string @name = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
					string @parser = ((Func<PacketBuffer, string>)((buffer) => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer).Value switch { 0 => "brigadier:bool", 1 => "brigadier:float", 2 => "brigadier:double", 3 => "brigadier:integer", 4 => "brigadier:long", 5 => "brigadier:string", 6 => "minecraft:entity", 7 => "minecraft:game_profile", 8 => "minecraft:block_pos", 9 => "minecraft:column_pos", 10 => "minecraft:vec3", 11 => "minecraft:vec2", 12 => "minecraft:block_state", 13 => "minecraft:block_predicate", 14 => "minecraft:item_stack", 15 => "minecraft:item_predicate", 16 => "minecraft:color", 17 => "minecraft:component", 18 => "minecraft:message", 19 => "minecraft:nbt", 20 => "minecraft:nbt_tag", 21 => "minecraft:nbt_path", 22 => "minecraft:objective", 23 => "minecraft:objective_criteria", 24 => "minecraft:operation", 25 => "minecraft:particle", 26 => "minecraft:angle", 27 => "minecraft:rotation", 28 => "minecraft:scoreboard_slot", 29 => "minecraft:score_holder", 30 => "minecraft:swizzle", 31 => "minecraft:team", 32 => "minecraft:item_slot", 33 => "minecraft:resource_location", 34 => "minecraft:mob_effect", 35 => "minecraft:function", 36 => "minecraft:entity_anchor", 37 => "minecraft:int_range", 38 => "minecraft:float_range", 39 => "minecraft:item_enchantment", 40 => "minecraft:entity_summon", 41 => "minecraft:dimension", 42 => "minecraft:time", 43 => "minecraft:resource_or_tag", 44 => "minecraft:resource", 45 => "template_mirror", 46 => "template_rotation", 47 => "minecraft:uuid", _ => throw new Exception() }))(buffer);
					PropertiesSwitch @properties = ((Func<PacketBuffer, PropertiesSwitch>)((buffer) => PropertiesSwitch.Read(buffer, @parser)))(buffer);
					SuggestionTypeSwitch @suggestionType = ((Func<PacketBuffer, SuggestionTypeSwitch>)((buffer) => SuggestionTypeSwitch.Read(buffer, @flags.HasCustomSuggestions, @flags)))(buffer);
					return new ExtraNodeDataSwitchState2Container(@name, @parser, @properties, @suggestionType);
				}
			}
			public object? Value { get; set; }
			public ExtraNodeDataSwitch(object? value) {
				Value = value;
			}
			public void Write(PacketBuffer buffer, byte state, FlagsBitfield @flags) {
				switch (state) {
					case 0: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
					case 1: ((Action<PacketBuffer, ExtraNodeDataSwitchState1Container>)((buffer, value) => value.Write(buffer )))(buffer, (ExtraNodeDataSwitchState1Container)this); break;
					case 2: ((Action<PacketBuffer, ExtraNodeDataSwitchState2Container>)((buffer, value) => value.Write(buffer , @flags)))(buffer, (ExtraNodeDataSwitchState2Container)this); break;
					default: throw new Exception($"Invalid value: '{state}'");
				}
			}
			public static ExtraNodeDataSwitch Read(PacketBuffer buffer, byte state, FlagsBitfield @flags) {
				object? value = state switch {
					0 => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
					1 => ((Func<PacketBuffer, ExtraNodeDataSwitchState1Container>)((buffer) => Mine.Net.CommandNode.ExtraNodeDataSwitch.ExtraNodeDataSwitchState1Container.Read(buffer )))(buffer),
					2 => ((Func<PacketBuffer, ExtraNodeDataSwitchState2Container>)((buffer) => Mine.Net.CommandNode.ExtraNodeDataSwitch.ExtraNodeDataSwitchState2Container.Read(buffer , @flags)))(buffer),
					 _ => throw new Exception($"Invalid value: '{state}'")
				};
				return new ExtraNodeDataSwitch(value);
			}
			public static implicit operator ExtraNodeDataSwitchState1Container?(ExtraNodeDataSwitch value) => (ExtraNodeDataSwitchState1Container?)value.Value;
			public static implicit operator ExtraNodeDataSwitchState2Container?(ExtraNodeDataSwitch value) => (ExtraNodeDataSwitchState2Container?)value.Value;
			public static implicit operator ExtraNodeDataSwitch?(ExtraNodeDataSwitchState1Container? value) => new ExtraNodeDataSwitch(value);
			public static implicit operator ExtraNodeDataSwitch?(ExtraNodeDataSwitchState2Container? value) => new ExtraNodeDataSwitch(value);
		}
		public FlagsBitfield Flags { get; set; }
		public VarInt[] Children { get; set; }
		public RedirectNodeSwitch RedirectNode { get; set; }
		public ExtraNodeDataSwitch ExtraNodeData { get; set; }
		public CommandNode(FlagsBitfield @flags, VarInt[] @children, RedirectNodeSwitch @redirectNode, ExtraNodeDataSwitch @extraNodeData) {
			Flags = @flags;
			Children = @children;
			RedirectNode = @redirectNode;
			ExtraNodeData = @extraNodeData;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, FlagsBitfield>)((buffer, value) => ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, value.Value)))(buffer, Flags);
			((Action<PacketBuffer, VarInt[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Children);
			((Action<PacketBuffer, RedirectNodeSwitch>)((buffer, value) => value.Write(buffer, Flags.HasRedirectNode)))(buffer, RedirectNode);
			((Action<PacketBuffer, ExtraNodeDataSwitch>)((buffer, value) => value.Write(buffer, Flags.CommandNodeType, Flags)))(buffer, ExtraNodeData);
		}
		public static CommandNode Read(PacketBuffer buffer ) {
			FlagsBitfield @flags = ((Func<PacketBuffer, FlagsBitfield>)((buffer) => new FlagsBitfield(((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer))))(buffer);
			VarInt[] @children = ((Func<PacketBuffer, VarInt[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			RedirectNodeSwitch @redirectNode = ((Func<PacketBuffer, RedirectNodeSwitch>)((buffer) => RedirectNodeSwitch.Read(buffer, @flags.HasRedirectNode)))(buffer);
			ExtraNodeDataSwitch @extraNodeData = ((Func<PacketBuffer, ExtraNodeDataSwitch>)((buffer) => ExtraNodeDataSwitch.Read(buffer, @flags.CommandNodeType, @flags)))(buffer);
			return new CommandNode(@flags, @children, @redirectNode, @extraNodeData);
		}
	}
}
namespace YAMNL
{
    public partial class PacketBuffer {
		#region Reading


		#endregion
		#region Writing


		#endregion
	}
}
namespace YAMNL.Play
{
}
namespace YAMNL
{
    public partial class PacketBuffer {
		#region Reading


		#endregion
		#region Writing


		#endregion
	}
}
namespace YAMNL.Play.Serverbound
{
    public class PacketTeleportConfirm : IPacketPayload {
		public VarInt TeleportId { get; set; }
		public PacketTeleportConfirm(VarInt @teleportId) {
			TeleportId = @teleportId;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, TeleportId);
		}
		public static PacketTeleportConfirm Read(PacketBuffer buffer ) {
			VarInt @teleportId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketTeleportConfirm(@teleportId);
		}
	}
	public class PacketQueryBlockNbt : IPacketPayload {
		public VarInt TransactionId { get; set; }
		public PositionBitfield Location { get; set; }
		public PacketQueryBlockNbt(VarInt @transactionId, PositionBitfield @location) {
			TransactionId = @transactionId;
			Location = @location;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, TransactionId);
			((Action<PacketBuffer, PositionBitfield>)((buffer, value) => ((Action<PacketBuffer, ulong>)((buffer, value) => buffer.WriteU64(value)))(buffer, value.Value)))(buffer, Location);
		}
		public static PacketQueryBlockNbt Read(PacketBuffer buffer ) {
			VarInt @transactionId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			PositionBitfield @location = ((Func<PacketBuffer, PositionBitfield>)((buffer) => new PositionBitfield(((Func<PacketBuffer, ulong>)((buffer) => buffer.ReadU64()))(buffer))))(buffer);
			return new PacketQueryBlockNbt(@transactionId, @location);
		}
	}
	public class PacketChatCommand : IPacketPayload {
		public class ArgumentSignaturesElementContainer {
			public string ArgumentName { get; set; }
			public byte[] Signature { get; set; }
			public ArgumentSignaturesElementContainer(string @argumentName, byte[] @signature) {
				ArgumentName = @argumentName;
				Signature = @signature;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, ArgumentName);
				((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteBuffer(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Signature);
			}
			public static ArgumentSignaturesElementContainer Read(PacketBuffer buffer ) {
				string @argumentName = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
				byte[] @signature = ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadBuffer(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer))))(buffer);
				return new ArgumentSignaturesElementContainer(@argumentName, @signature);
			}
		}
		public class LastMessageContainer {
			public UUID Sender { get; set; }
			public byte[] Signature { get; set; }
			public LastMessageContainer(UUID @sender, byte[] @signature) {
				Sender = @sender;
				Signature = @signature;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, UUID>)((buffer, value) => buffer.WriteUUID(value)))(buffer, Sender);
				((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Signature);
			}
			public static LastMessageContainer Read(PacketBuffer buffer ) {
				UUID @sender = ((Func<PacketBuffer, UUID>)((buffer) => buffer.ReadUUID()))(buffer);
				byte[] @signature = ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8())))))(buffer);
				return new LastMessageContainer(@sender, @signature);
			}
		}
		public string Command { get; set; }
		public long Timestamp { get; set; }
		public long Salt { get; set; }
		public ArgumentSignaturesElementContainer[] ArgumentSignatures { get; set; }
		public bool SignedPreview { get; set; }
		public PreviousMessagesElement[] PreviousMessages { get; set; }
		public LastMessageContainer? LastMessage { get; set; }
		public PacketChatCommand(string @command, long @timestamp, long @salt, ArgumentSignaturesElementContainer[] @argumentSignatures, bool @signedPreview, PreviousMessagesElement[] @previousMessages, LastMessageContainer? @lastMessage) {
			Command = @command;
			Timestamp = @timestamp;
			Salt = @salt;
			ArgumentSignatures = @argumentSignatures;
			SignedPreview = @signedPreview;
			PreviousMessages = @previousMessages;
			LastMessage = @lastMessage;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Command);
			((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value)))(buffer, Timestamp);
			((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value)))(buffer, Salt);
			((Action<PacketBuffer, ArgumentSignaturesElementContainer[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, ArgumentSignaturesElementContainer>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, ArgumentSignatures);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, SignedPreview);
			((Action<PacketBuffer, PreviousMessagesElement[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, PreviousMessagesElement>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, PreviousMessages);
			((Action<PacketBuffer, LastMessageContainer?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, LastMessageContainer>)((buffer, value) => value.Write(buffer ))))))(buffer, LastMessage);
		}
		public static PacketChatCommand Read(PacketBuffer buffer ) {
			string @command = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			long @timestamp = ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64()))(buffer);
			long @salt = ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64()))(buffer);
			ArgumentSignaturesElementContainer[] @argumentSignatures = ((Func<PacketBuffer, ArgumentSignaturesElementContainer[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, ArgumentSignaturesElementContainer>)((buffer) => Mine.Net.Play.Serverbound.PacketChatCommand.ArgumentSignaturesElementContainer.Read(buffer ))))))(buffer);
			bool @signedPreview = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			PreviousMessagesElement[] @previousMessages = ((Func<PacketBuffer, PreviousMessagesElement[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, PreviousMessagesElement>)((buffer) => Mine.Net.PreviousMessagesElement.Read(buffer ))))))(buffer);
			LastMessageContainer? @lastMessage = ((Func<PacketBuffer, LastMessageContainer?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, LastMessageContainer>)((buffer) => Mine.Net.Play.Serverbound.PacketChatCommand.LastMessageContainer.Read(buffer ))))))(buffer);
			return new PacketChatCommand(@command, @timestamp, @salt, @argumentSignatures, @signedPreview, @previousMessages, @lastMessage);
		}
	}
	public class PacketChatMessage : IPacketPayload {
		public class LastMessageContainer {
			public UUID LastMessageUser { get; set; }
			public byte[] LastMessageSignature { get; set; }
			public LastMessageContainer(UUID @lastMessageUser, byte[] @lastMessageSignature) {
				LastMessageUser = @lastMessageUser;
				LastMessageSignature = @lastMessageSignature;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, UUID>)((buffer, value) => buffer.WriteUUID(value)))(buffer, LastMessageUser);
				((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, LastMessageSignature);
			}
			public static LastMessageContainer Read(PacketBuffer buffer ) {
				UUID @lastMessageUser = ((Func<PacketBuffer, UUID>)((buffer) => buffer.ReadUUID()))(buffer);
				byte[] @lastMessageSignature = ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8())))))(buffer);
				return new LastMessageContainer(@lastMessageUser, @lastMessageSignature);
			}
		}
		public string Message { get; set; }
		public long Timestamp { get; set; }
		public long Salt { get; set; }
		public byte[] Signature { get; set; }
		public bool SignedPreview { get; set; }
		public PreviousMessagesElement[] PreviousMessages { get; set; }
		public LastMessageContainer? LastMessage { get; set; }
		public PacketChatMessage(string @message, long @timestamp, long @salt, byte[] @signature, bool @signedPreview, PreviousMessagesElement[] @previousMessages, LastMessageContainer? @lastMessage) {
			Message = @message;
			Timestamp = @timestamp;
			Salt = @salt;
			Signature = @signature;
			SignedPreview = @signedPreview;
			PreviousMessages = @previousMessages;
			LastMessage = @lastMessage;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Message);
			((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value)))(buffer, Timestamp);
			((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value)))(buffer, Salt);
			((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteBuffer(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Signature);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, SignedPreview);
			((Action<PacketBuffer, PreviousMessagesElement[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, PreviousMessagesElement>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, PreviousMessages);
			((Action<PacketBuffer, LastMessageContainer?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, LastMessageContainer>)((buffer, value) => value.Write(buffer ))))))(buffer, LastMessage);
		}
		public static PacketChatMessage Read(PacketBuffer buffer ) {
			string @message = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			long @timestamp = ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64()))(buffer);
			long @salt = ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64()))(buffer);
			byte[] @signature = ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadBuffer(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer))))(buffer);
			bool @signedPreview = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			PreviousMessagesElement[] @previousMessages = ((Func<PacketBuffer, PreviousMessagesElement[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, PreviousMessagesElement>)((buffer) => Mine.Net.PreviousMessagesElement.Read(buffer ))))))(buffer);
			LastMessageContainer? @lastMessage = ((Func<PacketBuffer, LastMessageContainer?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, LastMessageContainer>)((buffer) => Mine.Net.Play.Serverbound.PacketChatMessage.LastMessageContainer.Read(buffer ))))))(buffer);
			return new PacketChatMessage(@message, @timestamp, @salt, @signature, @signedPreview, @previousMessages, @lastMessage);
		}
	}
	public class PacketChatPreview : IPacketPayload {
		public int Query { get; set; }
		public string Message { get; set; }
		public PacketChatPreview(int @query, string @message) {
			Query = @query;
			Message = @message;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, Query);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Message);
		}
		public static PacketChatPreview Read(PacketBuffer buffer ) {
			int @query = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			string @message = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			return new PacketChatPreview(@query, @message);
		}
	}
	public class PacketSetDifficulty : IPacketPayload {
		public byte NewDifficulty { get; set; }
		public PacketSetDifficulty(byte @newDifficulty) {
			NewDifficulty = @newDifficulty;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, NewDifficulty);
		}
		public static PacketSetDifficulty Read(PacketBuffer buffer ) {
			byte @newDifficulty = ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer);
			return new PacketSetDifficulty(@newDifficulty);
		}
	}
	public class PacketMessageAcknowledgement : IPacketPayload {
		public class LastMessageContainer {
			public UUID Sender { get; set; }
			public byte[] Signature { get; set; }
			public LastMessageContainer(UUID @sender, byte[] @signature) {
				Sender = @sender;
				Signature = @signature;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, UUID>)((buffer, value) => buffer.WriteUUID(value)))(buffer, Sender);
				((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Signature);
			}
			public static LastMessageContainer Read(PacketBuffer buffer ) {
				UUID @sender = ((Func<PacketBuffer, UUID>)((buffer) => buffer.ReadUUID()))(buffer);
				byte[] @signature = ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8())))))(buffer);
				return new LastMessageContainer(@sender, @signature);
			}
		}
		public PreviousMessagesElement[] PreviousMessages { get; set; }
		public LastMessageContainer? LastMessage { get; set; }
		public PacketMessageAcknowledgement(PreviousMessagesElement[] @previousMessages, LastMessageContainer? @lastMessage) {
			PreviousMessages = @previousMessages;
			LastMessage = @lastMessage;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, PreviousMessagesElement[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, PreviousMessagesElement>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, PreviousMessages);
			((Action<PacketBuffer, LastMessageContainer?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, LastMessageContainer>)((buffer, value) => value.Write(buffer ))))))(buffer, LastMessage);
		}
		public static PacketMessageAcknowledgement Read(PacketBuffer buffer ) {
			PreviousMessagesElement[] @previousMessages = ((Func<PacketBuffer, PreviousMessagesElement[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, PreviousMessagesElement>)((buffer) => Mine.Net.PreviousMessagesElement.Read(buffer ))))))(buffer);
			LastMessageContainer? @lastMessage = ((Func<PacketBuffer, LastMessageContainer?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, LastMessageContainer>)((buffer) => Mine.Net.Play.Serverbound.PacketMessageAcknowledgement.LastMessageContainer.Read(buffer ))))))(buffer);
			return new PacketMessageAcknowledgement(@previousMessages, @lastMessage);
		}
	}
	public class PacketEditBook : IPacketPayload {
		public VarInt Hand { get; set; }
		public string[] Pages { get; set; }
		public string? Title { get; set; }
		public PacketEditBook(VarInt @hand, string[] @pages, string? @title) {
			Hand = @hand;
			Pages = @pages;
			Title = @title;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Hand);
			((Action<PacketBuffer, string[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Pages);
			((Action<PacketBuffer, string?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))))))(buffer, Title);
		}
		public static PacketEditBook Read(PacketBuffer buffer ) {
			VarInt @hand = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			string[] @pages = ((Func<PacketBuffer, string[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer);
			string? @title = ((Func<PacketBuffer, string?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer);
			return new PacketEditBook(@hand, @pages, @title);
		}
	}
	public class PacketQueryEntityNbt : IPacketPayload {
		public VarInt TransactionId { get; set; }
		public VarInt EntityId { get; set; }
		public PacketQueryEntityNbt(VarInt @transactionId, VarInt @entityId) {
			TransactionId = @transactionId;
			EntityId = @entityId;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, TransactionId);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, EntityId);
		}
		public static PacketQueryEntityNbt Read(PacketBuffer buffer ) {
			VarInt @transactionId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @entityId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketQueryEntityNbt(@transactionId, @entityId);
		}
	}
	public class PacketPickItem : IPacketPayload {
		public VarInt Slot { get; set; }
		public PacketPickItem(VarInt @slot) {
			Slot = @slot;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Slot);
		}
		public static PacketPickItem Read(PacketBuffer buffer ) {
			VarInt @slot = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketPickItem(@slot);
		}
	}
	public class PacketNameItem : IPacketPayload {
		public string Name { get; set; }
		public PacketNameItem(string @name) {
			Name = @name;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Name);
		}
		public static PacketNameItem Read(PacketBuffer buffer ) {
			string @name = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			return new PacketNameItem(@name);
		}
	}
	public class PacketSelectTrade : IPacketPayload {
		public VarInt Slot { get; set; }
		public PacketSelectTrade(VarInt @slot) {
			Slot = @slot;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Slot);
		}
		public static PacketSelectTrade Read(PacketBuffer buffer ) {
			VarInt @slot = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketSelectTrade(@slot);
		}
	}
	public class PacketSetBeaconEffect : IPacketPayload {
		public VarInt? PrimaryEffect { get; set; }
		public VarInt? SecondaryEffect { get; set; }
		public PacketSetBeaconEffect(VarInt? @primaryEffect, VarInt? @secondaryEffect) {
			PrimaryEffect = @primaryEffect;
			SecondaryEffect = @secondaryEffect;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, PrimaryEffect);
			((Action<PacketBuffer, VarInt?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, SecondaryEffect);
		}
		public static PacketSetBeaconEffect Read(PacketBuffer buffer ) {
			VarInt? @primaryEffect = ((Func<PacketBuffer, VarInt?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			VarInt? @secondaryEffect = ((Func<PacketBuffer, VarInt?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			return new PacketSetBeaconEffect(@primaryEffect, @secondaryEffect);
		}
	}
	public class PacketUpdateCommandBlock : IPacketPayload {
		public PositionBitfield Location { get; set; }
		public string Command { get; set; }
		public VarInt Mode { get; set; }
		public byte Flags { get; set; }
		public PacketUpdateCommandBlock(PositionBitfield @location, string @command, VarInt @mode, byte @flags) {
			Location = @location;
			Command = @command;
			Mode = @mode;
			Flags = @flags;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, PositionBitfield>)((buffer, value) => ((Action<PacketBuffer, ulong>)((buffer, value) => buffer.WriteU64(value)))(buffer, value.Value)))(buffer, Location);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Command);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Mode);
			((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, Flags);
		}
		public static PacketUpdateCommandBlock Read(PacketBuffer buffer ) {
			PositionBitfield @location = ((Func<PacketBuffer, PositionBitfield>)((buffer) => new PositionBitfield(((Func<PacketBuffer, ulong>)((buffer) => buffer.ReadU64()))(buffer))))(buffer);
			string @command = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			VarInt @mode = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			byte @flags = ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer);
			return new PacketUpdateCommandBlock(@location, @command, @mode, @flags);
		}
	}
	public class PacketUpdateCommandBlockMinecart : IPacketPayload {
		public VarInt EntityId { get; set; }
		public string Command { get; set; }
		public bool TrackOutput { get; set; }
		public PacketUpdateCommandBlockMinecart(VarInt @entityId, string @command, bool @trackOutput) {
			EntityId = @entityId;
			Command = @command;
			TrackOutput = @trackOutput;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, EntityId);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Command);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, TrackOutput);
		}
		public static PacketUpdateCommandBlockMinecart Read(PacketBuffer buffer ) {
			VarInt @entityId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			string @command = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			bool @trackOutput = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketUpdateCommandBlockMinecart(@entityId, @command, @trackOutput);
		}
	}
	public class PacketUpdateStructureBlock : IPacketPayload {
		public PositionBitfield Location { get; set; }
		public VarInt Action { get; set; }
		public VarInt Mode { get; set; }
		public string Name { get; set; }
		public sbyte OffsetX { get; set; }
		public sbyte OffsetY { get; set; }
		public sbyte OffsetZ { get; set; }
		public sbyte SizeX { get; set; }
		public sbyte SizeY { get; set; }
		public sbyte SizeZ { get; set; }
		public VarInt Mirror { get; set; }
		public VarInt Rotation { get; set; }
		public string Metadata { get; set; }
		public float Integrity { get; set; }
		public VarInt Seed { get; set; }
		public byte Flags { get; set; }
		public PacketUpdateStructureBlock(PositionBitfield @location, VarInt @action, VarInt @mode, string @name, sbyte @offsetX, sbyte @offsetY, sbyte @offsetZ, sbyte @sizeX, sbyte @sizeY, sbyte @sizeZ, VarInt @mirror, VarInt @rotation, string @metadata, float @integrity, VarInt @seed, byte @flags) {
			Location = @location;
			Action = @action;
			Mode = @mode;
			Name = @name;
			OffsetX = @offsetX;
			OffsetY = @offsetY;
			OffsetZ = @offsetZ;
			SizeX = @sizeX;
			SizeY = @sizeY;
			SizeZ = @sizeZ;
			Mirror = @mirror;
			Rotation = @rotation;
			Metadata = @metadata;
			Integrity = @integrity;
			Seed = @seed;
			Flags = @flags;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, PositionBitfield>)((buffer, value) => ((Action<PacketBuffer, ulong>)((buffer, value) => buffer.WriteU64(value)))(buffer, value.Value)))(buffer, Location);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Action);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Mode);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Name);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, OffsetX);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, OffsetY);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, OffsetZ);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, SizeX);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, SizeY);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, SizeZ);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Mirror);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Rotation);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Metadata);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, Integrity);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Seed);
			((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, Flags);
		}
		public static PacketUpdateStructureBlock Read(PacketBuffer buffer ) {
			PositionBitfield @location = ((Func<PacketBuffer, PositionBitfield>)((buffer) => new PositionBitfield(((Func<PacketBuffer, ulong>)((buffer) => buffer.ReadU64()))(buffer))))(buffer);
			VarInt @action = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @mode = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			string @name = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			sbyte @offsetX = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			sbyte @offsetY = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			sbyte @offsetZ = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			sbyte @sizeX = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			sbyte @sizeY = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			sbyte @sizeZ = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			VarInt @mirror = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @rotation = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			string @metadata = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			float @integrity = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			VarInt @seed = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			byte @flags = ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer);
			return new PacketUpdateStructureBlock(@location, @action, @mode, @name, @offsetX, @offsetY, @offsetZ, @sizeX, @sizeY, @sizeZ, @mirror, @rotation, @metadata, @integrity, @seed, @flags);
		}
	}
	public class PacketTabComplete : IPacketPayload {
		public VarInt TransactionId { get; set; }
		public string Text { get; set; }
		public PacketTabComplete(VarInt @transactionId, string @text) {
			TransactionId = @transactionId;
			Text = @text;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, TransactionId);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Text);
		}
		public static PacketTabComplete Read(PacketBuffer buffer ) {
			VarInt @transactionId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			string @text = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			return new PacketTabComplete(@transactionId, @text);
		}
	}
	public class PacketClientCommand : IPacketPayload {
		public VarInt ActionId { get; set; }
		public PacketClientCommand(VarInt @actionId) {
			ActionId = @actionId;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, ActionId);
		}
		public static PacketClientCommand Read(PacketBuffer buffer ) {
			VarInt @actionId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketClientCommand(@actionId);
		}
	}
	public class PacketSettings : IPacketPayload {
		public string Locale { get; set; }
		public sbyte ViewDistance { get; set; }
		public VarInt ChatFlags { get; set; }
		public bool ChatColors { get; set; }
		public byte SkinParts { get; set; }
		public VarInt MainHand { get; set; }
		public bool EnableTextFiltering { get; set; }
		public bool EnableServerListing { get; set; }
		public PacketSettings(string @locale, sbyte @viewDistance, VarInt @chatFlags, bool @chatColors, byte @skinParts, VarInt @mainHand, bool @enableTextFiltering, bool @enableServerListing) {
			Locale = @locale;
			ViewDistance = @viewDistance;
			ChatFlags = @chatFlags;
			ChatColors = @chatColors;
			SkinParts = @skinParts;
			MainHand = @mainHand;
			EnableTextFiltering = @enableTextFiltering;
			EnableServerListing = @enableServerListing;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Locale);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, ViewDistance);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, ChatFlags);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, ChatColors);
			((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, SkinParts);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, MainHand);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, EnableTextFiltering);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, EnableServerListing);
		}
		public static PacketSettings Read(PacketBuffer buffer ) {
			string @locale = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			sbyte @viewDistance = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			VarInt @chatFlags = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			bool @chatColors = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			byte @skinParts = ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer);
			VarInt @mainHand = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			bool @enableTextFiltering = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			bool @enableServerListing = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketSettings(@locale, @viewDistance, @chatFlags, @chatColors, @skinParts, @mainHand, @enableTextFiltering, @enableServerListing);
		}
	}
	public class PacketEnchantItem : IPacketPayload {
		public sbyte WindowId { get; set; }
		public sbyte Enchantment { get; set; }
		public PacketEnchantItem(sbyte @windowId, sbyte @enchantment) {
			WindowId = @windowId;
			Enchantment = @enchantment;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, WindowId);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, Enchantment);
		}
		public static PacketEnchantItem Read(PacketBuffer buffer ) {
			sbyte @windowId = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			sbyte @enchantment = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			return new PacketEnchantItem(@windowId, @enchantment);
		}
	}
	public class PacketWindowClick : IPacketPayload {
		public class ChangedSlotsElementContainer {
			public short Location { get; set; }
			public Slot Item { get; set; }
			public ChangedSlotsElementContainer(short @location, Slot @item) {
				Location = @location;
				Item = @item;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, short>)((buffer, value) => buffer.WriteI16(value)))(buffer, Location);
				((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer )))(buffer, Item);
			}
			public static ChangedSlotsElementContainer Read(PacketBuffer buffer ) {
				short @location = ((Func<PacketBuffer, short>)((buffer) => buffer.ReadI16()))(buffer);
				Slot @item = ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer )))(buffer);
				return new ChangedSlotsElementContainer(@location, @item);
			}
		}
		public byte WindowId { get; set; }
		public VarInt StateId { get; set; }
		public short Slot { get; set; }
		public sbyte MouseButton { get; set; }
		public VarInt Mode { get; set; }
		public ChangedSlotsElementContainer[] ChangedSlots { get; set; }
		public Slot CursorItem { get; set; }
		public PacketWindowClick(byte @windowId, VarInt @stateId, short @slot, sbyte @mouseButton, VarInt @mode, ChangedSlotsElementContainer[] @changedSlots, Slot @cursorItem) {
			WindowId = @windowId;
			StateId = @stateId;
			Slot = @slot;
			MouseButton = @mouseButton;
			Mode = @mode;
			ChangedSlots = @changedSlots;
			CursorItem = @cursorItem;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, WindowId);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, StateId);
			((Action<PacketBuffer, short>)((buffer, value) => buffer.WriteI16(value)))(buffer, Slot);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, MouseButton);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Mode);
			((Action<PacketBuffer, ChangedSlotsElementContainer[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, ChangedSlotsElementContainer>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, ChangedSlots);
			((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer )))(buffer, CursorItem);
		}
		public static PacketWindowClick Read(PacketBuffer buffer ) {
			byte @windowId = ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer);
			VarInt @stateId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			short @slot = ((Func<PacketBuffer, short>)((buffer) => buffer.ReadI16()))(buffer);
			sbyte @mouseButton = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			VarInt @mode = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			ChangedSlotsElementContainer[] @changedSlots = ((Func<PacketBuffer, ChangedSlotsElementContainer[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, ChangedSlotsElementContainer>)((buffer) => Mine.Net.Play.Serverbound.PacketWindowClick.ChangedSlotsElementContainer.Read(buffer ))))))(buffer);
			Slot @cursorItem = ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer )))(buffer);
			return new PacketWindowClick(@windowId, @stateId, @slot, @mouseButton, @mode, @changedSlots, @cursorItem);
		}
	}
	public class PacketCloseWindow : IPacketPayload {
		public byte WindowId { get; set; }
		public PacketCloseWindow(byte @windowId) {
			WindowId = @windowId;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, WindowId);
		}
		public static PacketCloseWindow Read(PacketBuffer buffer ) {
			byte @windowId = ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer);
			return new PacketCloseWindow(@windowId);
		}
	}
	public class PacketCustomPayload : IPacketPayload {
		public string Channel { get; set; }
		public byte[] Data { get; set; }
		public PacketCustomPayload(string @channel, byte[] @data) {
			Channel = @channel;
			Data = @data;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Channel);
			((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteRestBuffer(value)))(buffer, Data);
		}
		public static PacketCustomPayload Read(PacketBuffer buffer ) {
			string @channel = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			byte[] @data = ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadRestBuffer()))(buffer);
			return new PacketCustomPayload(@channel, @data);
		}
	}
	public class PacketUseEntity : IPacketPayload {
		public class XSwitch {
			public object? Value { get; set; }
			public XSwitch(object? value) {
				Value = value;
			}
			public void Write(PacketBuffer buffer, VarInt state) {
				switch (state) {
					case 2: ((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, (float)this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static XSwitch Read(PacketBuffer buffer, VarInt state) {
				object? value = state.Value switch {
					2 => ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new XSwitch(value);
			}
			public static implicit operator float?(XSwitch value) => (float?)value.Value;
			public static implicit operator XSwitch?(float? value) => new XSwitch(value);
		}
		public class YSwitch {
			public object? Value { get; set; }
			public YSwitch(object? value) {
				Value = value;
			}
			public void Write(PacketBuffer buffer, VarInt state) {
				switch (state) {
					case 2: ((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, (float)this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static YSwitch Read(PacketBuffer buffer, VarInt state) {
				object? value = state.Value switch {
					2 => ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new YSwitch(value);
			}
			public static implicit operator float?(YSwitch value) => (float?)value.Value;
			public static implicit operator YSwitch?(float? value) => new YSwitch(value);
		}
		public class ZSwitch {
			public object? Value { get; set; }
			public ZSwitch(object? value) {
				Value = value;
			}
			public void Write(PacketBuffer buffer, VarInt state) {
				switch (state) {
					case 2: ((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, (float)this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static ZSwitch Read(PacketBuffer buffer, VarInt state) {
				object? value = state.Value switch {
					2 => ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new ZSwitch(value);
			}
			public static implicit operator float?(ZSwitch value) => (float?)value.Value;
			public static implicit operator ZSwitch?(float? value) => new ZSwitch(value);
		}
		public class HandSwitch {
			public object? Value { get; set; }
			public HandSwitch(object? value) {
				Value = value;
			}
			public void Write(PacketBuffer buffer, VarInt state) {
				switch (state) {
					case 0: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
					case 2: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static HandSwitch Read(PacketBuffer buffer, VarInt state) {
				object? value = state.Value switch {
					0 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
					2 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new HandSwitch(value);
			}
			public static implicit operator VarInt?(HandSwitch value) => (VarInt?)value.Value;
			public static implicit operator HandSwitch?(VarInt? value) => new HandSwitch(value);
		}
		public VarInt Target { get; set; }
		public VarInt Mouse { get; set; }
		public XSwitch X { get; set; }
		public YSwitch Y { get; set; }
		public ZSwitch Z { get; set; }
		public HandSwitch Hand { get; set; }
		public bool Sneaking { get; set; }
		public PacketUseEntity(VarInt @target, VarInt @mouse, XSwitch @x, YSwitch @y, ZSwitch @z, HandSwitch @hand, bool @sneaking) {
			Target = @target;
			Mouse = @mouse;
			X = @x;
			Y = @y;
			Z = @z;
			Hand = @hand;
			Sneaking = @sneaking;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Target);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Mouse);
			((Action<PacketBuffer, XSwitch>)((buffer, value) => value.Write(buffer, Mouse)))(buffer, X);
			((Action<PacketBuffer, YSwitch>)((buffer, value) => value.Write(buffer, Mouse)))(buffer, Y);
			((Action<PacketBuffer, ZSwitch>)((buffer, value) => value.Write(buffer, Mouse)))(buffer, Z);
			((Action<PacketBuffer, HandSwitch>)((buffer, value) => value.Write(buffer, Mouse)))(buffer, Hand);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, Sneaking);
		}
		public static PacketUseEntity Read(PacketBuffer buffer ) {
			VarInt @target = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @mouse = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			XSwitch @x = ((Func<PacketBuffer, XSwitch>)((buffer) => XSwitch.Read(buffer, @mouse)))(buffer);
			YSwitch @y = ((Func<PacketBuffer, YSwitch>)((buffer) => YSwitch.Read(buffer, @mouse)))(buffer);
			ZSwitch @z = ((Func<PacketBuffer, ZSwitch>)((buffer) => ZSwitch.Read(buffer, @mouse)))(buffer);
			HandSwitch @hand = ((Func<PacketBuffer, HandSwitch>)((buffer) => HandSwitch.Read(buffer, @mouse)))(buffer);
			bool @sneaking = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketUseEntity(@target, @mouse, @x, @y, @z, @hand, @sneaking);
		}
	}
	public class PacketGenerateStructure : IPacketPayload {
		public PositionBitfield Location { get; set; }
		public VarInt Levels { get; set; }
		public bool KeepJigsaws { get; set; }
		public PacketGenerateStructure(PositionBitfield @location, VarInt @levels, bool @keepJigsaws) {
			Location = @location;
			Levels = @levels;
			KeepJigsaws = @keepJigsaws;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, PositionBitfield>)((buffer, value) => ((Action<PacketBuffer, ulong>)((buffer, value) => buffer.WriteU64(value)))(buffer, value.Value)))(buffer, Location);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Levels);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, KeepJigsaws);
		}
		public static PacketGenerateStructure Read(PacketBuffer buffer ) {
			PositionBitfield @location = ((Func<PacketBuffer, PositionBitfield>)((buffer) => new PositionBitfield(((Func<PacketBuffer, ulong>)((buffer) => buffer.ReadU64()))(buffer))))(buffer);
			VarInt @levels = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			bool @keepJigsaws = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketGenerateStructure(@location, @levels, @keepJigsaws);
		}
	}
	public class PacketKeepAlive : IPacketPayload {
		public long KeepAliveId { get; set; }
		public PacketKeepAlive(long @keepAliveId) {
			KeepAliveId = @keepAliveId;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value)))(buffer, KeepAliveId);
		}
		public static PacketKeepAlive Read(PacketBuffer buffer ) {
			long @keepAliveId = ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64()))(buffer);
			return new PacketKeepAlive(@keepAliveId);
		}
	}
	public class PacketLockDifficulty : IPacketPayload {
		public bool Locked { get; set; }
		public PacketLockDifficulty(bool @locked) {
			Locked = @locked;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, Locked);
		}
		public static PacketLockDifficulty Read(PacketBuffer buffer ) {
			bool @locked = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketLockDifficulty(@locked);
		}
	}
	public class PacketPosition : IPacketPayload {
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
		public bool OnGround { get; set; }
		public PacketPosition(double @x, double @y, double @z, bool @onGround) {
			X = @x;
			Y = @y;
			Z = @z;
			OnGround = @onGround;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, X);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, Y);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, Z);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, OnGround);
		}
		public static PacketPosition Read(PacketBuffer buffer ) {
			double @x = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @y = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @z = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			bool @onGround = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketPosition(@x, @y, @z, @onGround);
		}
	}
	public class PacketPositionLook : IPacketPayload {
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
		public float Yaw { get; set; }
		public float Pitch { get; set; }
		public bool OnGround { get; set; }
		public PacketPositionLook(double @x, double @y, double @z, float @yaw, float @pitch, bool @onGround) {
			X = @x;
			Y = @y;
			Z = @z;
			Yaw = @yaw;
			Pitch = @pitch;
			OnGround = @onGround;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, X);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, Y);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, Z);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, Yaw);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, Pitch);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, OnGround);
		}
		public static PacketPositionLook Read(PacketBuffer buffer ) {
			double @x = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @y = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @z = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			float @yaw = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			float @pitch = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			bool @onGround = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketPositionLook(@x, @y, @z, @yaw, @pitch, @onGround);
		}
	}
	public class PacketLook : IPacketPayload {
		public float Yaw { get; set; }
		public float Pitch { get; set; }
		public bool OnGround { get; set; }
		public PacketLook(float @yaw, float @pitch, bool @onGround) {
			Yaw = @yaw;
			Pitch = @pitch;
			OnGround = @onGround;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, Yaw);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, Pitch);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, OnGround);
		}
		public static PacketLook Read(PacketBuffer buffer ) {
			float @yaw = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			float @pitch = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			bool @onGround = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketLook(@yaw, @pitch, @onGround);
		}
	}
	public class PacketFlying : IPacketPayload {
		public bool OnGround { get; set; }
		public PacketFlying(bool @onGround) {
			OnGround = @onGround;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, OnGround);
		}
		public static PacketFlying Read(PacketBuffer buffer ) {
			bool @onGround = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketFlying(@onGround);
		}
	}
	public class PacketVehicleMove : IPacketPayload {
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
		public float Yaw { get; set; }
		public float Pitch { get; set; }
		public PacketVehicleMove(double @x, double @y, double @z, float @yaw, float @pitch) {
			X = @x;
			Y = @y;
			Z = @z;
			Yaw = @yaw;
			Pitch = @pitch;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, X);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, Y);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, Z);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, Yaw);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, Pitch);
		}
		public static PacketVehicleMove Read(PacketBuffer buffer ) {
			double @x = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @y = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @z = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			float @yaw = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			float @pitch = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			return new PacketVehicleMove(@x, @y, @z, @yaw, @pitch);
		}
	}
	public class PacketSteerBoat : IPacketPayload {
		public bool LeftPaddle { get; set; }
		public bool RightPaddle { get; set; }
		public PacketSteerBoat(bool @leftPaddle, bool @rightPaddle) {
			LeftPaddle = @leftPaddle;
			RightPaddle = @rightPaddle;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, LeftPaddle);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, RightPaddle);
		}
		public static PacketSteerBoat Read(PacketBuffer buffer ) {
			bool @leftPaddle = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			bool @rightPaddle = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketSteerBoat(@leftPaddle, @rightPaddle);
		}
	}
	public class PacketCraftRecipeRequest : IPacketPayload {
		public sbyte WindowId { get; set; }
		public string Recipe { get; set; }
		public bool MakeAll { get; set; }
		public PacketCraftRecipeRequest(sbyte @windowId, string @recipe, bool @makeAll) {
			WindowId = @windowId;
			Recipe = @recipe;
			MakeAll = @makeAll;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, WindowId);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Recipe);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, MakeAll);
		}
		public static PacketCraftRecipeRequest Read(PacketBuffer buffer ) {
			sbyte @windowId = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			string @recipe = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			bool @makeAll = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketCraftRecipeRequest(@windowId, @recipe, @makeAll);
		}
	}
	public class PacketAbilities : IPacketPayload {
		public sbyte Flags { get; set; }
		public PacketAbilities(sbyte @flags) {
			Flags = @flags;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, Flags);
		}
		public static PacketAbilities Read(PacketBuffer buffer ) {
			sbyte @flags = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			return new PacketAbilities(@flags);
		}
	}
	public class PacketBlockDig : IPacketPayload {
		public VarInt Status { get; set; }
		public PositionBitfield Location { get; set; }
		public sbyte Face { get; set; }
		public VarInt Sequence { get; set; }
		public PacketBlockDig(VarInt @status, PositionBitfield @location, sbyte @face, VarInt @sequence) {
			Status = @status;
			Location = @location;
			Face = @face;
			Sequence = @sequence;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Status);
			((Action<PacketBuffer, PositionBitfield>)((buffer, value) => ((Action<PacketBuffer, ulong>)((buffer, value) => buffer.WriteU64(value)))(buffer, value.Value)))(buffer, Location);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, Face);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Sequence);
		}
		public static PacketBlockDig Read(PacketBuffer buffer ) {
			VarInt @status = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			PositionBitfield @location = ((Func<PacketBuffer, PositionBitfield>)((buffer) => new PositionBitfield(((Func<PacketBuffer, ulong>)((buffer) => buffer.ReadU64()))(buffer))))(buffer);
			sbyte @face = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			VarInt @sequence = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketBlockDig(@status, @location, @face, @sequence);
		}
	}
	public class PacketEntityAction : IPacketPayload {
		public VarInt EntityId { get; set; }
		public VarInt ActionId { get; set; }
		public VarInt JumpBoost { get; set; }
		public PacketEntityAction(VarInt @entityId, VarInt @actionId, VarInt @jumpBoost) {
			EntityId = @entityId;
			ActionId = @actionId;
			JumpBoost = @jumpBoost;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, EntityId);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, ActionId);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, JumpBoost);
		}
		public static PacketEntityAction Read(PacketBuffer buffer ) {
			VarInt @entityId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @actionId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @jumpBoost = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketEntityAction(@entityId, @actionId, @jumpBoost);
		}
	}
	public class PacketSteerVehicle : IPacketPayload {
		public float Sideways { get; set; }
		public float Forward { get; set; }
		public byte Jump { get; set; }
		public PacketSteerVehicle(float @sideways, float @forward, byte @jump) {
			Sideways = @sideways;
			Forward = @forward;
			Jump = @jump;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, Sideways);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, Forward);
			((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, Jump);
		}
		public static PacketSteerVehicle Read(PacketBuffer buffer ) {
			float @sideways = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			float @forward = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			byte @jump = ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer);
			return new PacketSteerVehicle(@sideways, @forward, @jump);
		}
	}
	public class PacketDisplayedRecipe : IPacketPayload {
		public string RecipeId { get; set; }
		public PacketDisplayedRecipe(string @recipeId) {
			RecipeId = @recipeId;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, RecipeId);
		}
		public static PacketDisplayedRecipe Read(PacketBuffer buffer ) {
			string @recipeId = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			return new PacketDisplayedRecipe(@recipeId);
		}
	}
	public class PacketRecipeBook : IPacketPayload {
		public VarInt BookId { get; set; }
		public bool BookOpen { get; set; }
		public bool FilterActive { get; set; }
		public PacketRecipeBook(VarInt @bookId, bool @bookOpen, bool @filterActive) {
			BookId = @bookId;
			BookOpen = @bookOpen;
			FilterActive = @filterActive;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, BookId);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, BookOpen);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, FilterActive);
		}
		public static PacketRecipeBook Read(PacketBuffer buffer ) {
			VarInt @bookId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			bool @bookOpen = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			bool @filterActive = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketRecipeBook(@bookId, @bookOpen, @filterActive);
		}
	}
	public class PacketResourcePackReceive : IPacketPayload {
		public VarInt Result { get; set; }
		public PacketResourcePackReceive(VarInt @result) {
			Result = @result;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Result);
		}
		public static PacketResourcePackReceive Read(PacketBuffer buffer ) {
			VarInt @result = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketResourcePackReceive(@result);
		}
	}
	public class PacketHeldItemSlot : IPacketPayload {
		public short SlotId { get; set; }
		public PacketHeldItemSlot(short @slotId) {
			SlotId = @slotId;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, short>)((buffer, value) => buffer.WriteI16(value)))(buffer, SlotId);
		}
		public static PacketHeldItemSlot Read(PacketBuffer buffer ) {
			short @slotId = ((Func<PacketBuffer, short>)((buffer) => buffer.ReadI16()))(buffer);
			return new PacketHeldItemSlot(@slotId);
		}
	}
	public class PacketSetCreativeSlot : IPacketPayload {
		public short Slot { get; set; }
		public Slot Item { get; set; }
		public PacketSetCreativeSlot(short @slot, Slot @item) {
			Slot = @slot;
			Item = @item;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, short>)((buffer, value) => buffer.WriteI16(value)))(buffer, Slot);
			((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer )))(buffer, Item);
		}
		public static PacketSetCreativeSlot Read(PacketBuffer buffer ) {
			short @slot = ((Func<PacketBuffer, short>)((buffer) => buffer.ReadI16()))(buffer);
			Slot @item = ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer )))(buffer);
			return new PacketSetCreativeSlot(@slot, @item);
		}
	}
	public class PacketUpdateJigsawBlock : IPacketPayload {
		public PositionBitfield Location { get; set; }
		public string Name { get; set; }
		public string Target { get; set; }
		public string Pool { get; set; }
		public string FinalState { get; set; }
		public string JointType { get; set; }
		public PacketUpdateJigsawBlock(PositionBitfield @location, string @name, string @target, string @pool, string @finalState, string @jointType) {
			Location = @location;
			Name = @name;
			Target = @target;
			Pool = @pool;
			FinalState = @finalState;
			JointType = @jointType;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, PositionBitfield>)((buffer, value) => ((Action<PacketBuffer, ulong>)((buffer, value) => buffer.WriteU64(value)))(buffer, value.Value)))(buffer, Location);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Name);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Target);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Pool);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, FinalState);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, JointType);
		}
		public static PacketUpdateJigsawBlock Read(PacketBuffer buffer ) {
			PositionBitfield @location = ((Func<PacketBuffer, PositionBitfield>)((buffer) => new PositionBitfield(((Func<PacketBuffer, ulong>)((buffer) => buffer.ReadU64()))(buffer))))(buffer);
			string @name = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			string @target = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			string @pool = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			string @finalState = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			string @jointType = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			return new PacketUpdateJigsawBlock(@location, @name, @target, @pool, @finalState, @jointType);
		}
	}
	public class PacketUpdateSign : IPacketPayload {
		public PositionBitfield Location { get; set; }
		public string Text1 { get; set; }
		public string Text2 { get; set; }
		public string Text3 { get; set; }
		public string Text4 { get; set; }
		public PacketUpdateSign(PositionBitfield @location, string @text1, string @text2, string @text3, string @text4) {
			Location = @location;
			Text1 = @text1;
			Text2 = @text2;
			Text3 = @text3;
			Text4 = @text4;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, PositionBitfield>)((buffer, value) => ((Action<PacketBuffer, ulong>)((buffer, value) => buffer.WriteU64(value)))(buffer, value.Value)))(buffer, Location);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Text1);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Text2);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Text3);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Text4);
		}
		public static PacketUpdateSign Read(PacketBuffer buffer ) {
			PositionBitfield @location = ((Func<PacketBuffer, PositionBitfield>)((buffer) => new PositionBitfield(((Func<PacketBuffer, ulong>)((buffer) => buffer.ReadU64()))(buffer))))(buffer);
			string @text1 = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			string @text2 = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			string @text3 = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			string @text4 = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			return new PacketUpdateSign(@location, @text1, @text2, @text3, @text4);
		}
	}
	public class PacketArmAnimation : IPacketPayload {
		public VarInt Hand { get; set; }
		public PacketArmAnimation(VarInt @hand) {
			Hand = @hand;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Hand);
		}
		public static PacketArmAnimation Read(PacketBuffer buffer ) {
			VarInt @hand = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketArmAnimation(@hand);
		}
	}
	public class PacketSpectate : IPacketPayload {
		public UUID Target { get; set; }
		public PacketSpectate(UUID @target) {
			Target = @target;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, UUID>)((buffer, value) => buffer.WriteUUID(value)))(buffer, Target);
		}
		public static PacketSpectate Read(PacketBuffer buffer ) {
			UUID @target = ((Func<PacketBuffer, UUID>)((buffer) => buffer.ReadUUID()))(buffer);
			return new PacketSpectate(@target);
		}
	}
	public class PacketBlockPlace : IPacketPayload {
		public VarInt Hand { get; set; }
		public PositionBitfield Location { get; set; }
		public VarInt Direction { get; set; }
		public float CursorX { get; set; }
		public float CursorY { get; set; }
		public float CursorZ { get; set; }
		public bool InsideBlock { get; set; }
		public VarInt Sequence { get; set; }
		public PacketBlockPlace(VarInt @hand, PositionBitfield @location, VarInt @direction, float @cursorX, float @cursorY, float @cursorZ, bool @insideBlock, VarInt @sequence) {
			Hand = @hand;
			Location = @location;
			Direction = @direction;
			CursorX = @cursorX;
			CursorY = @cursorY;
			CursorZ = @cursorZ;
			InsideBlock = @insideBlock;
			Sequence = @sequence;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Hand);
			((Action<PacketBuffer, PositionBitfield>)((buffer, value) => ((Action<PacketBuffer, ulong>)((buffer, value) => buffer.WriteU64(value)))(buffer, value.Value)))(buffer, Location);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Direction);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, CursorX);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, CursorY);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, CursorZ);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, InsideBlock);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Sequence);
		}
		public static PacketBlockPlace Read(PacketBuffer buffer ) {
			VarInt @hand = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			PositionBitfield @location = ((Func<PacketBuffer, PositionBitfield>)((buffer) => new PositionBitfield(((Func<PacketBuffer, ulong>)((buffer) => buffer.ReadU64()))(buffer))))(buffer);
			VarInt @direction = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			float @cursorX = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			float @cursorY = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			float @cursorZ = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			bool @insideBlock = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			VarInt @sequence = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketBlockPlace(@hand, @location, @direction, @cursorX, @cursorY, @cursorZ, @insideBlock, @sequence);
		}
	}
	public class PacketUseItem : IPacketPayload {
		public VarInt Hand { get; set; }
		public VarInt Sequence { get; set; }
		public PacketUseItem(VarInt @hand, VarInt @sequence) {
			Hand = @hand;
			Sequence = @sequence;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Hand);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Sequence);
		}
		public static PacketUseItem Read(PacketBuffer buffer ) {
			VarInt @hand = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @sequence = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketUseItem(@hand, @sequence);
		}
	}
	public class PacketAdvancementTab : IPacketPayload {
		public class TabIdSwitch {
			public object? Value { get; set; }
			public TabIdSwitch(object? value) {
				Value = value;
			}
			public void Write(PacketBuffer buffer, VarInt state) {
				switch (state) {
					case 0: ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string)this); break;
					case 1: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
					default: throw new Exception($"Invalid value: '{state}'");
				}
			}
			public static TabIdSwitch Read(PacketBuffer buffer, VarInt state) {
				object? value = state.Value switch {
					0 => ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer),
					1 => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
					 _ => throw new Exception($"Invalid value: '{state}'")
				};
				return new TabIdSwitch(value);
			}
			public static implicit operator string?(TabIdSwitch value) => (string?)value.Value;
			public static implicit operator TabIdSwitch?(string? value) => new TabIdSwitch(value);
		}
		public VarInt Action { get; set; }
		public TabIdSwitch TabId { get; set; }
		public PacketAdvancementTab(VarInt @action, TabIdSwitch @tabId) {
			Action = @action;
			TabId = @tabId;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Action);
			((Action<PacketBuffer, TabIdSwitch>)((buffer, value) => value.Write(buffer, Action)))(buffer, TabId);
		}
		public static PacketAdvancementTab Read(PacketBuffer buffer ) {
			VarInt @action = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			TabIdSwitch @tabId = ((Func<PacketBuffer, TabIdSwitch>)((buffer) => TabIdSwitch.Read(buffer, @action)))(buffer);
			return new PacketAdvancementTab(@action, @tabId);
		}
	}
	public class PacketPong : IPacketPayload {
		public int Id { get; set; }
		public PacketPong(int @id) {
			Id = @id;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, Id);
		}
		public static PacketPong Read(PacketBuffer buffer ) {
			int @id = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			return new PacketPong(@id);
		}
	}
	public class Packet : IPacket {
		public class ParamsSwitch {
			public object? Value { get; set; }
			public ParamsSwitch(object? value) {
				Value = value;
			}
			public void Write(PacketBuffer buffer, string state) {
				switch (state) {
					case "teleport_confirm": ((Action<PacketBuffer, PacketTeleportConfirm>)((buffer, value) => value.Write(buffer )))(buffer, (PacketTeleportConfirm)this); break;
					case "query_block_nbt": ((Action<PacketBuffer, PacketQueryBlockNbt>)((buffer, value) => value.Write(buffer )))(buffer, (PacketQueryBlockNbt)this); break;
					case "set_difficulty": ((Action<PacketBuffer, PacketSetDifficulty>)((buffer, value) => value.Write(buffer )))(buffer, (PacketSetDifficulty)this); break;
					case "message_acknowledgement": ((Action<PacketBuffer, PacketMessageAcknowledgement>)((buffer, value) => value.Write(buffer )))(buffer, (PacketMessageAcknowledgement)this); break;
					case "chat_command": ((Action<PacketBuffer, PacketChatCommand>)((buffer, value) => value.Write(buffer )))(buffer, (PacketChatCommand)this); break;
					case "chat_message": ((Action<PacketBuffer, PacketChatMessage>)((buffer, value) => value.Write(buffer )))(buffer, (PacketChatMessage)this); break;
					case "chat_preview": ((Action<PacketBuffer, PacketChatPreview>)((buffer, value) => value.Write(buffer )))(buffer, (PacketChatPreview)this); break;
					case "client_command": ((Action<PacketBuffer, PacketClientCommand>)((buffer, value) => value.Write(buffer )))(buffer, (PacketClientCommand)this); break;
					case "settings": ((Action<PacketBuffer, PacketSettings>)((buffer, value) => value.Write(buffer )))(buffer, (PacketSettings)this); break;
					case "tab_complete": ((Action<PacketBuffer, PacketTabComplete>)((buffer, value) => value.Write(buffer )))(buffer, (PacketTabComplete)this); break;
					case "enchant_item": ((Action<PacketBuffer, PacketEnchantItem>)((buffer, value) => value.Write(buffer )))(buffer, (PacketEnchantItem)this); break;
					case "window_click": ((Action<PacketBuffer, PacketWindowClick>)((buffer, value) => value.Write(buffer )))(buffer, (PacketWindowClick)this); break;
					case "close_window": ((Action<PacketBuffer, PacketCloseWindow>)((buffer, value) => value.Write(buffer )))(buffer, (PacketCloseWindow)this); break;
					case "custom_payload": ((Action<PacketBuffer, PacketCustomPayload>)((buffer, value) => value.Write(buffer )))(buffer, (PacketCustomPayload)this); break;
					case "edit_book": ((Action<PacketBuffer, PacketEditBook>)((buffer, value) => value.Write(buffer )))(buffer, (PacketEditBook)this); break;
					case "query_entity_nbt": ((Action<PacketBuffer, PacketQueryEntityNbt>)((buffer, value) => value.Write(buffer )))(buffer, (PacketQueryEntityNbt)this); break;
					case "use_entity": ((Action<PacketBuffer, PacketUseEntity>)((buffer, value) => value.Write(buffer )))(buffer, (PacketUseEntity)this); break;
					case "generate_structure": ((Action<PacketBuffer, PacketGenerateStructure>)((buffer, value) => value.Write(buffer )))(buffer, (PacketGenerateStructure)this); break;
					case "keep_alive": ((Action<PacketBuffer, PacketKeepAlive>)((buffer, value) => value.Write(buffer )))(buffer, (PacketKeepAlive)this); break;
					case "lock_difficulty": ((Action<PacketBuffer, PacketLockDifficulty>)((buffer, value) => value.Write(buffer )))(buffer, (PacketLockDifficulty)this); break;
					case "position": ((Action<PacketBuffer, PacketPosition>)((buffer, value) => value.Write(buffer )))(buffer, (PacketPosition)this); break;
					case "position_look": ((Action<PacketBuffer, PacketPositionLook>)((buffer, value) => value.Write(buffer )))(buffer, (PacketPositionLook)this); break;
					case "look": ((Action<PacketBuffer, PacketLook>)((buffer, value) => value.Write(buffer )))(buffer, (PacketLook)this); break;
					case "flying": ((Action<PacketBuffer, PacketFlying>)((buffer, value) => value.Write(buffer )))(buffer, (PacketFlying)this); break;
					case "vehicle_move": ((Action<PacketBuffer, PacketVehicleMove>)((buffer, value) => value.Write(buffer )))(buffer, (PacketVehicleMove)this); break;
					case "steer_boat": ((Action<PacketBuffer, PacketSteerBoat>)((buffer, value) => value.Write(buffer )))(buffer, (PacketSteerBoat)this); break;
					case "pick_item": ((Action<PacketBuffer, PacketPickItem>)((buffer, value) => value.Write(buffer )))(buffer, (PacketPickItem)this); break;
					case "craft_recipe_request": ((Action<PacketBuffer, PacketCraftRecipeRequest>)((buffer, value) => value.Write(buffer )))(buffer, (PacketCraftRecipeRequest)this); break;
					case "abilities": ((Action<PacketBuffer, PacketAbilities>)((buffer, value) => value.Write(buffer )))(buffer, (PacketAbilities)this); break;
					case "block_dig": ((Action<PacketBuffer, PacketBlockDig>)((buffer, value) => value.Write(buffer )))(buffer, (PacketBlockDig)this); break;
					case "entity_action": ((Action<PacketBuffer, PacketEntityAction>)((buffer, value) => value.Write(buffer )))(buffer, (PacketEntityAction)this); break;
					case "steer_vehicle": ((Action<PacketBuffer, PacketSteerVehicle>)((buffer, value) => value.Write(buffer )))(buffer, (PacketSteerVehicle)this); break;
					case "pong": ((Action<PacketBuffer, PacketPong>)((buffer, value) => value.Write(buffer )))(buffer, (PacketPong)this); break;
					case "recipe_book": ((Action<PacketBuffer, PacketRecipeBook>)((buffer, value) => value.Write(buffer )))(buffer, (PacketRecipeBook)this); break;
					case "displayed_recipe": ((Action<PacketBuffer, PacketDisplayedRecipe>)((buffer, value) => value.Write(buffer )))(buffer, (PacketDisplayedRecipe)this); break;
					case "name_item": ((Action<PacketBuffer, PacketNameItem>)((buffer, value) => value.Write(buffer )))(buffer, (PacketNameItem)this); break;
					case "resource_pack_receive": ((Action<PacketBuffer, PacketResourcePackReceive>)((buffer, value) => value.Write(buffer )))(buffer, (PacketResourcePackReceive)this); break;
					case "advancement_tab": ((Action<PacketBuffer, PacketAdvancementTab>)((buffer, value) => value.Write(buffer )))(buffer, (PacketAdvancementTab)this); break;
					case "select_trade": ((Action<PacketBuffer, PacketSelectTrade>)((buffer, value) => value.Write(buffer )))(buffer, (PacketSelectTrade)this); break;
					case "set_beacon_effect": ((Action<PacketBuffer, PacketSetBeaconEffect>)((buffer, value) => value.Write(buffer )))(buffer, (PacketSetBeaconEffect)this); break;
					case "held_item_slot": ((Action<PacketBuffer, PacketHeldItemSlot>)((buffer, value) => value.Write(buffer )))(buffer, (PacketHeldItemSlot)this); break;
					case "update_command_block": ((Action<PacketBuffer, PacketUpdateCommandBlock>)((buffer, value) => value.Write(buffer )))(buffer, (PacketUpdateCommandBlock)this); break;
					case "update_command_block_minecart": ((Action<PacketBuffer, PacketUpdateCommandBlockMinecart>)((buffer, value) => value.Write(buffer )))(buffer, (PacketUpdateCommandBlockMinecart)this); break;
					case "set_creative_slot": ((Action<PacketBuffer, PacketSetCreativeSlot>)((buffer, value) => value.Write(buffer )))(buffer, (PacketSetCreativeSlot)this); break;
					case "update_jigsaw_block": ((Action<PacketBuffer, PacketUpdateJigsawBlock>)((buffer, value) => value.Write(buffer )))(buffer, (PacketUpdateJigsawBlock)this); break;
					case "update_structure_block": ((Action<PacketBuffer, PacketUpdateStructureBlock>)((buffer, value) => value.Write(buffer )))(buffer, (PacketUpdateStructureBlock)this); break;
					case "update_sign": ((Action<PacketBuffer, PacketUpdateSign>)((buffer, value) => value.Write(buffer )))(buffer, (PacketUpdateSign)this); break;
					case "arm_animation": ((Action<PacketBuffer, PacketArmAnimation>)((buffer, value) => value.Write(buffer )))(buffer, (PacketArmAnimation)this); break;
					case "spectate": ((Action<PacketBuffer, PacketSpectate>)((buffer, value) => value.Write(buffer )))(buffer, (PacketSpectate)this); break;
					case "block_place": ((Action<PacketBuffer, PacketBlockPlace>)((buffer, value) => value.Write(buffer )))(buffer, (PacketBlockPlace)this); break;
					case "use_item": ((Action<PacketBuffer, PacketUseItem>)((buffer, value) => value.Write(buffer )))(buffer, (PacketUseItem)this); break;
					default: throw new Exception($"Invalid value: '{state}'");
				}
			}
			public static ParamsSwitch Read(PacketBuffer buffer, string state) {
				object? value = state switch {
					"teleport_confirm" => ((Func<PacketBuffer, PacketTeleportConfirm>)((buffer) => Mine.Net.Play.Serverbound.PacketTeleportConfirm.Read(buffer )))(buffer),
					"query_block_nbt" => ((Func<PacketBuffer, PacketQueryBlockNbt>)((buffer) => Mine.Net.Play.Serverbound.PacketQueryBlockNbt.Read(buffer )))(buffer),
					"set_difficulty" => ((Func<PacketBuffer, PacketSetDifficulty>)((buffer) => Mine.Net.Play.Serverbound.PacketSetDifficulty.Read(buffer )))(buffer),
					"message_acknowledgement" => ((Func<PacketBuffer, PacketMessageAcknowledgement>)((buffer) => Mine.Net.Play.Serverbound.PacketMessageAcknowledgement.Read(buffer )))(buffer),
					"chat_command" => ((Func<PacketBuffer, PacketChatCommand>)((buffer) => Mine.Net.Play.Serverbound.PacketChatCommand.Read(buffer )))(buffer),
					"chat_message" => ((Func<PacketBuffer, PacketChatMessage>)((buffer) => Mine.Net.Play.Serverbound.PacketChatMessage.Read(buffer )))(buffer),
					"chat_preview" => ((Func<PacketBuffer, PacketChatPreview>)((buffer) => Mine.Net.Play.Serverbound.PacketChatPreview.Read(buffer )))(buffer),
					"client_command" => ((Func<PacketBuffer, PacketClientCommand>)((buffer) => Mine.Net.Play.Serverbound.PacketClientCommand.Read(buffer )))(buffer),
					"settings" => ((Func<PacketBuffer, PacketSettings>)((buffer) => Mine.Net.Play.Serverbound.PacketSettings.Read(buffer )))(buffer),
					"tab_complete" => ((Func<PacketBuffer, PacketTabComplete>)((buffer) => Mine.Net.Play.Serverbound.PacketTabComplete.Read(buffer )))(buffer),
					"enchant_item" => ((Func<PacketBuffer, PacketEnchantItem>)((buffer) => Mine.Net.Play.Serverbound.PacketEnchantItem.Read(buffer )))(buffer),
					"window_click" => ((Func<PacketBuffer, PacketWindowClick>)((buffer) => Mine.Net.Play.Serverbound.PacketWindowClick.Read(buffer )))(buffer),
					"close_window" => ((Func<PacketBuffer, PacketCloseWindow>)((buffer) => Mine.Net.Play.Serverbound.PacketCloseWindow.Read(buffer )))(buffer),
					"custom_payload" => ((Func<PacketBuffer, PacketCustomPayload>)((buffer) => Mine.Net.Play.Serverbound.PacketCustomPayload.Read(buffer )))(buffer),
					"edit_book" => ((Func<PacketBuffer, PacketEditBook>)((buffer) => Mine.Net.Play.Serverbound.PacketEditBook.Read(buffer )))(buffer),
					"query_entity_nbt" => ((Func<PacketBuffer, PacketQueryEntityNbt>)((buffer) => Mine.Net.Play.Serverbound.PacketQueryEntityNbt.Read(buffer )))(buffer),
					"use_entity" => ((Func<PacketBuffer, PacketUseEntity>)((buffer) => Mine.Net.Play.Serverbound.PacketUseEntity.Read(buffer )))(buffer),
					"generate_structure" => ((Func<PacketBuffer, PacketGenerateStructure>)((buffer) => Mine.Net.Play.Serverbound.PacketGenerateStructure.Read(buffer )))(buffer),
					"keep_alive" => ((Func<PacketBuffer, PacketKeepAlive>)((buffer) => Mine.Net.Play.Serverbound.PacketKeepAlive.Read(buffer )))(buffer),
					"lock_difficulty" => ((Func<PacketBuffer, PacketLockDifficulty>)((buffer) => Mine.Net.Play.Serverbound.PacketLockDifficulty.Read(buffer )))(buffer),
					"position" => ((Func<PacketBuffer, PacketPosition>)((buffer) => Mine.Net.Play.Serverbound.PacketPosition.Read(buffer )))(buffer),
					"position_look" => ((Func<PacketBuffer, PacketPositionLook>)((buffer) => Mine.Net.Play.Serverbound.PacketPositionLook.Read(buffer )))(buffer),
					"look" => ((Func<PacketBuffer, PacketLook>)((buffer) => Mine.Net.Play.Serverbound.PacketLook.Read(buffer )))(buffer),
					"flying" => ((Func<PacketBuffer, PacketFlying>)((buffer) => Mine.Net.Play.Serverbound.PacketFlying.Read(buffer )))(buffer),
					"vehicle_move" => ((Func<PacketBuffer, PacketVehicleMove>)((buffer) => Mine.Net.Play.Serverbound.PacketVehicleMove.Read(buffer )))(buffer),
					"steer_boat" => ((Func<PacketBuffer, PacketSteerBoat>)((buffer) => Mine.Net.Play.Serverbound.PacketSteerBoat.Read(buffer )))(buffer),
					"pick_item" => ((Func<PacketBuffer, PacketPickItem>)((buffer) => Mine.Net.Play.Serverbound.PacketPickItem.Read(buffer )))(buffer),
					"craft_recipe_request" => ((Func<PacketBuffer, PacketCraftRecipeRequest>)((buffer) => Mine.Net.Play.Serverbound.PacketCraftRecipeRequest.Read(buffer )))(buffer),
					"abilities" => ((Func<PacketBuffer, PacketAbilities>)((buffer) => Mine.Net.Play.Serverbound.PacketAbilities.Read(buffer )))(buffer),
					"block_dig" => ((Func<PacketBuffer, PacketBlockDig>)((buffer) => Mine.Net.Play.Serverbound.PacketBlockDig.Read(buffer )))(buffer),
					"entity_action" => ((Func<PacketBuffer, PacketEntityAction>)((buffer) => Mine.Net.Play.Serverbound.PacketEntityAction.Read(buffer )))(buffer),
					"steer_vehicle" => ((Func<PacketBuffer, PacketSteerVehicle>)((buffer) => Mine.Net.Play.Serverbound.PacketSteerVehicle.Read(buffer )))(buffer),
					"pong" => ((Func<PacketBuffer, PacketPong>)((buffer) => Mine.Net.Play.Serverbound.PacketPong.Read(buffer )))(buffer),
					"recipe_book" => ((Func<PacketBuffer, PacketRecipeBook>)((buffer) => Mine.Net.Play.Serverbound.PacketRecipeBook.Read(buffer )))(buffer),
					"displayed_recipe" => ((Func<PacketBuffer, PacketDisplayedRecipe>)((buffer) => Mine.Net.Play.Serverbound.PacketDisplayedRecipe.Read(buffer )))(buffer),
					"name_item" => ((Func<PacketBuffer, PacketNameItem>)((buffer) => Mine.Net.Play.Serverbound.PacketNameItem.Read(buffer )))(buffer),
					"resource_pack_receive" => ((Func<PacketBuffer, PacketResourcePackReceive>)((buffer) => Mine.Net.Play.Serverbound.PacketResourcePackReceive.Read(buffer )))(buffer),
					"advancement_tab" => ((Func<PacketBuffer, PacketAdvancementTab>)((buffer) => Mine.Net.Play.Serverbound.PacketAdvancementTab.Read(buffer )))(buffer),
					"select_trade" => ((Func<PacketBuffer, PacketSelectTrade>)((buffer) => Mine.Net.Play.Serverbound.PacketSelectTrade.Read(buffer )))(buffer),
					"set_beacon_effect" => ((Func<PacketBuffer, PacketSetBeaconEffect>)((buffer) => Mine.Net.Play.Serverbound.PacketSetBeaconEffect.Read(buffer )))(buffer),
					"held_item_slot" => ((Func<PacketBuffer, PacketHeldItemSlot>)((buffer) => Mine.Net.Play.Serverbound.PacketHeldItemSlot.Read(buffer )))(buffer),
					"update_command_block" => ((Func<PacketBuffer, PacketUpdateCommandBlock>)((buffer) => Mine.Net.Play.Serverbound.PacketUpdateCommandBlock.Read(buffer )))(buffer),
					"update_command_block_minecart" => ((Func<PacketBuffer, PacketUpdateCommandBlockMinecart>)((buffer) => Mine.Net.Play.Serverbound.PacketUpdateCommandBlockMinecart.Read(buffer )))(buffer),
					"set_creative_slot" => ((Func<PacketBuffer, PacketSetCreativeSlot>)((buffer) => Mine.Net.Play.Serverbound.PacketSetCreativeSlot.Read(buffer )))(buffer),
					"update_jigsaw_block" => ((Func<PacketBuffer, PacketUpdateJigsawBlock>)((buffer) => Mine.Net.Play.Serverbound.PacketUpdateJigsawBlock.Read(buffer )))(buffer),
					"update_structure_block" => ((Func<PacketBuffer, PacketUpdateStructureBlock>)((buffer) => Mine.Net.Play.Serverbound.PacketUpdateStructureBlock.Read(buffer )))(buffer),
					"update_sign" => ((Func<PacketBuffer, PacketUpdateSign>)((buffer) => Mine.Net.Play.Serverbound.PacketUpdateSign.Read(buffer )))(buffer),
					"arm_animation" => ((Func<PacketBuffer, PacketArmAnimation>)((buffer) => Mine.Net.Play.Serverbound.PacketArmAnimation.Read(buffer )))(buffer),
					"spectate" => ((Func<PacketBuffer, PacketSpectate>)((buffer) => Mine.Net.Play.Serverbound.PacketSpectate.Read(buffer )))(buffer),
					"block_place" => ((Func<PacketBuffer, PacketBlockPlace>)((buffer) => Mine.Net.Play.Serverbound.PacketBlockPlace.Read(buffer )))(buffer),
					"use_item" => ((Func<PacketBuffer, PacketUseItem>)((buffer) => Mine.Net.Play.Serverbound.PacketUseItem.Read(buffer )))(buffer),
					 _ => throw new Exception($"Invalid value: '{state}'")
				};
				return new ParamsSwitch(value);
			}
			public static implicit operator PacketTeleportConfirm?(ParamsSwitch value) => (PacketTeleportConfirm?)value.Value;
			public static implicit operator PacketQueryBlockNbt?(ParamsSwitch value) => (PacketQueryBlockNbt?)value.Value;
			public static implicit operator PacketSetDifficulty?(ParamsSwitch value) => (PacketSetDifficulty?)value.Value;
			public static implicit operator PacketMessageAcknowledgement?(ParamsSwitch value) => (PacketMessageAcknowledgement?)value.Value;
			public static implicit operator PacketChatCommand?(ParamsSwitch value) => (PacketChatCommand?)value.Value;
			public static implicit operator PacketChatMessage?(ParamsSwitch value) => (PacketChatMessage?)value.Value;
			public static implicit operator PacketChatPreview?(ParamsSwitch value) => (PacketChatPreview?)value.Value;
			public static implicit operator PacketClientCommand?(ParamsSwitch value) => (PacketClientCommand?)value.Value;
			public static implicit operator PacketSettings?(ParamsSwitch value) => (PacketSettings?)value.Value;
			public static implicit operator PacketTabComplete?(ParamsSwitch value) => (PacketTabComplete?)value.Value;
			public static implicit operator PacketEnchantItem?(ParamsSwitch value) => (PacketEnchantItem?)value.Value;
			public static implicit operator PacketWindowClick?(ParamsSwitch value) => (PacketWindowClick?)value.Value;
			public static implicit operator PacketCloseWindow?(ParamsSwitch value) => (PacketCloseWindow?)value.Value;
			public static implicit operator PacketCustomPayload?(ParamsSwitch value) => (PacketCustomPayload?)value.Value;
			public static implicit operator PacketEditBook?(ParamsSwitch value) => (PacketEditBook?)value.Value;
			public static implicit operator PacketQueryEntityNbt?(ParamsSwitch value) => (PacketQueryEntityNbt?)value.Value;
			public static implicit operator PacketUseEntity?(ParamsSwitch value) => (PacketUseEntity?)value.Value;
			public static implicit operator PacketGenerateStructure?(ParamsSwitch value) => (PacketGenerateStructure?)value.Value;
			public static implicit operator PacketKeepAlive?(ParamsSwitch value) => (PacketKeepAlive?)value.Value;
			public static implicit operator PacketLockDifficulty?(ParamsSwitch value) => (PacketLockDifficulty?)value.Value;
			public static implicit operator PacketPosition?(ParamsSwitch value) => (PacketPosition?)value.Value;
			public static implicit operator PacketPositionLook?(ParamsSwitch value) => (PacketPositionLook?)value.Value;
			public static implicit operator PacketLook?(ParamsSwitch value) => (PacketLook?)value.Value;
			public static implicit operator PacketFlying?(ParamsSwitch value) => (PacketFlying?)value.Value;
			public static implicit operator PacketVehicleMove?(ParamsSwitch value) => (PacketVehicleMove?)value.Value;
			public static implicit operator PacketSteerBoat?(ParamsSwitch value) => (PacketSteerBoat?)value.Value;
			public static implicit operator PacketPickItem?(ParamsSwitch value) => (PacketPickItem?)value.Value;
			public static implicit operator PacketCraftRecipeRequest?(ParamsSwitch value) => (PacketCraftRecipeRequest?)value.Value;
			public static implicit operator PacketAbilities?(ParamsSwitch value) => (PacketAbilities?)value.Value;
			public static implicit operator PacketBlockDig?(ParamsSwitch value) => (PacketBlockDig?)value.Value;
			public static implicit operator PacketEntityAction?(ParamsSwitch value) => (PacketEntityAction?)value.Value;
			public static implicit operator PacketSteerVehicle?(ParamsSwitch value) => (PacketSteerVehicle?)value.Value;
			public static implicit operator PacketPong?(ParamsSwitch value) => (PacketPong?)value.Value;
			public static implicit operator PacketRecipeBook?(ParamsSwitch value) => (PacketRecipeBook?)value.Value;
			public static implicit operator PacketDisplayedRecipe?(ParamsSwitch value) => (PacketDisplayedRecipe?)value.Value;
			public static implicit operator PacketNameItem?(ParamsSwitch value) => (PacketNameItem?)value.Value;
			public static implicit operator PacketResourcePackReceive?(ParamsSwitch value) => (PacketResourcePackReceive?)value.Value;
			public static implicit operator PacketAdvancementTab?(ParamsSwitch value) => (PacketAdvancementTab?)value.Value;
			public static implicit operator PacketSelectTrade?(ParamsSwitch value) => (PacketSelectTrade?)value.Value;
			public static implicit operator PacketSetBeaconEffect?(ParamsSwitch value) => (PacketSetBeaconEffect?)value.Value;
			public static implicit operator PacketHeldItemSlot?(ParamsSwitch value) => (PacketHeldItemSlot?)value.Value;
			public static implicit operator PacketUpdateCommandBlock?(ParamsSwitch value) => (PacketUpdateCommandBlock?)value.Value;
			public static implicit operator PacketUpdateCommandBlockMinecart?(ParamsSwitch value) => (PacketUpdateCommandBlockMinecart?)value.Value;
			public static implicit operator PacketSetCreativeSlot?(ParamsSwitch value) => (PacketSetCreativeSlot?)value.Value;
			public static implicit operator PacketUpdateJigsawBlock?(ParamsSwitch value) => (PacketUpdateJigsawBlock?)value.Value;
			public static implicit operator PacketUpdateStructureBlock?(ParamsSwitch value) => (PacketUpdateStructureBlock?)value.Value;
			public static implicit operator PacketUpdateSign?(ParamsSwitch value) => (PacketUpdateSign?)value.Value;
			public static implicit operator PacketArmAnimation?(ParamsSwitch value) => (PacketArmAnimation?)value.Value;
			public static implicit operator PacketSpectate?(ParamsSwitch value) => (PacketSpectate?)value.Value;
			public static implicit operator PacketBlockPlace?(ParamsSwitch value) => (PacketBlockPlace?)value.Value;
			public static implicit operator PacketUseItem?(ParamsSwitch value) => (PacketUseItem?)value.Value;
			public static implicit operator ParamsSwitch?(PacketTeleportConfirm? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketQueryBlockNbt? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketSetDifficulty? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketMessageAcknowledgement? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketChatCommand? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketChatMessage? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketChatPreview? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketClientCommand? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketSettings? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketTabComplete? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketEnchantItem? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketWindowClick? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketCloseWindow? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketCustomPayload? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketEditBook? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketQueryEntityNbt? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketUseEntity? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketGenerateStructure? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketKeepAlive? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketLockDifficulty? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketPosition? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketPositionLook? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketLook? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketFlying? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketVehicleMove? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketSteerBoat? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketPickItem? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketCraftRecipeRequest? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketAbilities? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketBlockDig? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketEntityAction? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketSteerVehicle? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketPong? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketRecipeBook? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketDisplayedRecipe? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketNameItem? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketResourcePackReceive? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketAdvancementTab? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketSelectTrade? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketSetBeaconEffect? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketHeldItemSlot? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketUpdateCommandBlock? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketUpdateCommandBlockMinecart? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketSetCreativeSlot? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketUpdateJigsawBlock? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketUpdateStructureBlock? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketUpdateSign? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketArmAnimation? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketSpectate? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketBlockPlace? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketUseItem? value) => new ParamsSwitch(value);
		}
		public string Name { get; set; }
		public ParamsSwitch Params { get; set; }
		public Packet(string @name, ParamsSwitch @params) {
			Name = @name;
			Params = @params;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, value switch { "teleport_confirm" => 0x00, "query_block_nbt" => 0x01, "set_difficulty" => 0x02, "message_acknowledgement" => 0x03, "chat_command" => 0x04, "chat_message" => 0x05, "chat_preview" => 0x06, "client_command" => 0x07, "settings" => 0x08, "tab_complete" => 0x09, "enchant_item" => 0x0a, "window_click" => 0x0b, "close_window" => 0x0c, "custom_payload" => 0x0d, "edit_book" => 0x0e, "query_entity_nbt" => 0x0f, "use_entity" => 0x10, "generate_structure" => 0x11, "keep_alive" => 0x12, "lock_difficulty" => 0x13, "position" => 0x14, "position_look" => 0x15, "look" => 0x16, "flying" => 0x17, "vehicle_move" => 0x18, "steer_boat" => 0x19, "pick_item" => 0x1a, "craft_recipe_request" => 0x1b, "abilities" => 0x1c, "block_dig" => 0x1d, "entity_action" => 0x1e, "steer_vehicle" => 0x1f, "pong" => 0x20, "recipe_book" => 0x21, "displayed_recipe" => 0x22, "name_item" => 0x23, "resource_pack_receive" => 0x24, "advancement_tab" => 0x25, "select_trade" => 0x26, "set_beacon_effect" => 0x27, "held_item_slot" => 0x28, "update_command_block" => 0x29, "update_command_block_minecart" => 0x2a, "set_creative_slot" => 0x2b, "update_jigsaw_block" => 0x2c, "update_structure_block" => 0x2d, "update_sign" => 0x2e, "arm_animation" => 0x2f, "spectate" => 0x30, "block_place" => 0x31, "use_item" => 0x32, _ => throw new Exception($"Value '{value}' not supported.") })))(buffer, Name);
			((Action<PacketBuffer, ParamsSwitch>)((buffer, value) => value.Write(buffer, Name)))(buffer, Params);
		}
		public static Packet Read(PacketBuffer buffer ) {
			string @name = ((Func<PacketBuffer, string>)((buffer) => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer).Value switch { 0x00 => "teleport_confirm", 0x01 => "query_block_nbt", 0x02 => "set_difficulty", 0x03 => "message_acknowledgement", 0x04 => "chat_command", 0x05 => "chat_message", 0x06 => "chat_preview", 0x07 => "client_command", 0x08 => "settings", 0x09 => "tab_complete", 0x0a => "enchant_item", 0x0b => "window_click", 0x0c => "close_window", 0x0d => "custom_payload", 0x0e => "edit_book", 0x0f => "query_entity_nbt", 0x10 => "use_entity", 0x11 => "generate_structure", 0x12 => "keep_alive", 0x13 => "lock_difficulty", 0x14 => "position", 0x15 => "position_look", 0x16 => "look", 0x17 => "flying", 0x18 => "vehicle_move", 0x19 => "steer_boat", 0x1a => "pick_item", 0x1b => "craft_recipe_request", 0x1c => "abilities", 0x1d => "block_dig", 0x1e => "entity_action", 0x1f => "steer_vehicle", 0x20 => "pong", 0x21 => "recipe_book", 0x22 => "displayed_recipe", 0x23 => "name_item", 0x24 => "resource_pack_receive", 0x25 => "advancement_tab", 0x26 => "select_trade", 0x27 => "set_beacon_effect", 0x28 => "held_item_slot", 0x29 => "update_command_block", 0x2a => "update_command_block_minecart", 0x2b => "set_creative_slot", 0x2c => "update_jigsaw_block", 0x2d => "update_structure_block", 0x2e => "update_sign", 0x2f => "arm_animation", 0x30 => "spectate", 0x31 => "block_place", 0x32 => "use_item", _ => throw new Exception() }))(buffer);
			ParamsSwitch @params = ((Func<PacketBuffer, ParamsSwitch>)((buffer) => ParamsSwitch.Read(buffer, @name)))(buffer);
			return new Packet(@name, @params);
		}
	}
	public class PlayPacketFactory : IPacketFactory {
		public IPacket ReadPacket(PacketBuffer buffer) {
			return Mine.Net.Play.Serverbound.Packet.Read(buffer);
		}
		public void WritePacket(PacketBuffer buffer, IPacketPayload packet) {
			switch (packet) {
				case PacketTeleportConfirm p_0x00: new Mine.Net.Play.Serverbound.Packet("teleport_confirm", p_0x00!).Write(buffer); break;
				case PacketQueryBlockNbt p_0x01: new Mine.Net.Play.Serverbound.Packet("query_block_nbt", p_0x01!).Write(buffer); break;
				case PacketSetDifficulty p_0x02: new Mine.Net.Play.Serverbound.Packet("set_difficulty", p_0x02!).Write(buffer); break;
				case PacketMessageAcknowledgement p_0x03: new Mine.Net.Play.Serverbound.Packet("message_acknowledgement", p_0x03!).Write(buffer); break;
				case PacketChatCommand p_0x04: new Mine.Net.Play.Serverbound.Packet("chat_command", p_0x04!).Write(buffer); break;
				case PacketChatMessage p_0x05: new Mine.Net.Play.Serverbound.Packet("chat_message", p_0x05!).Write(buffer); break;
				case PacketChatPreview p_0x06: new Mine.Net.Play.Serverbound.Packet("chat_preview", p_0x06!).Write(buffer); break;
				case PacketClientCommand p_0x07: new Mine.Net.Play.Serverbound.Packet("client_command", p_0x07!).Write(buffer); break;
				case PacketSettings p_0x08: new Mine.Net.Play.Serverbound.Packet("settings", p_0x08!).Write(buffer); break;
				case PacketTabComplete p_0x09: new Mine.Net.Play.Serverbound.Packet("tab_complete", p_0x09!).Write(buffer); break;
				case PacketEnchantItem p_0x0A: new Mine.Net.Play.Serverbound.Packet("enchant_item", p_0x0A!).Write(buffer); break;
				case PacketWindowClick p_0x0B: new Mine.Net.Play.Serverbound.Packet("window_click", p_0x0B!).Write(buffer); break;
				case PacketCloseWindow p_0x0C: new Mine.Net.Play.Serverbound.Packet("close_window", p_0x0C!).Write(buffer); break;
				case PacketCustomPayload p_0x0D: new Mine.Net.Play.Serverbound.Packet("custom_payload", p_0x0D!).Write(buffer); break;
				case PacketEditBook p_0x0E: new Mine.Net.Play.Serverbound.Packet("edit_book", p_0x0E!).Write(buffer); break;
				case PacketQueryEntityNbt p_0x0F: new Mine.Net.Play.Serverbound.Packet("query_entity_nbt", p_0x0F!).Write(buffer); break;
				case PacketUseEntity p_0x10: new Mine.Net.Play.Serverbound.Packet("use_entity", p_0x10!).Write(buffer); break;
				case PacketGenerateStructure p_0x11: new Mine.Net.Play.Serverbound.Packet("generate_structure", p_0x11!).Write(buffer); break;
				case PacketKeepAlive p_0x12: new Mine.Net.Play.Serverbound.Packet("keep_alive", p_0x12!).Write(buffer); break;
				case PacketLockDifficulty p_0x13: new Mine.Net.Play.Serverbound.Packet("lock_difficulty", p_0x13!).Write(buffer); break;
				case PacketPosition p_0x14: new Mine.Net.Play.Serverbound.Packet("position", p_0x14!).Write(buffer); break;
				case PacketPositionLook p_0x15: new Mine.Net.Play.Serverbound.Packet("position_look", p_0x15!).Write(buffer); break;
				case PacketLook p_0x16: new Mine.Net.Play.Serverbound.Packet("look", p_0x16!).Write(buffer); break;
				case PacketFlying p_0x17: new Mine.Net.Play.Serverbound.Packet("flying", p_0x17!).Write(buffer); break;
				case PacketVehicleMove p_0x18: new Mine.Net.Play.Serverbound.Packet("vehicle_move", p_0x18!).Write(buffer); break;
				case PacketSteerBoat p_0x19: new Mine.Net.Play.Serverbound.Packet("steer_boat", p_0x19!).Write(buffer); break;
				case PacketPickItem p_0x1A: new Mine.Net.Play.Serverbound.Packet("pick_item", p_0x1A!).Write(buffer); break;
				case PacketCraftRecipeRequest p_0x1B: new Mine.Net.Play.Serverbound.Packet("craft_recipe_request", p_0x1B!).Write(buffer); break;
				case PacketAbilities p_0x1C: new Mine.Net.Play.Serverbound.Packet("abilities", p_0x1C!).Write(buffer); break;
				case PacketBlockDig p_0x1D: new Mine.Net.Play.Serverbound.Packet("block_dig", p_0x1D!).Write(buffer); break;
				case PacketEntityAction p_0x1E: new Mine.Net.Play.Serverbound.Packet("entity_action", p_0x1E!).Write(buffer); break;
				case PacketSteerVehicle p_0x1F: new Mine.Net.Play.Serverbound.Packet("steer_vehicle", p_0x1F!).Write(buffer); break;
				case PacketPong p_0x20: new Mine.Net.Play.Serverbound.Packet("pong", p_0x20!).Write(buffer); break;
				case PacketRecipeBook p_0x21: new Mine.Net.Play.Serverbound.Packet("recipe_book", p_0x21!).Write(buffer); break;
				case PacketDisplayedRecipe p_0x22: new Mine.Net.Play.Serverbound.Packet("displayed_recipe", p_0x22!).Write(buffer); break;
				case PacketNameItem p_0x23: new Mine.Net.Play.Serverbound.Packet("name_item", p_0x23!).Write(buffer); break;
				case PacketResourcePackReceive p_0x24: new Mine.Net.Play.Serverbound.Packet("resource_pack_receive", p_0x24!).Write(buffer); break;
				case PacketAdvancementTab p_0x25: new Mine.Net.Play.Serverbound.Packet("advancement_tab", p_0x25!).Write(buffer); break;
				case PacketSelectTrade p_0x26: new Mine.Net.Play.Serverbound.Packet("select_trade", p_0x26!).Write(buffer); break;
				case PacketSetBeaconEffect p_0x27: new Mine.Net.Play.Serverbound.Packet("set_beacon_effect", p_0x27!).Write(buffer); break;
				case PacketHeldItemSlot p_0x28: new Mine.Net.Play.Serverbound.Packet("held_item_slot", p_0x28!).Write(buffer); break;
				case PacketUpdateCommandBlock p_0x29: new Mine.Net.Play.Serverbound.Packet("update_command_block", p_0x29!).Write(buffer); break;
				case PacketUpdateCommandBlockMinecart p_0x2A: new Mine.Net.Play.Serverbound.Packet("update_command_block_minecart", p_0x2A!).Write(buffer); break;
				case PacketSetCreativeSlot p_0x2B: new Mine.Net.Play.Serverbound.Packet("set_creative_slot", p_0x2B!).Write(buffer); break;
				case PacketUpdateJigsawBlock p_0x2C: new Mine.Net.Play.Serverbound.Packet("update_jigsaw_block", p_0x2C!).Write(buffer); break;
				case PacketUpdateStructureBlock p_0x2D: new Mine.Net.Play.Serverbound.Packet("update_structure_block", p_0x2D!).Write(buffer); break;
				case PacketUpdateSign p_0x2E: new Mine.Net.Play.Serverbound.Packet("update_sign", p_0x2E!).Write(buffer); break;
				case PacketArmAnimation p_0x2F: new Mine.Net.Play.Serverbound.Packet("arm_animation", p_0x2F!).Write(buffer); break;
				case PacketSpectate p_0x30: new Mine.Net.Play.Serverbound.Packet("spectate", p_0x30!).Write(buffer); break;
				case PacketBlockPlace p_0x31: new Mine.Net.Play.Serverbound.Packet("block_place", p_0x31!).Write(buffer); break;
				case PacketUseItem p_0x32: new Mine.Net.Play.Serverbound.Packet("use_item", p_0x32!).Write(buffer); break;
				default: throw new Exception($"Play cannot write packet of type {packet.GetType().FullName}");
			}
		}
	}
}
namespace YAMNL
{
    public partial class PacketBuffer {
		#region Reading


		#endregion
		#region Writing


		#endregion
	}
}
namespace YAMNL.Play.Clientbound
{
    public class PacketSpawnEntity : IPacketPayload {
		public VarInt EntityId { get; set; }
		public UUID ObjectUUID { get; set; }
		public VarInt Type { get; set; }
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
		public sbyte Pitch { get; set; }
		public sbyte Yaw { get; set; }
		public sbyte HeadPitch { get; set; }
		public VarInt ObjectData { get; set; }
		public short VelocityX { get; set; }
		public short VelocityY { get; set; }
		public short VelocityZ { get; set; }
		public PacketSpawnEntity(VarInt @entityId, UUID @objectUUID, VarInt @type, double @x, double @y, double @z, sbyte @pitch, sbyte @yaw, sbyte @headPitch, VarInt @objectData, short @velocityX, short @velocityY, short @velocityZ) {
			EntityId = @entityId;
			ObjectUUID = @objectUUID;
			Type = @type;
			X = @x;
			Y = @y;
			Z = @z;
			Pitch = @pitch;
			Yaw = @yaw;
			HeadPitch = @headPitch;
			ObjectData = @objectData;
			VelocityX = @velocityX;
			VelocityY = @velocityY;
			VelocityZ = @velocityZ;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, EntityId);
			((Action<PacketBuffer, UUID>)((buffer, value) => buffer.WriteUUID(value)))(buffer, ObjectUUID);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Type);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, X);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, Y);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, Z);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, Pitch);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, Yaw);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, HeadPitch);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, ObjectData);
			((Action<PacketBuffer, short>)((buffer, value) => buffer.WriteI16(value)))(buffer, VelocityX);
			((Action<PacketBuffer, short>)((buffer, value) => buffer.WriteI16(value)))(buffer, VelocityY);
			((Action<PacketBuffer, short>)((buffer, value) => buffer.WriteI16(value)))(buffer, VelocityZ);
		}
		public static PacketSpawnEntity Read(PacketBuffer buffer ) {
			VarInt @entityId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			UUID @objectUUID = ((Func<PacketBuffer, UUID>)((buffer) => buffer.ReadUUID()))(buffer);
			VarInt @type = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			double @x = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @y = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @z = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			sbyte @pitch = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			sbyte @yaw = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			sbyte @headPitch = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			VarInt @objectData = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			short @velocityX = ((Func<PacketBuffer, short>)((buffer) => buffer.ReadI16()))(buffer);
			short @velocityY = ((Func<PacketBuffer, short>)((buffer) => buffer.ReadI16()))(buffer);
			short @velocityZ = ((Func<PacketBuffer, short>)((buffer) => buffer.ReadI16()))(buffer);
			return new PacketSpawnEntity(@entityId, @objectUUID, @type, @x, @y, @z, @pitch, @yaw, @headPitch, @objectData, @velocityX, @velocityY, @velocityZ);
		}
	}
	public class PacketSpawnEntityExperienceOrb : IPacketPayload {
		public VarInt EntityId { get; set; }
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
		public short Count { get; set; }
		public PacketSpawnEntityExperienceOrb(VarInt @entityId, double @x, double @y, double @z, short @count) {
			EntityId = @entityId;
			X = @x;
			Y = @y;
			Z = @z;
			Count = @count;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, EntityId);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, X);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, Y);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, Z);
			((Action<PacketBuffer, short>)((buffer, value) => buffer.WriteI16(value)))(buffer, Count);
		}
		public static PacketSpawnEntityExperienceOrb Read(PacketBuffer buffer ) {
			VarInt @entityId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			double @x = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @y = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @z = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			short @count = ((Func<PacketBuffer, short>)((buffer) => buffer.ReadI16()))(buffer);
			return new PacketSpawnEntityExperienceOrb(@entityId, @x, @y, @z, @count);
		}
	}
	public class PacketNamedEntitySpawn : IPacketPayload {
		public VarInt EntityId { get; set; }
		public UUID PlayerUUID { get; set; }
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
		public sbyte Yaw { get; set; }
		public sbyte Pitch { get; set; }
		public PacketNamedEntitySpawn(VarInt @entityId, UUID @playerUUID, double @x, double @y, double @z, sbyte @yaw, sbyte @pitch) {
			EntityId = @entityId;
			PlayerUUID = @playerUUID;
			X = @x;
			Y = @y;
			Z = @z;
			Yaw = @yaw;
			Pitch = @pitch;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, EntityId);
			((Action<PacketBuffer, UUID>)((buffer, value) => buffer.WriteUUID(value)))(buffer, PlayerUUID);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, X);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, Y);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, Z);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, Yaw);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, Pitch);
		}
		public static PacketNamedEntitySpawn Read(PacketBuffer buffer ) {
			VarInt @entityId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			UUID @playerUUID = ((Func<PacketBuffer, UUID>)((buffer) => buffer.ReadUUID()))(buffer);
			double @x = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @y = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @z = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			sbyte @yaw = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			sbyte @pitch = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			return new PacketNamedEntitySpawn(@entityId, @playerUUID, @x, @y, @z, @yaw, @pitch);
		}
	}
	public class PacketAnimation : IPacketPayload {
		public VarInt EntityId { get; set; }
		public byte Animation { get; set; }
		public PacketAnimation(VarInt @entityId, byte @animation) {
			EntityId = @entityId;
			Animation = @animation;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, EntityId);
			((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, Animation);
		}
		public static PacketAnimation Read(PacketBuffer buffer ) {
			VarInt @entityId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			byte @animation = ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer);
			return new PacketAnimation(@entityId, @animation);
		}
	}
	public class PacketStatistics : IPacketPayload {
		public class EntriesElementContainer {
			public VarInt CategoryId { get; set; }
			public VarInt StatisticId { get; set; }
			public VarInt Value { get; set; }
			public EntriesElementContainer(VarInt @categoryId, VarInt @statisticId, VarInt @value) {
				CategoryId = @categoryId;
				StatisticId = @statisticId;
				Value = @value;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, CategoryId);
				((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, StatisticId);
				((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Value);
			}
			public static EntriesElementContainer Read(PacketBuffer buffer ) {
				VarInt @categoryId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
				VarInt @statisticId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
				VarInt @value = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
				return new EntriesElementContainer(@categoryId, @statisticId, @value);
			}
		}
		public EntriesElementContainer[] Entries { get; set; }
		public PacketStatistics(EntriesElementContainer[] @entries) {
			Entries = @entries;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, EntriesElementContainer[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, EntriesElementContainer>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Entries);
		}
		public static PacketStatistics Read(PacketBuffer buffer ) {
			EntriesElementContainer[] @entries = ((Func<PacketBuffer, EntriesElementContainer[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, EntriesElementContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketStatistics.EntriesElementContainer.Read(buffer ))))))(buffer);
			return new PacketStatistics(@entries);
		}
	}
	public class PacketAdvancements : IPacketPayload {
		public class AdvancementMappingElementContainer {
			public class ValueContainer {
				public class DisplayDataContainer {
					public class FlagsBitfield {
						public uint Value { get; set; }
						public FlagsBitfield(uint value) {
							Value = value;
						}
						public uint Unused { 
    get { 
        return (uint)(((uint)Value! >> 3 & (536870911)));
    }
	set { 
        var val = value << 3; 
        var inv = ~val; var x = (uint)Value! & (uint)inv; 
        Value = (uint)((uint)x | (uint)val); 
    }
}
						public byte Hidden { 
    get { 
        return (byte)(((byte)Value! >> 2 & (1)));
    }
	set { 
        var val = value << 2; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (uint)((byte)x | (byte)val); 
    }
}
						public byte ShowToast { 
    get { 
        return (byte)(((byte)Value! >> 1 & (1)));
    }
	set { 
        var val = value << 1; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (uint)((byte)x | (byte)val); 
    }
}
						public byte HasBackgroundTexture { 
    get { 
        return (byte)(((byte)Value! >> 0 & (1)));
    }
	set { 
        var val = value << 0; 
        var inv = ~val; var x = (byte)Value! & (byte)inv; 
        Value = (uint)((byte)x | (byte)val); 
    }
}
					}
					public class BackgroundTextureSwitch {
						public object? Value { get; set; }
						public BackgroundTextureSwitch(object? value) {
							Value = value;
						}
						public void Write(PacketBuffer buffer, byte state) {
							switch (state) {
								case 1: ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string)this); break;
								default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
							}
						}
						public static BackgroundTextureSwitch Read(PacketBuffer buffer, byte state) {
							object? value = state switch {
								1 => ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer),
								_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
							};
							return new BackgroundTextureSwitch(value);
						}
						public static implicit operator string?(BackgroundTextureSwitch value) => (string?)value.Value;
						public static implicit operator BackgroundTextureSwitch?(string? value) => new BackgroundTextureSwitch(value);
					}
					public string Title { get; set; }
					public string Description { get; set; }
					public Slot Icon { get; set; }
					public VarInt FrameType { get; set; }
					public FlagsBitfield Flags { get; set; }
					public BackgroundTextureSwitch BackgroundTexture { get; set; }
					public float XCord { get; set; }
					public float YCord { get; set; }
					public DisplayDataContainer(string @title, string @description, Slot @icon, VarInt @frameType, FlagsBitfield @flags, BackgroundTextureSwitch @backgroundTexture, float @xCord, float @yCord) {
						Title = @title;
						Description = @description;
						Icon = @icon;
						FrameType = @frameType;
						Flags = @flags;
						BackgroundTexture = @backgroundTexture;
						XCord = @xCord;
						YCord = @yCord;
					}
					public void Write(PacketBuffer buffer ) {
						((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Title);
						((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Description);
						((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer )))(buffer, Icon);
						((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, FrameType);
						((Action<PacketBuffer, FlagsBitfield>)((buffer, value) => ((Action<PacketBuffer, uint>)((buffer, value) => buffer.WriteU32(value)))(buffer, value.Value)))(buffer, Flags);
						((Action<PacketBuffer, BackgroundTextureSwitch>)((buffer, value) => value.Write(buffer, Flags.HasBackgroundTexture)))(buffer, BackgroundTexture);
						((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, XCord);
						((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, YCord);
					}
					public static DisplayDataContainer Read(PacketBuffer buffer ) {
						string @title = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
						string @description = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
						Slot @icon = ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer )))(buffer);
						VarInt @frameType = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
						FlagsBitfield @flags = ((Func<PacketBuffer, FlagsBitfield>)((buffer) => new FlagsBitfield(((Func<PacketBuffer, uint>)((buffer) => buffer.ReadU32()))(buffer))))(buffer);
						BackgroundTextureSwitch @backgroundTexture = ((Func<PacketBuffer, BackgroundTextureSwitch>)((buffer) => BackgroundTextureSwitch.Read(buffer, @flags.HasBackgroundTexture)))(buffer);
						float @xCord = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
						float @yCord = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
						return new DisplayDataContainer(@title, @description, @icon, @frameType, @flags, @backgroundTexture, @xCord, @yCord);
					}
				}
				public class CriteriaElementContainer {
					public string Key { get; set; }
					public object? Value { get; set; }
					public CriteriaElementContainer(string @key, object? @value) {
						Key = @key;
						Value = @value;
					}
					public void Write(PacketBuffer buffer ) {
						((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Key);
						((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, Value);
					}
					public static CriteriaElementContainer Read(PacketBuffer buffer ) {
						string @key = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
						object? @value = ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer);
						return new CriteriaElementContainer(@key, @value);
					}
				}
				public string? ParentId { get; set; }
				public DisplayDataContainer? DisplayData { get; set; }
				public CriteriaElementContainer[] Criteria { get; set; }
				public string[][] Requirements { get; set; }
				public ValueContainer(string? @parentId, DisplayDataContainer? @displayData, CriteriaElementContainer[] @criteria, string[][] @requirements) {
					ParentId = @parentId;
					DisplayData = @displayData;
					Criteria = @criteria;
					Requirements = @requirements;
				}
				public void Write(PacketBuffer buffer ) {
					((Action<PacketBuffer, string?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))))))(buffer, ParentId);
					((Action<PacketBuffer, DisplayDataContainer?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, DisplayDataContainer>)((buffer, value) => value.Write(buffer ))))))(buffer, DisplayData);
					((Action<PacketBuffer, CriteriaElementContainer[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, CriteriaElementContainer>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Criteria);
					((Action<PacketBuffer, string[][]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, string[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Requirements);
				}
				public static ValueContainer Read(PacketBuffer buffer ) {
					string? @parentId = ((Func<PacketBuffer, string?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer);
					DisplayDataContainer? @displayData = ((Func<PacketBuffer, DisplayDataContainer?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, DisplayDataContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketAdvancements.AdvancementMappingElementContainer.ValueContainer.DisplayDataContainer.Read(buffer ))))))(buffer);
					CriteriaElementContainer[] @criteria = ((Func<PacketBuffer, CriteriaElementContainer[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, CriteriaElementContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketAdvancements.AdvancementMappingElementContainer.ValueContainer.CriteriaElementContainer.Read(buffer ))))))(buffer);
					string[][] @requirements = ((Func<PacketBuffer, string[][]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, string[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))))))))(buffer);
					return new ValueContainer(@parentId, @displayData, @criteria, @requirements);
				}
			}
			public string Key { get; set; }
			public ValueContainer Value { get; set; }
			public AdvancementMappingElementContainer(string @key, ValueContainer @value) {
				Key = @key;
				Value = @value;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Key);
				((Action<PacketBuffer, ValueContainer>)((buffer, value) => value.Write(buffer )))(buffer, Value);
			}
			public static AdvancementMappingElementContainer Read(PacketBuffer buffer ) {
				string @key = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
				ValueContainer @value = ((Func<PacketBuffer, ValueContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketAdvancements.AdvancementMappingElementContainer.ValueContainer.Read(buffer )))(buffer);
				return new AdvancementMappingElementContainer(@key, @value);
			}
		}
		public class ProgressMappingElementContainer {
			public class ValueElementContainer {
				public string CriterionIdentifier { get; set; }
				public long? CriterionProgress { get; set; }
				public ValueElementContainer(string @criterionIdentifier, long? @criterionProgress) {
					CriterionIdentifier = @criterionIdentifier;
					CriterionProgress = @criterionProgress;
				}
				public void Write(PacketBuffer buffer ) {
					((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, CriterionIdentifier);
					((Action<PacketBuffer, long?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value))))))(buffer, CriterionProgress);
				}
				public static ValueElementContainer Read(PacketBuffer buffer ) {
					string @criterionIdentifier = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
					long? @criterionProgress = ((Func<PacketBuffer, long?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64())))))(buffer);
					return new ValueElementContainer(@criterionIdentifier, @criterionProgress);
				}
			}
			public string Key { get; set; }
			public ValueElementContainer[] Value { get; set; }
			public ProgressMappingElementContainer(string @key, ValueElementContainer[] @value) {
				Key = @key;
				Value = @value;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Key);
				((Action<PacketBuffer, ValueElementContainer[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, ValueElementContainer>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Value);
			}
			public static ProgressMappingElementContainer Read(PacketBuffer buffer ) {
				string @key = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
				ValueElementContainer[] @value = ((Func<PacketBuffer, ValueElementContainer[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, ValueElementContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketAdvancements.ProgressMappingElementContainer.ValueElementContainer.Read(buffer ))))))(buffer);
				return new ProgressMappingElementContainer(@key, @value);
			}
		}
		public bool Reset { get; set; }
		public AdvancementMappingElementContainer[] AdvancementMapping { get; set; }
		public string[] Identifiers { get; set; }
		public ProgressMappingElementContainer[] ProgressMapping { get; set; }
		public PacketAdvancements(bool @reset, AdvancementMappingElementContainer[] @advancementMapping, string[] @identifiers, ProgressMappingElementContainer[] @progressMapping) {
			Reset = @reset;
			AdvancementMapping = @advancementMapping;
			Identifiers = @identifiers;
			ProgressMapping = @progressMapping;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, Reset);
			((Action<PacketBuffer, AdvancementMappingElementContainer[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, AdvancementMappingElementContainer>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, AdvancementMapping);
			((Action<PacketBuffer, string[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Identifiers);
			((Action<PacketBuffer, ProgressMappingElementContainer[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, ProgressMappingElementContainer>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, ProgressMapping);
		}
		public static PacketAdvancements Read(PacketBuffer buffer ) {
			bool @reset = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			AdvancementMappingElementContainer[] @advancementMapping = ((Func<PacketBuffer, AdvancementMappingElementContainer[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, AdvancementMappingElementContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketAdvancements.AdvancementMappingElementContainer.Read(buffer ))))))(buffer);
			string[] @identifiers = ((Func<PacketBuffer, string[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer);
			ProgressMappingElementContainer[] @progressMapping = ((Func<PacketBuffer, ProgressMappingElementContainer[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, ProgressMappingElementContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketAdvancements.ProgressMappingElementContainer.Read(buffer ))))))(buffer);
			return new PacketAdvancements(@reset, @advancementMapping, @identifiers, @progressMapping);
		}
	}
	public class PacketBlockBreakAnimation : IPacketPayload {
		public VarInt EntityId { get; set; }
		public PositionBitfield Location { get; set; }
		public sbyte DestroyStage { get; set; }
		public PacketBlockBreakAnimation(VarInt @entityId, PositionBitfield @location, sbyte @destroyStage) {
			EntityId = @entityId;
			Location = @location;
			DestroyStage = @destroyStage;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, EntityId);
			((Action<PacketBuffer, PositionBitfield>)((buffer, value) => ((Action<PacketBuffer, ulong>)((buffer, value) => buffer.WriteU64(value)))(buffer, value.Value)))(buffer, Location);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, DestroyStage);
		}
		public static PacketBlockBreakAnimation Read(PacketBuffer buffer ) {
			VarInt @entityId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			PositionBitfield @location = ((Func<PacketBuffer, PositionBitfield>)((buffer) => new PositionBitfield(((Func<PacketBuffer, ulong>)((buffer) => buffer.ReadU64()))(buffer))))(buffer);
			sbyte @destroyStage = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			return new PacketBlockBreakAnimation(@entityId, @location, @destroyStage);
		}
	}
	public class PacketTileEntityData : IPacketPayload {
		public PositionBitfield Location { get; set; }
		public VarInt Action { get; set; }
		public NbtCompound? NbtData { get; set; }
		public PacketTileEntityData(PositionBitfield @location, VarInt @action, NbtCompound? @nbtData) {
			Location = @location;
			Action = @action;
			NbtData = @nbtData;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, PositionBitfield>)((buffer, value) => ((Action<PacketBuffer, ulong>)((buffer, value) => buffer.WriteU64(value)))(buffer, value.Value)))(buffer, Location);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Action);
			((Action<PacketBuffer, NbtCompound?>)((buffer, value) => buffer.WriteOptionalNbt(value)))(buffer, NbtData);
		}
		public static PacketTileEntityData Read(PacketBuffer buffer ) {
			PositionBitfield @location = ((Func<PacketBuffer, PositionBitfield>)((buffer) => new PositionBitfield(((Func<PacketBuffer, ulong>)((buffer) => buffer.ReadU64()))(buffer))))(buffer);
			VarInt @action = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			NbtCompound? @nbtData = ((Func<PacketBuffer, NbtCompound?>)((buffer) => buffer.ReadOptionalNbt()))(buffer);
			return new PacketTileEntityData(@location, @action, @nbtData);
		}
	}
	public class PacketBlockAction : IPacketPayload {
		public PositionBitfield Location { get; set; }
		public byte Byte1 { get; set; }
		public byte Byte2 { get; set; }
		public VarInt BlockId { get; set; }
		public PacketBlockAction(PositionBitfield @location, byte @byte1, byte @byte2, VarInt @blockId) {
			Location = @location;
			Byte1 = @byte1;
			Byte2 = @byte2;
			BlockId = @blockId;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, PositionBitfield>)((buffer, value) => ((Action<PacketBuffer, ulong>)((buffer, value) => buffer.WriteU64(value)))(buffer, value.Value)))(buffer, Location);
			((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, Byte1);
			((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, Byte2);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, BlockId);
		}
		public static PacketBlockAction Read(PacketBuffer buffer ) {
			PositionBitfield @location = ((Func<PacketBuffer, PositionBitfield>)((buffer) => new PositionBitfield(((Func<PacketBuffer, ulong>)((buffer) => buffer.ReadU64()))(buffer))))(buffer);
			byte @byte1 = ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer);
			byte @byte2 = ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer);
			VarInt @blockId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketBlockAction(@location, @byte1, @byte2, @blockId);
		}
	}
	public class PacketBlockChange : IPacketPayload {
		public PositionBitfield Location { get; set; }
		public VarInt Type { get; set; }
		public PacketBlockChange(PositionBitfield @location, VarInt @type) {
			Location = @location;
			Type = @type;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, PositionBitfield>)((buffer, value) => ((Action<PacketBuffer, ulong>)((buffer, value) => buffer.WriteU64(value)))(buffer, value.Value)))(buffer, Location);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Type);
		}
		public static PacketBlockChange Read(PacketBuffer buffer ) {
			PositionBitfield @location = ((Func<PacketBuffer, PositionBitfield>)((buffer) => new PositionBitfield(((Func<PacketBuffer, ulong>)((buffer) => buffer.ReadU64()))(buffer))))(buffer);
			VarInt @type = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketBlockChange(@location, @type);
		}
	}
	public class PacketBossBar : IPacketPayload {
		public class TitleSwitch {
			public object? Value { get; set; }
			public TitleSwitch(object? value) {
				Value = value;
			}
			public void Write(PacketBuffer buffer, VarInt state) {
				switch (state) {
					case 0: ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string)this); break;
					case 3: ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string)this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static TitleSwitch Read(PacketBuffer buffer, VarInt state) {
				object? value = state.Value switch {
					0 => ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer),
					3 => ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new TitleSwitch(value);
			}
			public static implicit operator string?(TitleSwitch value) => (string?)value.Value;
			public static implicit operator TitleSwitch?(string? value) => new TitleSwitch(value);
		}
		public class HealthSwitch {
			public object? Value { get; set; }
			public HealthSwitch(object? value) {
				Value = value;
			}
			public void Write(PacketBuffer buffer, VarInt state) {
				switch (state) {
					case 0: ((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, (float)this); break;
					case 2: ((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, (float)this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static HealthSwitch Read(PacketBuffer buffer, VarInt state) {
				object? value = state.Value switch {
					0 => ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer),
					2 => ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new HealthSwitch(value);
			}
			public static implicit operator float?(HealthSwitch value) => (float?)value.Value;
			public static implicit operator HealthSwitch?(float? value) => new HealthSwitch(value);
		}
		public class ColorSwitch {
			public object? Value { get; set; }
			public ColorSwitch(object? value) {
				Value = value;
			}
			public void Write(PacketBuffer buffer, VarInt state) {
				switch (state) {
					case 0: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
					case 4: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static ColorSwitch Read(PacketBuffer buffer, VarInt state) {
				object? value = state.Value switch {
					0 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
					4 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new ColorSwitch(value);
			}
			public static implicit operator VarInt?(ColorSwitch value) => (VarInt?)value.Value;
			public static implicit operator ColorSwitch?(VarInt? value) => new ColorSwitch(value);
		}
		public class DividersSwitch {
			public object? Value { get; set; }
			public DividersSwitch(object? value) {
				Value = value;
			}
			public void Write(PacketBuffer buffer, VarInt state) {
				switch (state) {
					case 0: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
					case 4: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static DividersSwitch Read(PacketBuffer buffer, VarInt state) {
				object? value = state.Value switch {
					0 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
					4 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new DividersSwitch(value);
			}
			public static implicit operator VarInt?(DividersSwitch value) => (VarInt?)value.Value;
			public static implicit operator DividersSwitch?(VarInt? value) => new DividersSwitch(value);
		}
		public class FlagsSwitch {
			public object? Value { get; set; }
			public FlagsSwitch(object? value) {
				Value = value;
			}
			public void Write(PacketBuffer buffer, VarInt state) {
				switch (state) {
					case 0: ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, (byte)this); break;
					case 5: ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, (byte)this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static FlagsSwitch Read(PacketBuffer buffer, VarInt state) {
				object? value = state.Value switch {
					0 => ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer),
					5 => ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new FlagsSwitch(value);
			}
			public static implicit operator byte?(FlagsSwitch value) => (byte?)value.Value;
			public static implicit operator FlagsSwitch?(byte? value) => new FlagsSwitch(value);
		}
		public UUID EntityUUID { get; set; }
		public VarInt Action { get; set; }
		public TitleSwitch Title { get; set; }
		public HealthSwitch Health { get; set; }
		public ColorSwitch Color { get; set; }
		public DividersSwitch Dividers { get; set; }
		public FlagsSwitch Flags { get; set; }
		public PacketBossBar(UUID @entityUUID, VarInt @action, TitleSwitch @title, HealthSwitch @health, ColorSwitch @color, DividersSwitch @dividers, FlagsSwitch @flags) {
			EntityUUID = @entityUUID;
			Action = @action;
			Title = @title;
			Health = @health;
			Color = @color;
			Dividers = @dividers;
			Flags = @flags;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, UUID>)((buffer, value) => buffer.WriteUUID(value)))(buffer, EntityUUID);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Action);
			((Action<PacketBuffer, TitleSwitch>)((buffer, value) => value.Write(buffer, Action)))(buffer, Title);
			((Action<PacketBuffer, HealthSwitch>)((buffer, value) => value.Write(buffer, Action)))(buffer, Health);
			((Action<PacketBuffer, ColorSwitch>)((buffer, value) => value.Write(buffer, Action)))(buffer, Color);
			((Action<PacketBuffer, DividersSwitch>)((buffer, value) => value.Write(buffer, Action)))(buffer, Dividers);
			((Action<PacketBuffer, FlagsSwitch>)((buffer, value) => value.Write(buffer, Action)))(buffer, Flags);
		}
		public static PacketBossBar Read(PacketBuffer buffer ) {
			UUID @entityUUID = ((Func<PacketBuffer, UUID>)((buffer) => buffer.ReadUUID()))(buffer);
			VarInt @action = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			TitleSwitch @title = ((Func<PacketBuffer, TitleSwitch>)((buffer) => TitleSwitch.Read(buffer, @action)))(buffer);
			HealthSwitch @health = ((Func<PacketBuffer, HealthSwitch>)((buffer) => HealthSwitch.Read(buffer, @action)))(buffer);
			ColorSwitch @color = ((Func<PacketBuffer, ColorSwitch>)((buffer) => ColorSwitch.Read(buffer, @action)))(buffer);
			DividersSwitch @dividers = ((Func<PacketBuffer, DividersSwitch>)((buffer) => DividersSwitch.Read(buffer, @action)))(buffer);
			FlagsSwitch @flags = ((Func<PacketBuffer, FlagsSwitch>)((buffer) => FlagsSwitch.Read(buffer, @action)))(buffer);
			return new PacketBossBar(@entityUUID, @action, @title, @health, @color, @dividers, @flags);
		}
	}
	public class PacketDifficulty : IPacketPayload {
		public byte Difficulty { get; set; }
		public bool DifficultyLocked { get; set; }
		public PacketDifficulty(byte @difficulty, bool @difficultyLocked) {
			Difficulty = @difficulty;
			DifficultyLocked = @difficultyLocked;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, Difficulty);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, DifficultyLocked);
		}
		public static PacketDifficulty Read(PacketBuffer buffer ) {
			byte @difficulty = ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer);
			bool @difficultyLocked = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketDifficulty(@difficulty, @difficultyLocked);
		}
	}
	public class PacketChatPreview : IPacketPayload {
		public int QueryId { get; set; }
		public string? Message { get; set; }
		public PacketChatPreview(int @queryId, string? @message) {
			QueryId = @queryId;
			Message = @message;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, QueryId);
			((Action<PacketBuffer, string?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))))))(buffer, Message);
		}
		public static PacketChatPreview Read(PacketBuffer buffer ) {
			int @queryId = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			string? @message = ((Func<PacketBuffer, string?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer);
			return new PacketChatPreview(@queryId, @message);
		}
	}
	public class PacketTabComplete : IPacketPayload {
		public class MatchesElementContainer {
			public string Match { get; set; }
			public string? Tooltip { get; set; }
			public MatchesElementContainer(string @match, string? @tooltip) {
				Match = @match;
				Tooltip = @tooltip;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Match);
				((Action<PacketBuffer, string?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))))))(buffer, Tooltip);
			}
			public static MatchesElementContainer Read(PacketBuffer buffer ) {
				string @match = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
				string? @tooltip = ((Func<PacketBuffer, string?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer);
				return new MatchesElementContainer(@match, @tooltip);
			}
		}
		public VarInt TransactionId { get; set; }
		public VarInt Start { get; set; }
		public VarInt Length { get; set; }
		public MatchesElementContainer[] Matches { get; set; }
		public PacketTabComplete(VarInt @transactionId, VarInt @start, VarInt @length, MatchesElementContainer[] @matches) {
			TransactionId = @transactionId;
			Start = @start;
			Length = @length;
			Matches = @matches;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, TransactionId);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Start);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Length);
			((Action<PacketBuffer, MatchesElementContainer[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, MatchesElementContainer>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Matches);
		}
		public static PacketTabComplete Read(PacketBuffer buffer ) {
			VarInt @transactionId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @start = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @length = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			MatchesElementContainer[] @matches = ((Func<PacketBuffer, MatchesElementContainer[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, MatchesElementContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketTabComplete.MatchesElementContainer.Read(buffer ))))))(buffer);
			return new PacketTabComplete(@transactionId, @start, @length, @matches);
		}
	}
	public class PacketDeclareCommands : IPacketPayload {
		public CommandNode[] Nodes { get; set; }
		public VarInt RootIndex { get; set; }
		public PacketDeclareCommands(CommandNode[] @nodes, VarInt @rootIndex) {
			Nodes = @nodes;
			RootIndex = @rootIndex;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, CommandNode[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, CommandNode>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Nodes);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, RootIndex);
		}
		public static PacketDeclareCommands Read(PacketBuffer buffer ) {
			CommandNode[] @nodes = ((Func<PacketBuffer, CommandNode[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, CommandNode>)((buffer) => Mine.Net.CommandNode.Read(buffer ))))))(buffer);
			VarInt @rootIndex = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketDeclareCommands(@nodes, @rootIndex);
		}
	}
	public class PacketFacePlayer : IPacketPayload {
		public class EntityIdSwitch {
			public object? Value { get; set; }
			public EntityIdSwitch(object? value) {
				Value = value;
			}
			public void Write(PacketBuffer buffer, bool state) {
				switch (state) {
					case true: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static EntityIdSwitch Read(PacketBuffer buffer, bool state) {
				object? value = state switch {
					true => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new EntityIdSwitch(value);
			}
			public static implicit operator VarInt?(EntityIdSwitch value) => (VarInt?)value.Value;
			public static implicit operator EntityIdSwitch?(VarInt? value) => new EntityIdSwitch(value);
		}
		public class EntityFeetEyesSwitch {
			public object? Value { get; set; }
			public EntityFeetEyesSwitch(object? value) {
				Value = value;
			}
			public void Write(PacketBuffer buffer, bool state) {
				switch (state) {
					case true: ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string)this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static EntityFeetEyesSwitch Read(PacketBuffer buffer, bool state) {
				object? value = state switch {
					true => ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new EntityFeetEyesSwitch(value);
			}
			public static implicit operator string?(EntityFeetEyesSwitch value) => (string?)value.Value;
			public static implicit operator EntityFeetEyesSwitch?(string? value) => new EntityFeetEyesSwitch(value);
		}
		public VarInt FeetEyes { get; set; }
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
		public bool IsEntity { get; set; }
		public EntityIdSwitch EntityId { get; set; }
		public EntityFeetEyesSwitch EntityFeetEyes { get; set; }
		public PacketFacePlayer(VarInt @feetEyes, double @x, double @y, double @z, bool @isEntity, EntityIdSwitch @entityId, EntityFeetEyesSwitch @entityFeetEyes) {
			FeetEyes = @feetEyes;
			X = @x;
			Y = @y;
			Z = @z;
			IsEntity = @isEntity;
			EntityId = @entityId;
			EntityFeetEyes = @entityFeetEyes;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, FeetEyes);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, X);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, Y);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, Z);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, IsEntity);
			((Action<PacketBuffer, EntityIdSwitch>)((buffer, value) => value.Write(buffer, IsEntity)))(buffer, EntityId);
			((Action<PacketBuffer, EntityFeetEyesSwitch>)((buffer, value) => value.Write(buffer, IsEntity)))(buffer, EntityFeetEyes);
		}
		public static PacketFacePlayer Read(PacketBuffer buffer ) {
			VarInt @feetEyes = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			double @x = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @y = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @z = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			bool @isEntity = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			EntityIdSwitch @entityId = ((Func<PacketBuffer, EntityIdSwitch>)((buffer) => EntityIdSwitch.Read(buffer, @isEntity)))(buffer);
			EntityFeetEyesSwitch @entityFeetEyes = ((Func<PacketBuffer, EntityFeetEyesSwitch>)((buffer) => EntityFeetEyesSwitch.Read(buffer, @isEntity)))(buffer);
			return new PacketFacePlayer(@feetEyes, @x, @y, @z, @isEntity, @entityId, @entityFeetEyes);
		}
	}
	public class PacketNbtQueryResponse : IPacketPayload {
		public VarInt TransactionId { get; set; }
		public NbtCompound? Nbt { get; set; }
		public PacketNbtQueryResponse(VarInt @transactionId, NbtCompound? @nbt) {
			TransactionId = @transactionId;
			Nbt = @nbt;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, TransactionId);
			((Action<PacketBuffer, NbtCompound?>)((buffer, value) => buffer.WriteOptionalNbt(value)))(buffer, Nbt);
		}
		public static PacketNbtQueryResponse Read(PacketBuffer buffer ) {
			VarInt @transactionId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			NbtCompound? @nbt = ((Func<PacketBuffer, NbtCompound?>)((buffer) => buffer.ReadOptionalNbt()))(buffer);
			return new PacketNbtQueryResponse(@transactionId, @nbt);
		}
	}
	public class PacketMultiBlockChange : IPacketPayload {
		public class ChunkCoordinatesBitfield {
			public ulong Value { get; set; }
			public ChunkCoordinatesBitfield(ulong value) {
				Value = value;
			}
			public int X { 
    get { 
        return (int)(((int)Value! >> 42 & (4194303)));
    }
	set { 
        var val = value << 42; 
        var inv = ~val; var x = (int)Value! & (int)inv; 
        Value = (ulong)((uint)x | (uint)val); 
    }
}
			public int Z { 
    get { 
        return (int)(((int)Value! >> 20 & (4194303)));
    }
	set { 
        var val = value << 20; 
        var inv = ~val; var x = (int)Value! & (int)inv; 
        Value = (ulong)((uint)x | (uint)val); 
    }
}
			public int Y { 
    get { 
        return (int)(((int)Value! >> 0 & (1048575)));
    }
	set { 
        var val = value << 0; 
        var inv = ~val; var x = (int)Value! & (int)inv; 
        Value = (ulong)((uint)x | (uint)val); 
    }
}
		}
		public ChunkCoordinatesBitfield ChunkCoordinates { get; set; }
		public bool SuppressLightUpdates { get; set; }
		public VarInt[] Records { get; set; }
		public PacketMultiBlockChange(ChunkCoordinatesBitfield @chunkCoordinates, bool @suppressLightUpdates, VarInt[] @records) {
			ChunkCoordinates = @chunkCoordinates;
			SuppressLightUpdates = @suppressLightUpdates;
			Records = @records;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, ChunkCoordinatesBitfield>)((buffer, value) => ((Action<PacketBuffer, ulong>)((buffer, value) => buffer.WriteU64(value)))(buffer, value.Value)))(buffer, ChunkCoordinates);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, SuppressLightUpdates);
			((Action<PacketBuffer, VarInt[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Records);
		}
		public static PacketMultiBlockChange Read(PacketBuffer buffer ) {
			ChunkCoordinatesBitfield @chunkCoordinates = ((Func<PacketBuffer, ChunkCoordinatesBitfield>)((buffer) => new ChunkCoordinatesBitfield(((Func<PacketBuffer, ulong>)((buffer) => buffer.ReadU64()))(buffer))))(buffer);
			bool @suppressLightUpdates = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			VarInt[] @records = ((Func<PacketBuffer, VarInt[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			return new PacketMultiBlockChange(@chunkCoordinates, @suppressLightUpdates, @records);
		}
	}
	public class PacketCloseWindow : IPacketPayload {
		public byte WindowId { get; set; }
		public PacketCloseWindow(byte @windowId) {
			WindowId = @windowId;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, WindowId);
		}
		public static PacketCloseWindow Read(PacketBuffer buffer ) {
			byte @windowId = ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer);
			return new PacketCloseWindow(@windowId);
		}
	}
	public class PacketOpenWindow : IPacketPayload {
		public VarInt WindowId { get; set; }
		public VarInt InventoryType { get; set; }
		public string WindowTitle { get; set; }
		public PacketOpenWindow(VarInt @windowId, VarInt @inventoryType, string @windowTitle) {
			WindowId = @windowId;
			InventoryType = @inventoryType;
			WindowTitle = @windowTitle;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, WindowId);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, InventoryType);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, WindowTitle);
		}
		public static PacketOpenWindow Read(PacketBuffer buffer ) {
			VarInt @windowId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @inventoryType = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			string @windowTitle = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			return new PacketOpenWindow(@windowId, @inventoryType, @windowTitle);
		}
	}
	public class PacketWindowItems : IPacketPayload {
		public byte WindowId { get; set; }
		public VarInt StateId { get; set; }
		public Slot[] Items { get; set; }
		public Slot CarriedItem { get; set; }
		public PacketWindowItems(byte @windowId, VarInt @stateId, Slot[] @items, Slot @carriedItem) {
			WindowId = @windowId;
			StateId = @stateId;
			Items = @items;
			CarriedItem = @carriedItem;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, WindowId);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, StateId);
			((Action<PacketBuffer, Slot[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Items);
			((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer )))(buffer, CarriedItem);
		}
		public static PacketWindowItems Read(PacketBuffer buffer ) {
			byte @windowId = ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer);
			VarInt @stateId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			Slot[] @items = ((Func<PacketBuffer, Slot[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer ))))))(buffer);
			Slot @carriedItem = ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer )))(buffer);
			return new PacketWindowItems(@windowId, @stateId, @items, @carriedItem);
		}
	}
	public class PacketCraftProgressBar : IPacketPayload {
		public byte WindowId { get; set; }
		public short Property { get; set; }
		public short Value { get; set; }
		public PacketCraftProgressBar(byte @windowId, short @property, short @value) {
			WindowId = @windowId;
			Property = @property;
			Value = @value;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, WindowId);
			((Action<PacketBuffer, short>)((buffer, value) => buffer.WriteI16(value)))(buffer, Property);
			((Action<PacketBuffer, short>)((buffer, value) => buffer.WriteI16(value)))(buffer, Value);
		}
		public static PacketCraftProgressBar Read(PacketBuffer buffer ) {
			byte @windowId = ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer);
			short @property = ((Func<PacketBuffer, short>)((buffer) => buffer.ReadI16()))(buffer);
			short @value = ((Func<PacketBuffer, short>)((buffer) => buffer.ReadI16()))(buffer);
			return new PacketCraftProgressBar(@windowId, @property, @value);
		}
	}
	public class PacketSetSlot : IPacketPayload {
		public sbyte WindowId { get; set; }
		public VarInt StateId { get; set; }
		public short Slot { get; set; }
		public Slot Item { get; set; }
		public PacketSetSlot(sbyte @windowId, VarInt @stateId, short @slot, Slot @item) {
			WindowId = @windowId;
			StateId = @stateId;
			Slot = @slot;
			Item = @item;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, WindowId);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, StateId);
			((Action<PacketBuffer, short>)((buffer, value) => buffer.WriteI16(value)))(buffer, Slot);
			((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer )))(buffer, Item);
		}
		public static PacketSetSlot Read(PacketBuffer buffer ) {
			sbyte @windowId = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			VarInt @stateId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			short @slot = ((Func<PacketBuffer, short>)((buffer) => buffer.ReadI16()))(buffer);
			Slot @item = ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer )))(buffer);
			return new PacketSetSlot(@windowId, @stateId, @slot, @item);
		}
	}
	public class PacketSetCooldown : IPacketPayload {
		public VarInt ItemID { get; set; }
		public VarInt CooldownTicks { get; set; }
		public PacketSetCooldown(VarInt @itemID, VarInt @cooldownTicks) {
			ItemID = @itemID;
			CooldownTicks = @cooldownTicks;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, ItemID);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, CooldownTicks);
		}
		public static PacketSetCooldown Read(PacketBuffer buffer ) {
			VarInt @itemID = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @cooldownTicks = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketSetCooldown(@itemID, @cooldownTicks);
		}
	}
	public class PacketChatSuggestions : IPacketPayload {
		public VarInt Action { get; set; }
		public string[] Entries { get; set; }
		public PacketChatSuggestions(VarInt @action, string[] @entries) {
			Action = @action;
			Entries = @entries;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, Action);
			((Action<PacketBuffer, string[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Entries);
		}
		public static PacketChatSuggestions Read(PacketBuffer buffer ) {
			VarInt @action = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			string[] @entries = ((Func<PacketBuffer, string[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer);
			return new PacketChatSuggestions(@action, @entries);
		}
	}
	public class PacketCustomPayload : IPacketPayload {
		public string Channel { get; set; }
		public byte[] Data { get; set; }
		public PacketCustomPayload(string @channel, byte[] @data) {
			Channel = @channel;
			Data = @data;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Channel);
			((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteRestBuffer(value)))(buffer, Data);
		}
		public static PacketCustomPayload Read(PacketBuffer buffer ) {
			string @channel = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			byte[] @data = ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadRestBuffer()))(buffer);
			return new PacketCustomPayload(@channel, @data);
		}
	}
	public class PacketNamedSoundEffect : IPacketPayload {
		public string SoundName { get; set; }
		public VarInt SoundCategory { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }
		public float Volume { get; set; }
		public float Pitch { get; set; }
		public long Seed { get; set; }
		public PacketNamedSoundEffect(string @soundName, VarInt @soundCategory, int @x, int @y, int @z, float @volume, float @pitch, long @seed) {
			SoundName = @soundName;
			SoundCategory = @soundCategory;
			X = @x;
			Y = @y;
			Z = @z;
			Volume = @volume;
			Pitch = @pitch;
			Seed = @seed;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, SoundName);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, SoundCategory);
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, X);
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, Y);
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, Z);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, Volume);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, Pitch);
			((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value)))(buffer, Seed);
		}
		public static PacketNamedSoundEffect Read(PacketBuffer buffer ) {
			string @soundName = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			VarInt @soundCategory = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			int @x = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			int @y = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			int @z = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			float @volume = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			float @pitch = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			long @seed = ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64()))(buffer);
			return new PacketNamedSoundEffect(@soundName, @soundCategory, @x, @y, @z, @volume, @pitch, @seed);
		}
	}
	public class PacketHideMessage : IPacketPayload {
		public byte[] Signature { get; set; }
		public PacketHideMessage(byte[] @signature) {
			Signature = @signature;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Signature);
		}
		public static PacketHideMessage Read(PacketBuffer buffer ) {
			byte[] @signature = ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8())))))(buffer);
			return new PacketHideMessage(@signature);
		}
	}
	public class PacketKickDisconnect : IPacketPayload {
		public string Reason { get; set; }
		public PacketKickDisconnect(string @reason) {
			Reason = @reason;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, Reason);
		}
		public static PacketKickDisconnect Read(PacketBuffer buffer ) {
			string @reason = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			return new PacketKickDisconnect(@reason);
		}
	}
	public class PacketEntityStatus : IPacketPayload {
		public int EntityId { get; set; }
		public sbyte EntityStatus { get; set; }
		public PacketEntityStatus(int @entityId, sbyte @entityStatus) {
			EntityId = @entityId;
			EntityStatus = @entityStatus;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, EntityId);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, EntityStatus);
		}
		public static PacketEntityStatus Read(PacketBuffer buffer ) {
			int @entityId = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			sbyte @entityStatus = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			return new PacketEntityStatus(@entityId, @entityStatus);
		}
	}
	public class PacketExplosion : IPacketPayload {
		public class AffectedBlockOffsetsElementContainer {
			public sbyte X { get; set; }
			public sbyte Y { get; set; }
			public sbyte Z { get; set; }
			public AffectedBlockOffsetsElementContainer(sbyte @x, sbyte @y, sbyte @z) {
				X = @x;
				Y = @y;
				Z = @z;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, X);
				((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, Y);
				((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, Z);
			}
			public static AffectedBlockOffsetsElementContainer Read(PacketBuffer buffer ) {
				sbyte @x = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
				sbyte @y = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
				sbyte @z = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
				return new AffectedBlockOffsetsElementContainer(@x, @y, @z);
			}
		}
		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }
		public float Radius { get; set; }
		public AffectedBlockOffsetsElementContainer[] AffectedBlockOffsets { get; set; }
		public float PlayerMotionX { get; set; }
		public float PlayerMotionY { get; set; }
		public float PlayerMotionZ { get; set; }
		public PacketExplosion(float @x, float @y, float @z, float @radius, AffectedBlockOffsetsElementContainer[] @affectedBlockOffsets, float @playerMotionX, float @playerMotionY, float @playerMotionZ) {
			X = @x;
			Y = @y;
			Z = @z;
			Radius = @radius;
			AffectedBlockOffsets = @affectedBlockOffsets;
			PlayerMotionX = @playerMotionX;
			PlayerMotionY = @playerMotionY;
			PlayerMotionZ = @playerMotionZ;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, X);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, Y);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, Z);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, Radius);
			((Action<PacketBuffer, AffectedBlockOffsetsElementContainer[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, AffectedBlockOffsetsElementContainer>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, AffectedBlockOffsets);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, PlayerMotionX);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, PlayerMotionY);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, PlayerMotionZ);
		}
		public static PacketExplosion Read(PacketBuffer buffer ) {
			float @x = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			float @y = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			float @z = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			float @radius = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			AffectedBlockOffsetsElementContainer[] @affectedBlockOffsets = ((Func<PacketBuffer, AffectedBlockOffsetsElementContainer[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, AffectedBlockOffsetsElementContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketExplosion.AffectedBlockOffsetsElementContainer.Read(buffer ))))))(buffer);
			float @playerMotionX = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			float @playerMotionY = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			float @playerMotionZ = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			return new PacketExplosion(@x, @y, @z, @radius, @affectedBlockOffsets, @playerMotionX, @playerMotionY, @playerMotionZ);
		}
	}
	public class PacketUnloadChunk : IPacketPayload {
		public int ChunkX { get; set; }
		public int ChunkZ { get; set; }
		public PacketUnloadChunk(int @chunkX, int @chunkZ) {
			ChunkX = @chunkX;
			ChunkZ = @chunkZ;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, ChunkX);
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, ChunkZ);
		}
		public static PacketUnloadChunk Read(PacketBuffer buffer ) {
			int @chunkX = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			int @chunkZ = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			return new PacketUnloadChunk(@chunkX, @chunkZ);
		}
	}
	public class PacketGameStateChange : IPacketPayload {
		public byte Reason { get; set; }
		public float GameMode { get; set; }
		public PacketGameStateChange(byte @reason, float @gameMode) {
			Reason = @reason;
			GameMode = @gameMode;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, Reason);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, GameMode);
		}
		public static PacketGameStateChange Read(PacketBuffer buffer ) {
			byte @reason = ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer);
			float @gameMode = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			return new PacketGameStateChange(@reason, @gameMode);
		}
	}
	public class PacketOpenHorseWindow : IPacketPayload {
		public byte WindowId { get; set; }
		public VarInt NbSlots { get; set; }
		public int EntityId { get; set; }
		public PacketOpenHorseWindow(byte @windowId, VarInt @nbSlots, int @entityId) {
			WindowId = @windowId;
			NbSlots = @nbSlots;
			EntityId = @entityId;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, WindowId);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, NbSlots);
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, EntityId);
		}
		public static PacketOpenHorseWindow Read(PacketBuffer buffer ) {
			byte @windowId = ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer);
			VarInt @nbSlots = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			int @entityId = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			return new PacketOpenHorseWindow(@windowId, @nbSlots, @entityId);
		}
	}
	public class PacketKeepAlive : IPacketPayload {
		public long KeepAliveId { get; set; }
		public PacketKeepAlive(long @keepAliveId) {
			KeepAliveId = @keepAliveId;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value)))(buffer, KeepAliveId);
		}
		public static PacketKeepAlive Read(PacketBuffer buffer ) {
			long @keepAliveId = ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64()))(buffer);
			return new PacketKeepAlive(@keepAliveId);
		}
	}
	public class PacketMapChunk : IPacketPayload {
		public int X { get; set; }
		public int Z { get; set; }
		public NbtCompound Heightmaps { get; set; }
		public byte[] ChunkData { get; set; }
		public ChunkBlockEntity[] BlockEntities { get; set; }
		public bool TrustEdges { get; set; }
		public long[] SkyLightMask { get; set; }
		public long[] BlockLightMask { get; set; }
		public long[] EmptySkyLightMask { get; set; }
		public long[] EmptyBlockLightMask { get; set; }
		public byte[][] SkyLight { get; set; }
		public byte[][] BlockLight { get; set; }
		public PacketMapChunk(int @x, int @z, NbtCompound @heightmaps, byte[] @chunkData, ChunkBlockEntity[] @blockEntities, bool @trustEdges, long[] @skyLightMask, long[] @blockLightMask, long[] @emptySkyLightMask, long[] @emptyBlockLightMask, byte[][] @skyLight, byte[][] @blockLight) {
			X = @x;
			Z = @z;
			Heightmaps = @heightmaps;
			ChunkData = @chunkData;
			BlockEntities = @blockEntities;
			TrustEdges = @trustEdges;
			SkyLightMask = @skyLightMask;
			BlockLightMask = @blockLightMask;
			EmptySkyLightMask = @emptySkyLightMask;
			EmptyBlockLightMask = @emptyBlockLightMask;
			SkyLight = @skyLight;
			BlockLight = @blockLight;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, X);
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, Z);
			((Action<PacketBuffer, NbtCompound>)((buffer, value) => buffer.WriteNbt(value)))(buffer, Heightmaps);
			((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteBuffer(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, ChunkData);
			((Action<PacketBuffer, ChunkBlockEntity[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, ChunkBlockEntity>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, BlockEntities);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, TrustEdges);
			((Action<PacketBuffer, long[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, SkyLightMask);
			((Action<PacketBuffer, long[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, BlockLightMask);
			((Action<PacketBuffer, long[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, EmptySkyLightMask);
			((Action<PacketBuffer, long[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, EmptyBlockLightMask);
			((Action<PacketBuffer, byte[][]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, SkyLight);
			((Action<PacketBuffer, byte[][]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, BlockLight);
		}
		public static PacketMapChunk Read(PacketBuffer buffer ) {
			int @x = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			int @z = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			NbtCompound @heightmaps = ((Func<PacketBuffer, NbtCompound>)((buffer) => buffer.ReadNbt()))(buffer);
			byte[] @chunkData = ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadBuffer(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer))))(buffer);
			ChunkBlockEntity[] @blockEntities = ((Func<PacketBuffer, ChunkBlockEntity[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, ChunkBlockEntity>)((buffer) => Mine.Net.ChunkBlockEntity.Read(buffer ))))))(buffer);
			bool @trustEdges = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			long[] @skyLightMask = ((Func<PacketBuffer, long[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64())))))(buffer);
			long[] @blockLightMask = ((Func<PacketBuffer, long[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64())))))(buffer);
			long[] @emptySkyLightMask = ((Func<PacketBuffer, long[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64())))))(buffer);
			long[] @emptyBlockLightMask = ((Func<PacketBuffer, long[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64())))))(buffer);
			byte[][] @skyLight = ((Func<PacketBuffer, byte[][]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))))))))(buffer);
			byte[][] @blockLight = ((Func<PacketBuffer, byte[][]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))))))))(buffer);
			return new PacketMapChunk(@x, @z, @heightmaps, @chunkData, @blockEntities, @trustEdges, @skyLightMask, @blockLightMask, @emptySkyLightMask, @emptyBlockLightMask, @skyLight, @blockLight);
		}
	}
	public class PacketWorldEvent : IPacketPayload {
		public int EffectId { get; set; }
		public PositionBitfield Location { get; set; }
		public int Data { get; set; }
		public bool Global { get; set; }
		public PacketWorldEvent(int @effectId, PositionBitfield @location, int @data, bool @global) {
			EffectId = @effectId;
			Location = @location;
			Data = @data;
			Global = @global;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, EffectId);
			((Action<PacketBuffer, PositionBitfield>)((buffer, value) => ((Action<PacketBuffer, ulong>)((buffer, value) => buffer.WriteU64(value)))(buffer, value.Value)))(buffer, Location);
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, Data);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, Global);
		}
		public static PacketWorldEvent Read(PacketBuffer buffer ) {
			int @effectId = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			PositionBitfield @location = ((Func<PacketBuffer, PositionBitfield>)((buffer) => new PositionBitfield(((Func<PacketBuffer, ulong>)((buffer) => buffer.ReadU64()))(buffer))))(buffer);
			int @data = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			bool @global = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketWorldEvent(@effectId, @location, @data, @global);
		}
	}
	public class PacketWorldParticles : IPacketPayload {
		public VarInt ParticleId { get; set; }
		public bool LongDistance { get; set; }
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
		public float OffsetX { get; set; }
		public float OffsetY { get; set; }
		public float OffsetZ { get; set; }
		public float ParticleData { get; set; }
		public int Particles { get; set; }
		public ParticleDataSwitch Data { get; set; }
		public PacketWorldParticles(VarInt @particleId, bool @longDistance, double @x, double @y, double @z, float @offsetX, float @offsetY, float @offsetZ, float @particleData, int @particles, ParticleDataSwitch @data) {
			ParticleId = @particleId;
			LongDistance = @longDistance;
			X = @x;
			Y = @y;
			Z = @z;
			this.OffsetX = @offsetX;
			this.OffsetY = @offsetY;
			this.OffsetZ = @offsetZ;
			this.ParticleData = @particleData;
			this.Particles = @particles;
			this.Data = @data;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.ParticleId);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.LongDistance);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, this.X);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, this.Y);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, this.Z);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, this.OffsetX);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, this.OffsetY);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, this.OffsetZ);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, this.ParticleData);
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, this.Particles);
			((Action<PacketBuffer, ParticleDataSwitch>)((buffer, value) => value.Write(buffer, ParticleId)))(buffer, this.Data);
		}
		public static PacketWorldParticles Read(PacketBuffer buffer ) {
			VarInt @particleId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			bool @longDistance = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			double @x = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @y = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @z = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			float @offsetX = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			float @offsetY = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			float @offsetZ = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			float @particleData = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			int @particles = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			ParticleDataSwitch @data = ((Func<PacketBuffer, ParticleDataSwitch>)((buffer) => ParticleDataSwitch.Read(buffer, @particleId)))(buffer);
			return new PacketWorldParticles(@particleId, @longDistance, @x, @y, @z, @offsetX, @offsetY, @offsetZ, @particleData, @particles, @data);
		}
	}
	public class PacketUpdateLight : IPacketPayload {
		public VarInt ChunkX { get; set; }
		public VarInt ChunkZ { get; set; }
		public bool TrustEdges { get; set; }
		public long[] SkyLightMask { get; set; }
		public long[] BlockLightMask { get; set; }
		public long[] EmptySkyLightMask { get; set; }
		public long[] EmptyBlockLightMask { get; set; }
		public byte[][] SkyLight { get; set; }
		public byte[][] BlockLight { get; set; }
		public PacketUpdateLight(VarInt @chunkX, VarInt @chunkZ, bool @trustEdges, long[] @skyLightMask, long[] @blockLightMask, long[] @emptySkyLightMask, long[] @emptyBlockLightMask, byte[][] @skyLight, byte[][] @blockLight) {
			this.ChunkX = @chunkX;
			this.ChunkZ = @chunkZ;
			this.TrustEdges = @trustEdges;
			this.SkyLightMask = @skyLightMask;
			this.BlockLightMask = @blockLightMask;
			this.EmptySkyLightMask = @emptySkyLightMask;
			this.EmptyBlockLightMask = @emptyBlockLightMask;
			this.SkyLight = @skyLight;
			this.BlockLight = @blockLight;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.ChunkX);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.ChunkZ);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.TrustEdges);
			((Action<PacketBuffer, long[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.SkyLightMask);
			((Action<PacketBuffer, long[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.BlockLightMask);
			((Action<PacketBuffer, long[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.EmptySkyLightMask);
			((Action<PacketBuffer, long[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.EmptyBlockLightMask);
			((Action<PacketBuffer, byte[][]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.SkyLight);
			((Action<PacketBuffer, byte[][]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.BlockLight);
		}
		public static PacketUpdateLight Read(PacketBuffer buffer ) {
			VarInt @chunkX = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @chunkZ = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			bool @trustEdges = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			long[] @skyLightMask = ((Func<PacketBuffer, long[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64())))))(buffer);
			long[] @blockLightMask = ((Func<PacketBuffer, long[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64())))))(buffer);
			long[] @emptySkyLightMask = ((Func<PacketBuffer, long[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64())))))(buffer);
			long[] @emptyBlockLightMask = ((Func<PacketBuffer, long[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64())))))(buffer);
			byte[][] @skyLight = ((Func<PacketBuffer, byte[][]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))))))))(buffer);
			byte[][] @blockLight = ((Func<PacketBuffer, byte[][]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))))))))(buffer);
			return new PacketUpdateLight(@chunkX, @chunkZ, @trustEdges, @skyLightMask, @blockLightMask, @emptySkyLightMask, @emptyBlockLightMask, @skyLight, @blockLight);
		}
	}
	public class PacketLogin : IPacketPayload {
		public class DeathContainer {
			public string DimensionName { get; set; }
			public PositionBitfield Location { get; set; }
			public DeathContainer(string @dimensionName, PositionBitfield @location) {
				this.DimensionName = @dimensionName;
				this.Location = @location;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.DimensionName);
				((Action<PacketBuffer, PositionBitfield>)((buffer, value) => ((Action<PacketBuffer, ulong>)((buffer, value) => buffer.WriteU64(value)))(buffer, value.Value)))(buffer, this.Location);
			}
			public static DeathContainer Read(PacketBuffer buffer ) {
				string @dimensionName = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
				PositionBitfield @location = ((Func<PacketBuffer, PositionBitfield>)((buffer) => new PositionBitfield(((Func<PacketBuffer, ulong>)((buffer) => buffer.ReadU64()))(buffer))))(buffer);
				return new DeathContainer(@dimensionName, @location);
			}
		}
		public int EntityId { get; set; }
		public bool IsHardcore { get; set; }
		public byte GameMode { get; set; }
		public sbyte PreviousGameMode { get; set; }
		public string[] WorldNames { get; set; }
		public NbtCompound DimensionCodec { get; set; }
		public string WorldType { get; set; }
		public string WorldName { get; set; }
		public long HashedSeed { get; set; }
		public VarInt MaxPlayers { get; set; }
		public VarInt ViewDistance { get; set; }
		public VarInt SimulationDistance { get; set; }
		public bool ReducedDebugInfo { get; set; }
		public bool EnableRespawnScreen { get; set; }
		public bool IsDebug { get; set; }
		public bool IsFlat { get; set; }
		public DeathContainer? Death { get; set; }
		public PacketLogin(int @entityId, bool @isHardcore, byte @gameMode, sbyte @previousGameMode, string[] @worldNames, NbtCompound @dimensionCodec, string @worldType, string @worldName, long @hashedSeed, VarInt @maxPlayers, VarInt @viewDistance, VarInt @simulationDistance, bool @reducedDebugInfo, bool @enableRespawnScreen, bool @isDebug, bool @isFlat, DeathContainer? @death) {
			this.EntityId = @entityId;
			this.IsHardcore = @isHardcore;
			this.GameMode = @gameMode;
			this.PreviousGameMode = @previousGameMode;
			this.WorldNames = @worldNames;
			this.DimensionCodec = @dimensionCodec;
			this.WorldType = @worldType;
			this.WorldName = @worldName;
			this.HashedSeed = @hashedSeed;
			this.MaxPlayers = @maxPlayers;
			this.ViewDistance = @viewDistance;
			this.SimulationDistance = @simulationDistance;
			this.ReducedDebugInfo = @reducedDebugInfo;
			this.EnableRespawnScreen = @enableRespawnScreen;
			this.IsDebug = @isDebug;
			this.IsFlat = @isFlat;
			this.Death = @death;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, this.EntityId);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.IsHardcore);
			((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, this.GameMode);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.PreviousGameMode);
			((Action<PacketBuffer, string[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.WorldNames);
			((Action<PacketBuffer, NbtCompound>)((buffer, value) => buffer.WriteNbt(value)))(buffer, this.DimensionCodec);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.WorldType);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.WorldName);
			((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value)))(buffer, this.HashedSeed);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.MaxPlayers);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.ViewDistance);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.SimulationDistance);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.ReducedDebugInfo);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.EnableRespawnScreen);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.IsDebug);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.IsFlat);
			((Action<PacketBuffer, DeathContainer?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, DeathContainer>)((buffer, value) => value.Write(buffer ))))))(buffer, this.Death);
		}
		public static PacketLogin Read(PacketBuffer buffer ) {
			int @entityId = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			bool @isHardcore = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			byte @gameMode = ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer);
			sbyte @previousGameMode = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			string[] @worldNames = ((Func<PacketBuffer, string[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer);
			NbtCompound @dimensionCodec = ((Func<PacketBuffer, NbtCompound>)((buffer) => buffer.ReadNbt()))(buffer);
			string @worldType = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			string @worldName = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			long @hashedSeed = ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64()))(buffer);
			VarInt @maxPlayers = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @viewDistance = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @simulationDistance = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			bool @reducedDebugInfo = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			bool @enableRespawnScreen = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			bool @isDebug = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			bool @isFlat = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			DeathContainer? @death = ((Func<PacketBuffer, DeathContainer?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, DeathContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketLogin.DeathContainer.Read(buffer ))))))(buffer);
			return new PacketLogin(@entityId, @isHardcore, @gameMode, @previousGameMode, @worldNames, @dimensionCodec, @worldType, @worldName, @hashedSeed, @maxPlayers, @viewDistance, @simulationDistance, @reducedDebugInfo, @enableRespawnScreen, @isDebug, @isFlat, @death);
		}
	}
	public class PacketMap : IPacketPayload {
		public class IconsElementContainer {
			public VarInt Type { get; set; }
			public sbyte X { get; set; }
			public sbyte Z { get; set; }
			public byte Direction { get; set; }
			public string? DisplayName { get; set; }
			public IconsElementContainer(VarInt @type, sbyte @x, sbyte @z, byte @direction, string? @displayName) {
				this.Type = @type;
				this.X = @x;
				this.Z = @z;
				this.Direction = @direction;
				this.DisplayName = @displayName;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.Type);
				((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.X);
				((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.Z);
				((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, this.Direction);
				((Action<PacketBuffer, string?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))))))(buffer, this.DisplayName);
			}
			public static IconsElementContainer Read(PacketBuffer buffer ) {
				VarInt @type = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
				sbyte @x = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
				sbyte @z = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
				byte @direction = ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer);
				string? @displayName = ((Func<PacketBuffer, string?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer);
				return new IconsElementContainer(@type, @x, @z, @direction, @displayName);
			}
		}
		public class RowsSwitch {
			public object? Value { get; set; }
			public RowsSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, byte state) {
				switch (state) {
					case 0: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
					default: ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, (byte)Value); break;
				}
			}
			public static RowsSwitch Read(PacketBuffer buffer, byte state) {
				object? value = state switch {
					0 => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
					_ => ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer)
				};
				return new RowsSwitch(value);
			}
		}
		public class XSwitch {
			public object? Value { get; set; }
			public XSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, byte state) {
				switch (state) {
					case 0: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
					default: ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, (byte)Value); break;
				}
			}
			public static XSwitch Read(PacketBuffer buffer, byte state) {
				object? value = state switch {
					0 => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
					_ => ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer)
				};
				return new XSwitch(value);
			}
		}
		public class YSwitch {
			public object? Value { get; set; }
			public YSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, byte state) {
				switch (state) {
					case 0: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
					default: ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, (byte)Value); break;
				}
			}
			public static YSwitch Read(PacketBuffer buffer, byte state) {
				object? value = state switch {
					0 => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
					_ => ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer)
				};
				return new YSwitch(value);
			}
		}
		public class DataSwitch {
			public object? Value { get; set; }
			public DataSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, byte state) {
				switch (state) {
					case 0: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
					default: ((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteBuffer(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (byte[])Value); break;
				}
			}
			public static DataSwitch Read(PacketBuffer buffer, byte state) {
				object? value = state switch {
					0 => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
					_ => ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadBuffer(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer))))(buffer)
				};
				return new DataSwitch(value);
			}
		}
		public VarInt ItemDamage { get; set; }
		public sbyte Scale { get; set; }
		public bool Locked { get; set; }
		public IconsElementContainer[]? Icons { get; set; }
		public byte Columns { get; set; }
		public RowsSwitch Rows { get; set; }
		public XSwitch X { get; set; }
		public YSwitch Y { get; set; }
		public DataSwitch Data { get; set; }
		public PacketMap(VarInt @itemDamage, sbyte @scale, bool @locked, IconsElementContainer[]? @icons, byte @columns, RowsSwitch @rows, XSwitch @x, YSwitch @y, DataSwitch @data) {
			this.ItemDamage = @itemDamage;
			this.Scale = @scale;
			this.Locked = @locked;
			this.Icons = @icons;
			this.Columns = @columns;
			this.Rows = @rows;
			this.X = @x;
			this.Y = @y;
			this.Data = @data;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.ItemDamage);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.Scale);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.Locked);
			((Action<PacketBuffer, IconsElementContainer[]?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, IconsElementContainer[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, IconsElementContainer>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))))))(buffer, this.Icons);
			((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, this.Columns);
			((Action<PacketBuffer, RowsSwitch>)((buffer, value) => value.Write(buffer, Columns)))(buffer, this.Rows);
			((Action<PacketBuffer, XSwitch>)((buffer, value) => value.Write(buffer, Columns)))(buffer, this.X);
			((Action<PacketBuffer, YSwitch>)((buffer, value) => value.Write(buffer, Columns)))(buffer, this.Y);
			((Action<PacketBuffer, DataSwitch>)((buffer, value) => value.Write(buffer, Columns)))(buffer, this.Data);
		}
		public static PacketMap Read(PacketBuffer buffer ) {
			VarInt @itemDamage = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			sbyte @scale = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			bool @locked = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			IconsElementContainer[]? @icons = ((Func<PacketBuffer, IconsElementContainer[]?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, IconsElementContainer[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, IconsElementContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketMap.IconsElementContainer.Read(buffer )))))))))(buffer);
			byte @columns = ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer);
			RowsSwitch @rows = ((Func<PacketBuffer, RowsSwitch>)((buffer) => RowsSwitch.Read(buffer, @columns)))(buffer);
			XSwitch @x = ((Func<PacketBuffer, XSwitch>)((buffer) => XSwitch.Read(buffer, @columns)))(buffer);
			YSwitch @y = ((Func<PacketBuffer, YSwitch>)((buffer) => YSwitch.Read(buffer, @columns)))(buffer);
			DataSwitch @data = ((Func<PacketBuffer, DataSwitch>)((buffer) => DataSwitch.Read(buffer, @columns)))(buffer);
			return new PacketMap(@itemDamage, @scale, @locked, @icons, @columns, @rows, @x, @y, @data);
		}
	}
	public class PacketTradeList : IPacketPayload {
		public class TradesElementContainer {
			public Slot InputItem1 { get; set; }
			public Slot OutputItem { get; set; }
			public Slot InputItem2 { get; set; }
			public bool TradeDisabled { get; set; }
			public int NbTradeUses { get; set; }
			public int MaximumNbTradeUses { get; set; }
			public int Xp { get; set; }
			public int SpecialPrice { get; set; }
			public float PriceMultiplier { get; set; }
			public int Demand { get; set; }
			public TradesElementContainer(Slot @inputItem1, Slot @outputItem, Slot @inputItem2, bool @tradeDisabled, int @nbTradeUses, int @maximumNbTradeUses, int @xp, int @specialPrice, float @priceMultiplier, int @demand) {
				this.InputItem1 = @inputItem1;
				this.OutputItem = @outputItem;
				this.InputItem2 = @inputItem2;
				this.TradeDisabled = @tradeDisabled;
				this.NbTradeUses = @nbTradeUses;
				this.MaximumNbTradeUses = @maximumNbTradeUses;
				this.Xp = @xp;
				this.SpecialPrice = @specialPrice;
				this.PriceMultiplier = @priceMultiplier;
				this.Demand = @demand;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer )))(buffer, this.InputItem1);
				((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer )))(buffer, this.OutputItem);
				((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer )))(buffer, this.InputItem2);
				((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.TradeDisabled);
				((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, this.NbTradeUses);
				((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, this.MaximumNbTradeUses);
				((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, this.Xp);
				((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, this.SpecialPrice);
				((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, this.PriceMultiplier);
				((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, this.Demand);
			}
			public static TradesElementContainer Read(PacketBuffer buffer ) {
				Slot @inputItem1 = ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer )))(buffer);
				Slot @outputItem = ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer )))(buffer);
				Slot @inputItem2 = ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer )))(buffer);
				bool @tradeDisabled = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
				int @nbTradeUses = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
				int @maximumNbTradeUses = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
				int @xp = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
				int @specialPrice = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
				float @priceMultiplier = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
				int @demand = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
				return new TradesElementContainer(@inputItem1, @outputItem, @inputItem2, @tradeDisabled, @nbTradeUses, @maximumNbTradeUses, @xp, @specialPrice, @priceMultiplier, @demand);
			}
		}
		public VarInt WindowId { get; set; }
		public TradesElementContainer[] Trades { get; set; }
		public VarInt VillagerLevel { get; set; }
		public VarInt Experience { get; set; }
		public bool IsRegularVillager { get; set; }
		public bool CanRestock { get; set; }
		public PacketTradeList(VarInt @windowId, TradesElementContainer[] @trades, VarInt @villagerLevel, VarInt @experience, bool @isRegularVillager, bool @canRestock) {
			this.WindowId = @windowId;
			this.Trades = @trades;
			this.VillagerLevel = @villagerLevel;
			this.Experience = @experience;
			this.IsRegularVillager = @isRegularVillager;
			this.CanRestock = @canRestock;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.WindowId);
			((Action<PacketBuffer, TradesElementContainer[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, TradesElementContainer>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Trades);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.VillagerLevel);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.Experience);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.IsRegularVillager);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.CanRestock);
		}
		public static PacketTradeList Read(PacketBuffer buffer ) {
			VarInt @windowId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			TradesElementContainer[] @trades = ((Func<PacketBuffer, TradesElementContainer[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, TradesElementContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketTradeList.TradesElementContainer.Read(buffer ))))))(buffer);
			VarInt @villagerLevel = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @experience = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			bool @isRegularVillager = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			bool @canRestock = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketTradeList(@windowId, @trades, @villagerLevel, @experience, @isRegularVillager, @canRestock);
		}
	}
	public class PacketRelEntityMove : IPacketPayload {
		public VarInt EntityId { get; set; }
		public short DX { get; set; }
		public short DY { get; set; }
		public short DZ { get; set; }
		public bool OnGround { get; set; }
		public PacketRelEntityMove(VarInt @entityId, short @dX, short @dY, short @dZ, bool @onGround) {
			this.EntityId = @entityId;
			this.DX = @dX;
			this.DY = @dY;
			this.DZ = @dZ;
			this.OnGround = @onGround;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.EntityId);
			((Action<PacketBuffer, short>)((buffer, value) => buffer.WriteI16(value)))(buffer, this.DX);
			((Action<PacketBuffer, short>)((buffer, value) => buffer.WriteI16(value)))(buffer, this.DY);
			((Action<PacketBuffer, short>)((buffer, value) => buffer.WriteI16(value)))(buffer, this.DZ);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.OnGround);
		}
		public static PacketRelEntityMove Read(PacketBuffer buffer ) {
			VarInt @entityId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			short @dX = ((Func<PacketBuffer, short>)((buffer) => buffer.ReadI16()))(buffer);
			short @dY = ((Func<PacketBuffer, short>)((buffer) => buffer.ReadI16()))(buffer);
			short @dZ = ((Func<PacketBuffer, short>)((buffer) => buffer.ReadI16()))(buffer);
			bool @onGround = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketRelEntityMove(@entityId, @dX, @dY, @dZ, @onGround);
		}
	}
	public class PacketEntityMoveLook : IPacketPayload {
		public VarInt EntityId { get; set; }
		public short DX { get; set; }
		public short DY { get; set; }
		public short DZ { get; set; }
		public sbyte Yaw { get; set; }
		public sbyte Pitch { get; set; }
		public bool OnGround { get; set; }
		public PacketEntityMoveLook(VarInt @entityId, short @dX, short @dY, short @dZ, sbyte @yaw, sbyte @pitch, bool @onGround) {
			this.EntityId = @entityId;
			this.DX = @dX;
			this.DY = @dY;
			this.DZ = @dZ;
			this.Yaw = @yaw;
			this.Pitch = @pitch;
			this.OnGround = @onGround;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.EntityId);
			((Action<PacketBuffer, short>)((buffer, value) => buffer.WriteI16(value)))(buffer, this.DX);
			((Action<PacketBuffer, short>)((buffer, value) => buffer.WriteI16(value)))(buffer, this.DY);
			((Action<PacketBuffer, short>)((buffer, value) => buffer.WriteI16(value)))(buffer, this.DZ);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.Yaw);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.Pitch);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.OnGround);
		}
		public static PacketEntityMoveLook Read(PacketBuffer buffer ) {
			VarInt @entityId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			short @dX = ((Func<PacketBuffer, short>)((buffer) => buffer.ReadI16()))(buffer);
			short @dY = ((Func<PacketBuffer, short>)((buffer) => buffer.ReadI16()))(buffer);
			short @dZ = ((Func<PacketBuffer, short>)((buffer) => buffer.ReadI16()))(buffer);
			sbyte @yaw = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			sbyte @pitch = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			bool @onGround = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketEntityMoveLook(@entityId, @dX, @dY, @dZ, @yaw, @pitch, @onGround);
		}
	}
	public class PacketEntityLook : IPacketPayload {
		public VarInt EntityId { get; set; }
		public sbyte Yaw { get; set; }
		public sbyte Pitch { get; set; }
		public bool OnGround { get; set; }
		public PacketEntityLook(VarInt @entityId, sbyte @yaw, sbyte @pitch, bool @onGround) {
			this.EntityId = @entityId;
			this.Yaw = @yaw;
			this.Pitch = @pitch;
			this.OnGround = @onGround;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.EntityId);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.Yaw);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.Pitch);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.OnGround);
		}
		public static PacketEntityLook Read(PacketBuffer buffer ) {
			VarInt @entityId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			sbyte @yaw = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			sbyte @pitch = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			bool @onGround = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketEntityLook(@entityId, @yaw, @pitch, @onGround);
		}
	}
	public class PacketVehicleMove : IPacketPayload {
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
		public float Yaw { get; set; }
		public float Pitch { get; set; }
		public PacketVehicleMove(double @x, double @y, double @z, float @yaw, float @pitch) {
			this.X = @x;
			this.Y = @y;
			this.Z = @z;
			this.Yaw = @yaw;
			this.Pitch = @pitch;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, this.X);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, this.Y);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, this.Z);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, this.Yaw);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, this.Pitch);
		}
		public static PacketVehicleMove Read(PacketBuffer buffer ) {
			double @x = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @y = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @z = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			float @yaw = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			float @pitch = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			return new PacketVehicleMove(@x, @y, @z, @yaw, @pitch);
		}
	}
	public class PacketOpenBook : IPacketPayload {
		public VarInt Hand { get; set; }
		public PacketOpenBook(VarInt @hand) {
			this.Hand = @hand;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.Hand);
		}
		public static PacketOpenBook Read(PacketBuffer buffer ) {
			VarInt @hand = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketOpenBook(@hand);
		}
	}
	public class PacketOpenSignEntity : IPacketPayload {
		public PositionBitfield Location { get; set; }
		public PacketOpenSignEntity(PositionBitfield @location) {
			this.Location = @location;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, PositionBitfield>)((buffer, value) => ((Action<PacketBuffer, ulong>)((buffer, value) => buffer.WriteU64(value)))(buffer, value.Value)))(buffer, this.Location);
		}
		public static PacketOpenSignEntity Read(PacketBuffer buffer ) {
			PositionBitfield @location = ((Func<PacketBuffer, PositionBitfield>)((buffer) => new PositionBitfield(((Func<PacketBuffer, ulong>)((buffer) => buffer.ReadU64()))(buffer))))(buffer);
			return new PacketOpenSignEntity(@location);
		}
	}
	public class PacketCraftRecipeResponse : IPacketPayload {
		public sbyte WindowId { get; set; }
		public string Recipe { get; set; }
		public PacketCraftRecipeResponse(sbyte @windowId, string @recipe) {
			this.WindowId = @windowId;
			this.Recipe = @recipe;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.WindowId);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Recipe);
		}
		public static PacketCraftRecipeResponse Read(PacketBuffer buffer ) {
			sbyte @windowId = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			string @recipe = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			return new PacketCraftRecipeResponse(@windowId, @recipe);
		}
	}
	public class PacketAbilities : IPacketPayload {
		public sbyte Flags { get; set; }
		public float FlyingSpeed { get; set; }
		public float WalkingSpeed { get; set; }
		public PacketAbilities(sbyte @flags, float @flyingSpeed, float @walkingSpeed) {
			this.Flags = @flags;
			this.FlyingSpeed = @flyingSpeed;
			this.WalkingSpeed = @walkingSpeed;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.Flags);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, this.FlyingSpeed);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, this.WalkingSpeed);
		}
		public static PacketAbilities Read(PacketBuffer buffer ) {
			sbyte @flags = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			float @flyingSpeed = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			float @walkingSpeed = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			return new PacketAbilities(@flags, @flyingSpeed, @walkingSpeed);
		}
	}
	public class PacketPlayerChat : IPacketPayload {
		public class FilterTypeMaskSwitch {
			public object? Value { get; set; }
			public FilterTypeMaskSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, VarInt state) {
				switch (state) {
					case 2: ((Action<PacketBuffer, long[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (long[])this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static FilterTypeMaskSwitch Read(PacketBuffer buffer, VarInt state) {
				object? value = state.Value switch {
					2 => ((Func<PacketBuffer, long[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64())))))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new FilterTypeMaskSwitch(value);
			}
			public static implicit operator long[]?(FilterTypeMaskSwitch value) => (long[]?)value.Value;
			public static implicit operator FilterTypeMaskSwitch?(long[]? value) => new FilterTypeMaskSwitch(value);
		}
		public byte[]? MessageSignature { get; set; }
		public UUID SenderUuid { get; set; }
		public byte[] HeaderSignature { get; set; }
		public string PlainMessage { get; set; }
		public string? FormattedMessage { get; set; }
		public long Timestamp { get; set; }
		public long Salt { get; set; }
		public PreviousMessagesElement[] PreviousMessages { get; set; }
		public string? UnsignedContent { get; set; }
		public VarInt FilterType { get; set; }
		public FilterTypeMaskSwitch FilterTypeMask { get; set; }
		public VarInt Type { get; set; }
		public string NetworkName { get; set; }
		public string? NetworkTargetName { get; set; }
		public PacketPlayerChat(byte[]? @messageSignature, UUID @senderUuid, byte[] @headerSignature, string @plainMessage, string? @formattedMessage, long @timestamp, long @salt, PreviousMessagesElement[] @previousMessages, string? @unsignedContent, VarInt @filterType, FilterTypeMaskSwitch @filterTypeMask, VarInt @type, string @networkName, string? @networkTargetName) {
			this.MessageSignature = @messageSignature;
			this.SenderUuid = @senderUuid;
			this.HeaderSignature = @headerSignature;
			this.PlainMessage = @plainMessage;
			this.FormattedMessage = @formattedMessage;
			this.Timestamp = @timestamp;
			this.Salt = @salt;
			this.PreviousMessages = @previousMessages;
			this.UnsignedContent = @unsignedContent;
			this.FilterType = @filterType;
			this.FilterTypeMask = @filterTypeMask;
			this.Type = @type;
			this.NetworkName = @networkName;
			this.NetworkTargetName = @networkTargetName;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, byte[]?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))))))(buffer, this.MessageSignature);
			((Action<PacketBuffer, UUID>)((buffer, value) => buffer.WriteUUID(value)))(buffer, this.SenderUuid);
			((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.HeaderSignature);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.PlainMessage);
			((Action<PacketBuffer, string?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))))))(buffer, this.FormattedMessage);
			((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value)))(buffer, this.Timestamp);
			((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value)))(buffer, this.Salt);
			((Action<PacketBuffer, PreviousMessagesElement[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, PreviousMessagesElement>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.PreviousMessages);
			((Action<PacketBuffer, string?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))))))(buffer, this.UnsignedContent);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.FilterType);
			((Action<PacketBuffer, FilterTypeMaskSwitch>)((buffer, value) => value.Write(buffer, FilterType)))(buffer, this.FilterTypeMask);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.Type);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.NetworkName);
			((Action<PacketBuffer, string?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))))))(buffer, this.NetworkTargetName);
		}
		public static PacketPlayerChat Read(PacketBuffer buffer ) {
			byte[]? @messageSignature = ((Func<PacketBuffer, byte[]?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))))))))(buffer);
			UUID @senderUuid = ((Func<PacketBuffer, UUID>)((buffer) => buffer.ReadUUID()))(buffer);
			byte[] @headerSignature = ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8())))))(buffer);
			string @plainMessage = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			string? @formattedMessage = ((Func<PacketBuffer, string?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer);
			long @timestamp = ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64()))(buffer);
			long @salt = ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64()))(buffer);
			PreviousMessagesElement[] @previousMessages = ((Func<PacketBuffer, PreviousMessagesElement[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, PreviousMessagesElement>)((buffer) => Mine.Net.PreviousMessagesElement.Read(buffer ))))))(buffer);
			string? @unsignedContent = ((Func<PacketBuffer, string?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer);
			VarInt @filterType = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			FilterTypeMaskSwitch @filterTypeMask = ((Func<PacketBuffer, FilterTypeMaskSwitch>)((buffer) => FilterTypeMaskSwitch.Read(buffer, @filterType)))(buffer);
			VarInt @type = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			string @networkName = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			string? @networkTargetName = ((Func<PacketBuffer, string?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer);
			return new PacketPlayerChat(@messageSignature, @senderUuid, @headerSignature, @plainMessage, @formattedMessage, @timestamp, @salt, @previousMessages, @unsignedContent, @filterType, @filterTypeMask, @type, @networkName, @networkTargetName);
		}
	}
	public class PacketEndCombatEvent : IPacketPayload {
		public VarInt Duration { get; set; }
		public int EntityId { get; set; }
		public PacketEndCombatEvent(VarInt @duration, int @entityId) {
			this.Duration = @duration;
			this.EntityId = @entityId;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.Duration);
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, this.EntityId);
		}
		public static PacketEndCombatEvent Read(PacketBuffer buffer ) {
			VarInt @duration = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			int @entityId = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			return new PacketEndCombatEvent(@duration, @entityId);
		}
	}
	public class PacketEnterCombatEvent : IPacketPayload {
		public PacketEnterCombatEvent() {
		}
		public void Write(PacketBuffer buffer ) {
		}
		public static PacketEnterCombatEvent Read(PacketBuffer buffer ) {
			return new PacketEnterCombatEvent();
		}
	}
	public class PacketDeathCombatEvent : IPacketPayload {
		public VarInt PlayerId { get; set; }
		public int EntityId { get; set; }
		public string Message { get; set; }
		public PacketDeathCombatEvent(VarInt @playerId, int @entityId, string @message) {
			this.PlayerId = @playerId;
			this.EntityId = @entityId;
			this.Message = @message;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.PlayerId);
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, this.EntityId);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Message);
		}
		public static PacketDeathCombatEvent Read(PacketBuffer buffer ) {
			VarInt @playerId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			int @entityId = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			string @message = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			return new PacketDeathCombatEvent(@playerId, @entityId, @message);
		}
	}
	public class PacketPlayerInfo : IPacketPayload {
		public class DataElementContainer {
			public class NameSwitch {
				public object? Value { get; set; }
				public NameSwitch(object? value) {
					this.Value = value;
				}
				public void Write(PacketBuffer buffer, VarInt state, VarInt @action) {
					switch (state) {
						case 0: ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string)this); break;
						default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
					}
				}
				public static NameSwitch Read(PacketBuffer buffer, VarInt state, VarInt @action) {
					object? value = state.Value switch {
						0 => ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer),
						_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
					};
					return new NameSwitch(value);
				}
				public static implicit operator string?(NameSwitch value) => (string?)value.Value;
				public static implicit operator NameSwitch?(string? value) => new NameSwitch(value);
			}
			public class PropertiesSwitch {
				public class PropertiesSwitchState0ElementContainer {
					public string Name { get; set; }
					public string Value { get; set; }
					public string? Signature { get; set; }
					public PropertiesSwitchState0ElementContainer(string @name, string @value, string? @signature) {
						this.Name = @name;
						this.Value = @value;
						this.Signature = @signature;
					}
					public void Write(PacketBuffer buffer ) {
						((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Name);
						((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Value);
						((Action<PacketBuffer, string?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))))))(buffer, this.Signature);
					}
					public static PropertiesSwitchState0ElementContainer Read(PacketBuffer buffer ) {
						string @name = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
						string @value = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
						string? @signature = ((Func<PacketBuffer, string?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer);
						return new PropertiesSwitchState0ElementContainer(@name, @value, @signature);
					}
				}
				public object? Value { get; set; }
				public PropertiesSwitch(object? value) {
					this.Value = value;
				}
				public void Write(PacketBuffer buffer, VarInt state, VarInt @action) {
					switch (state) {
						case 0: ((Action<PacketBuffer, PropertiesSwitchState0ElementContainer[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, PropertiesSwitchState0ElementContainer>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (PropertiesSwitchState0ElementContainer[])this); break;
						default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
					}
				}
				public static PropertiesSwitch Read(PacketBuffer buffer, VarInt state, VarInt @action) {
					object? value = state.Value switch {
						0 => ((Func<PacketBuffer, PropertiesSwitchState0ElementContainer[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, PropertiesSwitchState0ElementContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketPlayerInfo.DataElementContainer.PropertiesSwitch.PropertiesSwitchState0ElementContainer.Read(buffer ))))))(buffer),
						_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
					};
					return new PropertiesSwitch(value);
				}
				public static implicit operator PropertiesSwitchState0ElementContainer[]?(PropertiesSwitch value) => (PropertiesSwitchState0ElementContainer[]?)value.Value;
				public static implicit operator PropertiesSwitch?(PropertiesSwitchState0ElementContainer[]? value) => new PropertiesSwitch(value);
			}
			public class GamemodeSwitch {
				public object? Value { get; set; }
				public GamemodeSwitch(object? value) {
					this.Value = value;
				}
				public void Write(PacketBuffer buffer, VarInt state, VarInt @action) {
					switch (state) {
						case 0: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
						case 1: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
						default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
					}
				}
				public static GamemodeSwitch Read(PacketBuffer buffer, VarInt state, VarInt @action) {
					object? value = state.Value switch {
						0 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
						1 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
						_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
					};
					return new GamemodeSwitch(value);
				}
				public static implicit operator VarInt?(GamemodeSwitch value) => (VarInt?)value.Value;
				public static implicit operator GamemodeSwitch?(VarInt? value) => new GamemodeSwitch(value);
			}
			public class PingSwitch {
				public object? Value { get; set; }
				public PingSwitch(object? value) {
					this.Value = value;
				}
				public void Write(PacketBuffer buffer, VarInt state, VarInt @action) {
					switch (state) {
						case 0: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
						case 2: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
						default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
					}
				}
				public static PingSwitch Read(PacketBuffer buffer, VarInt state, VarInt @action) {
					object? value = state.Value switch {
						0 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
						2 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
						_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
					};
					return new PingSwitch(value);
				}
				public static implicit operator VarInt?(PingSwitch value) => (VarInt?)value.Value;
				public static implicit operator PingSwitch?(VarInt? value) => new PingSwitch(value);
			}
			public class DisplayNameSwitch {
				public object? Value { get; set; }
				public DisplayNameSwitch(object? value) {
					this.Value = value;
				}
				public void Write(PacketBuffer buffer, VarInt state, VarInt @action) {
					switch (state) {
						case 0: ((Action<PacketBuffer, string?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))))))(buffer, (string?)this); break;
						case 3: ((Action<PacketBuffer, string?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))))))(buffer, (string?)this); break;
						default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
					}
				}
				public static DisplayNameSwitch Read(PacketBuffer buffer, VarInt state, VarInt @action) {
					object? value = state.Value switch {
						0 => ((Func<PacketBuffer, string?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer),
						3 => ((Func<PacketBuffer, string?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer),
						_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
					};
					return new DisplayNameSwitch(value);
				}
				public static implicit operator string?(DisplayNameSwitch value) => (string?)value.Value;
				public static implicit operator DisplayNameSwitch?(string? value) => new DisplayNameSwitch(value);
			}
			public class CryptoSwitch {
				public class CryptoSwitchState0Container {
					public long Timestamp { get; set; }
					public byte[] PublicKey { get; set; }
					public byte[] Signature { get; set; }
					public CryptoSwitchState0Container(long @timestamp, byte[] @publicKey, byte[] @signature) {
						this.Timestamp = @timestamp;
						this.PublicKey = @publicKey;
						this.Signature = @signature;
					}
					public void Write(PacketBuffer buffer ) {
						((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value)))(buffer, this.Timestamp);
						((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteBuffer(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.PublicKey);
						((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteBuffer(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Signature);
					}
					public static CryptoSwitchState0Container Read(PacketBuffer buffer ) {
						long @timestamp = ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64()))(buffer);
						byte[] @publicKey = ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadBuffer(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer))))(buffer);
						byte[] @signature = ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadBuffer(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer))))(buffer);
						return new CryptoSwitchState0Container(@timestamp, @publicKey, @signature);
					}
				}
				public object? Value { get; set; }
				public CryptoSwitch(object? value) {
					this.Value = value;
				}
				public void Write(PacketBuffer buffer, VarInt state, VarInt @action) {
					switch (state) {
						case 0: ((Action<PacketBuffer, CryptoSwitchState0Container?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, CryptoSwitchState0Container>)((buffer, value) => value.Write(buffer ))))))(buffer, (CryptoSwitchState0Container?)this); break;
						default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
					}
				}
				public static CryptoSwitch Read(PacketBuffer buffer, VarInt state, VarInt @action) {
					object? value = state.Value switch {
						0 => ((Func<PacketBuffer, CryptoSwitchState0Container?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, CryptoSwitchState0Container>)((buffer) => Mine.Net.Play.Clientbound.PacketPlayerInfo.DataElementContainer.CryptoSwitch.CryptoSwitchState0Container.Read(buffer ))))))(buffer),
						_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
					};
					return new CryptoSwitch(value);
				}
				public static implicit operator CryptoSwitchState0Container?(CryptoSwitch value) => (CryptoSwitchState0Container?)value.Value;
				public static implicit operator CryptoSwitch?(CryptoSwitchState0Container? value) => new CryptoSwitch(value);
			}
			public UUID UUID { get; set; }
			public NameSwitch Name { get; set; }
			public PropertiesSwitch Properties { get; set; }
			public GamemodeSwitch Gamemode { get; set; }
			public PingSwitch Ping { get; set; }
			public DisplayNameSwitch DisplayName { get; set; }
			public CryptoSwitch Crypto { get; set; }
			public DataElementContainer(UUID @uUID, NameSwitch @name, PropertiesSwitch @properties, GamemodeSwitch @gamemode, PingSwitch @ping, DisplayNameSwitch @displayName, CryptoSwitch @crypto) {
				this.UUID = @uUID;
				this.Name = @name;
				this.Properties = @properties;
				this.Gamemode = @gamemode;
				this.Ping = @ping;
				this.DisplayName = @displayName;
				this.Crypto = @crypto;
			}
			public void Write(PacketBuffer buffer , VarInt @action) {
				((Action<PacketBuffer, UUID>)((buffer, value) => buffer.WriteUUID(value)))(buffer, this.UUID);
				((Action<PacketBuffer, NameSwitch>)((buffer, value) => value.Write(buffer, @action, @action)))(buffer, this.Name);
				((Action<PacketBuffer, PropertiesSwitch>)((buffer, value) => value.Write(buffer, @action, @action)))(buffer, this.Properties);
				((Action<PacketBuffer, GamemodeSwitch>)((buffer, value) => value.Write(buffer, @action, @action)))(buffer, this.Gamemode);
				((Action<PacketBuffer, PingSwitch>)((buffer, value) => value.Write(buffer, @action, @action)))(buffer, this.Ping);
				((Action<PacketBuffer, DisplayNameSwitch>)((buffer, value) => value.Write(buffer, @action, @action)))(buffer, this.DisplayName);
				((Action<PacketBuffer, CryptoSwitch>)((buffer, value) => value.Write(buffer, @action, @action)))(buffer, this.Crypto);
			}
			public static DataElementContainer Read(PacketBuffer buffer , VarInt @action) {
				UUID @uUID = ((Func<PacketBuffer, UUID>)((buffer) => buffer.ReadUUID()))(buffer);
				NameSwitch @name = ((Func<PacketBuffer, NameSwitch>)((buffer) => NameSwitch.Read(buffer, @action, @action)))(buffer);
				PropertiesSwitch @properties = ((Func<PacketBuffer, PropertiesSwitch>)((buffer) => PropertiesSwitch.Read(buffer, @action, @action)))(buffer);
				GamemodeSwitch @gamemode = ((Func<PacketBuffer, GamemodeSwitch>)((buffer) => GamemodeSwitch.Read(buffer, @action, @action)))(buffer);
				PingSwitch @ping = ((Func<PacketBuffer, PingSwitch>)((buffer) => PingSwitch.Read(buffer, @action, @action)))(buffer);
				DisplayNameSwitch @displayName = ((Func<PacketBuffer, DisplayNameSwitch>)((buffer) => DisplayNameSwitch.Read(buffer, @action, @action)))(buffer);
				CryptoSwitch @crypto = ((Func<PacketBuffer, CryptoSwitch>)((buffer) => CryptoSwitch.Read(buffer, @action, @action)))(buffer);
				return new DataElementContainer(@uUID, @name, @properties, @gamemode, @ping, @displayName, @crypto);
			}
		}
		public VarInt Action { get; set; }
		public DataElementContainer[] Data { get; set; }
		public PacketPlayerInfo(VarInt @action, DataElementContainer[] @data) {
			this.Action = @action;
			this.Data = @data;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.Action);
			((Action<PacketBuffer, DataElementContainer[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, DataElementContainer>)((buffer, value) => value.Write(buffer , Action))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Data);
		}
		public static PacketPlayerInfo Read(PacketBuffer buffer ) {
			VarInt @action = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			DataElementContainer[] @data = ((Func<PacketBuffer, DataElementContainer[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, DataElementContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketPlayerInfo.DataElementContainer.Read(buffer , @action))))))(buffer);
			return new PacketPlayerInfo(@action, @data);
		}
	}
	public class PacketPosition : IPacketPayload {
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
		public float Yaw { get; set; }
		public float Pitch { get; set; }
		public sbyte Flags { get; set; }
		public VarInt TeleportId { get; set; }
		public bool DismountVehicle { get; set; }
		public PacketPosition(double @x, double @y, double @z, float @yaw, float @pitch, sbyte @flags, VarInt @teleportId, bool @dismountVehicle) {
			this.X = @x;
			this.Y = @y;
			this.Z = @z;
			this.Yaw = @yaw;
			this.Pitch = @pitch;
			this.Flags = @flags;
			this.TeleportId = @teleportId;
			this.DismountVehicle = @dismountVehicle;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, this.X);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, this.Y);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, this.Z);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, this.Yaw);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, this.Pitch);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.Flags);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.TeleportId);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.DismountVehicle);
		}
		public static PacketPosition Read(PacketBuffer buffer ) {
			double @x = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @y = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @z = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			float @yaw = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			float @pitch = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			sbyte @flags = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			VarInt @teleportId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			bool @dismountVehicle = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketPosition(@x, @y, @z, @yaw, @pitch, @flags, @teleportId, @dismountVehicle);
		}
	}
	public class PacketUnlockRecipes : IPacketPayload {
		public class Recipes2Switch {
			public object? Value { get; set; }
			public Recipes2Switch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, VarInt state) {
				switch (state) {
					case 0: ((Action<PacketBuffer, string[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string[])this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static Recipes2Switch Read(PacketBuffer buffer, VarInt state) {
				object? value = state.Value switch {
					0 => ((Func<PacketBuffer, string[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new Recipes2Switch(value);
			}
			public static implicit operator string[]?(Recipes2Switch value) => (string[]?)value.Value;
			public static implicit operator Recipes2Switch?(string[]? value) => new Recipes2Switch(value);
		}
		public VarInt Action { get; set; }
		public bool CraftingBookOpen { get; set; }
		public bool FilteringCraftable { get; set; }
		public bool SmeltingBookOpen { get; set; }
		public bool FilteringSmeltable { get; set; }
		public bool BlastFurnaceOpen { get; set; }
		public bool FilteringBlastFurnace { get; set; }
		public bool SmokerBookOpen { get; set; }
		public bool FilteringSmoker { get; set; }
		public string[] Recipes1 { get; set; }
		public Recipes2Switch Recipes2 { get; set; }
		public PacketUnlockRecipes(VarInt @action, bool @craftingBookOpen, bool @filteringCraftable, bool @smeltingBookOpen, bool @filteringSmeltable, bool @blastFurnaceOpen, bool @filteringBlastFurnace, bool @smokerBookOpen, bool @filteringSmoker, string[] @recipes1, Recipes2Switch @recipes2) {
			this.Action = @action;
			this.CraftingBookOpen = @craftingBookOpen;
			this.FilteringCraftable = @filteringCraftable;
			this.SmeltingBookOpen = @smeltingBookOpen;
			this.FilteringSmeltable = @filteringSmeltable;
			this.BlastFurnaceOpen = @blastFurnaceOpen;
			this.FilteringBlastFurnace = @filteringBlastFurnace;
			this.SmokerBookOpen = @smokerBookOpen;
			this.FilteringSmoker = @filteringSmoker;
			this.Recipes1 = @recipes1;
			this.Recipes2 = @recipes2;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.Action);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.CraftingBookOpen);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.FilteringCraftable);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.SmeltingBookOpen);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.FilteringSmeltable);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.BlastFurnaceOpen);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.FilteringBlastFurnace);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.SmokerBookOpen);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.FilteringSmoker);
			((Action<PacketBuffer, string[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Recipes1);
			((Action<PacketBuffer, Recipes2Switch>)((buffer, value) => value.Write(buffer, Action)))(buffer, this.Recipes2);
		}
		public static PacketUnlockRecipes Read(PacketBuffer buffer ) {
			VarInt @action = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			bool @craftingBookOpen = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			bool @filteringCraftable = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			bool @smeltingBookOpen = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			bool @filteringSmeltable = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			bool @blastFurnaceOpen = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			bool @filteringBlastFurnace = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			bool @smokerBookOpen = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			bool @filteringSmoker = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			string[] @recipes1 = ((Func<PacketBuffer, string[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer);
			Recipes2Switch @recipes2 = ((Func<PacketBuffer, Recipes2Switch>)((buffer) => Recipes2Switch.Read(buffer, @action)))(buffer);
			return new PacketUnlockRecipes(@action, @craftingBookOpen, @filteringCraftable, @smeltingBookOpen, @filteringSmeltable, @blastFurnaceOpen, @filteringBlastFurnace, @smokerBookOpen, @filteringSmoker, @recipes1, @recipes2);
		}
	}
	public class PacketEntityDestroy : IPacketPayload {
		public VarInt[] EntityIds { get; set; }
		public PacketEntityDestroy(VarInt[] @entityIds) {
			this.EntityIds = @entityIds;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.EntityIds);
		}
		public static PacketEntityDestroy Read(PacketBuffer buffer ) {
			VarInt[] @entityIds = ((Func<PacketBuffer, VarInt[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			return new PacketEntityDestroy(@entityIds);
		}
	}
	public class PacketRemoveEntityEffect : IPacketPayload {
		public VarInt EntityId { get; set; }
		public VarInt EffectId { get; set; }
		public PacketRemoveEntityEffect(VarInt @entityId, VarInt @effectId) {
			this.EntityId = @entityId;
			this.EffectId = @effectId;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.EntityId);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.EffectId);
		}
		public static PacketRemoveEntityEffect Read(PacketBuffer buffer ) {
			VarInt @entityId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @effectId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketRemoveEntityEffect(@entityId, @effectId);
		}
	}
	public class PacketResourcePackSend : IPacketPayload {
		public string Url { get; set; }
		public string Hash { get; set; }
		public bool Forced { get; set; }
		public string? PromptMessage { get; set; }
		public PacketResourcePackSend(string @url, string @hash, bool @forced, string? @promptMessage) {
			this.Url = @url;
			this.Hash = @hash;
			this.Forced = @forced;
			this.PromptMessage = @promptMessage;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Url);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Hash);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.Forced);
			((Action<PacketBuffer, string?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))))))(buffer, this.PromptMessage);
		}
		public static PacketResourcePackSend Read(PacketBuffer buffer ) {
			string @url = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			string @hash = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			bool @forced = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			string? @promptMessage = ((Func<PacketBuffer, string?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer);
			return new PacketResourcePackSend(@url, @hash, @forced, @promptMessage);
		}
	}
	public class PacketRespawn : IPacketPayload {
		public class DeathContainer {
			public string DimensionName { get; set; }
			public PositionBitfield Location { get; set; }
			public DeathContainer(string @dimensionName, PositionBitfield @location) {
				this.DimensionName = @dimensionName;
				this.Location = @location;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.DimensionName);
				((Action<PacketBuffer, PositionBitfield>)((buffer, value) => ((Action<PacketBuffer, ulong>)((buffer, value) => buffer.WriteU64(value)))(buffer, value.Value)))(buffer, this.Location);
			}
			public static DeathContainer Read(PacketBuffer buffer ) {
				string @dimensionName = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
				PositionBitfield @location = ((Func<PacketBuffer, PositionBitfield>)((buffer) => new PositionBitfield(((Func<PacketBuffer, ulong>)((buffer) => buffer.ReadU64()))(buffer))))(buffer);
				return new DeathContainer(@dimensionName, @location);
			}
		}
		public string Dimension { get; set; }
		public string WorldName { get; set; }
		public long HashedSeed { get; set; }
		public sbyte Gamemode { get; set; }
		public byte PreviousGamemode { get; set; }
		public bool IsDebug { get; set; }
		public bool IsFlat { get; set; }
		public bool CopyMetadata { get; set; }
		public DeathContainer? Death { get; set; }
		public PacketRespawn(string @dimension, string @worldName, long @hashedSeed, sbyte @gamemode, byte @previousGamemode, bool @isDebug, bool @isFlat, bool @copyMetadata, DeathContainer? @death) {
			this.Dimension = @dimension;
			this.WorldName = @worldName;
			this.HashedSeed = @hashedSeed;
			this.Gamemode = @gamemode;
			this.PreviousGamemode = @previousGamemode;
			this.IsDebug = @isDebug;
			this.IsFlat = @isFlat;
			this.CopyMetadata = @copyMetadata;
			this.Death = @death;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Dimension);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.WorldName);
			((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value)))(buffer, this.HashedSeed);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.Gamemode);
			((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, this.PreviousGamemode);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.IsDebug);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.IsFlat);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.CopyMetadata);
			((Action<PacketBuffer, DeathContainer?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, DeathContainer>)((buffer, value) => value.Write(buffer ))))))(buffer, this.Death);
		}
		public static PacketRespawn Read(PacketBuffer buffer ) {
			string @dimension = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			string @worldName = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			long @hashedSeed = ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64()))(buffer);
			sbyte @gamemode = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			byte @previousGamemode = ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer);
			bool @isDebug = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			bool @isFlat = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			bool @copyMetadata = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			DeathContainer? @death = ((Func<PacketBuffer, DeathContainer?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, DeathContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketRespawn.DeathContainer.Read(buffer ))))))(buffer);
			return new PacketRespawn(@dimension, @worldName, @hashedSeed, @gamemode, @previousGamemode, @isDebug, @isFlat, @copyMetadata, @death);
		}
	}
	public class PacketEntityHeadRotation : IPacketPayload {
		public VarInt EntityId { get; set; }
		public sbyte HeadYaw { get; set; }
		public PacketEntityHeadRotation(VarInt @entityId, sbyte @headYaw) {
			this.EntityId = @entityId;
			this.HeadYaw = @headYaw;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.EntityId);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.HeadYaw);
		}
		public static PacketEntityHeadRotation Read(PacketBuffer buffer ) {
			VarInt @entityId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			sbyte @headYaw = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			return new PacketEntityHeadRotation(@entityId, @headYaw);
		}
	}
	public class PacketCamera : IPacketPayload {
		public VarInt CameraId { get; set; }
		public PacketCamera(VarInt @cameraId) {
			this.CameraId = @cameraId;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.CameraId);
		}
		public static PacketCamera Read(PacketBuffer buffer ) {
			VarInt @cameraId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketCamera(@cameraId);
		}
	}
	public class PacketHeldItemSlot : IPacketPayload {
		public sbyte Slot { get; set; }
		public PacketHeldItemSlot(sbyte @slot) {
			this.Slot = @slot;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.Slot);
		}
		public static PacketHeldItemSlot Read(PacketBuffer buffer ) {
			sbyte @slot = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			return new PacketHeldItemSlot(@slot);
		}
	}
	public class PacketUpdateViewPosition : IPacketPayload {
		public VarInt ChunkX { get; set; }
		public VarInt ChunkZ { get; set; }
		public PacketUpdateViewPosition(VarInt @chunkX, VarInt @chunkZ) {
			this.ChunkX = @chunkX;
			this.ChunkZ = @chunkZ;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.ChunkX);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.ChunkZ);
		}
		public static PacketUpdateViewPosition Read(PacketBuffer buffer ) {
			VarInt @chunkX = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @chunkZ = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketUpdateViewPosition(@chunkX, @chunkZ);
		}
	}
	public class PacketUpdateViewDistance : IPacketPayload {
		public VarInt ViewDistance { get; set; }
		public PacketUpdateViewDistance(VarInt @viewDistance) {
			this.ViewDistance = @viewDistance;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.ViewDistance);
		}
		public static PacketUpdateViewDistance Read(PacketBuffer buffer ) {
			VarInt @viewDistance = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketUpdateViewDistance(@viewDistance);
		}
	}
	public class PacketShouldDisplayChatPreview : IPacketPayload {
		public bool ShouldDisplayChatPreview { get; set; }
		public PacketShouldDisplayChatPreview(bool @shouldDisplayChatPreview) {
			this.ShouldDisplayChatPreview = @shouldDisplayChatPreview;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.ShouldDisplayChatPreview);
		}
		public static PacketShouldDisplayChatPreview Read(PacketBuffer buffer ) {
			bool @shouldDisplayChatPreview = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketShouldDisplayChatPreview(@shouldDisplayChatPreview);
		}
	}
	public class PacketScoreboardDisplayObjective : IPacketPayload {
		public sbyte Position { get; set; }
		public string Name { get; set; }
		public PacketScoreboardDisplayObjective(sbyte @position, string @name) {
			this.Position = @position;
			this.Name = @name;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.Position);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Name);
		}
		public static PacketScoreboardDisplayObjective Read(PacketBuffer buffer ) {
			sbyte @position = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			string @name = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			return new PacketScoreboardDisplayObjective(@position, @name);
		}
	}
	public class PacketEntityMetadata : IPacketPayload {
		public VarInt EntityId { get; set; }
		public EntityMetadataLoopElement[] Metadata { get; set; }
		public PacketEntityMetadata(VarInt @entityId, EntityMetadataLoopElement[] @metadata) {
			this.EntityId = @entityId;
			this.Metadata = @metadata;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.EntityId);
			((Action<PacketBuffer, EntityMetadataLoopElement[]>)((buffer, value) => buffer.WriteEntityMetadataLoop(value, 255, ((Action<PacketBuffer, EntityMetadataLoopElement>)((buffer, value) => value.Write(buffer ))))))(buffer, this.Metadata);
		}
		public static PacketEntityMetadata Read(PacketBuffer buffer ) {
			VarInt @entityId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			EntityMetadataLoopElement[] @metadata = ((Func<PacketBuffer, EntityMetadataLoopElement[]>)((buffer) => buffer.ReadEntityMetadataLoop(255, ((Func<PacketBuffer, EntityMetadataLoopElement>)((buffer) => Mine.Net.EntityMetadataLoopElement.Read(buffer ))))))(buffer);
			return new PacketEntityMetadata(@entityId, @metadata);
		}
	}
	public class PacketAttachEntity : IPacketPayload {
		public int EntityId { get; set; }
		public int VehicleId { get; set; }
		public PacketAttachEntity(int @entityId, int @vehicleId) {
			this.EntityId = @entityId;
			this.VehicleId = @vehicleId;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, this.EntityId);
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, this.VehicleId);
		}
		public static PacketAttachEntity Read(PacketBuffer buffer ) {
			int @entityId = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			int @vehicleId = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			return new PacketAttachEntity(@entityId, @vehicleId);
		}
	}
	public class PacketEntityVelocity : IPacketPayload {
		public VarInt EntityId { get; set; }
		public short VelocityX { get; set; }
		public short VelocityY { get; set; }
		public short VelocityZ { get; set; }
		public PacketEntityVelocity(VarInt @entityId, short @velocityX, short @velocityY, short @velocityZ) {
			this.EntityId = @entityId;
			this.VelocityX = @velocityX;
			this.VelocityY = @velocityY;
			this.VelocityZ = @velocityZ;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.EntityId);
			((Action<PacketBuffer, short>)((buffer, value) => buffer.WriteI16(value)))(buffer, this.VelocityX);
			((Action<PacketBuffer, short>)((buffer, value) => buffer.WriteI16(value)))(buffer, this.VelocityY);
			((Action<PacketBuffer, short>)((buffer, value) => buffer.WriteI16(value)))(buffer, this.VelocityZ);
		}
		public static PacketEntityVelocity Read(PacketBuffer buffer ) {
			VarInt @entityId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			short @velocityX = ((Func<PacketBuffer, short>)((buffer) => buffer.ReadI16()))(buffer);
			short @velocityY = ((Func<PacketBuffer, short>)((buffer) => buffer.ReadI16()))(buffer);
			short @velocityZ = ((Func<PacketBuffer, short>)((buffer) => buffer.ReadI16()))(buffer);
			return new PacketEntityVelocity(@entityId, @velocityX, @velocityY, @velocityZ);
		}
	}
	public class PacketEntityEquipment : IPacketPayload {
		public class EquipmentsLoopElementContainer {
			public sbyte Slot { get; set; }
			public Slot Item { get; set; }
			public EquipmentsLoopElementContainer(sbyte @slot, Slot @item) {
				this.Slot = @slot;
				this.Item = @item;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.Slot);
				((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer )))(buffer, this.Item);
			}
			public static EquipmentsLoopElementContainer Read(PacketBuffer buffer ) {
				sbyte @slot = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
				Slot @item = ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer )))(buffer);
				return new EquipmentsLoopElementContainer(@slot, @item);
			}
		}
		public VarInt EntityId { get; set; }
		public EquipmentsLoopElementContainer[] Equipments { get; set; }
		public PacketEntityEquipment(VarInt @entityId, EquipmentsLoopElementContainer[] @equipments) {
			this.EntityId = @entityId;
			this.Equipments = @equipments;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.EntityId);
			((Action<PacketBuffer, EquipmentsLoopElementContainer[]>)((buffer, value) => buffer.WriteTopBitSetTerminatedArray(value, ((Action<PacketBuffer, EquipmentsLoopElementContainer>)((buffer, value) => value.Write(buffer ))))))(buffer, this.Equipments);
		}
		public static PacketEntityEquipment Read(PacketBuffer buffer ) {
			VarInt @entityId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			EquipmentsLoopElementContainer[] @equipments = ((Func<PacketBuffer, EquipmentsLoopElementContainer[]>)((buffer) => buffer.ReadTopBitSetTerminatedArray(((Func<PacketBuffer, EquipmentsLoopElementContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketEntityEquipment.EquipmentsLoopElementContainer.Read(buffer ))))))(buffer);
			return new PacketEntityEquipment(@entityId, @equipments);
		}
	}
	public class PacketExperience : IPacketPayload {
		public float ExperienceBar { get; set; }
		public VarInt Level { get; set; }
		public VarInt TotalExperience { get; set; }
		public PacketExperience(float @experienceBar, VarInt @level, VarInt @totalExperience) {
			this.ExperienceBar = @experienceBar;
			this.Level = @level;
			this.TotalExperience = @totalExperience;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, this.ExperienceBar);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.Level);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.TotalExperience);
		}
		public static PacketExperience Read(PacketBuffer buffer ) {
			float @experienceBar = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			VarInt @level = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @totalExperience = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketExperience(@experienceBar, @level, @totalExperience);
		}
	}
	public class PacketUpdateHealth : IPacketPayload {
		public float Health { get; set; }
		public VarInt Food { get; set; }
		public float FoodSaturation { get; set; }
		public PacketUpdateHealth(float @health, VarInt @food, float @foodSaturation) {
			this.Health = @health;
			this.Food = @food;
			this.FoodSaturation = @foodSaturation;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, this.Health);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.Food);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, this.FoodSaturation);
		}
		public static PacketUpdateHealth Read(PacketBuffer buffer ) {
			float @health = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			VarInt @food = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			float @foodSaturation = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			return new PacketUpdateHealth(@health, @food, @foodSaturation);
		}
	}
	public class PacketScoreboardObjective : IPacketPayload {
		public class DisplayTextSwitch {
			public object? Value { get; set; }
			public DisplayTextSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, sbyte state) {
				switch (state) {
					case 0: ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string)this); break;
					case 2: ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string)this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static DisplayTextSwitch Read(PacketBuffer buffer, sbyte state) {
				object? value = state switch {
					0 => ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer),
					2 => ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new DisplayTextSwitch(value);
			}
			public static implicit operator string?(DisplayTextSwitch value) => (string?)value.Value;
			public static implicit operator DisplayTextSwitch?(string? value) => new DisplayTextSwitch(value);
		}
		public class TypeSwitch {
			public object? Value { get; set; }
			public TypeSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, sbyte state) {
				switch (state) {
					case 0: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
					case 2: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static TypeSwitch Read(PacketBuffer buffer, sbyte state) {
				object? value = state switch {
					0 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
					2 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new TypeSwitch(value);
			}
			public static implicit operator VarInt?(TypeSwitch value) => (VarInt?)value.Value;
			public static implicit operator TypeSwitch?(VarInt? value) => new TypeSwitch(value);
		}
		public string Name { get; set; }
		public sbyte Action { get; set; }
		public DisplayTextSwitch DisplayText { get; set; }
		public TypeSwitch Type { get; set; }
		public PacketScoreboardObjective(string @name, sbyte @action, DisplayTextSwitch @displayText, TypeSwitch @type) {
			this.Name = @name;
			this.Action = @action;
			this.DisplayText = @displayText;
			this.Type = @type;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Name);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.Action);
			((Action<PacketBuffer, DisplayTextSwitch>)((buffer, value) => value.Write(buffer, Action)))(buffer, this.DisplayText);
			((Action<PacketBuffer, TypeSwitch>)((buffer, value) => value.Write(buffer, Action)))(buffer, this.Type);
		}
		public static PacketScoreboardObjective Read(PacketBuffer buffer ) {
			string @name = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			sbyte @action = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			DisplayTextSwitch @displayText = ((Func<PacketBuffer, DisplayTextSwitch>)((buffer) => DisplayTextSwitch.Read(buffer, @action)))(buffer);
			TypeSwitch @type = ((Func<PacketBuffer, TypeSwitch>)((buffer) => TypeSwitch.Read(buffer, @action)))(buffer);
			return new PacketScoreboardObjective(@name, @action, @displayText, @type);
		}
	}
	public class PacketSetPassengers : IPacketPayload {
		public VarInt EntityId { get; set; }
		public VarInt[] Passengers { get; set; }
		public PacketSetPassengers(VarInt @entityId, VarInt[] @passengers) {
			this.EntityId = @entityId;
			this.Passengers = @passengers;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.EntityId);
			((Action<PacketBuffer, VarInt[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Passengers);
		}
		public static PacketSetPassengers Read(PacketBuffer buffer ) {
			VarInt @entityId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt[] @passengers = ((Func<PacketBuffer, VarInt[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			return new PacketSetPassengers(@entityId, @passengers);
		}
	}
	public class PacketTeams : IPacketPayload {
		public class NameSwitch {
			public object? Value { get; set; }
			public NameSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, sbyte state) {
				switch (state) {
					case 0: ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string)this); break;
					case 2: ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string)this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static NameSwitch Read(PacketBuffer buffer, sbyte state) {
				object? value = state switch {
					0 => ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer),
					2 => ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new NameSwitch(value);
			}
			public static implicit operator string?(NameSwitch value) => (string?)value.Value;
			public static implicit operator NameSwitch?(string? value) => new NameSwitch(value);
		}
		public class FriendlyFireSwitch {
			public object? Value { get; set; }
			public FriendlyFireSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, sbyte state) {
				switch (state) {
					case 0: ((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, (sbyte)this); break;
					case 2: ((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, (sbyte)this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static FriendlyFireSwitch Read(PacketBuffer buffer, sbyte state) {
				object? value = state switch {
					0 => ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer),
					2 => ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new FriendlyFireSwitch(value);
			}
			public static implicit operator sbyte?(FriendlyFireSwitch value) => (sbyte?)value.Value;
			public static implicit operator FriendlyFireSwitch?(sbyte? value) => new FriendlyFireSwitch(value);
		}
		public class NameTagVisibilitySwitch {
			public object? Value { get; set; }
			public NameTagVisibilitySwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, sbyte state) {
				switch (state) {
					case 0: ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string)this); break;
					case 2: ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string)this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static NameTagVisibilitySwitch Read(PacketBuffer buffer, sbyte state) {
				object? value = state switch {
					0 => ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer),
					2 => ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new NameTagVisibilitySwitch(value);
			}
			public static implicit operator string?(NameTagVisibilitySwitch value) => (string?)value.Value;
			public static implicit operator NameTagVisibilitySwitch?(string? value) => new NameTagVisibilitySwitch(value);
		}
		public class CollisionRuleSwitch {
			public object? Value { get; set; }
			public CollisionRuleSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, sbyte state) {
				switch (state) {
					case 0: ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string)this); break;
					case 2: ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string)this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static CollisionRuleSwitch Read(PacketBuffer buffer, sbyte state) {
				object? value = state switch {
					0 => ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer),
					2 => ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new CollisionRuleSwitch(value);
			}
			public static implicit operator string?(CollisionRuleSwitch value) => (string?)value.Value;
			public static implicit operator CollisionRuleSwitch?(string? value) => new CollisionRuleSwitch(value);
		}
		public class FormattingSwitch {
			public object? Value { get; set; }
			public FormattingSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, sbyte state) {
				switch (state) {
					case 0: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
					case 2: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static FormattingSwitch Read(PacketBuffer buffer, sbyte state) {
				object? value = state switch {
					0 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
					2 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new FormattingSwitch(value);
			}
			public static implicit operator VarInt?(FormattingSwitch value) => (VarInt?)value.Value;
			public static implicit operator FormattingSwitch?(VarInt? value) => new FormattingSwitch(value);
		}
		public class PrefixSwitch {
			public object? Value { get; set; }
			public PrefixSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, sbyte state) {
				switch (state) {
					case 0: ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string)this); break;
					case 2: ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string)this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static PrefixSwitch Read(PacketBuffer buffer, sbyte state) {
				object? value = state switch {
					0 => ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer),
					2 => ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new PrefixSwitch(value);
			}
			public static implicit operator string?(PrefixSwitch value) => (string?)value.Value;
			public static implicit operator PrefixSwitch?(string? value) => new PrefixSwitch(value);
		}
		public class SuffixSwitch {
			public object? Value { get; set; }
			public SuffixSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, sbyte state) {
				switch (state) {
					case 0: ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string)this); break;
					case 2: ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string)this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static SuffixSwitch Read(PacketBuffer buffer, sbyte state) {
				object? value = state switch {
					0 => ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer),
					2 => ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new SuffixSwitch(value);
			}
			public static implicit operator string?(SuffixSwitch value) => (string?)value.Value;
			public static implicit operator SuffixSwitch?(string? value) => new SuffixSwitch(value);
		}
		public class PlayersSwitch {
			public object? Value { get; set; }
			public PlayersSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, sbyte state) {
				switch (state) {
					case 0: ((Action<PacketBuffer, string[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string[])this); break;
					case 3: ((Action<PacketBuffer, string[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string[])this); break;
					case 4: ((Action<PacketBuffer, string[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string[])this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static PlayersSwitch Read(PacketBuffer buffer, sbyte state) {
				object? value = state switch {
					0 => ((Func<PacketBuffer, string[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer),
					3 => ((Func<PacketBuffer, string[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer),
					4 => ((Func<PacketBuffer, string[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new PlayersSwitch(value);
			}
			public static implicit operator string[]?(PlayersSwitch value) => (string[]?)value.Value;
			public static implicit operator PlayersSwitch?(string[]? value) => new PlayersSwitch(value);
		}
		public string Team { get; set; }
		public sbyte Mode { get; set; }
		public NameSwitch Name { get; set; }
		public FriendlyFireSwitch FriendlyFire { get; set; }
		public NameTagVisibilitySwitch NameTagVisibility { get; set; }
		public CollisionRuleSwitch CollisionRule { get; set; }
		public FormattingSwitch Formatting { get; set; }
		public PrefixSwitch Prefix { get; set; }
		public SuffixSwitch Suffix { get; set; }
		public PlayersSwitch Players { get; set; }
		public PacketTeams(string @team, sbyte @mode, NameSwitch @name, FriendlyFireSwitch @friendlyFire, NameTagVisibilitySwitch @nameTagVisibility, CollisionRuleSwitch @collisionRule, FormattingSwitch @formatting, PrefixSwitch @prefix, SuffixSwitch @suffix, PlayersSwitch @players) {
			this.Team = @team;
			this.Mode = @mode;
			this.Name = @name;
			this.FriendlyFire = @friendlyFire;
			this.NameTagVisibility = @nameTagVisibility;
			this.CollisionRule = @collisionRule;
			this.Formatting = @formatting;
			this.Prefix = @prefix;
			this.Suffix = @suffix;
			this.Players = @players;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Team);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.Mode);
			((Action<PacketBuffer, NameSwitch>)((buffer, value) => value.Write(buffer, Mode)))(buffer, this.Name);
			((Action<PacketBuffer, FriendlyFireSwitch>)((buffer, value) => value.Write(buffer, Mode)))(buffer, this.FriendlyFire);
			((Action<PacketBuffer, NameTagVisibilitySwitch>)((buffer, value) => value.Write(buffer, Mode)))(buffer, this.NameTagVisibility);
			((Action<PacketBuffer, CollisionRuleSwitch>)((buffer, value) => value.Write(buffer, Mode)))(buffer, this.CollisionRule);
			((Action<PacketBuffer, FormattingSwitch>)((buffer, value) => value.Write(buffer, Mode)))(buffer, this.Formatting);
			((Action<PacketBuffer, PrefixSwitch>)((buffer, value) => value.Write(buffer, Mode)))(buffer, this.Prefix);
			((Action<PacketBuffer, SuffixSwitch>)((buffer, value) => value.Write(buffer, Mode)))(buffer, this.Suffix);
			((Action<PacketBuffer, PlayersSwitch>)((buffer, value) => value.Write(buffer, Mode)))(buffer, this.Players);
		}
		public static PacketTeams Read(PacketBuffer buffer ) {
			string @team = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			sbyte @mode = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			NameSwitch @name = ((Func<PacketBuffer, NameSwitch>)((buffer) => NameSwitch.Read(buffer, @mode)))(buffer);
			FriendlyFireSwitch @friendlyFire = ((Func<PacketBuffer, FriendlyFireSwitch>)((buffer) => FriendlyFireSwitch.Read(buffer, @mode)))(buffer);
			NameTagVisibilitySwitch @nameTagVisibility = ((Func<PacketBuffer, NameTagVisibilitySwitch>)((buffer) => NameTagVisibilitySwitch.Read(buffer, @mode)))(buffer);
			CollisionRuleSwitch @collisionRule = ((Func<PacketBuffer, CollisionRuleSwitch>)((buffer) => CollisionRuleSwitch.Read(buffer, @mode)))(buffer);
			FormattingSwitch @formatting = ((Func<PacketBuffer, FormattingSwitch>)((buffer) => FormattingSwitch.Read(buffer, @mode)))(buffer);
			PrefixSwitch @prefix = ((Func<PacketBuffer, PrefixSwitch>)((buffer) => PrefixSwitch.Read(buffer, @mode)))(buffer);
			SuffixSwitch @suffix = ((Func<PacketBuffer, SuffixSwitch>)((buffer) => SuffixSwitch.Read(buffer, @mode)))(buffer);
			PlayersSwitch @players = ((Func<PacketBuffer, PlayersSwitch>)((buffer) => PlayersSwitch.Read(buffer, @mode)))(buffer);
			return new PacketTeams(@team, @mode, @name, @friendlyFire, @nameTagVisibility, @collisionRule, @formatting, @prefix, @suffix, @players);
		}
	}
	public class PacketScoreboardScore : IPacketPayload {
		public class ValueSwitch {
			public object? Value { get; set; }
			public ValueSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, VarInt state) {
				switch (state) {
					case 1: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
					default: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)Value); break;
				}
			}
			public static ValueSwitch Read(PacketBuffer buffer, VarInt state) {
				object? value = state.Value switch {
					1 => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
					_ => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer)
				};
				return new ValueSwitch(value);
			}
		}
		public string ItemName { get; set; }
		public VarInt Action { get; set; }
		public string ScoreName { get; set; }
		public ValueSwitch Value { get; set; }
		public PacketScoreboardScore(string @itemName, VarInt @action, string @scoreName, ValueSwitch @value) {
			this.ItemName = @itemName;
			this.Action = @action;
			this.ScoreName = @scoreName;
			this.Value = @value;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.ItemName);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.Action);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.ScoreName);
			((Action<PacketBuffer, ValueSwitch>)((buffer, value) => value.Write(buffer, Action)))(buffer, this.Value);
		}
		public static PacketScoreboardScore Read(PacketBuffer buffer ) {
			string @itemName = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			VarInt @action = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			string @scoreName = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			ValueSwitch @value = ((Func<PacketBuffer, ValueSwitch>)((buffer) => ValueSwitch.Read(buffer, @action)))(buffer);
			return new PacketScoreboardScore(@itemName, @action, @scoreName, @value);
		}
	}
	public class PacketSpawnPosition : IPacketPayload {
		public PositionBitfield Location { get; set; }
		public float Angle { get; set; }
		public PacketSpawnPosition(PositionBitfield @location, float @angle) {
			this.Location = @location;
			this.Angle = @angle;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, PositionBitfield>)((buffer, value) => ((Action<PacketBuffer, ulong>)((buffer, value) => buffer.WriteU64(value)))(buffer, value.Value)))(buffer, this.Location);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, this.Angle);
		}
		public static PacketSpawnPosition Read(PacketBuffer buffer ) {
			PositionBitfield @location = ((Func<PacketBuffer, PositionBitfield>)((buffer) => new PositionBitfield(((Func<PacketBuffer, ulong>)((buffer) => buffer.ReadU64()))(buffer))))(buffer);
			float @angle = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			return new PacketSpawnPosition(@location, @angle);
		}
	}
	public class PacketUpdateTime : IPacketPayload {
		public long Age { get; set; }
		public long Time { get; set; }
		public PacketUpdateTime(long @age, long @time) {
			this.Age = @age;
			this.Time = @time;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value)))(buffer, this.Age);
			((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value)))(buffer, this.Time);
		}
		public static PacketUpdateTime Read(PacketBuffer buffer ) {
			long @age = ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64()))(buffer);
			long @time = ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64()))(buffer);
			return new PacketUpdateTime(@age, @time);
		}
	}
	public class PacketEntitySoundEffect : IPacketPayload {
		public VarInt SoundId { get; set; }
		public VarInt SoundCategory { get; set; }
		public VarInt EntityId { get; set; }
		public float Volume { get; set; }
		public float Pitch { get; set; }
		public PacketEntitySoundEffect(VarInt @soundId, VarInt @soundCategory, VarInt @entityId, float @volume, float @pitch) {
			this.SoundId = @soundId;
			this.SoundCategory = @soundCategory;
			this.EntityId = @entityId;
			this.Volume = @volume;
			this.Pitch = @pitch;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.SoundId);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.SoundCategory);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.EntityId);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, this.Volume);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, this.Pitch);
		}
		public static PacketEntitySoundEffect Read(PacketBuffer buffer ) {
			VarInt @soundId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @soundCategory = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @entityId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			float @volume = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			float @pitch = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			return new PacketEntitySoundEffect(@soundId, @soundCategory, @entityId, @volume, @pitch);
		}
	}
	public class PacketStopSound : IPacketPayload {
		public class SourceSwitch {
			public object? Value { get; set; }
			public SourceSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, sbyte state) {
				switch (state) {
					case 3: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
					case 1: ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, (VarInt)this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static SourceSwitch Read(PacketBuffer buffer, sbyte state) {
				object? value = state switch {
					3 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
					1 => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new SourceSwitch(value);
			}
			public static implicit operator VarInt?(SourceSwitch value) => (VarInt?)value.Value;
			public static implicit operator SourceSwitch?(VarInt? value) => new SourceSwitch(value);
		}
		public class SoundSwitch {
			public object? Value { get; set; }
			public SoundSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, sbyte state) {
				switch (state) {
					case 3: ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string)this); break;
					case 2: ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, (string)this); break;
					default: ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)Value); break;
				}
			}
			public static SoundSwitch Read(PacketBuffer buffer, sbyte state) {
				object? value = state switch {
					3 => ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer),
					2 => ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer),
					_ => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer)
				};
				return new SoundSwitch(value);
			}
			public static implicit operator string?(SoundSwitch value) => (string?)value.Value;
			public static implicit operator SoundSwitch?(string? value) => new SoundSwitch(value);
		}
		public sbyte Flags { get; set; }
		public SourceSwitch Source { get; set; }
		public SoundSwitch Sound { get; set; }
		public PacketStopSound(sbyte @flags, SourceSwitch @source, SoundSwitch @sound) {
			this.Flags = @flags;
			this.Source = @source;
			this.Sound = @sound;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.Flags);
			((Action<PacketBuffer, SourceSwitch>)((buffer, value) => value.Write(buffer, Flags)))(buffer, this.Source);
			((Action<PacketBuffer, SoundSwitch>)((buffer, value) => value.Write(buffer, Flags)))(buffer, this.Sound);
		}
		public static PacketStopSound Read(PacketBuffer buffer ) {
			sbyte @flags = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			SourceSwitch @source = ((Func<PacketBuffer, SourceSwitch>)((buffer) => SourceSwitch.Read(buffer, @flags)))(buffer);
			SoundSwitch @sound = ((Func<PacketBuffer, SoundSwitch>)((buffer) => SoundSwitch.Read(buffer, @flags)))(buffer);
			return new PacketStopSound(@flags, @source, @sound);
		}
	}
	public class PacketSoundEffect : IPacketPayload {
		public VarInt SoundId { get; set; }
		public VarInt SoundCategory { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }
		public float Volume { get; set; }
		public float Pitch { get; set; }
		public long Seed { get; set; }
		public PacketSoundEffect(VarInt @soundId, VarInt @soundCategory, int @x, int @y, int @z, float @volume, float @pitch, long @seed) {
			this.SoundId = @soundId;
			this.SoundCategory = @soundCategory;
			this.X = @x;
			this.Y = @y;
			this.Z = @z;
			this.Volume = @volume;
			this.Pitch = @pitch;
			this.Seed = @seed;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.SoundId);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.SoundCategory);
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, this.X);
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, this.Y);
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, this.Z);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, this.Volume);
			((Action<PacketBuffer, float>)((buffer, value) => buffer.WriteF32(value)))(buffer, this.Pitch);
			((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value)))(buffer, this.Seed);
		}
		public static PacketSoundEffect Read(PacketBuffer buffer ) {
			VarInt @soundId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @soundCategory = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			int @x = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			int @y = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			int @z = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			float @volume = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			float @pitch = ((Func<PacketBuffer, float>)((buffer) => buffer.ReadF32()))(buffer);
			long @seed = ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64()))(buffer);
			return new PacketSoundEffect(@soundId, @soundCategory, @x, @y, @z, @volume, @pitch, @seed);
		}
	}
	public class PacketSystemChat : IPacketPayload {
		public string Content { get; set; }
		public bool IsActionBar { get; set; }
		public PacketSystemChat(string @content, bool @isActionBar) {
			this.Content = @content;
			this.IsActionBar = @isActionBar;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Content);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.IsActionBar);
		}
		public static PacketSystemChat Read(PacketBuffer buffer ) {
			string @content = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			bool @isActionBar = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketSystemChat(@content, @isActionBar);
		}
	}
	public class PacketPlayerlistHeader : IPacketPayload {
		public string Header { get; set; }
		public string Footer { get; set; }
		public PacketPlayerlistHeader(string @header, string @footer) {
			this.Header = @header;
			this.Footer = @footer;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Header);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Footer);
		}
		public static PacketPlayerlistHeader Read(PacketBuffer buffer ) {
			string @header = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			string @footer = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			return new PacketPlayerlistHeader(@header, @footer);
		}
	}
	public class PacketCollect : IPacketPayload {
		public VarInt CollectedEntityId { get; set; }
		public VarInt CollectorEntityId { get; set; }
		public VarInt PickupItemCount { get; set; }
		public PacketCollect(VarInt @collectedEntityId, VarInt @collectorEntityId, VarInt @pickupItemCount) {
			this.CollectedEntityId = @collectedEntityId;
			this.CollectorEntityId = @collectorEntityId;
			this.PickupItemCount = @pickupItemCount;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.CollectedEntityId);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.CollectorEntityId);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.PickupItemCount);
		}
		public static PacketCollect Read(PacketBuffer buffer ) {
			VarInt @collectedEntityId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @collectorEntityId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @pickupItemCount = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketCollect(@collectedEntityId, @collectorEntityId, @pickupItemCount);
		}
	}
	public class PacketEntityTeleport : IPacketPayload {
		public VarInt EntityId { get; set; }
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
		public sbyte Yaw { get; set; }
		public sbyte Pitch { get; set; }
		public bool OnGround { get; set; }
		public PacketEntityTeleport(VarInt @entityId, double @x, double @y, double @z, sbyte @yaw, sbyte @pitch, bool @onGround) {
			this.EntityId = @entityId;
			this.X = @x;
			this.Y = @y;
			this.Z = @z;
			this.Yaw = @yaw;
			this.Pitch = @pitch;
			this.OnGround = @onGround;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.EntityId);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, this.X);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, this.Y);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, this.Z);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.Yaw);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.Pitch);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.OnGround);
		}
		public static PacketEntityTeleport Read(PacketBuffer buffer ) {
			VarInt @entityId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			double @x = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @y = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @z = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			sbyte @yaw = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			sbyte @pitch = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			bool @onGround = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketEntityTeleport(@entityId, @x, @y, @z, @yaw, @pitch, @onGround);
		}
	}
	public class PacketEntityUpdateAttributes : IPacketPayload {
		public class PropertiesElementContainer {
			public class ModifiersElementContainer {
				public UUID Uuid { get; set; }
				public double Amount { get; set; }
				public sbyte Operation { get; set; }
				public ModifiersElementContainer(UUID @uuid, double @amount, sbyte @operation) {
					this.Uuid = @uuid;
					this.Amount = @amount;
					this.Operation = @operation;
				}
				public void Write(PacketBuffer buffer ) {
					((Action<PacketBuffer, UUID>)((buffer, value) => buffer.WriteUUID(value)))(buffer, this.Uuid);
					((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, this.Amount);
					((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.Operation);
				}
				public static ModifiersElementContainer Read(PacketBuffer buffer ) {
					UUID @uuid = ((Func<PacketBuffer, UUID>)((buffer) => buffer.ReadUUID()))(buffer);
					double @amount = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
					sbyte @operation = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
					return new ModifiersElementContainer(@uuid, @amount, @operation);
				}
			}
			public string Key { get; set; }
			public double Value { get; set; }
			public ModifiersElementContainer[] Modifiers { get; set; }
			public PropertiesElementContainer(string @key, double @value, ModifiersElementContainer[] @modifiers) {
				this.Key = @key;
				this.Value = @value;
				this.Modifiers = @modifiers;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Key);
				((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, this.Value);
				((Action<PacketBuffer, ModifiersElementContainer[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, ModifiersElementContainer>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Modifiers);
			}
			public static PropertiesElementContainer Read(PacketBuffer buffer ) {
				string @key = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
				double @value = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
				ModifiersElementContainer[] @modifiers = ((Func<PacketBuffer, ModifiersElementContainer[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, ModifiersElementContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketEntityUpdateAttributes.PropertiesElementContainer.ModifiersElementContainer.Read(buffer ))))))(buffer);
				return new PropertiesElementContainer(@key, @value, @modifiers);
			}
		}
		public VarInt EntityId { get; set; }
		public PropertiesElementContainer[] Properties { get; set; }
		public PacketEntityUpdateAttributes(VarInt @entityId, PropertiesElementContainer[] @properties) {
			this.EntityId = @entityId;
			this.Properties = @properties;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.EntityId);
			((Action<PacketBuffer, PropertiesElementContainer[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, PropertiesElementContainer>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Properties);
		}
		public static PacketEntityUpdateAttributes Read(PacketBuffer buffer ) {
			VarInt @entityId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			PropertiesElementContainer[] @properties = ((Func<PacketBuffer, PropertiesElementContainer[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, PropertiesElementContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketEntityUpdateAttributes.PropertiesElementContainer.Read(buffer ))))))(buffer);
			return new PacketEntityUpdateAttributes(@entityId, @properties);
		}
	}
	public class PacketEntityEffect : IPacketPayload {
		public VarInt EntityId { get; set; }
		public VarInt EffectId { get; set; }
		public sbyte Amplifier { get; set; }
		public VarInt Duration { get; set; }
		public sbyte HideParticles { get; set; }
		public NbtCompound? FactorCodec { get; set; }
		public PacketEntityEffect(VarInt @entityId, VarInt @effectId, sbyte @amplifier, VarInt @duration, sbyte @hideParticles, NbtCompound? @factorCodec) {
			this.EntityId = @entityId;
			this.EffectId = @effectId;
			this.Amplifier = @amplifier;
			this.Duration = @duration;
			this.HideParticles = @hideParticles;
			this.FactorCodec = @factorCodec;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.EntityId);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.EffectId);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.Amplifier);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.Duration);
			((Action<PacketBuffer, sbyte>)((buffer, value) => buffer.WriteI8(value)))(buffer, this.HideParticles);
			((Action<PacketBuffer, NbtCompound?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, NbtCompound>)((buffer, value) => buffer.WriteNbt(value))))))(buffer, this.FactorCodec);
		}
		public static PacketEntityEffect Read(PacketBuffer buffer ) {
			VarInt @entityId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @effectId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			sbyte @amplifier = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			VarInt @duration = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			sbyte @hideParticles = ((Func<PacketBuffer, sbyte>)((buffer) => buffer.ReadI8()))(buffer);
			NbtCompound? @factorCodec = ((Func<PacketBuffer, NbtCompound?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, NbtCompound>)((buffer) => buffer.ReadNbt())))))(buffer);
			return new PacketEntityEffect(@entityId, @effectId, @amplifier, @duration, @hideParticles, @factorCodec);
		}
	}
	public class PacketSelectAdvancementTab : IPacketPayload {
		public string? Id { get; set; }
		public PacketSelectAdvancementTab(string? @id) {
			this.Id = @id;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))))))(buffer, this.Id);
		}
		public static PacketSelectAdvancementTab Read(PacketBuffer buffer ) {
			string? @id = ((Func<PacketBuffer, string?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer);
			return new PacketSelectAdvancementTab(@id);
		}
	}
	public class PacketServerData : IPacketPayload {
		public string? Motd { get; set; }
		public string? Icon { get; set; }
		public bool PreviewsChat { get; set; }
		public bool EnforcesSecureChat { get; set; }
		public PacketServerData(string? @motd, string? @icon, bool @previewsChat, bool @enforcesSecureChat) {
			this.Motd = @motd;
			this.Icon = @icon;
			this.PreviewsChat = @previewsChat;
			this.EnforcesSecureChat = @enforcesSecureChat;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))))))(buffer, this.Motd);
			((Action<PacketBuffer, string?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))))))(buffer, this.Icon);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.PreviewsChat);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.EnforcesSecureChat);
		}
		public static PacketServerData Read(PacketBuffer buffer ) {
			string? @motd = ((Func<PacketBuffer, string?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer);
			string? @icon = ((Func<PacketBuffer, string?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer);
			bool @previewsChat = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			bool @enforcesSecureChat = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketServerData(@motd, @icon, @previewsChat, @enforcesSecureChat);
		}
	}
	public class PacketDeclareRecipes : IPacketPayload {
		public class RecipesElementContainer {
			public class DataSwitch {
				public class DataswitchstateminecraftCraftingShapelessContainer {
					public string Group { get; set; }
					public Slot[][] Ingredients { get; set; }
					public Slot Result { get; set; }
					public DataswitchstateminecraftCraftingShapelessContainer(string @group, Slot[][] @ingredients, Slot @result) {
						this.Group = @group;
						this.Ingredients = @ingredients;
						this.Result = @result;
					}
					public void Write(PacketBuffer buffer ) {
						((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Group);
						((Action<PacketBuffer, Slot[][]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, Slot[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Ingredients);
						((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer )))(buffer, this.Result);
					}
					public static DataswitchstateminecraftCraftingShapelessContainer Read(PacketBuffer buffer ) {
						string @group = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
						Slot[][] @ingredients = ((Func<PacketBuffer, Slot[][]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, Slot[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer )))))))))(buffer);
						Slot @result = ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer )))(buffer);
						return new DataswitchstateminecraftCraftingShapelessContainer(@group, @ingredients, @result);
					}
				}
				public class DataswitchstateminecraftCraftingShapedContainer {
					public VarInt Width { get; set; }
					public VarInt Height { get; set; }
					public string Group { get; set; }
					public Slot[][][] Ingredients { get; set; }
					public Slot Result { get; set; }
					public DataswitchstateminecraftCraftingShapedContainer(VarInt @width, VarInt @height, string @group, Slot[][][] @ingredients, Slot @result) {
						this.Width = @width;
						this.Height = @height;
						this.Group = @group;
						this.Ingredients = @ingredients;
						this.Result = @result;
					}
					public void Write(PacketBuffer buffer ) {
						((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.Width);
						((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.Height);
						((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Group);
						((Action<PacketBuffer, Slot[][][]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, Slot[][]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, Slot[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Ingredients);
						((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer )))(buffer, this.Result);
					}
					public static DataswitchstateminecraftCraftingShapedContainer Read(PacketBuffer buffer ) {
						VarInt @width = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
						VarInt @height = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
						string @group = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
						Slot[][][] @ingredients = ((Func<PacketBuffer, Slot[][][]>)((buffer) => buffer.ReadArray(@width, ((Func<PacketBuffer, Slot[][]>)((buffer) => buffer.ReadArray(@height, ((Func<PacketBuffer, Slot[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer ))))))))))))(buffer);
						Slot @result = ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer )))(buffer);
						return new DataswitchstateminecraftCraftingShapedContainer(@width, @height, @group, @ingredients, @result);
					}
				}
				public class DataSwitchStateminecraftStonecuttingContainer {
					public string Group { get; set; }
					public Slot[] Ingredient { get; set; }
					public Slot Result { get; set; }
					public DataSwitchStateminecraftStonecuttingContainer(string @group, Slot[] @ingredient, Slot @result) {
						this.Group = @group;
						this.Ingredient = @ingredient;
						this.Result = @result;
					}
					public void Write(PacketBuffer buffer ) {
						((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Group);
						((Action<PacketBuffer, Slot[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Ingredient);
						((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer )))(buffer, this.Result);
					}
					public static DataSwitchStateminecraftStonecuttingContainer Read(PacketBuffer buffer ) {
						string @group = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
						Slot[] @ingredient = ((Func<PacketBuffer, Slot[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer ))))))(buffer);
						Slot @result = ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer )))(buffer);
						return new DataSwitchStateminecraftStonecuttingContainer(@group, @ingredient, @result);
					}
				}
				public class DataSwitchStateminecraftSmithingContainer {
					public Slot[] Base { get; set; }
					public Slot[] Addition { get; set; }
					public Slot Result { get; set; }
					public DataSwitchStateminecraftSmithingContainer(Slot[] @base, Slot[] @addition, Slot @result) {
						this.Base = @base;
						this.Addition = @addition;
						this.Result = @result;
					}
					public void Write(PacketBuffer buffer ) {
						((Action<PacketBuffer, Slot[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Base);
						((Action<PacketBuffer, Slot[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Addition);
						((Action<PacketBuffer, Slot>)((buffer, value) => value.Write(buffer )))(buffer, this.Result);
					}
					public static DataSwitchStateminecraftSmithingContainer Read(PacketBuffer buffer ) {
						Slot[] @base = ((Func<PacketBuffer, Slot[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer ))))))(buffer);
						Slot[] @addition = ((Func<PacketBuffer, Slot[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer ))))))(buffer);
						Slot @result = ((Func<PacketBuffer, Slot>)((buffer) => Mine.Net.Slot.Read(buffer )))(buffer);
						return new DataSwitchStateminecraftSmithingContainer(@base, @addition, @result);
					}
				}
				public object? Value { get; set; }
				public DataSwitch(object? value) {
					this.Value = value;
				}
				public void Write(PacketBuffer buffer, string state) {
					switch (state) {
						case "minecraft:crafting_shapeless": ((Action<PacketBuffer, DataswitchstateminecraftCraftingShapelessContainer>)((buffer, value) => value.Write(buffer )))(buffer, (DataswitchstateminecraftCraftingShapelessContainer)this); break;
						case "minecraft:crafting_shaped": ((Action<PacketBuffer, DataswitchstateminecraftCraftingShapedContainer>)((buffer, value) => value.Write(buffer )))(buffer, (DataswitchstateminecraftCraftingShapedContainer)this); break;
						case "minecraft:crafting_special_armordye": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
						case "minecraft:crafting_special_bookcloning": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
						case "minecraft:crafting_special_mapcloning": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
						case "minecraft:crafting_special_mapextending": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
						case "minecraft:crafting_special_firework_rocket": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
						case "minecraft:crafting_special_firework_star": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
						case "minecraft:crafting_special_firework_star_fade": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
						case "minecraft:crafting_special_repairitem": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
						case "minecraft:crafting_special_tippedarrow": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
						case "minecraft:crafting_special_bannerduplicate": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
						case "minecraft:crafting_special_banneraddpattern": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
						case "minecraft:crafting_special_shielddecoration": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
						case "minecraft:crafting_special_shulkerboxcoloring": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
						case "minecraft:crafting_special_suspiciousstew": ((Action<PacketBuffer, object?>)((buffer, value) => buffer.WriteVoid(value)))(buffer, (object?)this); break;
						case "minecraft:smelting": ((Action<PacketBuffer, MinecraftSmeltingFormat>)((buffer, value) => value.Write(buffer )))(buffer, (MinecraftSmeltingFormat)this); break;
						case "minecraft:blasting": ((Action<PacketBuffer, MinecraftSmeltingFormat>)((buffer, value) => value.Write(buffer )))(buffer, (MinecraftSmeltingFormat)this); break;
						case "minecraft:smoking": ((Action<PacketBuffer, MinecraftSmeltingFormat>)((buffer, value) => value.Write(buffer )))(buffer, (MinecraftSmeltingFormat)this); break;
						case "minecraft:campfire_cooking": ((Action<PacketBuffer, MinecraftSmeltingFormat>)((buffer, value) => value.Write(buffer )))(buffer, (MinecraftSmeltingFormat)this); break;
						case "minecraft:stonecutting": ((Action<PacketBuffer, DataSwitchStateminecraftStonecuttingContainer>)((buffer, value) => value.Write(buffer )))(buffer, (DataSwitchStateminecraftStonecuttingContainer)this); break;
						case "minecraft:smithing": ((Action<PacketBuffer, DataSwitchStateminecraftSmithingContainer>)((buffer, value) => value.Write(buffer )))(buffer, (DataSwitchStateminecraftSmithingContainer)this); break;
						default: throw new Exception($"Invalid value: '{state}'");
					}
				}
				public static DataSwitch Read(PacketBuffer buffer, string state) {
					object? value = state switch {
						"minecraft:crafting_shapeless" => ((Func<PacketBuffer, DataswitchstateminecraftCraftingShapelessContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketDeclareRecipes.RecipesElementContainer.DataSwitch.DataswitchstateminecraftCraftingShapelessContainer.Read(buffer )))(buffer),
						"minecraft:crafting_shaped" => ((Func<PacketBuffer, DataswitchstateminecraftCraftingShapedContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketDeclareRecipes.RecipesElementContainer.DataSwitch.DataswitchstateminecraftCraftingShapedContainer.Read(buffer )))(buffer),
						"minecraft:crafting_special_armordye" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
						"minecraft:crafting_special_bookcloning" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
						"minecraft:crafting_special_mapcloning" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
						"minecraft:crafting_special_mapextending" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
						"minecraft:crafting_special_firework_rocket" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
						"minecraft:crafting_special_firework_star" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
						"minecraft:crafting_special_firework_star_fade" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
						"minecraft:crafting_special_repairitem" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
						"minecraft:crafting_special_tippedarrow" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
						"minecraft:crafting_special_bannerduplicate" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
						"minecraft:crafting_special_banneraddpattern" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
						"minecraft:crafting_special_shielddecoration" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
						"minecraft:crafting_special_shulkerboxcoloring" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
						"minecraft:crafting_special_suspiciousstew" => ((Func<PacketBuffer, object?>)((buffer) => buffer.ReadVoid()))(buffer),
						"minecraft:smelting" => ((Func<PacketBuffer, MinecraftSmeltingFormat>)((buffer) => Mine.Net.MinecraftSmeltingFormat.Read(buffer )))(buffer),
						"minecraft:blasting" => ((Func<PacketBuffer, MinecraftSmeltingFormat>)((buffer) => Mine.Net.MinecraftSmeltingFormat.Read(buffer )))(buffer),
						"minecraft:smoking" => ((Func<PacketBuffer, MinecraftSmeltingFormat>)((buffer) => Mine.Net.MinecraftSmeltingFormat.Read(buffer )))(buffer),
						"minecraft:campfire_cooking" => ((Func<PacketBuffer, MinecraftSmeltingFormat>)((buffer) => Mine.Net.MinecraftSmeltingFormat.Read(buffer )))(buffer),
						"minecraft:stonecutting" => ((Func<PacketBuffer, DataSwitchStateminecraftStonecuttingContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketDeclareRecipes.RecipesElementContainer.DataSwitch.DataSwitchStateminecraftStonecuttingContainer.Read(buffer )))(buffer),
						"minecraft:smithing" => ((Func<PacketBuffer, DataSwitchStateminecraftSmithingContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketDeclareRecipes.RecipesElementContainer.DataSwitch.DataSwitchStateminecraftSmithingContainer.Read(buffer )))(buffer),
						 _ => throw new Exception($"Invalid value: '{state}'")
					};
					return new DataSwitch(value);
				}
				public static implicit operator DataswitchstateminecraftCraftingShapelessContainer?(DataSwitch value) => (DataswitchstateminecraftCraftingShapelessContainer?)value.Value;
				public static implicit operator DataswitchstateminecraftCraftingShapedContainer?(DataSwitch value) => (DataswitchstateminecraftCraftingShapedContainer?)value.Value;
				public static implicit operator MinecraftSmeltingFormat?(DataSwitch value) => (MinecraftSmeltingFormat?)value.Value;
				public static implicit operator DataSwitchStateminecraftStonecuttingContainer?(DataSwitch value) => (DataSwitchStateminecraftStonecuttingContainer?)value.Value;
				public static implicit operator DataSwitchStateminecraftSmithingContainer?(DataSwitch value) => (DataSwitchStateminecraftSmithingContainer?)value.Value;
				public static implicit operator DataSwitch?(DataswitchstateminecraftCraftingShapelessContainer? value) => new DataSwitch(value);
				public static implicit operator DataSwitch?(DataswitchstateminecraftCraftingShapedContainer? value) => new DataSwitch(value);
				public static implicit operator DataSwitch?(MinecraftSmeltingFormat? value) => new DataSwitch(value);
				public static implicit operator DataSwitch?(DataSwitchStateminecraftStonecuttingContainer? value) => new DataSwitch(value);
				public static implicit operator DataSwitch?(DataSwitchStateminecraftSmithingContainer? value) => new DataSwitch(value);
			}
			public string Type { get; set; }
			public string RecipeId { get; set; }
			public DataSwitch Data { get; set; }
			public RecipesElementContainer(string @type, string @recipeId, DataSwitch @data) {
				this.Type = @type;
				this.RecipeId = @recipeId;
				this.Data = @data;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Type);
				((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.RecipeId);
				((Action<PacketBuffer, DataSwitch>)((buffer, value) => value.Write(buffer, Type)))(buffer, this.Data);
			}
			public static RecipesElementContainer Read(PacketBuffer buffer ) {
				string @type = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
				string @recipeId = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
				DataSwitch @data = ((Func<PacketBuffer, DataSwitch>)((buffer) => DataSwitch.Read(buffer, @type)))(buffer);
				return new RecipesElementContainer(@type, @recipeId, @data);
			}
		}
		public RecipesElementContainer[] Recipes { get; set; }
		public PacketDeclareRecipes(RecipesElementContainer[] @recipes) {
			this.Recipes = @recipes;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, RecipesElementContainer[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, RecipesElementContainer>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Recipes);
		}
		public static PacketDeclareRecipes Read(PacketBuffer buffer ) {
			RecipesElementContainer[] @recipes = ((Func<PacketBuffer, RecipesElementContainer[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, RecipesElementContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketDeclareRecipes.RecipesElementContainer.Read(buffer ))))))(buffer);
			return new PacketDeclareRecipes(@recipes);
		}
	}
	public class PacketTags : IPacketPayload {
		public class TagsElementContainer {
			public string TagType { get; set; }
			public TagsElement[] Tags { get; set; }
			public TagsElementContainer(string @tagType, TagsElement[] @tags) {
				this.TagType = @tagType;
				this.Tags = @tags;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.TagType);
				((Action<PacketBuffer, TagsElement[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, TagsElement>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Tags);
			}
			public static TagsElementContainer Read(PacketBuffer buffer ) {
				string @tagType = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
				TagsElement[] @tags = ((Func<PacketBuffer, TagsElement[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, TagsElement>)((buffer) => Mine.Net.TagsElement.Read(buffer ))))))(buffer);
				return new TagsElementContainer(@tagType, @tags);
			}
		}
		public TagsElementContainer[] Tags { get; set; }
		public PacketTags(TagsElementContainer[] @tags) {
			this.Tags = @tags;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, TagsElementContainer[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, TagsElementContainer>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Tags);
		}
		public static PacketTags Read(PacketBuffer buffer ) {
			TagsElementContainer[] @tags = ((Func<PacketBuffer, TagsElementContainer[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, TagsElementContainer>)((buffer) => Mine.Net.Play.Clientbound.PacketTags.TagsElementContainer.Read(buffer ))))))(buffer);
			return new PacketTags(@tags);
		}
	}
	public class PacketAcknowledgePlayerDigging : IPacketPayload {
		public VarInt SequenceId { get; set; }
		public PacketAcknowledgePlayerDigging(VarInt @sequenceId) {
			this.SequenceId = @sequenceId;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.SequenceId);
		}
		public static PacketAcknowledgePlayerDigging Read(PacketBuffer buffer ) {
			VarInt @sequenceId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketAcknowledgePlayerDigging(@sequenceId);
		}
	}
	public class PacketClearTitles : IPacketPayload {
		public bool Reset { get; set; }
		public PacketClearTitles(bool @reset) {
			this.Reset = @reset;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.Reset);
		}
		public static PacketClearTitles Read(PacketBuffer buffer ) {
			bool @reset = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			return new PacketClearTitles(@reset);
		}
	}
	public class PacketInitializeWorldBorder : IPacketPayload {
		public double X { get; set; }
		public double Z { get; set; }
		public double OldDiameter { get; set; }
		public double NewDiameter { get; set; }
		public VarInt Speed { get; set; }
		public VarInt PortalTeleportBoundary { get; set; }
		public VarInt WarningBlocks { get; set; }
		public VarInt WarningTime { get; set; }
		public PacketInitializeWorldBorder(double @x, double @z, double @oldDiameter, double @newDiameter, VarInt @speed, VarInt @portalTeleportBoundary, VarInt @warningBlocks, VarInt @warningTime) {
			this.X = @x;
			this.Z = @z;
			this.OldDiameter = @oldDiameter;
			this.NewDiameter = @newDiameter;
			this.Speed = @speed;
			this.PortalTeleportBoundary = @portalTeleportBoundary;
			this.WarningBlocks = @warningBlocks;
			this.WarningTime = @warningTime;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, this.X);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, this.Z);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, this.OldDiameter);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, this.NewDiameter);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.Speed);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.PortalTeleportBoundary);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.WarningBlocks);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.WarningTime);
		}
		public static PacketInitializeWorldBorder Read(PacketBuffer buffer ) {
			double @x = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @z = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @oldDiameter = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @newDiameter = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			VarInt @speed = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @portalTeleportBoundary = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @warningBlocks = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			VarInt @warningTime = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketInitializeWorldBorder(@x, @z, @oldDiameter, @newDiameter, @speed, @portalTeleportBoundary, @warningBlocks, @warningTime);
		}
	}
	public class PacketActionBar : IPacketPayload {
		public string Text { get; set; }
		public PacketActionBar(string @text) {
			this.Text = @text;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Text);
		}
		public static PacketActionBar Read(PacketBuffer buffer ) {
			string @text = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			return new PacketActionBar(@text);
		}
	}
	public class PacketWorldBorderCenter : IPacketPayload {
		public double X { get; set; }
		public double Z { get; set; }
		public PacketWorldBorderCenter(double @x, double @z) {
			this.X = @x;
			this.Z = @z;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, this.X);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, this.Z);
		}
		public static PacketWorldBorderCenter Read(PacketBuffer buffer ) {
			double @x = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @z = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			return new PacketWorldBorderCenter(@x, @z);
		}
	}
	public class PacketWorldBorderLerpSize : IPacketPayload {
		public double OldDiameter { get; set; }
		public double NewDiameter { get; set; }
		public VarInt Speed { get; set; }
		public PacketWorldBorderLerpSize(double @oldDiameter, double @newDiameter, VarInt @speed) {
			this.OldDiameter = @oldDiameter;
			this.NewDiameter = @newDiameter;
			this.Speed = @speed;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, this.OldDiameter);
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, this.NewDiameter);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.Speed);
		}
		public static PacketWorldBorderLerpSize Read(PacketBuffer buffer ) {
			double @oldDiameter = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			double @newDiameter = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			VarInt @speed = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketWorldBorderLerpSize(@oldDiameter, @newDiameter, @speed);
		}
	}
	public class PacketWorldBorderSize : IPacketPayload {
		public double Diameter { get; set; }
		public PacketWorldBorderSize(double @diameter) {
			this.Diameter = @diameter;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, double>)((buffer, value) => buffer.WriteF64(value)))(buffer, this.Diameter);
		}
		public static PacketWorldBorderSize Read(PacketBuffer buffer ) {
			double @diameter = ((Func<PacketBuffer, double>)((buffer) => buffer.ReadF64()))(buffer);
			return new PacketWorldBorderSize(@diameter);
		}
	}
	public class PacketWorldBorderWarningDelay : IPacketPayload {
		public VarInt WarningTime { get; set; }
		public PacketWorldBorderWarningDelay(VarInt @warningTime) {
			this.WarningTime = @warningTime;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.WarningTime);
		}
		public static PacketWorldBorderWarningDelay Read(PacketBuffer buffer ) {
			VarInt @warningTime = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketWorldBorderWarningDelay(@warningTime);
		}
	}
	public class PacketWorldBorderWarningReach : IPacketPayload {
		public VarInt WarningBlocks { get; set; }
		public PacketWorldBorderWarningReach(VarInt @warningBlocks) {
			this.WarningBlocks = @warningBlocks;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.WarningBlocks);
		}
		public static PacketWorldBorderWarningReach Read(PacketBuffer buffer ) {
			VarInt @warningBlocks = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketWorldBorderWarningReach(@warningBlocks);
		}
	}
	public class PacketPing : IPacketPayload {
		public int Id { get; set; }
		public PacketPing(int @id) {
			this.Id = @id;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, this.Id);
		}
		public static PacketPing Read(PacketBuffer buffer ) {
			int @id = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			return new PacketPing(@id);
		}
	}
	public class PacketSetTitleSubtitle : IPacketPayload {
		public string Text { get; set; }
		public PacketSetTitleSubtitle(string @text) {
			this.Text = @text;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Text);
		}
		public static PacketSetTitleSubtitle Read(PacketBuffer buffer ) {
			string @text = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			return new PacketSetTitleSubtitle(@text);
		}
	}
	public class PacketSetTitleText : IPacketPayload {
		public string Text { get; set; }
		public PacketSetTitleText(string @text) {
			this.Text = @text;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Text);
		}
		public static PacketSetTitleText Read(PacketBuffer buffer ) {
			string @text = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			return new PacketSetTitleText(@text);
		}
	}
	public class PacketSetTitleTime : IPacketPayload {
		public int FadeIn { get; set; }
		public int Stay { get; set; }
		public int FadeOut { get; set; }
		public PacketSetTitleTime(int @fadeIn, int @stay, int @fadeOut) {
			this.FadeIn = @fadeIn;
			this.Stay = @stay;
			this.FadeOut = @fadeOut;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, this.FadeIn);
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, this.Stay);
			((Action<PacketBuffer, int>)((buffer, value) => buffer.WriteI32(value)))(buffer, this.FadeOut);
		}
		public static PacketSetTitleTime Read(PacketBuffer buffer ) {
			int @fadeIn = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			int @stay = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			int @fadeOut = ((Func<PacketBuffer, int>)((buffer) => buffer.ReadI32()))(buffer);
			return new PacketSetTitleTime(@fadeIn, @stay, @fadeOut);
		}
	}
	public class PacketSimulationDistance : IPacketPayload {
		public VarInt Distance { get; set; }
		public PacketSimulationDistance(VarInt @distance) {
			this.Distance = @distance;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.Distance);
		}
		public static PacketSimulationDistance Read(PacketBuffer buffer ) {
			VarInt @distance = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketSimulationDistance(@distance);
		}
	}
	public class PacketMessageHeader : IPacketPayload {
		public byte[]? MessageSignature { get; set; }
		public UUID SenderUuid { get; set; }
		public byte[] HeaderSignature { get; set; }
		public byte[] BodyDigest { get; set; }
		public PacketMessageHeader(byte[]? @messageSignature, UUID @senderUuid, byte[] @headerSignature, byte[] @bodyDigest) {
			this.MessageSignature = @messageSignature;
			this.SenderUuid = @senderUuid;
			this.HeaderSignature = @headerSignature;
			this.BodyDigest = @bodyDigest;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, byte[]?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))))))(buffer, this.MessageSignature);
			((Action<PacketBuffer, UUID>)((buffer, value) => buffer.WriteUUID(value)))(buffer, this.SenderUuid);
			((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.HeaderSignature);
			((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.BodyDigest);
		}
		public static PacketMessageHeader Read(PacketBuffer buffer ) {
			byte[]? @messageSignature = ((Func<PacketBuffer, byte[]?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))))))))(buffer);
			UUID @senderUuid = ((Func<PacketBuffer, UUID>)((buffer) => buffer.ReadUUID()))(buffer);
			byte[] @headerSignature = ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8())))))(buffer);
			byte[] @bodyDigest = ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8())))))(buffer);
			return new PacketMessageHeader(@messageSignature, @senderUuid, @headerSignature, @bodyDigest);
		}
	}
	public class Packet : IPacket {
		public class ParamsSwitch {
			public object? Value { get; set; }
			public ParamsSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, string state) {
				switch (state) {
					case "spawn_entity": ((Action<PacketBuffer, PacketSpawnEntity>)((buffer, value) => value.Write(buffer )))(buffer, (PacketSpawnEntity)this); break;
					case "spawn_entity_experience_orb": ((Action<PacketBuffer, PacketSpawnEntityExperienceOrb>)((buffer, value) => value.Write(buffer )))(buffer, (PacketSpawnEntityExperienceOrb)this); break;
					case "named_entity_spawn": ((Action<PacketBuffer, PacketNamedEntitySpawn>)((buffer, value) => value.Write(buffer )))(buffer, (PacketNamedEntitySpawn)this); break;
					case "animation": ((Action<PacketBuffer, PacketAnimation>)((buffer, value) => value.Write(buffer )))(buffer, (PacketAnimation)this); break;
					case "statistics": ((Action<PacketBuffer, PacketStatistics>)((buffer, value) => value.Write(buffer )))(buffer, (PacketStatistics)this); break;
					case "acknowledge_player_digging": ((Action<PacketBuffer, PacketAcknowledgePlayerDigging>)((buffer, value) => value.Write(buffer )))(buffer, (PacketAcknowledgePlayerDigging)this); break;
					case "block_break_animation": ((Action<PacketBuffer, PacketBlockBreakAnimation>)((buffer, value) => value.Write(buffer )))(buffer, (PacketBlockBreakAnimation)this); break;
					case "tile_entity_data": ((Action<PacketBuffer, PacketTileEntityData>)((buffer, value) => value.Write(buffer )))(buffer, (PacketTileEntityData)this); break;
					case "block_action": ((Action<PacketBuffer, PacketBlockAction>)((buffer, value) => value.Write(buffer )))(buffer, (PacketBlockAction)this); break;
					case "block_change": ((Action<PacketBuffer, PacketBlockChange>)((buffer, value) => value.Write(buffer )))(buffer, (PacketBlockChange)this); break;
					case "boss_bar": ((Action<PacketBuffer, PacketBossBar>)((buffer, value) => value.Write(buffer )))(buffer, (PacketBossBar)this); break;
					case "difficulty": ((Action<PacketBuffer, PacketDifficulty>)((buffer, value) => value.Write(buffer )))(buffer, (PacketDifficulty)this); break;
					case "chat_preview": ((Action<PacketBuffer, PacketChatPreview>)((buffer, value) => value.Write(buffer )))(buffer, (PacketChatPreview)this); break;
					case "clear_titles": ((Action<PacketBuffer, PacketClearTitles>)((buffer, value) => value.Write(buffer )))(buffer, (PacketClearTitles)this); break;
					case "tab_complete": ((Action<PacketBuffer, PacketTabComplete>)((buffer, value) => value.Write(buffer )))(buffer, (PacketTabComplete)this); break;
					case "declare_commands": ((Action<PacketBuffer, PacketDeclareCommands>)((buffer, value) => value.Write(buffer )))(buffer, (PacketDeclareCommands)this); break;
					case "close_window": ((Action<PacketBuffer, PacketCloseWindow>)((buffer, value) => value.Write(buffer )))(buffer, (PacketCloseWindow)this); break;
					case "window_items": ((Action<PacketBuffer, PacketWindowItems>)((buffer, value) => value.Write(buffer )))(buffer, (PacketWindowItems)this); break;
					case "craft_progress_bar": ((Action<PacketBuffer, PacketCraftProgressBar>)((buffer, value) => value.Write(buffer )))(buffer, (PacketCraftProgressBar)this); break;
					case "set_slot": ((Action<PacketBuffer, PacketSetSlot>)((buffer, value) => value.Write(buffer )))(buffer, (PacketSetSlot)this); break;
					case "set_cooldown": ((Action<PacketBuffer, PacketSetCooldown>)((buffer, value) => value.Write(buffer )))(buffer, (PacketSetCooldown)this); break;
					case "chat_suggestions": ((Action<PacketBuffer, PacketChatSuggestions>)((buffer, value) => value.Write(buffer )))(buffer, (PacketChatSuggestions)this); break;
					case "custom_payload": ((Action<PacketBuffer, PacketCustomPayload>)((buffer, value) => value.Write(buffer )))(buffer, (PacketCustomPayload)this); break;
					case "named_sound_effect": ((Action<PacketBuffer, PacketNamedSoundEffect>)((buffer, value) => value.Write(buffer )))(buffer, (PacketNamedSoundEffect)this); break;
					case "hide_message": ((Action<PacketBuffer, PacketHideMessage>)((buffer, value) => value.Write(buffer )))(buffer, (PacketHideMessage)this); break;
					case "kick_disconnect": ((Action<PacketBuffer, PacketKickDisconnect>)((buffer, value) => value.Write(buffer )))(buffer, (PacketKickDisconnect)this); break;
					case "entity_status": ((Action<PacketBuffer, PacketEntityStatus>)((buffer, value) => value.Write(buffer )))(buffer, (PacketEntityStatus)this); break;
					case "explosion": ((Action<PacketBuffer, PacketExplosion>)((buffer, value) => value.Write(buffer )))(buffer, (PacketExplosion)this); break;
					case "unload_chunk": ((Action<PacketBuffer, PacketUnloadChunk>)((buffer, value) => value.Write(buffer )))(buffer, (PacketUnloadChunk)this); break;
					case "game_state_change": ((Action<PacketBuffer, PacketGameStateChange>)((buffer, value) => value.Write(buffer )))(buffer, (PacketGameStateChange)this); break;
					case "open_horse_window": ((Action<PacketBuffer, PacketOpenHorseWindow>)((buffer, value) => value.Write(buffer )))(buffer, (PacketOpenHorseWindow)this); break;
					case "initialize_world_border": ((Action<PacketBuffer, PacketInitializeWorldBorder>)((buffer, value) => value.Write(buffer )))(buffer, (PacketInitializeWorldBorder)this); break;
					case "keep_alive": ((Action<PacketBuffer, PacketKeepAlive>)((buffer, value) => value.Write(buffer )))(buffer, (PacketKeepAlive)this); break;
					case "map_chunk": ((Action<PacketBuffer, PacketMapChunk>)((buffer, value) => value.Write(buffer )))(buffer, (PacketMapChunk)this); break;
					case "world_event": ((Action<PacketBuffer, PacketWorldEvent>)((buffer, value) => value.Write(buffer )))(buffer, (PacketWorldEvent)this); break;
					case "world_particles": ((Action<PacketBuffer, PacketWorldParticles>)((buffer, value) => value.Write(buffer )))(buffer, (PacketWorldParticles)this); break;
					case "update_light": ((Action<PacketBuffer, PacketUpdateLight>)((buffer, value) => value.Write(buffer )))(buffer, (PacketUpdateLight)this); break;
					case "login": ((Action<PacketBuffer, PacketLogin>)((buffer, value) => value.Write(buffer )))(buffer, (PacketLogin)this); break;
					case "map": ((Action<PacketBuffer, PacketMap>)((buffer, value) => value.Write(buffer )))(buffer, (PacketMap)this); break;
					case "trade_list": ((Action<PacketBuffer, PacketTradeList>)((buffer, value) => value.Write(buffer )))(buffer, (PacketTradeList)this); break;
					case "rel_entity_move": ((Action<PacketBuffer, PacketRelEntityMove>)((buffer, value) => value.Write(buffer )))(buffer, (PacketRelEntityMove)this); break;
					case "entity_move_look": ((Action<PacketBuffer, PacketEntityMoveLook>)((buffer, value) => value.Write(buffer )))(buffer, (PacketEntityMoveLook)this); break;
					case "entity_look": ((Action<PacketBuffer, PacketEntityLook>)((buffer, value) => value.Write(buffer )))(buffer, (PacketEntityLook)this); break;
					case "vehicle_move": ((Action<PacketBuffer, PacketVehicleMove>)((buffer, value) => value.Write(buffer )))(buffer, (PacketVehicleMove)this); break;
					case "open_book": ((Action<PacketBuffer, PacketOpenBook>)((buffer, value) => value.Write(buffer )))(buffer, (PacketOpenBook)this); break;
					case "open_window": ((Action<PacketBuffer, PacketOpenWindow>)((buffer, value) => value.Write(buffer )))(buffer, (PacketOpenWindow)this); break;
					case "open_sign_entity": ((Action<PacketBuffer, PacketOpenSignEntity>)((buffer, value) => value.Write(buffer )))(buffer, (PacketOpenSignEntity)this); break;
					case "ping": ((Action<PacketBuffer, PacketPing>)((buffer, value) => value.Write(buffer )))(buffer, (PacketPing)this); break;
					case "craft_recipe_response": ((Action<PacketBuffer, PacketCraftRecipeResponse>)((buffer, value) => value.Write(buffer )))(buffer, (PacketCraftRecipeResponse)this); break;
					case "abilities": ((Action<PacketBuffer, PacketAbilities>)((buffer, value) => value.Write(buffer )))(buffer, (PacketAbilities)this); break;
					case "message_header": ((Action<PacketBuffer, PacketMessageHeader>)((buffer, value) => value.Write(buffer )))(buffer, (PacketMessageHeader)this); break;
					case "player_chat": ((Action<PacketBuffer, PacketPlayerChat>)((buffer, value) => value.Write(buffer )))(buffer, (PacketPlayerChat)this); break;
					case "end_combat_event": ((Action<PacketBuffer, PacketEndCombatEvent>)((buffer, value) => value.Write(buffer )))(buffer, (PacketEndCombatEvent)this); break;
					case "enter_combat_event": ((Action<PacketBuffer, PacketEnterCombatEvent>)((buffer, value) => value.Write(buffer )))(buffer, (PacketEnterCombatEvent)this); break;
					case "death_combat_event": ((Action<PacketBuffer, PacketDeathCombatEvent>)((buffer, value) => value.Write(buffer )))(buffer, (PacketDeathCombatEvent)this); break;
					case "player_info": ((Action<PacketBuffer, PacketPlayerInfo>)((buffer, value) => value.Write(buffer )))(buffer, (PacketPlayerInfo)this); break;
					case "face_player": ((Action<PacketBuffer, PacketFacePlayer>)((buffer, value) => value.Write(buffer )))(buffer, (PacketFacePlayer)this); break;
					case "position": ((Action<PacketBuffer, PacketPosition>)((buffer, value) => value.Write(buffer )))(buffer, (PacketPosition)this); break;
					case "unlock_recipes": ((Action<PacketBuffer, PacketUnlockRecipes>)((buffer, value) => value.Write(buffer )))(buffer, (PacketUnlockRecipes)this); break;
					case "entity_destroy": ((Action<PacketBuffer, PacketEntityDestroy>)((buffer, value) => value.Write(buffer )))(buffer, (PacketEntityDestroy)this); break;
					case "remove_entity_effect": ((Action<PacketBuffer, PacketRemoveEntityEffect>)((buffer, value) => value.Write(buffer )))(buffer, (PacketRemoveEntityEffect)this); break;
					case "resource_pack_send": ((Action<PacketBuffer, PacketResourcePackSend>)((buffer, value) => value.Write(buffer )))(buffer, (PacketResourcePackSend)this); break;
					case "respawn": ((Action<PacketBuffer, PacketRespawn>)((buffer, value) => value.Write(buffer )))(buffer, (PacketRespawn)this); break;
					case "entity_head_rotation": ((Action<PacketBuffer, PacketEntityHeadRotation>)((buffer, value) => value.Write(buffer )))(buffer, (PacketEntityHeadRotation)this); break;
					case "multi_block_change": ((Action<PacketBuffer, PacketMultiBlockChange>)((buffer, value) => value.Write(buffer )))(buffer, (PacketMultiBlockChange)this); break;
					case "select_advancement_tab": ((Action<PacketBuffer, PacketSelectAdvancementTab>)((buffer, value) => value.Write(buffer )))(buffer, (PacketSelectAdvancementTab)this); break;
					case "server_data": ((Action<PacketBuffer, PacketServerData>)((buffer, value) => value.Write(buffer )))(buffer, (PacketServerData)this); break;
					case "action_bar": ((Action<PacketBuffer, PacketActionBar>)((buffer, value) => value.Write(buffer )))(buffer, (PacketActionBar)this); break;
					case "world_border_center": ((Action<PacketBuffer, PacketWorldBorderCenter>)((buffer, value) => value.Write(buffer )))(buffer, (PacketWorldBorderCenter)this); break;
					case "world_border_lerp_size": ((Action<PacketBuffer, PacketWorldBorderLerpSize>)((buffer, value) => value.Write(buffer )))(buffer, (PacketWorldBorderLerpSize)this); break;
					case "world_border_size": ((Action<PacketBuffer, PacketWorldBorderSize>)((buffer, value) => value.Write(buffer )))(buffer, (PacketWorldBorderSize)this); break;
					case "world_border_warning_delay": ((Action<PacketBuffer, PacketWorldBorderWarningDelay>)((buffer, value) => value.Write(buffer )))(buffer, (PacketWorldBorderWarningDelay)this); break;
					case "world_border_warning_reach": ((Action<PacketBuffer, PacketWorldBorderWarningReach>)((buffer, value) => value.Write(buffer )))(buffer, (PacketWorldBorderWarningReach)this); break;
					case "camera": ((Action<PacketBuffer, PacketCamera>)((buffer, value) => value.Write(buffer )))(buffer, (PacketCamera)this); break;
					case "held_item_slot": ((Action<PacketBuffer, PacketHeldItemSlot>)((buffer, value) => value.Write(buffer )))(buffer, (PacketHeldItemSlot)this); break;
					case "update_view_position": ((Action<PacketBuffer, PacketUpdateViewPosition>)((buffer, value) => value.Write(buffer )))(buffer, (PacketUpdateViewPosition)this); break;
					case "update_view_distance": ((Action<PacketBuffer, PacketUpdateViewDistance>)((buffer, value) => value.Write(buffer )))(buffer, (PacketUpdateViewDistance)this); break;
					case "spawn_position": ((Action<PacketBuffer, PacketSpawnPosition>)((buffer, value) => value.Write(buffer )))(buffer, (PacketSpawnPosition)this); break;
					case "should_display_chat_preview": ((Action<PacketBuffer, PacketShouldDisplayChatPreview>)((buffer, value) => value.Write(buffer )))(buffer, (PacketShouldDisplayChatPreview)this); break;
					case "scoreboard_display_objective": ((Action<PacketBuffer, PacketScoreboardDisplayObjective>)((buffer, value) => value.Write(buffer )))(buffer, (PacketScoreboardDisplayObjective)this); break;
					case "entity_metadata": ((Action<PacketBuffer, PacketEntityMetadata>)((buffer, value) => value.Write(buffer )))(buffer, (PacketEntityMetadata)this); break;
					case "attach_entity": ((Action<PacketBuffer, PacketAttachEntity>)((buffer, value) => value.Write(buffer )))(buffer, (PacketAttachEntity)this); break;
					case "entity_velocity": ((Action<PacketBuffer, PacketEntityVelocity>)((buffer, value) => value.Write(buffer )))(buffer, (PacketEntityVelocity)this); break;
					case "entity_equipment": ((Action<PacketBuffer, PacketEntityEquipment>)((buffer, value) => value.Write(buffer )))(buffer, (PacketEntityEquipment)this); break;
					case "experience": ((Action<PacketBuffer, PacketExperience>)((buffer, value) => value.Write(buffer )))(buffer, (PacketExperience)this); break;
					case "update_health": ((Action<PacketBuffer, PacketUpdateHealth>)((buffer, value) => value.Write(buffer )))(buffer, (PacketUpdateHealth)this); break;
					case "scoreboard_objective": ((Action<PacketBuffer, PacketScoreboardObjective>)((buffer, value) => value.Write(buffer )))(buffer, (PacketScoreboardObjective)this); break;
					case "set_passengers": ((Action<PacketBuffer, PacketSetPassengers>)((buffer, value) => value.Write(buffer )))(buffer, (PacketSetPassengers)this); break;
					case "teams": ((Action<PacketBuffer, PacketTeams>)((buffer, value) => value.Write(buffer )))(buffer, (PacketTeams)this); break;
					case "scoreboard_score": ((Action<PacketBuffer, PacketScoreboardScore>)((buffer, value) => value.Write(buffer )))(buffer, (PacketScoreboardScore)this); break;
					case "simulation_distance": ((Action<PacketBuffer, PacketSimulationDistance>)((buffer, value) => value.Write(buffer )))(buffer, (PacketSimulationDistance)this); break;
					case "set_title_subtitle": ((Action<PacketBuffer, PacketSetTitleSubtitle>)((buffer, value) => value.Write(buffer )))(buffer, (PacketSetTitleSubtitle)this); break;
					case "update_time": ((Action<PacketBuffer, PacketUpdateTime>)((buffer, value) => value.Write(buffer )))(buffer, (PacketUpdateTime)this); break;
					case "set_title_text": ((Action<PacketBuffer, PacketSetTitleText>)((buffer, value) => value.Write(buffer )))(buffer, (PacketSetTitleText)this); break;
					case "set_title_time": ((Action<PacketBuffer, PacketSetTitleTime>)((buffer, value) => value.Write(buffer )))(buffer, (PacketSetTitleTime)this); break;
					case "entity_sound_effect": ((Action<PacketBuffer, PacketEntitySoundEffect>)((buffer, value) => value.Write(buffer )))(buffer, (PacketEntitySoundEffect)this); break;
					case "sound_effect": ((Action<PacketBuffer, PacketSoundEffect>)((buffer, value) => value.Write(buffer )))(buffer, (PacketSoundEffect)this); break;
					case "stop_sound": ((Action<PacketBuffer, PacketStopSound>)((buffer, value) => value.Write(buffer )))(buffer, (PacketStopSound)this); break;
					case "system_chat": ((Action<PacketBuffer, PacketSystemChat>)((buffer, value) => value.Write(buffer )))(buffer, (PacketSystemChat)this); break;
					case "playerlist_header": ((Action<PacketBuffer, PacketPlayerlistHeader>)((buffer, value) => value.Write(buffer )))(buffer, (PacketPlayerlistHeader)this); break;
					case "nbt_query_response": ((Action<PacketBuffer, PacketNbtQueryResponse>)((buffer, value) => value.Write(buffer )))(buffer, (PacketNbtQueryResponse)this); break;
					case "collect": ((Action<PacketBuffer, PacketCollect>)((buffer, value) => value.Write(buffer )))(buffer, (PacketCollect)this); break;
					case "entity_teleport": ((Action<PacketBuffer, PacketEntityTeleport>)((buffer, value) => value.Write(buffer )))(buffer, (PacketEntityTeleport)this); break;
					case "advancements": ((Action<PacketBuffer, PacketAdvancements>)((buffer, value) => value.Write(buffer )))(buffer, (PacketAdvancements)this); break;
					case "entity_update_attributes": ((Action<PacketBuffer, PacketEntityUpdateAttributes>)((buffer, value) => value.Write(buffer )))(buffer, (PacketEntityUpdateAttributes)this); break;
					case "entity_effect": ((Action<PacketBuffer, PacketEntityEffect>)((buffer, value) => value.Write(buffer )))(buffer, (PacketEntityEffect)this); break;
					case "declare_recipes": ((Action<PacketBuffer, PacketDeclareRecipes>)((buffer, value) => value.Write(buffer )))(buffer, (PacketDeclareRecipes)this); break;
					case "tags": ((Action<PacketBuffer, PacketTags>)((buffer, value) => value.Write(buffer )))(buffer, (PacketTags)this); break;
					default: throw new Exception($"Invalid value: '{state}'");
				}
			}
			public static ParamsSwitch Read(PacketBuffer buffer, string state) {
				object? value = state switch {
					"spawn_entity" => ((Func<PacketBuffer, PacketSpawnEntity>)((buffer) => Mine.Net.Play.Clientbound.PacketSpawnEntity.Read(buffer )))(buffer),
					"spawn_entity_experience_orb" => ((Func<PacketBuffer, PacketSpawnEntityExperienceOrb>)((buffer) => Mine.Net.Play.Clientbound.PacketSpawnEntityExperienceOrb.Read(buffer )))(buffer),
					"named_entity_spawn" => ((Func<PacketBuffer, PacketNamedEntitySpawn>)((buffer) => Mine.Net.Play.Clientbound.PacketNamedEntitySpawn.Read(buffer )))(buffer),
					"animation" => ((Func<PacketBuffer, PacketAnimation>)((buffer) => Mine.Net.Play.Clientbound.PacketAnimation.Read(buffer )))(buffer),
					"statistics" => ((Func<PacketBuffer, PacketStatistics>)((buffer) => Mine.Net.Play.Clientbound.PacketStatistics.Read(buffer )))(buffer),
					"acknowledge_player_digging" => ((Func<PacketBuffer, PacketAcknowledgePlayerDigging>)((buffer) => Mine.Net.Play.Clientbound.PacketAcknowledgePlayerDigging.Read(buffer )))(buffer),
					"block_break_animation" => ((Func<PacketBuffer, PacketBlockBreakAnimation>)((buffer) => Mine.Net.Play.Clientbound.PacketBlockBreakAnimation.Read(buffer )))(buffer),
					"tile_entity_data" => ((Func<PacketBuffer, PacketTileEntityData>)((buffer) => Mine.Net.Play.Clientbound.PacketTileEntityData.Read(buffer )))(buffer),
					"block_action" => ((Func<PacketBuffer, PacketBlockAction>)((buffer) => Mine.Net.Play.Clientbound.PacketBlockAction.Read(buffer )))(buffer),
					"block_change" => ((Func<PacketBuffer, PacketBlockChange>)((buffer) => Mine.Net.Play.Clientbound.PacketBlockChange.Read(buffer )))(buffer),
					"boss_bar" => ((Func<PacketBuffer, PacketBossBar>)((buffer) => Mine.Net.Play.Clientbound.PacketBossBar.Read(buffer )))(buffer),
					"difficulty" => ((Func<PacketBuffer, PacketDifficulty>)((buffer) => Mine.Net.Play.Clientbound.PacketDifficulty.Read(buffer )))(buffer),
					"chat_preview" => ((Func<PacketBuffer, PacketChatPreview>)((buffer) => Mine.Net.Play.Clientbound.PacketChatPreview.Read(buffer )))(buffer),
					"clear_titles" => ((Func<PacketBuffer, PacketClearTitles>)((buffer) => Mine.Net.Play.Clientbound.PacketClearTitles.Read(buffer )))(buffer),
					"tab_complete" => ((Func<PacketBuffer, PacketTabComplete>)((buffer) => Mine.Net.Play.Clientbound.PacketTabComplete.Read(buffer )))(buffer),
					"declare_commands" => ((Func<PacketBuffer, PacketDeclareCommands>)((buffer) => Mine.Net.Play.Clientbound.PacketDeclareCommands.Read(buffer )))(buffer),
					"close_window" => ((Func<PacketBuffer, PacketCloseWindow>)((buffer) => Mine.Net.Play.Clientbound.PacketCloseWindow.Read(buffer )))(buffer),
					"window_items" => ((Func<PacketBuffer, PacketWindowItems>)((buffer) => Mine.Net.Play.Clientbound.PacketWindowItems.Read(buffer )))(buffer),
					"craft_progress_bar" => ((Func<PacketBuffer, PacketCraftProgressBar>)((buffer) => Mine.Net.Play.Clientbound.PacketCraftProgressBar.Read(buffer )))(buffer),
					"set_slot" => ((Func<PacketBuffer, PacketSetSlot>)((buffer) => Mine.Net.Play.Clientbound.PacketSetSlot.Read(buffer )))(buffer),
					"set_cooldown" => ((Func<PacketBuffer, PacketSetCooldown>)((buffer) => Mine.Net.Play.Clientbound.PacketSetCooldown.Read(buffer )))(buffer),
					"chat_suggestions" => ((Func<PacketBuffer, PacketChatSuggestions>)((buffer) => Mine.Net.Play.Clientbound.PacketChatSuggestions.Read(buffer )))(buffer),
					"custom_payload" => ((Func<PacketBuffer, PacketCustomPayload>)((buffer) => Mine.Net.Play.Clientbound.PacketCustomPayload.Read(buffer )))(buffer),
					"named_sound_effect" => ((Func<PacketBuffer, PacketNamedSoundEffect>)((buffer) => Mine.Net.Play.Clientbound.PacketNamedSoundEffect.Read(buffer )))(buffer),
					"hide_message" => ((Func<PacketBuffer, PacketHideMessage>)((buffer) => Mine.Net.Play.Clientbound.PacketHideMessage.Read(buffer )))(buffer),
					"kick_disconnect" => ((Func<PacketBuffer, PacketKickDisconnect>)((buffer) => Mine.Net.Play.Clientbound.PacketKickDisconnect.Read(buffer )))(buffer),
					"entity_status" => ((Func<PacketBuffer, PacketEntityStatus>)((buffer) => Mine.Net.Play.Clientbound.PacketEntityStatus.Read(buffer )))(buffer),
					"explosion" => ((Func<PacketBuffer, PacketExplosion>)((buffer) => Mine.Net.Play.Clientbound.PacketExplosion.Read(buffer )))(buffer),
					"unload_chunk" => ((Func<PacketBuffer, PacketUnloadChunk>)((buffer) => Mine.Net.Play.Clientbound.PacketUnloadChunk.Read(buffer )))(buffer),
					"game_state_change" => ((Func<PacketBuffer, PacketGameStateChange>)((buffer) => Mine.Net.Play.Clientbound.PacketGameStateChange.Read(buffer )))(buffer),
					"open_horse_window" => ((Func<PacketBuffer, PacketOpenHorseWindow>)((buffer) => Mine.Net.Play.Clientbound.PacketOpenHorseWindow.Read(buffer )))(buffer),
					"initialize_world_border" => ((Func<PacketBuffer, PacketInitializeWorldBorder>)((buffer) => Mine.Net.Play.Clientbound.PacketInitializeWorldBorder.Read(buffer )))(buffer),
					"keep_alive" => ((Func<PacketBuffer, PacketKeepAlive>)((buffer) => Mine.Net.Play.Clientbound.PacketKeepAlive.Read(buffer )))(buffer),
					"map_chunk" => ((Func<PacketBuffer, PacketMapChunk>)((buffer) => Mine.Net.Play.Clientbound.PacketMapChunk.Read(buffer )))(buffer),
					"world_event" => ((Func<PacketBuffer, PacketWorldEvent>)((buffer) => Mine.Net.Play.Clientbound.PacketWorldEvent.Read(buffer )))(buffer),
					"world_particles" => ((Func<PacketBuffer, PacketWorldParticles>)((buffer) => Mine.Net.Play.Clientbound.PacketWorldParticles.Read(buffer )))(buffer),
					"update_light" => ((Func<PacketBuffer, PacketUpdateLight>)((buffer) => Mine.Net.Play.Clientbound.PacketUpdateLight.Read(buffer )))(buffer),
					"login" => ((Func<PacketBuffer, PacketLogin>)((buffer) => Mine.Net.Play.Clientbound.PacketLogin.Read(buffer )))(buffer),
					"map" => ((Func<PacketBuffer, PacketMap>)((buffer) => Mine.Net.Play.Clientbound.PacketMap.Read(buffer )))(buffer),
					"trade_list" => ((Func<PacketBuffer, PacketTradeList>)((buffer) => Mine.Net.Play.Clientbound.PacketTradeList.Read(buffer )))(buffer),
					"rel_entity_move" => ((Func<PacketBuffer, PacketRelEntityMove>)((buffer) => Mine.Net.Play.Clientbound.PacketRelEntityMove.Read(buffer )))(buffer),
					"entity_move_look" => ((Func<PacketBuffer, PacketEntityMoveLook>)((buffer) => Mine.Net.Play.Clientbound.PacketEntityMoveLook.Read(buffer )))(buffer),
					"entity_look" => ((Func<PacketBuffer, PacketEntityLook>)((buffer) => Mine.Net.Play.Clientbound.PacketEntityLook.Read(buffer )))(buffer),
					"vehicle_move" => ((Func<PacketBuffer, PacketVehicleMove>)((buffer) => Mine.Net.Play.Clientbound.PacketVehicleMove.Read(buffer )))(buffer),
					"open_book" => ((Func<PacketBuffer, PacketOpenBook>)((buffer) => Mine.Net.Play.Clientbound.PacketOpenBook.Read(buffer )))(buffer),
					"open_window" => ((Func<PacketBuffer, PacketOpenWindow>)((buffer) => Mine.Net.Play.Clientbound.PacketOpenWindow.Read(buffer )))(buffer),
					"open_sign_entity" => ((Func<PacketBuffer, PacketOpenSignEntity>)((buffer) => Mine.Net.Play.Clientbound.PacketOpenSignEntity.Read(buffer )))(buffer),
					"ping" => ((Func<PacketBuffer, PacketPing>)((buffer) => Mine.Net.Play.Clientbound.PacketPing.Read(buffer )))(buffer),
					"craft_recipe_response" => ((Func<PacketBuffer, PacketCraftRecipeResponse>)((buffer) => Mine.Net.Play.Clientbound.PacketCraftRecipeResponse.Read(buffer )))(buffer),
					"abilities" => ((Func<PacketBuffer, PacketAbilities>)((buffer) => Mine.Net.Play.Clientbound.PacketAbilities.Read(buffer )))(buffer),
					"message_header" => ((Func<PacketBuffer, PacketMessageHeader>)((buffer) => Mine.Net.Play.Clientbound.PacketMessageHeader.Read(buffer )))(buffer),
					"player_chat" => ((Func<PacketBuffer, PacketPlayerChat>)((buffer) => Mine.Net.Play.Clientbound.PacketPlayerChat.Read(buffer )))(buffer),
					"end_combat_event" => ((Func<PacketBuffer, PacketEndCombatEvent>)((buffer) => Mine.Net.Play.Clientbound.PacketEndCombatEvent.Read(buffer )))(buffer),
					"enter_combat_event" => ((Func<PacketBuffer, PacketEnterCombatEvent>)((buffer) => Mine.Net.Play.Clientbound.PacketEnterCombatEvent.Read(buffer )))(buffer),
					"death_combat_event" => ((Func<PacketBuffer, PacketDeathCombatEvent>)((buffer) => Mine.Net.Play.Clientbound.PacketDeathCombatEvent.Read(buffer )))(buffer),
					"player_info" => ((Func<PacketBuffer, PacketPlayerInfo>)((buffer) => Mine.Net.Play.Clientbound.PacketPlayerInfo.Read(buffer )))(buffer),
					"face_player" => ((Func<PacketBuffer, PacketFacePlayer>)((buffer) => Mine.Net.Play.Clientbound.PacketFacePlayer.Read(buffer )))(buffer),
					"position" => ((Func<PacketBuffer, PacketPosition>)((buffer) => Mine.Net.Play.Clientbound.PacketPosition.Read(buffer )))(buffer),
					"unlock_recipes" => ((Func<PacketBuffer, PacketUnlockRecipes>)((buffer) => Mine.Net.Play.Clientbound.PacketUnlockRecipes.Read(buffer )))(buffer),
					"entity_destroy" => ((Func<PacketBuffer, PacketEntityDestroy>)((buffer) => Mine.Net.Play.Clientbound.PacketEntityDestroy.Read(buffer )))(buffer),
					"remove_entity_effect" => ((Func<PacketBuffer, PacketRemoveEntityEffect>)((buffer) => Mine.Net.Play.Clientbound.PacketRemoveEntityEffect.Read(buffer )))(buffer),
					"resource_pack_send" => ((Func<PacketBuffer, PacketResourcePackSend>)((buffer) => Mine.Net.Play.Clientbound.PacketResourcePackSend.Read(buffer )))(buffer),
					"respawn" => ((Func<PacketBuffer, PacketRespawn>)((buffer) => Mine.Net.Play.Clientbound.PacketRespawn.Read(buffer )))(buffer),
					"entity_head_rotation" => ((Func<PacketBuffer, PacketEntityHeadRotation>)((buffer) => Mine.Net.Play.Clientbound.PacketEntityHeadRotation.Read(buffer )))(buffer),
					"multi_block_change" => ((Func<PacketBuffer, PacketMultiBlockChange>)((buffer) => Mine.Net.Play.Clientbound.PacketMultiBlockChange.Read(buffer )))(buffer),
					"select_advancement_tab" => ((Func<PacketBuffer, PacketSelectAdvancementTab>)((buffer) => Mine.Net.Play.Clientbound.PacketSelectAdvancementTab.Read(buffer )))(buffer),
					"server_data" => ((Func<PacketBuffer, PacketServerData>)((buffer) => Mine.Net.Play.Clientbound.PacketServerData.Read(buffer )))(buffer),
					"action_bar" => ((Func<PacketBuffer, PacketActionBar>)((buffer) => Mine.Net.Play.Clientbound.PacketActionBar.Read(buffer )))(buffer),
					"world_border_center" => ((Func<PacketBuffer, PacketWorldBorderCenter>)((buffer) => Mine.Net.Play.Clientbound.PacketWorldBorderCenter.Read(buffer )))(buffer),
					"world_border_lerp_size" => ((Func<PacketBuffer, PacketWorldBorderLerpSize>)((buffer) => Mine.Net.Play.Clientbound.PacketWorldBorderLerpSize.Read(buffer )))(buffer),
					"world_border_size" => ((Func<PacketBuffer, PacketWorldBorderSize>)((buffer) => Mine.Net.Play.Clientbound.PacketWorldBorderSize.Read(buffer )))(buffer),
					"world_border_warning_delay" => ((Func<PacketBuffer, PacketWorldBorderWarningDelay>)((buffer) => Mine.Net.Play.Clientbound.PacketWorldBorderWarningDelay.Read(buffer )))(buffer),
					"world_border_warning_reach" => ((Func<PacketBuffer, PacketWorldBorderWarningReach>)((buffer) => Mine.Net.Play.Clientbound.PacketWorldBorderWarningReach.Read(buffer )))(buffer),
					"camera" => ((Func<PacketBuffer, PacketCamera>)((buffer) => Mine.Net.Play.Clientbound.PacketCamera.Read(buffer )))(buffer),
					"held_item_slot" => ((Func<PacketBuffer, PacketHeldItemSlot>)((buffer) => Mine.Net.Play.Clientbound.PacketHeldItemSlot.Read(buffer )))(buffer),
					"update_view_position" => ((Func<PacketBuffer, PacketUpdateViewPosition>)((buffer) => Mine.Net.Play.Clientbound.PacketUpdateViewPosition.Read(buffer )))(buffer),
					"update_view_distance" => ((Func<PacketBuffer, PacketUpdateViewDistance>)((buffer) => Mine.Net.Play.Clientbound.PacketUpdateViewDistance.Read(buffer )))(buffer),
					"spawn_position" => ((Func<PacketBuffer, PacketSpawnPosition>)((buffer) => Mine.Net.Play.Clientbound.PacketSpawnPosition.Read(buffer )))(buffer),
					"should_display_chat_preview" => ((Func<PacketBuffer, PacketShouldDisplayChatPreview>)((buffer) => Mine.Net.Play.Clientbound.PacketShouldDisplayChatPreview.Read(buffer )))(buffer),
					"scoreboard_display_objective" => ((Func<PacketBuffer, PacketScoreboardDisplayObjective>)((buffer) => Mine.Net.Play.Clientbound.PacketScoreboardDisplayObjective.Read(buffer )))(buffer),
					"entity_metadata" => ((Func<PacketBuffer, PacketEntityMetadata>)((buffer) => Mine.Net.Play.Clientbound.PacketEntityMetadata.Read(buffer )))(buffer),
					"attach_entity" => ((Func<PacketBuffer, PacketAttachEntity>)((buffer) => Mine.Net.Play.Clientbound.PacketAttachEntity.Read(buffer )))(buffer),
					"entity_velocity" => ((Func<PacketBuffer, PacketEntityVelocity>)((buffer) => Mine.Net.Play.Clientbound.PacketEntityVelocity.Read(buffer )))(buffer),
					"entity_equipment" => ((Func<PacketBuffer, PacketEntityEquipment>)((buffer) => Mine.Net.Play.Clientbound.PacketEntityEquipment.Read(buffer )))(buffer),
					"experience" => ((Func<PacketBuffer, PacketExperience>)((buffer) => Mine.Net.Play.Clientbound.PacketExperience.Read(buffer )))(buffer),
					"update_health" => ((Func<PacketBuffer, PacketUpdateHealth>)((buffer) => Mine.Net.Play.Clientbound.PacketUpdateHealth.Read(buffer )))(buffer),
					"scoreboard_objective" => ((Func<PacketBuffer, PacketScoreboardObjective>)((buffer) => Mine.Net.Play.Clientbound.PacketScoreboardObjective.Read(buffer )))(buffer),
					"set_passengers" => ((Func<PacketBuffer, PacketSetPassengers>)((buffer) => Mine.Net.Play.Clientbound.PacketSetPassengers.Read(buffer )))(buffer),
					"teams" => ((Func<PacketBuffer, PacketTeams>)((buffer) => Mine.Net.Play.Clientbound.PacketTeams.Read(buffer )))(buffer),
					"scoreboard_score" => ((Func<PacketBuffer, PacketScoreboardScore>)((buffer) => Mine.Net.Play.Clientbound.PacketScoreboardScore.Read(buffer )))(buffer),
					"simulation_distance" => ((Func<PacketBuffer, PacketSimulationDistance>)((buffer) => Mine.Net.Play.Clientbound.PacketSimulationDistance.Read(buffer )))(buffer),
					"set_title_subtitle" => ((Func<PacketBuffer, PacketSetTitleSubtitle>)((buffer) => Mine.Net.Play.Clientbound.PacketSetTitleSubtitle.Read(buffer )))(buffer),
					"update_time" => ((Func<PacketBuffer, PacketUpdateTime>)((buffer) => Mine.Net.Play.Clientbound.PacketUpdateTime.Read(buffer )))(buffer),
					"set_title_text" => ((Func<PacketBuffer, PacketSetTitleText>)((buffer) => Mine.Net.Play.Clientbound.PacketSetTitleText.Read(buffer )))(buffer),
					"set_title_time" => ((Func<PacketBuffer, PacketSetTitleTime>)((buffer) => Mine.Net.Play.Clientbound.PacketSetTitleTime.Read(buffer )))(buffer),
					"entity_sound_effect" => ((Func<PacketBuffer, PacketEntitySoundEffect>)((buffer) => Mine.Net.Play.Clientbound.PacketEntitySoundEffect.Read(buffer )))(buffer),
					"sound_effect" => ((Func<PacketBuffer, PacketSoundEffect>)((buffer) => Mine.Net.Play.Clientbound.PacketSoundEffect.Read(buffer )))(buffer),
					"stop_sound" => ((Func<PacketBuffer, PacketStopSound>)((buffer) => Mine.Net.Play.Clientbound.PacketStopSound.Read(buffer )))(buffer),
					"system_chat" => ((Func<PacketBuffer, PacketSystemChat>)((buffer) => Mine.Net.Play.Clientbound.PacketSystemChat.Read(buffer )))(buffer),
					"playerlist_header" => ((Func<PacketBuffer, PacketPlayerlistHeader>)((buffer) => Mine.Net.Play.Clientbound.PacketPlayerlistHeader.Read(buffer )))(buffer),
					"nbt_query_response" => ((Func<PacketBuffer, PacketNbtQueryResponse>)((buffer) => Mine.Net.Play.Clientbound.PacketNbtQueryResponse.Read(buffer )))(buffer),
					"collect" => ((Func<PacketBuffer, PacketCollect>)((buffer) => Mine.Net.Play.Clientbound.PacketCollect.Read(buffer )))(buffer),
					"entity_teleport" => ((Func<PacketBuffer, PacketEntityTeleport>)((buffer) => Mine.Net.Play.Clientbound.PacketEntityTeleport.Read(buffer )))(buffer),
					"advancements" => ((Func<PacketBuffer, PacketAdvancements>)((buffer) => Mine.Net.Play.Clientbound.PacketAdvancements.Read(buffer )))(buffer),
					"entity_update_attributes" => ((Func<PacketBuffer, PacketEntityUpdateAttributes>)((buffer) => Mine.Net.Play.Clientbound.PacketEntityUpdateAttributes.Read(buffer )))(buffer),
					"entity_effect" => ((Func<PacketBuffer, PacketEntityEffect>)((buffer) => Mine.Net.Play.Clientbound.PacketEntityEffect.Read(buffer )))(buffer),
					"declare_recipes" => ((Func<PacketBuffer, PacketDeclareRecipes>)((buffer) => Mine.Net.Play.Clientbound.PacketDeclareRecipes.Read(buffer )))(buffer),
					"tags" => ((Func<PacketBuffer, PacketTags>)((buffer) => Mine.Net.Play.Clientbound.PacketTags.Read(buffer )))(buffer),
					 _ => throw new Exception($"Invalid value: '{state}'")
				};
				return new ParamsSwitch(value);
			}
			public static implicit operator PacketSpawnEntity?(ParamsSwitch value) => (PacketSpawnEntity?)value.Value;
			public static implicit operator PacketSpawnEntityExperienceOrb?(ParamsSwitch value) => (PacketSpawnEntityExperienceOrb?)value.Value;
			public static implicit operator PacketNamedEntitySpawn?(ParamsSwitch value) => (PacketNamedEntitySpawn?)value.Value;
			public static implicit operator PacketAnimation?(ParamsSwitch value) => (PacketAnimation?)value.Value;
			public static implicit operator PacketStatistics?(ParamsSwitch value) => (PacketStatistics?)value.Value;
			public static implicit operator PacketAcknowledgePlayerDigging?(ParamsSwitch value) => (PacketAcknowledgePlayerDigging?)value.Value;
			public static implicit operator PacketBlockBreakAnimation?(ParamsSwitch value) => (PacketBlockBreakAnimation?)value.Value;
			public static implicit operator PacketTileEntityData?(ParamsSwitch value) => (PacketTileEntityData?)value.Value;
			public static implicit operator PacketBlockAction?(ParamsSwitch value) => (PacketBlockAction?)value.Value;
			public static implicit operator PacketBlockChange?(ParamsSwitch value) => (PacketBlockChange?)value.Value;
			public static implicit operator PacketBossBar?(ParamsSwitch value) => (PacketBossBar?)value.Value;
			public static implicit operator PacketDifficulty?(ParamsSwitch value) => (PacketDifficulty?)value.Value;
			public static implicit operator PacketChatPreview?(ParamsSwitch value) => (PacketChatPreview?)value.Value;
			public static implicit operator PacketClearTitles?(ParamsSwitch value) => (PacketClearTitles?)value.Value;
			public static implicit operator PacketTabComplete?(ParamsSwitch value) => (PacketTabComplete?)value.Value;
			public static implicit operator PacketDeclareCommands?(ParamsSwitch value) => (PacketDeclareCommands?)value.Value;
			public static implicit operator PacketCloseWindow?(ParamsSwitch value) => (PacketCloseWindow?)value.Value;
			public static implicit operator PacketWindowItems?(ParamsSwitch value) => (PacketWindowItems?)value.Value;
			public static implicit operator PacketCraftProgressBar?(ParamsSwitch value) => (PacketCraftProgressBar?)value.Value;
			public static implicit operator PacketSetSlot?(ParamsSwitch value) => (PacketSetSlot?)value.Value;
			public static implicit operator PacketSetCooldown?(ParamsSwitch value) => (PacketSetCooldown?)value.Value;
			public static implicit operator PacketChatSuggestions?(ParamsSwitch value) => (PacketChatSuggestions?)value.Value;
			public static implicit operator PacketCustomPayload?(ParamsSwitch value) => (PacketCustomPayload?)value.Value;
			public static implicit operator PacketNamedSoundEffect?(ParamsSwitch value) => (PacketNamedSoundEffect?)value.Value;
			public static implicit operator PacketHideMessage?(ParamsSwitch value) => (PacketHideMessage?)value.Value;
			public static implicit operator PacketKickDisconnect?(ParamsSwitch value) => (PacketKickDisconnect?)value.Value;
			public static implicit operator PacketEntityStatus?(ParamsSwitch value) => (PacketEntityStatus?)value.Value;
			public static implicit operator PacketExplosion?(ParamsSwitch value) => (PacketExplosion?)value.Value;
			public static implicit operator PacketUnloadChunk?(ParamsSwitch value) => (PacketUnloadChunk?)value.Value;
			public static implicit operator PacketGameStateChange?(ParamsSwitch value) => (PacketGameStateChange?)value.Value;
			public static implicit operator PacketOpenHorseWindow?(ParamsSwitch value) => (PacketOpenHorseWindow?)value.Value;
			public static implicit operator PacketInitializeWorldBorder?(ParamsSwitch value) => (PacketInitializeWorldBorder?)value.Value;
			public static implicit operator PacketKeepAlive?(ParamsSwitch value) => (PacketKeepAlive?)value.Value;
			public static implicit operator PacketMapChunk?(ParamsSwitch value) => (PacketMapChunk?)value.Value;
			public static implicit operator PacketWorldEvent?(ParamsSwitch value) => (PacketWorldEvent?)value.Value;
			public static implicit operator PacketWorldParticles?(ParamsSwitch value) => (PacketWorldParticles?)value.Value;
			public static implicit operator PacketUpdateLight?(ParamsSwitch value) => (PacketUpdateLight?)value.Value;
			public static implicit operator PacketLogin?(ParamsSwitch value) => (PacketLogin?)value.Value;
			public static implicit operator PacketMap?(ParamsSwitch value) => (PacketMap?)value.Value;
			public static implicit operator PacketTradeList?(ParamsSwitch value) => (PacketTradeList?)value.Value;
			public static implicit operator PacketRelEntityMove?(ParamsSwitch value) => (PacketRelEntityMove?)value.Value;
			public static implicit operator PacketEntityMoveLook?(ParamsSwitch value) => (PacketEntityMoveLook?)value.Value;
			public static implicit operator PacketEntityLook?(ParamsSwitch value) => (PacketEntityLook?)value.Value;
			public static implicit operator PacketVehicleMove?(ParamsSwitch value) => (PacketVehicleMove?)value.Value;
			public static implicit operator PacketOpenBook?(ParamsSwitch value) => (PacketOpenBook?)value.Value;
			public static implicit operator PacketOpenWindow?(ParamsSwitch value) => (PacketOpenWindow?)value.Value;
			public static implicit operator PacketOpenSignEntity?(ParamsSwitch value) => (PacketOpenSignEntity?)value.Value;
			public static implicit operator PacketPing?(ParamsSwitch value) => (PacketPing?)value.Value;
			public static implicit operator PacketCraftRecipeResponse?(ParamsSwitch value) => (PacketCraftRecipeResponse?)value.Value;
			public static implicit operator PacketAbilities?(ParamsSwitch value) => (PacketAbilities?)value.Value;
			public static implicit operator PacketMessageHeader?(ParamsSwitch value) => (PacketMessageHeader?)value.Value;
			public static implicit operator PacketPlayerChat?(ParamsSwitch value) => (PacketPlayerChat?)value.Value;
			public static implicit operator PacketEndCombatEvent?(ParamsSwitch value) => (PacketEndCombatEvent?)value.Value;
			public static implicit operator PacketEnterCombatEvent?(ParamsSwitch value) => (PacketEnterCombatEvent?)value.Value;
			public static implicit operator PacketDeathCombatEvent?(ParamsSwitch value) => (PacketDeathCombatEvent?)value.Value;
			public static implicit operator PacketPlayerInfo?(ParamsSwitch value) => (PacketPlayerInfo?)value.Value;
			public static implicit operator PacketFacePlayer?(ParamsSwitch value) => (PacketFacePlayer?)value.Value;
			public static implicit operator PacketPosition?(ParamsSwitch value) => (PacketPosition?)value.Value;
			public static implicit operator PacketUnlockRecipes?(ParamsSwitch value) => (PacketUnlockRecipes?)value.Value;
			public static implicit operator PacketEntityDestroy?(ParamsSwitch value) => (PacketEntityDestroy?)value.Value;
			public static implicit operator PacketRemoveEntityEffect?(ParamsSwitch value) => (PacketRemoveEntityEffect?)value.Value;
			public static implicit operator PacketResourcePackSend?(ParamsSwitch value) => (PacketResourcePackSend?)value.Value;
			public static implicit operator PacketRespawn?(ParamsSwitch value) => (PacketRespawn?)value.Value;
			public static implicit operator PacketEntityHeadRotation?(ParamsSwitch value) => (PacketEntityHeadRotation?)value.Value;
			public static implicit operator PacketMultiBlockChange?(ParamsSwitch value) => (PacketMultiBlockChange?)value.Value;
			public static implicit operator PacketSelectAdvancementTab?(ParamsSwitch value) => (PacketSelectAdvancementTab?)value.Value;
			public static implicit operator PacketServerData?(ParamsSwitch value) => (PacketServerData?)value.Value;
			public static implicit operator PacketActionBar?(ParamsSwitch value) => (PacketActionBar?)value.Value;
			public static implicit operator PacketWorldBorderCenter?(ParamsSwitch value) => (PacketWorldBorderCenter?)value.Value;
			public static implicit operator PacketWorldBorderLerpSize?(ParamsSwitch value) => (PacketWorldBorderLerpSize?)value.Value;
			public static implicit operator PacketWorldBorderSize?(ParamsSwitch value) => (PacketWorldBorderSize?)value.Value;
			public static implicit operator PacketWorldBorderWarningDelay?(ParamsSwitch value) => (PacketWorldBorderWarningDelay?)value.Value;
			public static implicit operator PacketWorldBorderWarningReach?(ParamsSwitch value) => (PacketWorldBorderWarningReach?)value.Value;
			public static implicit operator PacketCamera?(ParamsSwitch value) => (PacketCamera?)value.Value;
			public static implicit operator PacketHeldItemSlot?(ParamsSwitch value) => (PacketHeldItemSlot?)value.Value;
			public static implicit operator PacketUpdateViewPosition?(ParamsSwitch value) => (PacketUpdateViewPosition?)value.Value;
			public static implicit operator PacketUpdateViewDistance?(ParamsSwitch value) => (PacketUpdateViewDistance?)value.Value;
			public static implicit operator PacketSpawnPosition?(ParamsSwitch value) => (PacketSpawnPosition?)value.Value;
			public static implicit operator PacketShouldDisplayChatPreview?(ParamsSwitch value) => (PacketShouldDisplayChatPreview?)value.Value;
			public static implicit operator PacketScoreboardDisplayObjective?(ParamsSwitch value) => (PacketScoreboardDisplayObjective?)value.Value;
			public static implicit operator PacketEntityMetadata?(ParamsSwitch value) => (PacketEntityMetadata?)value.Value;
			public static implicit operator PacketAttachEntity?(ParamsSwitch value) => (PacketAttachEntity?)value.Value;
			public static implicit operator PacketEntityVelocity?(ParamsSwitch value) => (PacketEntityVelocity?)value.Value;
			public static implicit operator PacketEntityEquipment?(ParamsSwitch value) => (PacketEntityEquipment?)value.Value;
			public static implicit operator PacketExperience?(ParamsSwitch value) => (PacketExperience?)value.Value;
			public static implicit operator PacketUpdateHealth?(ParamsSwitch value) => (PacketUpdateHealth?)value.Value;
			public static implicit operator PacketScoreboardObjective?(ParamsSwitch value) => (PacketScoreboardObjective?)value.Value;
			public static implicit operator PacketSetPassengers?(ParamsSwitch value) => (PacketSetPassengers?)value.Value;
			public static implicit operator PacketTeams?(ParamsSwitch value) => (PacketTeams?)value.Value;
			public static implicit operator PacketScoreboardScore?(ParamsSwitch value) => (PacketScoreboardScore?)value.Value;
			public static implicit operator PacketSimulationDistance?(ParamsSwitch value) => (PacketSimulationDistance?)value.Value;
			public static implicit operator PacketSetTitleSubtitle?(ParamsSwitch value) => (PacketSetTitleSubtitle?)value.Value;
			public static implicit operator PacketUpdateTime?(ParamsSwitch value) => (PacketUpdateTime?)value.Value;
			public static implicit operator PacketSetTitleText?(ParamsSwitch value) => (PacketSetTitleText?)value.Value;
			public static implicit operator PacketSetTitleTime?(ParamsSwitch value) => (PacketSetTitleTime?)value.Value;
			public static implicit operator PacketEntitySoundEffect?(ParamsSwitch value) => (PacketEntitySoundEffect?)value.Value;
			public static implicit operator PacketSoundEffect?(ParamsSwitch value) => (PacketSoundEffect?)value.Value;
			public static implicit operator PacketStopSound?(ParamsSwitch value) => (PacketStopSound?)value.Value;
			public static implicit operator PacketSystemChat?(ParamsSwitch value) => (PacketSystemChat?)value.Value;
			public static implicit operator PacketPlayerlistHeader?(ParamsSwitch value) => (PacketPlayerlistHeader?)value.Value;
			public static implicit operator PacketNbtQueryResponse?(ParamsSwitch value) => (PacketNbtQueryResponse?)value.Value;
			public static implicit operator PacketCollect?(ParamsSwitch value) => (PacketCollect?)value.Value;
			public static implicit operator PacketEntityTeleport?(ParamsSwitch value) => (PacketEntityTeleport?)value.Value;
			public static implicit operator PacketAdvancements?(ParamsSwitch value) => (PacketAdvancements?)value.Value;
			public static implicit operator PacketEntityUpdateAttributes?(ParamsSwitch value) => (PacketEntityUpdateAttributes?)value.Value;
			public static implicit operator PacketEntityEffect?(ParamsSwitch value) => (PacketEntityEffect?)value.Value;
			public static implicit operator PacketDeclareRecipes?(ParamsSwitch value) => (PacketDeclareRecipes?)value.Value;
			public static implicit operator PacketTags?(ParamsSwitch value) => (PacketTags?)value.Value;
			public static implicit operator ParamsSwitch?(PacketSpawnEntity? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketSpawnEntityExperienceOrb? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketNamedEntitySpawn? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketAnimation? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketStatistics? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketAcknowledgePlayerDigging? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketBlockBreakAnimation? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketTileEntityData? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketBlockAction? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketBlockChange? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketBossBar? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketDifficulty? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketChatPreview? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketClearTitles? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketTabComplete? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketDeclareCommands? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketCloseWindow? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketWindowItems? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketCraftProgressBar? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketSetSlot? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketSetCooldown? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketChatSuggestions? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketCustomPayload? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketNamedSoundEffect? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketHideMessage? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketKickDisconnect? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketEntityStatus? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketExplosion? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketUnloadChunk? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketGameStateChange? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketOpenHorseWindow? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketInitializeWorldBorder? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketKeepAlive? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketMapChunk? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketWorldEvent? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketWorldParticles? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketUpdateLight? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketLogin? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketMap? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketTradeList? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketRelEntityMove? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketEntityMoveLook? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketEntityLook? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketVehicleMove? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketOpenBook? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketOpenWindow? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketOpenSignEntity? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketPing? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketCraftRecipeResponse? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketAbilities? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketMessageHeader? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketPlayerChat? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketEndCombatEvent? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketEnterCombatEvent? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketDeathCombatEvent? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketPlayerInfo? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketFacePlayer? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketPosition? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketUnlockRecipes? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketEntityDestroy? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketRemoveEntityEffect? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketResourcePackSend? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketRespawn? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketEntityHeadRotation? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketMultiBlockChange? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketSelectAdvancementTab? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketServerData? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketActionBar? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketWorldBorderCenter? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketWorldBorderLerpSize? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketWorldBorderSize? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketWorldBorderWarningDelay? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketWorldBorderWarningReach? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketCamera? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketHeldItemSlot? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketUpdateViewPosition? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketUpdateViewDistance? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketSpawnPosition? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketShouldDisplayChatPreview? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketScoreboardDisplayObjective? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketEntityMetadata? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketAttachEntity? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketEntityVelocity? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketEntityEquipment? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketExperience? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketUpdateHealth? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketScoreboardObjective? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketSetPassengers? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketTeams? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketScoreboardScore? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketSimulationDistance? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketSetTitleSubtitle? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketUpdateTime? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketSetTitleText? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketSetTitleTime? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketEntitySoundEffect? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketSoundEffect? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketStopSound? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketSystemChat? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketPlayerlistHeader? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketNbtQueryResponse? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketCollect? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketEntityTeleport? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketAdvancements? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketEntityUpdateAttributes? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketEntityEffect? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketDeclareRecipes? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketTags? value) => new ParamsSwitch(value);
		}
		public string Name { get; set; }
		public ParamsSwitch Params { get; set; }
		public Packet(string @name, ParamsSwitch @params) {
			this.Name = @name;
			this.Params = @params;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, value switch { "spawn_entity" => 0x00, "spawn_entity_experience_orb" => 0x01, "named_entity_spawn" => 0x02, "animation" => 0x03, "statistics" => 0x04, "acknowledge_player_digging" => 0x05, "block_break_animation" => 0x06, "tile_entity_data" => 0x07, "block_action" => 0x08, "block_change" => 0x09, "boss_bar" => 0x0a, "difficulty" => 0x0b, "chat_preview" => 0x0c, "clear_titles" => 0x0d, "tab_complete" => 0x0e, "declare_commands" => 0x0f, "close_window" => 0x10, "window_items" => 0x11, "craft_progress_bar" => 0x12, "set_slot" => 0x13, "set_cooldown" => 0x14, "chat_suggestions" => 0x15, "custom_payload" => 0x16, "named_sound_effect" => 0x17, "hide_message" => 0x18, "kick_disconnect" => 0x19, "entity_status" => 0x1a, "explosion" => 0x1b, "unload_chunk" => 0x1c, "game_state_change" => 0x1d, "open_horse_window" => 0x1e, "initialize_world_border" => 0x1f, "keep_alive" => 0x20, "map_chunk" => 0x21, "world_event" => 0x22, "world_particles" => 0x23, "update_light" => 0x24, "login" => 0x25, "map" => 0x26, "trade_list" => 0x27, "rel_entity_move" => 0x28, "entity_move_look" => 0x29, "entity_look" => 0x2a, "vehicle_move" => 0x2b, "open_book" => 0x2c, "open_window" => 0x2d, "open_sign_entity" => 0x2e, "ping" => 0x2f, "craft_recipe_response" => 0x30, "abilities" => 0x31, "message_header" => 0x32, "player_chat" => 0x33, "end_combat_event" => 0x34, "enter_combat_event" => 0x35, "death_combat_event" => 0x36, "player_info" => 0x37, "face_player" => 0x38, "position" => 0x39, "unlock_recipes" => 0x3a, "entity_destroy" => 0x3b, "remove_entity_effect" => 0x3c, "resource_pack_send" => 0x3d, "respawn" => 0x3e, "entity_head_rotation" => 0x3f, "multi_block_change" => 0x40, "select_advancement_tab" => 0x41, "server_data" => 0x42, "action_bar" => 0x43, "world_border_center" => 0x44, "world_border_lerp_size" => 0x45, "world_border_size" => 0x46, "world_border_warning_delay" => 0x47, "world_border_warning_reach" => 0x48, "camera" => 0x49, "held_item_slot" => 0x4a, "update_view_position" => 0x4b, "update_view_distance" => 0x4c, "spawn_position" => 0x4d, "should_display_chat_preview" => 0x4e, "scoreboard_display_objective" => 0x4f, "entity_metadata" => 0x50, "attach_entity" => 0x51, "entity_velocity" => 0x52, "entity_equipment" => 0x53, "experience" => 0x54, "update_health" => 0x55, "scoreboard_objective" => 0x56, "set_passengers" => 0x57, "teams" => 0x58, "scoreboard_score" => 0x59, "simulation_distance" => 0x5a, "set_title_subtitle" => 0x5b, "update_time" => 0x5c, "set_title_text" => 0x5d, "set_title_time" => 0x5e, "entity_sound_effect" => 0x5f, "sound_effect" => 0x60, "stop_sound" => 0x61, "system_chat" => 0x62, "playerlist_header" => 0x63, "nbt_query_response" => 0x64, "collect" => 0x65, "entity_teleport" => 0x66, "advancements" => 0x67, "entity_update_attributes" => 0x68, "entity_effect" => 0x69, "declare_recipes" => 0x6a, "tags" => 0x6b, _ => throw new Exception($"Value '{value}' not supported.") })))(buffer, this.Name);
			((Action<PacketBuffer, ParamsSwitch>)((buffer, value) => value.Write(buffer, Name)))(buffer, this.Params);
		}
		public static Packet Read(PacketBuffer buffer ) {
			string @name = ((Func<PacketBuffer, string>)((buffer) => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer).Value switch { 0x00 => "spawn_entity", 0x01 => "spawn_entity_experience_orb", 0x02 => "named_entity_spawn", 0x03 => "animation", 0x04 => "statistics", 0x05 => "acknowledge_player_digging", 0x06 => "block_break_animation", 0x07 => "tile_entity_data", 0x08 => "block_action", 0x09 => "block_change", 0x0a => "boss_bar", 0x0b => "difficulty", 0x0c => "chat_preview", 0x0d => "clear_titles", 0x0e => "tab_complete", 0x0f => "declare_commands", 0x10 => "close_window", 0x11 => "window_items", 0x12 => "craft_progress_bar", 0x13 => "set_slot", 0x14 => "set_cooldown", 0x15 => "chat_suggestions", 0x16 => "custom_payload", 0x17 => "named_sound_effect", 0x18 => "hide_message", 0x19 => "kick_disconnect", 0x1a => "entity_status", 0x1b => "explosion", 0x1c => "unload_chunk", 0x1d => "game_state_change", 0x1e => "open_horse_window", 0x1f => "initialize_world_border", 0x20 => "keep_alive", 0x21 => "map_chunk", 0x22 => "world_event", 0x23 => "world_particles", 0x24 => "update_light", 0x25 => "login", 0x26 => "map", 0x27 => "trade_list", 0x28 => "rel_entity_move", 0x29 => "entity_move_look", 0x2a => "entity_look", 0x2b => "vehicle_move", 0x2c => "open_book", 0x2d => "open_window", 0x2e => "open_sign_entity", 0x2f => "ping", 0x30 => "craft_recipe_response", 0x31 => "abilities", 0x32 => "message_header", 0x33 => "player_chat", 0x34 => "end_combat_event", 0x35 => "enter_combat_event", 0x36 => "death_combat_event", 0x37 => "player_info", 0x38 => "face_player", 0x39 => "position", 0x3a => "unlock_recipes", 0x3b => "entity_destroy", 0x3c => "remove_entity_effect", 0x3d => "resource_pack_send", 0x3e => "respawn", 0x3f => "entity_head_rotation", 0x40 => "multi_block_change", 0x41 => "select_advancement_tab", 0x42 => "server_data", 0x43 => "action_bar", 0x44 => "world_border_center", 0x45 => "world_border_lerp_size", 0x46 => "world_border_size", 0x47 => "world_border_warning_delay", 0x48 => "world_border_warning_reach", 0x49 => "camera", 0x4a => "held_item_slot", 0x4b => "update_view_position", 0x4c => "update_view_distance", 0x4d => "spawn_position", 0x4e => "should_display_chat_preview", 0x4f => "scoreboard_display_objective", 0x50 => "entity_metadata", 0x51 => "attach_entity", 0x52 => "entity_velocity", 0x53 => "entity_equipment", 0x54 => "experience", 0x55 => "update_health", 0x56 => "scoreboard_objective", 0x57 => "set_passengers", 0x58 => "teams", 0x59 => "scoreboard_score", 0x5a => "simulation_distance", 0x5b => "set_title_subtitle", 0x5c => "update_time", 0x5d => "set_title_text", 0x5e => "set_title_time", 0x5f => "entity_sound_effect", 0x60 => "sound_effect", 0x61 => "stop_sound", 0x62 => "system_chat", 0x63 => "playerlist_header", 0x64 => "nbt_query_response", 0x65 => "collect", 0x66 => "entity_teleport", 0x67 => "advancements", 0x68 => "entity_update_attributes", 0x69 => "entity_effect", 0x6a => "declare_recipes", 0x6b => "tags", _ => throw new Exception() }))(buffer);
			ParamsSwitch @params = ((Func<PacketBuffer, ParamsSwitch>)((buffer) => ParamsSwitch.Read(buffer, @name)))(buffer);
			return new Packet(@name, @params);
		}
	}
	public class PlayPacketFactory : IPacketFactory {
		public IPacket ReadPacket(PacketBuffer buffer) {
			return Mine.Net.Play.Clientbound.Packet.Read(buffer);
		}
		public void WritePacket(PacketBuffer buffer, IPacketPayload packet) {
			switch (packet) {
				case PacketSpawnEntity p_0x00: new Mine.Net.Play.Clientbound.Packet("spawn_entity", p_0x00!).Write(buffer); break;
				case PacketSpawnEntityExperienceOrb p_0x01: new Mine.Net.Play.Clientbound.Packet("spawn_entity_experience_orb", p_0x01!).Write(buffer); break;
				case PacketNamedEntitySpawn p_0x02: new Mine.Net.Play.Clientbound.Packet("named_entity_spawn", p_0x02!).Write(buffer); break;
				case PacketAnimation p_0x03: new Mine.Net.Play.Clientbound.Packet("animation", p_0x03!).Write(buffer); break;
				case PacketStatistics p_0x04: new Mine.Net.Play.Clientbound.Packet("statistics", p_0x04!).Write(buffer); break;
				case PacketAcknowledgePlayerDigging p_0x05: new Mine.Net.Play.Clientbound.Packet("acknowledge_player_digging", p_0x05!).Write(buffer); break;
				case PacketBlockBreakAnimation p_0x06: new Mine.Net.Play.Clientbound.Packet("block_break_animation", p_0x06!).Write(buffer); break;
				case PacketTileEntityData p_0x07: new Mine.Net.Play.Clientbound.Packet("tile_entity_data", p_0x07!).Write(buffer); break;
				case PacketBlockAction p_0x08: new Mine.Net.Play.Clientbound.Packet("block_action", p_0x08!).Write(buffer); break;
				case PacketBlockChange p_0x09: new Mine.Net.Play.Clientbound.Packet("block_change", p_0x09!).Write(buffer); break;
				case PacketBossBar p_0x0A: new Mine.Net.Play.Clientbound.Packet("boss_bar", p_0x0A!).Write(buffer); break;
				case PacketDifficulty p_0x0B: new Mine.Net.Play.Clientbound.Packet("difficulty", p_0x0B!).Write(buffer); break;
				case PacketChatPreview p_0x0C: new Mine.Net.Play.Clientbound.Packet("chat_preview", p_0x0C!).Write(buffer); break;
				case PacketClearTitles p_0x0D: new Mine.Net.Play.Clientbound.Packet("clear_titles", p_0x0D!).Write(buffer); break;
				case PacketTabComplete p_0x0E: new Mine.Net.Play.Clientbound.Packet("tab_complete", p_0x0E!).Write(buffer); break;
				case PacketDeclareCommands p_0x0F: new Mine.Net.Play.Clientbound.Packet("declare_commands", p_0x0F!).Write(buffer); break;
				case PacketCloseWindow p_0x10: new Mine.Net.Play.Clientbound.Packet("close_window", p_0x10!).Write(buffer); break;
				case PacketWindowItems p_0x11: new Mine.Net.Play.Clientbound.Packet("window_items", p_0x11!).Write(buffer); break;
				case PacketCraftProgressBar p_0x12: new Mine.Net.Play.Clientbound.Packet("craft_progress_bar", p_0x12!).Write(buffer); break;
				case PacketSetSlot p_0x13: new Mine.Net.Play.Clientbound.Packet("set_slot", p_0x13!).Write(buffer); break;
				case PacketSetCooldown p_0x14: new Mine.Net.Play.Clientbound.Packet("set_cooldown", p_0x14!).Write(buffer); break;
				case PacketChatSuggestions p_0x15: new Mine.Net.Play.Clientbound.Packet("chat_suggestions", p_0x15!).Write(buffer); break;
				case PacketCustomPayload p_0x16: new Mine.Net.Play.Clientbound.Packet("custom_payload", p_0x16!).Write(buffer); break;
				case PacketNamedSoundEffect p_0x17: new Mine.Net.Play.Clientbound.Packet("named_sound_effect", p_0x17!).Write(buffer); break;
				case PacketHideMessage p_0x18: new Mine.Net.Play.Clientbound.Packet("hide_message", p_0x18!).Write(buffer); break;
				case PacketKickDisconnect p_0x19: new Mine.Net.Play.Clientbound.Packet("kick_disconnect", p_0x19!).Write(buffer); break;
				case PacketEntityStatus p_0x1A: new Mine.Net.Play.Clientbound.Packet("entity_status", p_0x1A!).Write(buffer); break;
				case PacketExplosion p_0x1B: new Mine.Net.Play.Clientbound.Packet("explosion", p_0x1B!).Write(buffer); break;
				case PacketUnloadChunk p_0x1C: new Mine.Net.Play.Clientbound.Packet("unload_chunk", p_0x1C!).Write(buffer); break;
				case PacketGameStateChange p_0x1D: new Mine.Net.Play.Clientbound.Packet("game_state_change", p_0x1D!).Write(buffer); break;
				case PacketOpenHorseWindow p_0x1E: new Mine.Net.Play.Clientbound.Packet("open_horse_window", p_0x1E!).Write(buffer); break;
				case PacketInitializeWorldBorder p_0x1F: new Mine.Net.Play.Clientbound.Packet("initialize_world_border", p_0x1F!).Write(buffer); break;
				case PacketKeepAlive p_0x20: new Mine.Net.Play.Clientbound.Packet("keep_alive", p_0x20!).Write(buffer); break;
				case PacketMapChunk p_0x21: new Mine.Net.Play.Clientbound.Packet("map_chunk", p_0x21!).Write(buffer); break;
				case PacketWorldEvent p_0x22: new Mine.Net.Play.Clientbound.Packet("world_event", p_0x22!).Write(buffer); break;
				case PacketWorldParticles p_0x23: new Mine.Net.Play.Clientbound.Packet("world_particles", p_0x23!).Write(buffer); break;
				case PacketUpdateLight p_0x24: new Mine.Net.Play.Clientbound.Packet("update_light", p_0x24!).Write(buffer); break;
				case PacketLogin p_0x25: new Mine.Net.Play.Clientbound.Packet("login", p_0x25!).Write(buffer); break;
				case PacketMap p_0x26: new Mine.Net.Play.Clientbound.Packet("map", p_0x26!).Write(buffer); break;
				case PacketTradeList p_0x27: new Mine.Net.Play.Clientbound.Packet("trade_list", p_0x27!).Write(buffer); break;
				case PacketRelEntityMove p_0x28: new Mine.Net.Play.Clientbound.Packet("rel_entity_move", p_0x28!).Write(buffer); break;
				case PacketEntityMoveLook p_0x29: new Mine.Net.Play.Clientbound.Packet("entity_move_look", p_0x29!).Write(buffer); break;
				case PacketEntityLook p_0x2A: new Mine.Net.Play.Clientbound.Packet("entity_look", p_0x2A!).Write(buffer); break;
				case PacketVehicleMove p_0x2B: new Mine.Net.Play.Clientbound.Packet("vehicle_move", p_0x2B!).Write(buffer); break;
				case PacketOpenBook p_0x2C: new Mine.Net.Play.Clientbound.Packet("open_book", p_0x2C!).Write(buffer); break;
				case PacketOpenWindow p_0x2D: new Mine.Net.Play.Clientbound.Packet("open_window", p_0x2D!).Write(buffer); break;
				case PacketOpenSignEntity p_0x2E: new Mine.Net.Play.Clientbound.Packet("open_sign_entity", p_0x2E!).Write(buffer); break;
				case PacketPing p_0x2F: new Mine.Net.Play.Clientbound.Packet("ping", p_0x2F!).Write(buffer); break;
				case PacketCraftRecipeResponse p_0x30: new Mine.Net.Play.Clientbound.Packet("craft_recipe_response", p_0x30!).Write(buffer); break;
				case PacketAbilities p_0x31: new Mine.Net.Play.Clientbound.Packet("abilities", p_0x31!).Write(buffer); break;
				case PacketMessageHeader p_0x32: new Mine.Net.Play.Clientbound.Packet("message_header", p_0x32!).Write(buffer); break;
				case PacketPlayerChat p_0x33: new Mine.Net.Play.Clientbound.Packet("player_chat", p_0x33!).Write(buffer); break;
				case PacketEndCombatEvent p_0x34: new Mine.Net.Play.Clientbound.Packet("end_combat_event", p_0x34!).Write(buffer); break;
				case PacketEnterCombatEvent p_0x35: new Mine.Net.Play.Clientbound.Packet("enter_combat_event", p_0x35!).Write(buffer); break;
				case PacketDeathCombatEvent p_0x36: new Mine.Net.Play.Clientbound.Packet("death_combat_event", p_0x36!).Write(buffer); break;
				case PacketPlayerInfo p_0x37: new Mine.Net.Play.Clientbound.Packet("player_info", p_0x37!).Write(buffer); break;
				case PacketFacePlayer p_0x38: new Mine.Net.Play.Clientbound.Packet("face_player", p_0x38!).Write(buffer); break;
				case PacketPosition p_0x39: new Mine.Net.Play.Clientbound.Packet("position", p_0x39!).Write(buffer); break;
				case PacketUnlockRecipes p_0x3A: new Mine.Net.Play.Clientbound.Packet("unlock_recipes", p_0x3A!).Write(buffer); break;
				case PacketEntityDestroy p_0x3B: new Mine.Net.Play.Clientbound.Packet("entity_destroy", p_0x3B!).Write(buffer); break;
				case PacketRemoveEntityEffect p_0x3C: new Mine.Net.Play.Clientbound.Packet("remove_entity_effect", p_0x3C!).Write(buffer); break;
				case PacketResourcePackSend p_0x3D: new Mine.Net.Play.Clientbound.Packet("resource_pack_send", p_0x3D!).Write(buffer); break;
				case PacketRespawn p_0x3E: new Mine.Net.Play.Clientbound.Packet("respawn", p_0x3E!).Write(buffer); break;
				case PacketEntityHeadRotation p_0x3F: new Mine.Net.Play.Clientbound.Packet("entity_head_rotation", p_0x3F!).Write(buffer); break;
				case PacketMultiBlockChange p_0x40: new Mine.Net.Play.Clientbound.Packet("multi_block_change", p_0x40!).Write(buffer); break;
				case PacketSelectAdvancementTab p_0x41: new Mine.Net.Play.Clientbound.Packet("select_advancement_tab", p_0x41!).Write(buffer); break;
				case PacketServerData p_0x42: new Mine.Net.Play.Clientbound.Packet("server_data", p_0x42!).Write(buffer); break;
				case PacketActionBar p_0x43: new Mine.Net.Play.Clientbound.Packet("action_bar", p_0x43!).Write(buffer); break;
				case PacketWorldBorderCenter p_0x44: new Mine.Net.Play.Clientbound.Packet("world_border_center", p_0x44!).Write(buffer); break;
				case PacketWorldBorderLerpSize p_0x45: new Mine.Net.Play.Clientbound.Packet("world_border_lerp_size", p_0x45!).Write(buffer); break;
				case PacketWorldBorderSize p_0x46: new Mine.Net.Play.Clientbound.Packet("world_border_size", p_0x46!).Write(buffer); break;
				case PacketWorldBorderWarningDelay p_0x47: new Mine.Net.Play.Clientbound.Packet("world_border_warning_delay", p_0x47!).Write(buffer); break;
				case PacketWorldBorderWarningReach p_0x48: new Mine.Net.Play.Clientbound.Packet("world_border_warning_reach", p_0x48!).Write(buffer); break;
				case PacketCamera p_0x49: new Mine.Net.Play.Clientbound.Packet("camera", p_0x49!).Write(buffer); break;
				case PacketHeldItemSlot p_0x4A: new Mine.Net.Play.Clientbound.Packet("held_item_slot", p_0x4A!).Write(buffer); break;
				case PacketUpdateViewPosition p_0x4B: new Mine.Net.Play.Clientbound.Packet("update_view_position", p_0x4B!).Write(buffer); break;
				case PacketUpdateViewDistance p_0x4C: new Mine.Net.Play.Clientbound.Packet("update_view_distance", p_0x4C!).Write(buffer); break;
				case PacketSpawnPosition p_0x4D: new Mine.Net.Play.Clientbound.Packet("spawn_position", p_0x4D!).Write(buffer); break;
				case PacketShouldDisplayChatPreview p_0x4E: new Mine.Net.Play.Clientbound.Packet("should_display_chat_preview", p_0x4E!).Write(buffer); break;
				case PacketScoreboardDisplayObjective p_0x4F: new Mine.Net.Play.Clientbound.Packet("scoreboard_display_objective", p_0x4F!).Write(buffer); break;
				case PacketEntityMetadata p_0x50: new Mine.Net.Play.Clientbound.Packet("entity_metadata", p_0x50!).Write(buffer); break;
				case PacketAttachEntity p_0x51: new Mine.Net.Play.Clientbound.Packet("attach_entity", p_0x51!).Write(buffer); break;
				case PacketEntityVelocity p_0x52: new Mine.Net.Play.Clientbound.Packet("entity_velocity", p_0x52!).Write(buffer); break;
				case PacketEntityEquipment p_0x53: new Mine.Net.Play.Clientbound.Packet("entity_equipment", p_0x53!).Write(buffer); break;
				case PacketExperience p_0x54: new Mine.Net.Play.Clientbound.Packet("experience", p_0x54!).Write(buffer); break;
				case PacketUpdateHealth p_0x55: new Mine.Net.Play.Clientbound.Packet("update_health", p_0x55!).Write(buffer); break;
				case PacketScoreboardObjective p_0x56: new Mine.Net.Play.Clientbound.Packet("scoreboard_objective", p_0x56!).Write(buffer); break;
				case PacketSetPassengers p_0x57: new Mine.Net.Play.Clientbound.Packet("set_passengers", p_0x57!).Write(buffer); break;
				case PacketTeams p_0x58: new Mine.Net.Play.Clientbound.Packet("teams", p_0x58!).Write(buffer); break;
				case PacketScoreboardScore p_0x59: new Mine.Net.Play.Clientbound.Packet("scoreboard_score", p_0x59!).Write(buffer); break;
				case PacketSimulationDistance p_0x5A: new Mine.Net.Play.Clientbound.Packet("simulation_distance", p_0x5A!).Write(buffer); break;
				case PacketSetTitleSubtitle p_0x5B: new Mine.Net.Play.Clientbound.Packet("set_title_subtitle", p_0x5B!).Write(buffer); break;
				case PacketUpdateTime p_0x5C: new Mine.Net.Play.Clientbound.Packet("update_time", p_0x5C!).Write(buffer); break;
				case PacketSetTitleText p_0x5D: new Mine.Net.Play.Clientbound.Packet("set_title_text", p_0x5D!).Write(buffer); break;
				case PacketSetTitleTime p_0x5E: new Mine.Net.Play.Clientbound.Packet("set_title_time", p_0x5E!).Write(buffer); break;
				case PacketEntitySoundEffect p_0x5F: new Mine.Net.Play.Clientbound.Packet("entity_sound_effect", p_0x5F!).Write(buffer); break;
				case PacketSoundEffect p_0x60: new Mine.Net.Play.Clientbound.Packet("sound_effect", p_0x60!).Write(buffer); break;
				case PacketStopSound p_0x61: new Mine.Net.Play.Clientbound.Packet("stop_sound", p_0x61!).Write(buffer); break;
				case PacketSystemChat p_0x62: new Mine.Net.Play.Clientbound.Packet("system_chat", p_0x62!).Write(buffer); break;
				case PacketPlayerlistHeader p_0x63: new Mine.Net.Play.Clientbound.Packet("playerlist_header", p_0x63!).Write(buffer); break;
				case PacketNbtQueryResponse p_0x64: new Mine.Net.Play.Clientbound.Packet("nbt_query_response", p_0x64!).Write(buffer); break;
				case PacketCollect p_0x65: new Mine.Net.Play.Clientbound.Packet("collect", p_0x65!).Write(buffer); break;
				case PacketEntityTeleport p_0x66: new Mine.Net.Play.Clientbound.Packet("entity_teleport", p_0x66!).Write(buffer); break;
				case PacketAdvancements p_0x67: new Mine.Net.Play.Clientbound.Packet("advancements", p_0x67!).Write(buffer); break;
				case PacketEntityUpdateAttributes p_0x68: new Mine.Net.Play.Clientbound.Packet("entity_update_attributes", p_0x68!).Write(buffer); break;
				case PacketEntityEffect p_0x69: new Mine.Net.Play.Clientbound.Packet("entity_effect", p_0x69!).Write(buffer); break;
				case PacketDeclareRecipes p_0x6A: new Mine.Net.Play.Clientbound.Packet("declare_recipes", p_0x6A!).Write(buffer); break;
				case PacketTags p_0x6B: new Mine.Net.Play.Clientbound.Packet("tags", p_0x6B!).Write(buffer); break;
				default: throw new Exception($"Play cannot write packet of type {packet.GetType().FullName}");
			}
		}
	}
}
namespace YAMNL
{
    public partial class PacketBuffer {
		#region Reading


		#endregion
		#region Writing


		#endregion
	}
}
namespace YAMNL.Login
{
}
namespace YAMNL
{
    public partial class PacketBuffer {
		#region Reading


		#endregion
		#region Writing


		#endregion
	}
}
namespace YAMNL.Login.Serverbound
{
    public class PacketLoginStart : IPacketPayload {
		public class SignatureContainer {
			public long Timestamp { get; set; }
			public byte[] PublicKey { get; set; }
			public byte[] Signature { get; set; }
			public SignatureContainer(long @timestamp, byte[] @publicKey, byte[] @signature) {
				this.Timestamp = @timestamp;
				this.PublicKey = @publicKey;
				this.Signature = @signature;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value)))(buffer, this.Timestamp);
				((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteBuffer(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.PublicKey);
				((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteBuffer(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Signature);
			}
			public static SignatureContainer Read(PacketBuffer buffer ) {
				long @timestamp = ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64()))(buffer);
				byte[] @publicKey = ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadBuffer(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer))))(buffer);
				byte[] @signature = ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadBuffer(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer))))(buffer);
				return new SignatureContainer(@timestamp, @publicKey, @signature);
			}
		}
		public string Username { get; set; }
		public SignatureContainer? Signature { get; set; }
		public UUID? PlayerUUID { get; set; }
		public PacketLoginStart(string @username, SignatureContainer? @signature, UUID? @playerUUID) {
			this.Username = @username;
			this.Signature = @signature;
			this.PlayerUUID = @playerUUID;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Username);
			((Action<PacketBuffer, SignatureContainer?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, SignatureContainer>)((buffer, value) => value.Write(buffer ))))))(buffer, this.Signature);
			((Action<PacketBuffer, UUID?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, UUID>)((buffer, value) => buffer.WriteUUID(value))))))(buffer, this.PlayerUUID);
		}
		public static PacketLoginStart Read(PacketBuffer buffer ) {
			string @username = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			SignatureContainer? @signature = ((Func<PacketBuffer, SignatureContainer?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, SignatureContainer>)((buffer) => Mine.Net.Login.Serverbound.PacketLoginStart.SignatureContainer.Read(buffer ))))))(buffer);
			UUID? @playerUUID = ((Func<PacketBuffer, UUID?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, UUID>)((buffer) => buffer.ReadUUID())))))(buffer);
			return new PacketLoginStart(@username, @signature, @playerUUID);
		}
	}
	public class PacketEncryptionBegin : IPacketPayload {
		public class CryptoSwitch {
			public class CryptoSwitchStatetrueContainer {
				public byte[] VerifyToken { get; set; }
				public CryptoSwitchStatetrueContainer(byte[] @verifyToken) {
					this.VerifyToken = @verifyToken;
				}
				public void Write(PacketBuffer buffer ) {
					((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteBuffer(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.VerifyToken);
				}
				public static CryptoSwitchStatetrueContainer Read(PacketBuffer buffer ) {
					byte[] @verifyToken = ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadBuffer(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer))))(buffer);
					return new CryptoSwitchStatetrueContainer(@verifyToken);
				}
			}
			public class CryptoSwitchStatefalseContainer {
				public long Salt { get; set; }
				public byte[] MessageSignature { get; set; }
				public CryptoSwitchStatefalseContainer(long @salt, byte[] @messageSignature) {
					this.Salt = @salt;
					this.MessageSignature = @messageSignature;
				}
				public void Write(PacketBuffer buffer ) {
					((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value)))(buffer, this.Salt);
					((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteBuffer(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.MessageSignature);
				}
				public static CryptoSwitchStatefalseContainer Read(PacketBuffer buffer ) {
					long @salt = ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64()))(buffer);
					byte[] @messageSignature = ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadBuffer(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer))))(buffer);
					return new CryptoSwitchStatefalseContainer(@salt, @messageSignature);
				}
			}
			public object? Value { get; set; }
			public CryptoSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, bool state) {
				switch (state) {
					case true: ((Action<PacketBuffer, CryptoSwitchStatetrueContainer>)((buffer, value) => value.Write(buffer )))(buffer, (CryptoSwitchStatetrueContainer)this); break;
					case false: ((Action<PacketBuffer, CryptoSwitchStatefalseContainer>)((buffer, value) => value.Write(buffer )))(buffer, (CryptoSwitchStatefalseContainer)this); break;
					default: throw new Exception($"Invalid value: '{state}'");
				}
			}
			public static CryptoSwitch Read(PacketBuffer buffer, bool state) {
				object? value = state switch {
					true => ((Func<PacketBuffer, CryptoSwitchStatetrueContainer>)((buffer) => Mine.Net.Login.Serverbound.PacketEncryptionBegin.CryptoSwitch.CryptoSwitchStatetrueContainer.Read(buffer )))(buffer),
					false => ((Func<PacketBuffer, CryptoSwitchStatefalseContainer>)((buffer) => Mine.Net.Login.Serverbound.PacketEncryptionBegin.CryptoSwitch.CryptoSwitchStatefalseContainer.Read(buffer )))(buffer),
				};
				return new CryptoSwitch(value);
			}
			public static implicit operator CryptoSwitchStatetrueContainer?(CryptoSwitch value) => (CryptoSwitchStatetrueContainer?)value.Value;
			public static implicit operator CryptoSwitchStatefalseContainer?(CryptoSwitch value) => (CryptoSwitchStatefalseContainer?)value.Value;
			public static implicit operator CryptoSwitch?(CryptoSwitchStatetrueContainer? value) => new CryptoSwitch(value);
			public static implicit operator CryptoSwitch?(CryptoSwitchStatefalseContainer? value) => new CryptoSwitch(value);
		}
		public byte[] SharedSecret { get; set; }
		public bool HasVerifyToken { get; set; }
		public CryptoSwitch Crypto { get; set; }
		public PacketEncryptionBegin(byte[] @sharedSecret, bool @hasVerifyToken, CryptoSwitch @crypto) {
			this.SharedSecret = @sharedSecret;
			this.HasVerifyToken = @hasVerifyToken;
			this.Crypto = @crypto;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteBuffer(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.SharedSecret);
			((Action<PacketBuffer, bool>)((buffer, value) => buffer.WriteBool(value)))(buffer, this.HasVerifyToken);
			((Action<PacketBuffer, CryptoSwitch>)((buffer, value) => value.Write(buffer, HasVerifyToken)))(buffer, this.Crypto);
		}
		public static PacketEncryptionBegin Read(PacketBuffer buffer ) {
			byte[] @sharedSecret = ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadBuffer(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer))))(buffer);
			bool @hasVerifyToken = ((Func<PacketBuffer, bool>)((buffer) => buffer.ReadBool()))(buffer);
			CryptoSwitch @crypto = ((Func<PacketBuffer, CryptoSwitch>)((buffer) => CryptoSwitch.Read(buffer, @hasVerifyToken)))(buffer);
			return new PacketEncryptionBegin(@sharedSecret, @hasVerifyToken, @crypto);
		}
	}
	public class PacketLoginPluginResponse : IPacketPayload {
		public VarInt MessageId { get; set; }
		public byte[]? Data { get; set; }
		public PacketLoginPluginResponse(VarInt @messageId, byte[]? @data) {
			this.MessageId = @messageId;
			this.Data = @data;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.MessageId);
			((Action<PacketBuffer, byte[]?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteRestBuffer(value))))))(buffer, this.Data);
		}
		public static PacketLoginPluginResponse Read(PacketBuffer buffer ) {
			VarInt @messageId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			byte[]? @data = ((Func<PacketBuffer, byte[]?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadRestBuffer())))))(buffer);
			return new PacketLoginPluginResponse(@messageId, @data);
		}
	}
	public class Packet : IPacket {
		public class ParamsSwitch {
			public object? Value { get; set; }
			public ParamsSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, string state) {
				switch (state) {
					case "login_start": ((Action<PacketBuffer, PacketLoginStart>)((buffer, value) => value.Write(buffer )))(buffer, (PacketLoginStart)this); break;
					case "encryption_begin": ((Action<PacketBuffer, PacketEncryptionBegin>)((buffer, value) => value.Write(buffer )))(buffer, (PacketEncryptionBegin)this); break;
					case "login_plugin_response": ((Action<PacketBuffer, PacketLoginPluginResponse>)((buffer, value) => value.Write(buffer )))(buffer, (PacketLoginPluginResponse)this); break;
					default: throw new Exception($"Invalid value: '{state}'");
				}
			}
			public static ParamsSwitch Read(PacketBuffer buffer, string state) {
				object? value = state switch {
					"login_start" => ((Func<PacketBuffer, PacketLoginStart>)((buffer) => Mine.Net.Login.Serverbound.PacketLoginStart.Read(buffer )))(buffer),
					"encryption_begin" => ((Func<PacketBuffer, PacketEncryptionBegin>)((buffer) => Mine.Net.Login.Serverbound.PacketEncryptionBegin.Read(buffer )))(buffer),
					"login_plugin_response" => ((Func<PacketBuffer, PacketLoginPluginResponse>)((buffer) => Mine.Net.Login.Serverbound.PacketLoginPluginResponse.Read(buffer )))(buffer),
					 _ => throw new Exception($"Invalid value: '{state}'")
				};
				return new ParamsSwitch(value);
			}
			public static implicit operator PacketLoginStart?(ParamsSwitch value) => (PacketLoginStart?)value.Value;
			public static implicit operator PacketEncryptionBegin?(ParamsSwitch value) => (PacketEncryptionBegin?)value.Value;
			public static implicit operator PacketLoginPluginResponse?(ParamsSwitch value) => (PacketLoginPluginResponse?)value.Value;
			public static implicit operator ParamsSwitch?(PacketLoginStart? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketEncryptionBegin? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketLoginPluginResponse? value) => new ParamsSwitch(value);
		}
		public string Name { get; set; }
		public ParamsSwitch Params { get; set; }
		public Packet(string @name, ParamsSwitch @params) {
			this.Name = @name;
			this.Params = @params;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, value switch { "login_start" => 0x00, "encryption_begin" => 0x01, "login_plugin_response" => 0x02, _ => throw new Exception($"Value '{value}' not supported.") })))(buffer, this.Name);
			((Action<PacketBuffer, ParamsSwitch>)((buffer, value) => value.Write(buffer, Name)))(buffer, this.Params);
		}
		public static Packet Read(PacketBuffer buffer ) {
			string @name = ((Func<PacketBuffer, string>)((buffer) => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer).Value switch { 0x00 => "login_start", 0x01 => "encryption_begin", 0x02 => "login_plugin_response", _ => throw new Exception() }))(buffer);
			ParamsSwitch @params = ((Func<PacketBuffer, ParamsSwitch>)((buffer) => ParamsSwitch.Read(buffer, @name)))(buffer);
			return new Packet(@name, @params);
		}
	}
	public class LoginPacketFactory : IPacketFactory {
		public IPacket ReadPacket(PacketBuffer buffer) {
			return Mine.Net.Login.Serverbound.Packet.Read(buffer);
		}
		public void WritePacket(PacketBuffer buffer, IPacketPayload packet) {
			switch (packet) {
				case PacketLoginStart p_0x00: new Mine.Net.Login.Serverbound.Packet("login_start", p_0x00!).Write(buffer); break;
				case PacketEncryptionBegin p_0x01: new Mine.Net.Login.Serverbound.Packet("encryption_begin", p_0x01!).Write(buffer); break;
				case PacketLoginPluginResponse p_0x02: new Mine.Net.Login.Serverbound.Packet("login_plugin_response", p_0x02!).Write(buffer); break;
				default: throw new Exception($"Login cannot write packet of type {packet.GetType().FullName}");
			}
		}
	}
}
namespace YAMNL
{
    public partial class PacketBuffer {
		#region Reading


		#endregion
		#region Writing


		#endregion
	}
}
namespace YAMNL.Login.Clientbound
{
    public class PacketDisconnect : IPacketPayload {
		public string Reason { get; set; }
		public PacketDisconnect(string @reason) {
			this.Reason = @reason;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Reason);
		}
		public static PacketDisconnect Read(PacketBuffer buffer ) {
			string @reason = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			return new PacketDisconnect(@reason);
		}
	}
	public class PacketEncryptionBegin : IPacketPayload {
		public string ServerId { get; set; }
		public byte[] PublicKey { get; set; }
		public byte[] VerifyToken { get; set; }
		public PacketEncryptionBegin(string @serverId, byte[] @publicKey, byte[] @verifyToken) {
			this.ServerId = @serverId;
			this.PublicKey = @publicKey;
			this.VerifyToken = @verifyToken;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.ServerId);
			((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteBuffer(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.PublicKey);
			((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteBuffer(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.VerifyToken);
		}
		public static PacketEncryptionBegin Read(PacketBuffer buffer ) {
			string @serverId = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			byte[] @publicKey = ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadBuffer(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer))))(buffer);
			byte[] @verifyToken = ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadBuffer(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer))))(buffer);
			return new PacketEncryptionBegin(@serverId, @publicKey, @verifyToken);
		}
	}
	public class PacketSuccess : IPacketPayload {
		public class PropertiesElementContainer {
			public string Name { get; set; }
			public string Value { get; set; }
			public string? Signature { get; set; }
			public PropertiesElementContainer(string @name, string @value, string? @signature) {
				this.Name = @name;
				this.Value = @value;
				this.Signature = @signature;
			}
			public void Write(PacketBuffer buffer ) {
				((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Name);
				((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Value);
				((Action<PacketBuffer, string?>)((buffer, value) => buffer.WriteOption(value, ((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))))))))(buffer, this.Signature);
			}
			public static PropertiesElementContainer Read(PacketBuffer buffer ) {
				string @name = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
				string @value = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
				string? @signature = ((Func<PacketBuffer, string?>)((buffer) => buffer.ReadOption(((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))))))))(buffer);
				return new PropertiesElementContainer(@name, @value, @signature);
			}
		}
		public UUID Uuid { get; set; }
		public string Username { get; set; }
		public PropertiesElementContainer[] Properties { get; set; }
		public PacketSuccess(UUID @uuid, string @username, PropertiesElementContainer[] @properties) {
			this.Uuid = @uuid;
			this.Username = @username;
			this.Properties = @properties;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, UUID>)((buffer, value) => buffer.WriteUUID(value)))(buffer, this.Uuid);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Username);
			((Action<PacketBuffer, PropertiesElementContainer[]>)((buffer, value) => buffer.WriteArray(value, ((Action<PacketBuffer, PropertiesElementContainer>)((buffer, value) => value.Write(buffer ))), ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Properties);
		}
		public static PacketSuccess Read(PacketBuffer buffer ) {
			UUID @uuid = ((Func<PacketBuffer, UUID>)((buffer) => buffer.ReadUUID()))(buffer);
			string @username = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			PropertiesElementContainer[] @properties = ((Func<PacketBuffer, PropertiesElementContainer[]>)((buffer) => buffer.ReadArray(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer), ((Func<PacketBuffer, PropertiesElementContainer>)((buffer) => Mine.Net.Login.Clientbound.PacketSuccess.PropertiesElementContainer.Read(buffer ))))))(buffer);
			return new PacketSuccess(@uuid, @username, @properties);
		}
	}
	public class PacketCompress : IPacketPayload {
		public VarInt Threshold { get; set; }
		public PacketCompress(VarInt @threshold) {
			this.Threshold = @threshold;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.Threshold);
		}
		public static PacketCompress Read(PacketBuffer buffer ) {
			VarInt @threshold = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketCompress(@threshold);
		}
	}
	public class PacketLoginPluginRequest : IPacketPayload {
		public VarInt MessageId { get; set; }
		public string Channel { get; set; }
		public byte[] Data { get; set; }
		public PacketLoginPluginRequest(VarInt @messageId, string @channel, byte[] @data) {
			this.MessageId = @messageId;
			this.Channel = @channel;
			this.Data = @data;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.MessageId);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Channel);
			((Action<PacketBuffer, byte[]>)((buffer, value) => buffer.WriteRestBuffer(value)))(buffer, this.Data);
		}
		public static PacketLoginPluginRequest Read(PacketBuffer buffer ) {
			VarInt @messageId = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			string @channel = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			byte[] @data = ((Func<PacketBuffer, byte[]>)((buffer) => buffer.ReadRestBuffer()))(buffer);
			return new PacketLoginPluginRequest(@messageId, @channel, @data);
		}
	}
	public class Packet : IPacket {
		public class ParamsSwitch {
			public object? Value { get; set; }
			public ParamsSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, string state) {
				switch (state) {
					case "disconnect": ((Action<PacketBuffer, PacketDisconnect>)((buffer, value) => value.Write(buffer )))(buffer, (PacketDisconnect)this); break;
					case "encryption_begin": ((Action<PacketBuffer, PacketEncryptionBegin>)((buffer, value) => value.Write(buffer )))(buffer, (PacketEncryptionBegin)this); break;
					case "success": ((Action<PacketBuffer, PacketSuccess>)((buffer, value) => value.Write(buffer )))(buffer, (PacketSuccess)this); break;
					case "compress": ((Action<PacketBuffer, PacketCompress>)((buffer, value) => value.Write(buffer )))(buffer, (PacketCompress)this); break;
					case "login_plugin_request": ((Action<PacketBuffer, PacketLoginPluginRequest>)((buffer, value) => value.Write(buffer )))(buffer, (PacketLoginPluginRequest)this); break;
					default: throw new Exception($"Invalid value: '{state}'");
				}
			}
			public static ParamsSwitch Read(PacketBuffer buffer, string state) {
				object? value = state switch {
					"disconnect" => ((Func<PacketBuffer, PacketDisconnect>)((buffer) => Mine.Net.Login.Clientbound.PacketDisconnect.Read(buffer )))(buffer),
					"encryption_begin" => ((Func<PacketBuffer, PacketEncryptionBegin>)((buffer) => Mine.Net.Login.Clientbound.PacketEncryptionBegin.Read(buffer )))(buffer),
					"success" => ((Func<PacketBuffer, PacketSuccess>)((buffer) => Mine.Net.Login.Clientbound.PacketSuccess.Read(buffer )))(buffer),
					"compress" => ((Func<PacketBuffer, PacketCompress>)((buffer) => Mine.Net.Login.Clientbound.PacketCompress.Read(buffer )))(buffer),
					"login_plugin_request" => ((Func<PacketBuffer, PacketLoginPluginRequest>)((buffer) => Mine.Net.Login.Clientbound.PacketLoginPluginRequest.Read(buffer )))(buffer),
					 _ => throw new Exception($"Invalid value: '{state}'")
				};
				return new ParamsSwitch(value);
			}
			public static implicit operator PacketDisconnect?(ParamsSwitch value) => (PacketDisconnect?)value.Value;
			public static implicit operator PacketEncryptionBegin?(ParamsSwitch value) => (PacketEncryptionBegin?)value.Value;
			public static implicit operator PacketSuccess?(ParamsSwitch value) => (PacketSuccess?)value.Value;
			public static implicit operator PacketCompress?(ParamsSwitch value) => (PacketCompress?)value.Value;
			public static implicit operator PacketLoginPluginRequest?(ParamsSwitch value) => (PacketLoginPluginRequest?)value.Value;
			public static implicit operator ParamsSwitch?(PacketDisconnect? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketEncryptionBegin? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketSuccess? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketCompress? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketLoginPluginRequest? value) => new ParamsSwitch(value);
		}
		public string Name { get; set; }
		public ParamsSwitch Params { get; set; }
		public Packet(string @name, ParamsSwitch @params) {
			this.Name = @name;
			this.Params = @params;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, value switch { "disconnect" => 0x00, "encryption_begin" => 0x01, "success" => 0x02, "compress" => 0x03, "login_plugin_request" => 0x04, _ => throw new Exception($"Value '{value}' not supported.") })))(buffer, this.Name);
			((Action<PacketBuffer, ParamsSwitch>)((buffer, value) => value.Write(buffer, Name)))(buffer, this.Params);
		}
		public static Packet Read(PacketBuffer buffer ) {
			string @name = ((Func<PacketBuffer, string>)((buffer) => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer).Value switch { 0x00 => "disconnect", 0x01 => "encryption_begin", 0x02 => "success", 0x03 => "compress", 0x04 => "login_plugin_request", _ => throw new Exception() }))(buffer);
			ParamsSwitch @params = ((Func<PacketBuffer, ParamsSwitch>)((buffer) => ParamsSwitch.Read(buffer, @name)))(buffer);
			return new Packet(@name, @params);
		}
	}
	public class LoginPacketFactory : IPacketFactory {
		public IPacket ReadPacket(PacketBuffer buffer) {
			return Mine.Net.Login.Clientbound.Packet.Read(buffer);
		}
		public void WritePacket(PacketBuffer buffer, IPacketPayload packet) {
			switch (packet) {
				case PacketDisconnect p_0x00: new Mine.Net.Login.Clientbound.Packet("disconnect", p_0x00!).Write(buffer); break;
				case PacketEncryptionBegin p_0x01: new Mine.Net.Login.Clientbound.Packet("encryption_begin", p_0x01!).Write(buffer); break;
				case PacketSuccess p_0x02: new Mine.Net.Login.Clientbound.Packet("success", p_0x02!).Write(buffer); break;
				case PacketCompress p_0x03: new Mine.Net.Login.Clientbound.Packet("compress", p_0x03!).Write(buffer); break;
				case PacketLoginPluginRequest p_0x04: new Mine.Net.Login.Clientbound.Packet("login_plugin_request", p_0x04!).Write(buffer); break;
				default: throw new Exception($"Login cannot write packet of type {packet.GetType().FullName}");
			}
		}
	}
}
namespace YAMNL
{
    public partial class PacketBuffer {
		#region Reading


		#endregion
		#region Writing


		#endregion
	}
}
namespace YAMNL.Status
{
}
namespace YAMNL
{
    public partial class PacketBuffer {
		#region Reading


		#endregion
		#region Writing


		#endregion
	}
}
namespace YAMNL.Status.Serverbound
{
    public class PacketPingStart : IPacketPayload {
		public PacketPingStart() {
		}
		public void Write(PacketBuffer buffer ) {
		}
		public static PacketPingStart Read(PacketBuffer buffer ) {
			return new PacketPingStart();
		}
	}
	public class PacketPing : IPacketPayload {
		public long Time { get; set; }
		public PacketPing(long @time) {
			this.Time = @time;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value)))(buffer, this.Time);
		}
		public static PacketPing Read(PacketBuffer buffer ) {
			long @time = ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64()))(buffer);
			return new PacketPing(@time);
		}
	}
	public class Packet : IPacket {
		public class ParamsSwitch {
			public object? Value { get; set; }
			public ParamsSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, string state) {
				switch (state) {
					case "ping_start": ((Action<PacketBuffer, PacketPingStart>)((buffer, value) => value.Write(buffer )))(buffer, (PacketPingStart)this); break;
					case "ping": ((Action<PacketBuffer, PacketPing>)((buffer, value) => value.Write(buffer )))(buffer, (PacketPing)this); break;
					default: throw new Exception($"Invalid value: '{state}'");
				}
			}
			public static ParamsSwitch Read(PacketBuffer buffer, string state) {
				object? value = state switch {
					"ping_start" => ((Func<PacketBuffer, PacketPingStart>)((buffer) => Mine.Net.Status.Serverbound.PacketPingStart.Read(buffer )))(buffer),
					"ping" => ((Func<PacketBuffer, PacketPing>)((buffer) => Mine.Net.Status.Serverbound.PacketPing.Read(buffer )))(buffer),
					 _ => throw new Exception($"Invalid value: '{state}'")
				};
				return new ParamsSwitch(value);
			}
			public static implicit operator PacketPingStart?(ParamsSwitch value) => (PacketPingStart?)value.Value;
			public static implicit operator PacketPing?(ParamsSwitch value) => (PacketPing?)value.Value;
			public static implicit operator ParamsSwitch?(PacketPingStart? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketPing? value) => new ParamsSwitch(value);
		}
		public string Name { get; set; }
		public ParamsSwitch Params { get; set; }
		public Packet(string @name, ParamsSwitch @params) {
			this.Name = @name;
			this.Params = @params;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, value switch { "ping_start" => 0x00, "ping" => 0x01, _ => throw new Exception($"Value '{value}' not supported.") })))(buffer, this.Name);
			((Action<PacketBuffer, ParamsSwitch>)((buffer, value) => value.Write(buffer, Name)))(buffer, this.Params);
		}
		public static Packet Read(PacketBuffer buffer ) {
			string @name = ((Func<PacketBuffer, string>)((buffer) => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer).Value switch { 0x00 => "ping_start", 0x01 => "ping", _ => throw new Exception() }))(buffer);
			ParamsSwitch @params = ((Func<PacketBuffer, ParamsSwitch>)((buffer) => ParamsSwitch.Read(buffer, @name)))(buffer);
			return new Packet(@name, @params);
		}
	}
	public class StatusPacketFactory : IPacketFactory {
		public IPacket ReadPacket(PacketBuffer buffer) {
			return Mine.Net.Status.Serverbound.Packet.Read(buffer);
		}
		public void WritePacket(PacketBuffer buffer, IPacketPayload packet) {
			switch (packet) {
				case PacketPingStart p_0x00: new Mine.Net.Status.Serverbound.Packet("ping_start", p_0x00!).Write(buffer); break;
				case PacketPing p_0x01: new Mine.Net.Status.Serverbound.Packet("ping", p_0x01!).Write(buffer); break;
				default: throw new Exception($"Status cannot write packet of type {packet.GetType().FullName}");
			}
		}
	}
}
namespace YAMNL
{
    public partial class PacketBuffer {
		#region Reading


		#endregion
		#region Writing


		#endregion
	}
}
namespace YAMNL.Status.Clientbound
{
    public class PacketServerInfo : IPacketPayload {
		public string Response { get; set; }
		public PacketServerInfo(string @response) {
			this.Response = @response;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.Response);
		}
		public static PacketServerInfo Read(PacketBuffer buffer ) {
			string @response = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			return new PacketServerInfo(@response);
		}
	}
	public class PacketPing : IPacketPayload {
		public long Time { get; set; }
		public PacketPing(long @time) {
			this.Time = @time;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, long>)((buffer, value) => buffer.WriteI64(value)))(buffer, this.Time);
		}
		public static PacketPing Read(PacketBuffer buffer ) {
			long @time = ((Func<PacketBuffer, long>)((buffer) => buffer.ReadI64()))(buffer);
			return new PacketPing(@time);
		}
	}
	public class Packet : IPacket {
		public class ParamsSwitch {
			public object? Value { get; set; }
			public ParamsSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, string state) {
				switch (state) {
					case "server_info": ((Action<PacketBuffer, PacketServerInfo>)((buffer, value) => value.Write(buffer )))(buffer, (PacketServerInfo)this); break;
					case "ping": ((Action<PacketBuffer, PacketPing>)((buffer, value) => value.Write(buffer )))(buffer, (PacketPing)this); break;
					default: throw new Exception($"Invalid value: '{state}'");
				}
			}
			public static ParamsSwitch Read(PacketBuffer buffer, string state) {
				object? value = state switch {
					"server_info" => ((Func<PacketBuffer, PacketServerInfo>)((buffer) => Mine.Net.Status.Clientbound.PacketServerInfo.Read(buffer )))(buffer),
					"ping" => ((Func<PacketBuffer, PacketPing>)((buffer) => Mine.Net.Status.Clientbound.PacketPing.Read(buffer )))(buffer),
					 _ => throw new Exception($"Invalid value: '{state}'")
				};
				return new ParamsSwitch(value);
			}
			public static implicit operator PacketServerInfo?(ParamsSwitch value) => (PacketServerInfo?)value.Value;
			public static implicit operator PacketPing?(ParamsSwitch value) => (PacketPing?)value.Value;
			public static implicit operator ParamsSwitch?(PacketServerInfo? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketPing? value) => new ParamsSwitch(value);
		}
		public string Name { get; set; }
		public ParamsSwitch Params { get; set; }
		public Packet(string @name, ParamsSwitch @params) {
			this.Name = @name;
			this.Params = @params;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, value switch { "server_info" => 0x00, "ping" => 0x01, _ => throw new Exception($"Value '{value}' not supported.") })))(buffer, this.Name);
			((Action<PacketBuffer, ParamsSwitch>)((buffer, value) => value.Write(buffer, Name)))(buffer, this.Params);
		}
		public static Packet Read(PacketBuffer buffer ) {
			string @name = ((Func<PacketBuffer, string>)((buffer) => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer).Value switch { 0x00 => "server_info", 0x01 => "ping", _ => throw new Exception() }))(buffer);
			ParamsSwitch @params = ((Func<PacketBuffer, ParamsSwitch>)((buffer) => ParamsSwitch.Read(buffer, @name)))(buffer);
			return new Packet(@name, @params);
		}
	}
	public class StatusPacketFactory : IPacketFactory {
		public IPacket ReadPacket(PacketBuffer buffer) {
			return Mine.Net.Status.Clientbound.Packet.Read(buffer);
		}
		public void WritePacket(PacketBuffer buffer, IPacketPayload packet) {
			switch (packet) {
				case PacketServerInfo p_0x00: new Mine.Net.Status.Clientbound.Packet("server_info", p_0x00!).Write(buffer); break;
				case PacketPing p_0x01: new Mine.Net.Status.Clientbound.Packet("ping", p_0x01!).Write(buffer); break;
				default: throw new Exception($"Status cannot write packet of type {packet.GetType().FullName}");
			}
		}
	}
}
namespace YAMNL
{
    public partial class PacketBuffer {
		#region Reading


		#endregion
		#region Writing


		#endregion
	}
}
namespace YAMNL.Handshaking
{
}
namespace YAMNL
{
    public partial class PacketBuffer {
		#region Reading


		#endregion
		#region Writing


		#endregion
	}
}
namespace YAMNL.Handshaking.Serverbound
{
    public class PacketSetProtocol : IPacketPayload {
		public VarInt ProtocolVersion { get; set; }
		public string ServerHost { get; set; }
		public ushort ServerPort { get; set; }
		public VarInt NextState { get; set; }
		public PacketSetProtocol(VarInt @protocolVersion, string @serverHost, ushort @serverPort, VarInt @nextState) {
			this.ProtocolVersion = @protocolVersion;
			this.ServerHost = @serverHost;
			this.ServerPort = @serverPort;
			this.NextState = @nextState;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.ProtocolVersion);
			((Action<PacketBuffer, string>)((buffer, value) => buffer.WritePString(value, ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value))))))(buffer, this.ServerHost);
			((Action<PacketBuffer, ushort>)((buffer, value) => buffer.WriteU16(value)))(buffer, this.ServerPort);
			((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, this.NextState);
		}
		public static PacketSetProtocol Read(PacketBuffer buffer ) {
			VarInt @protocolVersion = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			string @serverHost = ((Func<PacketBuffer, string>)((buffer) => buffer.ReadPString(((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt())))))(buffer);
			ushort @serverPort = ((Func<PacketBuffer, ushort>)((buffer) => buffer.ReadU16()))(buffer);
			VarInt @nextState = ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer);
			return new PacketSetProtocol(@protocolVersion, @serverHost, @serverPort, @nextState);
		}
	}
	public class PacketLegacyServerListPing : IPacketPayload {
		public byte Payload { get; set; }
		public PacketLegacyServerListPing(byte @payload) {
			this.Payload = @payload;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, byte>)((buffer, value) => buffer.WriteU8(value)))(buffer, this.Payload);
		}
		public static PacketLegacyServerListPing Read(PacketBuffer buffer ) {
			byte @payload = ((Func<PacketBuffer, byte>)((buffer) => buffer.ReadU8()))(buffer);
			return new PacketLegacyServerListPing(@payload);
		}
	}
	public class Packet : IPacket {
		public class ParamsSwitch {
			public object? Value { get; set; }
			public ParamsSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, string state) {
				switch (state) {
					case "set_protocol": ((Action<PacketBuffer, PacketSetProtocol>)((buffer, value) => value.Write(buffer )))(buffer, (PacketSetProtocol)this); break;
					case "legacy_server_list_ping": ((Action<PacketBuffer, PacketLegacyServerListPing>)((buffer, value) => value.Write(buffer )))(buffer, (PacketLegacyServerListPing)this); break;
					default: throw new Exception($"Invalid value: '{state}'");
				}
			}
			public static ParamsSwitch Read(PacketBuffer buffer, string state) {
				object? value = state switch {
					"set_protocol" => ((Func<PacketBuffer, PacketSetProtocol>)((buffer) => Mine.Net.Handshaking.Serverbound.PacketSetProtocol.Read(buffer )))(buffer),
					"legacy_server_list_ping" => ((Func<PacketBuffer, PacketLegacyServerListPing>)((buffer) => Mine.Net.Handshaking.Serverbound.PacketLegacyServerListPing.Read(buffer )))(buffer),
					 _ => throw new Exception($"Invalid value: '{state}'")
				};
				return new ParamsSwitch(value);
			}
			public static implicit operator PacketSetProtocol?(ParamsSwitch value) => (PacketSetProtocol?)value.Value;
			public static implicit operator PacketLegacyServerListPing?(ParamsSwitch value) => (PacketLegacyServerListPing?)value.Value;
			public static implicit operator ParamsSwitch?(PacketSetProtocol? value) => new ParamsSwitch(value);
			public static implicit operator ParamsSwitch?(PacketLegacyServerListPing? value) => new ParamsSwitch(value);
		}
		public string Name { get; set; }
		public ParamsSwitch Params { get; set; }
		public Packet(string @name, ParamsSwitch @params) {
			this.Name = @name;
			this.Params = @params;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, value switch { "set_protocol" => 0x00, "legacy_server_list_ping" => 0xfe, _ => throw new Exception($"Value '{value}' not supported.") })))(buffer, this.Name);
			((Action<PacketBuffer, ParamsSwitch>)((buffer, value) => value.Write(buffer, Name)))(buffer, this.Params);
		}
		public static Packet Read(PacketBuffer buffer ) {
			string @name = ((Func<PacketBuffer, string>)((buffer) => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer).Value switch { 0x00 => "set_protocol", 0xfe => "legacy_server_list_ping", _ => throw new Exception() }))(buffer);
			ParamsSwitch @params = ((Func<PacketBuffer, ParamsSwitch>)((buffer) => ParamsSwitch.Read(buffer, @name)))(buffer);
			return new Packet(@name, @params);
		}
	}
	public class HandshakingPacketFactory : IPacketFactory {
		public IPacket ReadPacket(PacketBuffer buffer) {
			return Mine.Net.Handshaking.Serverbound.Packet.Read(buffer);
		}
		public void WritePacket(PacketBuffer buffer, IPacketPayload packet) {
			switch (packet) {
				case PacketSetProtocol p_0x00: new Mine.Net.Handshaking.Serverbound.Packet("set_protocol", p_0x00!).Write(buffer); break;
				case PacketLegacyServerListPing p_0x01: new Mine.Net.Handshaking.Serverbound.Packet("legacy_server_list_ping", p_0x01!).Write(buffer); break;
				default: throw new Exception($"Handshaking cannot write packet of type {packet.GetType().FullName}");
			}
		}
	}
}
namespace YAMNL
{
    public partial class PacketBuffer {
		#region Reading


		#endregion
		#region Writing


		#endregion
	}
}
namespace YAMNL.Handshaking.Clientbound
{
    public class Packet : IPacket {
		public class ParamsSwitch {
			public object? Value { get; set; }
			public ParamsSwitch(object? value) {
				this.Value = value;
			}
			public void Write(PacketBuffer buffer, string state) {
				switch (state) {
					default: throw new Exception($"Invalid value: '{state}'");
				}
			}
			public static ParamsSwitch Read(PacketBuffer buffer, string state) {
				object? value = state switch {
					 _ => throw new Exception($"Invalid value: '{state}'")
				};
				return new ParamsSwitch(value);
			}
		}
		public string Name { get; set; }
		public ParamsSwitch Params { get; set; }
		public Packet(string @name, ParamsSwitch @params) {
			this.Name = @name;
			this.Params = @params;
		}
		public void Write(PacketBuffer buffer ) {
			((Action<PacketBuffer, string>)((buffer, value) => ((Action<PacketBuffer, VarInt>)((buffer, value) => buffer.WriteVarInt(value)))(buffer, value switch {  _ => throw new Exception($"Value '{value}' not supported.") })))(buffer, this.Name);
			((Action<PacketBuffer, ParamsSwitch>)((buffer, value) => value.Write(buffer, Name)))(buffer, this.Params);
		}
		public static Packet Read(PacketBuffer buffer ) {
			string @name = ((Func<PacketBuffer, string>)((buffer) => ((Func<PacketBuffer, VarInt>)((buffer) => buffer.ReadVarInt()))(buffer).Value switch {  _ => throw new Exception() }))(buffer);
			ParamsSwitch @params = ((Func<PacketBuffer, ParamsSwitch>)((buffer) => ParamsSwitch.Read(buffer, @name)))(buffer);
			return new Packet(@name, @params);
		}
	}
	public class HandshakingPacketFactory : IPacketFactory {
		public IPacket ReadPacket(PacketBuffer buffer) {
			return Mine.Net.Handshaking.Clientbound.Packet.Read(buffer);
		}
		public void WritePacket(PacketBuffer buffer, IPacketPayload packet) {
			switch (packet) {
				default: throw new Exception($"Handshaking cannot write packet of type {packet.GetType().FullName}");
			}
		}
	}
}
