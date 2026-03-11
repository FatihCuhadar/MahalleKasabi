# 04 — UI System

## Ekran Hiyerarşisi

```
Canvas (Screen Space - Overlay)
├── HUD                         → Ana oyun ekranı üstü bilgiler
│   ├── MoneyText               → "₺1.250"
│   ├── ShopLevelText           → "Seviye 3"
│   └── SettingsButton
├── CounterArea                 → Tezgah dokunuş alanı
│   └── PrepProgressBar         → Hazırlama ilerleme çubuğu
├── UpgradePanel                → Alt panel (swipe up ile açılır)
│   ├── ShopUpgradeSection
│   ├── WorkersSection
│   └── ProductsSection
├── OfflineEarningsPopup        → Oyun açılırken gösterilir
├── SettingsPanel
└── ToastMessage                → Geçici bildirim ("Yükseltme yapıldı!")
```

---

## UIManager.cs
**Konum:** `Assets/Scripts/UI/UIManager.cs`

### Sorumluluklar
- Tüm UI panellerinin açılıp kapanmasını yönetir
- Tek nokta UI erişimi (singleton)
- Para göstergesini sürekli güncel tutar

### Inspector Alanları
```csharp
[SerializeField] Text        moneyText
[SerializeField] Text        shopLevelText
[SerializeField] GameObject  upgradePanel
[SerializeField] GameObject  offlineEarningsPopup
[SerializeField] GameObject  settingsPanel
[SerializeField] Text        toastText
[SerializeField] Animator    toastAnimator
```

### Public Metodlar
```csharp
void RefreshMoneyUI()
    → moneyText.text = FormatMoney(PlayerData.Instance.currentMoney)
    → shopLevelText.text = "Seviye " + PlayerData.Instance.shopLevel

void ShowOfflineEarningsPopup(float amount, float seconds)
    → Popup'ı aç, kazanç miktarını ve geçen süreyi göster
    → "4 saat boyunca ₺2.340 kazandın!" formatında

void ShowUpgradePanel()   → upgradePanel.SetActive(true) + animasyon
void HideUpgradePanel()   → upgradePanel.SetActive(false)
void ShowToast(string message, float duration = 2f)
    → toastText'i güncelle, animasyonu oynat, duration sonra gizle

string FormatMoney(float amount)
    → 999 altı: "₺999"
    → 1K-999K:  "₺1.2K"
    → 1M+:      "₺1.5M"
```

---

## HUDController.cs
**Konum:** `Assets/Scripts/UI/HUDController.cs`

### Sorumluluklar
- Tezgah dokunuşunu algılar
- Hazırlama animasyonunu gösterir

### Dokunuş Algılama
```
OnCounterTap() → yalnızca tezgah alanına dokunulduğunda tetikle
→ OrderManager.Instance.FulfillNextOrder() çağır
→ PrepProgressBar animasyonu başlat (hazırlama süresi kadar dolar)
```

### PrepProgressBar
- `Image` component, `fillAmount` = 0'dan 1'e
- `ShopManager.GetPreparationTime()` süresinde dolar
- Dolunca flash efekti + ses çal

---

## UpgradePanel.cs
**Konum:** `Assets/Scripts/UI/UpgradePanel.cs`

### Sorumluluklar
- Dükkan yükseltme butonunu yönetir
- Çalışan kartlarını listeler
- Açık/kilitli ürünleri gösterir

### Dükkan Yükseltme Butonu
```
Buton metni: "Dükkanı Yükselt — ₺500"
Para yeterliyse: aktif, yeşil
Para yetmiyorsa: deaktif, gri, kilit ikonu
Basıldığında: ShopManager.TryUpgradeShop() çağır
Başarılıysa: ShowToast("Dükkan Seviye X'e yükseltildi! 🎉")
```

### Çalışan Kartı (WorkerCard prefab)
Her çalışan için bir kart oluştur. Kart içeriği:
```
[İkon] [Ad]        [Seviye: 3/5]
Kasiyer            ★★★☆☆
Sipariş hızı: 2.0x
[İşe Al — ₺300]  veya  [Yükselt — ₺2.000]
```
- Kilitliyse: tüm kart gri + kilit ikonu + "Seviye X'te açılır"
- Maksimum seviyedeyse: "MAKSİMUM" rozeti

---

## OfflineEarningsPopup.cs
**Konum:** `Assets/Scripts/UI/OfflineEarningsPopup.cs`

### Görsel İçerik
```
🌙 Hoş Geldin!
4 saat 23 dakika boyunca
dükkanın çalıştı!

₺2.340 kazandın!

[2x Kazan — Reklam İzle]    [Almak İçin Dokun]
```

### Buton Davranışları
- "Reklam İzle" → rewarded ad göster, başarılıysa amount*2 ver
- "Almak İçin Dokun" → popup'ı kapat, normal miktarı ver (zaten eklenmiş)

---

## ToastMessage
- Canvas'ın en üstünde, ortada
- Animasyon: yukarı kayar, 0.3sn fade in → 1.5sn bekle → 0.3sn fade out
- DOTween ile yap: `transform.DOLocalMoveY`, `canvasGroup.DOFade`
- Renk: Başarı=yeşil, Hata=kırmızı, Bilgi=sarı

---

## Renk Paleti

```
Ana Renk (Teal):     #0D7377
Vurgu (Turuncu):     #E8762B
Para (Altın):        #F4A422
Arka Plan:           #1A1A2E
Kart Arka Plan:      #16213E
Başarı (Yeşil):      #2D6A4F
Hata (Kırmızı):      #C1121F
Kilitli (Gri):       #6C757D
Metin (Beyaz):       #FFFFFF
Metin (Soluk):       #ADB5BD
```

## Font
- Başlık: Bold, 32-40px
- Gövde: Regular, 20-24px
- Küçük: Regular, 16px
- Türkçe karakter desteği zorunlu (ş, ğ, ü, ö, ç, ı)
- Unity'de TextMeshPro kullan (TMP)

## Ses Efektleri (AudioManager üzerinden çağrılacak)
```
"coin_collect"      → Müşteri ödeme yapınca
"order_complete"    → Sipariş tamamlanınca
"upgrade_success"   → Yükseltme yapılınca
"customer_angry"    → Müşteri kızgın gidince
"button_click"      → Her UI butonu basışında
```
