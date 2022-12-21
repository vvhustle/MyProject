using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetBundleMenu
{
    [MenuItem("AssetBundle/Build AssetBundles (Windows)")]
    static void BuildAllAssetBundlesWindows()
    {
        string assetBundleDirectory = Path.Combine(Application.dataPath, "../../GM_Seed_server/data/assetbundles/Windows");
        Debug.Log(assetBundleDirectory);
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory,
                                        BuildAssetBundleOptions.None,
                                        BuildTarget.StandaloneWindows);
        System.Diagnostics.Process.Start(assetBundleDirectory);
    }

    [MenuItem("AssetBundle/Build AssetBundles (Android)")]
    static void BuildAllAssetBundlesAndroid()
    {
        string assetBundleDirectory = Path.Combine(Application.dataPath, "../../GM_Seed_server/data/assetbundles/Android");
        Debug.Log(assetBundleDirectory);
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory,
                                        BuildAssetBundleOptions.None,
                                        BuildTarget.Android);
        System.Diagnostics.Process.Start(assetBundleDirectory);
    }

    [MenuItem("Assets/Set Bundle Name as Folder Slash Name")]
    private static void CreateSpritePrefabMenu()
    {
        foreach (var selectedObj in Selection.objects)
        {
            var texturePath = AssetDatabase.GetAssetPath(selectedObj);
            var assetPath = Path.GetDirectoryName(texturePath);
            var folderName = assetPath.Substring(assetPath.LastIndexOf('\\') + 1);
            AssetImporter.GetAtPath(texturePath).SetAssetBundleNameAndVariant($"{folderName}/{selectedObj.name}", "");
        }
    }
}
