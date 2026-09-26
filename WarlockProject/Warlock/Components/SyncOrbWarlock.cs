using UnityEngine.Networking;
using R2API.Networking.Interfaces;
using UnityEngine;
using RoR2;
using WarlockMod.Warlock.Content;

namespace WarlockMod.Warlock.Components
{
    public class SyncOrbWarlock : INetMessage
    {
        private NetworkInstanceId netId;

        public SyncOrbWarlock() { }
        public SyncOrbWarlock(NetworkInstanceId netId) => this.netId = netId;
        public void Deserialize(NetworkReader reader) => netId = reader.ReadNetworkId();
        public void Serialize(NetworkWriter writer) => writer.Write(netId);

        public void OnReceived()
        {
            if (!NetworkClient.active) return;
            var bodyObject = Util.FindNetworkObject(netId);
            if (!bodyObject || !bodyObject.GetComponent<WarlockController>()) return;
            var body = bodyObject.GetComponent<CharacterBody>();
            var modelTransform = body && body.modelLocator ? body.modelLocator.modelTransform : null;
            if (!modelTransform) return;
            var overlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
            overlay.duration = 1f;
            overlay.destroyComponentOnEnd = true;
            overlay.originalMaterial = WarlockAssets.destealthMaterial;
            overlay.inspectorCharacterModel = modelTransform.GetComponent<CharacterModel>();
            overlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
            overlay.animateShaderAlpha = true;
            Util.PlaySound("Play_item_proc_novaonheal_spawn", bodyObject);
        }
    }
}
