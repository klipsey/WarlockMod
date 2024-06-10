using UnityEngine;
using RoR2;
using RoR2.Orbs;
using EntityStates.ImpMonster;
using UnityEngine.Networking;
using R2API.Networking;
using R2API.Networking.Interfaces;
using System.Reflection;
using WarlockMod.Warlock.Content;

namespace WarlockMod.Warlock.Components
{
    public class ConsumeOrb : Orb
    {
        public override void Begin()
        {
            base.duration = Mathf.Clamp(base.distanceToTarget / 5f, 0.5f, 1.5f);

            EffectData effectData = new EffectData
            {
                origin = this.origin,
                genericFloat = base.duration
            };

            effectData.SetHurtBoxReference(this.target);

            GameObject effectPrefab = WarlockAssets.consumeOrb;

            EffectManager.SpawnEffect(effectPrefab, effectData, true);
        }

        public override void OnArrival()
        {
            if (this.target)
            {
                if (this.target.healthComponent)
                {
                    if(NetworkServer.active)
                    {
                        this.target.healthComponent.body.AddBuff(WarlockBuffs.warlockCrimsonManaFullStack);
                    }
                    NetworkIdentity identity = this.target.healthComponent.gameObject.GetComponent<NetworkIdentity>();
                    if (!identity) return;
                    new SyncOrbWarlock(identity.netId, this.target.healthComponent.gameObject).Send(NetworkDestination.Clients);
                }
            }
        }
    }
}