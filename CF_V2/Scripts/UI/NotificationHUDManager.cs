using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class NotificationHUDManager : MonoBehaviour
    {
        [Tooltip("UI panel containing the layoutGroup for displaying notifications")]
        public RectTransform NotificationPanel;

        [Tooltip("Prefab for the notifications")]
        public GameObject NotificationPrefab;

        void Awake()
        {

            EventManager.AddListener<ObjectiveUpdateEvent>(OnObjectiveUpdateEvent);
        }

        private void Start()
        {
            PlayerWeaponsManager playerWeaponsManager = FindObjectOfType<PlayerWeaponsManager>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerWeaponsManager, NotificationHUDManager>(playerWeaponsManager,
                this);
            playerWeaponsManager.OnAddedWeapon += OnAddedWeapon;

            // todo jetpack
            //Jetpack jetpack = FindObjectOfType<Jetpack>();
            //DebugUtility.HandleErrorIfNullFindObject<Jetpack, NotificationHUDManager>(jetpack, this);
            //jetpack.OnUnlockJetpack += OnUnlockJetpack;

        }

        void OnObjectiveUpdateEvent(ObjectiveUpdateEvent evt)
        {
            if (!string.IsNullOrEmpty(evt.NotificationText))
                CreateNotification(evt.NotificationText);
        }

        void OnAddedWeapon(WeaponController weaponController)
        {
             //CreateNotification("Picked up weapon : " + weaponController.WeaponName);
        }

        void OnUnlockJetpack(bool unlock)
        {
            CreateNotification("Jetpack unlocked");
        }

        public void CreateNotification(string text)
        {
            GameObject notificationInstance = Instantiate(NotificationPrefab, NotificationPanel);
            notificationInstance.transform.SetSiblingIndex(0);

            NotificationToast toast = notificationInstance.GetComponent<NotificationToast>();
            if (toast)
            {
                toast.Initialize(text);
            }
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<ObjectiveUpdateEvent>(OnObjectiveUpdateEvent);
        }
    }
}