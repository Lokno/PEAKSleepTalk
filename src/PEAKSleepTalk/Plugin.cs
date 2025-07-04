using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace PEAKSleepTalk;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; } = null!;

    private void Awake()
    {
        Log = Logger;

        // Log our awake here so we can see it in LogOutput.log file
        Log.LogInfo($"Plugin {Name} is loaded: Allow passed out scouts to speak");

        var harmony = new Harmony(Id);
        harmony.PatchAll();
    }
}
