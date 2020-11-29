using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class TileMemo : MonoBehaviour
{
    GameObject gameSystemHandler;

    Sprite hiddenSprite;
    Sprite revealedSprite;
    private Button btn;

    [SerializeField]
    private bool revealed;

    //enum type of TileMemo buttons
    public enum ButtonType
    {
        red,
        blue,
        green,
        yellow,
        purple
    }

    //current type of the button
    public ButtonType btnType;


    //Initialize TileMemo Button with a given btnType
    public void Initialize(ButtonType initType = ButtonType.red)
    {
        gameSystemHandler = GameObject.Find("GameSystemHandler");

        btn = GetComponent<Button>();

        btn.onClick.AddListener(ButtonClicked);

        hiddenSprite = Resources.Load<Sprite>("Sprites/tilememo/tilememo_empty");
        revealedSprite = Resources.Load<Sprite>("Sprites/tilememo/normal/tilememo_" + initType);

        btnType = initType;

        btn.image.sprite = hiddenSprite;

        revealed = false;

        Disable();
    }

    void ButtonClicked()
    {
        gameSystemHandler.GetComponent<GameSystem>().GuessClickedTile(btnType);

        RevealButton();
        Disable();
    }

    public void ResetButtonType(ButtonType type)
    {
        btn.image.sprite = hiddenSprite;
        Disable();
        HideButton();
        if (btnType != type)
        {
            btnType = type;
            revealedSprite = Resources.Load<Sprite>("Sprites/tilememo/normal/tilememo_" + btnType);
        }
    }

    public void RevealButton()
    {
        btn.image.sprite = revealedSprite;
        revealed = true;
    }

    public void HideButton()
    {
        if (!revealed) return;
        btn.image.sprite = hiddenSprite;
        revealed = false;
    }

    public bool IsRevealed()
    {
        return revealed;
    }
    
    public static ButtonType CastToBtnType(int n)
    {
        switch (n)
        {
            case 1: return ButtonType.blue;
            case 2: return ButtonType.green;
            case 3: return ButtonType.yellow;
            case 4: return ButtonType.purple;
            default: return ButtonType.red;
        }
    }

    public void Enable() => btn.interactable = true;
    public void Disable() => btn.interactable = false;
}
