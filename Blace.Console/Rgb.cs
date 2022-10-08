using System.Runtime.InteropServices;

namespace Blace.Console;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Rgb
{
    private readonly byte _b0;
    private readonly byte _b1;
    private readonly byte _b2;

    public Rgb(uint value)
    {
        _b0 = (byte)(value & 0xFF);
        _b1 = (byte)((value >> 8) & 0xFF);
        _b2 = (byte)((value >> 16) & 0xFF);
    }

    public uint Value => (uint)(_b0 | (_b1 << 8) | (_b2 << 16));

    public static implicit operator Rgb(uint value) => new(value);
}