using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BtnType = TileMemo.ButtonType;

public class TileRequest : MonoBehaviour
{
    Image image;

    [SerializeField]
    BtnType requestedType;

    static Sprite hiddenSprite;
    Sprite shownSprite;
    Sprite lightenShownSprite;

    public BtnType GetRequestedType() => requestedType;

    public void Initialize(BtnType requestType)
    {
        image = GetComponent<Image>();
        requestedType = requestType;

        hiddenSprite = Resources.Load<Sprite>("Sprites/tilememo/tilememo_unknown");
        shownSprite = Resources.Load<Sprite>("Sprites/tilememo/normal/tilememo_" + requestedType);
        lightenShownSprite = Resources.Load<Sprite>("Sprites/tilememo/lighten/tilememo_" + requestedType + "_lighten");

        image.sprite = hiddenSprite;

        isShaking = false;
        isLighten = false;
    }

    public void Show()
    {
        image.sprite = shownSprite;
        StartCoroutine(ScaleSize());
    }

    IEnumerator ScaleSize()
    {
        var thisTransform = this.transform;
        var scaleVec = new Vector3(1f, 1f, 0f);
        while (thisTransform.localScale.x < 1.05f)
        {
            thisTransform.localScale += (scaleVec * Time.deltaTime);
            if (thisTransform.localScale.x > 1.05f) thisTransform.localScale = new Vector3(1.05f, 1.05f, 0f);
            yield return new WaitForEndOfFrame();
        }
    }

    bool isShaking;

    bool isLighten;

    //change the sprite into brighten version
    public void LightenToggle()
    {
        if (isLighten) image.sprite = shownSprite;
        else image.sprite = lightenShownSprite;
        isLighten = !isLighten;
    }


    //shake the request tile
    public void Shake() { if (!isShaking) { StartCoroutine(ShakingCoroutine()); isShaking = true; } }
    IEnumerator ShakingCoroutine()
    {
        var thisRectTransform = GetComponent<RectTransform>();
        float timer = 0f;

        int shakeDirection = 1;
        int shakeCount = 0;

        float shakePeriod = 0.025f;

        var thisOriginalPosition = thisRectTransform.position;

        thisRectTransform.SetPositionAndRotation(thisRectTransform.position + new Vector3(-shakeDirection * 2f, 0f, 0f), Quaternion.identity);

        while (shakeCount < 10)
        {
            timer += Time.deltaTime;
            if (timer > shakePeriod)
            {
                thisRectTransform.SetPositionAndRotation(thisRectTransform.position + new Vector3(shakeDirection * 4f,0f,0f), Quaternion.identity);
                shakeDirection *= -1;
                shakeCount++;
                timer -= shakePeriod;
            }
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(shakePeriod);

        thisRectTransform.SetPositionAndRotation(thisOriginalPosition, Quaternion.identity);

        isShaking = false;
    }

}
