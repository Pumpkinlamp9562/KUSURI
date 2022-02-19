using UnityEngine;
using System.Collections.Generic;

using uNature.Core.FoliageClasses;

namespace uNature.Core.Utility
{
    /// <summary>
    /// Using this class you can paint an brush on the scene.
    /// </summary>
    [ExecuteInEditMode]
    public class UNBrushUtility : MonoBehaviour
    {
        const string brushGOPath = "Brushes/Prefabs/BrushProjector";
        private static Quaternion DownRotation = Quaternion.Euler(90, 0, 0);

        static UNBrushUtility _instance;
        public static UNBrushUtility instance
        {
            get
            {
                if(_instance == null)
                {
                    var instances = GameObject.FindObjectsOfType<UNBrushUtility>();
                    
                    for(int i = 0; i < instances.Length; i++)
                    {
                        DestroyImmediate(instances[i].gameObject);
                    }

                    if(_instance == null)
                    {
                        GameObject go = GameObject.Instantiate(Resources.Load<GameObject>(brushGOPath));
                        go.hideFlags = HideFlags.HideInHierarchy;

                        _instance = go.GetComponent<UNBrushUtility>();

                        if(_instance == null)
                        {
                            _instance = go.AddComponent<UNBrushUtility>();
                        }
                    }
                }

                return _instance;
            }
        }

        static Projector _projector;
        public static Projector projector
        {
            get
            {
                if(_projector == null)
                {
                    _projector = instance.GetComponent<Projector>();
                }

                return _projector;
            }
        }

        #region Terrain Splat Textures
        [System.NonSerialized]
        private List<UN_TerrainTexturePrototype> _splatPrototypes = null;
        public List<UN_TerrainTexturePrototype> splatPrototypes
        {
            get
            {
                if (_splatPrototypes == null)
                {
                    _splatPrototypes = new List<UN_TerrainTexturePrototype>();

                    Terrain[] terrains = Terrain.activeTerrains;
                    Terrain terrain;

                    SplatPrototype splat;
                    bool found = false;

                    for (int i = 0; i < terrains.Length; i++)
                    {
                        terrain = terrains[i];

                        for (int b = 0; b < terrain.terrainData.splatPrototypes.Length; b++)
                        {
                            splat = terrain.terrainData.splatPrototypes[b];

                            for (int c = 0; c < _splatPrototypes.Count; c++)
                            {
                                if (_splatPrototypes[c].splatTexture == splat.texture)
                                {
                                    found = true;
                                    break;
                                }
                            }

                            if (found)
                            {
                                continue;
                            }

                            _splatPrototypes.Add(new UN_TerrainTexturePrototype(splat.texture));
                        }
                    }
                }

                return _splatPrototypes;
            }
        }
        #endregion

        /// <summary>
        /// Draw a brush on the scene.
        /// </summary>
        /// <param name="brushTexture">The brush's texture.</param>
        /// <param name="brushColor">The brush's color.</param>
        /// <param name="position">The brush's origin position (for example the camera's position).</param>
        /// <param name="rotation">The brush's origin rotation (for example the camera's rotation).</param>
        /// <param name="brushSize">The brush's size. (Varies from 1 -> 100)</param>
        public void DrawBrush(Texture2D brushTexture, Color brushColor, Vector3 originPosition, Quaternion originRotation, float brushSize)
        {
            projector.enabled = true;

            projector.material.SetTexture("_ShadowTex", brushTexture);

            projector.transform.position = originPosition;
            projector.transform.rotation = originRotation;

            projector.orthographicSize = brushSize;
        }

        /// <summary>
        /// Draw a brush on the scene.
        /// </summary>
        /// <param name="brushTexture">The brush's texture.</param>
        /// <param name="brushColor">The brush's color.</param>
        /// <param name="position">The brush's origin position (for example the camera's position).</param>
        /// <param name="rotation">The brush's origin rotation (for example the camera's rotation).</param>
        /// <param name="brushSize">The brush's size. (Varies from 1 -> 100)</param>
        public void DrawBrush(Texture2D brushTexture, Color brushColor, Vector3 hitPoint, float height, float brushSize)
        {
            projector.enabled = true;

            projector.material.SetTexture("_ShadowTex", brushTexture);

            hitPoint.y = height;
            projector.transform.position = hitPoint;

            projector.transform.rotation = DownRotation;

            projector.orthographicSize = brushSize;
        }

        /// <summary>
        /// Resize texture by Justin Markwell and Smoke.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        /// <returns></returns>
        public static Texture2D Resize(Texture2D source, int newWidth, int newHeight)
        {
            source.filterMode = FilterMode.Point;
            RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
            rt.filterMode = FilterMode.Point;
            RenderTexture.active = rt;
            Graphics.Blit(source, rt);
            var nTex = new Texture2D(newWidth, newHeight);
            nTex.hideFlags = HideFlags.HideAndDontSave;
            nTex.ReadPixels(new Rect(0, 0, newWidth, newWidth), 0, 0);
            nTex.Apply();
            RenderTexture.active = null;
            return nTex;
        }

        /// <summary>
        /// Checks if the splats are in that specific position.
        /// </summary>
        /// <param name="hit"></param>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public static float CheckSplatPaint(RaycastHit hit, Vector3 worldPosition, List<UN_TerrainTexturePrototype> chosenSplats)
        {
            if (chosenSplats.Count == 0) return 1;

            Terrain terrain = hit.transform.GetComponent<Terrain>();

            if (terrain == null) return 0;

            TerrainData terrainData = terrain.terrainData;

            worldPosition.x -= terrain.transform.position.x;
            worldPosition.z -= terrain.transform.position.z;

            worldPosition.x = (worldPosition.x / terrainData.size.x) * terrainData.alphamapWidth;
            worldPosition.z = (worldPosition.z / terrainData.size.z) * terrainData.alphamapHeight;

            var alphaMap = terrainData.GetAlphamaps((int)worldPosition.x, (int)worldPosition.z, 1, 1);

            int overlapAmount = 0;
            float splatsAverege = 0;

            float splatValue;

            for(int i = 0; i < terrainData.splatPrototypes.Length; i++)
            {
                for (int b = 0; b < chosenSplats.Count; b++)
                {
                    if(terrainData.splatPrototypes[i].texture == chosenSplats[b].splatTexture)
                    {
                        splatValue = alphaMap[0, 0, i];

                        if (splatValue != 0)
                        {
                            splatsAverege += splatValue;
                            overlapAmount++;
                        }

                        break;
                    }
                }
            }

            return overlapAmount == 0 ? 0 : splatsAverege / overlapAmount;
        }

        /// <summary>
        /// Use a brush to paint on a position.
        /// </summary>
        /// <param name="isErase">Are you erasing grass?</param>
        /// <param name="brushSize">Brush size (1 ~ 100)/param>
        /// <param name="paintDensity">Paint density (when not erasing, can only increase density in a certain zone)</param>
        /// <param name="eraseDensity">Erase density (when erasing, can only decrease density in a certain zone)</param>
        /// <param name="createNewManagers">Create new managers on areas that arent poppulated</param>
        /// <param name="useTerrainBasedPainting">Use terrain painting features (only paint on certain splats, etc)</param>
        /// <param name="splats">Chosen terrain splats (Leave null if not using terrain features)</param>
        /// <param name="interpolateSplatValues">Will it interpolate the splat values (when there's underlaying splats, leave false if terrain painting isnt enabled)</param>
        /// <param name="selectedPrototypes">The protototypes you want to draw/ erase.</param>
        /// <param name="position">The position you want to draw on (world coordinates)</param>
        /// <param name="brush">The brush you want to draw with.</param>
        public static void PaintBrush(bool isErase, int brushSize, byte paintDensity, byte eraseDensity, bool createNewManagers, bool useTerrainBasedPainting, List<UN_TerrainTexturePrototype> splats, bool interpolateSplatValues, List<FoliagePrototype> selectedPrototypes, Vector2 position, PaintBrush brush)
        {
            var mManager = FoliageCore_MainManager.instance;

            brushSize = Mathf.Clamp(brushSize, 1, 100);
            brush.TryToResize(brushSize);

            var texture = brush.instancedTexture;

            var textureWidth = texture.width;
            var textureHeight = texture.height;
            var radius = Mathf.FloorToInt((float)textureWidth / 2);

            Vector3 worldPosition;
            worldPosition.y = 1000;

            float splatsValue = 1;

            for (var prototypeIndex = 0; prototypeIndex < selectedPrototypes.Count; prototypeIndex++)
            {
                var prototype = selectedPrototypes[prototypeIndex];
                var maxGeneratableDensity = prototype.maxGeneratedDensity;

                for (var x = 0; x < textureWidth; x++)
                {
                    for (var y = 0; y < textureHeight; y++)
                    {
                        var pixel = brush.pixels[x, y];
                        if (pixel.r != 255 || pixel.g != 255 || pixel.b != 255) continue;

                        var maskedDensity = Mathf.FloorToInt(paintDensity * ((float)pixel.a / 255));

                        worldPosition.x = x - radius + (int)position.x;
                        worldPosition.z = y - radius + (int)position.y;

                        var posX = (int)(worldPosition.x - mManager.transform.position.x);
                        var posZ = (int)(worldPosition.z - mManager.transform.position.z);

                        var chunkIndex = mManager.GetChunkID(posX, posZ);

                        if (!createNewManagers)
                        {
                            if (!mManager.sector.foliageChunks[chunkIndex].isFoliageInstanceAttached) continue;
                        }

                        if (useTerrainBasedPainting)
                        {
                            RaycastHit hit;
                            var rayCastHit = Physics.Raycast(worldPosition, Vector3.down, out hit, Mathf.Infinity);

                            splatsValue = rayCastHit ? CheckSplatPaint(hit, worldPosition, splats) : 0;
                            if (splatsValue == 0) continue;
                        }

                        var mInstance = mManager.sector.foliageChunks[chunkIndex].GetOrCreateFoliageManagerInstance();

                        var grassMap = mInstance.grassMap;
                        var mapWidth = grassMap.mapWidth;

                        var transformedCordX = mInstance.TransformCord(worldPosition.x, mInstance.pos.x);
                        var transformedCordZ = mInstance.TransformCord(worldPosition.z, mInstance.pos.z);
                        var transformedIndex = transformedCordX + transformedCordZ * mapWidth;

                        var density = grassMap.GetPrototypeDensity(transformedCordX, transformedCordZ, prototype.id);
                        
                        var realPaintDensity = (byte)Mathf.Clamp(maskedDensity * (interpolateSplatValues ? splatsValue : 1), 0, maxGeneratableDensity);
                        if (realPaintDensity <= density && (!isErase || eraseDensity >= density)) continue;

                        grassMap.SetDensity(transformedIndex, prototype.id, isErase ? eraseDensity : realPaintDensity);

                        grassMap.dirty = true;
                        grassMap.SetPixels32Delayed();
                    }
                }
            }

            FoliageCore_MainManager.SaveDelayedMaps();
            FoliageMeshManager.MarkDensitiesDirty();
        }

        /// <summary>
        /// On Start
        /// </summary>
        private void Start()
        {
            projector.enabled = false;
        }

        void Update()
        {
            if (!Application.isPlaying)
            {
                projector.enabled = false;
            }
        }
    }

    /// <summary>
    /// The prototype of the terrain texture.
    /// [Used for listing the paintable surfaces]
    /// </summary>
    [System.Serializable]
    public class UN_TerrainTexturePrototype : BasePrototypeItem
    {
        [System.NonSerialized]
        public Texture2D splatTexture;

        public UN_TerrainTexturePrototype(Texture2D splatTexture) : base()
        {
            this.splatTexture = splatTexture;
        }

        /// <summary>
        /// Get a preview of the splat texture.
        /// </summary>
        /// <returns></returns>
        protected override Texture2D GetPreview()
        {
            if (splatTexture == null) return null;

            #if UNITY_EDITOR
            return UnityEditor.AssetPreview.GetAssetPreview(splatTexture);
            #else
            return null;
            #endif
        }
    }
}
