using UnityEngine;

public interface  IDammage
{
    float MaxHealth { get; }
    float CurrentHealth { get; } 
    void Die()
    {
        
    }

    void Damage(float damageamount )
    {
        
    }
    
    
}
