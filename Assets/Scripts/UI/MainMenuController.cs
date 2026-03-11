using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main menu: Play, Settings, High Score buttons.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button highScoreButton;

    void Start()
    {
        if (playButton != null)
            playButton.onClick.AddListener(OnPlay);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettings);
        if (highScoreButton != null)
            highScoreButton.onClick.AddListener(OnHighScore);
    }

    void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlay);
        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OnSettings);
        if (highScoreButton != null)
            highScoreButton.onClick.RemoveListener(OnHighScore);
    }

    private void OnPlay()
    {
        AudioManager.Instance?.Play("button_click");
        SceneLoader.Load("Main");
    }

    private void OnSettings()
    {
        AudioManager.Instance?.Play("button_click");
        // TODO: settings panel
        Debug.Log("[MainMenu] Settings tapped");
    }

    private void OnHighScore()
    {
        AudioManager.Instance?.Play("button_click");
        float best = PlayerPrefs.GetFloat("HighScore", 0f);
        Debug.Log($"[MainMenu] High Score: {best:F0} TL");
        // TODO: high score panel
    }
}
