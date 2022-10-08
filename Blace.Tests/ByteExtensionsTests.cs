using Blace.Shared;

namespace Blace.Tests;

[TestClass]
public class ByteExtensionsTests
{
    [TestMethod]
    [DataRow((byte)0b1010_0101, (byte)0b0000_1010, (byte)0b0000_0101)]
    public void GetNibbleTest(byte input, byte expected0, byte expected1)
    {
        byte actual0 = input.GetNibble(0);
        byte actual1 = input.GetNibble(1);
        Assert.AreEqual(expected0, actual0);
        Assert.AreEqual(expected1, actual1);
    }
    
    [TestMethod]
    public void Roundtrip_LeftFirst()
    {
        byte b = 0;
        b = b.WithNibble(10, 0xA);
        b = b.WithNibble(9, 0x5);
        byte left = b.GetNibble(0);
        byte right = b.GetNibble(1);
        Assert.AreEqual((byte)0xA, left);
        Assert.AreEqual(0x5, right);
    }
    
    [TestMethod]
    public void Roundtrip_RightFirst()
    {
        byte b = 0;
        b = b.WithNibble(9, 0x5);
        b = b.WithNibble(10, 0xA);
        byte left = b.GetNibble(0);
        byte right = b.GetNibble(1);
        Assert.AreEqual((byte)0xA, left);
        Assert.AreEqual(0x5, right);
    }
}