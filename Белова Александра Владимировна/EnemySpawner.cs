using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    
    public GameObject myPrefab;// ����� �������, ������� ����� ��������
    public GameObject targetForPrefab;// ����� �������, ������� ����� ����� ����� � ������������� �������, ���� ���������
    public Bounds spawnerBoundBox; //����� ���� ������ ��������, ������� �������
    public List<Object> listOfPrefabs = new List<Object>(); // ������ ������������ �������� ��� �� �������� � ������
    public int maxCapacityOfArea = 0; // ������������ ���������� �������� ������� ������������� � �������� � ��������� ����
    public float spawnCooldown = 10f; // ����� �������� ��� ������ ��������
    private bool canSpawn = true; //

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (canSpawn)
        {
            if (myPrefab != null) //���������, ��� ������ ������
            {
                if (listOfPrefabs.Count < maxCapacityOfArea) //���������, ��� � �������� �����������/������� ������ ��������, ��� �������� �������� - ����� ������� ��� ������
                {
                    Vector3 pointToSpawn = GetRandomPoint();
                    GameObject obj = Instantiate(myPrefab, pointToSpawn, Quaternion.identity);
                    StartCoroutine(WaitCooldown()); // ��� �������� ������� ��������� �������
                    listOfPrefabs.Add(obj); 
                    if (obj.GetComponent<AIMotor>() != null)
                    {
                        obj.GetComponent<AIMotor>().boundBox = spawnerBoundBox; //����������� ���� �������� ��� ���� ��, �� � ������� ��� �� ���������� ��� �����������
                        obj.GetComponent<AIMotor>().target = targetForPrefab.transform; //����������� ���� ��� ������������� � ���������� �����
                    }
                    if (obj.GetComponent<Statblock>() != null)
                    {
                        obj.GetComponent<Statblock>().targetToAttack = targetForPrefab.transform; //����������� ���� ��� ����� � ���������� �����
                    }
                }
            }
        }
        for(int i = 0; i < listOfPrefabs.Count; i++) 
        {
            Object obj = listOfPrefabs[i]; //��������, ��� �� ������� ��� ����������/����
            if (obj == null) listOfPrefabs.Remove(obj);
        }
        
    }
    IEnumerator WaitCooldown()
    {
        canSpawn = false;
        yield return new WaitForSeconds(spawnCooldown);
        canSpawn = true;
    }
    Vector3 GetRandomPoint()
    {
        //����� ����� � �������� �������
        float randomX = Random.Range(-spawnerBoundBox.extents.x + spawnerBoundBox.center.x, spawnerBoundBox.extents.x + spawnerBoundBox.center.x);
        float randomZ = Random.Range(-spawnerBoundBox.extents.z + spawnerBoundBox.center.z, spawnerBoundBox.extents.z + spawnerBoundBox.center.z);
        float randomY = 0;
        if (myPrefab != null) randomY = myPrefab.transform.position.y;
        Vector3 point = new Vector3(randomX, randomY, randomZ); //����� ���� �� ������ ������� �� ������, ��� �� �������, ����� �������� ������
        return point;
    }
    private void OnDrawGizmos() // ��������� ������� ���� ������
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(spawnerBoundBox.center, spawnerBoundBox.size);
    }
}
