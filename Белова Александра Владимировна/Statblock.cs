using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statblock : MonoBehaviour, IStatblock
{
    [Header("Main Settings")]
    bool isAggro = false;
    public Transform targetToAttack;
    public float health; //по нему на старте мы запоминаем maxHealth
    public float defence;
    public int baseDamageDie = 4; //куб атаки по умолчанию, без оружия, обновлять при оружии в экипировке
    public float attackRange; //дистанция возможной атаки, для ближнего боя
    public float attackCooldown = 5;
    public Animation allAnimations; //анимации 
    public AudioClip takeDamageSFX; //звук получения урона
    public AudioClip attackSFX; //звуки атак 
    public AudioSource _audioSource; //источник звука атак
    public Camera _cam; //для рейкаста и атаки

    //добавить инвентарь, мб списком айди вещей, при смерти выбрасываем?
    //вещи и экипировка дают бонусы к защите/урону, надо учитывать

    [Header("Stats")]
    public float strength = 10; //сила
    public float dexterity = 10; //ловкость
    public float constitution = 10; // стойкость
    public float intelegence = 10; //интеллект
    public float wisdom = 10; //мудрость
    public float charisma = 10; //харизма

    private float maxHealth; //максимальное значение здоровья, модификаторов не будет
    private bool isDead = false;
    private bool canAttack = true; //для кулдауна атаки
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
        //подписываем функцию смерти на событие "кончается здоровье"
        GlobalEventManager.OnEnemyHealthEnd += Die;

        maxHealth = health;
        //модификаторы характеристик
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
        //получаем компонент ИИ для изменения значения агрессивности
        AIMotor AI = transform.GetComponent<AIMotor>();
        if ( AI != null) isAggro = AI.isAggro;
        //проверяем, жив ли ИИ, может ли атаковать и агрессивен ли - по кулдауну атакуем цель
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
        if (health > 0) // наносим урон 
        {
            //если бросок на попадание прошел уровень защиты - наносим урон
            if (attackRoll >= defence || attackRoll == 20)
            {
                if (attackRoll == 20) // критическое попадание
                {
                    health -= damage * 2;
                    //для НПС, если игрок атакует его - переключается в режим агрессии
                    AIMotor motor = transform.GetComponent<AIMotor>();
                    //проверяем, есть ли ИИ и тег НПС
                    if (motor != null && motor.isNPC == true)
                    {
                        motor.IsAggro = true;
                    }
                    //играем анимацию получения урона, если она имеется
                    if (allAnimations != null && allAnimations.GetClip("TakeDamage") != null)
                    {
                        allAnimations.Play("TakeDamage");
                    }
                    //звук получения урона
                    if (_audioSource != null && takeDamageSFX != null)
                    {
                        _audioSource.PlayOneShot(takeDamageSFX);
                    }
                }
                else // обычное попадание
                {
                    health -= damage;
                    //для НПС, если игрок атакует его - переключается в режим агрессии
                    AIMotor motor = transform.GetComponent<AIMotor>(); 
                    //проверяем, есть ли ИИ и тег НПС
                    if (motor != null && motor.isNPC == true) 
                    {
                        motor.IsAggro = true;
                    }
                    //играем анимацию получения урона, если она имеется
                    if (allAnimations != null && allAnimations.GetClip("TakeDamage") != null)
                    {
                        allAnimations.Play("TakeDamage");
                    }
                    //звук получения урона
                    if (_audioSource != null && takeDamageSFX != null)
                    {
                        _audioSource.PlayOneShot(takeDamageSFX);
                    }
                }
                if (health <= 0)
                {
                    //посылаем сообщение о смерти
                    GlobalEventManager.SendEnemyHealthEnd();
                    canAttack = false;
                    Die();
                }
            }
            else { }//промах
        }
    }
    public void TakeHeal(float heal)
    {
        //Может быть восполнено не больше чем максимум здоровья
        if (health + heal > maxHealth) //если пытаемся восстановить больше, чем максимальное здоровье
        {
            health = maxHealth;
        }
        else //просто добавляем сколько планировалось
        {
            health += heal;
        }
        //звук лечения
        //изменение "полоски здоровья"

        //событие повышения здоровья на экране игрока
        Debug.Log($"Вылечено {heal} здоровья.");
        Debug.Log("Здоровье: " + health);
    }

    public void Attack(Transform target) //атака выбранной цели
    {
        if (_audioSource != null && attackSFX != null)  _audioSource.PlayOneShot(attackSFX); //звук атаки
        if (allAnimations != null && allAnimations.GetClip("TakeDamage") != null) allAnimations.Play("Attack"); //играем анимацию атаки, если она имеется

        //сделать бросок на атаку + бонус мастерства + модификатор от оружия на попадание
        int attackRoll = Random.Range(0, 20 + 1); // + БМ + взять бонус из оружия
        int damageRoll = Random.Range(1, baseDamageDie + 1); // + взять бонус из оружия

        //вызвать урон у противника
        if (target != null && target.GetComponent<PlayerStatblock>() != null && !target.GetComponent<PlayerStatblock>().isDead)
        {
            target.GetComponent<PlayerStatblock>().TakeDamage(damageRoll, attackRoll);
        }
        StartCoroutine(AttackCooldown(attackCooldown)); //старт кулдауна атаки
    }
    public virtual void Attack() //метод рейкаста
    {
        if (_audioSource != null && attackSFX != null) _audioSource.PlayOneShot(attackSFX); //звук атаки
        if (allAnimations != null && allAnimations.GetClip("Attack") != null) allAnimations.Play("Attack"); //играем анимацию атаки, если она имеется
        RaycastHit hit; //наша цель, по которой попадаем

        if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out hit, attackRange))//проверяем, задеты ли хитбоксы противника/объекта, если да - делаем бросок атаки и урон
        {
            //сделать бросок на атаку + бонус мастерства + модификатор от оружия на попадание
            int attackRoll = Random.Range(0, 20 + 1); // + БМ + взять бонус из оружия
            int damageRoll = Random.Range(1, baseDamageDie + 1); // + взять бонус из оружия

            //вызвать урон у противника
            GameObject hitObject = hit.transform.gameObject;
            PlayerStatblock target = hitObject.GetComponent<PlayerStatblock>();
            if (target != null && !target.isDead) 
            {
                target.TakeDamage(damageRoll, attackRoll);  //вызываем получение урона и звук получения урона
            }
        }
        else
        {
            //нет целей
        }
        StartCoroutine(AttackCooldown(attackCooldown)); //старт кулдауна атаки
    }
    public void Die()
    {
        // проверяем, вызван ли метод впервые для избежания ошибок
        if (health <= 0 & isDead == false)
        {
            isDead = true;
            Debug.Log($"{transform.name} died");
            AIMotor motor = transform.GetComponent<AIMotor>();
            //проверяем, есть ли ИИ у умершего
            if (motor != null) 
            {
                motor.IsDead = true;
            }
            //играем анимацию смерти, если есть
            if (allAnimations != null && allAnimations.GetClip("Die") != null) 
            {
                allAnimations.Play("Die");
            }
            //корутина, через время тело и объект исчезнут
            StartCoroutine(DeathCourutine(10));
        }
    }
    private IEnumerator DeathCourutine(int deathTime = 10) //начинается при смерти
    {
        if (health <= 0)
        {
            //Debug.Log($"{transform.name} body will remain for {deathTime} seconds");
            //инвентарь моба становится доступным для обыска, булевое значение доступности инвенторя = true
            if (transform.FindChild("sword_epic") != null 
                && transform.FindChild("sword_epic").GetComponent<LootableItem>() != null) transform.FindChild("sword_epic").GetComponent<LootableItem>().lootable = true;
            //тело лежит
            yield return new WaitForSeconds(deathTime);
            //удаление объекта
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
