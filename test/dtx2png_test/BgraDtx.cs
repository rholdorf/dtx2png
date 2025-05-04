using SixLabors.ImageSharp.Formats.Png;

namespace dtx2png_test;
using dtx2png;

public class BgraDtx
{
    private BinaryReader GetReader()
    {
        var data = MockDtxFiles.GetBgraDtxData();
        return new BinaryReader(new MemoryStream(data));
    }
    
    [Fact]
    public void DtxIsSupported()
    {
        // Arrange
        using var reader = GetReader();
        
        // Act
        var dtxFile = new DtxFile(reader);
        
        // Assert
        Assert.True(dtxFile.Header.Supported);
    }

    [Fact]
    public void DtxHasOneTexture()
    {
        // Arrange
        using var reader = GetReader();
        
        // act
        var dtxFile = new DtxFile(reader);
        
        // assert
        Assert.Single(dtxFile.Textures);
    }

    [Fact]
    public void DtxDimensions()
    {
        // Arrange
        using var reader = GetReader();
        
        // act
        var dtxFile = new DtxFile(reader);
        
        // assert
        Assert.Equal(512, dtxFile.Header.Width);        
        Assert.Equal(128, dtxFile.Header.Height);
    }
    
    [Fact]
    public void DtxIsBgra()
    {
        // Arrange
        using var reader = GetReader();
        
        // act
        var dtxFile = new DtxFile(reader);
        
        // assert
        Assert.Equal(TextureFormat.BGRA, dtxFile.Textures[0].Format);        
    }    

    [Fact]
    public void DtxConvertsToPng()
    {
        // Arrange
        using var reader = GetReader();

        // act
        var dtxFile = new DtxFile(reader);
        var current = dtxFile.Textures[0];
        using var img = Util.FromTexture(current);
        using var outStream = new MemoryStream();
        img.Save(outStream, new PngEncoder());

        // assert
        Assert.Equal(MockDtxFiles.GetBgraPngData(), outStream.ToArray());
    }
}