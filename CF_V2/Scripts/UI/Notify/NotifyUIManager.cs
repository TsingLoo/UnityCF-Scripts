using System;
using System.Linq;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class NotifyUIManager : MonoBehaviour
    {
        public RectTransform NotifyPanel;
        public RectTransform KillFeedPanel;

        public GameObject NotifyPrefab;
        public GameObject KillFeedPrefab;

        void Awake()
        {
            EventManager.AddListener<ObjectiveUpdateEvent>(OnObjectiveUpdateEvent);

            // kill feed
            EventManager.AddListener<BotDeathEvent>(OnBotDeath);
        }

        private void Start()
        {
            PlayerWeaponsManager playerWeaponsManager = FindObjectOfType<PlayerWeaponsManager>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerWeaponsManager, NotifyUIManager>(playerWeaponsManager,
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
                CreateNotify(evt.NotificationText);
        }

        private void CreateKillFeed(string killedBy, string weaponAssetName, 
            string pawnName)
        {
            GameObject newNotify = Instantiate(KillFeedPrefab, KillFeedPanel);
            // bottom
            newNotify.transform.SetAsLastSibling();

            KillFeed toast = newNotify.GetComponent<KillFeed>();
            if (toast)
            {
                toast.Initialize(pawnName, killedBy, weaponAssetName);
            }
        }

        public void CreateNotify(string text)
        {
            GameObject newNotify = Instantiate(NotifyPrefab, NotifyPanel);
            // up
            newNotify.transform.SetAsFirstSibling();

            Notify toast = newNotify.GetComponent<Notify>();
            if (toast)
            {
                toast.Initialize(text);
            }
        }

        #region Notify events
        void OnAddedWeapon(WeaponController weaponController)
        {
            CreateNotify("Add weapon: " + weaponController.WeaponName);
        }

        void OnUnlockJetpack(bool unlock)
        {
            CreateNotify("Jetpack unlocked");
        }

        private void OnBotDeath(BotDeathEvent evt)
        {
            var health = evt.Bot.GetComponent<Health>();
            var damageSource = health.damageSources.LastOrDefault();
            var pawn = damageSource.GetComponentInParent<PawnController>();
            var weapon = damageSource.GetComponent<WeaponController>();
            var killedBy = "kill in action: ";
            if (pawn)
            {
                killedBy = pawn.PawnName;
            }
            // todo ref
            if (weapon)
            {
                CreateKillFeed(killedBy, weapon.WeaponAssetName, evt.PawnName);
            }
        }

        #endregion

        void OnDestroy()
        {
            EventManager.RemoveListener<ObjectiveUpdateEvent>(OnObjectiveUpdateEvent);
        }
    }
}