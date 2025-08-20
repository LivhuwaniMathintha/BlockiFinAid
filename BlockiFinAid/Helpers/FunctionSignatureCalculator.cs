using System.Text;
using BlockiFinAid.Services.SmartContracts.BankAccount;
using BlockiFinAid.Services.SmartContracts.Funder;
using BlockiFinAid.Services.SmartContracts.Funding;
using BlockiFinAid.Services.SmartContracts.Payment;
using BlockiFinAid.Services.SmartContracts.User;
using Nethereum.Util;
using Nethereum.Hex.HexConvertors.Extensions;
namespace BlockiFinAid.Helpers;

public static class FunctionSignatureCalculator
{
   // Dictionary mapping group names to (model type, signature, isStateChanging) tuples
    private static readonly Dictionary<string, List<(Type modelType, string signature, bool isStateChanging)>> FunctionGroups = new Dictionary<string, List<(Type, string, bool)>>
    {
        {
            "BankAccount", new List<(Type, string, bool)>
            {
                (typeof(AddBankAccountFunction), "addBankAccount(bytes16,string,string,string,bool,string,bytes16,string)", true),
                (typeof(UpdateBankAccountByNumberFunction), "updateBankAccountByNumber(string,string,string,bool,string,bytes16,string)", true)
            }
        },
        {
            "Funder", new List<(Type, string, bool)>
            {
                (typeof(AddFunderFunction), "addFunder(bytes16,string,bool,bytes16,bool,string,string,string)", true),
                (typeof(UpdateFunderByIdFunction), "updateFunderById(bytes16,string,bool,bytes16,bool,string,string,string)", true)
            }
        },
        {
            "Funding", new List<(Type, string, bool)>
            {
                (typeof(AddFundingFunction), "addFunding(bytes16,bytes16,bytes16,bytes16,bytes16,string,bool,uint256,uint256,uint256,uint256,string,string)", true),
                (typeof(UpdateFundingFunction), "updateFunding(bytes16,bool,uint256,uint256,uint256,uint256,string,string)", true),
                (typeof(UpdateFundingDataConfirmedByIdFunction), "updateFundingDataConfirmedById(bytes16,bytes16,string,string)", true)
            }
        },
        {
            "Condition", new List<(Type, string, bool)>
            {
                (typeof(AddConditionFunction), "addCondition(bytes16,bool,string,string,uint256,uint256,uint256,uint256,uint256,bool,bytes16,string,string,bool,uint256)", true),
                (typeof(UpdateConditionFunction), "updateCondition(bytes16,bool,uint256,string,string)", true),
                (typeof(UpdateDataConfirmedByIdFunction), "updateDataConfirmedById(bytes16,bytes16,string,string)", true)
            }
        },
        {
            "Payment", new List<(Type, string, bool)>
            {
                (typeof(AddPaymentFunction), "addPayment(bytes16,string,string,string,string,uint256,string,string,string,bool,string,string,string)", true),
                (typeof(UpdatePaymentByIdFunction), "updatePaymentById(bytes16,string,string,string,string)", true)
            }
        },
        {
            "User", new List<(Type, string, bool)>
            {
                (typeof(AddUserFunction), "addUser(bytes16,bytes16,bytes16,bytes16,string,string,string,bool,bool,string,string,string,string)", true),
                (typeof(UpdateUserFunction), "updateUser(bytes16,bytes16,bytes16,bytes16,string,string,string,bool,bool,string,string,string,string)", true)
            }
        }
    };
    
    public static List<string> GetStateChangingFunctionSignaturesByGroup(string groupName)
    {
        if (!FunctionGroups.TryGetValue(groupName, out var group))
        {
            throw new ArgumentException($"No function group defined for {groupName}");
        }

        var signatures = new List<string>();
        var sha3 = new Sha3Keccack();

        foreach (var (_, signature, isStateChanging) in group)
        {
            if (isStateChanging)
            {
                var hash = sha3.CalculateHash(Encoding.UTF8.GetBytes(signature));
                string hex = Convert.ToHexString(hash.Take(4).ToArray()).ToLowerInvariant();
                signatures.Add("0x" + hex);
            }
        }
        return signatures;
    }
}