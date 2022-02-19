#if UN_UFPS

using UnityEngine;
using System.Collections;

using uNature.Core.Pooling;

namespace uNature.Extensions.UFPS
{
    public class UN_DamageHandler : vp_DamageHandler, IPoolComponent
    {
        HarvestableTIPoolItem _harvestableComponent;
        public HarvestableTIPoolItem harvestableComponent
        {
            get
            {
                if(_harvestableComponent == null)
                {
                    _harvestableComponent = GetComponentInParent<HarvestableTIPoolItem>();

                    if(_harvestableComponent == null)
                    {
                        this.enabled = false;
                        return null;
                    }
                }

                return _harvestableComponent;
            }
        }

        public bool copyTest = false;

        protected virtual void Start()
        {
            _harvestableComponent = GetComponentInParent<HarvestableTIPoolItem>();

            CurrentHealth = harvestableComponent.health;
            MaxHealth = harvestableComponent.maxHealth;
        }

        public override void Damage(vp_DamageInfo damageInfo)
        {
            harvestableComponent.Hit((int)damageInfo.Damage);
        }

        public override void Damage(float damage)
        {
            harvestableComponent.Hit((int)damage);
        }
    }
}

#endif