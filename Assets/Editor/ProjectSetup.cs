using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class ProjectSetup
{
    [MenuItem("MahalleKasabi/Setup Boot Scene and Build Settings")]
    public static void SetupAll()
    {
        CreateBootScene();
        SetBuildSettings();
        SetPlayerSettings();
        Debug.Log("[ProjectSetup] All settings configured successfully!");
    }

    static void CreateBootScene()
    {
        string bootPath = "Assets/Scenes/Boot.unity";

        // Check if Boot scene already exists
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(bootPath) != null)
        {
            Debug.Log("[ProjectSetup] Boot scene already exists, skipping creation.");
            return;
        }

        // Create new empty scene
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Add Camera
        GameObject camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.18f, 1f); // dark background
        camGO.tag = "MainCamera";

        // Add Canvas
        GameObject canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Add progress bar background
        GameObject barBG = new GameObject("ProgressBarBG");
        barBG.transform.SetParent(canvasGO.transform, false);
        var bgImage = barBG.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.3f, 1f);
        var bgRect = barBG.GetComponent<RectTransform>();
        bgRect.anchoredPosition = new Vector2(0, -100);
        bgRect.sizeDelta = new Vector2(400, 30);

        // Add progress bar fill
        GameObject barFill = new GameObject("ProgressBarFill");
        barFill.transform.SetParent(barBG.transform, false);
        var fillImage = barFill.AddComponent<UnityEngine.UI.Image>();
        fillImage.color = new Color(0.05f, 0.45f, 0.47f, 1f); // teal
        fillImage.type = UnityEngine.UI.Image.Type.Filled;
        fillImage.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        fillImage.fillAmount = 0f;
        var fillRect = barFill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;

        // Add BootLoader script
        GameObject bootGO = new GameObject("BootLoader");
        var bootLoader = bootGO.AddComponent<BootLoader>();
        // Set progressBar via SerializedObject
        var so = new SerializedObject(bootLoader);
        so.FindProperty("progressBar").objectReferenceValue = fillImage;
        so.ApplyModifiedProperties();

        // Save the scene
        EditorSceneManager.SaveScene(newScene, bootPath);
        AssetDatabase.Refresh();

        Debug.Log("[ProjectSetup] Boot scene created at " + bootPath);
    }

    static void SetBuildSettings()
    {
        // Set build scenes: Boot (0), MainMenu (1), Main (2)
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/Boot.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true),
        };

        Debug.Log("[ProjectSetup] Build settings updated: Boot (0), MainMenu (1), Main (2)");
    }

    static void SetPlayerSettings()
    {
        // Company and product name
        PlayerSettings.companyName = "FatihDev";
        PlayerSettings.productName = "Mahalle Kasabi";
        PlayerSettings.bundleVersion = "0.1.0";

        // Android settings
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.fatihdev.mahallekasabi");
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

        // IL2CPP
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);

        // ARM64 + ARMv7 (flag combination = 3)
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;

        Debug.Log("[ProjectSetup] Player settings configured: Android, IL2CPP, ARM64+ARMv7, API 26+");
    }
}
