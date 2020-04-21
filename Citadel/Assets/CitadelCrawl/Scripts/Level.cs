using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using VHS;

public class Level : MonoBehaviour
{
    #region Variables & Inspector Options
    [Header("World Options")]
    public int gridSpotSize = 32; //How wide a single grid slot is in unity units
    public float tilesPerLine = 25; //How many grid slots there are per axis in the world array
    public int SPAWN_BUFFER = 3; //How far away can things spawn from the player & the portal
    public int COLLECTABLE_SPAWN_CHANCE = 3, ENEMY_SPAWN_CHANCE = 2, WEAPON_SPAWN_CHANCE = 3; //Decimal Spawn Chances
    public int MAX_ENEMIES = 10, MAX_GUNS = 8;

    [Header("Room Options")]
    [Space(10)]
    public int MAX_ROOMS = 10, MINIMUM_ROOMS = 8; //Total Rooms that can spawn
    public int MAX_ROOM_SIZE = 6, MINIMUM_ROOM_SIZE = 3; //Room Size Cap

    [Header("World References")]
    [Space(10)]
    public GameObject worldGeoContainer;
    public GameObject worldNavMeshContainer;
    public GameObject worldEnemiesContainer;
    public GameObject worldWeaponsContainer;
    public GameObject worldCollectablesContainer;

    [Header("Entity References")]
    [Space(10)]
    public GameObject mapGameObject;
    public GameObject playerPrefab;
    public GameObject portalPrefab;
    public GameObject wallPrefab;
    public GameObject doorPrefab;
    public GameObject floorPrefab;
    public GameObject ceilingPrefab;
    public GameObject weaponPrefab;
    public GameObject collectablePrefab;
    public List<GameObject> enemyPrefabs;

    [Header("Variety References")]
    [Space(10)]
    public List<WallTexture> wallTextures;

    #region Stored Data
    [HideInInspector]
    public int levelIndex = 0;

    //World Data
    private GameObject[,] worldArray;
    private List<Vector2> emptyTileIndices;
    private List<Vector2> doorTileIndices;
    private Vector2 WORLD_SIZE;
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
    private WallTexture chosenWorldMat;
    private Vector2 farthestPoint;
    private float playerStartingHealth = 100;
    #endregion
    #endregion

    #region Unity Methods
    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
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
    #endregion

    /// <summary>
    /// Initializes a new Level and sets up all needed values
    /// </summary>
    public void InitLevel()
    {
        //Set up  
        if(GameVars.instance)
        {
            GameVars.instance.crawlManager = this;
            playerStartingHealth = GameVars.instance.currentCrawlHealth;
        }
        /* if (GameVars.instance) { levelIndex = GameVars.instance.currentCrawlLevel; } */
        worldArray = new GameObject[(int)tilesPerLine, (int)tilesPerLine];
        WORLD_SIZE = new Vector2(gridSpotSize * tilesPerLine, gridSpotSize * tilesPerLine);
        emptyTileIndices = new List<Vector2>();
        doorTileIndices = new List<Vector2>();

        roomItems = new List<GameObject>();
        roomEnemies = new List<GameObject>();

        if (GameVars.instance && GameVars.instance.currentCrawlLevel == 0)
        {
            chosenWorldMat = wallTextures[0];
        }
        else
        {
            if (wallTextures != null && wallTextures.Count > 0)
            {
                do
                {
                    foreach (WallTexture wallTexture in wallTextures)
                    {
                        if (wallTexture.RollChance(wallTexture.chosenPercent))
                        {
                            chosenWorldMat = wallTexture;
                        }
                    }
                } while (chosenWorldMat == null);
            }
        }
    }

    /// <summary>
    /// Resets the current Level by clearing out all the values and then initializing
    /// </summary>
    public void ResetLevel()
    {
        worldGeoCompleted = false;
        currentRoomGeneratedIndex = 0;

        //Clear
        for (int col = 0; col < tilesPerLine; col++)
        {
            for (int row = 0; row < tilesPerLine; row++)
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

    #region Main Generation Methods
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

        mapGameObject.SetActive(true);
        ClearInvisibleGeo();

        yield return new WaitUntil(() => worldGeoCompleted == true);

        ResetNavMesh();

        //Player spawns in first empty postion in worldArray
        player = GameObject.Instantiate(playerPrefab, new Vector3(emptyTileIndices[0].x * gridSpotSize, 1, emptyTileIndices[0].y * gridSpotSize), Quaternion.identity);
        InteractionController.instance.playerHealth = playerStartingHealth;

        FillRooms();

        //Stairs spawn in last empty position in worldArray
        portal = GameObject.Instantiate(portalPrefab, new Vector3(farthestPoint.x * gridSpotSize, 1.5f, farthestPoint.y * gridSpotSize), Quaternion.identity);
    }

    /// <summary>
    /// Fills worldArray with Wall gameObjects
    /// </summary>
    private void FillWorld()
    {
        for (int col = 0; col < tilesPerLine; col++)
        {
            for (int row = 0; row < tilesPerLine; row++)
            {
                AddWall(new Vector2(row, col), wallPrefab, 1, Quaternion.identity, false);

                GameObject wallMesh = worldArray[row, col].gameObject.transform.GetChild(0).gameObject;
                wallMesh.transform.localScale = new Vector3(gridSpotSize, wallMesh.transform.localScale.y, gridSpotSize);

                wallMesh.GetComponent<Renderer>().sharedMaterial = chosenWorldMat.ChooseWallMat();
                wallMesh.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = wallMesh.GetComponent<Renderer>().sharedMaterial;

                wallMesh.GetComponent<Renderer>().sharedMaterial.color = worldColor;

                worldArray[row, col].transform.parent = worldGeoContainer.transform;
            }
        }

        worldFloor = GameObject.Instantiate(floorPrefab, new Vector3(0, -0.5f, 0), Quaternion.identity);
        GameObject floorMesh = worldFloor.transform.GetChild(0).gameObject;
        floorMesh.transform.localScale = new Vector3(WORLD_SIZE.x, floorMesh.transform.localScale.y, WORLD_SIZE.y);
        floorMesh.transform.position = new Vector3((gridSpotSize * (tilesPerLine - 1)) / 2, floorMesh.transform.position.y, (gridSpotSize * (tilesPerLine - 1)) / 2);
        floorMesh.GetComponent<Renderer>().sharedMaterial.color = worldColor;

        worldCeil = GameObject.Instantiate(ceilingPrefab, new Vector3(0, wallPrefab.transform.GetChild(0).localScale.y + 1, 0), Quaternion.identity);
        GameObject ceilMesh = worldCeil.transform.GetChild(0).gameObject;
        ceilMesh.transform.localScale = new Vector3(WORLD_SIZE.x, ceilMesh.transform.localScale.y, WORLD_SIZE.y);
        ceilMesh.transform.position = new Vector3((gridSpotSize * (tilesPerLine - 1)) / 2, ceilMesh.transform.position.y, (gridSpotSize * (tilesPerLine - 1)) / 2);
        ceilMesh.GetComponent<Renderer>().sharedMaterial.color = worldColor;
    }

    /// <summary>
    /// Creates rooms and calls tunnelCreate()
    /// </summary>
    private void CreateRoom()
    {
        currentRoom.x = (int)(Random.Range(1, tilesPerLine - 1));
        currentRoom.y = (int)(Random.Range(1, tilesPerLine - 1));

        currentRoomSize = new Vector2(Random.Range(MINIMUM_ROOM_SIZE, MAX_ROOM_SIZE), Random.Range(MINIMUM_ROOM_SIZE, MAX_ROOM_SIZE));

        for (int i = 0; i < currentRoomSize.x; i++)
        {
            for (int q = 0; q < currentRoomSize.y; q++)
            {
                if (!((int)(currentRoom.x + q) > tilesPerLine - 2) && !((int)(currentRoom.y + i) > tilesPerLine - 2))
                {
                    Vector2 currentWall = new Vector2((int)(currentRoom.x + q), (int)(currentRoom.y + i));
                    DestroyWall(currentWall);
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
    private void CreateTunnel()
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
            } while (!(previousOffSet.x + previousRoom.x < tilesPerLine - 1 && previousOffSet.y + previousRoom.y < tilesPerLine - 1));

            do
            {
                currentOffSet.x = Random.Range(0, (int)(currentRoomSize.x - 1));
                currentOffSet.y = Random.Range(0, (int)(currentRoomSize.y - 1));
            } while (!(currentOffSet.x + currentRoom.x < tilesPerLine - 1 && currentOffSet.y + currentRoom.y < tilesPerLine - 1));

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
                if (i == 2 && (indices != emptyTileIndices[0] && indices != emptyTileIndices[emptyTileIndices.Count - 1]))
                {
                    DestroyWall(indices);
                    AddWall(indices, doorPrefab, 1, Quaternion.identity, true);

                    GameObject doorMesh = worldArray[(int)indices.x, (int)indices.y].gameObject.transform.GetChild(0).gameObject;
                    doorMesh.GetComponent<Renderer>().sharedMaterial.color = worldColor;

                    worldArray[(int)indices.x, (int)indices.y].transform.parent = worldGeoContainer.transform;
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

                if (i == 2 && (indices != emptyTileIndices[0] && indices != emptyTileIndices[emptyTileIndices.Count - 1]))
                {
                    DestroyWall(indices);
                    AddWall(indices, doorPrefab, 1, Quaternion.Euler(0, 90, 0), true);

                    GameObject doorMesh = worldArray[(int)indices.x, (int)indices.y].gameObject.transform.GetChild(0).gameObject;
                    doorMesh.GetComponent<Renderer>().sharedMaterial.color = worldColor;

                    worldArray[(int)indices.x, (int)indices.y].transform.parent = worldGeoContainer.transform;
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
    /// Fills empty spaces in the level with Items & Enemies
    /// </summary>
    private void FillRooms()
    {
        RemoveInvalidEmptySpaces();
        for (int i = 1; i < emptyTileIndices.Count - 1; i++)
        {
            if (emptyTileIndices[i] != farthestPoint)
            {
                int itemSpawnChance = (int)(Random.Range(0, 100));
                int enemySpawnChance = (int)(Random.Range(0, 100));
                int weaponSpawnChance = (int)(Random.Range(0, 100));

                if (weaponSpawnChance <= WEAPON_SPAWN_CHANCE && MAX_GUNS > 0)
                {
                    MAX_GUNS -= 1;
                    int weaponChooseChance = Random.Range(0, 100);
                    int whichWeaponToSpawn = 0;

                    if (weaponChooseChance <= 35) //35% - Pistol
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
                else if (enemySpawnChance <= ENEMY_SPAWN_CHANCE && MAX_ENEMIES > 0) //Spawn Enemies
                {
                    MAX_ENEMIES -= 1;
                    int whichEnemyToSpawn = Random.Range(0, enemyPrefabs.Count);
                    GameObject newEnemy = GameObject.Instantiate(enemyPrefabs[whichEnemyToSpawn], new Vector3((emptyTileIndices[i].x * gridSpotSize), 2, (emptyTileIndices[i].y * gridSpotSize)), Quaternion.identity);
                    newEnemy.transform.parent = worldEnemiesContainer.transform;
                    roomEnemies.Add(newEnemy);
                }
            }
        }
    }
    #endregion

    #region Generation Helper Methods
    /// <summary>
    /// Adds a wall & removes it from the emptyTiles array
    /// </summary>
    /// <param name="worldArrayIndices"></param>
    /// <param name="objectToAdd"></param>
    /// <param name="yComponent"></param>
    /// <param name="rotation"></param>
    private void AddWall(Vector2 worldArrayIndices, GameObject objectToAdd, float yComponent, Quaternion rotation, bool isDoor)
    {
        if (worldArray[(int)worldArrayIndices.x, (int)worldArrayIndices.y] != null)
        {
            DestroyWall(worldArrayIndices);
        }

        if (emptyTileIndices.Contains(worldArrayIndices))
        {
            emptyTileIndices.Remove(worldArrayIndices);
        }

        GameObject newWall = GameObject.Instantiate(objectToAdd, new Vector3((int)worldArrayIndices.x * gridSpotSize, yComponent, (int)worldArrayIndices.y * gridSpotSize), rotation);

        if (isDoor)
        {
            if(!doorTileIndices.Contains(worldArrayIndices))
            {
                doorTileIndices.Add(worldArrayIndices);
            }
        }
        else
        {
            GameObject newWallMesh = newWall.transform.GetChild(0).gameObject;
            newWallMesh.GetComponent<Renderer>().sharedMaterial = chosenWorldMat.ChooseWallMat();
            newWallMesh.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = newWallMesh.GetComponent<Renderer>().sharedMaterial;
        }

        worldArray[(int)worldArrayIndices.x, (int)worldArrayIndices.y] = newWall;
    }

    /// <summary>
    /// Destroy a wall & add it to the emptyTiles array
    /// </summary>
    /// <param name="worldArrayIndices"></param>
    private void DestroyWall(Vector2 worldArrayIndices)
    {
        if (worldArray[(int)worldArrayIndices.x, (int)worldArrayIndices.y] != null)
        {
            Destroy(worldArray[(int)worldArrayIndices.x, (int)worldArrayIndices.y]);
        }

        if (!emptyTileIndices.Contains(worldArrayIndices))
        {
            emptyTileIndices.Add(worldArrayIndices);
        }

        if (doorTileIndices.Contains(worldArrayIndices))
        {
            doorTileIndices.Remove(worldArrayIndices);
        }
    }

    /// <summary>
    /// Checks if a given vector2 is within the doorTiles array
    /// </summary>
    /// <param name="placeToCheck"></param>
    /// <returns></returns>
    private bool IsSpaceDoor(Vector2 placeToCheck)
    {
        if (doorTileIndices.Contains(placeToCheck))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if a given vector2 is within the emptyTiles array
    /// </summary>
    /// <param name="placeToCheck"></param>
    /// <returns></returns>
    private bool IsSpaceEmpty(Vector2 placeToCheck)
    {
        if (emptyTileIndices.Contains(placeToCheck))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Removes any empty spaces that are within the SPAWN_BUFFER range from either the first emptyTile or the last one
    /// </summary>
    private void RemoveInvalidEmptySpaces()
    {
        farthestPoint = emptyTileIndices[FindFarthestIndex(emptyTileIndices[0])];

        for (int i = emptyTileIndices.Count - 1; i > 0; i--)
        {
            if (emptyTileIndices[i] != farthestPoint)
            {
                if (Mathf.Abs(Vector2.Distance(emptyTileIndices[0], emptyTileIndices[i])) < SPAWN_BUFFER)
                {
                    emptyTileIndices.Remove(emptyTileIndices[i]);
                }
                else if (Mathf.Abs(Vector2.Distance(farthestPoint, emptyTileIndices[i])) < SPAWN_BUFFER)
                {
                    emptyTileIndices.Remove(emptyTileIndices[i]);
                }
            }
        }
    }

    private int FindFarthestIndex(Vector2 starting)
    {
        int currentFarthest = 0;
        float currentDistance = 0;
        for(int i = 0; i < emptyTileIndices.Count; i++)
        {
            float newDistance = Mathf.Abs(Vector2.Distance(starting, emptyTileIndices[i]));
            if (newDistance > currentDistance)
            {
                currentDistance = newDistance;
                currentFarthest = i;
            }
        }
        return currentFarthest;
    }

    /// <summary>
    /// Removes all walls that are outside of the level bounds and would not be seen by the player
    /// </summary>
    private void ClearInvisibleGeo()
    {
        for (int col = 0; col < tilesPerLine; col++)
        {
            for (int row = 0; row < tilesPerLine; row++)
            {
                bool cardinalsFull = true;
                bool diagonalsFull = true;

                //Cardinals
                if (col < worldArray.GetLength(0) - 1 && IsSpaceEmpty(new Vector2((int)row, (int)col + 1))) { cardinalsFull = false; }//Bottom
                if (col > 0 && IsSpaceEmpty(new Vector2((int)row, (int)col - 1))) { cardinalsFull = false; }//Top
                if (row > 0 && IsSpaceEmpty(new Vector2((int)row - 1, (int)col))) { cardinalsFull = false; }//Left
                if (row < worldArray.GetLength(0) - 1 && IsSpaceEmpty(new Vector2((int)row + 1, (int)col))) { cardinalsFull = false; }//Right 

                //Diagonals
                if ((col > 0 && row < worldArray.GetLength(0) - 1) && IsSpaceEmpty(new Vector2((int)row + 1, (int)col - 1))) { diagonalsFull = false; }//Top Right
                if ((col > 0 && row > 0) && IsSpaceEmpty(new Vector2((int)row - 1, (int)col - 1))) { diagonalsFull = false; }//Top Left
                if ((row > 0 && col < worldArray.GetLength(0) - 1) && IsSpaceEmpty(new Vector2((int)row - 1, (int)col + 1))) { diagonalsFull = false; }//Bottom Left 
                if ((row < worldArray.GetLength(0) - 1 && col < worldArray.GetLength(0) - 1) && IsSpaceEmpty(new Vector2((int)row + 1, (int)col + 1))) { diagonalsFull = false; }//Bottom Right

                if (cardinalsFull && diagonalsFull)
                {
                    Destroy(worldArray[row, col]);
                }
            }
        }

        for (int i = doorTileIndices.Count - 1; i > -1; i--)
        {
            bool topBottomEmpty = false;
            bool sidesEmpty = false;

            if (IsSpaceEmpty(new Vector2((int)doorTileIndices[i].x, (int)doorTileIndices[i].y + 1)) || (!IsSpaceEmpty(new Vector2((int)doorTileIndices[i].x, (int)doorTileIndices[i].y + 1)) && IsSpaceDoor(new Vector2((int)doorTileIndices[i].x, (int)doorTileIndices[i].y + 1)))) { topBottomEmpty = true; }//Bottom
            if (IsSpaceEmpty(new Vector2((int)doorTileIndices[i].x, (int)doorTileIndices[i].y - 1)) || (!IsSpaceEmpty(new Vector2((int)doorTileIndices[i].x, (int)doorTileIndices[i].y - 1)) && IsSpaceDoor(new Vector2((int)doorTileIndices[i].x, (int)doorTileIndices[i].y - 1)))) { topBottomEmpty = true; }//Top

            if (IsSpaceEmpty(new Vector2((int)doorTileIndices[i].x - 1, (int)doorTileIndices[i].y)) || (!IsSpaceEmpty(new Vector2((int)doorTileIndices[i].x - 1, (int)doorTileIndices[i].y)) && IsSpaceDoor(new Vector2((int)doorTileIndices[i].x - 1, (int)doorTileIndices[i].y)))) { sidesEmpty = true; }//Left
            if (IsSpaceEmpty(new Vector2((int)doorTileIndices[i].x + 1, (int)doorTileIndices[i].y)) || (!IsSpaceEmpty(new Vector2((int)doorTileIndices[i].x + 1, (int)doorTileIndices[i].y)) && IsSpaceDoor(new Vector2((int)doorTileIndices[i].x + 1, (int)doorTileIndices[i].y)))) { sidesEmpty = true; }//Right 

            if (sidesEmpty && topBottomEmpty)
            {
                DestroyWall(new Vector2((int)doorTileIndices[i].x, (int)doorTileIndices[i].y));
            }
        }

        worldGeoCompleted = true;
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Clears a given list of gameObject by making sure to destroy each one before removing it
    /// </summary>
    /// <param name="listToClear"></param>
    private void ClearGameObjectList(List<GameObject> listToClear)
    {
        for (int i = 0; i < listToClear.Count; i++)
        {
            Destroy(listToClear[i]);
        }
    }

    /// <summary>
    /// Clears the current NavMesh and then immediately rebakes it
    /// </summary>
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
    /// Increments the current level score
    /// </summary>
    public void IncrementLevelScore()
    {
        if (GameVars.instance)
        {
            GameVars.instance.currentCrawlLevel += 1;
            GameVars.instance.currentCrawlHealth = InteractionController.instance.playerHealth;
        }
    }

    /// <summary>
    /// Attempts to update the player's highscore if it is higher than what is currently saved
    /// </summary>
    public void AttemptUpdateScore()
    {
        if (GameVars.instance)
        {
            if (GameVars.instance.saveManager.crawlHighScore < GameVars.instance.currentCrawlLevel)
            {
                GameVars.instance.saveManager.crawlHighScore = GameVars.instance.currentCrawlLevel;
            }

            GameVars.instance.saveManager.UpdateSaveData();
            GameVars.instance.currentCrawlLevel = 0;
        }
    }
    #endregion

    #region Classes
    [System.Serializable]
    public class WallTexture
    {
        [Header("WallTexture Settings")]
        #region WallTexture
        [Tooltip("The possible mat varients on this WallTexture")]
        public List<WallMat> possibleWallMats;
        [Tooltip("Chance of this WallTexture being chosen")]
        [Range(0, 100)]
        public int chosenPercent = 100;
        #endregion

        #region WallTexture Data
        [HideInInspector]
        public Material chosenMaterial;
        #endregion

        public Material ChooseWallMat()
        {
            chosenMaterial = null;
            do
            {
                foreach(WallMat wallMat in possibleWallMats)
                {
                    if(RollChance(wallMat.chosenPercent))
                    {
                        chosenMaterial = wallMat.wallMaterial;
                        return wallMat.wallMaterial;
                    }
                }
            } while (chosenMaterial == null);

            return null;
        }

        public bool RollChance(int chance)
        {
            if (chosenPercent > 0)
            {
                if (Random.Range(0, 101) <= chance)
                {
                    return true;
                }
            }
            return false;
        }
    }

    [System.Serializable]
    public class WallMat
    {
        [Header("WallTexture Settings")]
        #region WallMat
        [Tooltip("The mat for this possible WallMat")]
        public Material wallMaterial;
        [Tooltip("Chance of this mat being chosen")]
        [Range(0, 100)]
        public int chosenPercent = 100;
        #endregion
    }
    #endregion
}