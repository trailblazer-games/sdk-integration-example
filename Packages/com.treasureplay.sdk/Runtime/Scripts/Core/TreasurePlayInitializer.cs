using UnityEngine;

namespace TreasurePlay.SDK.Core
{
    /// <summary>
    /// Scene entry point ensuring the TreasurePlaySDK runtime exists. Optionally initializes the SDK using inspector values for quick tests.
    /// </summary>
    public sealed class TreasurePlayInitializer : MonoBehaviour
    {
        [SerializeField]
        private TreasurePlaySettings _settingsOverride;

        [SerializeField]
        private bool _initializeOnAwake = false;

        [SerializeField]
        private bool _useInspectorIdentityForTesting = false;

        [SerializeField]
        private InspectorIdentity _inspectorIdentity = new InspectorIdentity();

        private void Awake()
        {
            if (!TreasurePlaySDK.Instance.IsInitialized)
            {
                TreasurePlayLogger.Log("TreasurePlayInitializer ensured SDK instance presence.");
            }

            if (_initializeOnAwake && _useInspectorIdentityForTesting && _inspectorIdentity.IsValid())
            {
                InitializeWithIdentity(_inspectorIdentity.ToUserIdentity());
            }
        }

        public void InitializeWithIdentity(TreasurePlayUserIdentity identity)
        {
            TreasurePlaySDK.Instance.Initialize(identity, _settingsOverride);
        }

        [System.Serializable]
        private sealed class InspectorIdentity
        {
            [SerializeField]
            private string _cuid = "";

            [SerializeField]
            private string _fbAi = "";

            [SerializeField]
            private string _email = "";

            [SerializeField]
            private string _displayName = "";

            public bool IsValid() => !string.IsNullOrWhiteSpace(_cuid) || !string.IsNullOrWhiteSpace(_fbAi);

            public TreasurePlayUserIdentity ToUserIdentity()
            {
                return new TreasurePlayUserIdentity.Builder()
                    .WithCuid(_cuid)
                    .WithFbAi(_fbAi)
                    .WithEmail(_email)
                    .WithDisplayName(_displayName)
                    .Build();
            }
        }
    }
}
