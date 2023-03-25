using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AnimationExtractor : MonoBehaviour
{
    [MenuItem("Assets/Extract Animation")]
    private static void ExtractAnimation()
    {
        foreach (var obj in Selection.objects)
        {
            var fileFullPath = AssetDatabase.GetAssetPath(obj);
            var baseDir = Path.GetDirectoryName(fileFullPath);
            var animationDir = Path.Combine(baseDir, "Animations");
            FileHelper.CreateDirectory(animationDir);

            ExtractAnimations(fileFullPath, animationDir);
        }
    }

    static void ExtractAnimations(string fbxFullPath, string animationDir)
    {
        var fileName = Path.GetFileNameWithoutExtension(fbxFullPath);
        var assetName = fileName;

        var assetPath = fbxFullPath;
        var assetRepresentationsAtPath = AssetDatabase
            .LoadAllAssetRepresentationsAtPath(assetPath);
        foreach (var assetRepresentation in assetRepresentationsAtPath)
        {
            var animationClip = assetRepresentation as AnimationClip;

            if (animationClip != null
                && !animationClip.name.Contains("tpose"))
            {
                // change to a new name
                var newName = GetClipNewName(assetName, animationClip.name);

                var filePath = $"{animationDir}/{newName}.anim";
                CreateAnimationClip(animationClip, filePath);
            }
        }
    }

    private static void CreateAnimationClip(AnimationClip srcClip
        , string filePath)
    {
        AnimationClip tempClip = new AnimationClip();
        EditorUtility.CopySerialized(srcClip, tempClip);
        AssetDatabase.CreateAsset(tempClip, filePath);
        AssetDatabase.SaveAssets();
    }

    private static string GetClipNewName(string assetName, string animNameOrgin)
    {
        var newName = assetName;
        var nameAffix = animNameOrgin;
        if (nameAffix == newName)
        {
            nameAffix = "";
        }

        if (animNameOrgin == "Unreal Take")
        {
            nameAffix = "";
        }
        // cs qc model
        else if (animNameOrgin.Contains("|"))
        {
            nameAffix = animNameOrgin.Split('|').LastOrDefault()
                .ToUpperCamel();

            nameAffix = nameAffix.Replace("Shoot", "Fire");
            nameAffix = nameAffix.Replace("Select", "Draw");
        }

        if (nameAffix != "")
        {
            newName += "_" + nameAffix;
        }
        return newName;
    }
}