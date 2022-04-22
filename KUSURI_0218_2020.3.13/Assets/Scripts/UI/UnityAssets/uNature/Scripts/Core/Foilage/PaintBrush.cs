using UnityEngine;
using System.Collections.Generic;

using uNature.Core.Utility;

namespace uNature.Core.FoliageClasses
{
    [System.Serializable]
    public class PaintBrush : BasePrototypeItem
    {
        public Texture2D brushTexture;

        #region Instancing
        int textureResizeTarget
        {
            get
            {
                return brushTexture.width / 20;
            }
        }

        Texture2D _instancedTexture;
        public Texture2D instancedTexture
        {
            get
            {
                if (_instancedTexture == null)
                {
                    _instancedTexture = UNBrushUtility.Resize(brushTexture, textureResizeTarget, textureResizeTarget);
                    _instancedTexture.hideFlags = HideFlags.DontSave;
                    _instancedTexture.Apply();

                    lastSize = 1;

                    _pixels = null;
                }

                return _instancedTexture;
            }
        }

        private int lastSize = 1;
        #endregion

        private Color32[,] _pixels;
        public Color32[,] pixels
        {
            get
            {
                if (_pixels == null)
                {
                    _pixels = new Color32[instancedTexture.width, instancedTexture.height];

                    for (int y = 0; y < instancedTexture.height; y++)
                    {
                        for (int x = 0; x < instancedTexture.width; x++)
                        {
                            _pixels[x, y] = instancedTexture.GetPixel(x, y);
                        }
                    }
                }

                return _pixels;
            }
        }

        public PaintBrush(Texture2D _texture)
        {
            this.brushTexture = _texture;
        }

        public void TryToResize(int size)
        {
            if(lastSize != size)
            {
                lastSize = size;

                Object.DestroyImmediate(_instancedTexture); // destroy the instance before instantiating a new one.

                _instancedTexture = UNBrushUtility.Resize(brushTexture, textureResizeTarget * size, textureResizeTarget * size);
                instancedTexture.Apply();

                _pixels = null;
            }
        }

        protected override Texture2D GetPreview()
        {
            #if UNITY_EDITOR
            return brushTexture;
            #else
            return null;
            #endif
        }
    }
}
