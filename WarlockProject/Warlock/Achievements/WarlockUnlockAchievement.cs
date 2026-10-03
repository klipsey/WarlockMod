using RoR2;
using RoR2.Achievements;
using System.Runtime.CompilerServices;
using UnityEngine.Networking;

namespace WarlockMod.Warlock.Achievements
{
    [RegisterAchievement(identifier, unlockableIdentifier, null, 3, typeof(WarlockUnlockServerAchievement))]
    public class WarlockUnlockAchievement : BaseAchievement
    {
        public const string identifier = WarlockSurvivor.WARLOCK_PREFIX + "unlockAchievement";
        public const string unlockableIdentifier = WarlockSurvivor.WARLOCK_PREFIX + "unlockUnlockable";
        private const float killTimeLimit = 5f;

        private sealed class FirstDamageTime
        {
            public readonly Run.FixedTimeStamp time = Run.FixedTimeStamp.now;
        }

        private static ConditionalWeakTable<HealthComponent, FirstDamageTime> firstDamageTimes =
            new ConditionalWeakTable<HealthComponent, FirstDamageTime>();

        internal static void InstallDamageTracking()
        {
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
            Run.onRunDestroyGlobal += ClearDamageTracking;
        }

        internal static void UninstallDamageTracking()
        {
            GlobalEventManager.onServerDamageDealt -= OnServerDamageDealt;
            Run.onRunDestroyGlobal -= ClearDamageTracking;
            ClearDamageTracking(null);
        }

        private static void ClearDamageTracking(Run run)
        {
            firstDamageTimes = new ConditionalWeakTable<HealthComponent, FirstDamageTime>();
        }

        private static void OnServerDamageDealt(DamageReport damageReport)
        {
            if (!NetworkServer.active || !Run.instance || !damageReport.victim || !damageReport.victimBody ||
                damageReport.victimBodyIndex != BodyCatalog.FindBodyIndex("ImpBossBody") ||
                damageReport.damageInfo.rejected || damageReport.damageDealt <= 0f) return;

            if (!firstDamageTimes.TryGetValue(damageReport.victim, out _))
                firstDamageTimes.Add(damageReport.victim, new FirstDamageTime());
        }

        public override void OnInstall()
        {
            base.OnInstall();
            SetServerTracked(true);
        }

        public class WarlockUnlockServerAchievement : BaseServerAchievement
        {
            private BodyIndex impBossBodyIndex;

            public override void OnInstall()
            {
                base.OnInstall();
                impBossBodyIndex = BodyCatalog.FindBodyIndex("ImpBossBody");
                GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeath;
            }

            public override void OnUninstall()
            {
                GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeath;
                base.OnUninstall();
            }

            private void OnCharacterDeath(DamageReport damageReport)
            {
                if (!damageReport.victim || !damageReport.victimBody ||
                    damageReport.victimBodyIndex != impBossBodyIndex ||
                    damageReport.victimTeamIndex == TeamIndex.Player ||
                    !firstDamageTimes.TryGetValue(damageReport.victim, out var firstDamageTime)) return;

                var playerMaster = networkUser.master;
                if (!playerMaster ||
                    (damageReport.attackerMaster != playerMaster && damageReport.attackerOwnerMaster != playerMaster)) return;

                float elapsed = firstDamageTime.time.timeSince;
                if (elapsed >= 0f && elapsed <= killTimeLimit)
                    Grant();
            }
        }
    }
}
