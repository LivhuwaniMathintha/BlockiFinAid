using System.ComponentModel;
using System.Reflection;
using Nethereum.Hex.HexTypes;

namespace BlockiFinAid.Data.Responses;

public class SmartContractResponse<T>
{
    public DateTime BlockTimeStamp { get; set; } = DateTime.UtcNow;
    public string ContractAddress { get; set; } = string.Empty;
    public string ByteCode { get; set; } = string.Empty;
    public List<string> FunctionSigHashes { get; set; } = new List<string>();
    public bool IsErc20 { get; set; } = false;
    public bool IsErc721 { get; set; } = false;
    public HexBigInteger? BlockNumber { get; set; }
    public string BlockHash { get; set; } = string.Empty;
    public T? Data { get; set; }
    public bool IsSuccess { get; set; } = false;
}

public enum OperationType
{
    // The value "Insert" is associated with the enum member Insert.
    [Description("Insert")]
    Insert,
    
    // The value "Update" is associated with the enum member Update.
    [Description("Update")]
    Update,
    
    // The value "Create" is associated with the enum member Create.
    [Description("Create")]
    Create,
    
    // The value "Get" is associated with the enum member Get.
    [Description("Get")]
    Get
}

public static class EnumExtensions
{
    /// <summary>
    /// Gets the string value from the Description attribute of an enum member.
    /// </summary>
    /// <param name="value">The enum member.</param>
    /// <returns>The string value from the Description attribute, or the enum's name if not found.</returns>
    public static string GetDescription(this Enum value)
    {
        // Get the FieldInfo for the enum member.
        FieldInfo fieldInfo = value.GetType().GetField(value.ToString());

        // Get all Description attributes on the field.
        DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

        // If there are attributes, return the description from the first one.
        if (attributes != null && attributes.Length > 0)
        {
            return attributes[0].Description;
        }
        else
        {
            // If no Description attribute is found, return the name of the enum member itself.
            return value.ToString();
        }
    }
}

