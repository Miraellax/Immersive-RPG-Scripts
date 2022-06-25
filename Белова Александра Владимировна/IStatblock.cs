using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStatblock 
{
    public void TakeDamage(float damage, int attackRoll);
    public void TakeHeal(float heal);
    public void Attack();
    public void Die();
}
