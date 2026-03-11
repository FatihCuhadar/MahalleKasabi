using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Creates/updates Assets/Resources/RuntimeConfig.asset with all customer prefabs and products.
/// </summary>
public static class RuntimeConfigSetup
{
    [MenuItem("MahalleKasabi/Setup RuntimeConfig")]
    public static void Setup()
    {
        // Ensure Resources folder exists
        string resourcesPath = "Assets/Resources";
        if (!AssetDatabase.IsValidFolder(resourcesPath))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        string assetPath = "Assets/Resources/RuntimeConfig.asset";

        // Load or create
        var config = AssetDatabase.LoadAssetAtPath<RuntimeConfig>(assetPath);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<RuntimeConfig>();
            AssetDatabase.CreateAsset(config, assetPath);
            Debug.Log("[RuntimeConfigSetup] Created new RuntimeConfig.asset");
        }

        var so = new SerializedObject(config);

        // ─── Customer Prefabs ───
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Customers" });
        var prefabProp = so.FindProperty("customerPrefabs");
        prefabProp.arraySize = prefabGuids.Length;
        for (int i = 0; i < prefabGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            prefabProp.GetArrayElementAtIndex(i).objectReferenceValue = prefab;
        }

        // ─── Product Data SOs ───
        string[] productGuids = AssetDatabase.FindAssets("t:ProductData", new[] { "Assets/ScriptableObjects/Products" });
        var productProp = so.FindProperty("productDataList");
        productProp.arraySize = productGuids.Length;
        for (int i = 0; i < productGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(productGuids[i]);
            var product = AssetDatabase.LoadAssetAtPath<ProductData>(path);
            productProp.GetArrayElementAtIndex(i).objectReferenceValue = product;
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[RuntimeConfigSetup] RuntimeConfig updated: {prefabGuids.Length} prefab(s), {productGuids.Length} product(s)");
    }
}
