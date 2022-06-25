using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
	private bool _isOpened;
	[SerializeField] private Animator _animator;


    public int keyId; //значение id предмета, который будет открывать дверь
    public bool isLocked = false; //показывает, заперта дверь ключом или нет, если да - открытие будет происходить только при наличии необходимого ключа, иначе ключ не нужен
    public AudioSource _audioSource; //источник звука
    public AudioClip notLocked; //звук отпирания двери
    public AudioClip unlocked; //звук отпирания двери
    public AudioClip stillLocked; //звук запертой двери

    public void Open(int objectInArm)
	{
        bool openTry = TryToOpen(objectInArm); //проверка, можем ли мы открыть с предметом в активном слоте
        Debug.Log(openTry);
        if (openTry)
        {
            _animator.SetBool("isOpened", _isOpened);
            _isOpened = !_isOpened;
        }
            

	}
    public bool TryToOpen(int objectId)
    {
        if (isLocked) //если дверь заперта ключом, проходим проверку, подходит ли ключ (предмет в руке, берем его id)
        {
            if (objectId == keyId) // предмет подходит
            {
                if (_audioSource != null && unlocked != null) _audioSource.PlayOneShot(unlocked); //играем звук открытия/отпирания двери
                return true;
            }
            else // предмет не подошел, дверь заперта
            {
                if (_audioSource != null && stillLocked != null) _audioSource.PlayOneShot(stillLocked); //играем звук запертой двери
                return false;
            }
        }
        else //ключ не требуется, дверь не заперта
        {
            if (_audioSource != null && notLocked != null) _audioSource.PlayOneShot(notLocked); //играем звук открытия/отпирания двери
            return true;
        }
    }
}
