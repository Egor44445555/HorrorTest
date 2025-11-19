using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestItem : MonoBehaviour
{
    public RectTransform stripe;
    public string nameQuest;
    public string idQuest;
    public TextMeshProUGUI text;

    float currentValueTimer = 0f;
    float totalTime = 180f;
    Coroutine сlosingTimerCoroutine;

    public void CloseQuest()
    {
        if (сlosingTimerCoroutine != null)
        {
            StopCoroutine(сlosingTimerCoroutine);
        }

        сlosingTimerCoroutine = StartCoroutine(ClosingTimer());
    }

    IEnumerator ClosingTimer()
    {
        float interval = 0.00001f;

        while (currentValueTimer < totalTime)
        {
            currentValueTimer += 4f;
            stripe.sizeDelta = new Vector2(currentValueTimer, stripe.sizeDelta.y);

            if (currentValueTimer == totalTime)
            {
                Destroy(gameObject);
            }

            yield return new WaitForSeconds(interval);
        }
    }

    void OnDestroy()
    {
        if (сlosingTimerCoroutine != null)
        {
            StopCoroutine(сlosingTimerCoroutine);
            сlosingTimerCoroutine = null;
        }
    }
}
