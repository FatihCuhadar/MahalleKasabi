# 03 — Shop & Upgrade System

## ShopManager.cs
**Konum:** `Assets/Scripts/Managers/ShopManager.cs`

### Sorumluluklar
- Dükkan seviyesini yönetir (1-10)
- Yükseltme satın alma mantığını yürütür
- Ürün hazırlama sürelerini ve çarpanları hesaplar
- Çalışan sistemiyle entegre çalışır

### Inspector Alanları
```csharp
[SerializeField] ShopUpgradeData[]  upgrades        // SO dizisi, Inspector'dan ata
[SerializeField] WorkerConfigData[] workerConfigs   // SO dizisi
```

### Public Metodlar
```csharp
bool  TryUpgradeShop()                              → para yetiyorsa seviye atla
float GetUpgradeCost()                              → bir sonraki seviyenin maliyeti
float GetPreparationTime(ProductData product)       → çalışana göre azaltılmış süre
float GetSpeedMultiplier()                          → kasiyere göre spawn hızı
float GetOfflineMultiplier()                        → muhasebeciyle offline bonus
bool  TryHireWorker(string workerType)              → çalışan al, para düş
bool  IsWorkerHired(string workerType)              → çalışan var mı
void  UpgradeWorker(string workerType)              → çalışanın seviyesini artır
float GetWorkerUpgradeCost(string workerType)       → çalışan yükseltme maliyeti
```

### Dükkan Seviye Maliyetleri
```
Seviye 1→2:   500₺
Seviye 2→3:   1.500₺
Seviye 3→4:   4.000₺
Seviye 4→5:   10.000₺
Seviye 5→6:   25.000₺
Seviye 6→7:   60.000₺
Seviye 7→8:   150.000₺
Seviye 8→9:   350.000₺
Seviye 9→10:  800.000₺
```

### Seviyeye Göre Açılan İçerikler
```
Seviye 1:  Dana kıyma, Köfte
Seviye 2:  Pirzola, Kaburga
Seviye 3:  Kuzu but (+ Kasiyer kilidini açar)
Seviye 4:  Sakatat çeşitleri
Seviye 5:  Sucuk, Pastırma (+ Teslimatçı kilidini açar)
Seviye 6:  Hazır marineli et
Seviye 7:  Özel kesim paketi (+ Muhasebeci kilidini açar)
Seviye 8:  Premium et çeşitleri
Seviye 9:  Özel sipariş sistemi
Seviye 10: Franchisor modu (maksimum gelir)
```

---

## OfflineEarningsManager.cs
**Konum:** `Assets/Scripts/Managers/OfflineEarningsManager.cs`

### Sorumluluklar
- Oyun kapalıyken geçen süreyi hesaplar
- Bu süreye göre para ekler
- Oyuncu oyuna döndüğünde popup gösterir

### Hesaplama Formülü
```
geçenSaniye    = (DateTime.UtcNow - PlayerData.lastSaveTime).TotalSeconds
maxOfflineSüre = 8 * 3600  (maksimum 8 saat sayılır)
etkinSüre      = Min(geçenSaniye, maxOfflineSüre)
kazanç         = etkinSüre * PlayerData.moneyPerSecond * GetOfflineMultiplier()
```

### Offline Multiplier Kaynakları
```
Muhasebeci Seviye 0 (yok):   x1.0
Muhasebeci Seviye 1:         x1.3
Muhasebeci Seviye 2:         x1.6
Muhasebeci Seviye 3:         x2.0
Muhasebeci Seviye 4:         x2.5
Muhasebeci Seviye 5:         x3.0
```

### CalculateOfflineEarnings() Akışı
1. `lastSaveTime` ile şimdiki zaman farkını al
2. Fark 30 saniyeden azsa → gösterme (anlamsız)
3. Kazancı hesapla
4. `PlayerData.AddMoney(kazanç)` çağır
5. `UIManager.ShowOfflineEarningsPopup(kazanç, geçenSüre)` çağır

---

## ProductData.cs (ScriptableObject)
**Konum:** `Assets/Scripts/Data/ProductData.cs`

### Alanlar
```csharp
[CreateAssetMenu(menuName = "MahalleKasabi/Product")]
public class ProductData : ScriptableObject
{
    string  productName        // "Dana Kıyma"
    Sprite  productIcon        // Ürün ikonu
    float   basePrice          // Temel satış fiyatı
    float   preparationTime    // Hazırlama süresi (saniye)
    int     unlockLevel        // Hangi dükkan seviyesinde açılır
    string  description        // Kısa açıklama
}
```

### Oluşturulacak Ürünler (ScriptableObject instanceleri)
```
Dana Kıyma:          fiyat=10,  süre=2sn,  kilit=1
Köfte:               fiyat=15,  süre=3sn,  kilit=1
Pirzola:             fiyat=20,  süre=4sn,  kilit=2
Kaburga:             fiyat=25,  süre=5sn,  kilit=2
Kuzu But:            fiyat=40,  süre=7sn,  kilit=3
Sakatat:             fiyat=12,  süre=3sn,  kilit=4
Sucuk:               fiyat=30,  süre=4sn,  kilit=5
Pastırma:            fiyat=35,  süre=5sn,  kilit=5
Marineli Et:         fiyat=55,  süre=8sn,  kilit=6
Özel Kesim:          fiyat=80,  süre=12sn, kilit=7
```

---

## WorkerConfigData.cs (ScriptableObject)
**Konum:** `Assets/Scripts/Data/WorkerConfigData.cs`

### Alanlar
```csharp
[CreateAssetMenu(menuName = "MahalleKasabi/Worker")]
public class WorkerConfigData : ScriptableObject
{
    string   workerType           // "kasiyer" vb.
    string   displayName          // "Kasiyer"
    Sprite   workerIcon
    float    baseCost             // İlk işe alım maliyeti
    float[]  upgradeCosts         // [500, 1500, 4000, 10000] — 4 yükseltme
    float[]  bonusValues          // Her seviyedeki bonus değeri
    string   bonusDescription     // "Sipariş alma hızını artırır"
}
```

### Çalışan Bonus Değerleri
```
Kasiyer (spawn hızı çarpanı):
  Seviye 1: 1.2x  | 2: 1.5x  | 3: 2.0x  | 4: 2.5x  | 5: 3.0x
  İşe alım: 300₺  | Yükseltmeler: 800, 2000, 5000, 12000₺

Kasap Yardımcısı (hazırlama hızı çarpanı):
  Seviye 1: 1.3x  | 2: 1.6x  | 3: 2.0x  | 4: 2.5x  | 5: 3.0x
  İşe alım: 500₺  | Yükseltmeler: 1200, 3000, 7000, 15000₺

Teslimatçı (toplu sipariş bonusu):
  Seviye 1: +10%  | 2: +20%  | 3: +35%  | 4: +50%  | 5: +75%
  İşe alım: 800₺  | Yükseltmeler: 2000, 5000, 12000, 25000₺
  NOT: Dükkan seviyesi 5 olmadan açılmaz.

Muhasebeci (offline kazanç çarpanı):
  Seviye 1: 1.3x  | 2: 1.6x  | 3: 2.0x  | 4: 2.5x  | 5: 3.0x
  İşe alım: 1000₺ | Yükseltmeler: 3000, 7000, 15000, 30000₺
  NOT: Dükkan seviyesi 7 olmadan açılmaz.
```

---

## ShopUpgradeData.cs (ScriptableObject)
**Konum:** `Assets/Scripts/Data/ShopUpgradeData.cs`

### Alanlar
```csharp
[CreateAssetMenu(menuName = "MahalleKasabi/ShopUpgrade")]
public class ShopUpgradeData : ScriptableObject
{
    int    level              // Bu kaydın ait olduğu seviye (1-10)
    float  cost               // Bu seviyeye geçiş maliyeti
    float  moneyPerSecond     // Bu seviyedeki offline kazanç hızı
    string unlockDescription  // "Kuzu but açıldı! Kasiyer artık kiralanabilir."
    Sprite shopVisual         // Bu seviyedeki dükkan görseli
}
```
