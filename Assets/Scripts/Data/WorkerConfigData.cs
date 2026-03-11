using UnityEngine;

[CreateAssetMenu(fileName = "Worker", menuName = "MahalleKasabi/Worker")]
public class WorkerConfigData : ScriptableObject
{
    public string workerType;
    public string displayName;
    public Sprite workerIcon;
    public float baseCost;
    public float[] upgradeCosts;
    public float[] bonusValues;
    public string bonusDescription;
    public int unlockAtShopLevel;
}
