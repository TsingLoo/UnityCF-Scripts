using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class RootMotionController : MonoBehaviour
    {
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnAnimatorMove()
        {
            // send to PlayerController
            SendMessageUpwards("OnUpdateRootMotion", _animator.deltaPosition);
        }
    }
}