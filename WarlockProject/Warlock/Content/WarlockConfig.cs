using WarlockMod.Modules;

namespace WarlockMod.Warlock.Content
{
    public static class WarlockConfig
    {
        public static int KillsPerCrimsonMana { get; private set; }
        public static float CrimsonSurgeDamage { get; private set; }
        public static float CrimsonSurgeProc { get; private set; }
        public static float BloodDashDamage { get; private set; }
        public static float BloodDashProc { get; private set; }
        public static float HexDamage { get; private set; }
        public static float HexDuration { get; private set; }
        public static float EmpoweredHexDamage { get; private set; }
        public static float BleedDamageMultiplier { get; private set; }
        public static float BleedDuration { get; private set; }

        public static void Init()
        {
            KillsPerCrimsonMana = Stack("01 - Crimson Mana", "Kills per Crimson Mana", 2, "Number of kills or direct skill hits on bosses required to gain one stack of Crimson Mana. Boss killing blows count once.");
            CrimsonSurgeDamage = Damage("02 - Crimson Surge", "Damage coefficient", 4f, "Damage per shot, as a multiple of base damage.");
            CrimsonSurgeProc = Proc("02 - Crimson Surge", "Proc coefficient", 1f, "Proc coefficient per shot.");
            BloodDashDamage = Damage("03 - Blood Dash", "Damage coefficient", 2f, "Damage per Crimson Mana blast or Meta Magic arrival blast, as a multiple of base damage.");
            BloodDashProc = Proc("03 - Blood Dash", "Proc coefficient", 1f, "Proc coefficient of each dash blast.");
            HexDamage = Damage("04 - Hex", "Damage coefficient", 0.5f, "Bonus damage per Hex stack, as a multiple of the triggering hit's damage.");
            HexDuration = Duration("04 - Hex", "Duration", 7f, "Duration in seconds of normal Hex, empowered Hex and its metamagic bleed mark.");
            EmpoweredHexDamage = Damage("05 - Empowered Hex", "Damage coefficient", 0.5f, "Delayed blast damage per empowered Hex stack, as a multiple of the triggering hit's damage.");
            BleedDamageMultiplier = Damage("06 - Metamagic Bleed", "Damage multiplier", 0.2f,
                "Multiplies vanilla bleed damage per metamagic stack. Normal Hex also scales bleed by the triggering hit's proc coefficient. Bleed cannot proc items.");
            BleedDuration = Duration("06 - Metamagic Bleed", "Duration", 2f, "Duration in seconds of each non-proccing metamagic bleed stack.");
        }

        private static int Stack(string section, string name, int value, string description) =>
            Config.BindInt(section, name, value, 1, 100, description).Value;

        private static float Damage(string section, string name, float value, string description) =>
            Config.BindFloat(section, name, value, 100f, description).Value;

        private static float Proc(string section, string name, float value, string description) =>
            Config.BindFloat(section, name, value, 10f, description).Value;

        private static float Duration(string section, string name, float value, string description) =>
            Config.BindFloat(section, name, value, 300f, description).Value;
    }
}
