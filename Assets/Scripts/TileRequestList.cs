using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using BtnType = TileMemo.ButtonType;

public class TileRequestList : MonoBehaviour
{
    List<GameObject> questionList = new List<GameObject>();
    public GameObject questionDisplayPrefab;
    private GameObject panel;

    // Start is called before the first frame update
    void Awake()
    {
        panel = GameObject.Find("TileRequestListPanel");
        //CreateQuestionList(2);
    }

    int currentRequestPtr;

    public void CreateQuestionList(int count, int range)
    {
        currentRequestPtr = 0;

        float prefabWidth = questionDisplayPrefab.GetComponent<RectTransform>().sizeDelta.x;

        Vector2 oofset = new Vector2((count - 1) * -(1.125f * prefabWidth / 2f), 0f);

        BtnType[] randomizedBtnTypeArr = GetRandomBtnTypeSequence(count, range);

        for (int i=0; i< count; i++)
        {
            var tileReq = Instantiate(questionDisplayPrefab, panel.transform);

            tileReq.name = "Tile Request [" + i + "]";

            tileReq.transform.localPosition = oofset;

            tileReq.GetComponent<TileRequest>().Initialize(randomizedBtnTypeArr[i]);

            questionList.Add(tileReq);

            oofset.x += 1.125f * prefabWidth;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*
     * Guess method:
     * return -1 if guess incorrect
     * return 0 if guess correctly and there is still more guesses left
     * return 1 if guess correctly and no guesses is needed
     */
    public int Guess(BtnType btnType)
    {
        var firstRequestTile = questionList[currentRequestPtr].GetComponent<TileRequest>();
        bool result = (btnType == firstRequestTile.GetRequestedType());
        //if correctly guess
        if (result)
        {
            //if there is no any more tile request; return 1
            if (++currentRequestPtr >= questionList.Count) return 1;
            //else show the next tile request and return 0
            RevealCurrent();
            return 0;
        }

        //guess incorrect
        //shake the button
        firstRequestTile.Shake();
        return -1;

    }
    //reveal current tile request currentRequestPtr is pointing to
    public void RevealCurrent()
    {
        var currentTileRequest = questionList[currentRequestPtr].GetComponent<TileRequest>();
        currentTileRequest.Show();
        currentTileRequest.LightenToggle();
        if (currentRequestPtr > 0) questionList[currentRequestPtr-1].GetComponent<TileRequest>().LightenToggle();
    }

    public void ResetList()
    {
        while (questionList.Count > 0)
        {
            GameObject.Destroy(questionList[0]);
            questionList.RemoveAt(0);
        }

        currentRequestPtr = 0;
    }

    public BtnType[] GetRandomBtnTypeSequence(int count, int typeCount)
    {
        if (count > 2*typeCount) return null;

        BtnType[] randomBtnTypeSequence = new BtnType[count];

        int[] btnTypeFreq = new int[typeCount];

        for (int i = 0; i < typeCount; i++) btnTypeFreq[i] = 0; 

        for (int i=0; i<count; i++)
        {
            int randomVal = UnityEngine.Random.Range(0, typeCount);

            while (btnTypeFreq[randomVal] > 1) randomVal = (randomVal + 1) % typeCount;

            ++btnTypeFreq[randomVal];
            randomBtnTypeSequence[i] = TileMemo.CastToBtnType(randomVal);

        }

        return randomBtnTypeSequence;
    }
}
