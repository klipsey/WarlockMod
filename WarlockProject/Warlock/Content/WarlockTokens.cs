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

            string desc = "Warlock <color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Combine <color=#981e33>Crimson Mana</color> and <color=#b97f8a>Meta Magic</color> together for even stronger effects." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Eldritch Surge is a great way to move around the map but remember that it's your only damaging skill!" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Hex is a great way to give utility to your damaging skills." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > You can build up infinite <color=#981e33>Crimson Mana</color>, so saving it to unleash a massive barrage of abilities can be extremely powerful." + Environment.NewLine + Environment.NewLine;

            string lore = "Warlock";
            string outro = "..and so he left, damning souls in his wake.";
            string outroFailure = "..and so he vanished, just another tool for his patron.";
            
            Language.Add(prefix + "NAME", "Warlock");
            Language.Add(prefix + "DESCRIPTION", desc);
            Language.Add(prefix + "SUBTITLE", "Curious Mage");
            Language.Add(prefix + "LORE", lore);
            Language.Add(prefix + "OUTRO_FLAVOR", outro);
            Language.Add(prefix + "OUTRO_FAILURE", outroFailure);

            #region Skins
            Language.Add(prefix + "MASTERY_SKIN_NAME", "Alternate");
            #endregion

            #region Passive
            Language.Add(prefix + "PASSIVE_NAME", "Crimson Mana");
            Language.Add(prefix + "PASSIVE_DESCRIPTION", $"<color=#981e33>Warlock</color> can empower or restore certain skills using <color=#981e33>Crimson Mana</color>. Gain a stack of <color=#981e33>Crimson Mana</color> after {WarlockConfig.KillsPerCrimsonMana} {(WarlockConfig.KillsPerCrimsonMana == 1 ? "kill" : "kills")}.");
            #endregion

            #region Primary
            Language.Add(prefix + "PRIMARY_SURGE_NAME", "Eldritch Surge");
            Language.Add(prefix + "PRIMARY_SURGE_DESCRIPTION", $"Fire a <style=cIsUtility>piercing</style> beam for <style=cIsDamage>{WarlockConfig.CrimsonSurgeDamage * 100f:0.###}% damage</style>. <color=#981e33>\nCRIMSON MANA: Increased fire rate for {WarlockConfig.PrimaryEmpowerDuration:0.###} seconds.</color> " +
                $"<color=#b97f8a>\nMETA MAGIC: Gain an additional shot per Meta Magic stack.</color>");
            Language.Add(prefix + "PRIMARY_EMPOWER1_NAME", "Empower Eldritch Surge");
            Language.Add(prefix + "PRIMARY_EMPOWER1_DESCRIPTION", $"Spend <color=#981e33>1 Crimson Mana</color> to empower Eldritch Surge for <style=cIsUtility>{WarlockConfig.PrimaryEmpowerDuration:0.###} seconds</style>, reducing its charge and firing times by <style=cIsUtility>15%</style>.");
            #endregion

            #region Secondary
            Language.Add(prefix + "SECONDARY_HEX_NAME", "Hex");
            Language.Add(prefix + "SECONDARY_HEX_DESCRIPTION", $"Curse an enemy for {WarlockConfig.HexDuration:0.###} seconds. Skills deal <style=cIsDamage>{WarlockConfig.HexDamage * 100f:0.###}% bonus damage</style> to this enemy per stack.<color=#981e33> \nCRIMSON MANA: Bonus damage becomes AOE for {WarlockConfig.EmpoweredHexDamage * 100f:0.###}% of the triggering hit's damage per stack.</color>" +
                $"<color=#b97f8a>\nMETA MAGIC: Apply a {WarlockConfig.BleedDuration:0.###}-second stack of bleed per stack of Meta Magic.</color>");
            Language.Add(prefix + "SECONDARY_EMPOWER_NAME", "Empower Hex");
            Language.Add(prefix + "SECONDARY_EMPOWER_DESCRIPTION", "Spend <color=#981e33>1 Crimson Mana</color> to <style=cIsUtility>restore all Hex charges</style> and gain empowered casts equal to half its maximum charges, rounded down. Empowered Hex turns its bonus damage into an <style=cIsDamage>explosion around the cursed enemy</style>.");
            #endregion

            #region Utility 
            Language.Add(prefix + "UTILITY_BLOOD_DASH_NAME", "Blood Dash");
            Language.Add(prefix + "UTILITY_BLOOD_DASH_DESCRIPTION", $"<style=cIsDamage>Stunning</style>. Deal <style=cIsDamage>{WarlockConfig.BloodDashDamage * 100f:0.###}% damage</style>, then <style=cIsUtility>disappear</style> and <style=cIsUtility>teleport</style> a short distance.<color=#981e33>\nCRIMSON MANA: Deals damage at both the end and the start for {WarlockConfig.UtilityEmpowerDuration:0.###} seconds.</color> " +
                $"<color=#b97f8a>\nMETA MAGIC: Reset all stocks when depleted at the cost of a Meta Magic stack.</color>");
            Language.Add(prefix + "UTILITY_EMPOWER_NAME", "Empower Blood Dash");
            Language.Add(prefix + "UTILITY_EMPOWER_DESCRIPTION", $"Spend <color=#981e33>1 Crimson Mana</color> to <style=cIsUtility>restore all Blood Dash charges</style> and empower it for <style=cIsUtility>{WarlockConfig.UtilityEmpowerDuration:0.###} seconds</style>. Each dash deals <style=cIsDamage>stunning damage at both departure and arrival</style>.");
            #endregion

            #region Special
            Language.Add(prefix + "SPECIAL_RITUAL_NAME", "Ritual");
            string ritualDescription = "Open a menu allowing you to restore the stocks of a selected skill additionally empowering it. Recast Ritual to gain a stack of Meta Magic at the cost of 1 Crimson Mana.";
            Language.Add(prefix + "SPECIAL_RITUAL_DESCRIPTION", ritualDescription);
            Language.Add(prefix + "SPECIAL_EMPOWER_NAME", "Gain Meta Magic");
            Language.Add(prefix + "SPECIAL_EMPOWER_DESCRIPTION", "Spend <color=#981e33>1 Crimson Mana</color> to gain <color=#b97f8a>1 stack of Meta Magic</color>. Each stack adds a shot to your next Eldritch Surge or adds bleed to your next Hex. Blood Dash consumes one stack to restore its charges when depleted.");
            Language.Add(prefix + "SPECIAL_SCEPTER_RITUAL_NAME", "Ritual (Scepter)");
            Language.Add(prefix + "SPECIAL_SCEPTER_RITUAL_DESCRIPTION", ritualDescription);

     
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