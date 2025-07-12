namespace BlockiFinAid.Helpers;

public static class GuidHelper
{
    public static byte[] GuidToBytes16OrZero(string guidString)
    {
        if (Guid.TryParse(guidString, out Guid guid))
        {
            return guid.ToByteArray(); // Valid Guid, returns 16 bytes
        }
        else
        {
            return new byte[16]; // All zeros, represents 0x000... in Solidity
        }
    }
}