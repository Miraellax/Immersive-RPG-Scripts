using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InstantiateDialogue : MonoBehaviour
{
    public TextAsset[] ta;
    public int currentTa = 0;
    public Dialogue dialog;
    public int currentNode;
    public bool ShowDialogue;

    public GUISkin skin;

    public List<Answer> answers = new List<Answer>();

    public bool isInRange = false;
    public object obj;

    void Start()
    {
        dialog = Dialogue.Load(ta[currentTa]);
        skin = Resources.Load("Skin") as GUISkin;
        UpdateAnswers();
    }
    private void Update() //проверка на наличие игрока в поле нпс и на нажатие клавиши для начала диалога
    {
        GameObject pauser = GameObject.FindWithTag("Pauser");
        GameObject gui = GameObject.FindWithTag("Player");

        if (isInRange && ShowDialogue == false && GetComponent<Statblock>().IsDead == false && GetComponent<AIMotor>().IsAggro == false && gui.GetComponent<PlayerMenu>().guiPause == false)
        {
            Debug.Log(obj + " начал диалог");
            ShowDialogue = true;
            pauser.GetComponent<Pauser>().globalPause = true;

        }
        if (ShowDialogue) pauser.GetComponent<Pauser>().globalPause = true;
    }


    void UpdateAnswers()
    {
        answers.Clear();
        for (int i = 0; i < dialog.nodes[currentNode].answers.Length; i++)
        {
            if (dialog.nodes[currentNode].answers[i].questName == null || dialog.nodes[currentNode].answers[i].needQuestValue == PlayerPrefs.GetInt(dialog.nodes[currentNode].answers[i].questName))
                answers.Add(dialog.nodes[currentNode].answers[i]);
        }
    }

    void OnGUI()
    {
        GUI.skin = skin;
        if (ShowDialogue)
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>().inventoryShow = false;
            GameObject pauser = GameObject.FindWithTag("Pauser");
            //PlayerStatblock.PlayerLock(false);

            UpdateAnswers();
            Cursor.lockState = CursorLockMode.None;
            GUI.Box(new Rect(Screen.width / 2 - 300, Screen.height - 300, 600, 250), "");
            GUI.Label(new Rect(Screen.width / 2 - 250, Screen.height - 280, 500, 90), dialog.nodes[currentNode].npcText);
            for (int i = 0; i < answers.Count; i++)
            {
                if (GUI.Button(new Rect(Screen.width / 2 - 250, Screen.height - 200 + 25 * i, 500, 25), answers[i].text))
                {
                    if (answers[i].questValue > 0)
                    {
                        PlayerPrefs.SetInt(answers[i].questName, answers[i].questValue);
                    }
                    if (answers[i].end == "true")
                    {

                        //PlayerStatblock.PlayerLock(true);

                        Debug.Log(obj + " закончил диалог"); //GetComponent<npctalk>().
                        Cursor.lockState = CursorLockMode.Locked;
                        ShowDialogue = false;
                        pauser.GetComponent<Pauser>().globalPause = false;
                    }
                    if (answers[i].rewardGold > 0)
                    {
                        Debug.Log($"+ {answers[i].rewardGold} золота");
                    }
                    currentNode = answers[i].nextNode;
                }
            }
        }
        else
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>().inventoryShow = true;
        }
    }
}