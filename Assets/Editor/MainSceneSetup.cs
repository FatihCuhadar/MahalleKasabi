using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class MainSceneSetup
{
    static readonly Color TEAL = new Color(0.051f, 0.451f, 0.467f, 1f);
    static readonly Color GOLD = new Color(0.957f, 0.643f, 0.133f, 1f);
    static readonly Color WHITE = Color.white;

    [MenuItem("MahalleKasabi/Setup Main Scene")]
    public static void Setup()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
        ClearScene();
        SetupCamera();

        var gameManagerGO = CreateManager("_GameManager");
        gameManagerGO.AddComponent<GameManager>();
        gameManagerGO.AddComponent<PlayerData>();

        var customerManagerGO = CreateManager("_CustomerManager");
        var customerMgr = customerManagerGO.AddComponent<CustomerManager>();
        CreateManager("_OrderManager").AddComponent<OrderManager>();
        CreateManager("_ShopManager").AddComponent<ShopManager>();
        CreateManager("_OfflineEarningsManager").AddComponent<OfflineEarningsManager>();

        var audioManagerGO = CreateManager("_AudioManager");
        var audioMgr = audioManagerGO.AddComponent<AudioManager>();
        var sfxSource = audioManagerGO.AddComponent<AudioSource>();
        var musicSource = audioManagerGO.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = 0.5f;

        CreateManager("_AdManager").AddComponent<AdManager>();

        var bg = new GameObject("BackgroundShop", typeof(SpriteRenderer));
        bg.transform.position = new Vector3(0f, 0f, 8f);
        var bgRenderer = bg.GetComponent<SpriteRenderer>();
        bgRenderer.sortingOrder = -10;
        bgRenderer.color = new Color(0.1f, 0.1f, 0.18f, 1f);

        var shopCounter = new GameObject("ShopCounter", typeof(SpriteRenderer));
        shopCounter.transform.position = new Vector3(0f, -2f, 0f);
        shopCounter.GetComponent<SpriteRenderer>().sortingOrder = 2;

        var spawnPoint = new GameObject("SpawnPoint");
        spawnPoint.transform.position = new Vector3(-8f, -2f, 0f);

        var canvasGO = CreateCanvas();
        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        var hudGO = CreatePanel(canvasGO.transform, "HUD", true);
        var hudController = hudGO.AddComponent<HUDController>();

        var moneyContainer = CreatePanel(hudGO.transform, "MoneyContainer", true);
        var moneyRect = moneyContainer.GetComponent<RectTransform>();
        SetAnchors(moneyRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        moneyRect.pivot = new Vector2(0.5f, 1f);
        moneyRect.anchoredPosition = new Vector2(0f, -24f);
        moneyRect.sizeDelta = new Vector2(360f, 56f);

        var moneyIconGO = new GameObject("MoneyCoinIcon", typeof(RectTransform), typeof(Image));
        moneyIconGO.transform.SetParent(moneyContainer.transform, false);
        var moneyIconImage = moneyIconGO.GetComponent<Image>();
        var coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Generated/coin_icon.png");
        if (coinSprite != null)
            moneyIconImage.sprite = coinSprite;
        var moneyIconRect = moneyIconGO.GetComponent<RectTransform>();
        moneyIconRect.anchorMin = new Vector2(0f, 0.5f);
        moneyIconRect.anchorMax = new Vector2(0f, 0.5f);
        moneyIconRect.pivot = new Vector2(0f, 0.5f);
        moneyIconRect.anchoredPosition = new Vector2(18f, 0f);
        moneyIconRect.sizeDelta = new Vector2(32f, 32f);

        var moneyTextGO = CreateTMPText(moneyContainer.transform, "MoneyText", "1.3K TL", 36, GOLD, TextAlignmentOptions.Left);
        var moneyTextRect = moneyTextGO.GetComponent<RectTransform>();
        moneyTextRect.anchorMin = new Vector2(0f, 0.5f);
        moneyTextRect.anchorMax = new Vector2(1f, 0.5f);
        moneyTextRect.pivot = new Vector2(0f, 0.5f);
        moneyTextRect.offsetMin = new Vector2(64f, -22f);
        moneyTextRect.offsetMax = new Vector2(-12f, 22f);

        var levelCapsule = CreatePanel(hudGO.transform, "LevelCapsule", true);
        levelCapsule.GetComponent<Image>().color = new Color(0.051f, 0.451f, 0.467f, 0.85f);
        var levelRect = levelCapsule.GetComponent<RectTransform>();
        SetAnchors(levelRect, new Vector2(0f, 1f), new Vector2(0f, 1f));
        levelRect.pivot = new Vector2(0f, 1f);
        levelRect.anchoredPosition = new Vector2(24f, -24f);
        levelRect.sizeDelta = new Vector2(170f, 42f);
        var levelTextGO = CreateTMPText(levelCapsule.transform, "ShopLevelText", "SEVIYE 1", 20, WHITE, TextAlignmentOptions.Center);
        StretchFull(levelTextGO.GetComponent<RectTransform>());

        var upgBtnGO = CreateButton(hudGO.transform, "UpgradeButton", "YUKSELT", TEAL);
        var upgBtnRect = upgBtnGO.GetComponent<RectTransform>();
        SetAnchors(upgBtnRect, new Vector2(1f, 1f), new Vector2(1f, 1f));
        upgBtnRect.pivot = new Vector2(1f, 1f);
        upgBtnRect.anchoredPosition = new Vector2(-24f, -24f);
        upgBtnRect.sizeDelta = new Vector2(230f, 56f);

        var counterBtnGO = CreateButton(hudGO.transform, "CounterButton", "SIPARIS VER", new Color(0.91f, 0.47f, 0.17f, 0.8f));
        var counterBtnRect = counterBtnGO.GetComponent<RectTransform>();
        SetAnchors(counterBtnRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        counterBtnRect.pivot = new Vector2(0.5f, 0f);
        counterBtnRect.anchoredPosition = new Vector2(0f, 38f);
        counterBtnRect.sizeDelta = new Vector2(200f, 60f);

        var prepBarBg = CreatePanel(counterBtnGO.transform, "PrepBarBG", true);
        prepBarBg.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.25f);
        var prepBgRect = prepBarBg.GetComponent<RectTransform>();
        SetAnchors(prepBgRect, new Vector2(0.08f, 0f), new Vector2(0.92f, 0f));
        prepBgRect.pivot = new Vector2(0.5f, 0f);
        prepBgRect.anchoredPosition = new Vector2(0f, 6f);
        prepBgRect.sizeDelta = new Vector2(0f, 8f);
        var prepBarGO = CreateFillImage(prepBarBg.transform, "PrepProgressBar", GOLD);
        StretchFull(prepBarGO.GetComponent<RectTransform>());

        var customerCountGO = CreateTMPText(hudGO.transform, "CustomerCountText", "Musteri: 0", 26, WHITE, TextAlignmentOptions.Right);
        var customerCountRect = customerCountGO.GetComponent<RectTransform>();
        SetAnchors(customerCountRect, new Vector2(1f, 0f), new Vector2(1f, 0f));
        customerCountRect.pivot = new Vector2(1f, 0f);
        customerCountRect.anchoredPosition = new Vector2(-20f, 24f);
        customerCountRect.sizeDelta = new Vector2(180f, 40f);

        var uiManager = canvasGO.AddComponent<UIManager>();

        var upgradePanelGO = CreatePanel(canvasGO.transform, "UpgradePanel", false);
        StretchFull(upgradePanelGO.GetComponent<RectTransform>());
        upgradePanelGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);
        upgradePanelGO.AddComponent<CanvasGroup>();
        var upgradePanel = upgradePanelGO.AddComponent<UpgradePanel>();

        var upgTitleGO = CreateTMPText(upgradePanelGO.transform, "TitleText", "Dukkan Yukseltme", 36, WHITE, TextAlignmentOptions.Center);
        var upgTitleRect = upgTitleGO.GetComponent<RectTransform>();
        SetAnchors(upgTitleRect, new Vector2(0f, 1f), new Vector2(1f, 1f));
        upgTitleRect.pivot = new Vector2(0.5f, 1f);
        upgTitleRect.anchoredPosition = new Vector2(0f, -40f);
        upgTitleRect.sizeDelta = new Vector2(0f, 60f);
        var shopLvlLabel = CreateTMPText(upgradePanelGO.transform, "ShopLevelLabel", "Seviye 1", 28, WHITE, TextAlignmentOptions.Center);
        var shopLvlRect = shopLvlLabel.GetComponent<RectTransform>();
        SetAnchors(shopLvlRect, new Vector2(0f, 1f), new Vector2(1f, 1f));
        shopLvlRect.pivot = new Vector2(0.5f, 1f);
        shopLvlRect.anchoredPosition = new Vector2(0f, -100f);
        shopLvlRect.sizeDelta = new Vector2(0f, 40f);

        var shopUpgBtnGO = CreateButton(upgradePanelGO.transform, "ShopUpgradeButton", "Dukkani Yukselt", TEAL);
        var shopUpgRect = shopUpgBtnGO.GetComponent<RectTransform>();
        SetAnchors(shopUpgRect, new Vector2(0.1f, 1f), new Vector2(0.9f, 1f));
        shopUpgRect.pivot = new Vector2(0.5f, 1f);
        shopUpgRect.anchoredPosition = new Vector2(0f, -160f);
        shopUpgRect.sizeDelta = new Vector2(0f, 70f);
        var upgCostGO = CreateTMPText(shopUpgBtnGO.transform, "UpgradeCostText", "500 TL", 24, WHITE, TextAlignmentOptions.Center);
        StretchFull(upgCostGO.GetComponent<RectTransform>());

        var scrollGO = CreateScrollView(upgradePanelGO.transform, "WorkerScrollView");
        var scrollRect2 = scrollGO.GetComponent<RectTransform>();
        SetAnchors(scrollRect2, new Vector2(0f, 0f), new Vector2(1f, 1f));
        scrollRect2.offsetMin = new Vector2(20f, 80f);
        scrollRect2.offsetMax = new Vector2(-20f, -250f);
        var workersContainer = scrollGO.GetComponent<ScrollRect>().content;
        workersContainer.gameObject.name = "WorkersContainer";
        var vlg = workersContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 10;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(10, 10, 10, 10);
        workersContainer.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var closeBtnGO = CreateButton(upgradePanelGO.transform, "CloseButton", "X", new Color(0.76f, 0.07f, 0.12f, 1f));
        var closeBtnRect = closeBtnGO.GetComponent<RectTransform>();
        SetAnchors(closeBtnRect, new Vector2(1f, 1f), new Vector2(1f, 1f));
        closeBtnRect.pivot = new Vector2(1f, 1f);
        closeBtnRect.anchoredPosition = new Vector2(-20f, -20f);
        closeBtnRect.sizeDelta = new Vector2(60f, 60f);

        var offlinePopupGO = CreatePanel(canvasGO.transform, "OfflineEarningsPopup", false);
        StretchFull(offlinePopupGO.GetComponent<RectTransform>());
        offlinePopupGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.9f);
        var offlinePopup = offlinePopupGO.AddComponent<OfflineEarningsPopup>();
        CreateTMPText(offlinePopupGO.transform, "TitleText", "Hos Geldin!", 36, GOLD, TextAlignmentOptions.Center, new Vector2(0f, 0.6f), new Vector2(1f, 0.8f));
        var timeTextGO = CreateTMPText(offlinePopupGO.transform, "TimeText", "Dukkanin calisti!", 24, WHITE, TextAlignmentOptions.Center, new Vector2(0f, 0.5f), new Vector2(1f, 0.6f));
        var amountTextGO = CreateTMPText(offlinePopupGO.transform, "AmountText", "0 TL kazandin!", 40, GOLD, TextAlignmentOptions.Center, new Vector2(0f, 0.38f), new Vector2(1f, 0.5f));
        var watchAdBtn = CreateButton(offlinePopupGO.transform, "WatchAdButton", "2x Kazan - Reklam Izle", GOLD);
        var watchAdRect = watchAdBtn.GetComponent<RectTransform>();
        SetAnchors(watchAdRect, new Vector2(0.1f, 0.2f), new Vector2(0.9f, 0.3f));
        watchAdRect.sizeDelta = Vector2.zero;
        var collectBtn = CreateButton(offlinePopupGO.transform, "CollectButton", "Almak Icin Dokun", TEAL);
        var collectRect = collectBtn.GetComponent<RectTransform>();
        SetAnchors(collectRect, new Vector2(0.1f, 0.08f), new Vector2(0.9f, 0.18f));
        collectRect.sizeDelta = Vector2.zero;

        var toastGO = CreatePanel(canvasGO.transform, "ToastMessage", false);
        var toastRect = toastGO.GetComponent<RectTransform>();
        SetAnchors(toastRect, new Vector2(0.1f, 0.85f), new Vector2(0.9f, 0.92f));
        toastGO.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.18f, 0.95f);
        var toastCG = toastGO.AddComponent<CanvasGroup>();
        toastCG.alpha = 0f;
        var toastTextGO = CreateTMPText(toastGO.transform, "ToastText", "", 24, WHITE, TextAlignmentOptions.Center);
        StretchFull(toastTextGO.GetComponent<RectTransform>());

        var uiSO = new SerializedObject(uiManager);
        uiSO.FindProperty("moneyText").objectReferenceValue = moneyTextGO.GetComponent<TMP_Text>();
        uiSO.FindProperty("shopLevelText").objectReferenceValue = levelTextGO.GetComponent<TMP_Text>();
        uiSO.FindProperty("customerCountText").objectReferenceValue = customerCountGO.GetComponent<TMP_Text>();
        uiSO.FindProperty("moneyContainer").objectReferenceValue = moneyContainer.GetComponent<RectTransform>();
        uiSO.FindProperty("upgradePanel").objectReferenceValue = upgradePanelGO;
        uiSO.FindProperty("offlineEarningsPopup").objectReferenceValue = offlinePopupGO;
        uiSO.FindProperty("toastText").objectReferenceValue = toastTextGO.GetComponent<TMP_Text>();
        uiSO.FindProperty("toastCanvasGroup").objectReferenceValue = toastCG;
        uiSO.FindProperty("toastRect").objectReferenceValue = toastGO.GetComponent<RectTransform>();
        uiSO.ApplyModifiedProperties();

        var hudSO = new SerializedObject(hudController);
        hudSO.FindProperty("prepProgressBar").objectReferenceValue = prepBarGO.GetComponent<Image>();
        hudSO.FindProperty("counterButton").objectReferenceValue = counterBtnGO.GetComponent<Button>();
        hudSO.FindProperty("upgradeButton").objectReferenceValue = upgBtnGO.GetComponent<Button>();
        hudSO.ApplyModifiedProperties();

        var upgSO = new SerializedObject(upgradePanel);
        upgSO.FindProperty("shopUpgradeButton").objectReferenceValue = shopUpgBtnGO.GetComponent<Button>();
        upgSO.FindProperty("upgradeCostText").objectReferenceValue = upgCostGO.GetComponent<TMP_Text>();
        upgSO.FindProperty("shopLevelLabel").objectReferenceValue = shopLvlLabel.GetComponent<TMP_Text>();
        upgSO.FindProperty("workersContainer").objectReferenceValue = workersContainer;
        upgSO.ApplyModifiedProperties();

        var offSO = new SerializedObject(offlinePopup);
        offSO.FindProperty("amountText").objectReferenceValue = amountTextGO.GetComponent<TMP_Text>();
        offSO.FindProperty("timeText").objectReferenceValue = timeTextGO.GetComponent<TMP_Text>();
        offSO.FindProperty("watchAdButton").objectReferenceValue = watchAdBtn.GetComponent<Button>();
        offSO.FindProperty("collectButton").objectReferenceValue = collectBtn.GetComponent<Button>();
        offSO.ApplyModifiedProperties();

        var audioSO = new SerializedObject(audioMgr);
        audioSO.FindProperty("sfxSource").objectReferenceValue = sfxSource;
        audioSO.FindProperty("musicSource").objectReferenceValue = musicSource;
        audioSO.ApplyModifiedProperties();

        var custSO = new SerializedObject(customerMgr);
        custSO.FindProperty("spawnPoint").objectReferenceValue = spawnPoint.transform;
        custSO.FindProperty("counterPosition").objectReferenceValue = shopCounter.transform;
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Customers" });
        var prefabProp = custSO.FindProperty("customerPrefabs");
        prefabProp.arraySize = prefabGuids.Length;
        for (int i = 0; i < prefabGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
            prefabProp.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
        string[] productGuids = AssetDatabase.FindAssets("t:ProductData", new[] { "Assets/ScriptableObjects/Products" });
        var productProp = custSO.FindProperty("productDataList");
        productProp.arraySize = productGuids.Length;
        for (int i = 0; i < productGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(productGuids[i]);
            productProp.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<ProductData>(path);
        }
        custSO.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[MainSceneSetup] Main scene setup complete.");
    }

    static void SetupCamera()
    {
        Camera cam = Object.FindFirstObjectByType<Camera>();
        if (cam == null) return;
        cam.orthographic = true;
        cam.orthographicSize = 5.8f;
        cam.transform.position = new Vector3(0f, -0.3f, -10f);
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.18f, 1f);
    }

    static void ClearScene()
    {
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var go in roots)
        {
            if (go.GetComponent<Camera>() != null) continue;
            if (go.GetComponent<Light>() != null) continue;
            Object.DestroyImmediate(go);
        }
    }

    static GameObject CreateManager(string name) => new GameObject(name);

    static GameObject CreateCanvas()
    {
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();
        return canvasGO;
    }

    static GameObject CreatePanel(Transform parent, string name, bool active)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = Color.clear;
        img.raycastTarget = false;
        StretchFull(go.GetComponent<RectTransform>());
        go.SetActive(active);
        return go;
    }

    static GameObject CreateTMPText(Transform parent, string name, string text, int fontSize, Color color, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.raycastTarget = false;
        return go;
    }

    static GameObject CreateTMPText(Transform parent, string name, string text, int fontSize, Color color, TextAlignmentOptions alignment, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = CreateTMPText(parent, name, text, fontSize, color, alignment);
        var rect = go.GetComponent<RectTransform>();
        SetAnchors(rect, anchorMin, anchorMax);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        return go;
    }

    static GameObject CreateButton(Transform parent, string name, string label, Color bgColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = bgColor;
        var btn = go.AddComponent<Button>();
        var cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        btn.colors = cb;

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        StretchFull(textGO.GetComponent<RectTransform>());
        return go;
    }

    static GameObject CreateFillImage(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Horizontal;
        img.fillAmount = 0f;
        img.raycastTarget = false;
        return go;
    }

    static GameObject CreateScrollView(Transform parent, string name)
    {
        var scrollGO = new GameObject(name);
        scrollGO.transform.SetParent(parent, false);
        scrollGO.AddComponent<Image>().color = Color.clear;
        var scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollGO.transform, false);
        viewport.AddComponent<Image>().color = Color.clear;
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        var vpRect = viewport.GetComponent<RectTransform>();
        StretchFull(vpRect);

        var content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        var contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0f, 300f);
        scrollRect.viewport = vpRect;
        scrollRect.content = contentRect;
        return scrollGO;
    }

    static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
    }
}
