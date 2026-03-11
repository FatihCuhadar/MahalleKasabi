# 02 — Customer System

## Genel Akış

```
CustomerManager
    └── Spawn(CustomerType)
            └── Customer.cs (her müşteri bir prefab instance'ı)
                    ├── Yürü → Tezgaha gel
                    ├── Sipariş balonu göster (CustomerOrderBubble.cs)
                    ├── Sabır geri sayımı başlat
                    │       ├── Süre doldu → Mutlu ayrıl + ödeme yap
                    │       └── Sabır bitti → Kızgın ayrıl, ödeme yok
                    └── OrderManager'a siparişi bildir
```

---

## CustomerManager.cs
**Konum:** `Assets/Scripts/Managers/CustomerManager.cs`

### Sorumluluklar
- Belirli aralıklarla müşteri oluşturur (spawn)
- Aynı anda maksimum kaç müşteri olabileceğini kontrol eder
- Dükkan seviyesine göre spawn hızını ve müşteri tipini ayarlar

### Inspector Alanları
```csharp
[SerializeField] GameObject[] customerPrefabs   // 4 farklı müşteri prefab
[SerializeField] Transform    spawnPoint         // Müşterilerin çıktığı nokta
[SerializeField] Transform    counterPosition    // Tezgahın önü
[SerializeField] float        baseSpawnInterval  // Default: 5f saniye
[SerializeField] int          maxCustomers       // Default: 5
```

### Spawn Mantığı
- Coroutine ile sürekli çalışır: `SpawnRoutine()`
- `spawnInterval = baseSpawnInterval / ShopManager.Instance.GetSpeedMultiplier()`
- Sahnedeki aktif müşteri sayısı `maxCustomers`'ı geçemez
- Spawn sonrası müşteriyi `activeCustomers` listesine ekle

### Müşteri Tipleri (CustomerType enum)
```
Normal      → sabır: 30sn, bonus: 0%
Aceleci     → sabır: 10sn, bonus: +50%
TopluSiparis→ sabır: 20sn, bonus: +200% (3x ürün ister)
VIP         → sabır: 60sn, bonus: +100%
```

### Spawn Ağırlıkları (dükkan seviyesine göre)
```
Seviye 1-3:   Normal %80, Aceleci %20
Seviye 4-6:   Normal %60, Aceleci %25, VIP %15
Seviye 7-10:  Normal %40, Aceleci %25, VIP %20, TopluSiparis %15
```

### Public Metodlar
```csharp
StartSpawning()                     → SpawnRoutine coroutine'i başlat
StopSpawning()                      → coroutine durdur
void CustomerLeft(Customer c)       → activeCustomers listesinden çıkar
int  GetActiveCustomerCount()       → aktif müşteri sayısı
```

---

## Customer.cs
**Konum:** `Assets/Scripts/Entities/Customer.cs`

### Sorumluluklar
- Tek bir müşterinin tüm yaşam döngüsünü yönetir
- Animasyonları tetikler
- Sipariş verir, bekler, sonuç alır

### Inspector Alanları
```csharp
[SerializeField] CustomerType    customerType
[SerializeField] float           moveSpeed      // Default: 3f
[SerializeField] Animator        animator
[SerializeField] GameObject      orderBubble    // Sipariş balonu prefab
[SerializeField] SpriteRenderer  spriteRenderer
```

### Yaşam Döngüsü Metodları
```csharp
void Initialize(CustomerType type)  → tipi ata, sabır süresini hesapla
void MoveToCounter()                → tezgaha doğru hareket başlat (coroutine)
void ShowOrder()                    → sipariş balonunu aç, OrderManager'a kaydet
void StartPatience()                → geri sayım başlat
void OnOrderFulfilled(float amount) → ödeme al, mutlu animasyon, ayrıl
void OnPatienceExpired()            → kızgın animasyon, ödeme yok, ayrıl
void Leave()                        → sahne dışına yürü, sonra Destroy(gameObject)
```

### Sabır Sistemi
- `patienceBar`: `Image` component, `fillAmount` ile dolar/boşalır
- Renk geçişi: Yeşil (1.0) → Sarı (0.5) → Kırmızı (0.2)
- `Color.Lerp` kullan, her frame güncelle

### Animator Parametreleri (string sabitler)
```
"Walk"      → bool
"Idle"      → trigger
"Happy"     → trigger
"Angry"     → trigger
```

---

## OrderManager.cs
**Konum:** `Assets/Scripts/Managers/OrderManager.cs`

### Sorumluluklar
- Aktif siparişleri listeler
- Oyuncu dokunuşuyla sipariş karşılama mantığını yürütür
- Ödeme hesaplar ve PlayerData'ya aktarır

### Veri Yapıları
```csharp
public class Order
{
    public Customer    customer;
    public ProductData product;      // ScriptableObject
    public int         quantity;     // 1-3 arası
    public float       payment;      // quantity * product.basePrice * bonusMultiplier
    public bool        isFulfilled;
}
```

### Public Metodlar
```csharp
void  RegisterOrder(Order order)            → aktif listeye ekle
void  FulfillOrder(Customer customer)       → siparişi tamamla, ödemeyi ver
float CalculatePayment(Order order)         → fiyat hesapla
bool  HasActiveOrder(Customer customer)     → müşterinin bekleyen siparişi var mı
```

### Sipariş Karşılama Akışı
1. Oyuncu tezgaha dokunur (`OnCounterTap()`)
2. Sıradaki siparişi al (`activeOrders[0]`)
3. `ShopManager.GetPreparationTime(product)` kadar bekle (coroutine)
4. Hazırlık animasyonunu göster
5. `customer.OnOrderFulfilled(payment)` çağır
6. `PlayerData.Instance.AddMoney(payment)` çağır
7. Siparişi listeden çıkar

---

## CustomerOrderBubble.cs
**Konum:** `Assets/Scripts/UI/CustomerOrderBubble.cs`

### Sorumluluklar
- Müşterinin başı üzerinde sipariş içeriğini gösterir
- Ürün ikonunu ve miktarını gösterir

### Inspector Alanları
```csharp
[SerializeField] Image  productIcon      // Ürün görseli
[SerializeField] Text   quantityText     // "x2" gibi
[SerializeField] Image  patienceBar      // Dolu → boşalan çubuk
```

### Metodlar
```csharp
void Show(ProductData product, int quantity)   → balonu aktif et, içeriği doldur
void Hide()                                    → balonu kapat
void UpdatePatience(float normalizedValue)     → 0.0-1.0, fillAmount'u güncelle
```

### Pozisyon
Müşterinin `transform.position + Vector3(0, 1.5f, 0)` noktasında sabit durur.
`LookAt(Camera.main)` ile her zaman kameraya dönsün (Billboard efekti).
