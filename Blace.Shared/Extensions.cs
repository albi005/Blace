namespace Blace.Shared;

public static class Extensions
{
    public static byte GetNibble(this byte b, int index)
    {
        int isRight = index % 2;
        int shiftIfRight = isRight * 4;
        int shiftIfLeft = 4 - shiftIfRight;
        return (byte)((byte)((b >> shiftIfLeft) << shiftIfRight) >> shiftIfRight);
    }

    public static byte WithNibble(this byte b, int index, byte value)
    {
        b = b.GetNibble(index + 1);
        int isRight = index % 2;
        int shiftIfRight = isRight * 4;
        int shiftIfLeft = 4 - shiftIfRight;
        return (byte)((b << shiftIfRight) | (value << shiftIfLeft));
    }
}