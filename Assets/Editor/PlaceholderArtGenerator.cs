using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class PlaceholderArtGenerator
{
    private const string OutputFolder = "Assets/Sprites/Generated";

    [MenuItem("MahalleKasabi/Generate Placeholder Art")]
    public static void GeneratePlaceholderArt()
    {
        EnsureFolder("Assets/Sprites");
        EnsureFolder(OutputFolder);

        Sprite customerBody = SaveAndImport("customer_body.png", CreateCustomerTexture(new Color(0.1f, 0.1f, 0.18f), Color.white));
        Sprite customerHappy = SaveAndImport("customer_happy.png", CreateCustomerTexture(new Color(0.1f, 0.1f, 0.18f), new Color(0.98f, 0.86f, 0.35f)));
        Sprite customerAngry = SaveAndImport("customer_angry.png", CreateCustomerTexture(new Color(0.1f, 0.1f, 0.18f), new Color(0.9f, 0.2f, 0.2f)));
        Sprite shopCounter = SaveAndImport("shop_counter.png", CreateShopCounterTexture());
        Sprite backgroundShop = SaveAndImport("background_shop.png", CreateBackgroundTexture());
        Sprite coinIcon = SaveAndImport("coin_icon.png", CreateCoinTexture());
        Sprite queueArrow = SaveAndImport("queue_arrow.png", CreateQueueArrowTexture());
        Sprite bubbleRounded = SaveAndImport("bubble_rounded.png", CreateRoundedRectTexture(128, 84, 12, Color.white));
        Sprite bubbleTail = SaveAndImport("bubble_tail.png", CreateBubbleTailTexture());
        Sprite whitePixel = SaveAndImport("white_pixel.png", CreateSolidTexture(8, 8, Color.white));

        ConfigureOrderBubblePrefab(bubbleRounded, bubbleTail, whitePixel);
        ConfigureCustomerPrefab(customerBody, customerHappy, customerAngry);
        WireSceneSprites(shopCounter, backgroundShop, queueArrow, coinIcon);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[PlaceholderArtGenerator] Placeholder art generated and prefabs updated.");
    }

    private static void ConfigureOrderBubblePrefab(Sprite background, Sprite tail, Sprite whitePixel)
    {
        const string prefabPath = "Assets/Prefabs/UI/OrderBubble.prefab";
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Prefabs/UI");

        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        if (root == null)
        {
            root = new GameObject("OrderBubble", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CustomerOrderBubble));
        }

        CleanupChildren(root.transform);

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(170f, 100f);
        rootRect.localScale = new Vector3(0.008f, 0.008f, 0.008f);

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 12;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100f;

        GameObject bg = NewUI("Background", root.transform);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.sprite = background;
        bgImage.type = Image.Type.Sliced;
        bgImage.color = Hex("#F5F5F5");
        bgImage.raycastTarget = false;
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        Stretch(bgRect, new Vector2(0f, 0.15f), Vector2.one);

        GameObject productNameGO = NewUI("ProductNameText", root.transform);
        TextMeshProUGUI productName = productNameGO.AddComponent<TextMeshProUGUI>();
        productName.fontSize = 18;
        productName.fontStyle = FontStyles.Bold;
        productName.color = Color.black;
        productName.alignment = TextAlignmentOptions.Center;
        productName.text = "ET";
        productName.raycastTarget = false;
        RectTransform productRect = productNameGO.GetComponent<RectTransform>();
        Stretch(productRect, new Vector2(0.06f, 0.5f), new Vector2(0.94f, 0.9f));

        GameObject qtyGO = NewUI("QuantityText", root.transform);
        TextMeshProUGUI qtyText = qtyGO.AddComponent<TextMeshProUGUI>();
        qtyText.fontSize = 14;
        qtyText.fontStyle = FontStyles.Bold;
        qtyText.color = Hex("#E8762B");
        qtyText.alignment = TextAlignmentOptions.Center;
        qtyText.text = "x1";
        qtyText.raycastTarget = false;
        RectTransform qtyRect = qtyGO.GetComponent<RectTransform>();
        Stretch(qtyRect, new Vector2(0.2f, 0.3f), new Vector2(0.8f, 0.52f));

        GameObject barBgGO = NewUI("PatienceBarBG", root.transform);
        Image barBg = barBgGO.AddComponent<Image>();
        barBg.sprite = whitePixel;
        barBg.type = Image.Type.Sliced;
        barBg.color = new Color(0f, 0f, 0f, 0.25f);
        barBg.raycastTarget = false;
        RectTransform barBgRect = barBgGO.GetComponent<RectTransform>();
        Stretch(barBgRect, new Vector2(0.08f, 0.17f), new Vector2(0.92f, 0.26f));

        GameObject barGO = NewUI("PatienceBar", barBgGO.transform);
        Image patienceBar = barGO.AddComponent<Image>();
        patienceBar.sprite = whitePixel;
        patienceBar.type = Image.Type.Filled;
        patienceBar.fillMethod = Image.FillMethod.Horizontal;
        patienceBar.fillAmount = 1f;
        patienceBar.color = Color.green;
        patienceBar.raycastTarget = false;
        RectTransform barRect = barGO.GetComponent<RectTransform>();
        Stretch(barRect, Vector2.zero, Vector2.one);

        GameObject tailGO = NewUI("Tail", root.transform);
        Image tailImg = tailGO.AddComponent<Image>();
        tailImg.sprite = tail;
        tailImg.color = Hex("#F5F5F5");
        tailImg.raycastTarget = false;
        RectTransform tailRect = tailGO.GetComponent<RectTransform>();
        tailRect.anchorMin = new Vector2(0.5f, 0f);
        tailRect.anchorMax = new Vector2(0.5f, 0f);
        tailRect.pivot = new Vector2(0.5f, 0f);
        tailRect.anchoredPosition = new Vector2(0f, 0f);
        tailRect.sizeDelta = new Vector2(20f, 16f);

        CustomerOrderBubble bubble = root.GetComponent<CustomerOrderBubble>();
        SerializedObject so = new SerializedObject(bubble);
        so.FindProperty("productNameText").objectReferenceValue = productName;
        so.FindProperty("quantityText").objectReferenceValue = qtyText;
        so.FindProperty("patienceBar").objectReferenceValue = patienceBar;
        so.FindProperty("productIcon").objectReferenceValue = null;
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static void ConfigureCustomerPrefab(Sprite body, Sprite happy, Sprite angry)
    {
        const string path = "Assets/Prefabs/Customers/Customer.prefab";
        if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), path)))
            return;

        GameObject root = PrefabUtility.LoadPrefabContents(path);
        Transform bodyTransform = root.transform.Find("Body");
        if (bodyTransform == null)
        {
            GameObject bodyGO = new GameObject("Body");
            bodyGO.transform.SetParent(root.transform, false);
            bodyTransform = bodyGO.transform;
        }

        SpriteRenderer renderer = bodyTransform.GetComponent<SpriteRenderer>();
        if (renderer == null) renderer = bodyTransform.gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = body;
        renderer.color = Color.white;

        Customer customer = root.GetComponent<Customer>();
        if (customer != null)
        {
            SerializedObject so = new SerializedObject(customer);
            so.FindProperty("spriteRenderer").objectReferenceValue = renderer;
            so.FindProperty("happySprite").objectReferenceValue = happy;
            so.FindProperty("angrySprite").objectReferenceValue = angry;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static void WireSceneSprites(Sprite shopCounter, Sprite backgroundShop, Sprite queueArrow, Sprite coinIcon)
    {
        CustomerManager manager = Object.FindFirstObjectByType<CustomerManager>();
        if (manager != null)
        {
            SerializedObject so = new SerializedObject(manager);
            so.FindProperty("queueArrowSprite").objectReferenceValue = queueArrow;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        SpriteRenderer counterRenderer = FindOrCreateSpriteRenderer("ShopCounter");
        if (counterRenderer != null)
        {
            counterRenderer.sprite = shopCounter;
            counterRenderer.color = Color.white;
            counterRenderer.sortingOrder = 2;
        }

        SpriteRenderer backgroundRenderer = FindOrCreateSpriteRenderer("BackgroundShop");
        if (backgroundRenderer != null)
        {
            backgroundRenderer.sprite = backgroundShop;
            backgroundRenderer.color = Hex("#1A1A2E");
            backgroundRenderer.sortingOrder = -10;
            backgroundRenderer.transform.position = new Vector3(0f, 0f, 8f);
            backgroundRenderer.transform.localScale = Vector3.one;
        }

        GameObject coinIconGO = GameObject.Find("MoneyCoinIcon");
        if (coinIconGO != null)
        {
            Image img = coinIconGO.GetComponent<Image>();
            if (img != null) img.sprite = coinIcon;
            RectTransform rt = coinIconGO.GetComponent<RectTransform>();
            if (rt != null) rt.sizeDelta = new Vector2(32f, 32f);
        }
    }

    private static SpriteRenderer FindOrCreateSpriteRenderer(string name)
    {
        GameObject go = GameObject.Find(name);
        if (go == null)
        {
            go = new GameObject(name);
        }

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) sr = go.AddComponent<SpriteRenderer>();
        return sr;
    }

    private static void CleanupChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(root.GetChild(i).gameObject);
        }
    }

    private static GameObject NewUI(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }

    private static Sprite SaveAndImport(string fileName, Texture2D texture)
    {
        string relative = $"{OutputFolder}/{fileName}";
        string absolute = Path.Combine(Directory.GetCurrentDirectory(), relative);
        File.WriteAllBytes(absolute, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(relative, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(relative) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(relative);
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(name))
                AssetDatabase.CreateFolder(parent, name);
        }
    }

    private static Texture2D CreateSolidTexture(int width, int height, Color color)
    {
        Texture2D tex = NewTexture(width, height);
        Fill(tex, color);
        tex.Apply();
        return tex;
    }

    private static Texture2D CreateRoundedRectTexture(int width, int height, int radius, Color color)
    {
        Texture2D tex = NewTexture(width, height);
        Color clear = new Color(0f, 0f, 0f, 0f);
        Fill(tex, clear);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool inside = true;
                if (x < radius && y < radius) inside = (new Vector2(x - radius, y - radius)).sqrMagnitude <= radius * radius;
                else if (x > width - radius - 1 && y < radius) inside = (new Vector2(x - (width - radius - 1), y - radius)).sqrMagnitude <= radius * radius;
                else if (x < radius && y > height - radius - 1) inside = (new Vector2(x - radius, y - (height - radius - 1))).sqrMagnitude <= radius * radius;
                else if (x > width - radius - 1 && y > height - radius - 1) inside = (new Vector2(x - (width - radius - 1), y - (height - radius - 1))).sqrMagnitude <= radius * radius;
                if (inside) tex.SetPixel(x, y, color);
            }
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D CreateBubbleTailTexture()
    {
        Texture2D tex = NewTexture(24, 18);
        Fill(tex, new Color(0f, 0f, 0f, 0f));
        Color c = Color.white;
        for (int y = 0; y < 18; y++)
        {
            int half = Mathf.Max(0, (17 - y) / 2);
            int left = 12 - half;
            int right = 12 + half;
            for (int x = left; x <= right; x++)
                tex.SetPixel(x, y, c);
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D CreateCustomerTexture(Color bg, Color person)
    {
        Texture2D tex = NewTexture(48, 64);
        Fill(tex, bg);
        DrawFilledCircle(tex, 24, 45, 10, person);
        DrawRect(tex, 16, 17, 16, 24, person);
        DrawRect(tex, 14, 8, 7, 11, person);
        DrawRect(tex, 27, 8, 7, 11, person);
        tex.Apply();
        return tex;
    }

    private static Texture2D CreateShopCounterTexture()
    {
        Texture2D tex = NewTexture(128, 32);
        Color wood = Hex("#8B4513");
        Fill(tex, wood);
        DrawRect(tex, 0, 25, 128, 2, Color.white);
        tex.Apply();
        return tex;
    }

    private static Texture2D CreateBackgroundTexture()
    {
        Texture2D tex = NewTexture(320, 180);
        Color top = Hex("#1A1A2E");
        Color bottom = Hex("#16213E");
        for (int y = 0; y < tex.height; y++)
        {
            float t = y / (tex.height - 1f);
            Color c = Color.Lerp(bottom, top, t);
            for (int x = 0; x < tex.width; x++)
                tex.SetPixel(x, y, c);
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D CreateCoinTexture()
    {
        Texture2D tex = NewTexture(32, 32);
        Fill(tex, new Color(0f, 0f, 0f, 0f));
        Color gold = Hex("#F4A422");
        DrawFilledCircle(tex, 16, 16, 14, gold);
        DrawCircleOutline(tex, 16, 16, 14, new Color(0.85f, 0.52f, 0.05f, 1f));
        DrawLetterT(tex, 9, 8, Color.white);
        DrawLetterL(tex, 17, 8, Color.white);
        tex.Apply();
        return tex;
    }

    private static Texture2D CreateQueueArrowTexture()
    {
        Texture2D tex = NewTexture(32, 32);
        Fill(tex, new Color(0f, 0f, 0f, 0f));
        Color c = Color.white;
        DrawRect(tex, 4, 14, 16, 4, c);
        DrawRightTriangle(tex, 20, 10, 10, 12, c);
        tex.Apply();
        return tex;
    }

    private static Texture2D NewTexture(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Point;
        return tex;
    }

    private static void Fill(Texture2D tex, Color c)
    {
        Color[] pixels = new Color[tex.width * tex.height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = c;
        tex.SetPixels(pixels);
    }

    private static void DrawRect(Texture2D tex, int x, int y, int w, int h, Color c)
    {
        for (int iy = y; iy < y + h; iy++)
        {
            if (iy < 0 || iy >= tex.height) continue;
            for (int ix = x; ix < x + w; ix++)
            {
                if (ix < 0 || ix >= tex.width) continue;
                tex.SetPixel(ix, iy, c);
            }
        }
    }

    private static void DrawFilledCircle(Texture2D tex, int cx, int cy, int radius, Color c)
    {
        int r2 = radius * radius;
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= r2)
                {
                    int px = cx + x;
                    int py = cy + y;
                    if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                        tex.SetPixel(px, py, c);
                }
            }
        }
    }

    private static void DrawCircleOutline(Texture2D tex, int cx, int cy, int radius, Color c)
    {
        int r2 = radius * radius;
        int inner = (radius - 2) * (radius - 2);
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                int d = x * x + y * y;
                if (d <= r2 && d >= inner)
                {
                    int px = cx + x;
                    int py = cy + y;
                    if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                        tex.SetPixel(px, py, c);
                }
            }
        }
    }

    private static void DrawRightTriangle(Texture2D tex, int x, int y, int w, int h, Color c)
    {
        for (int iy = 0; iy < h; iy++)
        {
            int span = Mathf.RoundToInt((iy / (float)(h - 1)) * w);
            for (int ix = 0; ix <= span; ix++)
            {
                int px = x + ix;
                int py = y + iy;
                if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                    tex.SetPixel(px, py, c);
            }
        }
    }

    private static void DrawLetterT(Texture2D tex, int x, int y, Color c)
    {
        DrawRect(tex, x, y + 10, 7, 2, c);
        DrawRect(tex, x + 2, y, 2, 12, c);
    }

    private static void DrawLetterL(Texture2D tex, int x, int y, Color c)
    {
        DrawRect(tex, x, y, 2, 12, c);
        DrawRect(tex, x, y, 6, 2, c);
    }

    private static Color Hex(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color)) return color;
        return Color.white;
    }
}
