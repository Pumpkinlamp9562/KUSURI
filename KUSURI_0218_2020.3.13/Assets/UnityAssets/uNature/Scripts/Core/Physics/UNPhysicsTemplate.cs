using UnityEngine;
using System.Collections;

using uNature.Core.FoliageClasses;

namespace uNature.Core
{
    public struct UNPhysicsTemplate
    {
        public Vector3 position;
        public Vector2 spread;

        public int densityIndex;
        public FoliagePrototype prototype;

        public UNPhysicsTemplate(Vector3 position, float spreadX, float spreadZ, int densityIndex, FoliagePrototype prototype)
        {
            this.position       = position;
            this.spread.x       = spreadX;
            this.spread.y       = spreadZ;
            this.densityIndex   = densityIndex;
            this.prototype      = prototype;
        }
    }
}