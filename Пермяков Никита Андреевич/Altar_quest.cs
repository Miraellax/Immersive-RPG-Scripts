using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Altar_quest : MonoBehaviour
{
    public Animator altar_rune;
    public AudioSource _audioSource;
    public AudioClip _audioClip;

    private void OnTriggerEnter(Collider collision)
    {
        
        if (collision.tag == "Player")
        {
            if (PlayerPrefs.GetInt("Quest1") == 1)
            {
                Debug.Log(collision.name + " зашел в алтарь");

                if (_audioSource != null && _audioClip != null) _audioSource.PlayOneShot(_audioClip);
                altar_rune.SetTrigger(name: "collapse_trigger");

                PlayerPrefs.SetInt("Quest1", 2);
            }
        }
    }
    

    private void OnTriggerExit(Collider collision)
    {
        Debug.Log(collision.name + " вышел из алтаря");
    }
}
