using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// Creates Boot (splash), MainMenu, and LoadingScreen overlay scenes,
/// and updates Build Settings to Boot(0) → MainMenu(1) → Main(2).
/// </summary>
public static class SceneSetupAll
{
    // Palette
    static readonly Color BG_DARK = new Color(0.1f, 0.1f, 0.18f, 1f);       // #1A1A2E
    static readonly Color TEAL = new Color(0.051f, 0.451f, 0.467f, 1f);      // #0D7377
    static readonly Color ORANGE = new Color(0.91f, 0.46f, 0.17f, 1f);       // #E8762B
    static readonly Color GOLD = new Color(0.957f, 0.643f, 0.133f, 1f);      // #F4A422
    static readonly Color WHITE = Color.white;
    static readonly Color GRAY = new Color(0.678f, 0.710f, 0.741f, 1f);      // #ADB5BD

    [MenuItem("MahalleKasabi/Setup All Scenes (Boot + MainMenu + Loading)")]
    public static void SetupAll()
    {
        CreateBootScene();
        CreateMainMenuScene();
        CreateLoadingOverlayInBoot(); // SceneLoader lives in Boot so it persists
        UpdateBuildSettings();
        RuntimeConfigSetup.Setup();

        Debug.Log("[SceneSetupAll] All scenes created. Build order: Boot(0) → MainMenu(1) → Main(2)");
    }

    // ═══════════════════════════════════════════════════════════════
    // BOOT SCENE — Splash Screen (2.5s)
    // ═══════════════════════════════════════════════════════════════
    static void CreateBootScene()
    {
        string path = "Assets/Scenes/Boot.unity";

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera — solid black
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor = Color.black;
        cam.clearFlags = CameraClearFlags.SolidColor;
        camGO.tag = "MainCamera";

        // Canvas
        var canvasGO = CreateCanvas("SplashCanvas");
        var canvasRect = canvasGO.GetComponent<RectTransform>();

        // CanvasGroup for fade
        var splashGroup = canvasGO.AddComponent<CanvasGroup>();
        splashGroup.alpha = 0f;

        // Black background image (ensures full black)
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = Color.black;
        bgImg.raycastTarget = false;
        StretchFull(bgGO.GetComponent<RectTransform>());

        // Title: "Mahalle Kasabi"
        var titleGO = CreateTMPText(canvasGO.transform, "TitleText", "Mahalle Kasabi", 72, WHITE,
            TextAlignmentOptions.Center, FontStyles.Bold);
        var titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.4f);
        titleRect.anchorMax = new Vector2(1, 0.6f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = Vector2.zero;

        // Subtitle / icon placeholder
        var subGO = CreateTMPText(canvasGO.transform, "IconText", "Et & Kasap", 28, GRAY,
            TextAlignmentOptions.Center, FontStyles.Italic);
        var subRect = subGO.GetComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0, 0.3f);
        subRect.anchorMax = new Vector2(1, 0.4f);
        subRect.anchoredPosition = Vector2.zero;
        subRect.sizeDelta = Vector2.zero;

        // BootLoader
        var bootGO = new GameObject("BootLoader");
        var bootLoader = bootGO.AddComponent<BootLoader>();
        var bootSO = new SerializedObject(bootLoader);
        bootSO.FindProperty("splashGroup").objectReferenceValue = splashGroup;
        bootSO.FindProperty("fadeInTime").floatValue = 0.8f;
        bootSO.FindProperty("holdTime").floatValue = 1.0f;
        bootSO.FindProperty("fadeOutTime").floatValue = 0.7f;
        bootSO.ApplyModifiedProperties();

        // ─── SceneLoader overlay (DontDestroyOnLoad, persists across scenes) ───
        CreateSceneLoaderOverlay(scene);

        EditorSceneManager.SaveScene(scene, path);
        AssetDatabase.Refresh();
        Debug.Log("[SceneSetupAll] Boot scene (splash) created at " + path);
    }

    // ═══════════════════════════════════════════════════════════════
    // SCENE LOADER OVERLAY — lives in Boot scene, DontDestroyOnLoad
    // ═══════════════════════════════════════════════════════════════
    static void CreateSceneLoaderOverlay(UnityEngine.SceneManagement.Scene scene)
    {
        // Root GO
        var loaderRootGO = new GameObject("_SceneLoader");

        // Canvas (overlay, high sort order)
        var loaderCanvasGO = new GameObject("LoadingCanvas");
        loaderCanvasGO.transform.SetParent(loaderRootGO.transform, false);

        var canvas = loaderCanvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        var scaler = loaderCanvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        loaderCanvasGO.AddComponent<GraphicRaycaster>();

        // CanvasGroup for the whole overlay
        var overlayGroup = loaderCanvasGO.AddComponent<CanvasGroup>();
        overlayGroup.alpha = 0f;
        overlayGroup.blocksRaycasts = false;

        // Dark background
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(loaderCanvasGO.transform, false);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.06f, 0.06f, 0.12f, 1f);
        bgImg.raycastTarget = true;
        StretchFull(bgGO.GetComponent<RectTransform>());

        // "Yukleniyor..." text
        var loadTextGO = CreateTMPText(loaderCanvasGO.transform, "LoadingText", "Yukleniyor...", 32, WHITE,
            TextAlignmentOptions.Center, FontStyles.Normal);
        var loadTextRect = loadTextGO.GetComponent<RectTransform>();
        loadTextRect.anchorMin = new Vector2(0, 0.5f);
        loadTextRect.anchorMax = new Vector2(1, 0.6f);
        loadTextRect.anchoredPosition = Vector2.zero;
        loadTextRect.sizeDelta = Vector2.zero;

        // Progress bar background
        var barBgGO = new GameObject("ProgressBarBG");
        barBgGO.transform.SetParent(loaderCanvasGO.transform, false);
        var barBgImg = barBgGO.AddComponent<Image>();
        barBgImg.color = new Color(0.2f, 0.2f, 0.3f, 1f);
        barBgImg.raycastTarget = false;
        var barBgRect = barBgGO.GetComponent<RectTransform>();
        barBgRect.anchorMin = new Vector2(0.15f, 0.42f);
        barBgRect.anchorMax = new Vector2(0.85f, 0.45f);
        barBgRect.anchoredPosition = Vector2.zero;
        barBgRect.sizeDelta = Vector2.zero;

        // Progress bar fill
        var barFillGO = new GameObject("ProgressBarFill");
        barFillGO.transform.SetParent(barBgGO.transform, false);
        var fillImg = barFillGO.AddComponent<Image>();
        fillImg.color = TEAL;
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 0f;
        fillImg.raycastTarget = false;
        StretchFull(barFillGO.GetComponent<RectTransform>());

        // SceneLoader component on root
        var sceneLoader = loaderRootGO.AddComponent<SceneLoader>();
        var slSO = new SerializedObject(sceneLoader);
        slSO.FindProperty("overlayGroup").objectReferenceValue = overlayGroup;
        slSO.FindProperty("progressBar").objectReferenceValue = fillImg;
        slSO.FindProperty("loadingText").objectReferenceValue = loadTextGO.GetComponent<TMP_Text>();
        slSO.ApplyModifiedProperties();
    }

    // Kept for backward compat — redirects to Boot scene creation
    static void CreateLoadingOverlayInBoot()
    {
        // Already handled inside CreateBootScene → CreateSceneLoaderOverlay
    }

    // ═══════════════════════════════════════════════════════════════
    // MAIN MENU SCENE
    // ═══════════════════════════════════════════════════════════════
    static void CreateMainMenuScene()
    {
        string path = "Assets/Scenes/MainMenu.unity";

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor = BG_DARK;
        cam.clearFlags = CameraClearFlags.SolidColor;
        camGO.tag = "MainCamera";

        // EventSystem (New Input System)
        var eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // Canvas
        var canvasGO = CreateCanvas("Canvas");

        // Dark background panel
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = BG_DARK;
        bgImg.raycastTarget = false;
        StretchFull(bgGO.GetComponent<RectTransform>());

        // ─── Title: "Mahalle Kasabi" ───
        var titleGO = CreateTMPText(canvasGO.transform, "TitleText", "Mahalle Kasabi", 60, TEAL,
            TextAlignmentOptions.Center, FontStyles.Bold);
        var titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.72f);
        titleRect.anchorMax = new Vector2(1, 0.85f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = Vector2.zero;

        // ─── Subtitle: "Kasabin Basari Hikayesi" ───
        var subtitleGO = CreateTMPText(canvasGO.transform, "SubtitleText",
            "Kasabin Basari Hikayesi", 24, GRAY,
            TextAlignmentOptions.Center, FontStyles.Italic);
        var subRect = subtitleGO.GetComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0, 0.66f);
        subRect.anchorMax = new Vector2(1, 0.72f);
        subRect.anchoredPosition = Vector2.zero;
        subRect.sizeDelta = Vector2.zero;

        // ─── PLAY Button ───
        var playBtnGO = CreateButton(canvasGO.transform, "PlayButton", "OYNA", ORANGE, 36);
        var playRect = playBtnGO.GetComponent<RectTransform>();
        playRect.anchorMin = new Vector2(0.5f, 0.5f);
        playRect.anchorMax = new Vector2(0.5f, 0.5f);
        playRect.pivot = new Vector2(0.5f, 0.5f);
        playRect.anchoredPosition = Vector2.zero;
        playRect.sizeDelta = new Vector2(300, 80);

        // ─── Settings Button ───
        var settingsBtnGO = CreateButton(canvasGO.transform, "SettingsButton", "Ayarlar", TEAL, 22);
        var settingsRect = settingsBtnGO.GetComponent<RectTransform>();
        settingsRect.anchorMin = new Vector2(0.5f, 0.5f);
        settingsRect.anchorMax = new Vector2(0.5f, 0.5f);
        settingsRect.pivot = new Vector2(0.5f, 0.5f);
        settingsRect.anchoredPosition = new Vector2(-100, -80);
        settingsRect.sizeDelta = new Vector2(170, 50);

        // ─── High Score Button ───
        var highScoreBtnGO = CreateButton(canvasGO.transform, "HighScoreButton", "En Yuksek Skor", TEAL, 22);
        var hsRect = highScoreBtnGO.GetComponent<RectTransform>();
        hsRect.anchorMin = new Vector2(0.5f, 0.5f);
        hsRect.anchorMax = new Vector2(0.5f, 0.5f);
        hsRect.pivot = new Vector2(0.5f, 0.5f);
        hsRect.anchoredPosition = new Vector2(100, -80);
        hsRect.sizeDelta = new Vector2(170, 50);

        // ─── Version Text ───
        var versionGO = CreateTMPText(canvasGO.transform, "VersionText", "v0.1.0", 16, GRAY,
            TextAlignmentOptions.BottomRight, FontStyles.Normal);
        var verRect = versionGO.GetComponent<RectTransform>();
        verRect.anchorMin = new Vector2(0.8f, 0f);
        verRect.anchorMax = new Vector2(1f, 0.05f);
        verRect.anchoredPosition = new Vector2(-10, 5);
        verRect.sizeDelta = Vector2.zero;

        // ─── MainMenuController ───
        var menuController = canvasGO.AddComponent<MainMenuController>();
        var mcSO = new SerializedObject(menuController);
        mcSO.FindProperty("playButton").objectReferenceValue = playBtnGO.GetComponent<Button>();
        mcSO.FindProperty("settingsButton").objectReferenceValue = settingsBtnGO.GetComponent<Button>();
        mcSO.FindProperty("highScoreButton").objectReferenceValue = highScoreBtnGO.GetComponent<Button>();
        mcSO.ApplyModifiedProperties();

        // Save
        EditorSceneManager.SaveScene(scene, path);
        AssetDatabase.Refresh();
        Debug.Log("[SceneSetupAll] MainMenu scene created at " + path);
    }

    // ═══════════════════════════════════════════════════════════════
    // BUILD SETTINGS
    // ═══════════════════════════════════════════════════════════════
    static void UpdateBuildSettings()
    {
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/Boot.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true),
        };

        Debug.Log("[SceneSetupAll] Build Settings: Boot(0) → MainMenu(1) → Main(2)");
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════

    static GameObject CreateCanvas(string name)
    {
        var canvasGO = new GameObject(name);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();
        return canvasGO;
    }

    static GameObject CreateTMPText(Transform parent, string name, string text,
        int fontSize, Color color, TextAlignmentOptions alignment, FontStyles style)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.fontStyle = style;
        tmp.raycastTarget = false;
        return go;
    }

    static GameObject CreateButton(Transform parent, string name, string label, Color bgColor, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = bgColor;

        var btn = go.AddComponent<Button>();
        var cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        btn.colors = cb;

        // Button text
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        StretchFull(textGO.GetComponent<RectTransform>());

        return go;
    }

    static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
    }
}
