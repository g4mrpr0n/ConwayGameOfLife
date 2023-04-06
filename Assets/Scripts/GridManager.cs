using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [SerializeField] public int _width, _height;

    [SerializeField] private Tile _tilePrefab;

    [SerializeField] private Transform _cam;

     public static bool _simStarted;

    public AudioSource tilePlacementAudio;
    public static AudioSource tilePlacementAudioStatic;

    private int neighborCount = 0;
    public static int maxGenerations;
    public int currentGeneration;

    public static Dictionary<Vector2, Tile> _tiles;
    public static Dictionary<Vector2, int> probabilities;
    public float _spacingX, _spacingY, _paddingX, _paddingY;
    public static int widthCopy, heightCopy;
    public static bool hasRestarted = false;

    private float timer = 0;
    private readonly float stateChangeTime = 1;

    public static bool allTilesDead = false, continuesim=true;
    private bool reset = true;

    public static Text generationInputField;
    public static Text currentGenerationTextField;

    void Start()
    {
        tilePlacementAudioStatic = tilePlacementAudio;
        heightCopy = _height;
        widthCopy = _width;
        GenerateGrid();
        currentGenerationTextField = GameObject.Find("GenerationCountNumber").GetComponent<Text>();
        generationInputField = GameObject.Find("GenerationNumberInputFieldText").GetComponent<Text>();
    }

    private void Update()
    {  
        if (currentGeneration >= maxGenerations || allTilesDead == true)
        {
            if (currentGeneration < maxGenerations)

            {
                currentGenerationTextField.text = "0";
                currentGeneration = 0;
            }
            if(reset)
            {
                GameObject.Find("StartStopButton").GetComponent<ButtonUI>().startstopsim(false);
                reset = false;
            }
            _simStarted = false;
        }
        if (hasRestarted)
        {
            currentGeneration = 0;
            hasRestarted = false;
        }
        if (_simStarted)
        {
            timer += Time.deltaTime;
            if (timer >= stateChangeTime)
            {
                CalculateDestinationStates();
                UpdateTileState();
                timer = 0;
                currentGeneration++;
                currentGenerationTextField.text = currentGeneration.ToString();
            }
        }
    }

    void GenerateGrid()
    {
        _tiles = new Dictionary<Vector2, Tile>();
        probabilities = new Dictionary<Vector2, int>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _spacingX = 0.7f; _spacingY = 0.7f;
                _paddingX = 2.4f; _paddingY = 2.25f;
                var spawnedTile = Instantiate(_tilePrefab, new Vector3((x * _spacingX) + _paddingX, (y * _spacingY) + _paddingY), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";

                if (x > widthCopy - 6 && y > heightCopy - 6)
                {
                    spawnedTile.isDeadly = true;
                    spawnedTile.currentTileState = TileState.Deadly;
                    spawnedTile.futureTileState = TileState.Deadly;
                }

                spawnedTile.Init();
                spawnedTile.posX = x;
                spawnedTile.posY = y;

                Vector2 pos = new Vector2(x, y);
                _tiles[pos] = spawnedTile;
                
            }
        }
        _cam.transform.position = new Vector3((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f, -10);
    }

    public Tile GetTileAtPosition(Vector2 pos)
    {
        if (_tiles.TryGetValue(pos, out var tile))
        {
            return tile;
        }
        return null;
    }

    private int CheckTileAtPosition(int x, int y)
    {
        if (x < 0 || y < 0 || x >= _width || y >= _height)
        {
            return -1;
        }
        
        Tile accessedTile = GetTileAtPosition(new Vector2(x, y));
        if (accessedTile.currentTileState == TileState.Alive)
        {
            return 1;
        }
        else if (accessedTile.currentTileState == TileState.Dead )
        {
            return 0;
        }
        return 2;
    }

    public void CalculateDestinationStates()
    {
        reset = true;
        neighborCount = 0;
        allTilesDead = true;
        bool foundposition;
        bool teleported;
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                neighborCount = 0;
                foundposition = false;
                teleported = false;
                Vector2 pos = new Vector2(x, y);
                Tile currentTile = _tiles[pos];
                if (!currentTile.needsChange) //tile has already been determined by teleportation mechancis
                {
                    continue;
                }
                int minX = x - 1, maxX = x + 1, minY = y - 1, maxY = y + 1;
                for (int m = minX; m <= maxX; m++)
                {
                    for (int n = minY; n <= maxY; n++)
                    {
                        if (m == x && n == y)
                        {
                            continue;
                        }
                        else
                        {
                            if (CheckTileAtPosition(m, n) == 1)
                            {
                                neighborCount++;
                            }
                        }
                    }
                }

                if(currentTile.currentTileState == TileState.Alive && currentTile.isDeadly) //if the tile is alive but deadly, we know it will be deadly no matter what so we can get this out of the way.
                {
                    currentTile.futureTileState = TileState.Deadly;
                    continue;
                }

                probabilities[pos] = UnityEngine.Random.Range(0, 2);
                if (probabilities[pos] == 1 && (currentTile.currentTileState==TileState.Alive && !currentTile.isDeadly))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        int randX = UnityEngine.Random.Range(0, widthCopy);
                        int randY = UnityEngine.Random.Range(0, heightCopy);
                        if (CheckTileAtPosition(randX, randY) == 0)
                        {
                            continuesim = true;
                            teleported=true;
                            Tile nextTile = _tiles[new Vector2(randX, randY)];
                            nextTile.needsChange = false;                          
                            nextTile.futureTileState = TileState.Alive;
                            currentTile.futureTileState = TileState.Dead;
                            break;
                        }
                    }
                }
               else if (neighborCount == 2 && (currentTile.currentTileState == TileState.Alive && !currentTile.isDeadly))
                {
                    
                   for (int i = 0; i < 9; i++)
                   {
                       int randx = UnityEngine.Random.Range(minX, maxX);
                       int randy = UnityEngine.Random.Range(minY, maxY);
                       if (CheckTileAtPosition(randx, randy) == 0)
                       {
                           continuesim = true;
                           foundposition = true;
                           Tile nextTile = _tiles[new Vector2(randx, randy)];
                           nextTile.needsChange = false;
                           nextTile.futureTileState = TileState.Alive;
                           currentTile.futureTileState = TileState.Dead;
                           break;
                       }
                   }
               }
               else if ((neighborCount == 2 || neighborCount == 3) && currentTile.currentTileState == TileState.Alive && !foundposition && !teleported)
               {
                   
                   continuesim = true;
                   currentTile.futureTileState = TileState.Alive;
               }

                else if (currentTile.currentTileState == TileState.Alive && (neighborCount < 2 || neighborCount > 3) && !foundposition && !teleported)
               {
                   currentTile.futureTileState = TileState.Dead;
               }
               else if (neighborCount == 3 && (currentTile.currentTileState == TileState.Dead || currentTile.currentTileState == TileState.Deadly))
               {
               
                   continuesim = true;
                   currentTile.futureTileState = TileState.Alive;
               }
               
                
            }
        }
    }

    public void UpdateTileState()
    {   
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Vector2 pos = new Vector2(x, y);
                Tile currentTile = _tiles[pos];
                currentTile.needsChange = true; //make all the current tiles need changes again for the new generation
                currentTile.currentTileState = currentTile.futureTileState;

                if (currentTile.currentTileState == TileState.Alive)
                {
                    allTilesDead = false;
                    currentTile._renderer.color = currentTile._offsetColor;
                }
                else if (currentTile.currentTileState == TileState.Dead && !currentTile.isDeadly)
                {
                    currentTile._renderer.color = currentTile._baseColor;
                }
                else if (currentTile.currentTileState == TileState.Deadly && currentTile.isDeadly)
                {
                    currentTile._renderer.color = currentTile._deadlyColor;
                }
            }
        }
    }
}