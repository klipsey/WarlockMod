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
        private bool arrived;
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
            if (!NetworkServer.active || arrived || !target || !target.healthComponent || !target.healthComponent.alive) return;
            arrived = true;
            var body = target.healthComponent.body;
            if (!body) return;
            var identity = body.GetComponent<NetworkIdentity>();
            if (identity) new SyncOrbWarlock(identity.netId).Send(NetworkDestination.Clients);
        }
    }
}