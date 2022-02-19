using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using uNature.Core.FoliageClasses;
using uNature.Core.Utility;

using System.Collections.Generic;

namespace uNature.Demo
{
    public class UN_ProceduralDemo_UIController : MonoBehaviour
    {
        #region General-Variables
        private static UN_ProceduralDemo_UIController _instance;
        public static UN_ProceduralDemo_UIController instance
        {
            get
            {
                return _instance;
            }
        }

        private CanvasGroup _group;
        private CanvasGroup group
        {
            get
            {
                if (_group == null)
                {
                    _group = GetComponent<CanvasGroup>();
                }

                return _group;
            }
        }

        public new bool enabled
        {
            get
            {
                return group.alpha > 0;
            }
            set
            {
                if (enabled != value)
                {
                    group.alpha = value ? 1 : 0;
                    group.interactable = value;
                    group.blocksRaycasts = value;
                }
            }
        }

        [Header("General")]
        [SerializeField]
        private GameObject prototypePrefab;

        [SerializeField]
        private EventSystem eventSystem;
        #endregion

        #region Grass-Variables
        [Header("Grass")]
        [SerializeField]
        private Transform grassPrototypesParent;

        [SerializeField]
        private Slider densitySlider;

        [SerializeField]
        private Slider viewDistanceSlider;

        [SerializeField]
        private Toggle castShadowsToggle;

        [SerializeField]
        private Toggle windEnabledToggle;

        [SerializeField]
        private Toggle colorMapsEnabledToggle;

        private List<UN_DemoPrototypeUI> grassPrototypes = new List<UN_DemoPrototypeUI>();

        [HideInInspector]
        public List<UN_DemoPrototypeUI> chosenPrototypes = new List<UN_DemoPrototypeUI>();

        private List<FoliagePrototype> chosenPrototypes_Converted = new List<FoliagePrototype>();
        #endregion

        #region Paint-Variables
        [Header("Paint")]
        [SerializeField]
        private Transform brushPrototypesParent;

        [SerializeField]
        private Slider brushSizeSlider;

        [SerializeField]
        private Slider paintDensitySlider;

        [SerializeField]
        private Slider eraseDensitySlider;

        private List<UN_DemoPrototypeUI> paintPrototypes = new List<UN_DemoPrototypeUI>();

        [HideInInspector]
        public UN_DemoPrototypeUI chosenBrush = null;

        private Vector3 lastBrushPosition = Vector3.zero;
        #endregion

        #region Callbacks
        public void OnDensityChanged(float tempValue)
        {
            float value = densitySlider.value;

            FoliageCore_MainManager.instance.density = value;
        }

        public void OnViewDistanceChanged(float tempValue)
        {
            float value = viewDistanceSlider.value;
            int transformedValue = (int)(500 * value);

            FoliageCore_MainManager.instance.globalFadeDistance = transformedValue;
        }

        public void OnCastShadowsChanged(bool tempValue)
        {
            bool value = castShadowsToggle.isOn;

            for (int i = 0; i < FoliageDB.unSortedPrototypes.Count; i++)
            {
                FoliageDB.unSortedPrototypes[i].castShadows = value;
            }
        }

        public void OnWindChanged(bool tempValue)
        {
            bool value = windEnabledToggle.isOn;

            FoliageDB.instance.globalWindSettings.windSpeed = value ? 0.5f : 0;
        }

        public void OnColorMapsChanged(bool tempValue)
        {
            bool value = colorMapsEnabledToggle.isOn;

            for (int i = 0; i < FoliageDB.unSortedPrototypes.Count; i++)
            {
                FoliageDB.unSortedPrototypes[i].useColorMap = value;
            }
        }
        #endregion

        #region Methods
        private void Start()
        {
            _instance = this;
            PoppulateBrushesAndGrassPrototypes();

            //restore settings
            for (int i = 0; i < FoliageDB.unSortedPrototypes.Count; i++)
            {
                FoliageDB.unSortedPrototypes[i].useColorMap = false;
                FoliageDB.unSortedPrototypes[i].castShadows = true;
            }

            FoliageCore_MainManager.instance.density = 1;
            FoliageCore_MainManager.instance.globalFadeDistance = 100;
        }

        private void PoppulateBrushesAndGrassPrototypes()
        {
            grassPrototypes.Clear();
            paintPrototypes.Clear();

            var prototypes = FoliageDB.unSortedPrototypes;
            FoliagePrototype prototype;

            var brushes = FoliageDB.instance.brushes;
            PaintBrush brush;

            GameObject currentPrototypeInstance;
            UN_DemoPrototypeUI prototypeInstance;

            for (int i = 0; i < prototypes.Count; i++)
            {
                prototype = prototypes[i];

                if (prototype.FoliageType != FoliageType.Texture) continue;

                currentPrototypeInstance = Instantiate(prototypePrefab);

                prototypeInstance = currentPrototypeInstance.GetComponent<UN_DemoPrototypeUI>();

                prototypeInstance.Initialize(prototype.FoliageTexture, null, prototype);

                currentPrototypeInstance.transform.SetParent(grassPrototypesParent);
                currentPrototypeInstance.transform.position = Vector3.zero;
                currentPrototypeInstance.transform.rotation = Quaternion.identity;
                currentPrototypeInstance.transform.localScale = Vector3.one;

                grassPrototypes.Add(prototypeInstance);
            }

            for (int i = 0; i < brushes.Count; i++)
            {
                brush = brushes[i];

                currentPrototypeInstance = Instantiate(prototypePrefab);

                prototypeInstance = currentPrototypeInstance.GetComponent<UN_DemoPrototypeUI>();

                prototypeInstance.Initialize(brush.brushTexture, brush, null);

                currentPrototypeInstance.transform.SetParent(brushPrototypesParent);
                currentPrototypeInstance.transform.position = Vector3.zero;
                currentPrototypeInstance.transform.rotation = Quaternion.identity;
                currentPrototypeInstance.transform.localScale = Vector3.one;

                paintPrototypes.Add(prototypeInstance);
            }
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Tab))
            {
                enabled = !enabled;
            }

            if (enabled && chosenBrush != null && chosenPrototypes.Count > 0)
            {
                RaycastHit hit;
                var ray = UN_CameraFly.instance.camera.ScreenPointToRay(Input.mousePosition);

                List<RaycastResult> raycastResults = new List<RaycastResult>();
                eventSystem.RaycastAll(new PointerEventData(eventSystem) { position = Input.mousePosition, pointerId = -1 }, raycastResults);

                if (raycastResults.Count > 0)
                {
                   return;
                }

                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    UNBrushUtility.instance.DrawBrush(chosenBrush.paintBrush.brushTexture, Color.white, hit.point, ray.origin.y, brushSizeSlider.value * 100f);

                    if (Input.GetMouseButton(0) && Vector3.Distance(lastBrushPosition, hit.point) > 1)
                    {
                        lastBrushPosition = hit.point;

                        chosenPrototypes_Converted.Clear();

                        for (int i = 0; i < chosenPrototypes.Count; i++)
                        {
                            chosenPrototypes_Converted.Add(chosenPrototypes[i].prototype);
                        }

                        UNBrushUtility.PaintBrush(Input.GetKey(KeyCode.LeftShift), (int)(brushSizeSlider.value * 100f), (byte)(paintDensitySlider.value * 10f), (byte)(eraseDensitySlider.value * 10f), true, false, null, false, chosenPrototypes_Converted, new Vector2(hit.point.x, hit.point.z), chosenBrush.paintBrush);
                    }
                }
                else
                {
                    UNBrushUtility.projector.enabled = false;
                }
            }
            else
            {
                UNBrushUtility.projector.enabled = false;
            }
        }
        #endregion
    }
}
