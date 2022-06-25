using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelPlayerQuestPrefs : MonoBehaviour
{
    void Start()
    {
        PlayerPrefs.DeleteKey("Quest1");     
    }
}
