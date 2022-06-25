using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactive : MonoBehaviour
{
	[SerializeField] private Camera _fpcCamera;
	private Ray _ray;
	private RaycastHit _hit;
	public bool canInterract = true;

	[SerializeField] private float _maxDistanceRay;


	private void Update()
	{
		Ray();
		DrawRay();
		if (canInterract) //если можем взаимодействовать
        {
			Interact();
		}
		
	}
	public void ChangeLockMode() //для установки значения, при false возможности взаимодействия нет
	{
		canInterract = !canInterract;
	}
	public void SetLockMode(bool mode) //для установки значения, при false возможности взаимодействия нет
	{
		canInterract = mode;
	}
	private void Ray()
	{
		_ray = _fpcCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
	}

	private void DrawRay()
	{
		if (Physics.Raycast(_ray, out _hit, _maxDistanceRay)) //если в пределах - рисуем зеленым
		{
			Debug.DrawRay(_ray.origin, _ray.direction * _maxDistanceRay, Color.blue);
		}

		if (_hit.transform == null) //луч не сталкивается ни с чем - рисуем красным
		{
			Debug.DrawRay(_ray.origin, _ray.direction * _maxDistanceRay, Color.red);
		}
	}
	private void Interact()
    {
		if (_hit.transform != null &&_hit.transform.GetComponent<Door>()) //взаимодействие с дверьми
        {
			//получаем значение айди объекта в руке игрока
			int objectInArm;
			if (transform.GetComponent<PlayerStatblock>() != null) objectInArm = 0;
			else objectInArm = transform.GetComponent<PlayerStatblock>().ActiveObjectID;

			Debug.DrawRay(_ray.origin, _ray.direction * _maxDistanceRay, Color.green);
			if (Input.GetKeyDown(KeyCode.E))
            {
				_hit.transform.GetComponent<Door>().Open(objectInArm);
            }
		}
		if (_hit.transform != null && _hit.transform.GetComponent<InstantiateDialogue>()) //взаимодействие с диалогами, НПС
		{
			Debug.DrawRay(_ray.origin, _ray.direction * _maxDistanceRay, Color.green);
			if (Input.GetKeyDown(KeyCode.E))
			{
				_hit.transform.GetComponent<InstantiateDialogue>().isInRange = true;
			}
            else
            {
				_hit.transform.GetComponent<InstantiateDialogue>().isInRange = false;
			}
		}
	}
}
