using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class PrepStation
{
    public int stationId;
    public bool isBusy;
    public float progress;
    public float duration;
    public Order order;
    public int lineIndex = -1;

    private Coroutine routine;

    public void StartPreparing(MonoBehaviour host, int id, Order targetOrder, int targetLineIndex, float prepDuration, Action<PrepStation> onDone)
    {
        Stop(host);
        stationId = id;
        order = targetOrder;
        lineIndex = targetLineIndex;
        duration = Mathf.Max(0.1f, prepDuration);
        progress = 0f;
        isBusy = true;
        routine = host.StartCoroutine(PrepareRoutine(onDone));
    }

    public void Stop(MonoBehaviour host)
    {
        if (routine != null && host != null)
            host.StopCoroutine(routine);
        routine = null;
        isBusy = false;
        progress = 0f;
        duration = 0f;
        order = null;
        lineIndex = -1;
    }

    private IEnumerator PrepareRoutine(Action<PrepStation> onDone)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            progress = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        progress = 1f;
        isBusy = false;
        onDone?.Invoke(this);
        routine = null;
    }
}
