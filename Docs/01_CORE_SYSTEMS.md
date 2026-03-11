# 01 — Core Systems

## GameManager.cs
**Konum:** `Assets/Scripts/Core/GameManager.cs`

### Sorumluluklar
- Oyunun genel durumunu yönetir (pause/resume)
- Uygulama kapanırken/arka plana alınırken otomatik kayıt tetikler
- Singleton pattern ile sahneler arası yaşar (`DontDestroyOnLoad`)
- Diğer manager'ların başlangıç sırasını koordine eder

### Public API
```
GameManager.Instance            → Singleton erişim
SaveGame()                      → Mevcut durumu kaydet
LoadGame()                      → Kaydı yükle
PauseGame()                     → Time.timeScale = 0, isGamePaused = true
ResumeGame()                    → Time.timeScale = 1, isGamePaused = false
```

### Events (static)
```
OnGameSaved     → Action, kayıt tamamlandığında
OnGameLoaded    → Action, yükleme tamamlandığında
```

### Başlangıç Sırası (Start metodu)
1. `SaveManager.Load()` çağır
2. `PlayerData` değerlerini UI'a yansıt
3. `OfflineEarningsManager.CalculateOfflineEarnings()` çağır
4. `CustomerManager.StartSpawning()` çağır

### Davranış Kuralları
- `OnApplicationPause(true)` → SaveGame çağır
- `OnApplicationQuit()` → SaveGame çağır
- Sahne geçişlerinde destroy edilmez

---

## SaveManager.cs
**Konum:** `Assets/Scripts/Core/SaveManager.cs`

### Sorumluluklar
- `PlayerPrefs` kullanarak veriyi cihaza kaydeder
- JSON serialize/deserialize işlemlerini yapar
- Oyunun çevrimdışı çalışmasını sağlar (internet gerektirmez)

### Kayıt Anahtarları (sabitler)
```
"player_money"          → float  : mevcut para
"player_total_earned"   → float  : tüm zamanların kazancı
"shop_level"            → int    : dükkan seviyesi (1-10)
"workers_json"          → string : WorkerData[] JSON olarak
"upgrades_json"         → string : satın alınan yükseltmeler
"last_save_time"        → string : DateTime.UtcNow.ToBinary().ToString()
```

### Public API (static metodlar)
```
Save()          → Tüm PlayerData'yı PlayerPrefs'e yaz, PlayerPrefs.Save() çağır
Load()          → PlayerPrefs'ten oku, PlayerData.Instance'a ata
DeleteAll()     → PlayerPrefs.DeleteAll() — sadece debug için
```

### Serialization
- `WorkerData[]` için `WorkerSaveWrapper` adında `[Serializable]` wrapper sınıf kullan
- `JsonUtility.ToJson` ve `JsonUtility.FromJson` kullan (Newtonsoft değil)
- Kayıt yoksa default değerler: para=100f, level=1, workers=boş dizi

### Hata Yönetimi
- `PlayerPrefs.GetFloat(key, defaultValue)` şeklinde her zaman default ver
- `DateTime.FromBinary` parse hatası durumunda `lastSaveTime = DateTime.UtcNow` ata

---

## PlayerData.cs
**Konum:** `Assets/Scripts/Core/PlayerData.cs`

### Sorumluluklar
- Oyuncuya ait tüm runtime verisini tutar
- Singleton pattern, `DontDestroyOnLoad`
- Para işlemleri için merkezi nokta

### Alanlar
```csharp
float  currentMoney       // Mevcut bakiye, başlangıç: 100f
float  totalEarned        // Tüm zamanların toplam kazancı
int    shopLevel          // 1-10 arası
float  moneyPerSecond     // Offline hesap için, yükseltmelerle artar
WorkerData[] workers      // Kilitli/açık tüm çalışanlar
DateTime lastSaveTime     // Son kayıt zamanı
```

### Public Metodlar
```csharp
bool  CanAfford(float amount)   → currentMoney >= amount
void  AddMoney(float amount)    → bakiyeye ekle + totalEarned'i güncelle + UI yenile
bool  SpendMoney(float amount)  → afford kontrolü yap, yeterliyse düş ve true dön
```

### AddMoney ve SpendMoney Sonrası
Her iki metoddan sonra `UIManager.Instance?.RefreshMoneyUI()` çağır.

---

## WorkerData.cs
**Konum:** `Assets/Scripts/Data/WorkerData.cs`

### Yapı
```csharp
[Serializable]
public class WorkerData
{
    string workerType;   // "kasiyer" | "kasap_yardimcisi" | "teslimatci" | "muhasebeci"
    int    level;        // 1-5
    bool   isUnlocked;
}
```

---

## Boot Sahnesi (Boot.unity)

### Amaç
Oyun ilk açıldığında kısa bir yükleme ekranı göstermek ve assetleri hazırlamak.

### Yapı
- `BootLoader.cs` scripti
- `Start()` içinde: Managers'ları yükle → 1.5 saniye bekle → Main sahnesine geç
- `SceneManager.LoadScene("Main")` kullan
- Progress bar animasyonu göster (fake loading, 0'dan 100'e 1.5 saniyede)
