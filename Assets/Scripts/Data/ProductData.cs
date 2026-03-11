using UnityEngine;

[CreateAssetMenu(fileName = "Product", menuName = "MahalleKasabi/Product")]
public class ProductData : ScriptableObject
{
    public string productName;
    public Sprite productIcon;
    public float basePrice;
    public float prepTime = 3f;
    public float preparationTime;
    public int unlockLevel;
    [TextArea] public string description;

    public float GetPrepTime()
    {
        if (prepTime > 0f) return prepTime;
        return preparationTime > 0f ? preparationTime : 1f;
    }
}
