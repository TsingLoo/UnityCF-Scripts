using System.Linq;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// pawn health
/// </summary>
public class Health : MonoBehaviour, IDamageable
{
    public float health = 100;
    public int pointValue;

    public ParticleSystem DestroyedEffect;

    [Header("Audio")]
    public RandomPlayer randomPlayer;
    public AudioSource audioSource;

    public bool IsAlive => _currentHealth > 0;
    public bool Destroyed => _destroyed;

    bool _destroyed = false;
    float _currentHealth;

    void Awake()
    {
    }

    void Start()
    {
        if (DestroyedEffect)
        {
            PoolSystem.Instance.InitPool(DestroyedEffect, 16);
        }
        
        _currentHealth = health;

        if(audioSource != null)
            audioSource.time = Random.Range(0.0f, audioSource.clip.length);
    }

    public void TakeDamage(float damage, 
        EDamageType damageType = EDamageType.Rifle)
    {
        _currentHealth -= damage;
        
        if(randomPlayer != null)
            randomPlayer.PlayRandom();

            // destroy
        if (_currentHealth <= 0)
        {
            Vector3 position = transform.position;

            //the audiosource of the target will get destroyed, so we need to grab a world one and play the clip through it
            if (randomPlayer != null)
            {
                var source = WorldAudioPool.GetWorldSFXSource();
                source.transform.position = position;
                source.pitch = randomPlayer.source.pitch;
                source.PlayOneShot(randomPlayer.GetRandomClip());
            }

            if (DestroyedEffect != null)
            {
                var effect = PoolSystem.Instance.GetInstance<ParticleSystem>(DestroyedEffect);
                effect.time = 0.0f;
                effect.Play();
                effect.transform.position = position;
            }


            // animation
            GetComponent<Animator>().Enable();
            GetComponent<Animator>().Play("Man_Death_Grenade"
                , 2);

            // on destroy called in anim

            int score = (int)health;
            GameSystem.Instance.EnemyKilled(score, damageType);
        }
    }


    internal void OnDestroy()
    {
        gameObject.SetActive(false);
        _destroyed = true;
    }
}
