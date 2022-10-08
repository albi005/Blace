using System.Text.Json;
using Blace.Shared;
using Blace.Shared.Models;
using OpenCvSharp;

namespace Blace.Console;

public static class Timelapse
{
    public static async Task Execute()
    {
        Rgb[] colors =
        {
            0xffffffff, 0xffd4d7d9, 0xff898d90, 0xff000000,
            0xffCE2939, 0xffffa800, 0xffffff00, 0xff477050,
            0xff7eed56, 0xff2450a4, 0xff3690ea, 0xff51e9f4,
            0xff811e9f, 0xffb44ac0, 0xfffe70bd, 0xff9c6926,
        };

        List<Tile> tiles = JsonSerializer.Deserialize<List<Tile>>(
            new FileStream(@"C:\Users\ragan\Desktop\tiles2.json", FileMode.Open))!;

        using VideoWriter writer = new("1080.avi", FourCC.DIVX, 144, new(1080, 1080));
        Rgb[] arr = new Rgb[128 * 128];
        Array.Fill<Rgb>(arr, 0xFF_FFFF);
        using Mat mat = new(128, 128, MatType.CV_8UC3, arr);
        using Mat resized = new();

        int i = 0;
        foreach (Tile tile in tiles)
        {
            if (tile.PlaceId != "fa30bf1a-83c7-4bd2-aa97-b450404f8140" || tile.DeleteId != null) continue;
            byte color = tile.Color;
            if (color > colors.Length - 1) color = color.GetNibble(0);
            arr[128 * tile.Y + tile.X] = colors[color];
            Cv2.Resize(mat, resized, new(1080, 1080), 0, 0, InterpolationFlags.Nearest);
            writer.Write(resized);
            System.Console.WriteLine($"{tile.CreatedTimeUtc} {i++}/{tiles.Count}");
        }
    }
}