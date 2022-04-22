using UnityEngine;
using System.Collections;

using uNature.Core.FoliageClasses;
using uNature.Core.Terrains;

namespace uNature.Demo
{
    public class DemoControlManager : MonoBehaviour
    {
        #region ExposedVariables
        [SerializeField]
        private UNTerrain terrain;

        [SerializeField]
        private Camera renderingCamera;

        [SerializeField]
        private Rigidbody touchBendingBallRigid;
        #endregion

        private bool _lodEnabled = true;
        public bool lodEnabled
        {
            get
            {
                return _lodEnabled;
            }
            set
            {
                if (_lodEnabled != value)
                {
                    _lodEnabled = value;

                    for (int i = 0; i < FoliageDB.unSortedPrototypes.Count; i++)
                    {
                        FoliageDB.unSortedPrototypes[i].useLODs = value;
                    }
                }
            }
        }

        private bool _uNatureGrass = true;
        public bool uNatureGrass
        {
            get
            {
                return _uNatureGrass;
            }
            set
            {
                if(_uNatureGrass != value)
                {
                    _uNatureGrass = value;

                    if(value) // switch to uNature Grass
                    {
                        terrain.terrainData.ClearDetails();
                        FoliageCore_MainManager.instance.enabled = true;
                    }
                    else
                    {
                        FoliageCore_MainManager.instance.enabled = false;
                        terrain.terrainData.ApplyBackup(terrain.terrain);
                    }
                }
            }
        }

        private bool _windEnabled = true;
        public bool windEnabled
        {
            get
            {
                return _windEnabled;
            }
            set
            {
                if (_windEnabled != value)
                {
                    _windEnabled = value;

                    FoliageDB.instance.globalWindSettings.windSpeed = value ? 2f : 0f;

                    terrain.terrain.terrainData.wavingGrassSpeed = value ? 0.5f : 0;
                    terrain.terrain.terrainData.wavingGrassStrength = value ? 0.5f : 0;
                    terrain.terrain.terrainData.wavingGrassAmount = value ? 0.25f : 0;
                }
            }
        }

        private bool _useInstancing = true;
        public bool useInstancing
        {
            get
            {
                return _useInstancing;
            }
            set
            {
                if (_useInstancing != value)
                {
                    _useInstancing = value;

                    for (int i = 0; i < FoliageDB.unSortedPrototypes.Count; i++)
                    {
                        FoliageDB.unSortedPrototypes[i].useInstancing = value;
                    }
                }
            }
        }

        private float _grassDensity = 1f;
        public float grassDensity
        {
            get
            {
                return _grassDensity;
            }
            set
            {
                if(_grassDensity != value)
                {
                    _grassDensity = value;

                    FoliageCore_MainManager.instance.density = value;
                    terrain.terrain.detailObjectDensity = value;
                }
            }
        }

        private bool _castShadows = false;
        public bool castShadows
        {
            get
            {
                return _castShadows;
            }
            set
            {
                if (_castShadows != value)
                {
                    _castShadows = value;

                    for (int i = 0; i < FoliageDB.unSortedPrototypes.Count; i++)
                    {
                        FoliageDB.unSortedPrototypes[i].castShadows = value;
                    }
                }
            }
        }
    
        private bool _colorMapsEnabled = false;
        public bool colorMapsEnabled
        {
            get
            {
                return _colorMapsEnabled;
            }
            set
            {
                if(_colorMapsEnabled != value)
                {
                    _colorMapsEnabled = value;

                    for (int i = 0; i < FoliageDB.unSortedPrototypes.Count; i++)
                    {
                        FoliageDB.unSortedPrototypes[i].useColorMap = value;
                    }
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                lodEnabled = !lodEnabled;
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                uNatureGrass = !uNatureGrass;
            }
            else if (Input.GetKeyDown(KeyCode.F2))
            {
                castShadows = !castShadows;
            }
            else if (Input.GetKeyDown(KeyCode.F3))
            {
                colorMapsEnabled = !colorMapsEnabled;
            }
            else if (Input.GetKeyDown(KeyCode.F4))
            {
                windEnabled = !windEnabled;
            }
            else if (Input.GetKeyDown(KeyCode.F5))
            {
                useInstancing = !useInstancing;
            }
            else if (Input.GetKeyDown(KeyCode.F6))
            {
                if(touchBendingBallRigid != null && renderingCamera != null)
                {
                    touchBendingBallRigid.transform.position = renderingCamera.transform.position + (renderingCamera.transform.forward * 1.5f);
                    touchBendingBallRigid.velocity = renderingCamera.transform.forward * 10f;
                }
            }
            else if (Input.GetKey(KeyCode.Equals))
            {
                grassDensity = Mathf.Clamp(grassDensity + Time.deltaTime, 0, 1);
            }
            else if (Input.GetKey(KeyCode.Minus))
            {
                grassDensity = Mathf.Clamp(grassDensity - Time.deltaTime, 0, 1);
            }
        }
    }
}