using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class CharacterEffect : MonoBehaviour
    {
        [Serializable]
        public struct FxData
        {
            public Transform node;

            public Vector3 offset;

            public Vector3 euler;

            public GameObject fxPrefab;
        }

        [SerializeField]
        private FxData[] datas;


        public virtual void SetEffectState(bool active)
        {
        }

        public CharacterEffect()
            : base()
        {
        }
    }
}