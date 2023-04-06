using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonUI : MonoBehaviour
{
    [SerializeField] public Sprite _newButtonSprite, _oldButtonSprite;
    public new AudioSource audio;

    public static Sprite _newButtonSpriteCopy, _oldButtonSpriteCopy;
    public Button button;
    public static Button buttonCopy;

    public ButtonUI() 
    {
        GridManager._simStarted = false;
        _newButtonSpriteCopy = _newButtonSprite;
        _oldButtonSpriteCopy = _oldButtonSprite;
        buttonCopy = button;
    }
    
    public void startstopsim(bool a)
    {
        
        GridManager.maxGenerations = (Int32.TryParse(GridManager.generationInputField.text, out var maxGeneration)) ? maxGeneration : int.MaxValue;
        if (!a)
        {
            GridManager._simStarted = false;
            button.image.sprite = _oldButtonSprite;
            //Debug.Log(GridManager._simStarted);
        }
        else
        {
            if (GridManager.allTilesDead)
            {
                GameObject.Find("GridManager").GetComponent<GridManager>().CalculateDestinationStates();
                if(GridManager.continuesim)
                {
                    GridManager.allTilesDead = false;
                    GridManager._simStarted = true;
                }
                GridManager.continuesim = false;
            }
            else { GridManager._simStarted = (GridManager._simStarted) ? false : true; }
            
            button.image.sprite = (button.image.sprite == _oldButtonSprite && GridManager._simStarted == true) ? _newButtonSprite : _oldButtonSprite;
            audio.Play();
        }
    }
}
