using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class buidlBundle : MonoBehaviour
{
    static string Path = "Assets/AssetBundle";
    static string FolderName = "AssetBundle";

    [MenuItem("Custom Editor/AssetBundle/BuildAssetBundle")]
    static void CreateAssetBundle()
    {
        CheckDirectory(Path);
        BuildPipeline.BuildAssetBundles(Path, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
        FileUtil.DeleteFileOrDirectory("Assets/StreamingAssets/" + FolderName);
        FileUtil.CopyFileOrDirectory(Path, "Assets/StreamingAssets/" + FolderName);
    }

    static void CheckDirectory(string path)
    {
        if (!Directory.Exists(path))
            AssetDatabase.CreateFolder("Assets", FolderName);
    }
}
