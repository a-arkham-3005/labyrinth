using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Threading;
using Unity.VisualScripting;
using System.IO;
using TMPro;
public class NewBehaviourScript : MonoBehaviour
{
    Camera _mainCam;
    int[,] field_ids = new int[10, 10]
    {
    {2,3,1,2,3,14,14,14,14,14},
    {3,2,3,0,1,14,14,14,14,14},
    {2,2,1,1,1,1,3,3,3,2},
    {2,2,3,2,3,2,2,2,1,0},
    {2,2,3,2,1,2,2,2,3,1},
    {3,3,1,3,1,3,2,2,1,2},
    {1,2,2,2,1,0,2,2,3,2},
    {3,1,1,1,3,1,1,3,3,2},
    {2,2,2,1,2,1,2,3,2,2},
    {2,2,2,1,1,1,2,3,2,2}
    };
    int[] entrances = new int[22]
    {
        1, 4, 10, 12, 27, 28, 29, 32, 34, 42, 48, 50, 51, 53, 55, 68, 70, 74, 77, 78, 87, 97
    };
    string[] whereToGo = new string[22]
    {
        "L", "L", "U|D", "L", "U", "D", "R", "L", "R", "R", "L", "U", "D", "D", "R", "R", "D", "D", "U", "D", "L", "R"
    };
    bool unavailableArea = true;
    public GameObject[,] map = new GameObject[10, 10];
    List<int> discoveredPoints = new List<int>();
    List<int> discoveredEnemies = new List<int>();
    int numOfMovesLeftForStep = 50;
    int numOfRounds = 10;
    int numOfMovesPerRound = 5;
    int movesNum = 1;
    List<string> moves = new List<string>();
    int playerLocation;
    int[] enemiesLocation = new int[5];
    bool[] enemiesEncountered = new bool[5]
    {
        false, false, false, false, false
    };
    private void Awake()
    {
        _mainCam = Camera.main;
    }
    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        var rayHit = Physics2D.GetRayIntersection(_mainCam.ScreenPointToRay(Mouse.current.position.ReadValue()));
        if (!rayHit.collider) return;
        string temp=rayHit.collider.gameObject.name;
        switch (temp)
        {
            case "Go left":
                {
                    if (moves.Count < numOfMovesPerRound)
                    {
                        moves.Add("L");
                        GameObject gameObject = GameObject.Find("Field" + movesNum);
                        gameObject.AddComponent(typeof(SpriteRenderer));
                        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                        spriteRenderer.sprite = Resources.Load<Sprite>("ffff/L");
                        movesNum++;
                    }
                    break;
                }
            case "Go right":
                {
                    if (moves.Count < numOfMovesPerRound)
                    {
                        moves.Add("R");
                        GameObject gameObject = GameObject.Find("Field" + movesNum);
                        gameObject.AddComponent(typeof(SpriteRenderer));
                        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                        spriteRenderer.sprite = Resources.Load<Sprite>("ffff/R");
                        movesNum++;
                    }
                    break;
                }
            case "Go up":
                {
                    if (moves.Count < numOfMovesPerRound)
                    {
                        moves.Add("U");
                        GameObject gameObject = GameObject.Find("Field" + movesNum);
                        gameObject.AddComponent(typeof(SpriteRenderer));
                        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                        spriteRenderer.sprite = Resources.Load<Sprite>("ffff/U");
                        movesNum++;
                    }
                    break;
                }
            case "Go down":
                {
                    if (moves.Count < numOfMovesPerRound)
                    {
                        moves.Add("D");
                        GameObject gameObject = GameObject.Find("Field" + movesNum);
                        gameObject.AddComponent(typeof(SpriteRenderer));
                        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                        spriteRenderer.sprite = Resources.Load<Sprite>("ffff/D");
                        movesNum++;
                    }
                    break;
                }
            case "Interaction":
                {
                    if (moves.Count < numOfMovesPerRound)
                    {
                        moves.Add("I");
                        GameObject gameObject = GameObject.Find("Field" + movesNum);
                        gameObject.AddComponent(typeof(SpriteRenderer));
                        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                        spriteRenderer.sprite = Resources.Load<Sprite>("ffff/I");
                        movesNum++;
                    }
                    break;
                }
            case "Clear":
                {
                    for(int i = 1; i < movesNum; i++)
                    {
                        GameObject gameObject = GameObject.Find("Field" + i);
                        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                        Destroy(spriteRenderer);
                    }
                    moves.Clear();
                    movesNum = 1;
                    break;
                }
            case "Execute":
                {
                    foreach(string move in moves)
                    {
                        switch (move)
                        {
                            case "L":
                                {
                                    int source_target_y = playerLocation / 10;
                                    int source_x = playerLocation % 10;
                                    if (source_x == 0) {
                                        UpdateMap();
                                        break;
                                    }
                                    int target_x = source_x - 1;
                                    int id = field_ids[source_target_y, target_x];
                                    int source_id = field_ids[source_target_y, source_x];
                                    if (id == 6 || id == 7 || id == 8 || id == 9 || id == 10)
                                    {
                                        if (source_id == 6 || source_id == 7 || source_id == 8 || source_id == 9 || source_id == 10)
                                        {
                                            playerLocation--;
                                            UpdateMap();
                                            break;
                                        }
                                        if (source_id != 3)
                                        {
                                            UpdateMap();
                                            break;
                                        }
                                        bool hasEntranceLeft = false;
                                        int entry_id = 0;
                                        for(int i = 0; i < 22; i++)
                                        {
                                            if (entrances[i] == playerLocation)
                                            {
                                                entry_id = i;
                                                break;
                                            }
                                        }
                                        string direction = whereToGo[entry_id];
                                        if (direction.Length > 1)
                                        {
                                            string[] directions = direction.Split('|');
                                            foreach (string dir in directions)
                                            {
                                                if (dir == "L") hasEntranceLeft = true;
                                            }
                                        }
                                        else hasEntranceLeft = (direction == "L");
                                        if ((id == 7 || id == 8) && unavailableArea) hasEntranceLeft = false;
                                        if (hasEntranceLeft)
                                        {
                                            playerLocation--;
                                        }
                                        UpdateMap();
                                        break;
                                    }
                                    if (source_id == 6 || source_id == 7 || source_id == 8 || source_id == 9 || source_id == 10)
                                    {
                                        if (id != 3)
                                        {
                                            UpdateMap();
                                            break;
                                        }
                                        bool hasExitLeft = false;
                                        int entry_id = 0;
                                        for (int i = 0; i < 22; i++)
                                        {
                                            if (entrances[i] == playerLocation-1)
                                            {
                                                entry_id = i;
                                                break;
                                            }
                                        }
                                        string direction = whereToGo[entry_id];
                                        if (direction.Length > 1)
                                        {
                                            string[] directions = direction.Split('|');
                                            foreach (string dir in directions)
                                            {
                                                if (dir == "R") hasExitLeft = true;
                                            }
                                        }
                                        else hasExitLeft = (direction == "R");
                                        if (hasExitLeft) { 
                                            playerLocation--;
                                        }
                                        foreach(int enemy in enemiesLocation)
                                        {
                                            if (playerLocation == enemy)
                                            {
                                                // here will be the code of enemy encounter
                                            }
                                        }
                                        UpdateMap();
                                        break;
                                    }
                                    playerLocation--;
                                    foreach (int enemy in enemiesLocation)
                                    {
                                        if (playerLocation == enemy)
                                        {
                                            // here will be the code of enemy encounter
                                        }
                                    }
                                    UpdateMap();
                                    break;
                                }
                            case "R":
                                {
                                    int source_target_y = playerLocation / 10;
                                    int source_x = playerLocation % 10;
                                    if (source_x == 9)
                                    {
                                        UpdateMap();
                                        break;
                                    }
                                    int target_x = source_x + 1;
                                    int id = field_ids[source_target_y, target_x];
                                    int source_id = field_ids[source_target_y, source_x];
                                    if (id == 6 || id == 7 || id == 8 || id == 9 || id == 10)
                                    {
                                        if (source_id == 6 || source_id == 7 || source_id == 8 || source_id == 9 || source_id == 10)
                                        {
                                            playerLocation++;
                                            UpdateMap();
                                            break;
                                        }
                                        if (source_id != 3)
                                        {
                                            UpdateMap();
                                            break;
                                        }
                                        bool hasEntranceRight = false;
                                        int entry_id = 0;
                                        for (int i = 0; i < 22; i++)
                                        {
                                            if (entrances[i] == playerLocation)
                                            {
                                                entry_id = i;
                                                break;
                                            }
                                        }
                                        string direction = whereToGo[entry_id];
                                        if (direction.Length > 1)
                                        {
                                            string[] directions = direction.Split('|');
                                            foreach (string dir in directions)
                                            {
                                                if (dir == "R") hasEntranceRight = true;
                                            }
                                        }
                                        else hasEntranceRight = (direction == "R");
                                        if ((id == 7 || id == 8) && unavailableArea) hasEntranceRight = false;
                                        if (hasEntranceRight) playerLocation++;
                                        UpdateMap();
                                        break;
                                    }
                                    if (source_id == 6 || source_id == 7 || source_id == 8 || source_id == 9 || source_id == 10)
                                    {
                                        if (id != 3)
                                        {
                                            UpdateMap();
                                            break;
                                        }
                                        bool hasExitRight = false;
                                        int entry_id = 0;
                                        for (int i = 0; i < 22; i++)
                                        {
                                            if (entrances[i] == playerLocation + 1)
                                            {
                                                entry_id = i;
                                                break;
                                            }
                                        }
                                        string direction = whereToGo[entry_id];
                                        if (direction.Length > 1)
                                        {
                                            string[] directions = direction.Split('|');
                                            foreach (string dir in directions)
                                            {
                                                if (dir == "L") hasExitRight = true;
                                            }
                                        }
                                        else hasExitRight = (direction == "L");
                                        if (hasExitRight) playerLocation++;
                                        foreach (int enemy in enemiesLocation)
                                        {
                                            if (playerLocation == enemy)
                                            {
                                                // here will be the code of enemy encounter
                                            }
                                        }
                                        UpdateMap();
                                        break;
                                    }
                                    playerLocation++;
                                    foreach (int enemy in enemiesLocation)
                                    {
                                        if (playerLocation == enemy)
                                        {
                                            // here will be the code of enemy encounter
                                        }
                                    }
                                    UpdateMap();
                                    break;
                                }
                            case "U":
                                {
                                    int source_target_x = playerLocation % 10;
                                    int source_y = playerLocation / 10;
                                    if (source_y == 0)
                                    {
                                        UpdateMap();
                                        break;
                                    }
                                    int target_y = source_y - 1;
                                    int id = field_ids[target_y, source_target_x];
                                    int source_id = field_ids[source_y, source_target_x];
                                    if (id == 6 || id == 7 || id == 8 || id == 9 || id == 10)
                                    {
                                        if (source_id == 6 || source_id == 7 || source_id == 8 || source_id == 9 || source_id == 10)
                                        {
                                            playerLocation -= 10;
                                            UpdateMap();
                                            break;
                                        }
                                        if (source_id != 3)
                                        {
                                            UpdateMap();
                                            break;
                                        }
                                        bool hasEntranceUp = false;
                                        int entry_id = 0;
                                        for (int i = 0; i < 22; i++)
                                        {
                                            if (entrances[i] == playerLocation)
                                            {
                                                entry_id = i;
                                                break;
                                            }
                                        }
                                        string direction = whereToGo[entry_id];
                                        if (direction.Length > 1)
                                        {
                                            string[] directions = direction.Split('|');
                                            foreach (string dir in directions)
                                            {
                                                if (dir == "U") hasEntranceUp = true;
                                            }
                                        }
                                        else hasEntranceUp = (direction == "U");
                                        if ((id == 7 || id == 8) && unavailableArea) hasEntranceUp = false;
                                        if (hasEntranceUp) playerLocation -= 10;
                                        UpdateMap();
                                        break;
                                    }
                                    if (source_id == 6 || source_id == 7 || source_id == 8 || source_id == 9 || source_id == 10)
                                    {
                                        if (id != 3)
                                        {
                                            UpdateMap();
                                            break;
                                        }
                                        bool hasExitUp = false;
                                        int entry_id = 0;
                                        for (int i = 0; i < 22; i++)
                                        {
                                            if (entrances[i] == playerLocation - 10)
                                            {
                                                entry_id = i;
                                                break;
                                            }
                                        }
                                        string direction = whereToGo[entry_id];
                                        if (direction.Length > 1)
                                        {
                                            string[] directions = direction.Split('|');
                                            foreach (string dir in directions)
                                            {
                                                if (dir == "D") hasExitUp = true;
                                            }
                                        }
                                        else hasExitUp = (direction == "D");
                                        if (hasExitUp) playerLocation -= 10;
                                        foreach (int enemy in enemiesLocation)
                                        {
                                            if (playerLocation == enemy)
                                            {
                                                // here will be the code of enemy encounter
                                            }
                                        }
                                        UpdateMap();
                                        break;
                                    }
                                    playerLocation -= 10;
                                    foreach (int enemy in enemiesLocation)
                                    {
                                        if (playerLocation == enemy)
                                        {
                                            // here will be the code of enemy encounter
                                        }
                                    }
                                    UpdateMap();
                                    break;
                                }
                            case "D":
                                {
                                    int source_target_x = playerLocation % 10;
                                    int source_y = playerLocation / 10;
                                    if (source_y == 9)
                                    {
                                        UpdateMap();
                                        break;
                                    }
                                    int target_y = source_y + 1;
                                    int id = field_ids[target_y, source_target_x];
                                    int source_id = field_ids[source_y, source_target_x];
                                    if (id == 6 || id == 7 || id == 8 || id == 9 || id == 10)
                                    {
                                        if (source_id == 6 || source_id == 7 || source_id == 8 || source_id == 9 || source_id == 10)
                                        {
                                            playerLocation += 10;
                                            UpdateMap();
                                            break;
                                        }
                                        if (source_id != 3)
                                        {
                                            UpdateMap();
                                            break;
                                        }
                                        bool hasEntranceDown = false;
                                        int entry_id = 0;
                                        for (int i = 0; i < 22; i++)
                                        {
                                            if (entrances[i] == playerLocation)
                                            {
                                                entry_id = i;
                                                break;
                                            }
                                        }
                                        string direction = whereToGo[entry_id];
                                        if (direction.Length > 1)
                                        {
                                            string[] directions = direction.Split('|');
                                            foreach (string dir in directions)
                                            {
                                                if (dir == "D") hasEntranceDown = true;
                                            }
                                        }
                                        else hasEntranceDown = (direction == "D");
                                        if ((id == 7 || id == 8) && unavailableArea) hasEntranceDown = false;
                                        if (hasEntranceDown) playerLocation += 10;
                                        UpdateMap();
                                        break;
                                    }
                                    if (source_id == 6 || source_id == 7 || source_id == 8 || source_id == 9 || source_id == 10)
                                    {
                                        if (id != 3)
                                        {
                                            UpdateMap();
                                            break;
                                        }
                                        bool hasExitDown = false;
                                        int entry_id = 0;
                                        for (int i = 0; i < 22; i++)
                                        {
                                            if (entrances[i] == playerLocation + 10)
                                            {
                                                entry_id = i;
                                                break;
                                            }
                                        }
                                        string direction = whereToGo[entry_id];
                                        if (direction.Length > 1)
                                        {
                                            string[] directions = direction.Split('|');
                                            foreach (string dir in directions)
                                            {
                                                if (dir == "U") hasExitDown = true;
                                            }
                                        }
                                        else hasExitDown = (direction == "U");
                                        if (hasExitDown) playerLocation += 10;
                                        foreach (int enemy in enemiesLocation)
                                        {
                                            if (playerLocation == enemy)
                                            {
                                                // here will be the code of enemy encounter
                                            }
                                        }
                                        UpdateMap();
                                        break;
                                    }
                                    playerLocation += 10;
                                    foreach (int enemy in enemiesLocation)
                                    {
                                        if (playerLocation == enemy)
                                        {
                                            // here will be the code of enemy encounter
                                        }
                                    }
                                    UpdateMap();
                                    break;
                                }
                            case "I":
                                {
                                    // here will be the code of interactions
                                    break;
                                }
                        }
                    }
                    for (int i = 1; i < movesNum; i++)
                    {
                        GameObject gameObject = GameObject.Find("Field" + i);
                        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                        Destroy(spriteRenderer);
                    }
                    moves.Clear();
                    numOfMovesLeftForStep -= (movesNum - 1);
                    numOfMovesPerRound -= (movesNum - 1);
                    if (numOfMovesPerRound == 0)
                    {
                        numOfRounds--;
                        numOfMovesPerRound = 5;
                    }
                    movesNum = 1;
                    break;
                }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        /*
        0 - unused tile
        1 - road
        2 - building
        3 - road with entrance to building
        14 - unavailable area
        roads further should be filled with:
        11 - info source lv0
        12 - key
        13 - gold bag
        in buildings shall be:
        6 - shop
        7 - main info source lv2
        8 - additional info source lv2
        9 - main info source lv1
        10 - additional info source lv1
        */
        // there should be a code soon which loads up information from the level whether there should actually be an unavailable area or not
        System.Random rand = new System.Random();
        //player - mobile, no id
        int position = rand.Next(0, 1000) % 6;
        switch (position)
        {
            case 0:
                {
                    playerLocation = 2;
                    break;
                }
            case 1:
                {
                    playerLocation = 4;
                    break;
                }
            case 2:
                {
                    playerLocation = 50;
                    break;
                }
            case 3:
                {
                    playerLocation = 93;
                    break;
                }
            case 4:
                {
                    playerLocation = 97;
                    break;
                }
            case 5:
                {
                    playerLocation = 49;
                    break;
                }
        }
        //enemies - mobile, no id
        for (int i = 0; i < 5; i++){
            while (true)
            {
                position = rand.Next(0, 100);
                int x = position / 10;
                int y = position % 10;
                if ((field_ids[x, y] == 1 || field_ids[x,y]==3)&& x*10+y!=playerLocation)
                {
                    enemiesLocation[i] = x * 10 + y;
                    break;
                }
            }
        }
        // shop - id 6
        for (int i = 0; i < 3; i++)
        {
            while (true)
            {
                position = rand.Next(0, 100);
                int x = position / 10;
                int y = position % 10;
                if (field_ids[x, y] == 2)
                {
                    field_ids[x, y] = 6;
                    break;
                }
            }
        }
        // MIS lv 2 - id 7 (in unavailable area)
        for (int i = 0; i < 3; i++)
        {
            while (true)
            {
                position = rand.Next(0, 100);
                int x = position / 10;
                int y = position % 10;
                if (field_ids[x, y] == 14)
                {
                    field_ids[x, y] = 7;
                    break;
                }
            }
        }
        // AIS lv 2 - id 8 (in unavailable area)
        for (int i = 0; i < 7; i++)
        {
            while (true)
            {
                position = rand.Next(0, 100);
                int x = position / 10;
                int y = position % 10;
                if (field_ids[x, y] == 14)
                {
                    field_ids[x, y] = 8;
                    break;
                }
            }
        }
        // MIS lv 1 - id 9
        for (int i = 0; i < 14; i++)
        {
            while (true)
            {
                position = rand.Next(0, 100);
                int x = position / 10;
                int y = position % 10;
                if (field_ids[x, y] == 2)
                {
                    field_ids[x, y] = 9;
                    break;
                }
            }
        }
        // AIS lv 1 - id 10
        for (int i = 0; i < 24; i++)
        {
            while (true)
            {
                position = rand.Next(0, 100);
                int x = position / 10;
                int y = position % 10;
                if (field_ids[x, y] == 2)
                {
                    field_ids[x, y] = 10;
                    break;
                }
            }
        }
        // IS lv 0 - id 11
        for (int i = 0; i < 7; i++)
        {
            while (true)
            {
                position = rand.Next(0, 100);
                int x = position / 10;
                int y = position % 10;
                if (field_ids[x, y] == 1 || field_ids[x, y] == 3)
                {
                    field_ids[x, y] = 11;
                    break;
                }
            }
        }
        // key - id 12
        while (true)
        {
            position = rand.Next(0, 100);
            int x = position / 10;
            int y = position % 10;
            if (field_ids[x, y] == 1 || field_ids[x, y] == 3)
            {
                field_ids[x, y] = 12;
                break;
            }
        }
        // gold bag - id 13
        while (true)
        {
            position = rand.Next(0, 100);
            int x = position / 10;
            int y = position % 10;
            if (field_ids[x, y] == 1 || field_ids[x, y] == 3)
            {
                field_ids[x, y] = 13;
                break;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        GameObject info = GameObject.Find("Info field");
        TextMeshPro infotext = info.GetComponent<TextMeshPro>();
        infotext.text = "Rounds left: " + numOfRounds + "\nMoves left: " + numOfMovesLeftForStep + "\nMoves left per round: " + numOfMovesPerRound;
        UpdateMap();
    }
    public void UpdateMap()
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                map[i, j] = GameObject.Find(i + ":" + j);
                SpriteRenderer rend = map[i, j].GetComponent<SpriteRenderer>();
                switch (field_ids[i, j])
                {
                    case 0:
                        {
                            rend.sprite = Resources.Load<Sprite>("ffff/tile");
                            break;
                        }
                    case 1:
                        {
                            rend.sprite = Resources.Load<Sprite>("ffff/road");
                            break;
                        }
                    case 2:
                        {
                            rend.sprite = Resources.Load<Sprite>("ffff/unknown_build");
                            break;
                        }
                    case 3:
                        {
                            rend.sprite = Resources.Load<Sprite>("ffff/entrance");
                            break;
                        }
                    case 6:
                        {
                            if (discoveredPoints.Contains(i * 10 + j))
                                rend.sprite = Resources.Load<Sprite>("ffff/shop");
                            else
                                rend.sprite = Resources.Load<Sprite>("ffff/unknown_build");
                            break;
                        }
                    case 7:
                        {
                            if (unavailableArea) rend.sprite = Resources.Load<Sprite>("ffff/unavailable");
                            else if (discoveredPoints.Contains(i * 10 + j))
                                rend.sprite = Resources.Load<Sprite>("ffff/lv2m");
                            else
                                rend.sprite = Resources.Load<Sprite>("ffff/unknown_build");
                            break;
                        }
                    case 8:
                        {
                            if (unavailableArea) rend.sprite = Resources.Load<Sprite>("ffff/unavailable");
                            else if (discoveredPoints.Contains(i * 10 + j))
                                rend.sprite = Resources.Load<Sprite>("ffff/lv2a");
                            else
                                rend.sprite = Resources.Load<Sprite>("ffff/unknown_build");
                            break;
                        }
                    case 9:
                        {
                            if (discoveredPoints.Contains(i * 10 + j))
                                rend.sprite = Resources.Load<Sprite>("ffff/lv1m");
                            else
                                rend.sprite = Resources.Load<Sprite>("ffff/unknown_build");
                            break;
                        }
                    case 10:
                        {
                            if (discoveredPoints.Contains(i * 10 + j))
                                rend.sprite = Resources.Load<Sprite>("ffff/lv1a");
                            else
                                rend.sprite = Resources.Load<Sprite>("ffff/unknown_build");
                            break;
                        }
                    case 11:
                        {
                            if (discoveredPoints.Contains(i * 10 + j))
                                rend.sprite = Resources.Load<Sprite>("ffff/lv0");
                            else
                                rend.sprite = Resources.Load<Sprite>("ffff/unknown");
                            break;
                        }
                    case 12:
                        {
                            if (discoveredPoints.Contains(i * 10 + j))
                                rend.sprite = Resources.Load<Sprite>("ffff/key");
                            else
                            {
                                rend.sprite = Resources.Load<Sprite>("ffff/road");
                                foreach (int k in entrances)
                                {
                                    if (k == i * 10 + j)
                                    {
                                        rend.sprite = Resources.Load<Sprite>("ffff/entrance");
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    case 13:
                        {
                            if (discoveredPoints.Contains(i * 10 + j))
                                rend.sprite = Resources.Load<Sprite>("ffff/gold_bag");
                            else
                            {
                                rend.sprite = Resources.Load<Sprite>("ffff/road");
                                foreach (int k in entrances)
                                {
                                    if (k == i * 10 + j)
                                    {
                                        rend.sprite = Resources.Load<Sprite>("ffff/entrance");
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    case 14:
                        {
                            rend.sprite = Resources.Load<Sprite>("ffff/unavailable");
                            break;
                        }
                }
                for (int l = 0; l < 5; l++)
                {
                    if (enemiesEncountered[l]) continue;
                    int loc = enemiesLocation[l];
                    if (loc == i * 10 + j)
                    {
                        if (discoveredEnemies.Contains(l))
                            rend.sprite = Resources.Load<Sprite>("ffff/enemy");
                        else
                            rend.sprite = Resources.Load<Sprite>("ffff/road");
                        foreach (int k in entrances)
                        {
                            if (k == i * 10 + j)
                            {
                                rend.sprite = Resources.Load<Sprite>("ffff/entrance");
                                break;
                            }
                        }
                    }
                }
                if (playerLocation == i * 10 + j) rend.sprite = Resources.Load<Sprite>("ffff/player");
            }
        }
    }
}
