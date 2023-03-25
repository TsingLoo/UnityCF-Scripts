using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class MovingPickup : Pickup
    {
        [Tooltip("Frequency at which the item will move up and down")]
        public float VerticalBobFrequency = 1f;

        [Tooltip("Distance the item will move up and down")]
        public float BobbingAmount = 1f;

        [Tooltip("Rotation angle per second")] 
        public float RotatingSpeed = 360f;


        protected override void Start()
        {
            base.Start();

            // ensure the physics setup is a kinematic rigidbody trigger
            pickupRigidbody.isKinematic = true;
            _pickupCollider.isTrigger = true;

            // Remember start position for animation
            m_StartPosition = transform.position;
        }

        void Update()
        {
            // Handle bobbing
            float bobbingAnimationPhase = ((Mathf.Sin(Time.time * VerticalBobFrequency) * 0.5f) + 0.5f) * BobbingAmount;
            transform.position = m_StartPosition + Vector3.up * bobbingAnimationPhase;

            // Handle rotating
            transform.Rotate(Vector3.up, RotatingSpeed * Time.deltaTime, Space.Self);
        }

    }
}