using System.Numerics;
using Nethereum.Web3;

namespace BlockiFinAid.Helpers;

public static class NonceManager
{
    private static BigInteger _currentNonce;
    private static readonly object _lockObject = new object();

    public static async Task InitializeNonce(Web3 web3, string accountAddress)
    {
        _currentNonce = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(accountAddress);
    }

    public static BigInteger GetNextNonce()
    {
        lock (_lockObject)
        {
            var nextNonce = _currentNonce;
            _currentNonce++;
            return nextNonce;
        }
    }
}