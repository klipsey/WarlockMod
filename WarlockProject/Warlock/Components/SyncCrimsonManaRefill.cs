using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine.Networking;
using WarlockMod.Warlock.SkillStates;

namespace WarlockMod.Warlock.Components
{
    public class SyncCrimsonManaRefill : INetMessage
    {
        private NetworkInstanceId bodyId;
        private uint requestId;
        private bool accepted;

        public SyncCrimsonManaRefill() { }

        public SyncCrimsonManaRefill(NetworkInstanceId bodyId, uint requestId, bool accepted)
        {
            this.bodyId = bodyId;
            this.requestId = requestId;
            this.accepted = accepted;
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(bodyId);
            writer.Write(requestId);
            writer.Write(accepted);
        }

        public void Deserialize(NetworkReader reader)
        {
            bodyId = reader.ReadNetworkId();
            requestId = reader.ReadUInt32();
            accepted = reader.ReadBoolean();
        }

        public void OnReceived()
        {
            if (NetworkServer.active || !NetworkClient.active) return;
            var bodyObject = Util.FindNetworkObject(bodyId);
            if (!bodyObject || !Util.HasEffectiveAuthority(bodyObject)) return;
            var machine = EntityStateMachine.FindByCustomName(bodyObject, "Weapon2");
            if (machine && machine.state is CrimsonManaRefill refill && refill.requestId == requestId)
                refill.ReceiveResult(accepted);
        }
    }
}
