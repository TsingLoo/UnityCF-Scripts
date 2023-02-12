using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Timeline;

public class WeaponScope : MonoBehaviour
{
    public static WeaponScope Instance { get; private set; }

    public GameObject _aimScope;
    [Header("Recoil")]
    // show up
    public float scaleFactor = 1.25f;
    public float scaleTime = 0.02f;

    public float scaleBackTime = 0.075f;



    void OnEnable()
    {
        Instance = this;

        // set by weapon
        //if(_aimScope == null)
        //{
        //    Debug.LogError("_sniperOverlay is null");
        //}
    }

    public void ShowScope(GameObject aimScope, 
        float aimingFOV,
        float aimSenseFactor)
    {
        #region layer
        ResetCamera1P();

        PlayerController.Instance.Camera1P_Main.cullingMask
            = GameSystem.Instance._camera1PAimLayer;

        //PlayerController.Instance.Camera1P_Main.cullingMask
        //    = GameSystem.Instance._camera1PAimLayer;
        PlayerController.Instance.Camera3P.cullingMask
            = GameSystem.Instance._camera1PAimLayer;

        //refer
        // todo not stable
        //PlayerController.Instance.Camera1P_Main.cullingMask
        //    = LayerHelper.RemoveLayer(PlayerController.Instance.Camera1P_Main.cullingMask
        //    , AllLayers.Weapon1P.GetValue());

        #endregion

        // fov
        if (aimingFOV > 0)
        {
            PlayerController.Instance.Camera1P_Main.fieldOfView
                = aimingFOV;

            PlayerController.Instance._mouseSensitivityUse
                = PlayerController.Instance.MouseSensitivity
                * aimSenseFactor;
        }


        // scope / crosshair
        _aimScope = Instantiate(aimScope, this.transform, false);
        _aimScope.Show();
        CrossHair.Instance.HideCrosshair();
    }

    //todo , unity bug
    private void ResetCamera1P()
    {
        PlayerController.Instance.Camera1P_Main.enabled = false;
        PlayerController.Instance.Camera1P_Main.enabled = true;
    }

    public void HideScope()
    {
        // layer
        ResetCamera1P();

        PlayerController.Instance.Camera1P_Main.cullingMask
           = GameSystem.Instance._camera1PLayer;
        PlayerController.Instance.Camera3P.cullingMask
            = GameSystem.Instance._camera3PLayer;


        // fov and sense
        PlayerController.Instance.Camera1P_Main.fieldOfView
            = PlayerController.Instance.Camera1P_Main_FOV;
        
        PlayerController.Instance._mouseSensitivityUse
            = PlayerController.Instance.MouseSensitivity;


        _aimScope.Hide();
        CrossHair.Instance.ShowCrosshair();

        // refer, not stable
        //PlayerController.Instance.Camera1P_Main.cullingMask
        //    = LayerHelper.AddLayer(PlayerController.Instance.Camera1P_Main.cullingMask
        //    , AllLayers.Weapon1P.GetValue());
    }

    internal void PlayRecoil()
    {
        var animId = "WeaponScope_Recoil";
        DOTween.Kill(animId);

        //markTransform.anchoredPosition = Vector2.zero;
        //markTransform.localScale = Vector3.zero;
        //markImage.color = Color.white;

        var scopeRecoil = _aimScope.GetComponent<WeaponScopeRecoil>();
        if (scopeRecoil != null )
        {
            scaleFactor = scopeRecoil.scaleFactor;
            scaleTime = scopeRecoil.scaleTime;

            scaleBackTime= scopeRecoil.scaleBackTime;
        }

        // scale up
        var markTransform = _aimScope.transform;
        markTransform.DOScale(Vector3.one * scaleFactor, scaleTime).SetEase(Ease.Linear).SetId(animId)
            .OnComplete(() =>
            {
                // scale back
                markTransform.DOScale(Vector3.one, scaleBackTime).SetEase(Ease.Linear).SetId(animId)
                .OnComplete(() =>
                {
                });
            });
    }
}
