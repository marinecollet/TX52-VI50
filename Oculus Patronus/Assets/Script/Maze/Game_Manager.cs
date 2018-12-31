﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_Manager : MonoBehaviour
{
    public static bool isSetup = false;
    public Maze mazePrefab;
    public LevelSettings[] levels;
    public Player player;

    private Maze mazeInstance;
    private int actualLevel;
    


    private void Awake()
    {
        actualLevel = 1;
    }

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        BeginGame();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RestartGame();

        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void BeginGame()
    {

        mazeInstance = Instantiate(mazePrefab) as Maze;
        if (levels != null && levels.Length > 0)
        {
            mazeInstance.Generate(levels[actualLevel - 1]);
        }
        else
        {
            mazeInstance.Generate();
        }
        isSetup = true;

    }

    void RestartGame()
    {

        Destroy(mazeInstance.gameObject);
        isSetup = false;

        BeginGame();
    }

    public void NextLevel()
    {
        Destroy(mazeInstance.gameObject);
        if (levels != null && levels.Length > 0)
        {
            actualLevel++;
            if (actualLevel > levels.Length)
                actualLevel = levels.Length;
        }

        BeginGame();
        if(player is PlayerSansCasque)
            player.move(new Vector3(0, 1.6f, 0));
        else
            player.move(new Vector3(0, 0f, 0));
    }
}
