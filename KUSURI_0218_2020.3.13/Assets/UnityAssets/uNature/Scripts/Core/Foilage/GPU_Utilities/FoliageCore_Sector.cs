using UnityEngine;
using System.Collections.Generic;

using uNature.Core.Sectors;

namespace uNature.Core.FoliageClasses
{
    public class FoliageCore_Sector : Sector
    {
        #region Variables
        public List<FoliageCore_Chunk> foliageChunks = new List<FoliageCore_Chunk>();

        protected override void OnChunkCreated(Chunk chunk)
        {
            base.OnChunkCreated(chunk);

            FoliageCore_Chunk FoliageChunkInstance = chunk as FoliageCore_Chunk;

            if (FoliageChunkInstance != null)
            {
                foliageChunks.Add(FoliageChunkInstance);
            }
        }

        protected override void OnStartCreatingChunks()
        {
            base.OnStartCreatingChunks();

            for (int i = 0; i < foliageChunks.Count; i++)
            {
                if (foliageChunks[i] != null)
                {
                    DestroyImmediate(foliageChunks[i]);
                }
            }

            foliageChunks.Clear();
        }

        protected override void OnResolutionChanged()
        {
            base.OnResolutionChanged();
        }
        #endregion
    }
}
