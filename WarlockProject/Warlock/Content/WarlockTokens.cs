﻿using System;
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
            Language.Add(prefix + "PASSIVE_NAME", "Meta Magic");
            Language.Add(prefix + "PASSIVE_DESCRIPTION", $"<color=#9B3737>Warlock</color> must restore skills using blood magic to cast abilities other than his primary. Gain a stack of blood magic after 3 kills.");
            #endregion

            #region Primary
            Language.Add(prefix + "PRIMARY_SURGE_NAME", "Crimson Surge");
            Language.Add(prefix + "PRIMARY_SURGE_DESCRIPTION", $"Fire a <style=cIsUtility>piercing</style> beam for <style=cIsDamage>{WarlockStaticValues.crimsonSurgeDamageCoefficient * 100f}% damage</style>. <color=#9B3737>\nMETAMAGIC: Increased fire rate and fire twice per shot.</color>");
            #endregion

            #region Secondary
            Language.Add(prefix + "SECONDARY_HEX_NAME", "Hex");
            Language.Add(prefix + "SECONDARY_HEX_DESCRIPTION", $"Curse an enemy for {WarlockStaticValues.hexDuration} seconds. Skills deal bonus damage to this enemy.<color=#9B3737> \nMETAMAGIC: Bonus damage becomes AOE.</color>");
            #endregion

            #region Utility 
            Language.Add(prefix + "UTILITY_BLOOD_DASH_NAME", "Blood Dash");
            Language.Add(prefix + "UTILITY_BLOOD_DASH_DESCRIPTION", $"<style=cIsDamage>Stunning</style>. Deal <style=cIsDamage>150% damage</style>, then <style=cIsUtility>disappear</style> and <style=cIsUtility>teleport</style> a short distance. <color=#9B3737>\nMETAMAGIC: Reset on kill and deals damage at both the end and the start</color>.");

            #endregion

            #region Special
            Language.Add(prefix + "SPECIAL_RITUAL_NAME", "Ritual");
            Language.Add(prefix + "SPECIAL_RITUAL_DESCRIPTION", $"Open a menu allowing you to restore the stocks of a certain skill and empowering it. Press again to cancel.");

            Language.Add(prefix + "SPECIAL_SCEPTER_RITUAL_NAME", "Ritual2");
            Language.Add(prefix + "SPECIAL_SCEPTER_RITUAL_DESCRIPTION", $"Target a <color=#9B3737>Guilty</color> enemy and force them to fight you for 10 seconds. Your primary can no longer hit you but will continuously add <color=#9B3737>Guilty's</color> buff to you. " +
                $"During this time all external <style=cIsDamage>damage</style> is negated but all your <style=cIsDamage>damage</style> dealt to others is <style=cIsUtility>negated</style>." + Tokens.ScepterDescription("Convict can target enemies without Guilty and damage you deal is no longer negated but is reduced by 75%."));
            #endregion

            #region Achievements
            Language.Add(Tokens.GetAchievementNameToken(WarlockMasterAchievement.identifier), "Interrogator: Mastery");
            Language.Add(Tokens.GetAchievementDescriptionToken(WarlockMasterAchievement.identifier), "As Interrogator, beat the game or obliterate on Monsoon.");
            /*
            Language.Add(Tokens.GetAchievementNameToken(SpyUnlockAchievement.identifier), "Dressed to Kill");
            Language.Add(Tokens.GetAchievementDescriptionToken(SpyUnlockAchievement.identifier), "Get a Backstab.");
            */
            #endregion

            #endregion
        }
    }
}