using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class ValidateFileName : MonoBehaviour
{

    [Header("生成物体")] public bool isGenerate = false;
    public GameObject Prefab;
    public Transform GenerateParent;

    [Header("修改生成物体名称")] public bool isSetGenerate = false;

    [Header("修改文件名称")] public bool isSetName = false;

    [Header("资源路径")] public string path;

    

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (isSetGenerate)
        {
            isSetGenerate = false;
            foreach (Transform item in GenerateParent)
            {
                item.name = NormalizeName(item.name); ;
            }
        }


        if (isGenerate)
        {
            isGenerate = false;

            if (Prefab == null || GenerateParent == null)
            {
                Debug.LogError("❌ 请指定预制体 Prefab 和生成父物体 GenerateParent！");
                return;
            }

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("❌ 请填写资源路径 path！");
                return;
            }

            Sprite[] sprites = Resources.LoadAll<Sprite>(path);
            if (sprites == null || sprites.Length == 0)
            {
                Debug.LogWarning($"⚠️ 在 Resources/{path} 下未找到任何 Sprite。");
                return;
            }

            foreach (var sprite in sprites)
            {
                GameObject obj = PrefabUtility.InstantiatePrefab(Prefab) as GameObject;
                if (obj == null)
                {
                    Debug.LogError("❌ 预制体实例化失败！");
                    return;
                }

                obj.transform.SetParent(GenerateParent, false);
                obj.name = sprite.name;

                // 处理图片赋值（尝试 Image 或 SpriteRenderer）
                var img = obj.GetComponentInChildren<UnityEngine.UI.Image>();
                if (img != null)
                {
                    img.sprite = sprite;
                }
                else
                {
                    var sr = obj.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.sprite = sprite;
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ {obj.name} 没有 Image 或 SpriteRenderer 组件");
                    }
                }

                Debug.Log($"✅ 生成: {obj.name}");
            }

            Debug.Log($"✅ 共生成 {sprites.Length} 个预制体");
        }


        if (isSetName)
        {
            isSetName = false;

            string targetFolder = $"Assets/Resources/{path}";

            if (!AssetDatabase.IsValidFolder(targetFolder))
            {
                Debug.LogError($"❌ 目录不存在: {targetFolder}");
                return;
            }

            string[] assetGuids = AssetDatabase.FindAssets("", new[] { targetFolder });
            int renameCount = 0;

            foreach (string guid in assetGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                // 跳过子目录或 .meta 文件
                if (AssetDatabase.IsValidFolder(assetPath) || assetPath.EndsWith(".meta")) continue;

                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                string newName = NormalizeName(fileName);

                if (newName != fileName)
                {
                    string error = AssetDatabase.RenameAsset(assetPath, newName);
                    if (string.IsNullOrEmpty(error))
                    {
                        Debug.Log($"✅ 重命名成功: {fileName} → {newName}");
                        renameCount++;
                    }
                    else
                    {
                        Debug.LogError($"❌ 重命名失败: {fileName}\n原因: {error}");
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"✅ 所有文件处理完成，总共重命名 {renameCount} 个资源。");
        }
#endif
    }

    // 替换字符
    private string NormalizeName(string name)
    {
        Dictionary<string, string> map = new Dictionary<string, string>
        {
            { "Ａ", "A" }, { "Ｂ", "B" }, { "Ｃ", "C" }, { "Ｆ", "F" },
            { "Ｋ", "K" }, { "Ｍ", "M" }, { "Ｎ", "N" }, 
            { "Ｏ", "O" }, { "Ｒ", "R" }, { "Ｓ", "S" },
            { "Ｕ", "U" }, { "Ｖ", "V" }, { "Ｗ", "W" }, 
            { "１", "1" }, { "２", "2" }, { "３", "3" }, { "４", "4" }, { "５", "5" }, { "６", "6" }, { "９", "9" },
            { "－", "-" }, { "＿", "_" }, { "（", "" },  { "）", "" },  { "(", "" },   { ")", "" },
            { "的", "@" }, 
            { "53-NO", "53NO" }, { "61-NC", "61NC" }, { "71-NC", "71NC" }, { "83-NO", "83NO" },
            { "54-NO", "54NO" }, { "62-NC", "62NC" }, { "72-NC", "72NC" }, { "84-NO", "84NO" }
        };

        foreach (var pair in map)
        {
            name = name.Replace(pair.Key, pair.Value);
        }

        return name;
    }
}
