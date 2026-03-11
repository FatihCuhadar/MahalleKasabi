using UnityEngine;

[CreateAssetMenu(fileName = "ShopUpgrade", menuName = "MahalleKasabi/ShopUpgrade")]
public class ShopUpgradeData : ScriptableObject
{
    public int level;
    public float cost;
    public float moneyPerSecond;
    [TextArea] public string unlockDescription;
    public Sprite shopVisual;
}
