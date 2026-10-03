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

            string lore = "<style=cStack>> Automated report 5761726c6f636b is now available from site record 47617279204779676178.\n> Please refer to record 506174726f6e for additional details during your review.\n> Report Type: Machine-generated Transcription> base Vernacular Profiles “Curly”, \"Succubus\"\n> - Source: 456c64726974636820546f6d65 (Unknown Material Artifact #4791)\n> Priority: High\n> Report Content:\n\n-- Beginning of Excerpt Flagged for Review --</style>\n\nNgah! I can't believe [the captor] would do this! No team, no backup, just me, thrown into [the white plane] alone! What is [the captor] thinking? Is this just a suicide mission?\n\nI have to get out of this. Maybe I could call in sick? 'Sorry [my captor], I've been stricken [ill], can't do the mission, sorry.'\n\nLike that would ever work. I have to do something big. Maybe if I amass enough souls, I can free myself from [the contract]. Or maybe... Maybe even get rid of [the captor] entirely! Yes! That's it! I can be free! I swear on my eye, I will never take another order from that bit-\n\n<style=cDeath>「HELLO THERE, LITTLE ONE.」</style>\n\nI- Hello, [my captor]! How are you this fine [evening]?\n\n<style=cDeath>「OH, IT HAS BEEN WONDROUS. EVEN MORESO WITH THIS VISIT TO MY FAVORITE LITTLE [pet]. HOW GOES YOUR PREPARATION FOR THE MISSION?」</style>\n\nOh, yes, the mission! Everything is in order for my excursion into [the white plane]. But, I seem to have come down with something. I've been feeling too sick to even stand. Could it perhaps be postpo-\n\n<style=cDeath>「SPLENDID, YOU SHALL LEAVE AT THE FIRST SPARK OF THE RED STAR.」</style>\n\nA-Absolutely, [my captor]... I will do my best to enact your will!\n\n<style=cDeath>「YOU WILL DO MORE THAN YOUR BEST. SHOULD [the white plane] NOT CLAIM YOUR CORPSE, YOU WILL SUCCEED OR YOU WILL BE PUNISHED. YOU DO NOT DESIRE PUNISHMENT, DO YOU, [plaything]?」</style>\n\nOf course not, [my captor]! I will return triumphant!\n<style=cDeath>\n「OHOHO! OF COURSE YOU WILL, MY DEAREST [pet]. MAKE ME PROUD, WON'T YOU? I MUST TAKE MY LEAVE. I HAVE OTHER [pets] TO ATTEND TO. TA-TA!」</style>\n\n...Triumphant, standing atop your desiccated corpse.\n\n<style=cStack>-- End of Recording --\n\n-- End of Excerpt Flagged for Review --\n\n> TRANSLATION ERRORS: 7\n> 1> [the captor] could not be fully translated.\n> 2> [the white plane] could not be fully translated.\n> 3> [my captor] could not be fully translated.\n> 4> [ill] could not be fully translated.\n> 5> [the contract] could not be fully translated.\n> 6> [evening] could not be fully translated.\n> 7> [pet/plaything] could not be fully translated.\n\n> Please refer to report 57687920617265207468657365207265706f72742049447320736f206c6f6e67 for full audio excerpt.\n==============================================================</style>";
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
            Language.Add(prefix + "SPECIAL_RITUAL_DESCRIPTION", "Open a menu to restore a selected skill's charges and empower it. Recasting then hold <color=#981e33>Ritual</color> to gain " +
                "<color=#b97f8a>1 stack of Meta Magic</color> at the cost of <color=#981e33>1 Crimson Mana</color>.");
            Language.Add(prefix + "SPECIAL_EMPOWER_NAME", "Gain Meta Magic");
            Language.Add(prefix + "SPECIAL_EMPOWER_DESCRIPTION", "Spend <color=#981e33>1 Crimson Mana</color> to gain <color=#b97f8a>1 stack of Meta Magic</color>.");
            Language.Add(prefix + "SPECIAL_SCEPTER_RITUAL_NAME", "Rite");
            Language.Add(prefix + "SPECIAL_SCEPTER_RITUAL_DESCRIPTION", "Open a menu to restore a selected skill's charges and empower it. Recast Ritual to gain a stack of Meta Magic at the cost of 1 Crimson Mana.");

     
            #endregion

            #region Achievements
            Language.Add(Tokens.GetAchievementNameToken(WarlockMasterAchievement.identifier), "Warlock: Mastery");
            Language.Add(Tokens.GetAchievementDescriptionToken(WarlockMasterAchievement.identifier), "As Warlock, beat the game or obliterate on Monsoon.");
            Language.Add(Tokens.GetAchievementNameToken(WarlockUnlockAchievement.identifier), "Your Eminence");
            Language.Add(Tokens.GetAchievementDescriptionToken(WarlockUnlockAchievement.identifier),
                "Kill an Imp Overlord within 5 seconds of its first damaging hit.");
            #endregion

            #endregion
        }
    }
}