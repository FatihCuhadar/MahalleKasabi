# Mahalle Kasabı — Proje Özeti

## Genel Bilgi

| Alan | Detay |
|------|-------|
| Oyun Adı | Mahalle Kasabı |
| Tür | Idle / Casual Simülasyon |
| Motor | Unity 6000.3.10f1 LTS |
| Şablon | Universal 2D |
| Hedef Platform | Android (önce) + iOS |
| Minimum Android | 8.0 (API 26) |
| Minimum iOS | 13.0 |
| Hedef APK Boyutu | < 100 MB |
| Dil | C# |
| Versiyon Kontrol | GitHub |

## Oyun Özeti

Oyuncu küçük bir kasap dükkanı işletir. Müşteriler gelir, sipariş verir, oyuncu siparişi
hazırlar ve müşteriye teslim eder. Kazanılan parayla dükkan geliştirilir, çalışan alınır,
yeni ürünler açılır. Oyundan çıkıldığında bile para kazanılmaya devam eder (offline kazanç).

## Klasör Yapısı (Unity Projesi)

```
Assets/
├── Scripts/
│   ├── Core/           → GameManager, SaveManager, PlayerData
│   ├── Managers/       → CustomerManager, OrderManager, ShopManager, OfflineEarningsManager
│   ├── UI/             → UIManager, HUDController, UpgradePanel, CustomerOrderBubble
│   ├── Data/           → ScriptableObject tanımları (ürünler, çalışanlar, yükseltmeler)
│   ├── Entities/       → Customer, Worker sınıfları
│   └── Utils/          → Helpers, Extensions
├── Prefabs/
│   ├── Customers/
│   ├── UI/
│   └── Effects/
├── ScriptableObjects/
│   ├── Products/
│   ├── Workers/
│   └── Upgrades/
├── Scenes/
│   ├── Main.unity      → Ana oyun sahnesi
│   └── Boot.unity      → Yükleme sahnesi
├── Sprites/
│   ├── Characters/
│   ├── UI/
│   └── Shop/
└── Audio/
    ├── SFX/
    └── Music/
```

## Bağımlılıklar (Package Manager'dan eklenecek)

- **Google AdMob (Mobile Ads Unity Plugin)** → Reklam sistemi
- **Unity IAP** → Uygulama içi satın alma
- **Firebase Analytics** → Kullanıcı takibi
- **DOTween** → UI animasyonları (Asset Store, ücretsiz)

## Dokümantasyon Dosyaları

| Dosya | İçerik |
|-------|--------|
| 01_CORE_SYSTEMS.md | GameManager, SaveManager, PlayerData |
| 02_CUSTOMER_SYSTEM.md | Müşteri spawn, sipariş, ödeme akışı |
| 03_SHOP_SYSTEM.md | Ürünler, yükseltmeler, çalışan sistemi |
| 04_UI_SYSTEM.md | Tüm UI ekranları ve bileşenleri |
| 05_OFFLINE_EARNINGS.md | Offline kazanç hesaplama sistemi |
| 06_ADS_AND_IAP.md | Reklam ve satın alma entegrasyonu |
| 07_DATA_DESIGN.md | ScriptableObject yapıları |
| 08_CLAUDE_CODE_PROMPTS.md | Hazır Claude Code komutları |
