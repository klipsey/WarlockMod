using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace WarlockMod.Warlock.Content
{
    internal static class Dots
    {
        internal static void InflictBleed(GameObject victim, GameObject attacker, int stacks, float damageMultiplier = 1f)
        {
            if (!NetworkServer.active || !victim || !attacker || stacks <= 0 || damageMultiplier <= 0f ||
                WarlockConfig.BleedDamageMultiplier <= 0f) return;
            for (int i = 0; i < stacks; i++)
                DotController.InflictDot(victim, attacker, null, DotController.DotIndex.Bleed, WarlockConfig.BleedDuration,
                    damageMultiplier * WarlockConfig.BleedDamageMultiplier);
        }
    }
}
