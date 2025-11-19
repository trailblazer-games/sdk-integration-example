#if UNITY_EDITOR
using TreasurePlay.SDK.Core;
using UnityEditor;
using UnityEngine;

namespace TreasurePlay.SDK.Editor
{
    [CustomEditor(typeof(TreasurePlaySettings))]
    public sealed class TreasurePlaySettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            var settings = (TreasurePlaySettings)target;

            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("apiKey"), new GUIContent("API Key"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("environment"), new GUIContent("Environment"));
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Rewards", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("coinId"), new GUIContent("Coin ID"));
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Endpoints", EditorStyles.boldLabel);
            
            GUI.enabled = false;
            EditorGUILayout.LabelField("API Endpoints", EditorStyles.miniBoldLabel);
            EditorGUILayout.TextField("Production API", settings.ProductionUrl);
            EditorGUILayout.TextField("Testnet API", settings.TestnetUrl);
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.LabelField("Inventory Endpoints", EditorStyles.miniBoldLabel);
            EditorGUILayout.TextField("Production Inventory", settings.ProductionInventoryUrlDisplay);
            EditorGUILayout.TextField("Testnet Inventory", settings.TestnetInventoryUrlDisplay);
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.LabelField("WebView", EditorStyles.miniBoldLabel);
            EditorGUILayout.TextField("WebView URL", settings.WebViewUrl);
            GUI.enabled = true;
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Logging", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumLogType"), new GUIContent("Minimum Log Type"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
