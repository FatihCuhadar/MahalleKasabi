using UnityEngine;

[CreateAssetMenu(fileName = "ShopLevelData", menuName = "MahalleKasabi/ShopLevelData")]
public class ShopLevelData : ScriptableObject
{
    public int level = 1;
    public string displayName = "Kucuk Tezgah";
    public float requiredTotalEarnings = 0f;
    [TextArea] public string unlockDescription;
    public Color backgroundTint = Color.white;
}
