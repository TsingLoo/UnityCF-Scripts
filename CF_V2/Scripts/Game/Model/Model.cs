using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class Model : MonoBehaviour
    {
        [Serializable]
        public struct Socket
        {
            public string name;

            public Transform node;

            public Vector3 offset;

            public Vector3 euler;
        }

        [Header("Weapon1P")]
        public List<GameObject> objectInPV;

        [Header("Player3P")]
        public List<GameObject> objectInCV;

        public List<Socket> sockets;

        private void OnOwnerObserveModeChange(Type mode)
        {
        }

        public void SetModelLayer(Type mdlType, int layer)
        {
        }

        public void BindSocket(Transform tsf, string socketName)
        {
        }

        public void BindSocket(Transform tsf, string socketName, Vector3 offset, Vector3 euler)
        {
        }

        public void BindSocketInWorldSpace(Transform tsf, string socketName, Vector3 offset, Vector3 euler)
        {
        }

    }
}