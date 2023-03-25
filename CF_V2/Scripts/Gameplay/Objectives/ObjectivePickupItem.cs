using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectivePickupItem : Objective
    {
        public GameObject ItemToPickup;

        protected override void Start()
        {
            base.Start();

            EventManager.AddListener<PickupEvent>(OnPickupEvent);
        }

        void OnPickupEvent(PickupEvent evt)
        {
            if (IsCompleted || ItemToPickup != evt.Pickup)
                return;

            CompleteObjective(descriptionText: string.Empty,
                counterText: string.Empty,
                notificationText: "Objective complete : " + Title);

            if (gameObject)
            {
                Destroy(gameObject);
            }
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<PickupEvent>(OnPickupEvent);
        }
    }
}