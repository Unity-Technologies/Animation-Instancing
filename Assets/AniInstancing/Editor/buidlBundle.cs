using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class buidlBundle : MonoBehaviour
{
    static string Path = "Assets/AssetBundle";
    static string FolderName = "AssetBundle";

    [MenuItem("Custom Editor/AssetBundle/BuildAssetBundle PC")]
    static void CreateAssetBundlePC()
    {
        CheckDirectory(Path);
        BuildPipeline.BuildAssetBundles(Path, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);
        FileUtil.DeleteFileOrDirectory("Assets/StreamingAssets/" + FolderName);
        FileUtil.CopyFileOrDirectory(Path, "Assets/StreamingAssets/" + FolderName);
    }

    [MenuItem("Custom Editor/AssetBundle/BuildAssetBundle OSX")]
    static void CreateAssetBundleOSX()
    {
        CheckDirectory(Path);
        BuildPipeline.BuildAssetBundles(Path, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneOSXIntel64);
        FileUtil.DeleteFileOrDirectory("Assets/StreamingAssets/" + FolderName);
        FileUtil.CopyFileOrDirectory(Path, "Assets/StreamingAssets/" + FolderName);
    }

    [MenuItem("Custom Editor/AssetBundle/BuildAssetBundle ios")]
	static void CreateAssetBundleIos()
	{
        CheckDirectory(Path);
        BuildPipeline.BuildAssetBundles (Path, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.iOS);
        FileUtil.DeleteFileOrDirectory("Assets/StreamingAssets/" + FolderName);
        FileUtil.CopyFileOrDirectory(Path, "Assets/StreamingAssets/" + FolderName);
    }

	[MenuItem("Custom Editor/AssetBundle/BuildAssetBundle Android")]
	static void CreateAssetBundleAndroid()
	{
        CheckDirectory(Path);
        BuildPipeline.BuildAssetBundles (Path, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
        FileUtil.DeleteFileOrDirectory("Assets/StreamingAssets/" + FolderName);
        FileUtil.CopyFileOrDirectory(Path, "Assets/StreamingAssets/" + FolderName);
    }

    static void CheckDirectory(string path)
    {
        if (!Directory.Exists(path))
            AssetDatabase.CreateFolder("Assets", FolderName);
    }
}
