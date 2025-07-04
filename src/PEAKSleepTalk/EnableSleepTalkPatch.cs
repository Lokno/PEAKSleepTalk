using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TextCore.Text;
using static PEAKSleepTalk.EnableSleepTalkPatch.PlayerConsciousnessManager;

namespace PEAKSleepTalk
{
    internal class EnableSleepTalkPatch
    {
        const float cooldownTime = 8f;
        const float audioLevelReductionFactor = 0.2f;
        static float audioLevel = 0.5f;

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
                ConsciousState state = PlayerConsciousnessManager.UpdateAndGet(__instance.character);
                bool canTalk = Time.time - state.startTime >= cooldownTime;
                __state = __instance.character.data.passedOut;

                if (canTalk && (__instance.character.data.passedOut || __instance.character.data.fullyPassedOut))
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
            [HarmonyPatch(typeof(CharacterVoiceHandler), nameof(CharacterVoiceHandler.Update))]
            private static void Prefix(CharacterVoiceHandler __instance, out ConsciousState __state)
            {
                FieldInfo CharacterField = AccessTools.Field(typeof(CharacterVoiceHandler), "m_character");
                Character character = (Character)CharacterField.GetValue(__instance);

                __state = PlayerConsciousnessManager.UpdateAndGet(character);
                bool canTalk = Time.time - __state.startTime >= cooldownTime;

                if(canTalk && (character.data.passedOut || character.data.fullyPassedOut))
                {
                    character.data.passedOut = false;
                    character.data.fullyPassedOut = false;

                    FieldInfo audioLevelField = AccessTools.Field(typeof(CharacterVoiceHandler), "audioLevel");
                    audioLevel = (float)audioLevelField.GetValue(__instance);

                    // reduce volume for passed out characters
                    float newAudioLevel = audioLevel * audioLevelReductionFactor;
                    audioLevelField.SetValue(__instance, newAudioLevel);
                }
            }

            [HarmonyPatch(typeof(CharacterVoiceHandler), nameof(CharacterVoiceHandler.Update))]
            private static void Postfix(CharacterVoiceHandler __instance, ConsciousState __state)
            {
                // restore unconscious state
                FieldInfo CharacterField = AccessTools.Field(typeof(CharacterVoiceHandler), "m_character");
                Character character = (Character)CharacterField.GetValue(__instance);
                character.data.passedOut = __state.passedOut;
                character.data.fullyPassedOut = __state.fullyPassedOut;

                if (character.data.passedOut || character.data.fullyPassedOut)
                {
                    // restore audio level
                    FieldInfo audioLevelField = AccessTools.Field(typeof(CharacterVoiceHandler), "audioLevel");
                    audioLevelField.SetValue(__instance, audioLevel);
                }
            }
        }
    }
}
