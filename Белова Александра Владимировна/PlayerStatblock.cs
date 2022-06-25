using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStatblock : MonoBehaviour, IStatblock
{
    [Header("Main Settings")]
    public float health; //по нему на старте мы запоминаем maxHealth
    public float defence; //значение защиты от атак
    public int baseDamageDie = 4; //куб атаки по умолчанию, без оружия, обновлять при оружии в экипировке
    public float attackRange; //дистанция возможной атаки, для ближнего боя
    public float attackCooldown = 3; //время между атаками
    public int ActiveObjectID; //айди активного предмета в руке, в хотбаре инвентаря
    public GameObject weapon; //выбранное оружие, на модели или в инвентаре
    public AudioClip takeDamageSFX; //звук получения урона
    public AudioClip attackSFX; //звук атаки оружием, потом следует брать из скрипта оружия
    public AudioClip emptyAttackSFX; //звук промаха оружием
    public AudioSource _audioSource; //источник звука атак
    public Camera _cam; //для рейкаста и атаки
    public Animator componentAnimator;
    public GameObject controls;

    [Header("Stats")]
    public float strength = 10; //сила
    public float dexterity = 10; //ловкость
    public float constitution = 10; // стойкость
    public float intelegence = 10; //интеллект
    public float wisdom = 10; //мудрость
    public float charisma = 10; //харизма

    private float maxHealth; //максимальное значение здоровья, модификаторов не будет
    public bool isDead = false;
    private bool canAttack = true; //для лока атаки
    private bool canAttackCooldown = true; //для кулдауна атаки

    [SerializeField]
    private UnityEvent<float> HpChagedPercent; 


    // Start is called before the first frame update
    void Start()
    {
        ////подписываем функцию смерти на событие "кончается здоровье"
        //GlobalEventManager.OnPlayerHealthEnd += Die;

        maxHealth = health;
        //модификаторы характеристик
        float strMod = (strength - 10) % 2;
        float dexMod = (dexterity - 10) % 2;
        float consMod = (constitution - 10) % 2;
        float intMod = (intelegence - 10) % 2;
        float wisMod = (wisdom - 10) % 2;
        float charMod = (charisma - 10) % 2;
       
        //health = health + 0;    // модификаторы экипировки и статов
        //speed = speed + 0;      // модификаторы экипировки и статов
        //damage = damage + 0;    // модификаторы экипировки и статов
    }

    // Update is called once per frame
    void Update()
    {
        ActiveObjectID = GetComponent<Inventory>().fastSlot[GetComponent<Inventory>().activeSlot]; //обновляем информацию о предмете, выбранном в хотбаре инвентаря

        if (canAttack & canAttackCooldown) //проверка, закончился ли кулдаун предыдущей атаки
        {
            if (Input.GetMouseButtonDown(0)) //атака
            {
                Attack();
            }
        }
        if (health <= 0 & !isDead) //проверка, жив ли
        {
            isDead = true; //чтобы вызвать смерть и экран смерти только один раз
            Die();
        }
    }
    public void SetLockMode(bool mode) //для установки значения диалога, при true атаковать не может
    {
        canAttack = mode;
    }

    public void TakeDamage(float damage, int attackRoll)
    {
        if (attackRoll >= defence || attackRoll == 20) //если бросок на попадание прошел уровень защиты - наносим урон
        {
            if (attackRoll == 20) // крит
            {
                healthBar -= damage * 2;
                if (_audioSource != null && takeDamageSFX != null) _audioSource.PlayOneShot(takeDamageSFX); //звук получения урона
                                                                                                            //звук попадания
                Debug.Log($"Нанесено {damage * 2} урона, крит!");
                Debug.Log("Здоровье: " + health);
            }
            else
            {
                healthBar -= damage;
                if (_audioSource != null && takeDamageSFX != null) _audioSource.PlayOneShot(takeDamageSFX); //звук получения урона

                //звук попадания
                Debug.Log($"Нанесено {damage} урона.");
                Debug.Log("Здоровье: " + health);
            }
            if (health <= 0) GlobalEventManager.SendPlayerHealthEnd(); //посылаем сообщение о смерти
        }
        else //промах
        { }

    }

    public float healthBar
    {
        get => health;
        set
        {
            Debug.Log("Изменение 1");
            health = value;
            HpChagedPercent?.Invoke(health / maxHealth);
            Debug.Log("Изменение 2");
        }
    }

    public void TakeHeal(float heal)
    {
        //не больше чем максимум здоровья восполнять может, написать условие не превышения 


        if (health + heal > maxHealth) //если пытаемся восстановить больше, чем максимальное здоровье
        {
            healthBar = maxHealth;
        }
        else //просто добавляем сколько планировалось
        {
            healthBar += heal;
        }
        //звук лечения
        //изменение "полоски здоровья"

        //событие повышения здоровья на экране игрока
        Debug.Log($"Вылечено {heal} здоровья.");
        Debug.Log("Здоровье: " + health);
    }

    public virtual void Attack()
    { 
        RaycastHit hit; //наша цель, по которой попадаем        
        // играем анимацию оружия, если она есть
        if (weapon.GetComponent<Animation>() != null) weapon.GetComponent<Animation>().Play();

        //проверяем, задеты ли хитбоксы противника/объекта, если да - делаем бросок атаки и урон
        if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out hit, attackRange)) 
        {
            //звук удара, попадание
            if (_audioSource != null && attackSFX != null) _audioSource.PlayOneShot(attackSFX);

            //сделать бросок на атаку и на урон
            int attackRoll = Random.Range(1, 20 + 1);
            int damageRoll = Random.Range(1, baseDamageDie + 1);

            //получить объект, по которому попал луч
            GameObject hitObject = hit.transform.gameObject;
            IStatblock target = hitObject.GetComponent<IStatblock>();
            //вызвать урон у противника, если он имеет компонент реализующий интерфейс статблока
            if (target != null)
            {
                // вызываем урон с передаваемыми аргументами 
                target.TakeDamage(damageRoll, attackRoll);
                // проверим, если у цели есть ИИ - надо изменить значение агрессии, если атаковали ранее дружелюбного нпс
                if (hitObject.GetComponent<AIMotor>() != null) hitObject.GetComponent<AIMotor>().isAggro = true;
            }
        }
        else // не попали 
        {
            if (emptyAttackSFX != null) _audioSource?.PlayOneShot(emptyAttackSFX); //звук промаха
        }
        StartCoroutine(AttackCooldown(attackCooldown)); //старт кулдауна атаки
    }
    public void Die() //подписана на событие и будет играться при окончании здоровья игрока
    {
        //лок взаимодействий для игрока
        GameObject pauser = GameObject.FindWithTag("Pauser");

        controls.SetActive(false);
        pauser.GetComponent<Pauser>().globalPause = true;
        GetComponent<PlayerMenu>().enabled = false;
        defence = 10000;
        componentAnimator.SetTrigger(name: "die_trigger");
        //экран смерти и загрузка следующей сцены после прошедшего времени
        StartCoroutine(DeathCourutine());
    }
    private IEnumerator DeathCourutine(int deathTime = 5)
    {
        Debug.Log("Вы умерли.");
        yield return new WaitForSeconds(deathTime);
        SceneTransition.SwitchToScene("test_menu"); //переход к следующей сцене
    }
    private IEnumerator AttackCooldown(float attackCooldown)
    {
        canAttackCooldown = false;
        //Debug.Log($"Кулдаун атаки на {attackCooldown}.");
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

        //блокировка всех возможностей взаимодействия для игрока
        playerMovement.SetLockMode(locker); 
        playerLook.SetLockMode(locker); 
        playerAttack.SetLockMode(locker);
        playerRigitbody.isKinematic = !locker;
        playerInteractive.SetLockMode(locker);
    }

    
    
        
    
    
    
    


}
