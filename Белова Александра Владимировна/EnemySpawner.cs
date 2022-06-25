using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    
    public GameObject myPrefab;// Выбор префаба, который будем спавнить
    public GameObject targetForPrefab;// Выбор префаба, который будет целью атаки и преследования префаба, если необходим
    public Bounds spawnerBoundBox; //Выбор зоны спавна префабов, границы коробки
    public List<Object> listOfPrefabs = new List<Object>(); // список заспавненных префабов для их контроля и спавна
    public int maxCapacityOfArea = 0; // максимальное количество объектов префаба прикрепленных к спавнеру и выбранной зоне
    public float spawnCooldown = 10f; // время кулдауна для спавна префабов
    private bool canSpawn = true; //

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (canSpawn)
        {
            if (myPrefab != null) //проверяем, что префаб выбран
            {
                if (listOfPrefabs.Count < maxCapacityOfArea) //проверяем, что к спавнеру прикреплено/создано меньше объектов, чем заданный максимум - можно создать еще одного
                {
                    Vector3 pointToSpawn = GetRandomPoint();
                    GameObject obj = Instantiate(myPrefab, pointToSpawn, Quaternion.identity);
                    StartCoroutine(WaitCooldown()); // при создании объекта запускаем кулдаун
                    listOfPrefabs.Add(obj); 
                    if (obj.GetComponent<AIMotor>() != null)
                    {
                        obj.GetComponent<AIMotor>().boundBox = spawnerBoundBox; //присваиваем зону спавнера как зону ИИ, тк в префабе она не обозначена или некорректна
                        obj.GetComponent<AIMotor>().target = targetForPrefab.transform; //присваиваем цель для преследования в конкретной сцене
                    }
                    if (obj.GetComponent<Statblock>() != null)
                    {
                        obj.GetComponent<Statblock>().targetToAttack = targetForPrefab.transform; //присваиваем цель для атаки в конкретной сцене
                    }
                }
            }
        }
        for(int i = 0; i < listOfPrefabs.Count; i++) 
        {
            Object obj = listOfPrefabs[i]; //проверим, все ли объекты еще существуют/живы
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
        //берем точку в пределах коробки
        float randomX = Random.Range(-spawnerBoundBox.extents.x + spawnerBoundBox.center.x, spawnerBoundBox.extents.x + spawnerBoundBox.center.x);
        float randomZ = Random.Range(-spawnerBoundBox.extents.z + spawnerBoundBox.center.z, spawnerBoundBox.extents.z + spawnerBoundBox.center.z);
        float randomY = 0;
        if (myPrefab != null) randomY = myPrefab.transform.position.y;
        Vector3 point = new Vector3(randomX, randomY, randomZ); //берем цель на уровне префаба по высоте, или на нулевой, чтобы избежать ошибки
        return point;
    }
    private void OnDrawGizmos() // отрисовка коробки зоны спавна
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(spawnerBoundBox.center, spawnerBoundBox.size);
    }
}
