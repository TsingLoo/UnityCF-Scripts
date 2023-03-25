using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class JetpackCounter : MonoBehaviour
    {
        [Tooltip("Image component representing jetpack fuel")]
        public Image JetpackFillImage;

        [Tooltip("Canvas group that contains the whole UI for the jetack")]
        public CanvasGroup MainCanvasGroup;

        [Tooltip("Component to animate the color when empty or full")]
        public FillBarColorChange FillBarColorChange;

        Jetpack m_Jetpack;

        void Awake()
        {

            FillBarColorChange.Initialize(1f, 0f);
        }

        private void Start()
        {
            m_Jetpack = FindObjectOfType<Jetpack>();
            Debug.Assert(m_Jetpack != null);
            //DebugUtility.HandleErrorIfNullFindObject<Jetpack, JetpackCounter>(m_Jetpack, this);

        }

        void Update()
        {
            MainCanvasGroup.gameObject.SetActive(m_Jetpack.IsJetpackUnlocked);

            if (m_Jetpack.IsJetpackUnlocked)
            {
                JetpackFillImage.fillAmount = m_Jetpack.CurrentFillRatio;
                FillBarColorChange.UpdateVisual(m_Jetpack.CurrentFillRatio);
            }
        }
    }
}