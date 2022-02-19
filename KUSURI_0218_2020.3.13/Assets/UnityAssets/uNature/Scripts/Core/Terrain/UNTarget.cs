using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uNature.Core.Seekers;
using uNature.Core.Threading;
using uNature.Core.Pooling;

namespace uNature.Core.Targets
{
    /// <summary>
    /// A target is what will be taken into account with the system.
    /// For example terrains.
    /// </summary>
    [ExecuteInEditMode]
    public abstract class UNTarget : ThreadItem
    {
        /// <summary>
        /// All of the targets in the scene.
        /// </summary>
        public static List<UNTarget> worldTargets = new List<UNTarget>();

        /// <summary>
        /// A Pool which is used to increase performance on runtime, which manages objects smartly than instantiating them manually on runtime each time.
        /// </summary>
        public Pool Pool = null;

        /// <summary>
        /// Was the Pool type de-serialized.
        /// </summary>
        [System.NonSerialized]
        private bool PoolTypeRead = false;

        /// <summary>
        /// Was the Pool type de-serialized.
        /// </summary>
        [SerializeField]
        public string PoolTypeSerializedName = "";

        /// <summary>
        /// What is the type of the Pool item created with this target?, for example harvestableTerrainItem.
        /// </summary>
        System.Type _PoolItemType;
        public System.Type PoolItemType
        {
            get
            {
                if(!PoolTypeRead) // deserialize the system.type, unity doesnt support it natively.
                {
                    if (PoolTypeSerializedName != "")
                    {
                        _PoolItemType = System.Type.GetType(PoolTypeSerializedName);
                    }

                    PoolTypeRead = true;
                }

                return _PoolItemType;
            }
            set
            {
                if (_PoolItemType != value)
                {
                    _PoolItemType = value;

                    PoolTypeSerializedName = value == null ? "" : value.FullName + ", " + value.Assembly.GetName().Name; // copy the value into a serializeable variable.

                    CreatePool(value);

                    #if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        UnityEditor.EditorUtility.SetDirty(this);
                    }
                    #endif
                }
            }
        }

        /// <summary>
        /// How many objects will be created for each Pool type.
        /// </summary>
        public int PoolAmount = 15;

        /// <summary>
        /// Will the system call a multi-threaded task for making the checks ?
        /// </summary>
        protected virtual bool useMultithreadedCheck
        {
            get { return true; }
        }

        /// <summary>
        /// Initiate awake settings.
        /// </summary>
        protected virtual void Awake()
        {
            if (!this.enabled || !Application.isPlaying) return;

            if (Pool == null)
                CreatePool(typeof(TerrainPoolItem));
        }

        /// <summary>
        /// Called every frame
        /// </summary>
        protected override void Update()
        {
            base.Update();
        }

        /// <summary>
        /// Add this target to the targets Pool
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            worldTargets.Add(this);
        }
        /// <summary>
        /// Remove this target to the targets Pool
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            worldTargets.Remove(this);
        }

        /// <summary>
        /// Check and apply AOI from seeker.
        /// <param name="seeker">Our seeker</param>
        /// <param name="seekerPos">the seeker position -> in order to maintain multithreading.</param>
        /// </summary>
        public virtual void Check(Seekers.UNSeeker seeker, Vector3 seekerPos, float seekingDistance, bool isPlaying)
        {
        }

        /// <summary>
        /// Confirm that a seeker is in the range of the target.
        /// </summary>
        /// <param name="seeker">The seeker.</param>
        /// <returns>Is the inrange of our target?</returns>
        public virtual bool InDistance(UNSeeker seeker)
        {
            return true;
        }

        /// <summary>
        /// Fix the position that is given to the local space position of this target - for example in the terrain you want to reduce the terrain position.
        /// </summary>
        /// <param name="position">the position</param>
        /// <returns>fixed position</returns>
        public virtual Vector3 FixPosition(Vector3 position)
        {
            return position;
        }

        /// <summary>
        /// Draw gizmos
        /// </summary>
        public virtual void OnDrawGizmos()
        {

        }

        /// <summary>
        /// Create Pool.
        /// </summary>
        public virtual void CreatePool(System.Type PoolItemType)
        {
            if(PoolItemType != null && PoolItemType.IsAbstract)
            {
                Debug.LogError("Cant add to Pool type : " + PoolItemType + " As it's an abstract class.");
                return;
            }

            this.PoolItemType = PoolItemType;

            if(Pool != null)
            {
                GameObject.DestroyImmediate(Pool.gameObject);
            }
            else
            {
                Pool.RemoveDuplications(name + " Pool");
            }

            Pool = Pool.CreatePool(name + " Pool", this.gameObject);
        }

        /// <summary>
        /// Check and apply aoi from a certain seeekr.
        /// </summary>
        /// <param name="seeker">our seeker.</param>
        /// <param name="distance">seeking distance</param>
        public static void CheckTargets(UNSeeker seeker, float distance)
        {
            if (UNThreadManager.instance == null) return;

            for(var i = 0; i < worldTargets.Count; i++)
            {
                var target = worldTargets[i];
                if (!target.InDistance(seeker)) continue;

                var task = new ThreadTask<UNTarget, UNSeeker, Vector3, bool>((_target, _seeker, _seekerPos, playing) =>
                {
                    _target.Check(_seeker, _seekerPos, _seeker.seekingDistance, playing);
                }, target, seeker, target.FixPosition(seeker.transform.position), Application.isPlaying);

                if (target.useMultithreadedCheck)
                {
                    UNThreadManager.instance.RunOnThread(task);
                }
                else
                {
                    task.Invoke();
                }
            }
        }
    }
}