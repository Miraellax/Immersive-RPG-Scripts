using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum BehaviourState {none, npc, wander, patrol, chase, dead}  //��������� ��

public class AIMotor : MonoBehaviour
{
    public BehaviourState initialState;
    public BehaviourState currentState = BehaviourState.none;
    private NavMeshAgent agent;
    private Vector3 targetPos; //����� ��� �������

    [Header("NPC Settings")]
    public bool isNPC;
    public bool isAggro;

    [Header("Wander Settings")]
    public Bounds boundBox;  //�������, ������ ������� ����� �������������� ����� ��� ������� ��
    public float wanderingSpeed; 

    [Header("Patrol Settings")]
    public Transform[] patrolPoints; //������ ����� ��� ��������������
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
    private void Awake() //����� ��������� � �����
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        
        SetState(initialState); //������� ���� ������ ��������� �� ����������
    }

    void Update()
    {
        if (isDead) //����  ����� - ������������� ���
        {
            SetState(BehaviourState.dead);
        }
        else
        {
            //����� � ������������� ����(������)
            if (target != null & isAggro ) //������ ���� ���� ���� ������������� � �����������
            {
                float targetDistance = Vector3.Distance(target.position, transform.position);
                if (targetDistance <= chaseDistance) //�������� ������������ ���� � ���� ������ � �����������
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
                else //������������ � ����������� ��������� ���� ���� ����� �� ������� ������
                {
                    SetState(initialState);
                }

                if (currentState == BehaviourState.chase && Vector3.Distance(targetPos, transform.position) <= 2f) //��������, ����� �� �� ���������� ������ � ���� �������������
                {
                    agent.speed = 0f;
                }
                else if (currentState == BehaviourState.chase)
                {
                    agent.speed = chasingSpeed;
                }
            }

            //��������� ���������
            float distance = Vector3.Distance(targetPos, transform.position);
            if (distance <= agent.stoppingDistance) //���� �������� � �����  �������, �� ������ ��� �����������, ��� �������� ��������� ����� �����
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

    //�� ��������� ������ �������� ������������ ��
    void SetState(BehaviourState s)
    {
        if (currentState != s)
        {
            currentState = s;
            if (currentState == BehaviourState.wander)
            {
                agent.speed = wanderingSpeed;
                //����� ����� ����� �������
                FindNewWanderTarget();
            }
            else if (currentState == BehaviourState.patrol)
            {
                agent.speed = patrollingSpeed;
                //���� � ��������� ����� �������, �������� ��� ��� - ������ � ����������
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
        //����� ��������� ����� ������ ����-�������
        targetPos = GetRandomPoint();
        //��������� �� ��������, ����� �������������
        StartCoroutine(WaitCooldown()); 
        //������ ���� ��� ��
        agent.SetDestination(targetPos);
        //��������� ��� �� ����� ��������� (�� stopped)
        agent.isStopped = false;
    }
    IEnumerator WaitCooldown()
    {
        //���������� �������
        float speedMemory = agent.speed;
        agent.speed = 0f;
        //����
        yield return new WaitForSeconds(1f);
        //���������� ����������
        agent.speed = speedMemory;
        StopCoroutine(WaitCooldown());
    }
   
    Vector3 GetRandomPoint()
    {
        bool ok = false;
        Vector3 point = Vector3.zero;
        while (!ok) //���������, ��� ����� �����
        {
            //����� ����� � �������� �������, �� ���, ����� �� ���� ����� �� ��� � ������� (�� ��� ������������ ���������� �������)
            float randomX = Random.Range(-boundBox.extents.x + agent.radius + boundBox.center.x, boundBox.extents.x - agent.radius + boundBox.center.x);
            float randomZ = Random.Range(-boundBox.extents.z + agent.radius + boundBox.center.z, boundBox.extents.z - agent.radius + boundBox.center.z);
            point = new Vector3(randomX, transform.position.y, randomZ); //����� ���� �� ������ ��������� �� ������

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
            //���� � ���� ����� ���� �� ��������� �����������
            targetPos = GetPatrolPoint();
        }
        else
        {
            //���� � ��������� �����
            targetPos = GetPatrolPoint(true);
        }
        //������ ����� ����� ��� �������
        agent.SetDestination(targetPos);
        //��������� ��� �� ����� ��������� (�� stopped)
        agent.isStopped = false;
    }

    Vector3 GetPatrolPoint(bool random = false)
    {
        if (random == false) //��������������� �� ������
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
                //����� ��������� ����� ��� �������
                return GetClosestPatrolPoint();

            }
        }
        else //������� �������� �� ������
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
        Gizmos.DrawWireCube(boundBox.center, boundBox.size); //������� ��� ����� �������
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetPos, 0.2f); //���� �������
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chaseDistance); //���� ������ ��
    }
}
