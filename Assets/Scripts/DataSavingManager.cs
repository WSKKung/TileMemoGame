using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Skytanet.SimpleDatabase;

public class DataSavingManager : MonoBehaviour
{
    SaveFile saveFile;

    private void Awake()
    {
        saveFile = new SaveFile("LevelInfinitySave");
        saveFile.Close();
    }

    public int GetHighscore()
    {
        if (!saveFile.IsOpen()) saveFile.ReOpen();

        int val;

        if (!saveFile.HasKey("highscore"))
        {
            saveFile.Set("highscore", 0);
        }

        val = saveFile.Get<int>("highscore");

        saveFile.Close();
        return val;
    }

    public void SetHighscore(int score)
    {
        if (!saveFile.IsOpen()) saveFile.ReOpen();
        saveFile.Set("highscore", score);
        saveFile.Close();
    }

    public void SetUnlockedLevelCount(int count)
    {
        if (!saveFile.IsOpen()) saveFile.ReOpen();
        saveFile.Set("unlocked_level_count", count);
        saveFile.Close();
    }

    public int GetUnlockedLevelCount()
    {
        if (!saveFile.IsOpen()) saveFile.ReOpen();

        int val = 1;

        if (!saveFile.HasKey("unlocked_level_count"))
        {
            saveFile.Set("unlocked_level_count", 1);
        }

        val = saveFile.Get<int>("unlocked_level_count");

        saveFile.Close();
        return val;
    }
    private void OnApplicationQuit()
    {
        if (saveFile.IsOpen()) saveFile.Close();
    }
}
