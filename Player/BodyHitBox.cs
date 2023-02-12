using UnityEngine;

/// <summary>
/// body part hit box
/// </summary>
public class BodyHitBox : MonoBehaviour, IDamageable
{
    Health health;
    string bodyPartName = string.Empty;
    float multiplier = 1f;
        
    void Start()
    {
        health = GetComponentInParent<Health>();

        bodyPartName = this.name;
    }

    public void TakeDamage(float damage, EDamageType damageType) 
    {
        if(health.IsAlive)
        {
            switch (bodyPartName)
            {
                case nameof(EBodyHitBoxParts.Hit_Head):
                    {
                        multiplier = 3f;
                        damageType = EDamageType.HeadShot;
                    }
                    break;
                case nameof(EBodyHitBoxParts.Hit_Body):
                    multiplier = 1f;
                    break;
                case nameof(EBodyHitBoxParts.Hit_Leg):
                    multiplier = 0.7f;
                    break;
                default:
                    break;
            }

            health.TakeDamage(damage * multiplier, damageType);
        }
    }
}
