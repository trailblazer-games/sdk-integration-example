using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace TreasurePlay.SDK.Networking
{
    internal static class UnityWebRequestExtensions
    {
        internal static async Task<UnityWebRequest> SendWebRequestAsync(this UnityWebRequest request, CancellationToken cancellationToken)
        {
            var completionSource = new TaskCompletionSource<UnityWebRequest>();

            cancellationToken.Register(() =>
            {
                if (!completionSource.Task.IsCompleted)
                {
                    request.Abort();
                    completionSource.TrySetCanceled(cancellationToken);
                }
            });

            request.SendWebRequest().completed += _ =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    completionSource.TrySetResult(request);
                }
                else if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    completionSource.TrySetException(new InvalidOperationException(request.error));
                }
                else
                {
                    completionSource.TrySetResult(request);
                }
            };

            return await completionSource.Task.ConfigureAwait(false);
        }
    }
}
