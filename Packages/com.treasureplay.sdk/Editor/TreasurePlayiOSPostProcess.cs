#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace TreasurePlay.SDK.Editor
{
    public static class TreasurePlayiOSPostProcess
    {
        [PostProcessBuild(999)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (buildTarget != BuildTarget.iOS)
                return;

            // Get plist
            string plistPath = pathToBuiltProject + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            // Get root
            PlistElementDict rootDict = plist.root;

            // Add WebKit framework
            string projPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
            PBXProject proj = new PBXProject();
            proj.ReadFromFile(projPath);

            // Get the UnityFramework target (where native code is linked)
            string frameworkTargetGuid = proj.GetUnityFrameworkTargetGuid();
            
            // Add WebKit framework to UnityFramework target
            proj.AddFrameworkToProject(frameworkTargetGuid, "WebKit.framework", false);
            
            UnityEngine.Debug.Log("[TreasurePlay] Added WebKit.framework to UnityFramework target");

            // Write changes
            proj.WriteToFile(projPath);
            
            // Write plist changes
            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }
}
#endif
