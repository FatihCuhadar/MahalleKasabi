using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradePanel : MonoBehaviour
{
    [Header("Shop Upgrade")]
    [SerializeField] private Button shopUpgradeButton;
    [SerializeField] private TMP_Text upgradeCostText;
    [SerializeField] private TMP_Text shopLevelLabel;

    [Header("Workers")]
    [SerializeField] private Transform workersContainer;
    [SerializeField] private GameObject workerCardPrefab;
    [SerializeField] private WorkerConfigData[] workerConfigs;

    // Colors
    private readonly Color activeColor = new Color(0.18f, 0.42f, 0.31f, 1f);   // #2D6A4F
    private readonly Color disabledColor = new Color(0.42f, 0.46f, 0.49f, 1f); // #6C757D

    void OnEnable()
    {
        RefreshPanel();
    }

    void Start()
    {
        if (shopUpgradeButton != null)
            shopUpgradeButton.onClick.AddListener(OnShopUpgradeClicked);
    }

    void OnDestroy()
    {
        if (shopUpgradeButton != null)
            shopUpgradeButton.onClick.RemoveListener(OnShopUpgradeClicked);
    }

    public void RefreshPanel()
    {
        RefreshShopUpgrade();
        SpawnWorkerCards();
    }

    // ───────────────────────── Shop Upgrade Section ─────────────────────────

    private void RefreshShopUpgrade()
    {
        if (PlayerData.Instance == null) return;

        int currentLevel = PlayerData.Instance.shopLevel;
        bool isMaxLevel = currentLevel >= 10;

        if (shopLevelLabel != null)
            shopLevelLabel.text = $"Seviye {currentLevel}";

        if (isMaxLevel)
        {
            if (upgradeCostText != null)
                upgradeCostText.text = "MAKSIMUM";
            if (shopUpgradeButton != null)
                shopUpgradeButton.interactable = false;
            return;
        }

        float cost = ShopManager.Instance != null ? ShopManager.Instance.GetUpgradeCost() : 0f;
        string costFormatted = UIManager.Instance != null
            ? UIManager.Instance.FormatMoney(cost)
            : $"{cost:F0} TL";

        if (upgradeCostText != null)
            upgradeCostText.text = $"Dukkani Yukselt \u2014 {costFormatted}";

        bool canAfford = PlayerData.Instance.CanAfford(cost);

        if (shopUpgradeButton != null)
        {
            shopUpgradeButton.interactable = canAfford;
            ColorBlock cb = shopUpgradeButton.colors;
            cb.normalColor = canAfford ? activeColor : disabledColor;
            shopUpgradeButton.colors = cb;
        }
    }

    private void OnShopUpgradeClicked()
    {
        AudioManager.Instance?.Play("button_click");

        if (ShopManager.Instance == null) return;

        if (ShopManager.Instance.TryUpgradeShop())
        {
            int newLevel = PlayerData.Instance != null ? PlayerData.Instance.shopLevel : 0;
            UIManager.Instance?.ShowToast(
                $"Dukkan Seviye {newLevel}'e yukseltildi!",
                new Color(0.18f, 0.42f, 0.31f, 1f), // green
                2f);
            RefreshPanel();
        }
        else
        {
            UIManager.Instance?.ShowToast("Yeterli paran yok!", new Color(0.76f, 0.07f, 0.12f, 1f), 2f);
        }
    }

    // ───────────────────────── Worker Cards ─────────────────────────

    private void SpawnWorkerCards()
    {
        if (workersContainer == null || workerCardPrefab == null) return;

        // Clear existing cards
        for (int i = workersContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(workersContainer.GetChild(i).gameObject);
        }

        if (workerConfigs == null) return;

        foreach (var config in workerConfigs)
        {
            GameObject card = Instantiate(workerCardPrefab, workersContainer);
            SetupWorkerCard(card, config);
        }
    }

    private void SetupWorkerCard(GameObject card, WorkerConfigData config)
    {
        if (config == null) return;

        // Find UI components in card
        TMP_Text nameText = FindChildText(card, "NameText");
        TMP_Text levelText = FindChildText(card, "LevelText");
        TMP_Text bonusText = FindChildText(card, "BonusText");
        TMP_Text buttonText = FindChildText(card, "ButtonText");
        Button actionButton = card.GetComponentInChildren<Button>();
        Image iconImage = FindChildImage(card, "Icon");

        bool isHired = ShopManager.Instance != null && ShopManager.Instance.IsWorkerHired(config.workerType);
        int shopLevel = PlayerData.Instance != null ? PlayerData.Instance.shopLevel : 1;
        bool isUnlockable = shopLevel >= config.unlockAtShopLevel;

        // Name
        if (nameText != null)
            nameText.text = config.displayName;

        // Icon
        if (iconImage != null && config.workerIcon != null)
            iconImage.sprite = config.workerIcon;

        if (!isUnlockable)
        {
            // Locked state
            if (levelText != null)
                levelText.text = $"Seviye {config.unlockAtShopLevel}'te acilir";
            if (bonusText != null)
                bonusText.text = "";
            if (buttonText != null)
                buttonText.text = "Kilitli";
            if (actionButton != null)
                actionButton.interactable = false;

            SetCardAlpha(card, 0.5f);
        }
        else if (!isHired)
        {
            // Available to hire
            if (levelText != null)
                levelText.text = "";
            if (bonusText != null)
                bonusText.text = config.bonusDescription;

            string costStr = UIManager.Instance != null
                ? UIManager.Instance.FormatMoney(config.baseCost)
                : $"{config.baseCost:F0} TL";

            if (buttonText != null)
                buttonText.text = $"Ise Al \u2014 {costStr}";

            bool canAfford = PlayerData.Instance != null && PlayerData.Instance.CanAfford(config.baseCost);
            if (actionButton != null)
            {
                actionButton.interactable = canAfford;
                string type = config.workerType;
                actionButton.onClick.AddListener(() => OnHireWorker(type));
            }

            SetCardAlpha(card, 1f);
        }
        else
        {
            // Hired — show level and upgrade option
            int level = ShopManager.Instance != null ? ShopManager.Instance.GetWorkerLevel(config.workerType) : 1;
            int maxLevel = config.bonusValues != null ? config.bonusValues.Length : 5;
            bool isMaxLevel = level >= maxLevel;

            if (levelText != null)
            {
                string stars = new string('\u2605', level) + new string('\u2606', maxLevel - level);
                levelText.text = $"Seviye {level}/{maxLevel} {stars}";
            }

            if (bonusText != null)
                bonusText.text = config.bonusDescription;

            if (isMaxLevel)
            {
                if (buttonText != null)
                    buttonText.text = "MAKSIMUM";
                if (actionButton != null)
                    actionButton.interactable = false;
            }
            else
            {
                float upgCost = ShopManager.Instance != null
                    ? ShopManager.Instance.GetWorkerUpgradeCost(config.workerType)
                    : 0f;
                string costStr = UIManager.Instance != null
                    ? UIManager.Instance.FormatMoney(upgCost)
                    : $"{upgCost:F0} TL";

                if (buttonText != null)
                    buttonText.text = $"Yukselt \u2014 {costStr}";

                bool canAfford = PlayerData.Instance != null && PlayerData.Instance.CanAfford(upgCost);
                if (actionButton != null)
                {
                    actionButton.interactable = canAfford;
                    string type = config.workerType;
                    actionButton.onClick.AddListener(() => OnUpgradeWorker(type));
                }
            }

            SetCardAlpha(card, 1f);
        }
    }

    private void OnHireWorker(string workerType)
    {
        AudioManager.Instance?.Play("button_click");
        if (ShopManager.Instance == null) return;

        if (ShopManager.Instance.TryHireWorker(workerType))
        {
            UIManager.Instance?.ShowToast("Calisan ise alindi!", new Color(0.18f, 0.42f, 0.31f, 1f), 2f);
            RefreshPanel();
        }
    }

    private void OnUpgradeWorker(string workerType)
    {
        AudioManager.Instance?.Play("button_click");
        if (ShopManager.Instance == null) return;

        if (ShopManager.Instance.TryUpgradeWorker(workerType))
        {
            UIManager.Instance?.ShowToast("Calisan yukseltildi!", new Color(0.18f, 0.42f, 0.31f, 1f), 2f);
            RefreshPanel();
        }
    }

    // ───────────────────────── Helpers ─────────────────────────

    private static void SetCardAlpha(GameObject card, float alpha)
    {
        CanvasGroup cg = card.GetComponent<CanvasGroup>();
        if (cg == null) cg = card.AddComponent<CanvasGroup>();
        cg.alpha = alpha;
    }

    private static TMP_Text FindChildText(GameObject parent, string childName)
    {
        Transform t = parent.transform.Find(childName);
        return t != null ? t.GetComponent<TMP_Text>() : null;
    }

    private static Image FindChildImage(GameObject parent, string childName)
    {
        Transform t = parent.transform.Find(childName);
        return t != null ? t.GetComponent<Image>() : null;
    }
}
