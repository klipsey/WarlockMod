using System;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using UnityEngine;

namespace WarlockMod.Modules
{
    public static class Config
    {
        public static ConfigFile MyConfig => WarlockPlugin.instance.Config;

        public static ConfigEntry<float> BindFloat(string section, string name, float defaultValue, float maximum, string description)
        {
            var entry = MyConfig.Bind(section, name, defaultValue, new ConfigDescription(
                description + " Restart required. Use matching settings on all multiplayer peers.",
                new AcceptableValueRange<float>(0f, maximum)));
            if (float.IsNaN(entry.Value) || float.IsInfinity(entry.Value))
            {
                Log.Warning($"Invalid value for {section}/{name}; restoring {defaultValue}.");
                entry.Value = defaultValue;
            }
            if (WarlockPlugin.riskOfOptionsInstalled) RegisterSlider(entry, maximum);
            return entry;
        }

        public static ConfigEntry<int> BindInt(string section, string name, int defaultValue, int minimum, int maximum, string description)
        {
            var entry = MyConfig.Bind(section, name, defaultValue, new ConfigDescription(
                description + " Restart required. Use matching settings on all multiplayer peers.",
                new AcceptableValueRange<int>(minimum, maximum)));
            if (WarlockPlugin.riskOfOptionsInstalled) RegisterIntSlider(entry, minimum, maximum);
            return entry;
        }

        public static ConfigEntry<bool> BindToggle(string section, string name, bool defaultValue, string description)
        {
            var entry = MyConfig.Bind(section, name, defaultValue, description + " Restart required.");
            if (WarlockPlugin.riskOfOptionsInstalled) RegisterToggle(entry);
            return entry;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void RegisterSlider(ConfigEntry<float> entry, float maximum)
        {
            ModSettingsManager.AddOption(new SliderOption(entry, new SliderConfig
            {
                min = 0f,
                max = maximum,
                formatString = "{0:0.###}",
                restartRequired = true
            }), WarlockPlugin.MODUID, WarlockPlugin.MODNAME);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void RegisterIntSlider(ConfigEntry<int> entry, int minimum, int maximum)
        {
            ModSettingsManager.AddOption(new IntSliderOption(entry, new IntSliderConfig
            {
                min = minimum,
                max = maximum,
                restartRequired = true
            }), WarlockPlugin.MODUID, WarlockPlugin.MODNAME);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void RegisterToggle(ConfigEntry<bool> entry)
        {
            ModSettingsManager.AddOption(new CheckBoxOption(entry, true), WarlockPlugin.MODUID, WarlockPlugin.MODNAME);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void InitializeOptions(Texture portrait)
        {
            ModSettingsManager.SetModDescription(
                "Warlock combat coefficients, durations, Crimson Mana and optional emotes. All settings are captured at startup and require a restart. " +
                "Gameplay settings must match on every multiplayer peer; they are not synchronized. Skill descriptions reflect the settings loaded at startup.",
                WarlockPlugin.MODUID, WarlockPlugin.MODNAME);
            if (!(portrait is Texture2D texture) || !texture)
            {
                Log.Error("Warlock portrait is missing; Risk of Options icon could not be set.");
                return;
            }
            ModSettingsManager.SetModIcon(Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)), WarlockPlugin.MODUID, WarlockPlugin.MODNAME);
        }
    }
}
