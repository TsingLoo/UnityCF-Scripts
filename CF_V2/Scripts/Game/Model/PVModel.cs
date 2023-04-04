using System;
using UnityEngine;

// PV Model
namespace Unity.FPS.Game
{
    public class PVModel : Model
    {
        [Serializable]
        public struct Data
        {
            public Transform Model;

            public Vector3 Pos_OnHand;

            public Vector3 Euler_OnHand;

            public Vector3 Pos_GiveUp;

            public Vector3 Euler_GiveUp;

            public ParticleSystem GunFire;
        }

        public Rigidbody rigidBody;

        [SerializeField]
        private Data left;
         
        [SerializeField]
        public Data right;

        [SerializeField]
        public float giveUpScale;

        [SerializeField]
        private Transform pickUpArea;


        public bool isMapGun { get; private set; }


        public void SetVisible(bool visible)
        {
        }


        public void SetOnMap(Transform setPos, Vector3 localPos, Vector3 localEuler)
        {
        }

        private void OnTriggerStay(Collider other)
        {
        }

    }
}