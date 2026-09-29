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
        private bool victimDied;

        public SyncBloodExplosion() { }
        public SyncBloodExplosion(NetworkInstanceId netId, Vector3 position, bool victimDied)
        {
            this.netId = netId;
            this.position = position;
            this.victimDied = victimDied;
        }
        public void Deserialize(NetworkReader reader)
        {
            netId = reader.ReadNetworkId();
            position = reader.ReadVector3();
            victimDied = reader.ReadBoolean();
        }
        public void Serialize(NetworkWriter writer)
        {
            writer.Write(netId);
            writer.Write(position);
            writer.Write(victimDied);
        }
        public void OnReceived()
        {
            if (!NetworkClient.active) return;
            var bodyObject = Util.FindNetworkObject(netId);
            EffectManager.SpawnEffect(WarlockAssets.bloodExplosionEffect,
                new EffectData { origin = position, rotation = Quaternion.identity, scale = 0.5f }, false);
            // The death message can arrive before the client's health update.
            if (victimDied && bodyObject && !bodyObject.GetComponent<BloodExplosion>())
                bodyObject.AddComponent<BloodExplosion>();
        }
    }
}
