# DTX to PNG Converter

This is **WIP**!

This tool aims to convert .DTX texture files (extracted from LithTech engine games such as Tron 2.0) into .PNG images, including all mipmap levels if present.

Requirements
    - .NET 8.0 SDK or later
(Install via Homebrew if necessary: ```brew install dotnet-sdk```)
    - SixLabors.ImageSharp library
(Already included in project via NuGet.)

## Building and Publishing (macOS)

To build and generate a standalone executable (without DLLs):

1. Open the terminal and navigate to the project folder.
2. Restore dependencies:

```dotnet restore```

3. Publish the project as a single file executable:

```dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true```

4. The output executable will be located in:

```./bin/Release/net8.0/osx-arm64/publish/dtx2png```

(Note: no .dll extension — it will be a real executable.)

5. (Optional) Give execution permission if needed:

```chmod +x ./bin/Release/net8.0/osx-arm64/publish/dtx2png```

⸻

## Usage

```./dtx2png <input.dtx> <output.png>```

Example:

```./dtx2png jet_texture.dtx jet_texture.png```

- The main image and all mipmap levels will be automatically extracted.
- Mipmap levels will be saved as ```<filename>_mip1.png```, ```<filename>_mip2.png```, etc.

⸻

> - Only 32-bit uncompressed DTX files are currently supported.
> - If your file uses a different compression or format, adjustments may be necessary.
> - This tool was designed to be clean, minimal, and cross-platform (macOS, Linux, Windows).
