using System;
using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class Minimap : MonoBehaviour
    {
        public Collider minimapBoundingBox;

        public Image mapImage; // squre
        public Image mapBorderImage;

        public Image playerImage;
        //public GameObject viewDir;

        Transform _playerTransform;
        ETeam _playerTeam;

        [Header("Icons")]
        public Sprite teamIcon;
        public Sprite enemyIcon;

        [Header("Settings")]
        public bool fixMapRotation;

        Dictionary<Transform, MinimapMarker> _elements = new Dictionary<Transform, MinimapMarker>();

        // todo map scale

        private void Awake()
        {            
            this.InitMap();
        }

        private void Start()
        {
            var playerCon = FindObjectOfType<PlayerController>();
            _playerTransform = playerCon.transform;

            // todo ref
            // when RegisterElement, player team is known
            _playerTeam = playerCon.GetComponent<Actor>().Team;
            InitTeam();
        }

        private void InitTeam()
        {
            // team
            foreach (var element in _elements)
            {
                SetTeamIcon(element.Key, element.Value);
            }

        }

        private void SetTeamIcon(Transform worldElement, MinimapMarker marker)
        {
            var _actor = worldElement.GetComponent<Actor>();
            if (_actor)
            {
                if (_actor.Team == _playerTeam)
                {
                    marker.MainImage.sprite = teamIcon;
                    marker.MainImage.color = Color.white;
                }
                else
                {
                    marker.MainImage.sprite = enemyIcon;
                    marker.MainImage.color = Color.red;
                }
            }
        }

        void InitMap()
        {
            this.mapImage.SetNativeSize();
            this.mapImage.transform.localPosition = Vector3.zero;
        }

        void Update()
        {
            // box size
            float realWidth = minimapBoundingBox.bounds.size.x;
            float realHeight = minimapBoundingBox.bounds.size.z;

            // player relative position on box
            float relativeX = _playerTransform.position.x - minimapBoundingBox.bounds.min.x;
            float relativeY = _playerTransform.position.z - minimapBoundingBox.bounds.min.z;

            // change map pivot to player position
            float pivotX = relativeX / realWidth;
            float pivotY = relativeY / realHeight;

            this.mapImage.rectTransform.pivot = new Vector2(pivotX, pivotY);
            //this.mapImage.rectTransform.localPosition = Vector2.zero;

            if (fixMapRotation) // player rotate
            {
                this.playerImage.transform.eulerAngles = new Vector3(0, 0, -_playerTransform.eulerAngles.y);
                //this.viewDir.transform.eulerAngles = new Vector3(0, 0, -Camera.main.transform.eulerAngles.y);
            }
            else // map rotate
            {
                // oppsite to player
                this.mapImage.transform.eulerAngles = new Vector3(0, 0, _playerTransform.eulerAngles.y);
                this.mapBorderImage.transform.eulerAngles = this.mapImage.transform.eulerAngles;
            }

            // test
            // todo min max zoom
            if (Input.GetKeyDown(KeyCode.J)) { OnZoomIn(); }
            if (Input.GetKeyDown(KeyCode.K)) { OnZoomOut(); }

            // actors
            foreach (var element in _elements)
            {
                UpdateElement(element);
            }
        }

        private void UpdateElement(KeyValuePair<Transform, MinimapMarker> element)
        {
            // world transform
            var worldElement = element.Key;
            // marker on minimap
            var marker = element.Value;

            // box size
            float realWidth = minimapBoundingBox.bounds.size.x;
            float realHeight = minimapBoundingBox.bounds.size.z;
            float realSize = Mathf.Max(realWidth, realHeight);

            // world relative position on box (center)
            float relativeX = worldElement.position.x - minimapBoundingBox.bounds.center.x;
            float relativeY = worldElement.position.z - minimapBoundingBox.bounds.center.z;
            // pivot ratio
            var pivot = new Vector2(relativeX / realSize, relativeY / realSize);

            // position on image
            var mapImageSizeVector = mapImage.sprite.rect.size; // map image size (pixel?)
            float mapImageSize = Mathf.Max(mapImageSizeVector.x, mapImageSizeVector.y);
            var positionOnMinimap = pivot * mapImageSize;

            // set position
            var rectTransform = marker.GetComponent<RectTransform>();
            rectTransform.anchoredPosition= positionOnMinimap;
            // rotation
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, -worldElement.rotation.eulerAngles.y);
        }

        public void RegisterElement(Transform worldElement, MinimapMarker marker)
        {
            marker.transform.SetParent(mapImage.gameObject.transform);
            marker.GetComponent<RectTransform>().localScale = Vector3.one;
            SetTeamIcon(worldElement, marker);

            _elements.Add(worldElement, marker);
        }

        public void UnregisterElement(Transform element)
        {
            if (_elements.TryGetValue(element, out MinimapMarker marker))
            {
                marker.SelfDestroy();
            }

            _elements.Remove(element);
        }



        #region Zoom
        public void OnZoomIn()
        {
            mapImage.transform.localScale += mapImage.transform.localScale * 0.2f;
        }

        public void OnZoomOut()
        {
            mapImage.transform.localScale -= mapImage.transform.localScale * 0.2f;
        }
        #endregion
        // End
    }
}