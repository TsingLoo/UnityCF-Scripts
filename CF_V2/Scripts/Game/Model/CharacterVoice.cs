using UnityEngine;

namespace Unity.FPS.Game
{
    [CreateAssetMenu]
    public class CharacterVoice : ScriptableObject
    {
        public AudioClip headShot;

        public AudioClip[] multilKill;

        public AudioClip grenade;

        public AudioClip knife;

        public AudioClip die;

        public AudioClip[] fireInTheHole;

        public AudioClip[] fireInTheHole_C;

        public AudioClip gameStart_TD;

        public AudioClip gameWin;

        public AudioClip gameDraw;

        public AudioClip gameLose;

        public AudioClip gameStart_DM;

        public AudioClip gameOver_DM;

        public CharacterVoice()
            : base()
        {
        }
    }
}