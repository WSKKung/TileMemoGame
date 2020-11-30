using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToSongBeatAnimation : MonoBehaviour
{
    bool animStart;
    public int animationType;
    public void StartAnimation() => StartAnimation(animationType);


    public void StopAnimation()
    {
        animStart = false;
    }

    public void StartAnimation(int animType)
    {
        animStart = true;
        switch (animType)
        {
            case 0: StartCoroutine(MovementAnimation()); break;
            case 1: StartCoroutine(FlashingImageAnimation()); break;
            case 2: StartCoroutine(FlashingTextAnimation()); break;
        }
    }

    IEnumerator MovementAnimation()
    {
        var originalPos = this.transform.localPosition;
        float speed = 8f;

        while(animStart)
        {
            float timer = 0f;
            while (timer < 0.428571f) //timer < 3/7
            {
                this.transform.localPosition += new Vector3(0f, -Time.deltaTime * speed, 0f);
                timer += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            this.transform.localPosition = originalPos;
            yield return null;
        }
        this.transform.localPosition = originalPos;
    }
    IEnumerator FlashingImageAnimation()
    {
        var image = GetComponent<Image>();
        image.color = new Color(0.8359375f, 0.8359375f, 0.8359375f);
        var originalColor = image.color;
        float multiplier = 3f;

        while (animStart)
        {
            image.color = new Color(1f, 1f, 1f);
            yield return new WaitForEndOfFrame();
            float timer = 0f;
            while (timer < 0.428571f) //timer < 3/7
            {
                float v = Mathf.Lerp(image.color.r, originalColor.r, multiplier * Time.deltaTime);
                image.color = new Color(v, v, v);
                timer += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
        image.color = originalColor;
    }
    IEnumerator FlashingTextAnimation()
    {
        var text = GetComponent<Text>();
        var originalColor = text.color;
        float multiplier = 3f;

        while (animStart)
        {
            text.color = new Color(1f, 1f, 1f);
            yield return new WaitForEndOfFrame();
            float timer = 0f;
            while (timer < 0.428571f) //timer < 3/7
            {
                float v = Mathf.Lerp(text.color.r, originalColor.r, multiplier * Time.deltaTime);
                text.color = new Color(v, v, v);
                timer += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            text.color = originalColor;
        }
        text.color = originalColor;
    }
}
