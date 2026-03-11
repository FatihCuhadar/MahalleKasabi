using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanel : MonoBehaviour
{
    [SerializeField] private Button shopUpgradeButton;
    [SerializeField] private TMP_Text upgradeCostText;
    [SerializeField] private TMP_Text shopLevelLabel;
    [SerializeField] private Transform workersContainer;
    [SerializeField] private GameObject workerCardPrefab;

    private readonly List<string> upgradeKeys = new List<string>
    {
        "prep_speed",
        "extra_station",
        "quality",
        "capacity",
        "menu",
        "vitrin"
    };

    private readonly Dictionary<string, string> labels = new Dictionary<string, string>
    {
        { "prep_speed", "Hazirlik Hizi" },
        { "extra_station", "Ekstra Istasyon" },
        { "quality", "Urun Kalitesi" },
        { "capacity", "Musteri Kapasitesi" },
        { "menu", "Menu Genisletme" },
        { "vitrin", "Vitrin" }
    };

    void OnEnable()
    {
        RefreshPanel();
    }

    void Start()
    {
        if (shopUpgradeButton != null)
            shopUpgradeButton.onClick.AddListener(OnPrimaryUpgradeClicked);
    }

    void OnDestroy()
    {
        if (shopUpgradeButton != null)
            shopUpgradeButton.onClick.RemoveListener(OnPrimaryUpgradeClicked);
    }

    public void RefreshPanel()
    {
        if (ShopManager.Instance == null || PlayerData.Instance == null) return;
        if (shopLevelLabel != null)
            shopLevelLabel.text = $"Dukkan Seviyesi {PlayerData.Instance.shopLevel}";

        UpdatePrimaryCard();
        BuildCards();
    }

    private void UpdatePrimaryCard()
    {
        if (upgradeCostText == null || shopUpgradeButton == null || ShopManager.Instance == null) return;
        string key = "prep_speed";
        bool maxed = ShopManager.Instance.IsMaxed(key);
        float cost = ShopManager.Instance.GetUpgradeCost(key);
        int level = ShopManager.Instance.GetLevel(key);
        if (maxed)
        {
            upgradeCostText.text = $"{labels[key]} - MAKS";
            shopUpgradeButton.interactable = false;
            return;
        }

        bool canAfford = PlayerData.Instance != null && PlayerData.Instance.CanAfford(cost);
        shopUpgradeButton.interactable = canAfford;
        upgradeCostText.text = $"{labels[key]} L{level + 1} - {UIManager.Instance.FormatMoney(cost)}";
        ColorBlock cb = shopUpgradeButton.colors;
        cb.normalColor = canAfford ? new Color(0.18f, 0.42f, 0.31f, 1f) : new Color(0.45f, 0.45f, 0.45f, 1f);
        shopUpgradeButton.colors = cb;
    }

    private void OnPrimaryUpgradeClicked()
    {
        TryUpgradeAndNotify("prep_speed");
    }

    private void BuildCards()
    {
        if (workersContainer == null || workerCardPrefab == null || ShopManager.Instance == null) return;
        for (int i = workersContainer.childCount - 1; i >= 0; i--)
            Destroy(workersContainer.GetChild(i).gameObject);

        for (int i = 0; i < upgradeKeys.Count; i++)
            CreateUpgradeCard(upgradeKeys[i]);

        CreateWorkerCard("cirak", "Cirak (2sn kolay siparis)");
        CreateWorkerCard("yardimci", "Kasap Yardimcisi");
        CreateWorkerCard("usta_kasap", "Usta Kasap (VIP dahil)");
    }

    private void CreateUpgradeCard(string key)
    {
        GameObject card = Instantiate(workerCardPrefab, workersContainer);
        TMP_Text nameText = FindChildText(card, "NameText");
        TMP_Text levelText = FindChildText(card, "LevelText");
        TMP_Text bonusText = FindChildText(card, "BonusText");
        TMP_Text buttonText = FindChildText(card, "ButtonText");
        Button actionButton = card.GetComponentInChildren<Button>();

        int level = ShopManager.Instance.GetLevel(key);
        bool maxed = ShopManager.Instance.IsMaxed(key);
        float cost = ShopManager.Instance.GetUpgradeCost(key);
        bool canAfford = PlayerData.Instance != null && PlayerData.Instance.CanAfford(cost);

        if (nameText != null) nameText.text = labels.ContainsKey(key) ? labels[key] : key;
        if (levelText != null) levelText.text = $"Seviye {level}";
        if (bonusText != null) bonusText.text = BuildUpgradeDescription(key, level);
        if (buttonText != null)
            buttonText.text = maxed ? "MAKS" : $"Yukselt - {UIManager.Instance.FormatMoney(cost)}";

        if (actionButton != null)
        {
            actionButton.interactable = !maxed && canAfford;
            string captured = key;
            actionButton.onClick.AddListener(() => TryUpgradeAndNotify(captured));
        }
    }

    private void CreateWorkerCard(string workerType, string title)
    {
        GameObject card = Instantiate(workerCardPrefab, workersContainer);
        TMP_Text nameText = FindChildText(card, "NameText");
        TMP_Text levelText = FindChildText(card, "LevelText");
        TMP_Text bonusText = FindChildText(card, "BonusText");
        TMP_Text buttonText = FindChildText(card, "ButtonText");
        Button actionButton = card.GetComponentInChildren<Button>();

        bool hired = ShopManager.Instance.IsWorkerHired(workerType);
        float hireCost = workerType == "cirak" ? 5000f : (workerType == "yardimci" ? 15000f : 40000f);
        bool canAfford = PlayerData.Instance != null && PlayerData.Instance.CanAfford(hireCost);

        if (nameText != null) nameText.text = title;
        if (levelText != null) levelText.text = hired ? "Durum: Aktif" : "Durum: Pasif";
        if (bonusText != null) bonusText.text = $"Maas: {ShopManager.Instance.GetWorkerSalaryPerMinute(workerType):F0} TL/dk";
        if (buttonText != null)
            buttonText.text = hired ? "Kiralandi" : $"Kirala - {UIManager.Instance.FormatMoney(hireCost)}";

        if (actionButton != null)
        {
            actionButton.interactable = !hired && canAfford;
            string captured = workerType;
            actionButton.onClick.AddListener(() =>
            {
                if (ShopManager.Instance.TryHireWorker(captured))
                {
                    UIManager.Instance?.ShowToast($"{title} kiralandi!", new Color(0.2f, 0.8f, 0.35f), 2f);
                    RefreshPanel();
                }
            });
        }
    }

    private void TryUpgradeAndNotify(string key)
    {
        if (ShopManager.Instance == null) return;
        if (ShopManager.Instance.TryUpgrade(key))
        {
            UIManager.Instance?.ShowToast($"{labels[key]} yukseltildi", new Color(0.2f, 0.8f, 0.35f), 2f);
            RefreshPanel();
        }
        else
        {
            UIManager.Instance?.ShowToast("Yeterli para yok veya maksimum seviye", new Color(0.85f, 0.2f, 0.2f), 2f);
        }
    }

    private string BuildUpgradeDescription(string key, int level)
    {
        if (key == "prep_speed") return $"Hazirlama hizi %{level * 20}";
        if (key == "extra_station") return $"Eszamanli istasyon: {1 + level}";
        if (key == "quality") return $"Odeme bonusu %{level * 15}";
        if (key == "capacity") return $"Maks musteri: {3 + level}";
        if (key == "menu") return $"Menu seviyesi: {1 + level}";
        if (key == "vitrin") return $"Spawn hizi x{(1f + level * 0.25f):F2}";
        return string.Empty;
    }

    private static TMP_Text FindChildText(GameObject parent, string childName)
    {
        Transform t = parent.transform.Find(childName);
        return t != null ? t.GetComponent<TMP_Text>() : null;
    }
}
