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
    public int x�apacity;
    public int y�apacity;
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
        x�apacity = 2;
        y�apacity = 2;
        torch.GetComponent<Renderer>().enabled = false;
        GameObject.FindGameObjectWithTag("PlayerTorch").GetComponent<Light>().intensity = 0;
        torch.GetComponent<AudioSource>().enabled = false;
    }
    void Update()
    {
        GameObject pauser = GameObject.FindWithTag("Pauser");
        GameObject torch = GameObject.Find("PlayerTorch");
        string s = GetComponent<Lib>().Type[GetComponent<Inventory>().fastSlot[GetComponent<Inventory>().activeSlot]];//��� ���
        float t = GetComponent<Lib>().Int[GetComponent<Inventory>().fastSlot[GetComponent<Inventory>().activeSlot]]; //��� ����

        
        if (Input.GetKeyDown(KeyCode.Tab) && pauser.GetComponent<Pauser>().globalPause == false && GetComponent<PlayerMenu>().guiPause == false) //�������� �� ��, ������ �� ��� ���������, ����� ����� ��� ������� ����
        {
            pauser.GetComponent<Pauser>().globalPause = true; //��������� ����� �����
            isInventoryActive = true; //����������� ���������
        }
        else if (Input.GetKeyDown(KeyCode.Tab) && pauser.GetComponent<Pauser>().globalPause == true && GetComponent<PlayerMenu>().guiPause == false)//�������� �� ��, ������ �� ��� ���������, ����� ����� ��� ������� ����
        {
            pauser.GetComponent<Pauser>().globalPause = false; //���������� ����� �����
            isInventoryActive = false;  //���������� ����������� ���������
        }

        if (Array.IndexOf(items, 0) == -1)
        {
            inventoryFull = true;
        } //�������� ����� �� ���������
        else
        {
            inventoryFull = false;
        }

        if (Input.GetKeyDown(KeyCode.E)) 
        {
            Pickup();
        }//���������� ���������

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            activeSlot = 0;
        }//������������ ����� ������� �������� �������
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            activeSlot = 1;
        }//������������ ����� ������� �������� �������
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            activeSlot = 2;
        }//������������ ����� ������� �������� �������
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            activeSlot = 3;
        }//������������ ����� ������� �������� �������

        if (Input.GetKeyDown(KeyCode.R))//������������� ���������
        {
            switch (lib.Type[fastSlot[activeSlot]]) //����������� ���� �������� � ����� �������� ������� � ������� ��� ����� ������������ � ������ ������
            {
                case "�����":
                    {
                        transform.GetComponent<IStatblock>().TakeHeal(lib.Int[fastSlot[activeSlot]]); //����� ������� �������������� �������� ������
                        fastSlot[activeSlot] = 0; //������� �����
                        break;
                    }
                case "����":
                    {
                        Unlock();//������� ���������� �����
                        break;
                    }
                case "�����":
                    {
                        torch.GetComponent<Renderer>().enabled = !torch.GetComponent<Renderer>().enabled; //������������ ��������� ������ ������
                        torch.GetComponent<AudioSource>().enabled = !torch.GetComponent<AudioSource>().enabled;// ������������ ����� ������ 
                        if (torchActive)//������������ ����� ������
                        {
                            GameObject.FindGameObjectWithTag("PlayerTorch").GetComponent<Light>().intensity = 0;
                        }
                        else
                        {
                            GameObject.FindGameObjectWithTag("PlayerTorch").GetComponent<Light>().intensity = 1;
                        }
                        torchActive = !torchActive;//������������ ��������� ����������
                        break;
                    }
            }
        }
    }
    void OnGUI()
    {
        float a = Screen.width / 13;
        if (inventoryShow && !GameObject.Find("Pauser").GetComponent<Pauser>().menuPause) //�������� ������� �� ��������� ��� �����
        {
            GameObject pauser = GameObject.FindWithTag("Pauser");
            if (isInventoryActive == true) //���������-���������� ���������  
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;// �������� ����������� �������
                for (int x = 0; x < x�apacity; x++) //��������� ��������� � ��������� � ����������� �� ���������� ������
                {
                    for (int y = 0; y < y�apacity; y++)
                    {
                        if (GUI.Button(new Rect(x * (a+a/10) + 20, y * (a + a / 10) + 20, a, a), lib.Images[items[y * x�apacity + x]]))
                        {
                            int loc = items[y * x�apacity + x];
                            items[y * x�apacity + x] = mouseSlot;
                            mouseSlot = loc;
                        }
                    }
                }
                GUI.DrawTexture(new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y, 50, 50), lib.Images[mouseSlot]);
                for (int x = 0; x < 4; x++)//���������-���������� �������������� �� ��������
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
            if (GameObject.FindGameObjectWithTag("NPC") != null) //�������� ���������� �� ���
            {
                if (!GameObject.FindGameObjectWithTag("NPC").GetComponent<InstantiateDialogue>().ShowDialogue) //���������-���������� ��� ���������� �������
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
        RaycastHit hit; //���� ����, �� ������� ��������
        if ((lib.Type[fastSlot[activeSlot]] == "����"))
        {
            if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out hit, lootrange))//���������, ������ �� �������� ������������ �������, ���� �� - �������� �������� �� ����������� �������
            {
                GameObject hitObject = hit.transform.gameObject;
                DoorLock target = hitObject.GetComponent<DoorLock>();
                GameObject openingDoor = GameObject.Find("Door");
                if (target != null)
                {
                    if (hitObject.GetComponent<DoorLock>().doorCode == lib.Int[fastSlot[activeSlot]] && !hitObject.GetComponent<Door>().isOpend) //�������� ���������� �� ���� � �� ������� �� �����, �� ������� �������� ����� �������� ����������
                    {
                        if (hitObject.GetComponent<DoorLock>().isLocked)
                        {
                            hitObject.GetComponent<DoorLock>().isLocked = false;
                            if (hitObject.GetComponent<Door>()._audioSource != null) hitObject.GetComponent<Door>()._audioSource.PlayOneShot(hitObject.GetComponent<Door>().unlocked); //������ ���� ���������/��������� �����
                        }
                        else
                        {
                            hitObject.GetComponent<DoorLock>().isLocked = true;
                            if (hitObject.GetComponent<Door>()._audioSource != null) hitObject.GetComponent<Door>()._audioSource.PlayOneShot(hitObject.GetComponent<Door>().unlocked); //������ ���� ���������/��������� �����
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
    } //���������� ����� �����
    void Pickup()
    {
        RaycastHit hit; //���� ����, �� ������� ��������
        if (!inventoryFull)
        {
            if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out hit, lootrange))//���������, ������ �� �������� ����������/�������, ���� �� - ������ ������ ����� � ����
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
    } //���������� ���������
}


