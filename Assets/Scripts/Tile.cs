using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] public Color _baseColor, _offsetColor, _deadlyColor;

    [SerializeField] public SpriteRenderer _renderer;

    [SerializeField] private GameObject _highlight;

    public AudioSource restartButtonAudio;
    public static Color basecolorcopy;

    public TileState currentTileState;
    public TileState futureTileState;

    public static bool hasRestarted = false;
    public bool isDeadly = false;
    public int posX, posY;

    public bool needsChange = true;

    public void Init()
    {
        if (isDeadly)
        {
            
            _renderer.color = _deadlyColor;
        }
        else
        {
            _renderer.color = _baseColor;
            currentTileState = TileState.Dead;
            futureTileState = TileState.Dead;
            basecolorcopy = _baseColor;
        }
        
        

    }
    void OnMouseEnter()
    {
        if (!isDeadly)
        {
            _highlight.SetActive(true);
        }
        
    }
    void OnMouseExit()
    {
        if (!isDeadly)
        {
            _highlight.SetActive(false);
        }
        
    }
    void OnMouseDown()
    {
        if (GridManager._simStarted)
        {
            return;
        }
        if (!isDeadly)
        {
            _renderer.color = (_highlight.activeSelf && _renderer.color == _baseColor && currentTileState == TileState.Dead) ? _offsetColor : _baseColor;
            currentTileState = (currentTileState == TileState.Dead) ? TileState.Alive : TileState.Dead;
            //Debug.Log($"Current tile state: at ({posX},{posY}): {currentTileState}");
            GridManager.tilePlacementAudioStatic.Play();
        }
    }

    public void restartButton()
    {
        //Debug.Log("restartbutton() called");
        restartButtonAudio.Play();
        
        for (int x = 0; x < GridManager.widthCopy; x++)
        {
            for (int y = 0; y < GridManager.heightCopy; y++)
            {
                Vector2 pos = new Vector2(x, y);
                Tile accessedTile = GridManager._tiles[pos];

                if (accessedTile.isDeadly)
                {
                    accessedTile._renderer.color = _deadlyColor;
                    accessedTile.currentTileState = TileState.Deadly;
                    accessedTile.futureTileState = TileState.Deadly;
                }
                else
                {        
                    accessedTile._renderer.color = basecolorcopy;
                    accessedTile.currentTileState = TileState.Dead;
                    accessedTile.futureTileState = TileState.Dead;
                }
                
            }
        }
        GridManager.currentGenerationTextField.text = "0";
        GameObject.Find("StartStopButton").GetComponent<ButtonUI>().startstopsim(false);
        GridManager.hasRestarted = true;
    }
}

public enum TileState
{
    Alive,
    Dead,
    Deadly
}
