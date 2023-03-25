using UnityEngine;

public class Projectile : MonoBehaviour
{    
    static Collider[] _sphereCastPool = new Collider[32];
    
    public bool updateDirection = false;
    public bool DestroyedOnHit = false;
    public float TimeToDestroy = 3.0f;
    float m_TimeSinceLaunch;

    public float ReachRadius = 5.0f;
    public int damage = 135;
    public EDamageType damageType = EDamageType.Granade;
    public AudioClip DestroyedSound;
    
    public GameObject EffectPrefab;

    WeaponController m_Owner;
    Rigidbody m_Rigidbody;
    Collider m_Collider;
    
    void Awake()
    {
        PoolSystem.Instance.InitPool(EffectPrefab, 5);
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Collider = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        if (updateDirection)
        {
            this.transform.forward =
                m_Rigidbody.velocity.normalized;
        }
    }

    public void Launch(WeaponController launcher,
        Vector3 direction, float force)
    {
        m_Owner = launcher;

        transform.position = launcher.GetCorrectedMuzzlePlace();
        transform.forward = launcher._muzzleShotPosition.forward;
        
        gameObject.SetActive(true);
        m_TimeSinceLaunch = 0.0f;
        m_Rigidbody.AddForce(direction * force);
    }
    
    void OnCollisionEnter(Collision other)
    {
        if (!GameSystem.Instance._grenadeLayer
            .Contains(other.gameObject.layer))
        {
            Physics.IgnoreCollision(m_Collider, other.collider);
        }
        else if (DestroyedOnHit)
        {
            Destroy();
        }
    }

    void Destroy()
    {
        Vector3 position = transform.position;

        // effect
        var effect = Instantiate(EffectPrefab,
            transform.position,
            Quaternion.identity);
        effect.SetActive(true);

        // effect using pool 
        //var effect = PoolSystem.Instance
        //    .GetInstance(PrefabOnDestruction);
        //effect.transform.position = position;
        //effect.SetActive(true);

        // hit
        int count = Physics.OverlapSphereNonAlloc
            (position, ReachRadius, _sphereCastPool, 
            GameSystem.Instance._grenadeLayer);

        for (int i = 0; i < count; ++i)
        {
            // not using IDamageable
            Health body = _sphereCastPool[i]
                .GetComponentInParent<Health>();
            
            if(body != null)
            {
                body.TakeDamage(damage, damageType);
            }
        }
        
        // retrive projectile
        gameObject.SetActive(false);
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;
        m_Owner.ReturnProjecticle(this);

        // audio
        var source = WorldAudioPool.GetWorldSFXSource();

        source.transform.position = position;
        source.pitch = Random.Range(0.8f, 1.1f);
        source.PlayOneShot(DestroyedSound);
    }

    void Update()
    {
        m_TimeSinceLaunch += Time.deltaTime;

        if (m_TimeSinceLaunch >= TimeToDestroy)
        {
            Destroy();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, ReachRadius);
    }
}
