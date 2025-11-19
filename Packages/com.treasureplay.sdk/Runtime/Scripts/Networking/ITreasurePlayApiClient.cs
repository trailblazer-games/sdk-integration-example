using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TreasurePlay.SDK.Core;

namespace TreasurePlay.SDK.Networking
{
    public interface ITreasurePlayApiClient
    {
        Task<TreasurePlayInitResponse> InitializeAsync(TreasurePlayInitRequest request, IDictionary<string, string> additionalHeaders, CancellationToken cancellationToken);
        Task<TreasurePlayInventoryResponse> GetInventoryAsync(string coinSkuId, string sessionToken, CancellationToken cancellationToken);
        Task<TreasurePlayRedeemResponse> RedeemAsync(string message, string sessionToken, CancellationToken cancellationToken);
    }
}
