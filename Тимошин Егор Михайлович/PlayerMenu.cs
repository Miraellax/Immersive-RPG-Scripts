using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMenu : MonoBehaviour
{
    public float timer;
    public bool isPause;
    public bool guiPause;
    public GameObject controls;

    void Update()
    {
        GameObject pauser = GameObject.FindWithTag("Pauser");        

        //Time.timeScale = timer;
        if (Input.GetKeyDown(KeyCode.Escape) && (pauser.GetComponent<Pauser>().globalPause == false || GameObject.FindWithTag("Player").GetComponent<Inventory>().isInventoryActive == true))
        {
            pauser.GetComponent<Pauser>().globalPause = true;
            guiPause = true;
            GameObject.FindWithTag("Player").GetComponent<Inventory>().enabled = false;
            GameObject.FindWithTag("Player").GetComponent<Inventory>().isInventoryActive = false;
            //Здесь включить 
            controls.SetActive(true);
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && (pauser.GetComponent<Pauser>().globalPause == true ))
        {
            pauser.GetComponent<Pauser>().globalPause = false;
            guiPause = false;
            GameObject.FindWithTag("Player").GetComponent<Inventory>().enabled = true;
            //Здесь выключить
            controls.SetActive(false);
        }
    }

    public void OnGUI()
    {
        GameObject pauser = GameObject.FindWithTag("Pauser");

        if (guiPause == true)
        {
            Cursor.lockState = CursorLockMode.None;
            
            Cursor.visible = true;// включаем отображение курсора
            if (GUI.Button(new Rect((float)(Screen.width / 2) - 75f, (float)(Screen.height / 2) - 80f, 150f, 45f), "Продолжить"))
            {
                pauser.GetComponent<Pauser>().globalPause = false;
                guiPause = false;
                //timer = 1;
                GameObject.FindWithTag("Player").GetComponent<Inventory>().enabled = true;
                Cursor.visible = false;
                controls.SetActive(false);
            }
            if (GUI.Button(new Rect((float)(Screen.width / 2) - 75f, (float)(Screen.height / 2) - 20f, 150f, 45f), "В Меню"))
            {
                guiPause = false;
                //timer = 0;
                SceneTransition.SwitchToScene("test_menu");
                Cursor.visible = true;
                controls.SetActive(false);
            }
        }
    }
}
