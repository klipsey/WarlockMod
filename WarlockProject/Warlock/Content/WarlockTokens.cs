using System;
using WarlockMod.Modules;
using WarlockMod.Warlock;
using WarlockMod.Warlock.Achievements;
using UnityEngine.UIElements;

namespace WarlockMod.Warlock.Content
{
    public static class WarlockTokens
    {
        public static void Init()
        {
            AddWarlockTokens();
        }

        public static void AddWarlockTokens()
        {
            #region Warlock
            string prefix = WarlockSurvivor.WARLOCK_PREFIX;

            string desc = "The Warlock is a small but powerful survivor that restores and empowers abilities with the blood of his enemies. <color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Combine <color=#981e33>Crimson Mana</color> and <color=#b97f8a>Meta Magic</color> together for even stronger effects." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Eldritch Surge is a great way to move around the map, but remember that it's also your main damaging skill." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Hex is a great way to increase your overall damage output and AOE capabilities." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > You can build up infinite <color=#981e33>Crimson Mana</color> and <color=#b97f8a>Meta Magic</color>, so saving it for a massive barrage of abilities can be extremely powerful." + Environment.NewLine + Environment.NewLine;

            string lore = "Warlock";
            string outro = "..and so he left, damning souls in his wake.";
            string outroFailure = "..and so he vanished, just another tool for his patron.";
            
            Language.Add(prefix + "NAME", "Warlock");
            Language.Add(prefix + "DESCRIPTION", desc);
            Language.Add(prefix + "SUBTITLE", "Cursed Pawn");
            Language.Add(prefix + "LORE", lore);
            Language.Add(prefix + "OUTRO_FLAVOR", outro);
            Language.Add(prefix + "OUTRO_FAILURE", outroFailure);

            #region Skins
            Language.Add(prefix + "MASTERY_SKIN_NAME", "Alternate");
            #endregion

            #region Passive
            Language.Add(prefix + "PASSIVE_NAME", "Crimson Mana");
            Language.Add(prefix + "PASSIVE_DESCRIPTION", $"<color=#981e33>Warlock</color> can empower or restore certain skills using <color=#981e33>Crimson Mana</color>. " +
                $"Gain a stack of <color=#981e33>Crimson Mana</color> after <style=cIsDamage>{WarlockConfig.KillsPerCrimsonMana}</style> {(WarlockConfig.KillsPerCrimsonMana == 1 ? "kill" : "kills")}.");
            #endregion

            #region Primary
            Language.Add(prefix + "PRIMARY_SURGE_NAME", "Eldritch Surge");
            Language.Add(prefix + "PRIMARY_SURGE_DESCRIPTION", $"Fire a <style=cIsUtility>piercing</style> beam for <style=cIsDamage>{WarlockConfig.CrimsonSurgeDamage * 100f:0.###}% damage</style>. " +
                $"<color=#981e33>\nCRIMSON MANA: Next cast weakens and knocks enemies back.</color> " +
                $"<color=#b97f8a>\nMETA MAGIC: Hold to consume stacks: +1 shot each.</color>");
            Language.Add(prefix + "PRIMARY_EMPOWER1_NAME", "Empower Eldritch Surge");
            Language.Add(prefix + "PRIMARY_EMPOWER1_DESCRIPTION", "Spend <color=#981e33>1 Crimson Mana</color> to bank <style=cIsUtility>1 empowered Eldritch Surge cast</style>. " +
                "Every shot in that cast <style=cIsUtility>weakens and knocks enemies back</style>.");
            #endregion

            #region Secondary
            Language.Add(prefix + "SECONDARY_HEX_NAME", "Hex");
            Language.Add(prefix + "SECONDARY_HEX_DESCRIPTION", $"Curse an enemy for <style=cIsUtility>{WarlockConfig.HexDuration:0.###} seconds</style>. Skills deal <style=cIsDamage>{WarlockConfig.HexDamage * 100f:0.###}% bonus damage</style> to this enemy per stack." +
                $"<color=#981e33>\nCRIMSON MANA: Bonus damage becomes AOE: {WarlockConfig.EmpoweredHexDamage * 100f:0.###}% of hit damage per stack.</color>" +
                $"<color=#b97f8a>\nMETA MAGIC: Hold to consume stacks: +1 curse and {WarlockConfig.BleedDuration:0.###}s on-hit bleed each.</color>");
            Language.Add(prefix + "SECONDARY_EMPOWER_NAME", "Empower Hex");
            Language.Add(prefix + "SECONDARY_EMPOWER_DESCRIPTION", "Spend <color=#981e33>1 Crimson Mana</color> to <style=cIsUtility>restore all Hex charges</style> and gain empowered casts equal to half its maximum charges, " +
                "rounded down. Empowered Hex turns its bonus damage into an <style=cIsDamage>explosion around the cursed enemy</style>.");
            #endregion

            #region Utility 
            Language.Add(prefix + "UTILITY_BLOOD_DASH_NAME", "Blood Dash");
            Language.Add(prefix + "UTILITY_BLOOD_DASH_DESCRIPTION", $"<style=cIsUtility>Disappear</style> and <style=cIsUtility>teleport</style> a short distance.<color=#981e33>\nCRIMSON MANA: Next cast: {WarlockConfig.BloodDashDamage * 100f:0.###}% stunning damage at both ends.</color> " +
                $"<color=#b97f8a>\nMETA MAGIC: Hold to consume stacks: longer blink, {WarlockConfig.BloodDashDamage * 100f:0.###}% arrival damage +100% each.</color>");
            Language.Add(prefix + "UTILITY_EMPOWER_NAME", "Empower Blood Dash");
            Language.Add(prefix + "UTILITY_EMPOWER_DESCRIPTION", "Spend <color=#981e33>1 Crimson Mana</color> to <style=cIsUtility>restore all Blood Dash charges</style> and bank <style=cIsUtility>1 empowered cast</style>. " +
                $"That cast deals <style=cIsDamage>{WarlockConfig.BloodDashDamage * 100f:0.###}% stunning damage at both departure and arrival</style>.");
            #endregion

            #region Special
            Language.Add(prefix + "SPECIAL_RITUAL_NAME", "Ritual");
            Language.Add(prefix + "SPECIAL_RITUAL_DESCRIPTION", "Open a menu to restore a selected skill's charges and empower it. Recast <color=#981e33>Ritual</color> to gain " +
                "<color=#b97f8a>1 stack of Meta Magic</color> at the cost of <color=#981e33>1 Crimson Mana</color>.");
            Language.Add(prefix + "SPECIAL_EMPOWER_NAME", "Gain Meta Magic");
            Language.Add(prefix + "SPECIAL_EMPOWER_DESCRIPTION", "Spend <color=#981e33>1 Crimson Mana</color> to gain <color=#b97f8a>1 stack of Meta Magic</color>.");
            Language.Add(prefix + "SPECIAL_SCEPTER_RITUAL_NAME", "Rite");
            Language.Add(prefix + "SPECIAL_SCEPTER_RITUAL_DESCRIPTION", "Open a menu to restore a selected skill's charges and empower it. Recast Ritual to gain a stack of Meta Magic at the cost of 1 Crimson Mana.");

     
            #endregion

            #region Achievements
            Language.Add(Tokens.GetAchievementNameToken(WarlockMasterAchievement.identifier), "Warlock: Mastery");
            Language.Add(Tokens.GetAchievementDescriptionToken(WarlockMasterAchievement.identifier), "As Warlock, beat the game or obliterate on Monsoon.");
            /*
            Language.Add(Tokens.GetAchievementNameToken(SpyUnlockAchievement.identifier), "Dressed to Kill");
            Language.Add(Tokens.GetAchievementDescriptionToken(SpyUnlockAchievement.identifier), "Get a Backstab.");
            */
            #endregion

            #endregion
        }
    }
}