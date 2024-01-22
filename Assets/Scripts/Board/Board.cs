using System.Collections.Generic;
using UnityEngine;
using System.IO;
[System.Serializable]
public class CustomBoardConfig
{
    public TileType[] customBoardConfig;
    public int width;
    public int height;
}



public class Board : MonoBehaviour
{
    public static Board instance;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Transform cam;
    [SerializeField] private Troop[] troopPrefab;
    [SerializeField] private Vector2Int[] troopPos;
    public int boardWidth,boardHeight;
    private Dictionary<Vector2, Tile> tiles;
    private List<Troop> troops;
    private TileType[,] customBoardConfig = {
        { TileType.Plains, TileType.Plains, TileType.Plains, TileType.Plains, TileType.Plains, TileType.Plains, TileType.Plains, TileType.Plains },
        { TileType.Plains, TileType.Forest, TileType.Plains, TileType.Plains, TileType.Plains, TileType.Plains, TileType.Plains, TileType.Plains },
        { TileType.Plains, TileType.Forest, TileType.Plains, TileType.Plains, TileType.Plains, TileType.Plains, TileType.Plains, TileType.Plains },
        { TileType.Plains, TileType.Forest, TileType.Plains, TileType.Mountain, TileType.Plains, TileType.Plains, TileType.Plains, TileType.Plains },
        { TileType.Plains, TileType.Forest, TileType.Mountain, TileType.Mountain, TileType.Plains, TileType.Plains, TileType.Plains, TileType.Plains },
        { TileType.Plains, TileType.Plains, TileType.Plains, TileType.Mountain, TileType.Plains, TileType.Plains, TileType.Plains, TileType.Plains },
        { TileType.Plains, TileType.Ruins, TileType.Ruins, TileType.Ruins, TileType.Ruins, TileType.Plains, TileType.Plains, TileType.Plains },
        { TileType.Plains, TileType.Plains, TileType.Plains, TileType.Plains, TileType.Plains, TileType.Plains, TileType.Plains, TileType.Plains },
    };
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        
        LoadCustomBoardConfig();
        //SaveToJson();
        SpawnTroops();
        
    }
    // void SaveToJson(){
    //     int width = customBoardConfig.GetLength(1);
    //     int height = customBoardConfig.GetLength(0);

    //     // Flatten the 2D array into a 1D array
    //     TileType[] flattenedArray = new TileType[width * height];
    //     for (int y = 0; y < height; y++)
    //     {
    //         for (int x = 0; x < width; x++)
    //         {
    //             flattenedArray[y * width + x] = customBoardConfig[y, x];
    //         }
    //     }

    //     // Create an instance of the serializable class
    //     CustomBoardConfig boardConfigData = new CustomBoardConfig
    //     {
    //         customBoardConfig = flattenedArray,
    //         width = width,
    //         height = height
    //     };

    //     string customBoard = JsonUtility.ToJson(boardConfigData);
    //     System.IO.File.WriteAllText(Application.streamingAssetsPath + "/customBoardConfig.json", customBoard);
    // }

    void LoadCustomBoardConfig()
    {
        
        string jsonPath = Path.Combine(Application.streamingAssetsPath, "CustomBoardConfig.json");
        if (File.Exists(jsonPath))
        {
            string jsonContent = File.ReadAllText(jsonPath);
            CustomBoardConfig customConfig = JsonUtility.FromJson<CustomBoardConfig>(jsonContent);
            // Process the loaded configuration
            ProcessCustomConfig(customConfig);
        }
        else
        {
            Debug.LogError("CustomBoardConfig.json not found!");
        }
    }

    void ProcessCustomConfig(CustomBoardConfig customConfig)
    {
        int height = customConfig.height;
        int width = customConfig.width;

        boardHeight = customConfig.height;
        boardWidth = customConfig.width;

        tiles = new Dictionary<Vector2, Tile>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int tileTypeIndex = (int)customConfig.customBoardConfig[y * width + x];
                TileType tileType = (TileType)tileTypeIndex;

                var spawnedTile = Instantiate(tilePrefab, new Vector3(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";

                spawnedTile.Init(tileType, new Vector2Int(x, y));

                tiles[new Vector2(x, y)] = spawnedTile;
            }
        }

        cam.transform.position = new Vector3((float)width / 2 - 0.5f, (float)height / 2 - 0.5f, -10);
    }
    void SpawnTroops()
    {
        troops = new List<Troop>();

        for(int i= 0; i< troopPos.Length; i++){
            Tile spawnTile = GetTileAtPosition(troopPos[i]);

            if (spawnTile != null)
            {
                Troop spawnedTroop = Instantiate(troopPrefab[i], spawnTile.transform.position, Quaternion.identity);
                spawnedTroop.SetInitialTile(spawnTile);
                troops.Add(spawnedTroop);
            }
        }
    }


    public Tile GetTileAtPosition(Vector2 pos)
    {
        if (tiles.TryGetValue(pos, out var tile))
        {
            return tile;
        }

        return null;
    }
}
