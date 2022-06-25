using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalEventManager : MonoBehaviour
{
    public static Action OnEnemyHealthEnd;
    public static Action OnPlayerHealthEnd;
    public static void SendEnemyHealthEnd()
    {
        Debug.Log($"Sent about death");
        if (OnEnemyHealthEnd != null) OnEnemyHealthEnd();
    }
    public static void SendPlayerHealthEnd()
    {
        Debug.Log("Sent about death");
        if (OnPlayerHealthEnd != null) OnPlayerHealthEnd();
    }
}
