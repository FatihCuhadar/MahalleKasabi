using UnityEngine;
using UnityEditor;

public static class SOGenerator
{
    [MenuItem("MahalleKasabi/Generate All ScriptableObjects")]
    public static void GenerateAll()
    {
        GenerateProducts();
        GenerateWorkers();
        GenerateShopUpgrades();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[SOGenerator] All ScriptableObjects created successfully!");
    }

    static void GenerateProducts()
    {
        string path = "Assets/ScriptableObjects/Products";

        CreateProduct(path, "DanaKiyma", "Dana Kiyma", 10f, 2f, 1, "Taze dana kiyma");
        CreateProduct(path, "Kofte", "Kofte", 15f, 3f, 1, "El yapimi kofte");
        CreateProduct(path, "Pirzola", "Pirzola", 20f, 4f, 2, "Kuzu pirzola");
        CreateProduct(path, "Kaburga", "Kaburga", 25f, 5f, 2, "Dana kaburga");
        CreateProduct(path, "KuzuBut", "Kuzu But", 40f, 7f, 3, "Butun kuzu but");
        CreateProduct(path, "Sakatat", "Sakatat", 12f, 3f, 4, "Taze sakatat cesitleri");
        CreateProduct(path, "Sucuk", "Sucuk", 30f, 4f, 5, "Ev yapimi sucuk");
        CreateProduct(path, "Pastirma", "Pastirma", 35f, 5f, 5, "Geleneksel pastirma");
        CreateProduct(path, "MarineliEt", "Marineli Et", 55f, 8f, 6, "Ozel marine soslu et");
        CreateProduct(path, "OzelKesim", "Ozel Kesim", 80f, 12f, 7, "Premium ozel kesim paketi");

        Debug.Log("[SOGenerator] 10 products created.");
    }

    static void CreateProduct(string folder, string fileName, string productName,
        float price, float prepTime, int unlockLevel, string desc)
    {
        ProductData so = ScriptableObject.CreateInstance<ProductData>();
        so.productName = productName;
        so.basePrice = price;
        so.preparationTime = prepTime;
        so.unlockLevel = unlockLevel;
        so.description = desc;
        AssetDatabase.CreateAsset(so, $"{folder}/{fileName}.asset");
    }

    static void GenerateWorkers()
    {
        string path = "Assets/ScriptableObjects/Workers";

        CreateWorker(path, "Kasiyer", "kasiyer", "Kasiyer", 300f,
            new float[] { 800, 2000, 5000, 12000 },
            new float[] { 1.2f, 1.5f, 2.0f, 2.5f, 3.0f },
            "Siparis alma hizini artirir", 3);

        CreateWorker(path, "KasapYardimcisi", "kasap_yardimcisi", "Kasap Yardimcisi", 500f,
            new float[] { 1200, 3000, 7000, 15000 },
            new float[] { 1.3f, 1.6f, 2.0f, 2.5f, 3.0f },
            "Hazirlama hizini artirir", 1);

        CreateWorker(path, "Teslimatci", "teslimatci", "Teslimatci", 800f,
            new float[] { 2000, 5000, 12000, 25000 },
            new float[] { 0.10f, 0.20f, 0.35f, 0.50f, 0.75f },
            "Toplu siparis bonusu verir", 5);

        CreateWorker(path, "Muhasebeci", "muhasebeci", "Muhasebeci", 1000f,
            new float[] { 3000, 7000, 15000, 30000 },
            new float[] { 1.3f, 1.6f, 2.0f, 2.5f, 3.0f },
            "Offline kazanc carpanini artirir", 7);

        Debug.Log("[SOGenerator] 4 workers created.");
    }

    static void CreateWorker(string folder, string fileName, string workerType,
        string displayName, float baseCost, float[] upgradeCosts, float[] bonusValues,
        string bonusDesc, int unlockLevel)
    {
        WorkerConfigData so = ScriptableObject.CreateInstance<WorkerConfigData>();
        so.workerType = workerType;
        so.displayName = displayName;
        so.baseCost = baseCost;
        so.upgradeCosts = upgradeCosts;
        so.bonusValues = bonusValues;
        so.bonusDescription = bonusDesc;
        so.unlockAtShopLevel = unlockLevel;
        AssetDatabase.CreateAsset(so, $"{folder}/{fileName}.asset");
    }

    static void GenerateShopUpgrades()
    {
        string path = "Assets/ScriptableObjects/Upgrades";

        CreateUpgrade(path, "ShopLevel_01", 1, 0f, 1f, "Baslangic dukkani. Dana kiyma ve kofte satilabilir.");
        CreateUpgrade(path, "ShopLevel_02", 2, 500f, 2f, "Pirzola ve kaburga acildi!");
        CreateUpgrade(path, "ShopLevel_03", 3, 1500f, 3.5f, "Kuzu but acildi! Kasiyer kiralanabilir.");
        CreateUpgrade(path, "ShopLevel_04", 4, 4000f, 5f, "Sakatat cesitleri acildi!");
        CreateUpgrade(path, "ShopLevel_05", 5, 10000f, 8f, "Sucuk ve pastirma acildi! Teslimatci kiralanabilir.");
        CreateUpgrade(path, "ShopLevel_06", 6, 25000f, 12f, "Marineli et acildi!");
        CreateUpgrade(path, "ShopLevel_07", 7, 60000f, 18f, "Ozel kesim paketi acildi! Muhasebeci kiralanabilir.");
        CreateUpgrade(path, "ShopLevel_08", 8, 150000f, 28f, "Premium et cesitleri acildi!");
        CreateUpgrade(path, "ShopLevel_09", 9, 350000f, 45f, "Ozel siparis sistemi acildi!");
        CreateUpgrade(path, "ShopLevel_10", 10, 800000f, 70f, "Franchisor modu! Maksimum gelir.");

        Debug.Log("[SOGenerator] 10 shop upgrades created.");
    }

    static void CreateUpgrade(string folder, string fileName, int level, float cost,
        float moneyPerSecond, string desc)
    {
        ShopUpgradeData so = ScriptableObject.CreateInstance<ShopUpgradeData>();
        so.level = level;
        so.cost = cost;
        so.moneyPerSecond = moneyPerSecond;
        so.unlockDescription = desc;
        AssetDatabase.CreateAsset(so, $"{folder}/{fileName}.asset");
    }
}
