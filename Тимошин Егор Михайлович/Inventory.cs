using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{

    public class InventoryObject : ScriptableObject
    {
        public int id, mass;
        public Sprite icon;
        public bool stackable;
    }
    public float timer;
    public bool isLooting;
    public bool isInventoryActive;
    public bool guipause;
    public int[] items;
    public int mouseSlot;
    public Lib lib;
    public int xСapacity;
    public int yСapacity;
    public int[] fastSlot;
    public float lootrange = 2f;
    public Camera _cam;
    public bool inventoryFull;
    public int activeSlot = 0;
    public bool torchActive = false;
    public bool inventoryShow = true;

    //SmallItem[] smallItemsList = new SmallItem[25];
    //BigItem[] bigItemList = new BigItem[25];
    private void Start()
    {
        GameObject torch = GameObject.Find("PlayerTorch");
        xСapacity = 2;
        yСapacity = 2;
        torch.GetComponent<Renderer>().enabled = false;
        GameObject.FindGameObjectWithTag("PlayerTorch").GetComponent<Light>().intensity = 0;
        torch.GetComponent<AudioSource>().enabled = false;
    }
    void Update()
    {
        GameObject pauser = GameObject.FindWithTag("Pauser");
        GameObject torch = GameObject.Find("PlayerTorch");
        string s = GetComponent<Lib>().Type[GetComponent<Inventory>().fastSlot[GetComponent<Inventory>().activeSlot]];//Это тип
        float t = GetComponent<Lib>().Int[GetComponent<Inventory>().fastSlot[GetComponent<Inventory>().activeSlot]]; //Это урон

        
        if (Input.GetKeyDown(KeyCode.Tab) && pauser.GetComponent<Pauser>().globalPause == false && GetComponent<PlayerMenu>().guiPause == false) //Проверка на то, открыт ли уже инвентарь, стоит пауза или открыто меню
        {
            pauser.GetComponent<Pauser>().globalPause = true; //Включение общей паузы
            isInventoryActive = true; //Отображения инвентаря
        }
        else if (Input.GetKeyDown(KeyCode.Tab) && pauser.GetComponent<Pauser>().globalPause == true && GetComponent<PlayerMenu>().guiPause == false)//Проверка на то, открыт ли уже инвентарь, стоит пауза или открыто меню
        {
            pauser.GetComponent<Pauser>().globalPause = false; //Выключение общей паузы
            isInventoryActive = false;  //Отключение отображения инвентаря
        }

        if (Array.IndexOf(items, 0) == -1)
        {
            inventoryFull = true;
        } //Проверка полон ли инвентарь
        else
        {
            inventoryFull = false;
        }

        if (Input.GetKeyDown(KeyCode.E)) 
        {
            Pickup();
        }//Поднимание предметов

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            activeSlot = 0;
        }//Переключение между слотами быстрого доступа
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            activeSlot = 1;
        }//Переключение между слотами быстрого доступа
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            activeSlot = 2;
        }//Переключение между слотами быстрого доступа
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            activeSlot = 3;
        }//Переключение между слотами быстрого доступа

        if (Input.GetKeyDown(KeyCode.R))//Использование предметов
        {
            switch (lib.Type[fastSlot[activeSlot]]) //Определение типа предмета в слоте быстрого доступа и решение что нужно использовать в каждом случае
            {
                case "Зелье":
                    {
                        transform.GetComponent<IStatblock>().TakeHeal(lib.Int[fastSlot[activeSlot]]); //Вызов функции восстановления здоровья игрока
                        fastSlot[activeSlot] = 0; //Очистка слота
                        break;
                    }
                case "Ключ":
                    {
                        Unlock();//Функция открывания двери
                        break;
                    }
                case "Факел":
                    {
                        torch.GetComponent<Renderer>().enabled = !torch.GetComponent<Renderer>().enabled; //Переключение отрисовки модели факела
                        torch.GetComponent<AudioSource>().enabled = !torch.GetComponent<AudioSource>().enabled;// Переключение звука факела 
                        if (torchActive)//Переключение света факела
                        {
                            GameObject.FindGameObjectWithTag("PlayerTorch").GetComponent<Light>().intensity = 0;
                        }
                        else
                        {
                            GameObject.FindGameObjectWithTag("PlayerTorch").GetComponent<Light>().intensity = 1;
                        }
                        torchActive = !torchActive;//Переключение служебной переменной
                        break;
                    }
            }
        }
    }
    void OnGUI()
    {
        float a = Screen.width / 13;
        if (inventoryShow && !GameObject.Find("Pauser").GetComponent<Pauser>().menuPause) //Проверка включен ли инвентарь или пауза
        {
            GameObject pauser = GameObject.FindWithTag("Pauser");
            if (isInventoryActive == true) //включение-выключение инвентаря  
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;// включаем отображение курсора
                for (int x = 0; x < xСapacity; x++) //Отрисовка инвентаря с размерами в зависимости от разрешения экрана
                {
                    for (int y = 0; y < yСapacity; y++)
                    {
                        if (GUI.Button(new Rect(x * (a+a/10) + 20, y * (a + a / 10) + 20, a, a), lib.Images[items[y * xСapacity + x]]))
                        {
                            int loc = items[y * xСapacity + x];
                            items[y * xСapacity + x] = mouseSlot;
                            mouseSlot = loc;
                        }
                    }
                }
                GUI.DrawTexture(new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y, 50, 50), lib.Images[mouseSlot]);
                for (int x = 0; x < 4; x++)//включение-выключение взаимодействия на хотбарах
                {
                    if (x == activeSlot)
                    {
                        if (GUI.Button(new Rect(Screen.width / 2 + x * (a + a / 10) - 2 * (a + a / 10) - a/20, Screen.height - a / 20 - Screen.height / 5, a+ a / 10, a+ a / 10), lib.Images[fastSlot[x]]))
                        {

                            int loc = fastSlot[x];
                            fastSlot[x] = mouseSlot;
                            mouseSlot = loc;
                        }
                    }
                    else
                    {
                        if (GUI.Button(new Rect(Screen.width / 2 + x * (a + a / 10) - 2 * (a + a / 10), Screen.height - Screen.height / 5, a, a), lib.Images[fastSlot[x]]))
                        {

                            int loc = fastSlot[x];
                            fastSlot[x] = mouseSlot;
                            mouseSlot = loc;
                        }
                    }
                }
            }
            if (GameObject.FindGameObjectWithTag("NPC") != null) //Проверка существует ли НИП
            {
                if (!GameObject.FindGameObjectWithTag("NPC").GetComponent<InstantiateDialogue>().ShowDialogue) //включение-выключение при открывании диалога
                {
                    for (int x = 0; x < 4; x++)
                    {
                        if (x == activeSlot)
                        {
                            if (GUI.Button(new Rect(Screen.width / 2 + x * (a + a / 10) - 2 * (a + a / 10) - a / 20, Screen.height - a / 20 - Screen.height / 5, a + a / 10, a + a / 10), lib.Images[fastSlot[x]]))
                            {
                            }
                        }
                        else
                        {
                            if (GUI.Button(new Rect(Screen.width / 2 + x * (a + a / 10) - 2 * (a + a / 10), Screen.height - Screen.height / 5, a, a), lib.Images[fastSlot[x]]))
                            {


                            }
                        }
                    }
                }
            }
            else
            {
                for (int x = 0; x < 4; x++)
                {
                    if (x == activeSlot)
                    {
                        if (GUI.Button(new Rect(Screen.width / 2 + x * (a + a / 10) - 2 * (a + a / 10) - a / 20, Screen.height - a / 20 - Screen.height / 5, a + a / 10, a + a / 10), lib.Images[fastSlot[x]]))
                        {
                        }
                    }
                    else
                    {
                        if (GUI.Button(new Rect(Screen.width / 2 + x * (a + a / 10) - 2 * (a + a / 10), Screen.height - Screen.height/5, a, a), lib.Images[fastSlot[x]]))
                        {


                        }
                    }
                }
            }
        }
    }
    void Unlock()
    {
        RaycastHit hit; //наша цель, по которой попадаем
        if ((lib.Type[fastSlot[activeSlot]] == "Ключ"))
        {
            if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out hit, lootrange))//проверяем, задеты ли хитбоксы лутабельного объекта, если да - начинаем проверку на возможность поднять
            {
                GameObject hitObject = hit.transform.gameObject;
                DoorLock target = hitObject.GetComponent<DoorLock>();
                GameObject openingDoor = GameObject.Find("Door");
                if (target != null)
                {
                    if (hitObject.GetComponent<DoorLock>().doorCode == lib.Int[fastSlot[activeSlot]] && !hitObject.GetComponent<Door>().isOpend) //Проверка подходящий ли ключ и не открыта ли дверь, по скольку открытую дверь запереть невозможно
                    {
                        if (hitObject.GetComponent<DoorLock>().isLocked)
                        {
                            hitObject.GetComponent<DoorLock>().isLocked = false;
                            if (hitObject.GetComponent<Door>()._audioSource != null) hitObject.GetComponent<Door>()._audioSource.PlayOneShot(hitObject.GetComponent<Door>().unlocked); //играем звук отпирания/запирания двери
                        }
                        else
                        {
                            hitObject.GetComponent<DoorLock>().isLocked = true;
                            if (hitObject.GetComponent<Door>()._audioSource != null) hitObject.GetComponent<Door>()._audioSource.PlayOneShot(hitObject.GetComponent<Door>().unlocked); //играем звук отпирания/запирания двери
                        }
                    }
                    else
                    {
                    }
                }
            }
            else
            {
            }
        }
    } //Открывание замка двери
    void Pickup()
    {
        RaycastHit hit; //наша цель, по которой попадаем
        if (!inventoryFull)
        {
            if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out hit, lootrange))//проверяем, задеты ли хитбоксы противника/объекта, если да - делаем бросок атаки и урон
            {
                GameObject hitObject = hit.transform.gameObject;
                LootableItem target = hitObject.GetComponent<LootableItem>();
                if (target != null && target.lootable)
                {
                    items[Array.IndexOf(items, 0)] = target.id;
                    hitObject.SetActive(false);
                }
            }
        }
    } //Поднимание предметов
}


