using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This handle impacts on object from the raycast of the weapon. 
/// It will create a pool of the prefabs for performance
/// optimisation.
/// </summary>
public class ImpactManager : MonoBehaviour
{
    [System.Serializable]
    public class ImpactSetting
    {
        public ParticleSystem ParticlePrefab;
        public AudioClip ImpactSound;
        public Material TargetMaterial;
    }

    static public ImpactManager Instance { get; protected set; }

    public ParticleSystem muzzleFlash;

    public ImpactSetting DefaultSettings;
    public ImpactSetting[] ImpactSettings;

    Dictionary<Material, ImpactSetting> m_SettingLookup = 
        new Dictionary<Material,ImpactSetting>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        PoolSystem.Instance.InitPool
            (DefaultSettings.ParticlePrefab, PoolSystem. poolSize);
        
        foreach (var impactSettings in ImpactSettings)
        {
            PoolSystem.Instance.InitPool
                (impactSettings.ParticlePrefab, PoolSystem.poolSize);
            m_SettingLookup.Add(impactSettings.TargetMaterial, 
                impactSettings);
        }
    }

    public void PlayImpact(Vector3 position, 
        Vector3 normal, 
        Material material = null)
    {
        ImpactSetting setting = null;
        if (material == null 
            || !m_SettingLookup.TryGetValue(material, out setting))
        {
            setting = DefaultSettings;
        }
        
        var particle =  PoolSystem.Instance
            .GetInstance(setting.ParticlePrefab);
        particle.gameObject.transform.position = position;
        particle.gameObject.transform.forward = normal;

        particle.gameObject.SetActive(true);
        particle.Play();

        if(setting.ImpactSound != null)
        {
            var source = WorldAudioPool.GetWorldSFXSource();

            source.transform.position = position;
            source.pitch = Random.Range(0.8f, 1.1f);
            source.PlayOneShot(setting.ImpactSound);
        }
    }
}
