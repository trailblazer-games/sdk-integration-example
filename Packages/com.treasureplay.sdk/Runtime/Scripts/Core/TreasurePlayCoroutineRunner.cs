using System.Collections;
using UnityEngine;

namespace TreasurePlay.SDK.Core
{
    /// <summary>
    /// Utility MonoBehaviour for executing coroutines from non-MonoBehaviour classes.
    /// </summary>
    internal sealed class TreasurePlayCoroutineRunner : MonoBehaviour
    {
        private static TreasurePlayCoroutineRunner _instance;

        internal static TreasurePlayCoroutineRunner Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                var runnerObject = new GameObject("TreasurePlayCoroutineRunner")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };

                _instance = runnerObject.AddComponent<TreasurePlayCoroutineRunner>();
                DontDestroyOnLoad(runnerObject);
                return _instance;
            }
        }

        internal void Run(IEnumerator coroutine)
        {
            if (coroutine == null)
            {
                return;
            }

            StartCoroutine(coroutine);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
