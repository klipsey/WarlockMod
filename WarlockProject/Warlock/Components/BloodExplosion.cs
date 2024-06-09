using UnityEngine;
using RoR2;

namespace WarlockMod.Warlock.Components
{
    public class BloodExplosion : MonoBehaviour
    {
        private void Awake()
        {
            CharacterBody characterBody = this.GetComponent<CharacterBody>();

            if (this.transform)
            {
                EffectManager.SpawnEffect(Content.WarlockAssets.bloodExplosionEffect, new EffectData
                {
                    origin = this.transform.position,
                    rotation = Quaternion.identity,
                    scale = 0.5f
                }, false);
            }
        }

        private void LateUpdate()
        {
            if (this.transform) this.transform.localScale = Vector3.zero;
        }
    }
}