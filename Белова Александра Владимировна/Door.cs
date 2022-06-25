using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
	private bool _isOpened;
	[SerializeField] private Animator _animator;


    public int keyId; //�������� id ��������, ������� ����� ��������� �����
    public bool isLocked = false; //����������, ������� ����� ������ ��� ���, ���� �� - �������� ����� ����������� ������ ��� ������� ������������ �����, ����� ���� �� �����
    public AudioSource _audioSource; //�������� �����
    public AudioClip notLocked; //���� ��������� �����
    public AudioClip unlocked; //���� ��������� �����
    public AudioClip stillLocked; //���� �������� �����

    public void Open(int objectInArm)
	{
        bool openTry = TryToOpen(objectInArm); //��������, ����� �� �� ������� � ��������� � �������� �����
        Debug.Log(openTry);
        if (openTry)
        {
            _animator.SetBool("isOpened", _isOpened);
            _isOpened = !_isOpened;
        }
            

	}
    public bool TryToOpen(int objectId)
    {
        if (isLocked) //���� ����� ������� ������, �������� ��������, �������� �� ���� (������� � ����, ����� ��� id)
        {
            if (objectId == keyId) // ������� ��������
            {
                if (_audioSource != null && unlocked != null) _audioSource.PlayOneShot(unlocked); //������ ���� ��������/��������� �����
                return true;
            }
            else // ������� �� �������, ����� �������
            {
                if (_audioSource != null && stillLocked != null) _audioSource.PlayOneShot(stillLocked); //������ ���� �������� �����
                return false;
            }
        }
        else //���� �� ���������, ����� �� �������
        {
            if (_audioSource != null && notLocked != null) _audioSource.PlayOneShot(notLocked); //������ ���� ��������/��������� �����
            return true;
        }
    }
}
