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
            static float audioLevel = 0.5f;

            [HarmonyPatch(typeof(CharacterVoiceHandler), nameof(CharacterVoiceHandler.Update))]
            private static void Prefix(CharacterVoiceHandler __instance, out PlayerConsciousnessManager.ConsciousState __state)
            {
                Character character = __instance.m_character;
                audioLevel = __instance.audioLevel;

                __state = PlayerConsciousnessManager.UpdateAndGet(character);
                bool canTalk = !ConfigurationManager.EnableQuietTime || (Time.time - __state.startTime >= ConfigurationManager.QuietTimeDuration);

                if (canTalk && !character.data.dead && character.data.fullyConscious&& (character.data.passedOut || character.data.fullyPassedOut))
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
            private static void Postfix(CharacterVoiceHandler __instance, PlayerConsciousnessManager.ConsciousState __state)
            {
                // restore unconscious state
                __instance.m_character.data.passedOut = __state.passedOut;
                __instance.m_character.data.fullyPassedOut = __state.fullyPassedOut;

                // restore audio level
                __instance.audioLevel = audioLevel;
            }
        }
    }
}
