using UnityEngine;

namespace Unity.FPS.UI
{
    public class Notify : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI TextContent;
        public CanvasGroup CanvasGroup;

        public float VisibleDuration = 3f;
        public float FadeInDuration = 0.1f;
        public float FadeOutDuration = 0.2f;


        public bool Initialized { get; private set; }
        protected float m_InitTime;

        public float TotalRunTime => VisibleDuration + FadeInDuration + FadeOutDuration;

        public void Initialize(string text)
        {
            TextContent.text = text;
            m_InitTime = Time.time;

            // start the fade out
            Initialized = true;
        }

        protected void Update()
        {
            if (Initialized)
            {
                float timeSinceInit = Time.time - m_InitTime;
               
                if (timeSinceInit < FadeInDuration)
                {
                    // fade in
                    CanvasGroup.alpha = timeSinceInit / FadeInDuration;
                }
                else if (timeSinceInit < FadeInDuration + VisibleDuration)
                {
                    // stay visible
                    CanvasGroup.alpha = 1f;
                }
                else if (timeSinceInit < FadeInDuration + VisibleDuration + FadeOutDuration)
                {
                    // fade out
                    CanvasGroup.alpha = 1 - (timeSinceInit - FadeInDuration - VisibleDuration) / FadeOutDuration;
                }
                else
                {
                    CanvasGroup.alpha = 0f;

                    // fade out over, destroy the object
                    Destroy(gameObject);
                }
            }
        }

        //
    }
}