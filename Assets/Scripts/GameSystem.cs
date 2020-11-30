using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.Events;

using BtnType = TileMemo.ButtonType;

public class GameSystem : MonoBehaviour
{

    const int MAX_LEVEL_COUNT = 16;

    public Canvas gameCanvas;
    public Canvas lobbyCanvas;

    private GameObject panel;

    public GameObject tileBtnPrefab;
    public GameObject countdownBar;
    public GameObject heartContainer;
    public GameObject skipButton;
    public GameObject highscorePanel;

    private SoundManager soundManager;

    private DataSavingManager saveManager;

    public GameObject gameResultPanel;
    public GameObject gameBackground;

    public Vector2Int btnsRectArraySize;

    List<GameObject> tileMemos = new List<GameObject>();

    TileRequestList tileRequestList;

    List<GameObject> gameplayGuiObjects;

    GameObject buttonStart;
    GameObject levelSelectDropdown;
    public GameObject levelDisplayText;

    int selectedLevel;
    int lastLevelUnlocked;

    List<Sprite> backgroundSprites = new List<Sprite>();

    void Awake()
    {
        panel = GameObject.Find("GamePanel");
        countdownBar.GetComponent<CountdownBarTimer>().Initialize();
        countdownBar.SetActive(false);

        //set tile request list
        tileRequestList = GetComponent<TileRequestList>();

        //initialize heart container obj
        heartContainer.GetComponent<HeartContainer>().Initialize();

        //initialize level settings
        InitializeLevelSettings();

        //set object
        buttonStart = GameObject.Find("ButtonStart");
        levelSelectDropdown = GameObject.Find("LevelSelectDropdown");
        levelDisplayText = GameObject.Find("LevelDisplayText");

        skipButton = GameObject.Find("SkipButton");

        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        saveManager = GetComponent<DataSavingManager>();

        //add tagged gui object to the corresponding list
        gameplayGuiObjects = GameObject.FindGameObjectsWithTag("GameplayGUI").ToList();

        //disable objects in game canvas
        gameCanvas.gameObject.SetActive(false);

        selectedLevel = 1;
        lastLevelUnlocked = 1;
        int unlockedLevels = saveManager.GetUnlockedLevelCount();

        UnlockLevels(unlockedLevels - 1);

        backgroundSprites.Add(Resources.Load<Sprite>("Sprites/background"));
        backgroundSprites.Add(Resources.Load<Sprite>("Sprites/background_hardstage"));

        soundManager.PlayMusic("lobby_song",1f,true);

        GameObject.Find("LogoImage").GetComponent<ToSongBeatAnimation>().StartAnimation();
        GameObject.Find("LobbyBackground").GetComponent<ToSongBeatAnimation>().StartAnimation();

    }

    //reset all buttons
    public void ResetAllButtons()
    {
        ResetRandomPool(tileMemos.Count, 5);

        //GameObject[] btnScripts = GameObject.FindGameObjectsWithTag("TileMemo");

        foreach (GameObject eachBtn in tileMemos)
        {
            TileMemo eachTileBtn = eachBtn.GetComponent<TileMemo>();
            eachTileBtn.ResetButtonType(GetRandomFromPool());
            eachTileBtn.Disable();
        }
    }

    //create an array of tiles
    public void CreateButtonsArray(int countX, int countY)
    {
        countX = Mathf.Clamp(countX, 2, 5);
        countY = Mathf.Clamp(countY, 2, 5);

        Rect panelRect = panel.GetComponent<RectTransform>().rect;
        float panelWidth = panelRect.width;
        float panelHeight = panelRect.height;

        Vector2 offsetFactor = new Vector2(0, -panelHeight * 0.05f);
        Vector2 offsetScaleFactor = new Vector2(0.08125f * countX - 0.025f, 0.04375f * countY - 0.0175f);

        Vector2 scale = Vector2.one * ( ( 17f - Math.Min(countX, countY)) / 12f );

        Vector2 offset = new Vector2(
            panelWidth * -offsetScaleFactor.x,
            panelHeight * -offsetScaleFactor.y + offsetFactor.y
            );
        Vector2 padding = new Vector2(
            panelWidth * 2.0f * offsetScaleFactor.x / (countX - 1), 
            panelHeight * 2.0f * offsetScaleFactor.y / (countY - 1)
            );

        for (int j = 0; j < countY; j++)
        {
            for (int i = 0; i < countX; i++)
            {
                //instantiate new tileBtnPrefab prefab with parent set to this panel
                GameObject newTileBtn = Instantiate(tileBtnPrefab, panel.transform);

                //set location
                newTileBtn.transform.localPosition = offset;

                //scale
                newTileBtn.transform.localScale = scale;

                //create var to store TileMemo component in the instantiate prefab
                TileMemo currentTileBtn = newTileBtn.GetComponent<TileMemo>();

                //initialize
                currentTileBtn.Initialize(GetRandomFromPool());

                //set name for debugging, not visible in the game and not used in the game system
                currentTileBtn.name = "TileMemo Button [" + (i + j * countX) + "]";

                //add the instantiated object to the list
                tileMemos.Add(newTileBtn);
                offset.x += padding.x;

            }
            offset.x = panelWidth * -offsetScaleFactor.x;
            offset.y += padding.y;
        }
    }

    //BtnType random pool
    List<BtnType> randomTileTypePool = new List<BtnType>();

    //reset random pool for a given pool size and amount of btnType type
    public void ResetRandomPool(int size, int typeCount)
    {
        size = Mathf.Clamp(size, 4, 25);
        typeCount = Mathf.Clamp(typeCount, 2, 5);

        randomTileTypePool.Clear();

        int k = size / typeCount;

        for (int i = 0; i < k; i++)
        {
            for (int j = 0; j < typeCount; j++)
            {
                randomTileTypePool.Add(TileMemo.CastToBtnType(j));
            }
        }
        List<BtnType> tempPool = new List<BtnType>();
        for (int i = 0; i < typeCount; i++) tempPool.Add(TileMemo.CastToBtnType(i));

        size %= typeCount;

        while (size > 0 && tempPool.Count > 0)
        {
            int rand = UnityEngine.Random.Range(0, tempPool.Count);

            randomTileTypePool.Add(tempPool[rand]);
            tempPool.RemoveAt(rand);

            --size;

        }

    }
    //get a random BtnType from a random pool
    public BtnType GetRandomFromPool()
    {
        if (randomTileTypePool.Count <= 0) return BtnType.red;

        int rand = UnityEngine.Random.Range(0, randomTileTypePool.Count);
        BtnType randTypeVal = randomTileTypePool[rand];
        randomTileTypePool.RemoveAt(rand);

        return randTypeVal;
    }
    public List<TileMemo> GetRandomBtn(int count = 1)
    {
        List<int> randomIndexPool = Enumerable.Range(0, tileMemos.Count).ToList();

        List<TileMemo> resultTileMemoList = new List<TileMemo>();

        while (count > 0 && randomIndexPool.Count > 0)
        {
            int rand = UnityEngine.Random.Range(0, randomIndexPool.Count);

            int pickedNum = randomIndexPool[rand];

            randomIndexPool.RemoveAt(rand);

            TileMemo pickedTileBtn = tileMemos[pickedNum].GetComponent<TileMemo>();
            if (!pickedTileBtn.IsRevealed()) resultTileMemoList.Add(pickedTileBtn);

            count--;
        }

        return resultTileMemoList;
    }

    const float TILES_REVEAL_TIME = 2.0f;

    //flag telling if the current level is Level Infinity, for endless gameplay
    bool isPlayingLevelInfinity = false;

    //start game

    public void GameStart()
    {
        soundManager.Play("button1_sound");
        if (selectedLevel == MAX_LEVEL_COUNT)
        {
            stage = 0;
            infinityLevelScore = 0;
            isPlayingLevelInfinity = true;
            GameLevelInfinityStart();
        }
        else
        {
            GameStart(selectedLevel);
            isPlayingLevelInfinity = false;
        }
    }

    float guessTimeTaken = 0f;

    public void GameStart(int selectedLvl)
    {
        currentPhase = 0;

        if (lobbyCanvas.gameObject.activeSelf)
        {
            GameObject.Find("LobbyBackground").GetComponent<ToSongBeatAnimation>().StopAnimation();
            GameObject.Find("LogoImage").GetComponent<ToSongBeatAnimation>().StopAnimation();
            lobbyCanvas.gameObject.SetActive(false);
        }

        gameCanvas.gameObject.SetActive(true);

        soundManager.StopCurrentMusic();

        --selectedLvl;

        //disable start button
        buttonStart.SetActive(false);

        //enable gameplay gui objects
        foreach (var eachObj in gameplayGuiObjects) eachObj.SetActive(true);

        //initialize lives & heart containers
        heartContainer.GetComponent<HeartContainer>().ResetLives(3);

        //set text for level display text
        levelDisplayText.GetComponent<Text>().text = string.Format("Level {0}", (selectedLvl + 1));

        //create tiles
        ResetRandomPool((int)(levelSettings[selectedLvl, 1] * levelSettings[selectedLvl, 1]), (int)levelSettings[selectedLvl, 0]);
        CreateButtonsArray((int)levelSettings[selectedLvl, 1], (int)levelSettings[selectedLvl, 1]);

        //enable countdown bar
        countdownBar.SetActive(true);

        //play gameplay song and set backgroud depend on the level
        if (selectedLvl < 10)
        {
            gameBackground.GetComponent<UnityEngine.UI.Image>().sprite = backgroundSprites[0];
            soundManager.PlayMusic("levelnormal_song", 1f, true);
        }
        else
        {
            soundManager.PlayMusic("levelhard_song", 1f, true);
            gameBackground.GetComponent<UnityEngine.UI.Image>().sprite = backgroundSprites[1];
        }
        //start the 1st phase
        currentPhaseCoroutine = StartCoroutine(RevealPhaseStart((int)levelSettings[selectedLvl, 2],
            TILES_REVEAL_TIME,
            levelSettings[selectedLvl, 3],
            selectedLvl));
    }

    int currentPhase;
    Coroutine currentPhaseCoroutine;
    //first phase: reveal phase
    IEnumerator RevealPhaseStart(int revealCount, float revealTime, float totalTime, int selectedLvl)
    {
        if (!skipButton.activeSelf) skipButton.SetActive(true);

        currentPhase = 1;
        //create tile list shown what tile player supposed to choose

        tileRequestList.CreateQuestionList((int)levelSettings[selectedLvl, 5], (int)levelSettings[selectedLvl, 0]);

        List<TileMemo> tileMemosList;

        countdownBar.GetComponent<CountdownBarTimer>().Countdown(totalTime);

        //disable all tiles
        foreach (GameObject eachTileMemo in tileMemos)
        {
            eachTileMemo.GetComponent<TileMemo>().Disable();
        }

        float realRevealTime = revealTime * 0.8f;
        float delayRevealTime = revealTime * 0.2f;

        //loop
        while (totalTime > 0.0f && currentPhase == 1)
        {
            //get random tiles
            tileMemosList = GetRandomBtn(revealCount);

            //reveal those tiles
            foreach (var eachTileMemo in tileMemosList)
            {
                eachTileMemo.RevealButton();
            }

            soundManager.Play("showtiles_sound");

            //delay
            yield return new WaitForSeconds(realRevealTime);

            //hide all tiles
            foreach (var eachTileMemo in tileMemosList)
            {
                eachTileMemo.HideButton();
            }

            //delay
            yield return new WaitForSeconds(delayRevealTime);
            totalTime -= revealTime;
        }

        //guess phase start
        currentPhaseCoroutine = StartCoroutine(GuessPhaseStart(levelSettings[selectedLvl, 4], selectedLvl));
    }

    //guess tile
    public void GuessClickedTile(BtnType guessBtnType)
    {
        var heartContainerComponent = heartContainer.GetComponent<HeartContainer>();
        switch (tileRequestList.Guess(guessBtnType))
        {
            case -1:
                heartContainerComponent.RemoveLife();
                if (heartContainerComponent.GetLives() <= 0)
                {
                    gameEndFlag = -1;
                    soundManager.Play("gameover_sound");
                }
                else soundManager.Play("incorrect_sound");
                break;
            case 0:
                soundManager.Play("correct_sound");
                break;
            case 1:
                gameEndFlag = 1;
                soundManager.Play("victory_sound");
                break;

        }
    }

    //flag variable to tell if the game is ended yet and ended in which state (end or lose)
    private int gameEndFlag;

    //guess phase start
    IEnumerator GuessPhaseStart(float guessTime, int selectedLvl)
    {
        guessTimeTaken = 0f;

        if (skipButton.activeSelf) skipButton.SetActive(false);

        currentPhase = 2;
        //countdown bar restart
        countdownBar.GetComponent<CountdownBarTimer>().Countdown(guessTime);

        //reveal the first request
        tileRequestList.RevealCurrent();

        //reset flag
        gameEndFlag = 0;

        //enable all tiles
        foreach (var eachTileMemo in tileMemos)
        {
            eachTileMemo.GetComponent<TileMemo>().Enable();
        }

        //while the game is not ended yet
        while (gameEndFlag == 0)
        {
            //if the time runs out, result in lose
            if (guessTime <= 0f)
            {
                gameEndFlag = -1;
                break;
            }
            guessTimeTaken += Time.deltaTime;
            guessTime -= Time.deltaTime;
            yield return null;
        }

        //stop countdown bar
        countdownBar.GetComponent<CountdownBarTimer>().SkipCountdown();

        //disable and reveal all tiles
        foreach (var eachTileMemo in tileMemos)
        {
            eachTileMemo.GetComponent<TileMemo>().RevealButton();
            eachTileMemo.GetComponent<TileMemo>().Disable();
        }

        soundManager.StopCurrentMusic();

        //unlock new level if win the game while playing on the most difficult level unlocked
        if (gameEndFlag > 0 && lastLevelUnlocked < MAX_LEVEL_COUNT && selectedLvl == lastLevelUnlocked) UnlockLevel();

        StartCoroutine(GameResultPanelTransition());

        countdownBar.SetActive(false);

    }

    private void UnlockLevel()
    {
        var dropdown = levelSelectDropdown.GetComponent<Dropdown>();
        ++lastLevelUnlocked;

        saveManager.SetUnlockedLevelCount(lastLevelUnlocked);
        Dropdown.OptionData newLvlOptionData = new Dropdown.OptionData();

        if (lastLevelUnlocked == MAX_LEVEL_COUNT)
        {
            newLvlOptionData.text = "Level Infinity";
            newLvlOptionData.image = Resources.Load<Sprite>("Sprites/level_infinity_option");

        } else
        {

            newLvlOptionData.text = "Level " + lastLevelUnlocked;
            newLvlOptionData.image = null;
        }

        dropdown.options.Add(newLvlOptionData);
    }
    //skip the phase
    public void SkipRevealPhase()
    {
        if (isPlayingLevelInfinity)
        {
            InfinityLevelSkipRevealPhase();
            return;
        }
        if (currentPhase != 1) return;
        countdownBar.GetComponent<CountdownBarTimer>().SkipCountdown();
        StopCoroutine(currentPhaseCoroutine);

        //hide all tiles
        foreach (var eachTileMemo in tileMemos)
        {
            eachTileMemo.GetComponent<TileMemo>().HideButton();
        }

        currentPhaseCoroutine = StartCoroutine(GuessPhaseStart(levelSettings[selectedLevel-1, 4], selectedLevel));
    }

    /*
     * tile type counts                   : levelSettings[i, 0]
     * tile rect size                     : levelSettings[i, 1]
     * number of tile revealed each times : levelSettings[i, 2]
     * total reveal time                  : levelSettings[i, 3]
     * guess time                         : levelSettings[i, 4]
     * guesses count in sequence          : levelSettings[i, 5]
    */
    readonly float[,] levelSettings = new float[MAX_LEVEL_COUNT, 6];

    void SetLevelSettings(int level, float tileTypeCount, float tileRectSize, float revealCount, float totalRevealTime, float guessTime, float guessCount)
    {
        levelSettings[level, 0] = tileTypeCount;
        levelSettings[level, 1] = tileRectSize;
        levelSettings[level, 2] = revealCount;
        levelSettings[level, 3] = totalRevealTime;
        levelSettings[level, 4] = guessTime;
        levelSettings[level, 5] = guessCount;
    }

    void InitializeLevelSettings()
    {
        SetLevelSettings(0, 2, 2, 2, 20f, 20f, 1);
        SetLevelSettings(1, 2, 3, 3, 20f, 10f, 1);
        SetLevelSettings(2, 3, 3, 3, 20f, 20f, 1);
        SetLevelSettings(3, 3, 3, 3, 20f, 10f, 1);
        SetLevelSettings(4, 3, 3, 3, 20f, 20f, 2);
        SetLevelSettings(5, 3, 4, 5, 20f, 20f, 2);
        SetLevelSettings(6, 3, 4, 5, 20f, 20f, 3);
        SetLevelSettings(7, 4, 4, 5, 20f, 30f, 2);
        SetLevelSettings(8, 4, 4, 5, 20f, 15f, 2);
        SetLevelSettings(9, 4, 4, 5, 20f, 30f, 3);
        SetLevelSettings(10, 4, 4, 5, 20f, 20f, 2);
        SetLevelSettings(11, 5, 5, 7, 25f, 40f, 2);
        SetLevelSettings(12, 5, 5, 7, 25f, 20f, 2);
        SetLevelSettings(13, 5, 5, 7, 25f, 40f, 3);
        SetLevelSettings(14, 5, 5, 7, 30f, 45f, 4);
        SetLevelSettings(15, 5, 5, 7, 30f, 45f, 4);
    }

    IEnumerator GameResultPanelTransition()
    {
        gameResultPanel.SetActive(true);

        var backButton = GameObject.Find("BackButton").GetComponent<UnityEngine.UI.Button>();
        var restartButton = GameObject.Find("RestartButton").GetComponent<UnityEngine.UI.Button>();

        var canvasGroup = gameResultPanel.GetComponent<CanvasGroup>();

        restartButton.onClick.RemoveAllListeners();

        if (gameEndFlag > 0)
        {
            string str = "You cleared this level in " + Math.Round(guessTimeTaken) + " second(s).\n";
            if (guessTimeTaken < 5f && heartContainer.GetComponent<HeartContainer>().GetLives() >= 3)
                str += "Excellent!";
            else if (guessTimeTaken < 20f)
                str += "Good job!";
            else
                str += "Not bad.";
            gameResultPanel.transform.Find("ExtraText").GetComponent<Text>().text = str;
            if (selectedLevel < MAX_LEVEL_COUNT - 1)
            {
                restartButton.onClick.AddListener(NextLevel);
                restartButton.gameObject.transform.Find("Text").GetComponent<Text>().text = "Next Level"; 
            } else
            {
                restartButton.onClick.AddListener(RestartLevel);
            }
        }
        else
        {
            string str;
            if (isPlayingLevelInfinity)
            {
                str = "Your total score is " + infinityLevelScore;
                if (UpdateHighscore(infinityLevelScore))
                    str += "\nThis is your new best score so far. Awesome!";
            } else
            {
                str = UnityEngine.Random.Range(1, 20) == 1 ?
                    "oof that sucks, u too lmao sry" :
                    "Do not worry, you can try again, Memorization is the key!";
            }
            gameResultPanel.transform.Find("ExtraText").GetComponent<Text>().text = str;
            restartButton.onClick.AddListener(RestartLevel);
            restartButton.gameObject.transform.Find("Text").GetComponent<Text>().text = "Restart";
        }

        backButton.enabled = false;
        restartButton.enabled = false;

        gameResultPanel.transform.Find("ResultText").GetComponent<Text>().text = 
            gameEndFlag > 0 ? "You Win!" : "You Lose";

        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        backButton.enabled = true;
        restartButton.enabled = true;
        yield return null;
    }

    public void ReturnToLobby()
    {
        soundManager.Play("button1_sound");

        GameResultPanelClose();
        ResetGame();

        lobbyCanvas.gameObject.SetActive(true);
        gameCanvas.gameObject.SetActive(false);

        //disable gameplay gui object
        foreach (var eachObj in gameplayGuiObjects) eachObj.SetActive(false);

        //enable start button
        buttonStart.SetActive(true);
        buttonStart.GetComponent<UnityEngine.UI.Button>().enabled = true;

        soundManager.PlayMusic("lobby_song", 0.5f, true);
        GameObject.Find("LogoImage").GetComponent<ToSongBeatAnimation>().StartAnimation(0);
        GameObject.Find("LobbyBackground").GetComponent<ToSongBeatAnimation>().StartAnimation(1);
    }

    public void RestartLevel()
    {
        soundManager.Play("button1_sound");

        GameResultPanelClose();
        ResetGame();
        if (isPlayingLevelInfinity)
        {
            stage = 0;
            infinityLevelScore = 0;
            GameLevelInfinityStart();
        }
        else GameStart(selectedLevel);
    }
    public void NextLevel()
    {
        soundManager.Play("button1_sound");

        GameResultPanelClose();
        ResetGame();
        GameStart(++selectedLevel);
    }

    void GameResultPanelClose()
    {
        gameResultPanel.GetComponent<CanvasGroup>().alpha = 0f;
        gameResultPanel.SetActive(false);
    }

    void ResetGame()
    {
        //destroy tiles 
        while (tileMemos.Count > 0)
        {
            GameObject.Destroy(tileMemos[0]);
            tileMemos.RemoveAt(0);
        }
        //reset tile request list
        tileRequestList.ResetList();

    }

    void ResultLostInvoke() => gameEndFlag = -1;
    void ResultWinInvoke() => gameEndFlag = 1;

    public void ChangeSelectedLevel(Dropdown dropdown)
    {
        int v = dropdown.value + 1;
        selectedLevel = v;
        if (selectedLevel == MAX_LEVEL_COUNT)
        {
            highscorePanel.SetActive(true);
            highscorePanel.GetComponentInChildren<Text>().text = "Your Highscore: " + saveManager.GetHighscore();
        } else
        {
            highscorePanel.SetActive(false);
        }
    }
    public void ExitGame()
    {
        soundManager.Play("button1_sound");
        Application.Quit();
    }

    int stage = 0;

    int infinityLevelScore = 0;

    void GameLevelInfinityStart()
    {
        //keeping track how many stages the player passes; the difficulty also increase depend on the current stage playing
        ++stage;

        guessTimeTaken = 0f;

        int difficulty = Math.Min(stage, MAX_LEVEL_COUNT - 1);

        currentPhase = 0;

        if (lobbyCanvas.gameObject.activeSelf)
        {
            GameObject.Find("LobbyBackground").GetComponent<ToSongBeatAnimation>().StopAnimation();
            GameObject.Find("LogoImage").GetComponent<ToSongBeatAnimation>().StopAnimation();
            lobbyCanvas.gameObject.SetActive(false);
        }

        gameCanvas.gameObject.SetActive(true);

        soundManager.StopCurrentMusic();

        //disable start button
        buttonStart.SetActive(false);

        //disable lobby gui objects : foreach (var eachObj in lobbyGuiObjects) eachObj.SetActive(false);

        //enable gameplay gui objects
        foreach (var eachObj in gameplayGuiObjects) eachObj.SetActive(true);

        //initialize lives & heart containers only the first stage of the game
        if (stage==1) heartContainer.GetComponent<HeartContainer>().ResetLives(5);

        //set text for level display text
        levelDisplayText.GetComponent<Text>().text = "Stage " + stage;

        //create tiles
        ResetRandomPool((int)(levelSettings[difficulty - 1, 1] * levelSettings[difficulty - 1, 1]), (int)levelSettings[difficulty - 1, 0]);
        CreateButtonsArray((int)levelSettings[difficulty - 1, 1], (int)levelSettings[difficulty - 1, 1]);

        //enable countdown bar
        countdownBar.SetActive(true);

        //play gameplay song and background
        soundManager.PlayMusic("levelinfinity_song", 1f, true);
        gameBackground.GetComponent<UnityEngine.UI.Image>().sprite = backgroundSprites[1];

        //start the 1st phase
        currentPhaseCoroutine = StartCoroutine(LevelInfinityRevealPhaseStart((int)levelSettings[difficulty - 1, 2], TILES_REVEAL_TIME, levelSettings[difficulty - 1, 3], difficulty - 1));
    }

    Coroutine levelInfinityTransitionCoroutine;

    IEnumerator LevelInfinityNextTransition()
    {

        gameResultPanel.SetActive(true);

        var canvasGroup = gameResultPanel.GetComponent<CanvasGroup>();

        var backButton = gameResultPanel.transform.Find("BackButtonGroup").gameObject;
        var restartButton = gameResultPanel.transform.Find("RestartButtonGroup").gameObject;

        string str = "You have passed Stage " + stage + "\nYour current score is now " + infinityLevelScore;
        Transform extraText = gameResultPanel.transform.Find("ExtraText");
        extraText.GetComponent<Text>().text = str;

        backButton.SetActive(false);
        restartButton.SetActive(false);

        gameResultPanel.transform.Find("ResultText").GetComponent<Text>().text = "You pass!";

        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += 2f * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1f);

        string originalStr = extraText.GetComponent<Text>().text;
        int timerSec = 3;
        while (timerSec > 0)
        {
            extraText.GetComponent<Text>().text = originalStr + "\n\n Next stage will begin in " + timerSec + " second(s).";
            timerSec -= 1;
            yield return new WaitForSeconds(1f);
        }

        canvasGroup.alpha = 0;

        backButton.SetActive(true);
        restartButton.SetActive(true);

        gameResultPanel.SetActive(false);

        ResetGame();
        GameLevelInfinityStart();
    }

    void LevelInfinityQuit()
    {
        StopCoroutine(levelInfinityTransitionCoroutine);
        ReturnToLobby();
    }

    //first infinity phase: reveal phase
    IEnumerator LevelInfinityRevealPhaseStart(int revealCount, float revealTime, float totalTime, int difficulty)
    {
        if (!skipButton.activeSelf) skipButton.SetActive(true);

        currentPhase = 1;
        //create tile list shown what tile player supposed to choose

        tileRequestList.CreateQuestionList((int)levelSettings[difficulty, 5], (int)levelSettings[difficulty, 0]);

        List<TileMemo> tileMemosList;

        countdownBar.GetComponent<CountdownBarTimer>().Countdown(totalTime);

        //disable all tiles
        foreach (GameObject eachTileMemo in tileMemos)
        {
            eachTileMemo.GetComponent<TileMemo>().Disable();
        }

        float realRevealTime = revealTime * 0.8f;
        float delayRevealTime = revealTime * 0.2f;

        //loop
        while (totalTime > 0.0f && currentPhase == 1)
        {
            //get random tiles
            tileMemosList = GetRandomBtn(revealCount);

            //reveal those tiles
            foreach (var eachTileMemo in tileMemosList)
            {
                eachTileMemo.RevealButton();
            }

            soundManager.Play("showtiles_sound");

            //delay
            yield return new WaitForSeconds(realRevealTime);

            //hide all tiles
            foreach (var eachTileMemo in tileMemosList)
            {
                eachTileMemo.HideButton();
            }

            //delay
            yield return new WaitForSeconds(delayRevealTime);
            totalTime -= revealTime;
        }

        //guess phase start
        currentPhaseCoroutine = StartCoroutine(LevelInfinityGuessPhaseStart(levelSettings[difficulty, 4], difficulty));
    }

    //guess phase start
    IEnumerator LevelInfinityGuessPhaseStart(float guessTime, int difficulty)
    {
        if (skipButton.activeSelf) skipButton.SetActive(false);

        currentPhase = 2;
        //countdown bar restart
        countdownBar.GetComponent<CountdownBarTimer>().Countdown(guessTime);

        //reveal the first request
        tileRequestList.RevealCurrent();

        //reset flag
        gameEndFlag = 0;

        //enable all tiles
        foreach (var eachTileMemo in tileMemos)
        {
            eachTileMemo.GetComponent<TileMemo>().Enable();
        }

        //while the game is not ended yet
        while (gameEndFlag == 0)
        {
            //if the time runs out, result in lose
            if (guessTime <= 0f)
            {
                ResultLostInvoke();
                break;
            }

            guessTimeTaken += Time.deltaTime;
            guessTime -= Time.deltaTime;

            yield return null;
        }

        //stop countdown bar
        countdownBar.GetComponent<CountdownBarTimer>().SkipCountdown();

        //disable and reveal all tiles
        foreach (var eachTileMemo in tileMemos)
        {
            eachTileMemo.GetComponent<TileMemo>().RevealButton();
            eachTileMemo.GetComponent<TileMemo>().Disable();
        }

        soundManager.StopCurrentMusic();

        if (gameEndFlag < 0)
        {
            StartCoroutine(GameResultPanelTransition());
        }
        else if (gameEndFlag > 0)
        {
            infinityLevelScore += (Mathf.RoundToInt(guessTime * 100f) + 500);
            levelInfinityTransitionCoroutine = StartCoroutine(LevelInfinityNextTransition());
        }

        countdownBar.SetActive(false);

    }

    public void InfinityLevelSkipRevealPhase()
    {
        if (currentPhase != 1) return;

        countdownBar.GetComponent<CountdownBarTimer>().SkipCountdown();
        StopCoroutine(currentPhaseCoroutine);

        //hide all tiles
        foreach (var eachTileMemo in tileMemos)
        {
            eachTileMemo.GetComponent<TileMemo>().HideButton();
        }

        int difficulty = Math.Min(stage, MAX_LEVEL_COUNT - 1);
        currentPhaseCoroutine = StartCoroutine(LevelInfinityGuessPhaseStart(levelSettings[difficulty - 1, 4], difficulty));
    }

    bool UpdateHighscore(int score)
    {
        if (score > saveManager.GetHighscore())
        {
            saveManager.SetHighscore(score);
            highscorePanel.GetComponentInChildren<Text>().text = "Your Highscore: " + score;
            return true;
        }
        return false;
    }

    void UnlockLevels(int count)
    {
        while (count > 0 && lastLevelUnlocked <= MAX_LEVEL_COUNT)
        {
            UnlockLevel();
            count--;
        }
    }
}
