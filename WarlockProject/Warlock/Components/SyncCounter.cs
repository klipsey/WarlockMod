using UnityEngine.Networking;
using R2API.Networking;
using R2API.Networking.Interfaces;
using UnityEngine;
using RoR2;

namespace WarlockMod.Warlock.Components
{
    public class SyncCounter : INetMessage
    {
        private NetworkInstanceId netId;
        private GameObject target;
        private bool increase;

        public SyncCounter()
        {
        }

        public SyncCounter(NetworkInstanceId netId, GameObject target, bool increase)
        {
            this.netId = netId;
            this.target = target;
            this.increase = increase;
        }

        public void Deserialize(NetworkReader reader)
        {
            this.netId = reader.ReadNetworkId();
            this.target = reader.ReadGameObject();
            this.increase = reader.ReadBoolean();
        }

        public void OnReceived()
        {
            GameObject bodyObject = Util.FindNetworkObject(this.netId);
            if (!bodyObject) return;

            WarlockController iController = bodyObject.GetComponent<WarlockController>();
            int i = increase ? 1 : -1;
            if(iController)
            {
            }
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(this.netId);
            writer.Write(this.target);
            writer.Write(this.increase);
        }
    }
}