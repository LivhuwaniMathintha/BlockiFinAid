namespace BlockiFinAid.Helpers;

using System;
using System.Linq;


// Helper class for GUID conversions between C# Guid and Solidity bytes16
public static class HexByteConvertor
{
    public static byte[] StringToBytes16(string hex)
    {
        if (string.IsNullOrEmpty(hex)) return new byte[16]; // Return empty bytes16 for null/empty input

        // Ensure the hex string is exactly 32 characters (16 bytes * 2 hex chars/byte)
        // and contains only valid hex characters.
        if (hex.Length != 32 || !hex.All(c => Uri.IsHexDigit(c)))
        {
            throw new ArgumentException("Hex string must be 32 characters long and contain only hexadecimal digits for bytes16.", nameof(hex));
        }

        byte[] bytes = new byte[16];
        for (int i = 0; i < 16; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return bytes;
    }

    public static string Bytes16ToString(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0) return string.Empty; // Return empty string for null/empty input

        if (bytes.Length != 16)
        {
            throw new ArgumentException("Byte array must be 16 bytes long for bytes16.", nameof(bytes));
        }

        // Convert byte array to hexadecimal string and remove hyphens
        return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
    }
}

