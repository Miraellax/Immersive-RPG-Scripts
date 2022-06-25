using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum BehaviourState {none, npc, wander, patrol, chase, dead}  //состо€ни€ »»

public class AIMotor : MonoBehaviour
{
    public BehaviourState initialState;
    public BehaviourState currentState = BehaviourState.none;
    private NavMeshAgent agent;
    private Vector3 targetPos; //точка дл€ патрул€

    [Header("NPC Settings")]
    public bool isNPC;
    public bool isAggro;

    [Header("Wander Settings")]
    public Bounds boundBox;  //коробка, внутри которой будут генерироватьс€ точки дл€ патрул€ »»
    public float wanderingSpeed; 

    [Header("Patrol Settings")]
    public Transform[] patrolPoints; //список точек дл€ патрулировани€
    public bool randomSequence = false;
    public float patrollingSpeed;

    [Header("Chase Settings")]
    public float chaseDistance = 2f;
    public Transform target;
    public float chasingSpeed;

    public bool isDead = false;
    public bool IsDead
    {
        set
        {
            isDead = value;
        }
        get
        {
            return isDead;
        }
    }
    public bool IsAggro
    {
        set
        {
            isAggro = value;
        }
        get
        {
            return isAggro;
        }
    }
    private void Awake() //берем компонент с врага
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        
        SetState(initialState); //сначала игры ставим состо€ние из инспектора
    }

    void Update()
    {
        if (isDead) //если  мертв - останавливаем все
        {
            SetState(BehaviourState.dead);
        }
        else
        {
            //поиск и преследование цели(игрока)
            if (target != null & isAggro ) //только если есть цель преследовани€ и агрессивный
            {
                float targetDistance = Vector3.Distance(target.position, transform.position);
                if (targetDistance <= chaseDistance) //начинаем преследовать если в поле зрени€ и агрессивный
                {
                    if (currentState != BehaviourState.chase)
                    {
                        SetState(BehaviourState.chase);
                    }
                    else
                    {
                        targetPos = target.position;
                        agent.SetDestination(targetPos);
                    }
                }
                else //возвращаемс€ к предыдущему состо€нию если цель вышла из радиуса зрени€
                {
                    SetState(initialState);
                }

                if (currentState == BehaviourState.chase && Vector3.Distance(targetPos, transform.position) <= 2f) //проверка, стоим ли мы достаточно близко к цели преследовани€
                {
                    agent.speed = 0f;
                }
                else if (currentState == BehaviourState.chase)
                {
                    agent.speed = chasingSpeed;
                }
            }

            //дистанции остановки
            float distance = Vector3.Distance(targetPos, transform.position);
            if (distance <= agent.stoppingDistance) //если подходим к точке  смотрим, мы бродим или патрулируем, там выбираем следующую точку целью
            {
                agent.isStopped = true;
                if (currentState == BehaviourState.wander)
                {
                    FindNewWanderTarget();
                }
                else if (currentState == BehaviourState.patrol)
                {
                    GoToNextPatrolPoint(randomSequence);
                }
            }
            else if (agent.isStopped == true)
            {
                agent.isStopped = false;
            }
        }
    }

    //от состо€ни€ мен€ем скорость передвижени€ »»
    void SetState(BehaviourState s)
    {
        if (currentState != s)
        {
            currentState = s;
            if (currentState == BehaviourState.wander)
            {
                agent.speed = wanderingSpeed;
                //найти новую точку патрул€
                FindNewWanderTarget();
            }
            else if (currentState == BehaviourState.patrol)
            {
                agent.speed = patrollingSpeed;
                //идти к следующей точке патрул€, рандомно или нет - решаем в инспекторе
                GoToNextPatrolPoint(randomSequence);
            }
            else if (currentState == BehaviourState.chase)
            {

                targetPos = target.position;
                agent.SetDestination(targetPos);
                agent.speed = chasingSpeed;
                
            }
            else if (currentState == BehaviourState.dead)
            {
                agent.speed = 0f;
            }
        }
    }
    void FindNewWanderTarget()
    {
        //берем рандомную точку внутри зоны-коробки
        targetPos = GetRandomPoint();
        //остановка на подумать, часть иммерсивности
        StartCoroutine(WaitCooldown()); 
        //задаем цель дл€ »»
        agent.SetDestination(targetPos);
        //провер€ем что »» может двигатьс€ (не stopped)
        agent.isStopped = false;
    }
    IEnumerator WaitCooldown()
    {
        //выполнение функций
        float speedMemory = agent.speed;
        agent.speed = 0f;
        //ждем
        yield return new WaitForSeconds(1f);
        //продолжаем выполнение
        agent.speed = speedMemory;
        StopCoroutine(WaitCooldown());
    }
   
    Vector3 GetRandomPoint()
    {
        bool ok = false;
        Vector3 point = Vector3.zero;
        while (!ok) //провер€ем, что можем дойти
        {
            //берем точку в пределах коробки, но так, чтобы »» смог дойти до нее в навмеше (не при динамическом обновлении локации)
            float randomX = Random.Range(-boundBox.extents.x + agent.radius + boundBox.center.x, boundBox.extents.x - agent.radius + boundBox.center.x);
            float randomZ = Random.Range(-boundBox.extents.z + agent.radius + boundBox.center.z, boundBox.extents.z - agent.radius + boundBox.center.z);
            point = new Vector3(randomX, transform.position.y, randomZ); //берем цель на уровне персонажа по высоте

            NavMeshPath path = new NavMeshPath();
            bool hasFoundPath = agent.CalculatePath(point, path);

            if (path.status == NavMeshPathStatus.PathComplete)
            {
                ok = true;
            }
            else
            {
                ok = false;
            }
        }
        return point;
    }

    void GoToNextPatrolPoint(bool random = false)
    {
        if (random == false)
        {
            //идти к след точке если не рандомноо патрулируем
            targetPos = GetPatrolPoint();
        }
        else
        {
            //идем к рандомной точке
            targetPos = GetPatrolPoint(true);
        }
        //задаем новую точку дл€ патрул€
        agent.SetDestination(targetPos);
        //провер€ем что »» может двигатьс€ (не stopped)
        agent.isStopped = false;
    }

    Vector3 GetPatrolPoint(bool random = false)
    {
        if (random == false) //последовательно по точкам
        {
            if(targetPos == Vector3.zero)
            {
                return patrolPoints[0].position;
            }
            else
            {
                Vector3 newPoint = Vector3.zero;
                for (int i = 0; i < patrolPoints.Length; i++)
                {
                    if (patrolPoints[i].position == targetPos)
                    {
                        if(i+1 >= patrolPoints.Length)
                        {
                            return patrolPoints[0].position;
                        }
                        else
                        {
                            return patrolPoints[i + 1].position;
                        }
                    }
                }
                //берем ближайшую точку дл€ патрул€
                return GetClosestPatrolPoint();

            }
        }
        else //патруль рандомно по точкам
        {
            return patrolPoints[Random.Range(0, patrolPoints.Length)].position;
        }
    }

    Vector3 GetClosestPatrolPoint()
    {
        Transform closest = null;
        foreach (Transform t in patrolPoints)
        {
            if (closest == null)
            {
                closest = t;
            }
            else if (Vector3.Distance(transform.position, t.position) < Vector3.Distance(transform.position, closest.position))
            {
                closest = t;
            }
        }
        return closest.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(boundBox.center, boundBox.size); //коробка дл€ точек патрул€
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetPos, 0.2f); //цель патрул€
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chaseDistance); //поле зрени€ »»
    }
}
