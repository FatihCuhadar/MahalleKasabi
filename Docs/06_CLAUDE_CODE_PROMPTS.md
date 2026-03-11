# 06 — Claude Code Komutları

Bu dosyadaki promptları **sırayla** Claude Code terminaline yapıştır.
Her prompt bağımsız çalışır. Bir sonrakine geçmeden önce Unity'de hata olmadığını kontrol et.

---

## AŞAMA 1 — Proje Yapısını Oluştur

```
Unity projesini MahalleKasabi adıyla Universal 2D şablonuyla oluşturduktan sonra
aşağıdaki klasör yapısını Assets altında oluştur:

Assets/Scripts/Core/
Assets/Scripts/Managers/
Assets/Scripts/UI/
Assets/Scripts/Data/
Assets/Scripts/Entities/
Assets/Scripts/Utils/
Assets/ScriptableObjects/Products/
Assets/ScriptableObjects/Workers/
Assets/ScriptableObjects/Upgrades/
Assets/Prefabs/Customers/
Assets/Prefabs/UI/
Assets/Prefabs/Effects/
Assets/Scenes/
Assets/Audio/SFX/
Assets/Audio/Music/
Assets/Sprites/Characters/
Assets/Sprites/UI/
Assets/Sprites/Shop/

Her klasöre boş bir .gitkeep dosyası ekle ki Git takip etsin.
Sonra bir .gitignore dosyası oluştur, içeriği standart Unity .gitignore olsun.
```

---

## AŞAMA 2 — Core Scriptler

```
Aşağıdaki C# scriptlerini Unity projemde Assets/Scripts/Core/ klasörüne yaz.
Her dosya ayrı bir .cs dosyası olacak. Namespace kullanma, sade tut.

1. GameManager.cs
   - Singleton pattern, DontDestroyOnLoad
   - static event Action OnGameSaved, OnGameLoaded
   - bool isGamePaused
   - Start(): LoadGame() çağır, ardından OfflineEarningsManager.Instance?.CalculateOfflineEarnings()
   - SaveGame() ve LoadGame() metodları
   - OnApplicationPause(true) ve OnApplicationQuit() → SaveGame() tetikler
   - PauseGame(): Time.timeScale=0, ResumeGame(): Time.timeScale=1

2. PlayerData.cs
   - Singleton pattern, DontDestroyOnLoad
   - float currentMoney (default 100), float totalEarned, int shopLevel (default 1)
   - float moneyPerSecond (default 1f)
   - WorkerData[] workers
   - DateTime lastSaveTime
   - bool CanAfford(float amount)
   - void AddMoney(float amount) → totalEarned'ı da güncelle, UIManager?.RefreshMoneyUI() çağır
   - bool SpendMoney(float amount) → afford kontrolü, UIManager?.RefreshMoneyUI() çağır

3. SaveManager.cs (static class)
   - static void Save(), static void Load(), static void DeleteAll()
   - PlayerPrefs keys: "player_money", "player_total_earned", "shop_level", "workers_json", "upgrades_json", "last_save_time"
   - WorkerData[] için WorkerSaveWrapper isimli [Serializable] wrapper
   - JsonUtility kullan (Newtonsoft değil)
   - Load'da her key için default değer ver

4. WorkerData.cs
   - [Serializable] class (MonoBehaviour değil)
   - string workerType, int level, bool isUnlocked
```

---

## AŞAMA 3 — Data ScriptableObjects

```
Assets/Scripts/Data/ klasörüne aşağıdaki iki ScriptableObject scripti yaz:

1. ProductData.cs
   [CreateAssetMenu(fileName="Product", menuName="MahalleKasabi/Product")]
   Alanlar: string productName, Sprite productIcon, float basePrice,
            float preparationTime, int unlockLevel, string description

2. WorkerConfigData.cs
   [CreateAssetMenu(fileName="Worker", menuName="MahalleKasabi/Worker")]
   Alanlar: string workerType, string displayName, Sprite workerIcon,
            float baseCost, float[] upgradeCosts, float[] bonusValues,
            string bonusDescription, int unlockAtShopLevel

3. ShopUpgradeData.cs
   [CreateAssetMenu(fileName="ShopUpgrade", menuName="MahalleKasabi/ShopUpgrade")]
   Alanlar: int level, float cost, float moneyPerSecond,
            string unlockDescription, Sprite shopVisual

Sonra Assets/ScriptableObjects/Products/ klasörüne şu 10 ürünün SO instance'larını oluştur:
(Context menu → Create → MahalleKasabi → Product)
Dana Kıyma: fiyat=10, süre=2, kilit=1
Köfte: fiyat=15, süre=3, kilit=1
Pirzola: fiyat=20, süre=4, kilit=2
Kaburga: fiyat=25, süre=5, kilit=2
Kuzu But: fiyat=40, süre=7, kilit=3
Sakatat: fiyat=12, süre=3, kilit=4
Sucuk: fiyat=30, süre=4, kilit=5
Pastırma: fiyat=35, süre=5, kilit=5
Marineli Et: fiyat=55, süre=8, kilit=6
Özel Kesim: fiyat=80, süre=12, kilit=7
```

---

## AŞAMA 4 — Customer System

```
Assets/Scripts/ altına şu scriptleri yaz:

1. Managers/CustomerManager.cs
   - Singleton, MonoBehaviour
   - SerializeField: GameObject[] customerPrefabs, Transform spawnPoint,
     Transform counterPosition, float baseSpawnInterval=5f, int maxCustomers=5
   - CustomerType enum: Normal, Aceleci, TopluSiparis, VIP
   - List<Customer> activeCustomers
   - IEnumerator SpawnRoutine(): ShopManager spawn hızına göre bekle, random tip seç
   - Spawn ağırlıkları: dükkan seviyesine göre değişsin (03_SHOP_SYSTEM.md'deki değerler)
   - StartSpawning(), StopSpawning(), CustomerLeft(Customer c), GetActiveCustomerCount()

2. Entities/Customer.cs
   - MonoBehaviour
   - SerializeField: CustomerType customerType, float moveSpeed=3f, Animator animator,
     GameObject orderBubblePrefab, SpriteRenderer spriteRenderer
   - float patienceTime (CustomerType'a göre: Normal=30, Aceleci=10, TopluSiparis=20, VIP=60)
   - float bonusMultiplier (Normal=1, Aceleci=1.5, TopluSiparis=3, VIP=2)
   - void Initialize(CustomerType type)
   - IEnumerator MoveToCounter(): spawnPoint'ten counterPosition'a lerp ile hareket
   - void ShowOrder(): OrderManager'a kaydet, balonu göster
   - IEnumerator PatienceCountdown(): Image fillAmount 1→0, renk geçişi yeşil→sarı→kırmızı
   - void OnOrderFulfilled(float amount): Happy animasyonu, ödeme al, Leave()
   - void OnPatienceExpired(): Angry animasyonu, Leave()
   - IEnumerator Leave(): sahne dışına hareket, Destroy

3. UI/CustomerOrderBubble.cs
   - MonoBehaviour
   - SerializeField: Image productIcon, TMP_Text quantityText, Image patienceBar
   - void Show(ProductData product, int quantity)
   - void Hide()
   - void UpdatePatience(float normalizedValue): fillAmount güncelle + renk geçişi
   - LateUpdate: billboard efekti için transform.LookAt(Camera.main.transform)
```

---

## AŞAMA 5 — Shop & Order System

```
Assets/Scripts/Managers/ klasörüne yaz:

1. OrderManager.cs
   - Singleton, MonoBehaviour
   - Order class (iç sınıf veya ayrı dosya):
     Customer customer, ProductData product, int quantity, float payment, bool isFulfilled
   - List<Order> activeOrders
   - void RegisterOrder(Customer customer, ProductData product, int quantity)
   - void FulfillNextOrder(): sıradaki siparişi tamamla, coroutine ile hazırlama süresi bekle
   - float CalculatePayment(Order order): quantity * basePrice * bonusMultiplier
   - bool HasActiveOrder(Customer customer)
   - Sipariş tamamlanınca: PlayerData.AddMoney(), customer.OnOrderFulfilled(), AudioManager.Play("coin_collect")

2. ShopManager.cs
   - Singleton, MonoBehaviour
   - SerializeField: ShopUpgradeData[] upgrades, WorkerConfigData[] workerConfigs
   - bool TryUpgradeShop(): para kontrolü, seviye artır, yeni içerikleri aç, UI güncelle
   - float GetUpgradeCost(): mevcut seviyeden bir üstünün maliyeti
   - float GetPreparationTime(ProductData p): p.preparationTime / GetWorkerSpeedMultiplier()
   - float GetSpeedMultiplier(): kasiyer seviyesine göre
   - float GetOfflineMultiplier(): muhasebeci seviyesine göre
   - bool TryHireWorker(string type): para kontrolü, PlayerData.workers güncelle
   - bool IsWorkerHired(string type)
   - bool TryUpgradeWorker(string type)
   - float GetWorkerUpgradeCost(string type)

3. OfflineEarningsManager.cs
   - Singleton, MonoBehaviour
   - void CalculateOfflineEarnings():
     * DateTime.UtcNow - PlayerData.lastSaveTime → geçen saniye
     * Max 8 saat (28800 saniye) ile sınırla
     * 30 saniyeden azsa işlem yapma
     * kazanç = geçenSaniye * PlayerData.moneyPerSecond * ShopManager.GetOfflineMultiplier()
     * PlayerData.AddMoney(kazanç)
     * UIManager.ShowOfflineEarningsPopup(kazanç, geçenSaniye) çağır
```

---

## AŞAMA 6 — UI System

```
Assets/Scripts/UI/ klasörüne yaz:

1. UIManager.cs
   - Singleton, MonoBehaviour
   - SerializeField: TMP_Text moneyText, TMP_Text shopLevelText,
     GameObject upgradePanel, GameObject offlineEarningsPopup,
     GameObject settingsPanel, TMP_Text toastText, CanvasGroup toastCanvasGroup
   - void RefreshMoneyUI(): FormatMoney() ile göster
   - string FormatMoney(float amount): <1K=normal, <1M="₺X.XK", üstü="₺X.XM"
   - void ShowOfflineEarningsPopup(float amount, float seconds)
   - void ShowUpgradePanel() / HideUpgradePanel()
   - void ShowToast(string message, Color color, float duration=2f)
   - Toast için DOTween kullan: DOFade animasyonu

2. HUDController.cs
   - MonoBehaviour
   - SerializeField: Image prepProgressBar, Button counterButton
   - void OnCounterTap(): OrderManager.FulfillNextOrder() tetikle
   - IEnumerator ShowPrepProgress(float duration): 0→1 fillAmount, bitince flash

3. UpgradePanel.cs
   - MonoBehaviour
   - SerializeField: Button shopUpgradeButton, TMP_Text upgradeCostText,
     Transform workersContainer, GameObject workerCardPrefab
   - void RefreshPanel(): tüm içeriği güncelle
   - void OnShopUpgradeClicked(): ShopManager.TryUpgradeShop()
   - void SpawnWorkerCards(): her çalışan için kart oluştur

4. OfflineEarningsPopup.cs
   - MonoBehaviour
   - SerializeField: TMP_Text amountText, TMP_Text timeText,
     Button watchAdButton, Button collectButton
   - void Show(float amount, float seconds)
   - void OnWatchAd(): AdManager.ShowRewardedAd(onSuccess: () => PlayerData.AddMoney(amount))
   - void OnCollect(): gameObject.SetActive(false)
   - string FormatDuration(float seconds): "4 saat 23 dakika" formatında
```

---

## AŞAMA 7 — Audio System

```
Assets/Scripts/Managers/AudioManager.cs dosyasını yaz:

- Singleton, DontDestroyOnLoad
- SerializeField: AudioSource sfxSource, AudioSource musicSource
- SerializeField: AudioClip[] sfxClips (inspector'dan atanacak)
  İsimler: "coin_collect", "order_complete", "upgrade_success",
           "customer_angry", "button_click", "level_up"
- void Play(string clipName): sfxClips içinde ismi eşleşeni bul, sfxSource.PlayOneShot
- void PlayMusic(AudioClip clip): musicSource.clip = clip, Play
- void SetMusicVolume(float v) ve SetSFXVolume(float v)
- bool isMuted — toggle için
- void ToggleMute()
```

---

## AŞAMA 8 — Final Kontrol ve Build Hazırlık

```
Projeyi review et ve şunları kontrol et:

1. Tüm Singleton'ların null safety için ?. operatörü kullandığından emin ol
2. Her MonoBehaviour'da OnDestroy'da event unsubscribe edildiğini kontrol et
3. PlayerPrefs.Save() sadece gerçek save anlarında çağrıldığını doğrula (her frame değil)
4. Tüm coroutine'lerin gameObject aktif değilken crash yapmadığından emin ol
   (while(gameObject.activeSelf) kontrolü ekle)
5. Time.timeScale=0 iken çalışması gereken UI coroutine'lerinde
   WaitForSecondsRealtime kullandığından emin ol
6. Build Settings'te Android platformuna geç:
   - Minimum API: 26 (Android 8.0)
   - Target API: en güncel
   - IL2CPP scripting backend
   - ARM64 + ARMv7 seç
7. PlayerSettings'te:
   - Company Name ve Product Name doldur
   - Bundle Identifier: com.SENINAD.mahallekasabi
   - Version: 0.1.0
```

---

## Hızlı Referans — Hangi Dosya Ne Yapar

| Script | Kısaca |
|--------|--------|
| GameManager | Oyun durumu, kayıt tetikleyici |
| PlayerData | Para, seviye, çalışan verisi |
| SaveManager | Diske yaz/oku |
| CustomerManager | Müşteri doğur |
| Customer | Tek müşterinin davranışı |
| OrderManager | Sipariş al, karşıla, öde |
| ShopManager | Yükselt, çalışan al, hız hesapla |
| OfflineEarningsManager | Kapalıyken kazanılan parayı hesapla |
| UIManager | Tüm UI tek noktadan |
| HUDController | Tezgah dokunuşu |
| UpgradePanel | Yükseltme ekranı |
| AdManager | Reklam göster |
| IAPManager | Satın alma |
| AudioManager | Ses çal |
