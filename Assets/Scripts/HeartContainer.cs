using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartContainer : MonoBehaviour
{
    int lives;
    List<GameObject> hearts;

    public GameObject heartPrefab;

    private int Lives {
        get
        {
            return lives;
        }
        set
        {
            lives = value;
        }
    }

    public void Initialize()
    {
        hearts = new List<GameObject>();
    }

    public void ResetLives(int lives)
    {
        Lives = lives;

        float prefabWidth = heartPrefab.GetComponent<RectTransform>().sizeDelta.x;

        float i = this.transform.localPosition.x - 0.5f * this.GetComponent<RectTransform>().sizeDelta.x + prefabWidth;

        foreach (var obj in hearts) { Destroy(obj); }
        hearts.Clear();

        while (hearts.Count < lives)
        {
            var heartObj = Instantiate(heartPrefab, this.transform);

            heartObj.transform.localPosition = new Vector3(i, 0f, 0f);

            hearts.Add(heartObj);

            i += prefabWidth;
        }
    }

    public void RemoveLife()
    {
        Lives--;
        Destroy(hearts[hearts.Count - 1]);
        hearts.RemoveAt(hearts.Count - 1);
    }

    public int GetLives() => Lives;
}
