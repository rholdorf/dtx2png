using System.Diagnostics;
using System.Text;
using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

public class DTX
{
	public const int DTX_VERSION_LT1 = -2;
	public const int DTX_VERSION_LT15 = -3;
	public const int DTX_VERSION_LT2 = -5;

	public const uint MAX_UINT = 0;

	public const int RESOURCE_TYPE_DTX = 0;
	public const int RESOURCE_TYPE_MODEL = 1;
	public const int RESOURCE_TYPE_SPRITE = 2;

	public const int DTX_FULLBRITE = (1 << 0);
	public const int DTX_PREFER16BIT = (1 << 1);
	public const int DTX_MIPSALLOCED = (1 << 2);
	public const int DTX_SECTIONSFIXED = (1 << 3);
	public const int DTX_NOSYSCACHE = (1 << 6);
	public const int DTX_PREFER4444 = (1 << 7);
	public const int DTX_PREFER5551 = (1 << 8);
	public const int DTX_32BITSYSCOPY = (1 << 9);
	public const int DTX_CUBEMAP = (1 << 10);
	public const int DTX_BUMPMAP = (1 << 11);
	public const int DTX_LUMBUMPMAP = (1 << 12);

	public const int DTX_FLAGSAVEMASK = (DTX_FULLBRITE | DTX_32BITSYSCOPY | DTX_PREFER16BIT | DTX_SECTIONSFIXED |
	                                     DTX_PREFER4444 | DTX_PREFER5551 | DTX_CUBEMAP | DTX_BUMPMAP | DTX_LUMBUMPMAP |
	                                     DTX_NOSYSCACHE);

	public const int DTX_COMMANDSTRING_LENGTH = 128;
	public const int BPP_8P = 0;
	public const int BPP_8 = 1;
	public const int BPP_16 = 2;
	public const int BPP_32 = 3;
	public const int BPP_S3TC_DXT1 = 4;
	public const int BPP_S3TC_DXT3 = 5;
	public const int BPP_S3TC_DXT5 = 6;
	public const int BPP_32P = 7;

	private int resource_type = 0;
	private int version = 0;
	private int width = 0;
	private int height = 0;
	private int mipmap_count = 0;
	private int section_count = 0;
	private uint flags = 0;
	private uint user_flags = 0;
	private int texture_group = 0;
	private int mipmaps_to_use = 0;
	private int bytes_per_pixel = 0;
	private int mipmap_offset = 0;
	private int mipmap_tex_coord_offset = 0;
	private int texture_priority = 0;
	private float detail_texture_scale = 0.0f;
	private int detail_texture_angle = 0;
	private string command_string = "";
	private Image<Rgba32> image;


	public void Read(BinaryReader f)
	{
		resource_type = f.ReadInt32();

		if (resource_type != 0)
			f.BaseStream.Seek(0, SeekOrigin.Begin);

		version = (int)f.ReadUInt32();

		if (DTX_VERSION_LT1 != version && DTX_VERSION_LT15 != version && DTX_VERSION_LT2 != version)
		{
			Debug.WriteLine($"Unsupported file version {version}");
			return;
		}

		width = f.ReadUInt16(); // get_16 - unsigned integer as 16 bits
		height = f.ReadUInt16(); // get_16 - unsigned integer as 16 bits
		mipmap_count = f.ReadUInt16(); // get_16 - unsigned integer as 16 bits
		section_count = f.ReadUInt16(); // get_16 - unsigned integer as 16 bits
		flags = f.ReadUInt32(); // get_32 - unsigned integer as 32 bits
		user_flags = f.ReadUInt32(); // get_32 - unsigned integer as 32 bits
		texture_group = f.ReadByte(); // get_8 - integer as 8 bits
		mipmaps_to_use = f.ReadByte(); // get_8 - integer as 8 bits
		bytes_per_pixel = f.ReadByte(); // get_8 - integer as 8 bits
		mipmap_offset = f.ReadByte(); // get_8 - integer as 8 bits
		mipmap_tex_coord_offset = f.ReadByte(); // get_8 - integer as 8 bits
		texture_priority = f.ReadByte(); // get_8 - integer as 8 bits
		detail_texture_scale = f.ReadSingle(); // get_float - float as 32 bits
		detail_texture_angle = f.ReadUInt16(); // get_16 - unsigned integer as 16 bits

		if (DTX_VERSION_LT15 == version || DTX_VERSION_LT2 == version)
		{
			command_string = Encoding.ASCII.GetString(f.ReadBytes(DTX_COMMANDSTRING_LENGTH)).TrimEnd('\0');
		}
		
		ReadTextureData(f);
	}
	
	/*
    func read_texture_data(f: File):
	   		var image: Image = null
	   
	   		if [DTX_VERSION_LT1, DTX_VERSION_LT15].has(this.version) or this.bytes_per_pixel == BPP_8P:
	   			image = this.read_8bit_palette(f)
	   		elif [BPP_S3TC_DXT1, BPP_S3TC_DXT3, BPP_S3TC_DXT5].has(this.bytes_per_pixel):
	   			image = this.read_compressed(f)
	   		elif this.bytes_per_pixel == BPP_32:
	   			image = this.read_32bit_texture(f)
	   		elif this.bytes_per_pixel == BPP_32P:
	   			image = this.read_32bit_palette(f)
	   
	   		return image	 
	 */
	private void ReadTextureData(BinaryReader f)
	{
		if (DTX_VERSION_LT1 == version || DTX_VERSION_LT15 == version || bytes_per_pixel == BPP_8P)
		{
			Read8BitPalette(f);
		}
		else if (BPP_S3TC_DXT1 == bytes_per_pixel || 
		         BPP_S3TC_DXT3 == bytes_per_pixel || 
		         BPP_S3TC_DXT5 == bytes_per_pixel)
		{
			//for (var i = 0; i < mipmap_count; i++)
			{
				//width = Util.DivideByPowerOfTwo(width, i);
				//height = Util.DivideByPowerOfTwo(height, i);
				ReadCompressed(f);
			}
		}
		else if (BPP_32 == bytes_per_pixel)
		{
			//Read32BitTexture(f);
		}
		else if (BPP_32P == bytes_per_pixel)
		{
			//Read32BitPalette(f);
		}
	}
	
	/*
	func read_compressed(f: File):
	   	var image = Image.new()
	   	var format = Image.FORMAT_DXT1
	   	var scale = 8

	   	if this.bytes_per_pixel == BPP_S3TC_DXT3:
	   		format = Image.FORMAT_DXT3
	   		scale = 16
	   	elif this.bytes_per_pixel == BPP_S3TC_DXT5:
	   		format = Image.FORMAT_DXT5
	   		scale = 16

	   	var compressed_width = int((this.width + 3) / 4)
	   	var compressed_height = int((this.height + 3) / 4)
	   	var data = f.get_buffer(compressed_width * compressed_height * scale)
	   	image.create_from_data(this.width, this.height, false, format, data)
	   	return image	 
	 */
	private void ReadCompressed(BinaryReader f)
	{
		var decoder = new BcDecoder();
		var format = CompressionFormat.Bc1; // DXT1
		var scale = 8;
		if (BPP_S3TC_DXT3 == bytes_per_pixel)
		{
			format = CompressionFormat.Bc2; // DXT3
			scale = 16;
		}
		else if (BPP_S3TC_DXT5 == bytes_per_pixel)
		{
			format = CompressionFormat.Bc3; // DXT5
			scale = 16;
		}
		
		var compressedWidth = (width + 3) / 4;
		var compressedHeight = (height + 3) / 4;
		var data = new byte[compressedWidth * compressedHeight * scale];
		var readCount = f.Read(data, 0, data.Length);
		if (data.Length != readCount)
		{
			Debug.WriteLine($"Failed to read {data.Length} bytes: {readCount} bytes read");
			return;
		}
		
		var blockSize = decoder.GetBlockSize(format);
		var blockCount = data.Length / blockSize;
		image = new Image<Rgba32>(width, height);

		var curX = 0;
		var curY = 0;
			
		for (var i = 0; i < blockCount; i++)
		{
			var block = new byte[blockSize];
			Array.Copy(data, i * blockSize, block, 0, blockSize);
			var decodedBlock = decoder.DecodeBlock(block, format);
			
			var decodedBlockSpan = decodedBlock.Span;
			for (var y = 0; y < decodedBlock.Height; y++)
			{
				for (var x = 0; x < decodedBlock.Width; x++)
				{
					var pixel = decodedBlockSpan[y,x];
					image[curX + x, curY + y] = new Rgba32(pixel.r, pixel.g, pixel.b, pixel.a);
				}
			}

			curX += decodedBlock.Width;
			if (curX < width) continue;
			curX = 0;
			curY += decodedBlock.Height;
		}

		image.Save("/Users/rui/src/tron/scratch/CELLPHONE_x.png", new PngEncoder());
	}
	
	/*
	func read_8bit_palette(f: File):
	   	var image = Image.new()
	   	var palette = []
	   	var _palette_header_1 = f.ReadUInt32()
	   	var _palette_header_2 = f.ReadUInt32()

	   	for _i in range(256):
	   		var a = f.ReadByte()
	   		var r = f.ReadByte()
	   		var g = f.ReadByte()
	   		var b = f.ReadByte()
	   		palette.append(Quat(r, g, b, a))

	   	var data = f.get_buffer(this.width * this.height * 1)
	   	var colour_data = PoolByteArray()
	   	var i = 0

	   	while (i < data.size()):
	   		colour_data.append(palette[data[i]].x)
	   		colour_data.append(palette[data[i]].y)
	   		colour_data.append(palette[data[i]].z)
	   		colour_data.append(palette[data[i]].w)
	   		i += 1

	   	image.create_from_data(this.width, this.height, false, Image.FORMAT_RGBA8, colour_data)
	   	return image
	 */
	private void Read8BitPalette(BinaryReader f)
	{
		var image = new Image<Rgba32>(width, height);
		var palette = new List<Quat>();

		var paletteHeader1 = f.ReadUInt32();
		var paletteHeader2 = f.ReadUInt32();

		for (var i = 0; i < 256; i++)
		{
			var a = f.ReadByte();
			var r = f.ReadByte();
			var g = f.ReadByte();
			var b = f.ReadByte();
			palette.Add(new Quat(r, g, b, a));
		}

		var bufferSize = width * height;
		var buffer = new byte[bufferSize];
		var readCount = f.Read(buffer, 0, buffer.Length);
		if (bufferSize != readCount)
		{
			Debug.WriteLine($"Failed to read {bufferSize} bytes: {readCount} bytes read");
			return;
		}

		for (var i = 0; i < buffer.Length; i++)
		{
			image[i % width, i / width] = new Rgba32(palette[buffer[i]].X, palette[buffer[i]].Y, palette[buffer[i]].Z,
				palette[buffer[i]].W);
		}
	}
}

/*

	func read_32bit_texture(f: File):
		var image = Image.new()
		var data = f.get_buffer(this.width * this.height * 4)
		image.create_from_data(this.width, this.height, false, Image.FORMAT_RGBA8, data)
		return image

	func read_32bit_palette(f: File):
		var image = Image.new()
		var palette = []
		var data = f.get_buffer(this.width * this.height * 1)
		var colour_data = PoolByteArray()
		var width = this.width
		var height = this.height
		for _i in range(this.mipmap_count - 1):
			width /= 2
			height /= 2
			var _unused = f.get_buffer(width * height * 1)

		if this.section_count != 1:
			print("Section count is not 1, even though we're a 32bit palette texture! Count: ", this.section_count)
			return null

		var _section_type = f.get_buffer(16)
		var _section_unk = f.get_buffer(12)
		var _section_length = f.ReadUInt32()

		for _i in range(256):
			var packed_data = f.ReadUInt32()
			var unpacked_data = this.convert_32_to_8_bit(packed_data)
			var a = 255 - unpacked_data.w
			var r = unpacked_data.x
			var g = unpacked_data.y
			var b = unpacked_data.z
			palette.append(Quat(r, g, b, a))

		var i = 0

		while (i < data.size()):
			colour_data.append(palette[data[i]].x)
			colour_data.append(palette[data[i]].y)
			colour_data.append(palette[data[i]].z)
			colour_data.append(palette[data[i]].w)
			i += 1

		image.create_from_data(this.width, this.height, false, Image.FORMAT_RGBA8, colour_data)
		return image

	func _make_response(code, message = ""):
		return {"code": code, "message": message}

	func convert_32_to_8_bit(value):
		var a = (value & -16777216) >> 24
		var r = (value & 16711680) >> 16
		var g = (value & 65280) >> 8
		var b = (value & 255)
		return Quat(r, g, b, a)

	func read_string(file: File, is_length_a_short = true):
		var length = 0
		if is_length_a_short:
			length = file.ReadUInt16()
		else:
			length = file.ReadUInt32()

		return file.get_buffer(length).get_string_from_ascii()

	func read_vector2(file: File):
		var vec2 = Vector2()
		vec2.x = file.get_float()
		vec2.y = file.get_float()
		return vec2

	func read_vector3(file: File):
		var vec3 = Vector3()
		vec3.x = file.get_float()
		vec3.y = file.get_float()
		vec3.z = file.get_float()
		return vec3

	func read_quat(file: File):
		var quat = Quat()
		quat.w = file.get_float()
		quat.x = file.get_float()
		quat.y = file.get_float()
		quat.z = file.get_float()
		return quat

	func read_matrix(file: File):
		var matrix_4x4 = []
		for _i in range(16):
			matrix_4x4.append(file.get_float())

		return this.convert_4x4_to_transform(matrix_4x4)

	func convert_4x4_to_transform(matrix):
		return Transform(
			Vector3(matrix[0], matrix[4], matrix[8]),
			Vector3(matrix[1], matrix[5], matrix[9]),
			Vector3(matrix[2], matrix[6], matrix[10]),
			Vector3(matrix[3], matrix[7], matrix[11])
		)
*/

public struct Vector2
{
    public float X, Y;
    public Vector2(float x, float y) { X = x; Y = y; }
}

public struct Vector3
{
    public float X, Y, Z;
    public Vector3(float x, float y, float z) { X = x; Y = y; Z = z; }
}

public struct Quat
{
    public float X, Y, Z, W;
    public Quat(float x, float y, float z, float w) { X = x; Y = y; Z = z; W = w; }
}

// public class Image
// {
//     public int Width, Height;
//     public byte[] Data;
//     public string Format;
//
//     public void CreateFromData(int width, int height, string format, byte[] data)
//     {
//         Width = width;
//         Height = height;
//         Format = format;
//         Data = data;
//     }
// }

public struct Transform
{
    public Vector3 XAxis, YAxis, ZAxis, Origin;
    public Transform(Vector3 x, Vector3 y, Vector3 z, Vector3 origin)
    {
        XAxis = x; YAxis = y; ZAxis = z; Origin = origin;
    }
}
