using UnityEngine;

[System.Serializable]
public class CustomerTypeStats
{
    public CustomerType type;
    public float patienceSeconds;
    public float paymentMultiplier;
    public float spawnWeight;
    public Color tintColor;
    public Color bubbleTint;
    public bool vipBadge;
}

public static class CustomerData
{
    public static CustomerTypeStats Get(CustomerType type)
    {
        switch (type)
        {
            case CustomerType.Aceleci:
                return new CustomerTypeStats
                {
                    type = type,
                    patienceSeconds = 7f,
                    paymentMultiplier = 1.2f,
                    spawnWeight = 20f,
                    tintColor = new Color(0.95f, 0.35f, 0.35f, 1f),
                    bubbleTint = new Color(1f, 0.85f, 0.85f, 1f),
                    vipBadge = false
                };
            case CustomerType.Sabirli:
                return new CustomerTypeStats
                {
                    type = type,
                    patienceSeconds = 30f,
                    paymentMultiplier = 1f,
                    spawnWeight = 15f,
                    tintColor = new Color(0.45f, 0.65f, 1f, 1f),
                    bubbleTint = new Color(0.9f, 0.95f, 1f, 1f),
                    vipBadge = false
                };
            case CustomerType.VIP:
                return new CustomerTypeStats
                {
                    type = type,
                    patienceSeconds = 20f,
                    paymentMultiplier = 1.5f,
                    spawnWeight = 5f,
                    tintColor = new Color(1f, 0.85f, 0.3f, 1f),
                    bubbleTint = new Color(1f, 0.95f, 0.75f, 1f),
                    vipBadge = true
                };
            default:
                return new CustomerTypeStats
                {
                    type = CustomerType.Normal,
                    patienceSeconds = 15f,
                    paymentMultiplier = 1f,
                    spawnWeight = 60f,
                    tintColor = Color.white,
                    bubbleTint = new Color(0.96f, 0.96f, 0.96f, 1f),
                    vipBadge = false
                };
        }
    }
}
