#if UNITY_EDITOR
using System.IO;
using TreasurePlay.SDK.Core;
using UnityEditor;
using UnityEngine;

namespace TreasurePlay.SDK.Editor
{
    internal static class TreasurePlayMenu
    {
        private const string SettingsAssetPath = "Assets/Resources/TreasurePlaySettings.asset";

        [MenuItem("Treasure Play/Create Settings Asset", priority = 0)]
        private static void CreateSettings()
        {
            var settings = ScriptableObject.CreateInstance<TreasurePlaySettings>();
            var directory = Path.GetDirectoryName(SettingsAssetPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            AssetDatabase.CreateAsset(settings, SettingsAssetPath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = settings;
        }

        [MenuItem("Treasure Play/Open Documentation", priority = 20)]
        private static void OpenDocs()
        {
            Application.OpenURL("http://console.treasureplay.com/documentation/sdk_integration");
        }
    }
}
#endif
