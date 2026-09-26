using UnityEngine.Networking;
using R2API.Networking.Interfaces;
using UnityEngine;
using RoR2;
using WarlockMod.Warlock.Content;

namespace WarlockMod.Warlock.Components
{
    public class SyncBloodExplosion : INetMessage
    {
        private NetworkInstanceId netId;
        private Vector3 position;

        public SyncBloodExplosion() { }
        public SyncBloodExplosion(NetworkInstanceId netId, Vector3 position)
        {
            this.netId = netId;
            this.position = position;
        }
        public void Deserialize(NetworkReader reader)
        {
            netId = reader.ReadNetworkId();
            position = reader.ReadVector3();
        }
        public void Serialize(NetworkWriter writer)
        {
            writer.Write(netId);
            writer.Write(position);
        }
        public void OnReceived()
        {
            if (!NetworkClient.active) return;
            var bodyObject = Util.FindNetworkObject(netId);
            var health = bodyObject ? bodyObject.GetComponent<HealthComponent>() : null;
            if (health && health.alive) return;
            EffectManager.SpawnEffect(WarlockAssets.bloodExplosionEffect,
                new EffectData { origin = position, rotation = Quaternion.identity, scale = 0.5f }, false);
            if (bodyObject && !bodyObject.GetComponent<BloodExplosion>()) bodyObject.AddComponent<BloodExplosion>();
        }
    }
}
