using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using uNature.Core.Settings;
using uNature.Core.Utility;

namespace uNature.Core.FoliageClasses
{
    /// <summary>
    /// The database class of the Foliage,
    /// holds a lot of important data such as Foliage prototypes, Foliage map and more.
    /// </summary>
    public sealed class FoliageDB : ScriptableObject
    {
        #region Static
        static string assetName = "FoliageDatabase";
        static string path = UNSettings.ProjectPath + "Resources/" + assetName;

        static FoliageDB _instance;
        /// <summary>
        /// Get the instance, if not found, it will automatically create one.
        /// </summary>
        public static FoliageDB instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = Resources.Load<FoliageDB>(assetName);

                    if(_instance == null) // if cant find the instance on the database, lets create one!
                    {
                        _instance = CreateInstance<FoliageDB>();

                        #if UNITY_EDITOR
                        UnityEditor.AssetDatabase.CreateAsset(_instance, path + ".asset");
                        UnityEditor.AssetDatabase.SaveAssets();
                        #endif
                    }
                }

                return _instance;
            }
        }
        #endregion

        #region Variables
        [SerializeField]
        internal List<FoliagePrototype> _prototypes = new List<FoliagePrototype>();
        public static List<FoliagePrototype> unSortedPrototypes
        {
            get
            {
                return instance._prototypes;
            }
        }

        static Dictionary<int, FoliagePrototype> _prototypesDictionary;
        public static Dictionary<int, FoliagePrototype> sortedPrototypes
        {
            get
            {
                if (_prototypesDictionary == null)
                {
                    _prototypesDictionary = new Dictionary<int, FoliagePrototype>();

                    for (int i = 0; i < instance._prototypes.Count; i++)
                    {
                        _prototypesDictionary.Add(instance._prototypes[i].id, instance._prototypes[i]);
                    }
                }

                return _prototypesDictionary;
            }
            private set
            {
                _prototypesDictionary = value;
            }
        }

        [System.NonSerialized]
        List<PaintBrush> _brushes = null;
        public List<PaintBrush> brushes
        {
            get
            {
                if(_brushes == null)
                {
                    _brushes = new List<PaintBrush>();

                    Texture2D[] cachedBrushes = Resources.LoadAll<Texture2D>("Brushes");

                    for(int i = 0; i < cachedBrushes.Length; i++)
                    {
                        _brushes.Add(new PaintBrush(cachedBrushes[i]));
                    }
                }

                return _brushes;
            }
            set
            {
                _brushes = value;
            }
        }

        public WindSettings globalWindSettings = new WindSettings();

        private byte getID
        {
            get
            {
                const byte LIMIT = byte.MaxValue;
                for (byte i = 1; i < LIMIT; i++)
                {
                    if (sortedPrototypes.ContainsKey(i)) continue;
                    return i;
                }

                throw new IndexOutOfRangeException("Can't create any more prototypes! Limit Reached!");
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Add a new Foliage prototype.
        /// </summary>
        public FoliagePrototype AddPrototype(Texture2D texture, GameObject prefab, float minWidth, float minHeight, float maxWidth, float maxHeight, float spread, int layer, Color healthyColor, Color dryColor)
        {
            var id = getID;
            var prototype = FoliagePrototype.CreatePrototype(texture, prefab, minWidth, minHeight, maxWidth, maxHeight, spread, layer, id, healthyColor, dryColor);

            FoliageMeshManager.GenerateFoliageMeshInstances(id);
            FoliageMeshManager.RegenerateQueueInstances();

            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
            #endif

            return prototype;
        }

        /// <summary>
        /// Add a new Foliage prototype.
        /// </summary>
        public FoliagePrototype AddPrototype(DetailPrototype detailPrototype)
        {
            return AddPrototype(detailPrototype.prototypeTexture, detailPrototype.prototype, detailPrototype.minWidth, detailPrototype.minHeight, detailPrototype.maxWidth, detailPrototype.maxHeight, detailPrototype.noiseSpread * 10, 0, detailPrototype.healthyColor, detailPrototype.dryColor);
        }

        /// <summary>
        /// Add a new Foliage prototype.
        /// </summary>
        public FoliagePrototype AddPrototype(Texture2D texture)
        {
            return AddPrototype(texture, null, 1.5f, 1.5f, 2f, 2f, 1f, 0, FoliagePrototype.DEFAULT_HEALTHY_COLOR, FoliagePrototype.DEFAULT_DRY_COLOR);
        }

        /// <summary>
        /// Add a new Foliage prototype.
        /// </summary>
        public FoliagePrototype AddPrototype(GameObject prefab)
        {
            if (prefab == null) return null;

            Color healthyColor = FoliagePrototype.DEFAULT_HEALTHY_COLOR;
            Color dryColor = FoliagePrototype.DEFAULT_DRY_COLOR;

            MeshRenderer mRenderer = prefab.GetComponentInChildren<MeshRenderer>();

            if (mRenderer != null)
            {
                Material mat = mRenderer.sharedMaterial;

                if(mat != null)
                {
                    if(mat.HasProperty("_healthyColor"))
                    {
                        healthyColor = mat.GetColor("_healthyColor");
                    }

                    if(mat.HasProperty("_dryColor"))
                    {
                        dryColor = mat.GetColor("_dryColor");
                    }
                }
            }


            return AddPrototype(null, prefab, 1.5f, 1f, 2f, 1.2f, 1f, prefab == null ? 0 : prefab.layer, healthyColor, dryColor);
        }

        /// <summary>
        /// Remove an existing Foliage prototype.
        /// </summary>
        public void RemovePrototype(FoliagePrototype prototype)
        {
            _prototypes.Remove(prototype);
            sortedPrototypes.Remove(prototype.id);

            FoliageMeshManager.DestroyMeshInstance(prototype.id);
            FoliageMeshManager.RegenerateQueueInstances();

            #if UNITY_EDITOR
            prototype.DisposeOfCurrentMaterial();
            #endif

            System.GC.SuppressFinalize(prototype);
        }

        /// <summary>
        /// Register a new prototype.
        /// </summary>
        /// <param name="prototype"></param>
        internal void RegisterNewPrototype(FoliagePrototype prototype)
        {
            _prototypes.Add(prototype);

            if (!sortedPrototypes.ContainsKey(prototype.id))
            {
                sortedPrototypes.Add(prototype.id, prototype);
            }
        }

        /// <summary>
        /// Update wind settings globally
        /// </summary>
        public void UpdateShaderWindSettings()
        {
            FoliagePrototype prototype;

            for(int i = 0; i < unSortedPrototypes.Count; i++)
            {
                prototype = unSortedPrototypes[i];

                if (!prototype.enabled) continue;

                prototype.ApplyWind();
            }
        }
        
        /// <summary>
        /// This will update the general settigns of the shader such as density, min width, max width etc
        /// </summary>
        public void UpdateShaderGeneralSettings()
        {
            for (int i = 0; i < unSortedPrototypes.Count; i++)
            {
                unSortedPrototypes[i].UpdateManagerInformation();
            }
        }

        /// <summary>
        /// Change the prototypes sorting.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public static void SwitchPrototypes(FoliagePrototype from, FoliagePrototype to)
        {
            int fromIndex = unSortedPrototypes.IndexOf(from);
            int toIndex = unSortedPrototypes.IndexOf(to);

            if (fromIndex == -1 || toIndex == -1) return;

            // replace slots
            unSortedPrototypes[fromIndex] = to;
            unSortedPrototypes[toIndex] = from;

            sortedPrototypes = null; // reset organization.

            var fromID = from.id;
            var toID = to.id;

            // replace ids
            from._foliageID_INTERNAL = toID;
            to._foliageID_INTERNAL = fromID;

            from.GenerateInstantiatedMesh(from.healthyColor, from.dryColor);
            to.GenerateInstantiatedMesh(to.healthyColor, to.dryColor);

            if (FoliageCore_MainManager.instance != null)
            {
                FoliageMeshManager.RegenerateQueueInstances();
                FoliageMeshManager.GenerateFoliageMeshInstances();
                instance.UpdateShaderGeneralSettings();
            }

            Debug.Log("Succesfully switched prototype " + from.name + " with prototype " + to.name);
        }
        #endregion
    }

    [System.Serializable]
    public class WindSettings
    {
        [SerializeField]
        float _windBending = 1;
        public float windBending
        {
            get
            {
                return _windBending;
            }
            set
            {
                if (_windBending != value)
                {
                    _windBending = value;

                    FoliageDB.instance.UpdateShaderWindSettings();
                }
            }
        }

        [SerializeField]
        float _windSpeed = 1;
        public float windSpeed
        {
            get
            {
                return _windSpeed;
            }
            set
            {
                if (_windSpeed != value)
                {
                    _windSpeed = value;

                    FoliageDB.instance.UpdateShaderWindSettings();
                }
            }
        }
    }
}
