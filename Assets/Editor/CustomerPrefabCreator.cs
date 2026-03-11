using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;

public static class CustomerPrefabCreator
{
    [MenuItem("MahalleKasabi/Create Customer Prefab")]
    public static void Create()
    {
        // ─── CREATE PLACEHOLDER SPRITES ───
        var whiteSprite = CreatePlaceholderSprite("WhitePlaceholder", 32, 32, Color.white);
        var redSprite = CreatePlaceholderSprite("RedPlaceholder", 32, 32, Color.red);
        var bubbleBGSprite = CreatePlaceholderSprite("BubbleBGPlaceholder", 64, 40, new Color(1f, 1f, 1f, 0.9f));

        // ─── ORDER BUBBLE PREFAB (World Space Canvas) ───
        var bubblePrefab = CreateOrderBubblePrefab(bubbleBGSprite, whiteSprite, redSprite);

        // ─── CUSTOMER PREFAB ───
        var customerGO = new GameObject("Customer");

        // Root components
        var customer = customerGO.AddComponent<Customer>();
        var rb = customerGO.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        var col = customerGO.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;

        // Body child — SpriteRenderer
        var bodyGO = new GameObject("Body");
        bodyGO.transform.SetParent(customerGO.transform, false);
        var bodySR = bodyGO.AddComponent<SpriteRenderer>();
        bodySR.sprite = whiteSprite;
        bodySR.color = new Color(0.6f, 0.85f, 0.65f, 1f); // light green tint

        // Wire Customer SerializeFields
        var custSO = new SerializedObject(customer);
        custSO.FindProperty("spriteRenderer").objectReferenceValue = bodySR;
        custSO.FindProperty("orderBubblePrefab").objectReferenceValue = bubblePrefab;
        custSO.FindProperty("moveSpeed").floatValue = 3f;
        custSO.ApplyModifiedProperties();

        // Save Customer prefab
        string prefabPath = "Assets/Prefabs/Customers/Customer.prefab";
        var savedPrefab = PrefabUtility.SaveAsPrefabAsset(customerGO, prefabPath);
        Object.DestroyImmediate(customerGO);

        // ─── WIRE TO CUSTOMER MANAGER ───
        WireToCustomerManager(savedPrefab);

        // ─── UPDATE RUNTIME CONFIG ───
        RuntimeConfigSetup.Setup();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[CustomerPrefabCreator] Customer prefab created at " + prefabPath);
        Debug.Log("[CustomerPrefabCreator] OrderBubble prefab created at Assets/Prefabs/UI/OrderBubble.prefab");
        Debug.Log("[CustomerPrefabCreator] Placeholder sprites saved to Assets/Sprites/");
    }

    static GameObject CreateOrderBubblePrefab(Sprite bgSprite, Sprite iconSprite, Sprite barSprite)
    {
        // Root: World Space Canvas + CustomerOrderBubble
        var bubbleRoot = new GameObject("OrderBubble");

        var canvas = bubbleRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;

        var scaler = bubbleRoot.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100f;

        bubbleRoot.AddComponent<GraphicRaycaster>();

        var rootRect = bubbleRoot.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(160, 100);
        rootRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        // Background
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(bubbleRoot.transform, false);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.sprite = bgSprite;
        bgImg.color = new Color(1f, 1f, 1f, 0.9f);
        bgImg.raycastTarget = false;
        var bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;

        // Product Icon
        var iconGO = new GameObject("ProductIcon");
        iconGO.transform.SetParent(bubbleRoot.transform, false);
        var iconImg = iconGO.AddComponent<Image>();
        iconImg.sprite = iconSprite;
        iconImg.color = Color.white;
        iconImg.raycastTarget = false;
        var iconRect = iconGO.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.05f, 0.2f);
        iconRect.anchorMax = new Vector2(0.55f, 0.85f);
        iconRect.sizeDelta = Vector2.zero;
        iconRect.anchoredPosition = Vector2.zero;

        // Quantity Text
        var qtyGO = new GameObject("QuantityText");
        qtyGO.transform.SetParent(bubbleRoot.transform, false);
        var qtyTMP = qtyGO.AddComponent<TextMeshProUGUI>();
        qtyTMP.text = "";
        qtyTMP.fontSize = 14;
        qtyTMP.color = Color.black;
        qtyTMP.alignment = TextAlignmentOptions.Center;
        qtyTMP.raycastTarget = false;
        var qtyRect = qtyGO.GetComponent<RectTransform>();
        qtyRect.anchorMin = new Vector2(0.55f, 0.4f);
        qtyRect.anchorMax = new Vector2(0.95f, 0.85f);
        qtyRect.sizeDelta = Vector2.zero;
        qtyRect.anchoredPosition = Vector2.zero;

        // Patience Bar
        var barGO = new GameObject("PatienceBar");
        barGO.transform.SetParent(bubbleRoot.transform, false);
        var barImg = barGO.AddComponent<Image>();
        barImg.sprite = barSprite;
        barImg.color = Color.green;
        barImg.type = Image.Type.Filled;
        barImg.fillMethod = Image.FillMethod.Horizontal;
        barImg.fillAmount = 1f;
        barImg.raycastTarget = false;
        var barRect = barGO.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0.05f, 0.05f);
        barRect.anchorMax = new Vector2(0.95f, 0.18f);
        barRect.sizeDelta = Vector2.zero;
        barRect.anchoredPosition = Vector2.zero;

        // Add CustomerOrderBubble and wire
        var bubble = bubbleRoot.AddComponent<CustomerOrderBubble>();
        var bubbleSO = new SerializedObject(bubble);
        bubbleSO.FindProperty("productIcon").objectReferenceValue = iconImg;
        bubbleSO.FindProperty("quantityText").objectReferenceValue = qtyTMP;
        bubbleSO.FindProperty("patienceBar").objectReferenceValue = barImg;
        bubbleSO.ApplyModifiedProperties();

        // Save as prefab
        string bubblePath = "Assets/Prefabs/UI/OrderBubble.prefab";
        var savedBubble = PrefabUtility.SaveAsPrefabAsset(bubbleRoot, bubblePath);
        Object.DestroyImmediate(bubbleRoot);

        return savedBubble;
    }

    static Sprite CreatePlaceholderSprite(string name, int width, int height, Color color)
    {
        string pngPath = $"Assets/Sprites/{name}.png";

        // Check if already exists
        var existing = AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
        if (existing != null) return existing;

        // Create texture
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();

        // Save as PNG
        byte[] pngData = tex.EncodeToPNG();
        Object.DestroyImmediate(tex);

        string fullPath = Path.Combine(Application.dataPath, $"Sprites/{name}.png");
        File.WriteAllBytes(fullPath, pngData);

        AssetDatabase.ImportAsset(pngPath);

        // Set texture import settings to Sprite
        var importer = AssetImporter.GetAtPath(pngPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
    }

    static void WireToCustomerManager(GameObject customerPrefab)
    {
        // Find CustomerManager in scene
        var custMgr = Object.FindFirstObjectByType<CustomerManager>();
        if (custMgr == null)
        {
            Debug.LogWarning("[CustomerPrefabCreator] CustomerManager not found in scene. Run 'Setup Main Scene' first, then run this again.");
            return;
        }

        var so = new SerializedObject(custMgr);

        // Wire customer prefab
        var prefabProp = so.FindProperty("customerPrefabs");
        prefabProp.arraySize = 1;
        prefabProp.GetArrayElementAtIndex(0).objectReferenceValue = customerPrefab;

        // Wire ProductData SOs from Assets/ScriptableObjects/Products/
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

        // Mark scene dirty so it saves
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log($"[CustomerPrefabCreator] CustomerManager wired: 1 prefab, {productGuids.Length} products.");
    }
}
