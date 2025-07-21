namespace BlockiFinAid.Helpers;

public static class GuidHelper
{
    public static byte[] GuidToBytes16OrZero(string guidString)
    {
        if (Guid.TryParse(guidString, out var guid))
        {
            var bytes = guid.ToByteArray();
            if (bytes.Length == 16)
                return bytes;
        }
        return new byte[16]; // zero bytes
    }
}