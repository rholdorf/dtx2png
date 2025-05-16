using System.Text;
using BCnEncoder.Decoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace dtx2png;

public class Dtx : IDisposable
{
	private const int DTX_VERSION_LT1 = -2;
	private const int DTX_VERSION_LT15 = -3;
	private const int DTX_VERSION_LT2 = -5;

	private const int DTX_COMMANDSTRING_LENGTH = 128;
	private const int BPP_8_P = 0;
	private const int BPP_32 = 3;
	private const int BPP_S3_TC_DXT1 = 4;
	private const int BPP_S3_TC_DXT3 = 5;
	private const int BPP_S3_TC_DXT5 = 6;
	private const int BPP_32_P = 7;

	private int _resourceType;
	private int _version;
	private int _width;
	private int _height;
	private int _mipmapCount;
	private uint _flags;
	private int _textureGroup;
	private int _bytesPerPixel;

	private TextureFormat _format = TextureFormat.Unknown;

	public List<Image<Rgba32>> Images { get; } = [];
	
	public string Error { get; private set; } = string.Empty;

	public bool Read(BinaryReader reader)
	{
		_resourceType = reader.ReadInt32();

		if (_resourceType != 0)
			reader.BaseStream.Seek(0, SeekOrigin.Begin);

		_version = (int)reader.ReadUInt32();

		if (DTX_VERSION_LT1 != _version && DTX_VERSION_LT15 != _version && DTX_VERSION_LT2 != _version)
		{
			Error = $"ERR: {((FileStream)reader.BaseStream).Name} - Unsupported file version {_version}";
			return false;
		}

		_width = reader.ReadUInt16(); 
		_height = reader.ReadUInt16(); 
		_mipmapCount = reader.ReadUInt16(); 
		_ = reader.ReadUInt16(); // possible SectionCount 
		_flags = reader.ReadUInt32(); 
		_ = reader.ReadUInt32(); // possible UserFlags 
		_textureGroup = reader.ReadByte(); 
		_ = reader.ReadByte(); // possible MipmapsToUse 
		_bytesPerPixel = reader.ReadByte(); 
		_ = reader.ReadByte(); // possible MipmapOffset 
		_ = reader.ReadByte(); // possible MipmapTextureCoordOffset
		_ = reader.ReadByte(); // possible TexturePriority
		_ = reader.ReadSingle(); // possible DetailTextureScale
		_ = reader.ReadUInt16(); // possible DetailTextureAngle

		if (DTX_VERSION_LT15 == _version || DTX_VERSION_LT2 == _version)
		{
			_ = reader.ReadBytes(DTX_COMMANDSTRING_LENGTH); // possible CommandString, in ASCII
		}

		_format = _flags switch
		{
			136 => TextureFormat.RGBA,
			8 => _textureGroup switch
			{
				0 => TextureFormat.BGRA,
				1 => TextureFormat.DXT1,
				_ => _format
			},
			_ => _format
		};
		
		return ReadTextureData(reader);
	}
	
	private bool ReadTextureData(BinaryReader reader)
	{
		if (DTX_VERSION_LT1 == _version || DTX_VERSION_LT15 == _version || _bytesPerPixel == BPP_8_P)
		{
			var image = Read8BitPalette(reader);
			Images.Add(image);
			return true;
		}
		
		if (BPP_S3_TC_DXT1 == _bytesPerPixel || 
		         BPP_S3_TC_DXT3 == _bytesPerPixel || 
		         BPP_S3_TC_DXT5 == _bytesPerPixel)
		{
			var originalWidth = _width;
			var originalHeight = _height;
			while (reader.BaseStream.Position < reader.BaseStream.Length)
			{
				if (Images.Count == 1)
				{
					reader.BaseStream.Seek(32, SeekOrigin.Current); // TODO: what are these bytes after the "first" image?
				}
				
				for (var i = 0; i < _mipmapCount; i++)
				{
					_width = Util.DivideByPowerOfTwo(originalWidth, i);
					_height = Util.DivideByPowerOfTwo(originalHeight, i);
					if (i == 0)
					{
						Images.Add(ReadCompressed(reader)); // keep only the first
					}
					else
					{
						using var mipmap = ReadCompressed(reader); // read the data, but ignore it for now
					}
				}
				
				_width = originalWidth;
				_height = originalHeight;
			}

			return true;
		} 
		
		if (BPP_32 == _bytesPerPixel)
		{
			var image = Read32BitTexture(reader);
			if(image == null)
				return false;
			
			Images.Add(image);
			return true;
		}
		
		if (BPP_32_P == _bytesPerPixel)
		{
			Error = $"{((FileStream)reader.BaseStream).Name} - 32bit Palette not implemented";
			return false;
			//Read32BitPalette(f);
		}

		Error = $"WAR: {((FileStream)reader.BaseStream).Name} - Unknown format";
		return false;
	}
	
	private Image<Rgba32> ReadCompressed(BinaryReader reader)
	{
		var decoder = new BcDecoder();
		var compressionFormat = CompressionFormat.Bc1; // DXT1
		var scale = 8;
		switch (_bytesPerPixel)
		{
			case BPP_S3_TC_DXT3:
				compressionFormat = CompressionFormat.Bc2; // DXT3
				scale = 16;
				break;
			case BPP_S3_TC_DXT5:
				compressionFormat = CompressionFormat.Bc3; // DXT5
				scale = 16;
				break;
		}
		
		var compressedWidth = (_width + 3) / 4;
		var compressedHeight = (_height + 3) / 4;
		var length = compressedWidth * compressedHeight * scale;
		var data = new byte[length];
		var readCount = reader.Read(data, 0, length);
		if (length != readCount)
		{
			Console.WriteLine($"WAR: {((FileStream)reader.BaseStream).Name} - Failed to read all {data.Length} bytes (only {readCount} bytes read) for Compressed {compressionFormat} data");
		}
		
		using var ms = new MemoryStream(data);
		return decoder.DecodeRawToImageRgba32(ms, _width, _height, compressionFormat);
	}

	private Image<Rgba32> Read8BitPalette(BinaryReader reader)
	{
		var ret = new Image<Rgba32>(_width, _height);
		var palette = new List<Quat>();

		_ = reader.ReadUInt32();
		_ = reader.ReadUInt32();

		for (var i = 0; i < 256; i++)
		{
			var a = reader.ReadByte();
			var r = reader.ReadByte();
			var g = reader.ReadByte();
			var b = reader.ReadByte();
			palette.Add(new Quat(r, g, b, a));
		}

		var bufferSize = _width * _height;
		var buffer = new byte[bufferSize];
		var readCount = reader.Read(buffer, 0, buffer.Length);
		if (bufferSize != readCount)
		{
			Console.WriteLine($"WAR: {((FileStream)reader.BaseStream).Name}Failed to read all {bufferSize} bytes (only {readCount} bytes read) for 8bit palette data");
		}
		
		for (var i = 0; i < readCount; i++)
		{
			ret[i % _width, i / _width] = new Rgba32(
				palette[buffer[i]].X, 
				palette[buffer[i]].Y,
				palette[buffer[i]].Z,
				palette[buffer[i]].W);
		}

		return ret;
	}

	private Image<Rgba32>? Read32BitTexture(BinaryReader reader)
	{
		var ret = new Image<Rgba32>(_width, _height);
		var size = _width * _height * 4;
		var data = new byte[size];
		var readCount = reader.Read(data, 0, data.Length);
		if (size != readCount)
		{
			Error = $"{((FileStream)reader.BaseStream).Name}Inconsistent read. Should be {size}, but {readCount} bytes where read.";
			return null;
		}

		var i = 0;
		byte r, g, b, a;
		for (var y = 0; y < _height; y++)
		for (var x = 0; x < _width; x++)
		{
			if (_format == TextureFormat.BGRA)
			{
				b = data[i++];
				g = data[i++];
				r = data[i++];
				a = data[i++];
			}
			else
			{
				r = data[i++];
				g = data[i++];
				b = data[i++];
				// alpha seems to be incorrect, skip it
				i++;
				a = 0xFF;
			}

			ret[x, y] = new Rgba32(r, g, b, a);
		}

		return ret;
	}

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposing) 
			return;
		
		if (Images.Count <= 0) 
			return;
		
		foreach (var image in Images)
			image.Dispose();

		Images.Clear();
	}
}

public struct Quat
{
	public float X, Y, Z, W;
	public Quat(float x, float y, float z, float w) { X = x; Y = y; Z = z; W = w; }
}
