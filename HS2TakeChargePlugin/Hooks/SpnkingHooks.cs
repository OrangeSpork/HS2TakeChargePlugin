using AIChara;
using DG.Tweening;
using HarmonyLib;
using Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HS2TakeChargePlugin.Hooks
{
    public static class SpnkingHooks
    {

        [HarmonyPostfix, HarmonyPatch(typeof(Spnking), "AfterWaitingAnimation")]
        static void SpankingAfterWaitingAnimationAutoSelector()
        {
            bool changeAction = Singleton<HSceneManager>.Instance.Hscene.ctrlAuto.IsChangeActionAtRestart();
            if (changeAction)
            {
                Singleton<HSceneFlagCtrl>.Instance.isAutoActionChange = true;
                Singleton<HSceneManager>.Instance.Hscene.ctrlAuto.Reset();
                Singleton<HSceneManager>.Instance.Hscene.ctrlFlag.speed = 0f;
                Singleton<HSceneManager>.Instance.Hscene.ctrlFlag.loopType = 0;
                Singleton<HSceneManager>.Instance.Hscene.ctrlFlag.isNotCtrl = false;
            }
        }


        private static MethodInfo playMethodInfo = AccessTools.Method(typeof(Spnking), "setPlay");
        private static FieldInfo voiceFieldInfo = AccessTools.Field(typeof(Spnking), "voice");
        private static FieldInfo spriteFieldInfo = AccessTools.Field(typeof(Spnking), "sprite");
        private static FieldInfo upFeelFieldInfo = AccessTools.Field(typeof(Spnking), "upFeel");
        private static FieldInfo chaFemalesFieldInfo = AccessTools.Field(typeof(Spnking), "chaFemales");
        private static FieldInfo timeFeelUpFieldInfo = AccessTools.Field(typeof(Spnking), "timeFeelUp");
        private static FieldInfo backupFieldInfo = AccessTools.Field(typeof(Spnking), "backupFeel");
        private static FieldInfo backupFeelFieldInfo = AccessTools.Field(typeof(Spnking), "backupFeelFlag");
        private static FieldInfo randVoicePlaysFieldInfo = AccessTools.Field(typeof(Spnking), "randVoicePlays");
        private static FieldInfo isAddFieldInfo = AccessTools.Field(typeof(Spnking), "isAddFeel");


        [HarmonyPrefix, HarmonyPatch(typeof(Spnking), "SpankingProc")]
        static bool SpankingAutoProc(int _state, Spnking __instance)
        {

            HSceneFlagCtrl ctrlFlag = Singleton<HSceneFlagCtrl>.Instance;
            if (ctrlFlag.initiative == 0)
            {
                return true;
            }
            HVoiceCtrl voice = (HVoiceCtrl)voiceFieldInfo.GetValue(__instance);
            HSceneSprite sprite = (HSceneSprite)spriteFieldInfo.GetValue(__instance);
            ChaControl[] chaFemales = (ChaControl[])chaFemalesFieldInfo.GetValue(__instance);
            ShuffleRand[] randVoicePlays = (ShuffleRand[])randVoicePlaysFieldInfo.GetValue(__instance);

            if (!ctrlFlag.stopFeelFemale)
            {
                ctrlFlag.feel_f = Mathf.Clamp01(ctrlFlag.feel_f - ctrlFlag.guageDecreaseRate * Time.deltaTime);
            }
            if (_state == 1 && ctrlFlag.feel_f < 0.5f)
            {
                playMethodInfo.Invoke(__instance, new object[] { "WIdle", false });
                voice.HouchiTime = 0f;
                return true;
            }
            if (!GeneralHooks.SpankTimer.IsTime())
            {
                return false;
            }
            else
            {
                GeneralHooks.SpankTimer.Reset();
            }
            if (voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice || voice.nowVoices[0].state == HVoiceCtrl.VoiceKind.startVoice)
            {
                Voice.Stop(ctrlFlag.voice.voiceTrs[0]);
                voice.ResetVoice();
            }

            string scene = "D_Action";
            switch (_state)
            {
                case 1:
                    scene = "SAction";
                    break;
                case 0:
                    scene = "WAction";
                    break;
            }

            playMethodInfo.Invoke(__instance, new object[] { scene, false });

            upFeelFieldInfo.SetValue(__instance, true);
            float value = Mathf.Clamp01(chaFemales[0].siriAkaRate + ctrlFlag.siriakaAddRate);
            chaFemales[0].ChangeSiriAkaRate(value);
            timeFeelUpFieldInfo.SetValue(__instance, 0f);
            backupFieldInfo.SetValue(__instance, ctrlFlag.feel_f);
            backupFeelFieldInfo.SetValue(__instance, ctrlFlag.feelPain);
            ctrlFlag.isNotCtrl = false;
            if (randVoicePlays[0].Get() == 0)
            {
                ctrlFlag.voice.playVoices[0] = true;
            }
            ctrlFlag.voice.playShorts[0] = 0;

            bool isAddFeel = (bool)isAddFieldInfo.GetValue(__instance);
            if (!isAddFeel && ctrlFlag.feel_f >= 0.70f)
            {
                SpankingAfterWaitingAnimationAutoSelector();
            }

            return false;
        }


        private static FieldInfo chaMalesFieldInfo = AccessTools.Field(typeof(Spnking), "chaMales");
        private static FieldInfo itemFieldInfo = AccessTools.Field(typeof(Spnking), "item");
        [HarmonyPostfix, HarmonyPatch(typeof(Spnking), "setAnimationParamater")]
        static void SpnkingSpeedGambit(Spnking __instance)
        {
            if (!HS2TakeChargePlugin.Instance.AnimationOverrideActive())
            {
                return;
            }

            ChaControl[] chaFemales = (ChaControl[])chaFemalesFieldInfo.GetValue(__instance);
            ChaControl[] chaMales = (ChaControl[])chaMalesFieldInfo.GetValue(__instance);
            HItemCtrl item = (HItemCtrl)itemFieldInfo.GetValue(__instance);


            if (chaFemales[0].visibleAll && chaFemales[0].objTop != null)
            {
                chaFemales[0].setAnimatorParamFloat("speed", AnimationStatus.FemaleSpeed);
                if (AnimationStatus.FemaleOffset != 0)
                    chaFemales[0].animBody.Play(AnimationStatus.PlayingAnimation, 0, (chaMales[0].animBody.GetCurrentAnimatorStateInfo(0).normalizedTime + AnimationStatus.FemaleOffset));
            }
            if (chaMales[0].objBodyBone != null)
            {
                chaMales[0].setAnimatorParamFloat("speed", AnimationStatus.MaleSpeed);
            }
            if (item.GetItem() != null)
            {
                item.setAnimatorParamFloat("speed", AnimationStatus.FemaleSpeed);
            }
  //          HS2TakeChargePlugin.Instance.Log.LogInfo(string.Format("Status: {0} {1} Female Sp: {2} Time: {3} Male Time: {4}", AnimationStatus.AnimSequence.IsPlaying(), AnimationStatus.PlayingAnimation, AnimationStatus.FemaleSpeed, chaFemales[0].animBody.GetCurrentAnimatorStateInfo(0).normalizedTime, chaMales[0].animBody.GetCurrentAnimatorStateInfo(0).normalizedTime));
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Spnking), "setPlay")]
        static void SpnkingAnimOffset(Spnking __instance, string _playAnimation)
        {
            AnimationStatus.PlayingAnimation = _playAnimation;
            ChaControl[] chaFemales = (ChaControl[])chaFemalesFieldInfo.GetValue(__instance);
            ChaControl[] chaMales = (ChaControl[])chaMalesFieldInfo.GetValue(__instance);
            HItemCtrl item = (HItemCtrl)itemFieldInfo.GetValue(__instance);

            if (AnimationStatus.FemaleSpeedTween != null)
            {
                AnimationStatus.AnimSequence.Kill();
                if (chaFemales[0].visibleAll && chaFemales[0].objBodyBone != null)
                {
                    chaFemales[0].setAnimatorParamFloat("speed", 0f);
                }
                if (chaMales[0].objBodyBone != null)
                {
                    chaMales[0].setAnimatorParamFloat("speed", 0f);
                }
                if (item.GetItem() != null)
                {
                    item.setAnimatorParamFloat("speed", 0f);
                }
  //              HS2TakeChargePlugin.Instance.Log.LogInfo(string.Format("Status: {0} {1} Female Sp: {2} Time: {3} Male Time: {4}", AnimationStatus.AnimSequence.IsPlaying(), AnimationStatus.PlayingAnimation, AnimationStatus.FemaleSpeed, chaFemales[0].animBody.GetCurrentAnimatorStateInfo(0).normalizedTime, chaMales[0].animBody.GetCurrentAnimatorStateInfo(0).normalizedTime));
            }

            TCAnimationTiming timing = HS2TakeChargePlugin.Instance.RuleSet.Timing(Singleton<HSceneFlagCtrl>.Instance.nowAnimationInfo.nameAnimation, PositionCategories.SPANKING.ToString(), SpnkingStageSwitch());

            AnimationStatus.FemaleSpeed = timing.minSpeed;
            AnimationStatus.MaleSpeed = timing.minSpeed;
            AnimationStatus.FemaleOffset = timing.minFemaleOffset;

            AnimationStatus.FemaleSpeedTween = DOTween.To(() => AnimationStatus.FemaleSpeed, newSpeed => AnimationStatus.FemaleSpeed = newSpeed, timing.maxSpeed, timing.speedLoopTime).SetEase(timing.SpeedEaseEnum());
            AnimationStatus.MaleSpeedTween = DOTween.To(() => AnimationStatus.MaleSpeed, newSpeed => AnimationStatus.MaleSpeed = newSpeed, timing.maxSpeed, timing.speedLoopTime).SetEase(timing.SpeedEaseEnum());
            AnimationStatus.FemaleOffsetTween = DOTween.To(() => AnimationStatus.FemaleOffset, newOffset => AnimationStatus.FemaleOffset = newOffset, timing.maxFemaleOffset, timing.femaleOffsetLoopTime).SetEase(timing.FemaleOffsetEaseEnum());
            AnimationStatus.AnimSequence = DOTween.Sequence();
            AnimationStatus.AnimSequence.Insert(0, AnimationStatus.FemaleSpeedTween);
            AnimationStatus.AnimSequence.Insert(0, AnimationStatus.MaleSpeedTween);
            AnimationStatus.AnimSequence.Insert(0, AnimationStatus.FemaleOffsetTween);
            AnimationStatus.AnimSequence.SetLoops(-1, timing.LoopTypeEnum());

        }

        static TCAnimationStage SpnkingStageSwitch()
        {
            switch (AnimationStatus.PlayingAnimation)
            {
                case "Idle":
                    return TCAnimationStage.IDLE;
                case "WIdle":
                case "WAction":
                case "D_Action":
                    return TCAnimationStage.SLOW_LOOP;
                case "SIdle":
                case "SAction":
                    return TCAnimationStage.FAST_LOOP;
                case "Orgasm":
                    return TCAnimationStage.O_LOOP;
                case "D_Orgasm":
                    return TCAnimationStage.ORGASM;
                case "D_Orgasm_A":
                    return TCAnimationStage.POST_ORGASM;
                default:
                    return TCAnimationStage.IDLE;
            }
        }
    }
}
