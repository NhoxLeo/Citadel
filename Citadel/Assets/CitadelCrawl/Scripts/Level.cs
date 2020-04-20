using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Level : MonoBehaviour
{
    [Header("World Options")]
    public int gridSpotSize = 32;
    public Vector2 tilesPerLine = new Vector2(25, 25);
    public int SPAWN_BUFFER = 8; //How far away can things spawn from the player & the portal

    [Header("Room Options")]
    [Space(10)]
    public int MAX_ROOMS = 10, MINIMUM_ROOMS = 8;
    public int MAX_ROOM_SIZE = 6, MINIMUM_ROOM_SIZE = 3;
    public int COLLECTABLE_SPAWN_CHANCE = 3, ENEMY_SPAWN_CHANCE = 2, WEAPON_SPAWN_CHANCE = 3; //Decimal Chance

    [Header("World References")]
    [Space(10)]
    public GameObject worldGeoContainer;
    public GameObject worldEnemiesContainer;
    public GameObject worldWeaponsContainer;
    public GameObject worldCollectablesContainer;
    public GameObject worldNavMeshContainer;

    [Header("Entity References")]
    [Space(10)]
    public GameObject playerPrefab;
    public GameObject map;
    public GameObject wallPrefab;
    public GameObject doorPrefab;
    public GameObject floorPrefab;
    public GameObject portalPrefab;
    public GameObject ceilingPrefab;
    public GameObject collectablePrefab;
    public List<GameObject> enemyPrefabs;
    public GameObject weaponPrefab;

    //World Data
    private int levelIndex = 0;
    private GameObject[,] worldArray;
    private Vector2 WORLD_SIZE;
    private List<Vector2> emptyTileIndices;
    private List<Vector2> doorTileIndices;
    private bool changeRoomColor;
    private Color worldColor = Color.white;
    private bool worldGeoCompleted = false, readyToBuild = false;
    private GameObject worldFloor, worldCeil, player, portal;

    //Room Data
    private List<GameObject> roomItems;
    private Vector2 currentRoom, previousRoom;
    private Vector2 currentRoomSize, previousRoomSize;
    private List<GameObject> roomEnemies;
    private int currentRoomGeneratedIndex;

    // Start is called before the first frame update
    void Start()
    {
        InitLevel();
        StartCoroutine(CreateLevel());
    }

    /*
    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.L))
        {
            StartCoroutine(CreateLevel());
        }

        if (Input.GetKeyUp(KeyCode.K))
        {
            readyToBuild = true;
        }
    }
    */

    private void InitLevel()
    {
        //Set up
        worldArray = new GameObject[(int)tilesPerLine.x, (int)tilesPerLine.y];
        WORLD_SIZE = new Vector2(gridSpotSize * tilesPerLine.x, gridSpotSize * tilesPerLine.y);
        emptyTileIndices = new List<Vector2>();
        doorTileIndices = new List<Vector2>();

        roomItems = new List<GameObject>();
        roomEnemies = new List<GameObject>();
    }

    private void ResetLevel()
    {
        worldGeoCompleted = false;
        currentRoomGeneratedIndex = 0;

        //Clear
        for (int col = 0; col < tilesPerLine.x; col++)
        {
            for (int row = 0; row < tilesPerLine.y; row++)
            {
                Destroy(worldArray[row, col]);
            }
        }

        emptyTileIndices.Clear();
        doorTileIndices.Clear();

        ClearGameObjectList(roomEnemies);
        ClearGameObjectList(roomItems);

        if (player)
        {
            Destroy(worldFloor);
            Destroy(worldCeil);
            Destroy(portal);
            Destroy(player);
        }

        InitLevel();

        readyToBuild = true;
    }

    private void ClearGameObjectList(List<GameObject> listToClear)
    {
        for(int i = 0; i < listToClear.Count; i++)
        {
            Destroy(listToClear[i]);
        }
    }

    private void ResetNavMesh()
    {
        NavMeshSurface[] navMeshesToBake = worldNavMeshContainer.GetComponents<NavMeshSurface>();
        foreach (NavMeshSurface surface in navMeshesToBake)
        {
            surface.RemoveData();
            surface.BuildNavMesh();
        }

        /*
        NavMeshObstacle[] wallObstacles = worldGeoContainer.transform.GetComponentsInChildren<NavMeshObstacle>();

        for (int i = 0; i < wallObstacles.Length; i++)
        {
            Destroy(wallObstacles[i]);
        }
        */
    }

    /// <summary>
    /// Creates a randomized level
    /// </summary>
    private IEnumerator CreateLevel()
    {
        readyToBuild = false;
        ResetLevel();

        yield return new WaitUntil(() => readyToBuild == true);

        if (levelIndex < 1)
        {
            worldColor = Color.white;
        }
        else if (changeRoomColor && (levelIndex & 2) == 0)
        {
            worldColor = new Color((Random.Range(20, 230)), (Random.Range(20, 230)), (Random.Range(20, 230)));
            changeRoomColor = false;
        }
        else if ((levelIndex & 2) != 0)
        {
            changeRoomColor = true;
        }

        FillWorld();

        //Number of Rooms
        int numberOfRooms = Random.Range(MINIMUM_ROOMS, MAX_ROOMS);

        for (int i = 0; i < numberOfRooms; i++)
        {
            CreateRoom();
        }

        yield return new WaitUntil(() => ((currentRoomGeneratedIndex) == numberOfRooms));

        map.SetActive(true);
        ClearInvisibleGeo();

        yield return new WaitUntil(() => worldGeoCompleted == true);

        ResetNavMesh();

        //Player spawns in first empty postion in worldArray
        player = GameObject.Instantiate(playerPrefab, new Vector3(emptyTileIndices[0].x * gridSpotSize, 1, emptyTileIndices[0].y * gridSpotSize), Quaternion.identity);

        FillRooms();

        //Stairs spawn in last empty position in worldArray
        portal = GameObject.Instantiate(portalPrefab, new Vector3(emptyTileIndices[emptyTileIndices.Count - 1].x * gridSpotSize, 1.5f, emptyTileIndices[emptyTileIndices.Count - 1].y * gridSpotSize), Quaternion.identity);
    }

    public void ClearInvisibleGeo()
    {
        for (int col = 0; col < tilesPerLine.x; col++)
        {
            for (int row = 0; row < tilesPerLine.y; row++)
            {
                bool cardinalsFull = true;
                bool diagonalsFull = true;

                //Cardinals
                if (col < worldArray.GetLength(0) - 1 && (worldArray[(int)row, (int)col + 1] == null)) { cardinalsFull = false; }//Bottom
                if (col > 0 && (worldArray[(int)row, (int)col - 1] == null)) { cardinalsFull = false; }//Top
                if (row > 0 && (worldArray[(int)row - 1, (int)col] == null)) { cardinalsFull = false; }//Left
                if (row < worldArray.GetLength(0) - 1 && (worldArray[(int)row + 1, (int)col] == null)) { cardinalsFull = false; }//Right 

                //Diagonals
                if ((col > 0 && row < worldArray.GetLength(0) - 1) && ((worldArray[(int)row + 1, (int)col - 1] == null))) { diagonalsFull = false; }//Top Right
                if ((col > 0 && row > 0) && ((worldArray[(int)row - 1, (int)col - 1] == null))) { diagonalsFull = false; }//Top Left
                if ((row > 0 && col < worldArray.GetLength(0) - 1) && ((worldArray[(int)row - 1, (int)col + 1] == null))) { diagonalsFull = false; }//Bottom Left 
                if ((row < worldArray.GetLength(0) - 1 && col < worldArray.GetLength(0) - 1) && ((worldArray[(int)row + 1, (int)col + 1] == null))) { diagonalsFull = false; }//Bottom Right

                if(cardinalsFull && diagonalsFull)
                {
                    Destroy(worldArray[row, col]);
                }
            }
        }

        for (int q = 0; q < 3; q++)
        {
            for (int i = 0; i < doorTileIndices.Count; i++)
            {
                bool sidesFull = true;
                bool topBottomFull = true;

                if (worldArray[(int)doorTileIndices[i].x, (int)doorTileIndices[i].y + 1] == null && CheckIfSpaceIsEmpty(new Vector2((int)doorTileIndices[i].x, (int)doorTileIndices[i].y + 1))) { topBottomFull = false; }//Bottom
                if (worldArray[(int)doorTileIndices[i].x, (int)doorTileIndices[i].y - 1] == null && CheckIfSpaceIsEmpty(new Vector2((int)doorTileIndices[i].x, (int)doorTileIndices[i].y - 1))) { topBottomFull = false; }//Top

                if (worldArray[(int)doorTileIndices[i].x - 1, (int)doorTileIndices[i].y] == null && CheckIfSpaceIsEmpty(new Vector2((int)doorTileIndices[i].x - 1, (int)doorTileIndices[i].y))) { sidesFull = false; }//Left
                if (worldArray[(int)doorTileIndices[i].x + 1, (int)doorTileIndices[i].y] == null && CheckIfSpaceIsEmpty(new Vector2((int)doorTileIndices[i].x + 1, (int)doorTileIndices[i].y))) { sidesFull = false; }//Right 

                if (!sidesFull && !topBottomFull)
                {
                    Destroy(worldArray[(int)doorTileIndices[i].x, (int)doorTileIndices[i].y]);
                }
            }
        }

        worldGeoCompleted = true;
    }

    public bool CheckIfSpaceIsEmpty(Vector2 placeToCheck)
    {
        for (int i = 0; i < doorTileIndices.Count; i++)
        {
            if(doorTileIndices[i].x == placeToCheck.x && doorTileIndices[i].y == placeToCheck.y)
            {
                return false;
            }
        }
        return true;
    }


    /// <summary>
    /// Creates rooms and calls tunnelCreate()
    /// </summary>
    public void CreateRoom()
    {
        currentRoom.x = (int)(Random.Range(1, tilesPerLine.x - 1));
        currentRoom.y = (int)(Random.Range(1, tilesPerLine.y - 1));

        currentRoomSize = new Vector2(Random.Range(MINIMUM_ROOM_SIZE, MAX_ROOM_SIZE), Random.Range(MINIMUM_ROOM_SIZE, MAX_ROOM_SIZE));

        for (int i = 0; i < currentRoomSize.x; i++)
        {
            for (int q = 0; q < currentRoomSize.y; q++)
            {
                if (!((int)(currentRoom.x + q) > tilesPerLine.x - 2) && !((int)(currentRoom.y + i) > tilesPerLine.y - 2))
                {
                    DestroyWall(new Vector2((int)(currentRoom.x + q), (int)(currentRoom.y + i)));
                }
            }
        }

        CreateTunnel();
        previousRoom = currentRoom;
        previousRoomSize = currentRoomSize;
    }

    /// <summary>
    /// Connects the previousRoom created to the currentRoom at a randomX position within the currentRoom and previousRoom
    /// </summary>
    public void CreateTunnel()
    {
        if (previousRoom.x != 0 && previousRoom.y != 0)
        {
            Vector2 direction = new Vector2(1, 1);

            //Calculates an offset for the tunnel to be created from the previousRoom and currentRoom
            Vector2 previousOffSet = new Vector2(0, 0);
            Vector2 currentOffSet = new Vector2(0, 0);

            //Keeps creating until not out of bounds
            do
            {
                previousOffSet.x = Random.Range(0, (int)(previousRoomSize.x - 1));
                previousOffSet.y = Random.Range(0, (int)(previousRoomSize.y - 1));
            } while (!(previousOffSet.x + previousRoom.x < tilesPerLine.x - 1 && previousOffSet.y + previousRoom.y < tilesPerLine.y - 1));

            do
            {
                currentOffSet.x = Random.Range(0, (int)(currentRoomSize.x - 1));
                currentOffSet.y = Random.Range(0, (int)(currentRoomSize.y - 1));
            } while (!(currentOffSet.x + currentRoom.x < tilesPerLine.x - 1 && currentOffSet.y + currentRoom.y < tilesPerLine.y - 1));

            //Creating Offset vectors
            Vector2 randomPreviousRoom = new Vector2((previousRoom.x + previousOffSet.x), (previousRoom.y + previousOffSet.y));
            Vector2 randomCurrentRoom = new Vector2((currentRoom.x + currentOffSet.x), (currentRoom.y + currentOffSet.y));

            //Direction of the rooms in relation to each other
            if (randomCurrentRoom.x < randomPreviousRoom.x) { direction.x = -1; }
            if (randomCurrentRoom.y < randomPreviousRoom.y) { direction.y = -1; }

            //Distance of components between the two rooms
            int magnatudeX = Mathf.Abs(((int)(randomPreviousRoom.x)) - (int)(randomCurrentRoom.x));
            int magnatudeY = Mathf.Abs(((int)(randomPreviousRoom.y)) - (int)(randomCurrentRoom.y));

            //Creates tunnels
            for (int i = 0; i <= magnatudeX; i++)
            {
                Vector2 indices = new Vector2((int)(randomPreviousRoom.x + (i * direction.x)), (int)(randomPreviousRoom.y));
                if(i == 2)
                {
                    DestroyWall(indices, true);
                    worldArray[(int)indices.x, (int)indices.y] = GameObject.Instantiate(doorPrefab, new Vector3((int)indices.x * gridSpotSize, 1, (int)indices.y * gridSpotSize), Quaternion.identity);

                    GameObject doorMesh = worldArray[(int)indices.x, (int)indices.y].gameObject.transform.GetChild(0).gameObject;
                    doorMesh.GetComponent<Renderer>().sharedMaterial.color = worldColor;

                    worldArray[(int)indices.x, (int)indices.y].transform.parent = worldGeoContainer.transform;
                    doorTileIndices.Add(indices);
                }
                else
                {
                    DestroyWall(indices);
                }
            }

            randomPreviousRoom.x += (magnatudeX * direction.x);

            for (int i = 0; i <= magnatudeY; i++)
            {
                Vector2 indices = new Vector2((int)(randomPreviousRoom.x), (int)(randomPreviousRoom.y + (i * direction.y)));
                
                if (i == 2)
                {
                    DestroyWall(indices, true);
                    worldArray[(int)indices.x, (int)indices.y] = GameObject.Instantiate(doorPrefab, new Vector3((int)indices.x * gridSpotSize, 1, (int)indices.y * gridSpotSize), Quaternion.Euler(0, 90, 0));

                    GameObject doorMesh = worldArray[(int)indices.x, (int)indices.y].gameObject.transform.GetChild(0).gameObject;
                    doorMesh.GetComponent<Renderer>().sharedMaterial.color = worldColor;

                    worldArray[(int)indices.x, (int)indices.y].transform.parent = worldGeoContainer.transform;
                    doorTileIndices.Add(indices);
                }
                else
                {
                    DestroyWall(indices);
                }
            }
        }
        currentRoomGeneratedIndex++;
    }

    /// <summary>
    /// Destroy a wall & add it to the emptyTitles array
    /// </summary>
    /// <param name="worldArrayIndices"></param>
    private void DestroyWall(Vector2 worldArrayIndices, bool dontAddToList = false)
    {
        if (dontAddToList == false)
        {
            if (worldArray[(int)worldArrayIndices.x, (int)worldArrayIndices.y] != null)
            {
                emptyTileIndices.Add(new Vector2((int)worldArrayIndices.x, (int)worldArrayIndices.y));
            }
        }
        Destroy(worldArray[(int)worldArrayIndices.x, (int)worldArrayIndices.y]);
    }

    /// <summary>
    /// Fills worldArray with Wall gameObjects
    /// </summary>
    public void FillWorld()
    {
        for (int col = 0; col < tilesPerLine.x; col++)
        {
            for (int row = 0; row < tilesPerLine.y; row++)
            {
                worldArray[row, col] = GameObject.Instantiate(wallPrefab, new Vector3(row * gridSpotSize, 1, col * gridSpotSize), Quaternion.identity);

                GameObject wallMesh = worldArray[row, col].gameObject.transform.GetChild(0).gameObject;
                wallMesh.transform.localScale = new Vector3(gridSpotSize, wallMesh.transform.localScale.y, gridSpotSize);
                wallMesh.GetComponent<Renderer>().sharedMaterial.color = worldColor;

                worldArray[row, col].transform.parent = worldGeoContainer.transform;
            }
        }

        worldFloor = GameObject.Instantiate(floorPrefab, new Vector3(0, -0.5f, 0), Quaternion.identity);
        GameObject floorMesh = worldFloor.transform.GetChild(0).gameObject;
        floorMesh.transform.localScale = new Vector3(WORLD_SIZE.x, floorMesh.transform.localScale.y, WORLD_SIZE.y);
        floorMesh.transform.position = new Vector3((gridSpotSize * (tilesPerLine.x - 1)) / 2, floorMesh.transform.position.y, (gridSpotSize * (tilesPerLine.y - 1)) / 2);
        floorMesh.GetComponent<Renderer>().sharedMaterial.color = worldColor;

        worldCeil = GameObject.Instantiate(ceilingPrefab, new Vector3(0, wallPrefab.transform.GetChild(0).localScale.y+1, 0), Quaternion.identity);
        GameObject ceilMesh = worldCeil.transform.GetChild(0).gameObject;
        ceilMesh.transform.localScale = new Vector3(WORLD_SIZE.x, ceilMesh.transform.localScale.y, WORLD_SIZE.y);
        ceilMesh.transform.position = new Vector3((gridSpotSize * (tilesPerLine.x - 1)) / 2, ceilMesh.transform.position.y, (gridSpotSize * (tilesPerLine.y - 1)) / 2);
        ceilMesh.GetComponent<Renderer>().sharedMaterial.color = worldColor;
    }

    /// <summary>
    /// Fills empty spaces in the level with Items & Enemies
    /// </summary>
    public void FillRooms()
    {
        for (int i = SPAWN_BUFFER; i < emptyTileIndices.Count - SPAWN_BUFFER; i++)
        {
            int itemSpawnChance = (int)(Random.Range(0, 100));
            int enemySpawnChance = (int)(Random.Range(0, 100));
            int weaponSpawnChance = (int)(Random.Range(0, 100));

            if(weaponSpawnChance <= WEAPON_SPAWN_CHANCE)
            {
                int weaponChooseChance = Random.Range(0, 100);
                int whichWeaponToSpawn = 0;

                if(weaponChooseChance <= 35) //35% - Pistol
                {
                    whichWeaponToSpawn = 0;
                }
                else if (weaponChooseChance > 35 && weaponChooseChance <= 60) //25% - Shotgun
                {
                    whichWeaponToSpawn = 1;
                }
                else if (weaponChooseChance > 60 && weaponChooseChance <= 75) //15% - Machine Gun
                {
                    whichWeaponToSpawn = 6;
                }
                else if (weaponChooseChance > 75 && weaponChooseChance <= 85) //10% - Pulse Rifle
                {
                    whichWeaponToSpawn = 5;
                }
                else if (weaponChooseChance > 85 && weaponChooseChance <= 90) //5% - Rocket Launcher
                {
                    whichWeaponToSpawn = 2;
                }
                else if (weaponChooseChance > 90 && weaponChooseChance <= 100) //10% - Grenades
                {
                    whichWeaponToSpawn = 4;
                }

                GameObject newWeapon = GameObject.Instantiate(weaponPrefab, new Vector3(emptyTileIndices[i].x * gridSpotSize, 2, emptyTileIndices[i].y * gridSpotSize), Quaternion.identity);
                newWeapon.GetComponent<PlayerPickup>().currentPickUpIndex = whichWeaponToSpawn;
                newWeapon.transform.parent = worldWeaponsContainer.transform;
                roomItems.Add(newWeapon);
            }       
            else if (enemySpawnChance <= ENEMY_SPAWN_CHANCE) //Spawn Enemies
            {
                int whichEnemyToSpawn = Random.Range(0, enemyPrefabs.Count);
                GameObject newEnemy = GameObject.Instantiate(enemyPrefabs[whichEnemyToSpawn], new Vector3((emptyTileIndices[i].x * gridSpotSize), 2, (emptyTileIndices[i].y * gridSpotSize)), Quaternion.identity);
                newEnemy.transform.parent = worldEnemiesContainer.transform;
                roomEnemies.Add(newEnemy);
            }
        }
    }
}
