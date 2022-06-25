using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStatblock : MonoBehaviour, IStatblock
{
    [Header("Main Settings")]
    public float health; //�� ���� �� ������ �� ���������� maxHealth
    public float defence; //�������� ������ �� ����
    public int baseDamageDie = 4; //��� ����� �� ���������, ��� ������, ��������� ��� ������ � ����������
    public float attackRange; //��������� ��������� �����, ��� �������� ���
    public float attackCooldown = 3; //����� ����� �������
    public int ActiveObjectID; //���� ��������� �������� � ����, � ������� ���������
    public GameObject weapon; //��������� ������, �� ������ ��� � ���������
    public AudioClip takeDamageSFX; //���� ��������� �����
    public AudioClip attackSFX; //���� ����� �������, ����� ������� ����� �� ������� ������
    public AudioClip emptyAttackSFX; //���� ������� �������
    public AudioSource _audioSource; //�������� ����� ����
    public Camera _cam; //��� �������� � �����
    public Animator componentAnimator;
    public GameObject controls;

    [Header("Stats")]
    public float strength = 10; //����
    public float dexterity = 10; //��������
    public float constitution = 10; // ���������
    public float intelegence = 10; //���������
    public float wisdom = 10; //��������
    public float charisma = 10; //�������

    private float maxHealth; //������������ �������� ��������, ������������� �� �����
    public bool isDead = false;
    private bool canAttack = true; //��� ���� �����
    private bool canAttackCooldown = true; //��� �������� �����

    [SerializeField]
    private UnityEvent<float> HpChagedPercent; 


    // Start is called before the first frame update
    void Start()
    {
        ////����������� ������� ������ �� ������� "��������� ��������"
        //GlobalEventManager.OnPlayerHealthEnd += Die;

        maxHealth = health;
        //������������ �������������
        float strMod = (strength - 10) % 2;
        float dexMod = (dexterity - 10) % 2;
        float consMod = (constitution - 10) % 2;
        float intMod = (intelegence - 10) % 2;
        float wisMod = (wisdom - 10) % 2;
        float charMod = (charisma - 10) % 2;
       
        //health = health + 0;    // ������������ ���������� � ������
        //speed = speed + 0;      // ������������ ���������� � ������
        //damage = damage + 0;    // ������������ ���������� � ������
    }

    // Update is called once per frame
    void Update()
    {
        ActiveObjectID = GetComponent<Inventory>().fastSlot[GetComponent<Inventory>().activeSlot]; //��������� ���������� � ��������, ��������� � ������� ���������

        if (canAttack & canAttackCooldown) //��������, ���������� �� ������� ���������� �����
        {
            if (Input.GetMouseButtonDown(0)) //�����
            {
                Attack();
            }
        }
        if (health <= 0 & !isDead) //��������, ��� ��
        {
            isDead = true; //����� ������� ������ � ����� ������ ������ ���� ���
            Die();
        }
    }
    public void SetLockMode(bool mode) //��� ��������� �������� �������, ��� true ��������� �� �����
    {
        canAttack = mode;
    }

    public void TakeDamage(float damage, int attackRoll)
    {
        if (attackRoll >= defence || attackRoll == 20) //���� ������ �� ��������� ������ ������� ������ - ������� ����
        {
            if (attackRoll == 20) // ����
            {
                healthBar -= damage * 2;
                if (_audioSource != null && takeDamageSFX != null) _audioSource.PlayOneShot(takeDamageSFX); //���� ��������� �����
                                                                                                            //���� ���������
                Debug.Log($"�������� {damage * 2} �����, ����!");
                Debug.Log("��������: " + health);
            }
            else
            {
                healthBar -= damage;
                if (_audioSource != null && takeDamageSFX != null) _audioSource.PlayOneShot(takeDamageSFX); //���� ��������� �����

                //���� ���������
                Debug.Log($"�������� {damage} �����.");
                Debug.Log("��������: " + health);
            }
            if (health <= 0) GlobalEventManager.SendPlayerHealthEnd(); //�������� ��������� � ������
        }
        else //������
        { }

    }

    public float healthBar
    {
        get => health;
        set
        {
            Debug.Log("��������� 1");
            health = value;
            HpChagedPercent?.Invoke(health / maxHealth);
            Debug.Log("��������� 2");
        }
    }

    public void TakeHeal(float heal)
    {
        //�� ������ ��� �������� �������� ���������� �����, �������� ������� �� ���������� 


        if (health + heal > maxHealth) //���� �������� ������������ ������, ��� ������������ ��������
        {
            healthBar = maxHealth;
        }
        else //������ ��������� ������� �������������
        {
            healthBar += heal;
        }
        //���� �������
        //��������� "������� ��������"

        //������� ��������� �������� �� ������ ������
        Debug.Log($"�������� {heal} ��������.");
        Debug.Log("��������: " + health);
    }

    public virtual void Attack()
    { 
        RaycastHit hit; //���� ����, �� ������� ��������        
        // ������ �������� ������, ���� ��� ����
        if (weapon.GetComponent<Animation>() != null) weapon.GetComponent<Animation>().Play();

        //���������, ������ �� �������� ����������/�������, ���� �� - ������ ������ ����� � ����
        if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out hit, attackRange)) 
        {
            //���� �����, ���������
            if (_audioSource != null && attackSFX != null) _audioSource.PlayOneShot(attackSFX);

            //������� ������ �� ����� � �� ����
            int attackRoll = Random.Range(1, 20 + 1);
            int damageRoll = Random.Range(1, baseDamageDie + 1);

            //�������� ������, �� �������� ����� ���
            GameObject hitObject = hit.transform.gameObject;
            IStatblock target = hitObject.GetComponent<IStatblock>();
            //������� ���� � ����������, ���� �� ����� ��������� ����������� ��������� ���������
            if (target != null)
            {
                // �������� ���� � ������������� ����������� 
                target.TakeDamage(damageRoll, attackRoll);
                // ��������, ���� � ���� ���� �� - ���� �������� �������� ��������, ���� ��������� ����� ������������ ���
                if (hitObject.GetComponent<AIMotor>() != null) hitObject.GetComponent<AIMotor>().isAggro = true;
            }
        }
        else // �� ������ 
        {
            if (emptyAttackSFX != null) _audioSource?.PlayOneShot(emptyAttackSFX); //���� �������
        }
        StartCoroutine(AttackCooldown(attackCooldown)); //����� �������� �����
    }
    public void Die() //��������� �� ������� � ����� �������� ��� ��������� �������� ������
    {
        //��� �������������� ��� ������
        GameObject pauser = GameObject.FindWithTag("Pauser");

        controls.SetActive(false);
        pauser.GetComponent<Pauser>().globalPause = true;
        GetComponent<PlayerMenu>().enabled = false;
        defence = 10000;
        componentAnimator.SetTrigger(name: "die_trigger");
        //����� ������ � �������� ��������� ����� ����� ���������� �������
        StartCoroutine(DeathCourutine());
    }
    private IEnumerator DeathCourutine(int deathTime = 5)
    {
        Debug.Log("�� ������.");
        yield return new WaitForSeconds(deathTime);
        SceneTransition.SwitchToScene("test_menu"); //������� � ��������� �����
    }
    private IEnumerator AttackCooldown(float attackCooldown)
    {
        canAttackCooldown = false;
        //Debug.Log($"������� ����� �� {attackCooldown}.");
        yield return new WaitForSeconds(attackCooldown);
        canAttackCooldown = true;
    }
    private IEnumerator SphereIndicator(Vector3 pos)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = pos;
        yield return new WaitForSeconds(1);
        Destroy(sphere);
    }
    static public void PlayerLock(bool locker)
    {
        GameObject player = GameObject.FindWithTag("Player");
        GameObject cam = GameObject.FindWithTag("MainCamera");
        FirstPersonMovement playerMovement = player.GetComponent<FirstPersonMovement>();
        FirstPersonLook playerLook = cam.GetComponent<FirstPersonLook>();
        PlayerStatblock playerAttack = player.GetComponent<PlayerStatblock>();
        Rigidbody playerRigitbody = player.GetComponent<Rigidbody>();
        Interactive playerInteractive = player.GetComponent<Interactive>();

        //���������� ���� ������������ �������������� ��� ������
        playerMovement.SetLockMode(locker); 
        playerLook.SetLockMode(locker); 
        playerAttack.SetLockMode(locker);
        playerRigitbody.isKinematic = !locker;
        playerInteractive.SetLockMode(locker);
    }

    
    
        
    
    
    
    


}
