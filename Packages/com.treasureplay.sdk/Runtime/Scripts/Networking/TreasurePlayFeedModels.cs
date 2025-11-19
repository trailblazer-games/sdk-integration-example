using System;

namespace TreasurePlay.SDK.Networking
{
    [Serializable]
    public sealed class TreasurePlayInventoryResponse
    {
        public bool success;
        public string tpUid;
        public string tokens;
        public string tokenType;
        public int status;
        
        public bool IsValid => success && status == 200;
        
        /// <summary>
        /// Gets the token amount as an integer (rounded down from decimal string)
        /// </summary>
        public int GetTokenAmount()
        {
            if (string.IsNullOrEmpty(tokens))
                return 0;
                
            if (float.TryParse(tokens, out float amount))
                return (int)amount;
                
            return 0;
        }
    }
    
    [Serializable]
    public sealed class TreasurePlayRedeemRequest
    {
        public string message;
        
        public TreasurePlayRedeemRequest(string message)
        {
            this.message = message;
        }
    }
    
    [Serializable]
    public sealed class TreasurePlayRedeemResponse
    {
        public bool success;
        public string updatedBalance;
        public string message;
        public int status;
        
        public bool IsValid => success && status == 200;
        
        /// <summary>
        /// Gets the updated balance as an integer (rounded down from decimal string)
        /// </summary>
        public int GetUpdatedBalance()
        {
            if (string.IsNullOrEmpty(updatedBalance))
                return 0;
                
            if (float.TryParse(updatedBalance, out float balance))
                return (int)balance;
                
            return 0;
        }
    }
}
