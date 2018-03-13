using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AnimationInstancing
{
    public class PostImportAnimationInfo : AssetPostprocessor
    {

        static void OnPostprocessAllAssets(string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            ModifyAnimationAsset(importedAssets);
        }


        static void ModifyAnimationAsset(string[] asset)
        {
            foreach (var path in asset)
            {
                if (path.EndsWith(".bytes") && path.Contains("AnimationTexture"))
                {
                    AssetImporter importer = AssetImporter.GetAtPath(path);
                    if (importer != null)
                    {
                        importer.assetBundleName = "AnimationTexture";
                    }
                }
            }
        }
    }
}