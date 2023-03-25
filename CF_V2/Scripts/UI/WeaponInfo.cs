using System.Linq;
using TMPro;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class WeaponInfo : MonoBehaviour
    {
        [Tooltip("CanvasGroup to fade the ammo UI")]
        public CanvasGroup CanvasGroup;

        [Header("Weapon Info")]
        public Text WeaponName;

        [HideInInspector] public string WeaponAssetName;

        public RawImage WeaponIconBG;
        public RawImage WeaponIconEffect;
        public RawImage WeaponIconLine;

        public bool UpdateInfo;

        [Header("Ammo")]
        public GameObject AmmoUI;
        public TextMeshProUGUI AmmoContent;
        public TextMeshProUGUI AmmoCarry;

        [Header("Selection")]
        [Range(0, 1)]
        [Tooltip("Opacity when weapon not selected")]
        public float UnselectedOpacity = 0.5f;

        [Tooltip("Scale when weapon not selected")]
        public Vector3 UnselectedScale = Vector3.one * 1f;

        PlayerWeaponsManager m_PlayerWeaponsManager;
        WeaponController m_Weapon;


        void Awake()
        {
            EventManager.AddListener<AmmoPickupEvent>(OnAmmoPickup);
        }

        void OnAmmoPickup(AmmoPickupEvent evt)
        {
            //todo
            //if (evt.Weapon == m_Weapon)
            //{
            //    AmmoCarry.text = m_Weapon.GetAmmoCarry().ToString();
            //}
        }

        public void InitWeapon(WeaponController weapon)
        {
            m_Weapon = weapon;
            WeaponAssetName = weapon.WeaponAssetName;
            m_PlayerWeaponsManager = FindObjectOfType<PlayerWeaponsManager>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerWeaponsManager, WeaponInfo>(m_PlayerWeaponsManager, this);

            // name
            if (WeaponName)
            {
                WeaponName.text = m_Weapon.WeaponName;
            }

            #region Icons
            var asset = weapon.WeaponAssetName;
            var iconDir = $"Weapons/{asset}/Icons";
            var icons = Resources.LoadAll<Texture2D>(iconDir);

            if (icons.HasValue())
            {
                // BG
                var iconBG = icons
                    .Where(it => it.name.EndsWith("_BG"))
                    .FirstOrDefault();

                if (iconBG != null)
                {
                    WeaponIconBG.texture = iconBG;
                    WeaponIconBG.Show();
                }
                else
                {
                    WeaponIconBG.Hide();
                }

                // Effect

                // Line
                var iconLine = icons
                    .Where(it => it.name.EndsWith("_LINE")
                        || it.name.EndsWith("_Line")
                        || it.name.EndsWith("_line"))
                    .FirstOrDefault();

                if (iconLine != null)
                {
                    WeaponIconLine.texture = iconLine;
                    WeaponIconLine.Show();
                }
                else
                {
                    WeaponIconLine.Hide();
                }
            }

            // todo add effect, change alpha
            //var effectRes = Resources.Load<Texture2D>
            //    (baseDir + weapon.WeaponResourceName + "_EFFECT");
            //if (effectRes != null)
            //{
            //    WeaponIconEffect.texture = effectRes;
            //}

            #endregion
            #region Ammo

            if (weapon.WeaponData.WeaponType == EWeaponType.Melee)
            {
                AmmoUI.Hide();
            }
            else
            {
                AmmoUI.Show();
            }
            #endregion

            #region Crosshair

            //if (weapon.weaponType == EWeaponType.Sniper)
            //{
            //    CrossHair.Instance.HideCrosshair();
            //}
            //else
            //{
            //    CrossHair.Instance.ShowCrosshair();
            //}
            #endregion
        }

        void Update()
        {
            if (UpdateInfo && m_Weapon)
            {
                AmmoContent.text = m_Weapon.GetAmmoContent().ToString();
                AmmoCarry.text = m_Weapon.GetAmmoCarry().ToString();
            }

            // todo add weapon switch control
            if (m_Weapon)
            {
                bool isActiveWeapon = m_Weapon == m_PlayerWeaponsManager.GetCurrentWeapon();

                // alpha
                if (CanvasGroup)
                {
                    CanvasGroup.alpha = Mathf.Lerp(CanvasGroup.alpha,
                        0, //isActiveWeapon ? 1f : UnselectedOpacity,
                        Time.deltaTime * 10);
                }
                transform.localScale = Vector3.Lerp(transform.localScale,
                    isActiveWeapon ? Vector3.one : UnselectedScale,
                    Time.deltaTime * 10);
            }
        }

        void Destroy()
        {
            EventManager.RemoveListener<AmmoPickupEvent>(OnAmmoPickup);
        }
    }
}