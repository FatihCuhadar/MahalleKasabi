using UnityEngine;

[CreateAssetMenu(fileName = "Product", menuName = "MahalleKasabi/Product")]
public class ProductData : ScriptableObject
{
    public string productName;
    public Sprite productIcon;
    public float basePrice;
    public float preparationTime;
    public int unlockLevel;
    [TextArea] public string description;
}
