using UnityEngine;
using UnityEngine.Audio;

namespace Unity.FPS.Game
{
    public class AudioUtility
    {
        static AudioManager s_AudioManager;

        public enum AudioGroups
        {
            Ambient,
            Map,

            Hud,
            
            Player,
            Bot,

            Weapon,
            Impact,
        }

        public static void CreateSFX(AudioClip clip, 
            Vector3 position,
            AudioGroups audioGroup, 
            float spatialBlend,
            float rolloffDistanceMin = 1f,
            float volume = 1f)
        {
            GameObject newSFX = new GameObject();
            newSFX.transform.position = position;
            
            AudioSource source = newSFX.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = spatialBlend;
            source.minDistance = rolloffDistanceMin;
            source.volume = volume;
            source.Play();

            source.outputAudioMixerGroup = GetAudioGroup(audioGroup);

            TimedSelfDestroy timedSelfDestroy = newSFX.AddComponent<TimedSelfDestroy>();
            timedSelfDestroy.LifeTime = clip.length;
        }

        public static AudioMixerGroup GetAudioGroup(AudioGroups group)
        {
            if (s_AudioManager == null)
                s_AudioManager = GameObject.FindObjectOfType<AudioManager>();

            var groups = s_AudioManager.FindMatchingGroups(group.ToString());

            if (groups.Length > 0)
                return groups[0];

            Debug.LogWarning("Didn't find audio group for " + group.ToString());
            return null;
        }

        public static void SetMasterVolume(float value)
        {
            if (s_AudioManager == null)
                s_AudioManager = GameObject.FindObjectOfType<AudioManager>();

            if (value <= 0)
                value = 0.001f;
            float valueInDb = Mathf.Log10(value) * 20;

            s_AudioManager.SetFloat("MasterVolume", valueInDb);
        }

        public static float GetMasterVolume()
        {
            if (s_AudioManager == null)
                s_AudioManager = GameObject.FindObjectOfType<AudioManager>();

            s_AudioManager.GetFloat("MasterVolume", out var valueInDb);
            return Mathf.Pow(10f, valueInDb / 20.0f);
        }
    }
}
