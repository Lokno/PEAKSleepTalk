using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace PEAKSleepTalk
{
    internal class EnableSleepTalkPatch
    {
        public static class PlayerConsciousnessManager
        {
            public class ConsciousState
            {
                public bool passedOut = false;
                public bool fullyPassedOut = false;
                public float startTime = 0f;
            }

            static Dictionary<Character, ConsciousState> states = new Dictionary<Character, ConsciousState> { };

            public static ConsciousState Create(Character character)
            {
                ConsciousState newState = new ConsciousState
                {
                    passedOut = character.data.passedOut,
                    fullyPassedOut = character.data.fullyPassedOut,
                    startTime = Time.time
                };
                return newState;
            }

            public static ConsciousState UpdateAndGet(Character character)
            {
                if (states.TryGetValue(character, out ConsciousState state))
                {
                    if (state.passedOut != character.data.passedOut || state.fullyPassedOut != character.data.fullyPassedOut)
                    {
                        state.passedOut = character.data.passedOut;
                        state.fullyPassedOut = character.data.fullyPassedOut;
                        state.startTime = Time.time;
                    }
                    return state;
                }
                else
                {
                    ConsciousState newState = Create(character);
                    states.Add(character, newState);
                    return newState;
                }
            }
        }

        [HarmonyPatch(typeof(AnimatedMouth))]
        public class AnimatedMouthPatch
        {
            [HarmonyPatch(typeof(AnimatedMouth), nameof(AnimatedMouth.ProcessMicData))]
            private static void Prefix(AnimatedMouth __instance, out bool __state)
            {
                PlayerConsciousnessManager.ConsciousState state = PlayerConsciousnessManager.UpdateAndGet(__instance.character);
                bool canTalk = !ConfigurationManager.EnableQuietTime || (Time.time - state.startTime >= ConfigurationManager.QuietTimeDuration);
                __state = __instance.character.data.passedOut;

                if (canTalk && !__instance.character.data.dead && (__instance.character.data.passedOut || __instance.character.data.fullyPassedOut))
                {    
                    __instance.character.data.passedOut = false;
                }
            }

            [HarmonyPatch(typeof(AnimatedMouth), nameof(AnimatedMouth.ProcessMicData))]
            private static void Postfix(AnimatedMouth __instance, bool __state)
            {
                __instance.character.data.passedOut = __state;
            }
        }

        [HarmonyPatch(typeof(CharacterVoiceHandler))]
        public class CharacterVoicePatch
        {
            public class StoreData
            {
                public PlayerConsciousnessManager.ConsciousState state = null!;
                public float audioLevel = 0.5f;
            }

            [HarmonyPatch(typeof(CharacterVoiceHandler), nameof(CharacterVoiceHandler.Update))]
            private static void Prefix(CharacterVoiceHandler __instance, out StoreData __state)
            {
                Character character = __instance.m_character;
                float audioLevel = __instance.audioLevel;

                __state = new StoreData
                {
                    state = PlayerConsciousnessManager.UpdateAndGet(character),
                    audioLevel = audioLevel
                };

                bool canTalk = !ConfigurationManager.EnableQuietTime || (Time.time - __state.state.startTime >= ConfigurationManager.QuietTimeDuration);

                if (canTalk && !character.data.dead && (character.data.passedOut || character.data.fullyPassedOut))
                {
                    character.data.passedOut = false;
                    character.data.fullyPassedOut = false;

                    if(ConfigurationManager.ReduceVolume)
                    {
                        __instance.audioLevel = audioLevel * ConfigurationManager.VolumeReductionFactor;
                    }
                }
            }

            [HarmonyPatch(typeof(CharacterVoiceHandler), nameof(CharacterVoiceHandler.Update))]
            private static void Postfix(CharacterVoiceHandler __instance, StoreData __state)
            {
                // restore unconscious state
                __instance.m_character.data.passedOut = __state.state.passedOut;
                __instance.m_character.data.fullyPassedOut = __state.state.fullyPassedOut;

                // restore audio level
                __instance.audioLevel = __state.audioLevel;
            }
        }

        [HarmonyPatch(typeof(MainCameraMovement))]
        public class PassedOutSpectatePatch
        {
            [HarmonyPatch(nameof(MainCameraMovement.HandleSpecSelection))]
            private static void Prefix(MainCameraMovement __instance, out float __state)
            {
                __state = __instance.sinceSwitch;
                if ((bool)Character.localCharacter && !Character.localCharacter.data.dead && !ConfigurationManager.AllowSpectate)
                {
                    __instance.sinceSwitch = 0.0f;
                }
            }
            [HarmonyPatch(nameof(MainCameraMovement.HandleSpecSelection))]
            private static void Postfix(MainCameraMovement __instance, float __state)
            {
                __instance.sinceSwitch = __state;
            }
        }
    }
}
