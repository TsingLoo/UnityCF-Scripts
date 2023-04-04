using System;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class CharacterModel : Model
    {

        [Serializable]
        public enum Sex // For animation
        {
            Man = 0,
            Woman = 1,
            Hulk = 2,
        }

        public enum FxType
        {
            None = 0,
            HumanSpawnFlash = 1,
            NanoGhostLowHpFlash = 2,
            Absorbed = 3
        }

        public string[] socketItemsName;

        public Sex sex;

        [SerializeField]
        private CharacterEffect characterEffect;

        private Renderer[] renderers;

        [Header("Bones")]
        public Transform spine;

        public Transform spine1;

        public Transform neck;


        public Transform[] hitboxes;


        public CharacterVoice voiceAsset;


        

    }
}