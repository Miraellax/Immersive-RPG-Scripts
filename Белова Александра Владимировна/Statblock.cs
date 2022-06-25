using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statblock : MonoBehaviour, IStatblock
{
    [Header("Main Settings")]
    bool isAggro = false;
    public Transform targetToAttack;
    public float health; //�� ���� �� ������ �� ���������� maxHealth
    public float defence;
    public int baseDamageDie = 4; //��� ����� �� ���������, ��� ������, ��������� ��� ������ � ����������
    public float attackRange; //��������� ��������� �����, ��� �������� ���
    public float attackCooldown = 5;
    public Animation allAnimations; //�������� 
    public AudioClip takeDamageSFX; //���� ��������� �����
    public AudioClip attackSFX; //����� ���� 
    public AudioSource _audioSource; //�������� ����� ����
    public Camera _cam; //��� �������� � �����

    //�������� ���������, �� ������� ���� �����, ��� ������ �����������?
    //���� � ���������� ���� ������ � ������/�����, ���� ���������

    [Header("Stats")]
    public float strength = 10; //����
    public float dexterity = 10; //��������
    public float constitution = 10; // ���������
    public float intelegence = 10; //���������
    public float wisdom = 10; //��������
    public float charisma = 10; //�������

    private float maxHealth; //������������ �������� ��������, ������������� �� �����
    private bool isDead = false;
    private bool canAttack = true; //��� �������� �����
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

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(this.GetType());
        //����������� ������� ������ �� ������� "��������� ��������"
        GlobalEventManager.OnEnemyHealthEnd += Die;

        maxHealth = health;
        //������������ �������������
        float strMod = (strength - 10) % 2;
        float dexMod = (dexterity - 10) % 2;
        float consMod = (constitution - 10) % 2;
        float intMod = (intelegence - 10) % 2;
        float wisMod = (wisdom - 10) % 2;
        float charMod = (charisma - 10) % 2;
        if (transform.FindChild("sword_epic") != null && 
            transform.FindChild("sword_epic").GetComponent<LootableItem>() != null) transform.FindChild("sword_epic").GetComponent<LootableItem>().lootable = false;
    }

    // Update is called once per frame
    void Update()
    {
        //�������� ��������� �� ��� ��������� �������� �������������
        AIMotor AI = transform.GetComponent<AIMotor>();
        if ( AI != null) isAggro = AI.isAggro;
        //���������, ��� �� ��, ����� �� ��������� � ���������� �� - �� �������� ������� ����
        if (canAttack & isAggro & !isDead) 
        {
            if (Vector3.Distance(targetToAttack.position, transform.position) <= attackRange)
            {
                transform.LookAt(targetToAttack);
                Attack(targetToAttack);
            }
        }
    }
    public void TakeDamage(float damage, int attackRoll)
    {
        if (health > 0) // ������� ���� 
        {
            //���� ������ �� ��������� ������ ������� ������ - ������� ����
            if (attackRoll >= defence || attackRoll == 20)
            {
                if (attackRoll == 20) // ����������� ���������
                {
                    health -= damage * 2;
                    //��� ���, ���� ����� ������� ��� - ������������� � ����� ��������
                    AIMotor motor = transform.GetComponent<AIMotor>();
                    //���������, ���� �� �� � ��� ���
                    if (motor != null && motor.isNPC == true)
                    {
                        motor.IsAggro = true;
                    }
                    //������ �������� ��������� �����, ���� ��� �������
                    if (allAnimations != null && allAnimations.GetClip("TakeDamage") != null)
                    {
                        allAnimations.Play("TakeDamage");
                    }
                    //���� ��������� �����
                    if (_audioSource != null && takeDamageSFX != null)
                    {
                        _audioSource.PlayOneShot(takeDamageSFX);
                    }
                }
                else // ������� ���������
                {
                    health -= damage;
                    //��� ���, ���� ����� ������� ��� - ������������� � ����� ��������
                    AIMotor motor = transform.GetComponent<AIMotor>(); 
                    //���������, ���� �� �� � ��� ���
                    if (motor != null && motor.isNPC == true) 
                    {
                        motor.IsAggro = true;
                    }
                    //������ �������� ��������� �����, ���� ��� �������
                    if (allAnimations != null && allAnimations.GetClip("TakeDamage") != null)
                    {
                        allAnimations.Play("TakeDamage");
                    }
                    //���� ��������� �����
                    if (_audioSource != null && takeDamageSFX != null)
                    {
                        _audioSource.PlayOneShot(takeDamageSFX);
                    }
                }
                if (health <= 0)
                {
                    //�������� ��������� � ������
                    GlobalEventManager.SendEnemyHealthEnd();
                    canAttack = false;
                    Die();
                }
            }
            else { }//������
        }
    }
    public void TakeHeal(float heal)
    {
        //����� ���� ���������� �� ������ ��� �������� ��������
        if (health + heal > maxHealth) //���� �������� ������������ ������, ��� ������������ ��������
        {
            health = maxHealth;
        }
        else //������ ��������� ������� �������������
        {
            health += heal;
        }
        //���� �������
        //��������� "������� ��������"

        //������� ��������� �������� �� ������ ������
        Debug.Log($"�������� {heal} ��������.");
        Debug.Log("��������: " + health);
    }

    public void Attack(Transform target) //����� ��������� ����
    {
        if (_audioSource != null && attackSFX != null)  _audioSource.PlayOneShot(attackSFX); //���� �����
        if (allAnimations != null && allAnimations.GetClip("TakeDamage") != null) allAnimations.Play("Attack"); //������ �������� �����, ���� ��� �������

        //������� ������ �� ����� + ����� ���������� + ����������� �� ������ �� ���������
        int attackRoll = Random.Range(0, 20 + 1); // + �� + ����� ����� �� ������
        int damageRoll = Random.Range(1, baseDamageDie + 1); // + ����� ����� �� ������

        //������� ���� � ����������
        if (target != null && target.GetComponent<PlayerStatblock>() != null && !target.GetComponent<PlayerStatblock>().isDead)
        {
            target.GetComponent<PlayerStatblock>().TakeDamage(damageRoll, attackRoll);
        }
        StartCoroutine(AttackCooldown(attackCooldown)); //����� �������� �����
    }
    public virtual void Attack() //����� ��������
    {
        if (_audioSource != null && attackSFX != null) _audioSource.PlayOneShot(attackSFX); //���� �����
        if (allAnimations != null && allAnimations.GetClip("Attack") != null) allAnimations.Play("Attack"); //������ �������� �����, ���� ��� �������
        RaycastHit hit; //���� ����, �� ������� ��������

        if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out hit, attackRange))//���������, ������ �� �������� ����������/�������, ���� �� - ������ ������ ����� � ����
        {
            //������� ������ �� ����� + ����� ���������� + ����������� �� ������ �� ���������
            int attackRoll = Random.Range(0, 20 + 1); // + �� + ����� ����� �� ������
            int damageRoll = Random.Range(1, baseDamageDie + 1); // + ����� ����� �� ������

            //������� ���� � ����������
            GameObject hitObject = hit.transform.gameObject;
            PlayerStatblock target = hitObject.GetComponent<PlayerStatblock>();
            if (target != null && !target.isDead) 
            {
                target.TakeDamage(damageRoll, attackRoll);  //�������� ��������� ����� � ���� ��������� �����
            }
        }
        else
        {
            //��� �����
        }
        StartCoroutine(AttackCooldown(attackCooldown)); //����� �������� �����
    }
    public void Die()
    {
        // ���������, ������ �� ����� ������� ��� ��������� ������
        if (health <= 0 & isDead == false)
        {
            isDead = true;
            Debug.Log($"{transform.name} died");
            AIMotor motor = transform.GetComponent<AIMotor>();
            //���������, ���� �� �� � ��������
            if (motor != null) 
            {
                motor.IsDead = true;
            }
            //������ �������� ������, ���� ����
            if (allAnimations != null && allAnimations.GetClip("Die") != null) 
            {
                allAnimations.Play("Die");
            }
            //��������, ����� ����� ���� � ������ ��������
            StartCoroutine(DeathCourutine(10));
        }
    }
    private IEnumerator DeathCourutine(int deathTime = 10) //���������� ��� ������
    {
        if (health <= 0)
        {
            //Debug.Log($"{transform.name} body will remain for {deathTime} seconds");
            //��������� ���� ���������� ��������� ��� ������, ������� �������� ����������� ��������� = true
            if (transform.FindChild("sword_epic") != null 
                && transform.FindChild("sword_epic").GetComponent<LootableItem>() != null) transform.FindChild("sword_epic").GetComponent<LootableItem>().lootable = true;
            //���� �����
            yield return new WaitForSeconds(deathTime);
            //�������� �������
            //gameObject.SetActive(false); 
            Destroy(gameObject);
        }

    }
    private IEnumerator AttackCooldown(float attackCooldown)
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    private IEnumerator Cooldown(int cooldown = 10)
    {
        yield return new WaitForSeconds(cooldown);
    }
}
