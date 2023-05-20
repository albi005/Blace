using Blace.Shared;
using Blace.Shared.Models;
using SkiaSharp;

namespace Blace.Client.Services;

public class PlaceService : HubClient
{
    private readonly IServer _server;

    private uint[] _bitmapData = null!;

    public PlaceService(IServer server)
    {
        _server = server;
    }

    public Place Place { get; private set; } = null!;
    public SKBitmap Bitmap { get; private set; } = null!;

    public float CurrentScale { get; set; }
    public float ScaleMultiplier { get; set; } = .9f;
    public int X => (int)Xf;
    public int Y => (int)Yf;
    public float Xf { get; set; }
    public float Yf { get; set; }
    public List<uint> Colors { get; } = new()
    {
        0xffffffff, 0xffd4d7d9, 0xff898d90, 0xff000000,
        0xffCE2939, 0xffffa800, 0xffffff00, 0xff477050,
        0xff7eed56, 0xff2450a4, 0xff3690ea, 0xff51e9f4,
        0xff811e9f, 0xffb44ac0, 0xfffe70bd, 0xff9c6926,
    };

    public byte GetColor(int x, int y) => (byte)Colors.IndexOf(GetData(x, y));
    public uint GetData(int x, int y) => _bitmapData[x + Place.Width * y];
    
    public void SetPixel(int x, int y, byte color) => _bitmapData[x + y * Place.Width] = Colors[color];

    public async Task Initialize()
    {
        await UpdatePlace(await _server.GetPlace());
    }

    public override Task UpdatePlace(Place place)
    {
        if (Place?.Id != place.Id)
        {
            Xf = place.Width / 2f;
            Yf = place.Height / 2f;
        }
        Place = place;
        UpdateBitmap();
        return Task.CompletedTask;
    }

    public override Task UpdatePixels(Pixel[] pixels)
    {
        foreach (Pixel pixel in pixels)
        {
            _bitmapData[pixel.X + pixel.Y * Place.Width] = Colors[pixel.Color];
        }

        return Task.CompletedTask;
    }

    private void UpdateBitmap()
    {
        _bitmapData = new uint[Place.Height * Place.Width];
        for (int y = 0; y < Place.Height; y++)
        {
            for (int x = 0; x < Place.Width; x++)
            {
                uint color = Colors[Place.Canvas![(x + Place.Width * y) / 2].GetNibble(x)];
                _bitmapData[x + Place.Width * y] = color;
            }
        }

        SKBitmap bitmap = new(Place.Width, Place.Height, SKColorType.Bgra8888, SKAlphaType.Opaque);

        unsafe
        {
            fixed (uint* pointer = _bitmapData)
            {
                bitmap.SetPixels((IntPtr)pointer);
            }
        }

        Bitmap = bitmap;
    }
}