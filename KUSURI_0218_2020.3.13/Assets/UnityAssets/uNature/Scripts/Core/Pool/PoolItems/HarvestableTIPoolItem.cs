using UnityEngine;
using System.Collections;

using uNature.Core.Threading;
using uNature.Core.ClassExtensions;
using uNature.Core.Terrains;
using uNature.Core.Settings;

namespace uNature.Core.Pooling
{
    public delegate void OnHealthChanged(int value);
    public delegate void OnItemStateChanged(HarvestableTIPoolItem item);
    public delegate void OnItemDamaged(HarvestableTIPoolItem item, int damage);

    /// <summary>
    /// A Pool item for terrain where the tree instances should be harvestable. ( Tree cutting for instance )
    /// Inherite from this class to create your own harvestable type.
    /// </summary>
    public class HarvestableTIPoolItem : TerrainPoolItem, IHarvestableItem
    {
        #region Variables
        /// <summary>
        /// Can this machine harvest a COLLIDER ?
        /// </summary>
        public static bool canHarvestCollider = true;

        /// <summary>
        /// Called when an HarvestableTreeInstance has been Pooled
        /// </summary>
        public static event OnItemStateChanged OnItemPooledEvent;

        /// <summary>
        /// Called when any harvestable item has been damaged.
        /// </summary>
        public static event OnItemDamaged OnItemDamagedEvent;

        /// <summary>
        /// Called when an HarvestableTreeInstance has been returned to Pool
        /// </summary>
        public static event OnItemStateChanged OnItemReturnedToPoolEvent;

        /// <summary>
        /// the minimum health possible to be assigned to the tree instance (For example - 0).
        /// </summary>
        public int minHealth = 0;
        /// <summary>
        /// The maximum amount of health that can be assigned to this tree instance, which will also be assigned on default (For example - 100).
        /// </summary>
        public int maxHealth = 100;

        [SerializeField]
        int _health = -1;
        public int health
        {
            get { return _health; }
            set
            {
                value = Mathf.Clamp(value, minHealth, maxHealth);

                if (value != _health)
                {
                    int damage = Mathf.Abs(_health - value);

                    _health = value;

                    if(OnHealthChangedEvent != null)
                    {
                        OnHealthChangedEvent(damage);
                    }
                }
            }
        }

        public float respawnTimeInMinutes = 2;

        public float minFallDisappearTime = 2;
        public float maxFallDisappearTime = 10;
        #endregion

        #region Events
        public event OnHealthChanged OnHealthChangedEvent;
        #endregion

        /// <summary>
        /// Called on awake.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            OnHealthChangedEvent += HandleHealthChange;

            health = isCollider ? maxHealth : minHealth;
        }

        /// <summary>
        /// Handle the health change, remove the current tree instance if needed, and instantiate a replacment.
        /// <param name="damage">The amount of damage that the tree has received.</param>
        /// </summary>
        public virtual void HandleHealthChange(int damage)
        {
            if (health == minHealth)
            {
                if (isCollider) // if we are a collider
                {
                    HandleColliderDeath();
                }
                else // if we arent a collider.
                {
                    HandleTreeInstanceDeath();
                }
            }
        }

        /// <summary>
        /// Handle death of the colliders ( remove tree instance from terrain and replace it with actual tree instance prefab )
        /// </summary>
        public virtual void HandleColliderDeath()
        {
            if (canHarvestCollider)
            {
                ConvertTreeInstanceOnTerrain(terrain, uid);
            }
        }

        /// <summary>
        /// Handle death of an actual tree instance ( Add gravity and make it fall )
        /// </summary>
        public virtual void HandleTreeInstanceDeath()
        {
            locked = true;

            rigid.useGravity = true;
            rigid.isKinematic = false;

            rigid.AddForce(transform.forward * 5);

            rigid.mass = 100;
            rigid.drag = 2;
            rigid.angularDrag = 5;

            #if UNITY_5_4_OR_NEWER
            Random.InitState(uid);
            #else
            Random.seed = uid;
            #endif

            UNThreadManager.instance.DelayActionSeconds(new ThreadTask<PoolItem>((PoolItem _item) =>
                {
                    _item.locked = false;
                    Pool.ReturnToPool(_item, true);
                }, this), Random.Range(minFallDisappearTime, maxFallDisappearTime));
        }

        /// <summary>
        /// Hit this harvestable building and apply damage
        /// </summary>
        public virtual void Hit()
        {
            Hit(10);
        }

        /// <summary>
        /// Hit this harvestable building and apply damage
        /// <param name="damage">apply the damage</param>
        /// </summary>
        public virtual void Hit(int damage)
        {
            if (!canModify || health == 0) return;

            health -= damage;

            if (OnItemDamagedEvent != null)
            {
                OnItemDamagedEvent(this, damage);
            }
        }

        /// <summary>
        /// Called when the item returns to the Pool, reset the propoties
        /// </summary>
        public override void OnReturnedToPool()
        {
            UNThreadManager.instance.RunOnUnityThread(new ThreadTask(() =>
            {
                rigid.isKinematic = true;
                health = maxHealth;

                if (OnItemReturnedToPoolEvent != null)
                {
                    OnItemReturnedToPoolEvent(this);
                }
            }));

            base.OnReturnedToPool();
        }

        /// <summary>
        /// Called when the item pulled to the Pool
        /// </summary>
        public override void OnPool()
        {
            base.OnPool();

            locked = false;

            UNThreadManager.instance.RunOnUnityThread(new ThreadTask(() =>
            {
                health = isCollider ? maxHealth : minHealth;

                if (OnItemPooledEvent != null)
                {
                    OnItemPooledEvent(this);
                }
            }));
        }
    }

    public interface IHarvestableItem
    {
        void Hit();
        void Hit(int damage);
    }
}
