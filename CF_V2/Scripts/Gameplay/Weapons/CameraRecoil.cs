using System;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class CameraRecoil : MonoBehaviour
    {
        private Transform thisTransform;

        PlayerWeaponsManager _weaponsManager;
        PlayerController _playerController;

        private void Awake()
        {
            thisTransform = transform;
        }

        private void Start()
        {
            _weaponsManager = FindObjectOfType<PlayerWeaponsManager>();
            _playerController = FindObjectOfType<PlayerController>();
        }

        private void Update()
        {
            if (_weaponsManager.GetCurrentWeapon() 
                && _weaponsManager.GetCurrentWeapon().HasRecoil()) 
            {
                // set rotation, only use y
                var cameraRecoil = _weaponsManager.accumulatedCameraRecoil//_weaponsManager.spreadThisShot + 
                    / 4f;
                if (_weaponsManager.IsAiming)
                {
                    cameraRecoil /= 2f;
                }

                transform.localRotation = Quaternion.Euler(-cameraRecoil.y, cameraRecoil.x, 0f);

                #region refer
                // set rotation
                //Vector3 finalEulerAngles = default;
                //finalEulerAngles = new Vector3(-spread.y, spread.x, 0f);
                //thisTransform.localEulerAngles = finalEulerAngles;
                #endregion
            }

        }


        //
    }
}