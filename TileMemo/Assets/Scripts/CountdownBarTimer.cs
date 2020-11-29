using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountdownBarTimer : MonoBehaviour
{
    private Slider slider;
    private float maxVal;
    private float currentValue = 0f;
    public float CurrentValue
    {
        get
        {
            return currentValue;
        }
        set
        {
            currentValue = value;
            slider.value = currentValue;
        }
    }

    bool currentlyCountingdown;

    void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        slider = GetComponent<Slider>();

        CurrentValue = 0f;
        maxVal = 1f;

        currentlyCountingdown = false;
    }

    public void SkipCountdown()
    {
        if (currentlyCountingdown)
        {
            StopCoroutine(coroutine);
            CurrentValue = maxVal;
            currentlyCountingdown = false;
        }
    }

    Coroutine coroutine;

    public void Countdown(float seconds) => coroutine = StartCoroutine(CountdownCoroutine(seconds));
    IEnumerator CountdownCoroutine(float seconds)
    {
        slider.maxValue = seconds;
        maxVal = seconds;
        CurrentValue = 0f;

        currentlyCountingdown = true;

        while (CurrentValue < maxVal)
        {
            CurrentValue += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        currentlyCountingdown = false;
    }

}
