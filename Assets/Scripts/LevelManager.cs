using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            Replay();
        }
    }

    public void Replay()
    {
        Application.LoadLevel(Application.loadedLevel);
        Time.timeScale = 1;
        
    }

    public void Quit()
    {
        Application.Quit();
        print("Quit");
    }


    
}
