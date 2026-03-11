using System;

[Serializable]
public class WorkerData
{
    public string workerType;
    public int level;
    public bool isUnlocked;

    public WorkerData(string workerType, int level = 1, bool isUnlocked = false)
    {
        this.workerType = workerType;
        this.level = level;
        this.isUnlocked = isUnlocked;
    }
}
