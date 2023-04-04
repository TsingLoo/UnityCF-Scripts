using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class KillFeed : Notify
    {
        public TMPro.TextMeshProUGUI KilledByText;
        public Image weaponIcon;

        public void Initialize(string text, string killedBy, string weaponAssetName)
        {
            base.Initialize(text);

            KilledByText.text = killedBy;

            var sprite = Resources.Load<Sprite>(ResPaths.KillFeed_Weapon +
                $"SHOT_WEAPON_{weaponAssetName}");
            Debug.Assert( sprite != null );
            weaponIcon.sprite = sprite;
        }


        //
    }
}