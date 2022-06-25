using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pauser : MonoBehaviour
{
    public bool globalPause;
    public bool menuPause;
    // Start is called before the first frame update
    public void Start()
    {
        globalPause  = false;
        menuPause = false;
    }
    void Update()
    {
        int timer;
        timer = 1;
        Time.timeScale = timer;

        if (globalPause)
        {
            Cursor.visible = true;
            PlayerStatblock.PlayerLock(false);
            timer = 0;
        }
        else
        {
            timer = 1;
            Cursor.visible = false;
            PlayerStatblock.PlayerLock(true);
        }
    }

}
