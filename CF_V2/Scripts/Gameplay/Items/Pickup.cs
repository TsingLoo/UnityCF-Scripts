using System;
using System.Collections;
using System.Net.Sockets;
using Unity.FPS.Game;
using Unity.FPS.Inventory;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class Pickup : MonoBehaviour
    {
        public Item PickupItem;

        protected AudioClip PickupSound;
        protected float PickupSoundVolume;

        [Header("FX")]
        public GameObject PickupVfxPrefab;

        public Rigidbody pickupRigidbody { get; private set; }

        protected Collider _pickupCollider;
        protected Vector3 m_StartPosition;

        [Header("Throw")]
        public float dropTime = 1.0f;
        public float dropForwardForce = 10;

        private void Awake()
        {
            Init();
        }

        protected virtual void Init()
        {
            pickupRigidbody = GetComponent<Rigidbody>();
            DebugUtility.HandleErrorIfNullGetComponent
                <Rigidbody, Pickup>(pickupRigidbody, this, gameObject);

            _pickupCollider = GetComponent<BoxCollider>();
            
            // todo ref?
            var pickupArea = transform.DeepFind("PickUpArea");
            if(pickupArea != null)
            {
                _pickupCollider = pickupArea.GetComponent<BoxCollider>();
            }

            DebugUtility.HandleErrorIfNullGetComponent
                <Collider, Pickup>(_pickupCollider, this, gameObject);
        }

        protected virtual void Start()
        {
        }

        void OnTriggerEnter(Collider other)
        {
            PawnController byPawn = other.GetComponent<PawnController>();

            if (byPawn != null)
            {
                OnPicked(byPawn);

                PickupEvent evt = Events.PickupEvent;
                evt.Pickup = gameObject;
                EventManager.Broadcast(evt);
            }
        }

        protected virtual void OnPicked(PawnController pawnController)
        {
            if (pawnController.PawnEquipment.TryEquipItem(PickupItem))
            {
                PlayPickupFX();
                Destroy(gameObject);
            }
        }

        public void PlayPickupFX()
        {
            if (PickupSound)
            {
                AudioUtility.CreateSFX(PickupSound, 
                    transform.position, AudioUtility.AudioGroups.Hud, 
                    0f, 1f, PickupSoundVolume);
            }

            if (PickupVfxPrefab)
            {
                var pickupVfxInstance = Instantiate(PickupVfxPrefab, transform.position, Quaternion.identity);
            }
        }

        internal void Throw()
        {
            //Set parent to null
            transform.SetParent(null);

            SetDroppingProperty();

            #region physics
            // todo inherit pawn velocity
            //rigidBody.velocity = playerVelocity;

            // force, forward
            pickupRigidbody.AddForce(transform.forward * dropForwardForce,
                ForceMode.Impulse);
            #endregion

            StartCoroutine(FinishDrop());
        }

        private IEnumerator FinishDrop()
        {
            yield return new WaitForSeconds(dropTime);

            SetOnGroundProperty();
        }

        private void SetDroppingProperty()
        {
            _pickupCollider.enabled = false;
        }

        private void SetOnGroundProperty()
        {
            _pickupCollider.enabled = true;
        }


        // End
    }
}
