# 05 — Reklam & Satın Alma Sistemi

## AdManager.cs
**Konum:** `Assets/Scripts/Managers/AdManager.cs`

### Kullanılan SDK
Google AdMob Unity Plugin (com.google.ads.mobile)
Package Manager → Add package by name ile ekle.

### Reklam Türleri

#### 1. Rewarded Ad (Ödüllü Video)
**Ne zaman gösterilir:**
- Offline earnings popup'ta "2x Kazan" butonuna basıldığında
- Hazırlama hızını 30 saniye hızlandırmak için
- Müşterinin sabrını yenilemek için

**Akış:**
```
ShowRewardedAd(onSuccess: Action) çağrılır
→ Reklam hazırsa göster
→ İzleme tamamlanırsa onSuccess() çağır
→ Hazır değilse: yükle + ShowToast("Reklam yükleniyor, lütfen bekle...")
```

#### 2. Interstitial Ad (Tam Ekran)
**Ne zaman gösterilir:**
- Her 5 sipariş tamamlandıktan sonra
- Dükkan yükseltme yapıldıktan sonra

**Kural:**
- Son gösterimden bu yana en az 120 saniye geçmiş olmalı
- `lastInterstitialTime` değişkeniyle kontrol et

#### 3. Banner Ad
**Ne zaman gösterilir:**
- Upgrade Panel açıkken ekranın altında sabit

**Boyut:** AdSize.Banner (320x50)
**Pozisyon:** AdPosition.Bottom

### Test ID'leri (development sırasında kullan)
```csharp
// Android Test IDs
const string REWARDED_TEST_ID     = "ca-app-pub-3940256099942544/5224354917"
const string INTERSTITIAL_TEST_ID = "ca-app-pub-3940256099942544/1033173712"
const string BANNER_TEST_ID       = "ca-app-pub-3940256099942544/6300978111"
```
**UYARI:** Yayın öncesi gerçek ID'lerle değiştirilecek.

### Inspector Alanları
```csharp
[Header("Production IDs — build öncesi doldur")]
[SerializeField] string rewardedAdId
[SerializeField] string interstitialAdId
[SerializeField] string bannerAdId
[SerializeField] bool   isTestMode = true   // true ise test ID kullan
```

### Public Metodlar
```csharp
void Initialize()
void ShowRewardedAd(Action onSuccess, Action onFailed = null)
void ShowInterstitialAd()
void ShowBanner()
void HideBanner()
bool IsRewardedAdReady()
```

### Reklam Yükleme Stratejisi
- Her reklam gösteriminin hemen ardından yeni reklam yüklemeye başla
- `OnAdClosed` callback'inde `LoadRewardedAd()` çağır
- Uygulama başlangıcında tüm reklam tiplerini preload et

---

## IAPManager.cs
**Konum:** `Assets/Scripts/Managers/IAPManager.cs`

### Kullanılan SDK
Unity IAP (com.unity.purchasing) — Package Manager'dan ekle.

### Ürün Listesi

| Ürün ID | Tür | Fiyat | İçerik |
|---------|-----|-------|--------|
| `starter_pack` | Consumable | ₺29 | x5 hız boost (30sn), 1 çalışan ücretsiz, özel kıyafet |
| `no_ads` | Non-Consumable | ₺49 | Reklamları kalıcı kapat |
| `monthly_premium` | Subscription | ₺19/ay | Offline x2, %50 reklam azalma, özel çerçeve |
| `coins_small` | Consumable | ₺9 | 500 oyun parası |
| `coins_medium` | Consumable | ₺19 | 1.500 oyun parası |
| `coins_large` | Consumable | ₺39 | 4.500 oyun parası |

### Metodlar
```csharp
void   InitializePurchasing()
void   BuyProduct(string productId)
bool   IsNoAdsPurchased()           → PlayerPrefs'ten kontrol et
bool   IsPremiumActive()            → Subscription aktif mi
void   RestorePurchases()           → iOS zorunlu
```

### Satın Alma Sonrası Aksiyonlar
```
starter_pack  → SpeedBoostManager.Apply(5, 30f), 1 random çalışan aç
no_ads        → PlayerPrefs.SetInt("no_ads", 1), banner gizle
monthly_premium → PlayerPrefs.SetInt("premium_active", 1)
coins_*       → PlayerData.AddMoney(amount)
```

---

## AdGating Kuralları

Reklam göstermeden önce her zaman şunu kontrol et:
```csharp
bool ShouldShowAd()
{
    if (IAPManager.Instance.IsNoAdsPurchased()) return false;
    if (IAPManager.Instance.IsPremiumActive()) return false; // interstitial için
    return true;
}
```
