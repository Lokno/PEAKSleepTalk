using BepInEx.Configuration;
using BepInEx;
using System;

namespace PEAKSleepTalk
{
    public class ConfigurationManager
    {
        private static ConfigEntry<bool> _configEnableReduceVolume = null!;
        private static ConfigEntry<float> _configReductionFactor = null!;
        private static ConfigEntry<bool> _configEnableQuietTime = null!;
        private static ConfigEntry<float> _configQuietTime = null!;

        public static bool ReduceVolume => _configEnableReduceVolume.Value;
        public static float VolumeReductionFactor => Math.Clamp(_configReductionFactor.Value, 0.0f, 1.0f);
        public static bool EnableQuietTime => _configEnableQuietTime.Value;
        public static float QuietTimeDuration => _configQuietTime.Value;

        public static void LoadConfig(BaseUnityPlugin plugin)
        {
            _configEnableReduceVolume = plugin.Config.Bind<bool>
            (
                "General",
                "ReduceVolume",
                true,
                "Enable volume reduction of unconscious players"
            );

            _configReductionFactor = plugin.Config.Bind<float>
            (
                "General",
                "VolumeReductionFactor",
                0.3f,
                "The factor by which the volume is reduced for unconscious players (0.0 to 1.0)"
            );

            _configEnableQuietTime = plugin.Config.Bind<bool>
            (
                "General",
                "QuietTime",
                true,
                "Enable a quiet time before unconscious players can speak again"
            );

            _configQuietTime = plugin.Config.Bind<float>
            (
                "General",
                "QuietTimeDuration",
                8.0f,
                "The time in seconds before unconscious players can speak again"
            );
        }
    }
}
