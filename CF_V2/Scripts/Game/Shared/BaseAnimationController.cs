using System.Linq;
using UnityEngine;

namespace Unity.FPS.Game
{
    public abstract class BaseAnimationController: BaseBehaviour
    {
        protected WeaponData _weaponData;

        #region Event
        /// <summary>
        /// anim event
        /// only accept 1 para
        /// </summary>
        /// <param name="animName_soundName">a:b</param>
        public void PlaySound(string animName_soundName)//, float volume
        {
            var animName = animName_soundName.Split(":").FirstOrDefault();
            var soundName = animName_soundName.Split(":").LastOrDefault();
            var clipEventDtos = _weaponData.AnimDtos
                .FirstOrDefault(it => it.AnimName.EndsWith(animName))
                .AnimEventDtos;
            var clipEventDto = clipEventDtos
                .FirstOrDefault(it => it.FunctionName == nameof(PlaySound));

            PlaySoundClip(soundName, clipEventDto.FloatParameter);

            // Debug.Log(animName_soundName);
        }

        #endregion

        #region Help function
        /// <summary>
        /// 
        /// </summary>
        /// <param name="animAffix">Idle</param>
        public void PlaySoundClip(string animAffix, float volume = 0f)
        {
            var audioClip = _weaponData.AudioClips
                .FirstOrDefault(it => it.name.EndsWith(animAffix));

            if (audioClip != null)
            {
                PlayOneShot(audioClip);
            }
        }

        #endregion

        #region Join
        protected string Join(string a, string b)
        {
            return AnimNames.Combine(a, b);
        }

        protected string Join(string a, string b, string c)
        {
            return AnimNames.Combine(a, b, c);
        }



        #endregion

        public virtual void PlayOneShot(AudioClip audioClip) { }

    }
}