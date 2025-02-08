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

            ////uncomment this to spit out a lanuage file with all the above tokens that people can translate
            ////make sure you set Language.usingLanguageFolder and printingEnabled to true
            //Language.PrintOutput("Spy.txt");
            //todo guide
            ////refer to guide on how to build and distribute your mod with the proper folders
        }

        public static void AddWarlockTokens()
        {
            #region Interrogator
            string prefix = WarlockSurvivor.INTERROGATOR_PREFIX;

            string desc = "Interrogator relishes the pain of others. Don't have too much fun hurting your allies, or do...<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Punish the Guilty after they hit you to gain attack speed and move speed. No running from justice." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > If you need a quick and dirty Guilty buff, swing and hit yourself instead. The law applies to everyone!" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Falsify is a great way to spot the Guilty before they commit crimes. Unethical? What do you mean?" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Convict a Guilty target to make sure they are punished for their acts. Guilty until proven innocent after all." + Environment.NewLine + Environment.NewLine;

            string lore = "Insert goodguy lore here";
            string outro = "..and so he left, itching to enact more \"justice\".";
            string outroFailure = "..and so he vanished, punished for his crimes.";
            
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
            Language.Add(prefix + "PASSIVE_DESCRIPTION", $"<color=#9B3737>Warlock</color> can empower or restore certain skills using <color=#9B3737>Crimson Mana</color>. Gain a stack of <color=#9B3737>Crimson Mana</color> after 3 kills.");
            #endregion

            #region Primary
            Language.Add(prefix + "PRIMARY_SURGE_NAME", "Eldritch Surge");
            Language.Add(prefix + "PRIMARY_SURGE_DESCRIPTION", $"Fire a <style=cIsUtility>piercing</style> beam for <style=cIsDamage>{WarlockStaticValues.crimsonSurgeDamageCoefficient * 100f}% damage</style>. <color=#9B3737>\nCRIMSON MANA: Increased fire rate for 7 seconds.</color> " +
                $"<color=#FF0000>\nMETA MAGIC: Gain an additional shot per Meta Magic stack.</color>");
            #endregion

            #region Secondary
            Language.Add(prefix + "SECONDARY_HEX_NAME", "Hex");
            Language.Add(prefix + "SECONDARY_HEX_DESCRIPTION", $"Curse an enemy for {WarlockStaticValues.hexDuration} seconds. Skills deal bonus damage to this enemy per stack.<color=#9B3737> \nCRIMSON MANA: Bonus damage becomes AOE.</color>" +
                $"<color=#FF0000>\nMETA MAGIC: Apply a stack of bleed per stack of Meta Magic.</color>");
            #endregion

            #region Utility 
            Language.Add(prefix + "UTILITY_BLOOD_DASH_NAME", "Blood Dash");
            Language.Add(prefix + "UTILITY_BLOOD_DASH_DESCRIPTION", $"<style=cIsDamage>Stunning</style>. Deal <style=cIsDamage>150% damage</style>, then <style=cIsUtility>disappear</style> and <style=cIsUtility>teleport</style> a short distance.<color=#9B3737>\nCRIMSON MANA: Deals damage at both the end and the start for 7 seconds.</color> " +
                $"<color=#FF0000>\nMETA MAGIC: Reset all stocks when depleted at the cost of a Meta Magic stack.</color>");
            #endregion

            #region Special
            Language.Add(prefix + "SPECIAL_RITUAL_NAME", "Ritual");
            Language.Add(prefix + "SPECIAL_RITUAL_DESCRIPTION", $"Open a menu allowing you to restore the stocks of a selected skill additionally empowering it. Recast Ritual to gain a stack of Meta Magic at the cost of 1 Crimson Mana.");

     
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