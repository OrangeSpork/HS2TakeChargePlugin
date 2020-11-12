﻿using AIChara;
using DG.Tweening;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HS2TakeChargePlugin.Hooks
{
    public static class LesHooks
    {
        [HarmonyPrefix, HarmonyPatch(typeof(Les), "AutoStartProcTrigger")]
        static void FixLesWheelValue(bool _start, ref float _wheel)
        {
            _wheel = GeneralHooks.FixWheelValue(_wheel);
        }

        private static FieldInfo chaFemalesFieldInfo = AccessTools.Field(typeof(Les), "chaFemales");
        private static FieldInfo itemFieldInfo = AccessTools.Field(typeof(Les), "item");
        [HarmonyPostfix, HarmonyPatch(typeof(Les), "setAnimationParamater")]
        static void LesSpeedGambit(Les __instance)
        {
            if (!HS2TakeChargePlugin.Instance.AnimationOverrideActive())
            {
                return;
            }

            ChaControl[] chaFemales = (ChaControl[])chaFemalesFieldInfo.GetValue(__instance);
            HItemCtrl item = (HItemCtrl)itemFieldInfo.GetValue(__instance);
           
            for (int j = 0; j < chaFemales.Length; j++)
            {
                if (chaFemales[j].visibleAll && !(chaFemales[j].objTop == null))
                {
                    chaFemales[j].setAnimatorParamFloat("speed", AnimationStatus.FemaleSpeed);
                    if (AnimationStatus.FemaleOffset != 0 && j > 0)
                        chaFemales[j].animBody.Play(AnimationStatus.PlayingAnimation, 0, (chaFemales[0].animBody.GetCurrentAnimatorStateInfo(0).normalizedTime + AnimationStatus.FemaleOffset));
                }
            }
            if (item.GetItem() != null)
            {
                item.setAnimatorParamFloat("speed", AnimationStatus.FemaleSpeed);
            }
      //      HS2TakeChargePlugin.Instance.Log.LogInfo(string.Format("Status: {0} {1} Female Sp: {2} Time: {3} Female2 Time: {4}", AnimationStatus.AnimSequence.IsPlaying(), AnimationStatus.PlayingAnimation, AnimationStatus.FemaleSpeed, chaFemales[0].animBody.GetCurrentAnimatorStateInfo(0).normalizedTime, chaFemales[1].animBody.GetCurrentAnimatorStateInfo(0).normalizedTime));
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Les), "setPlay")]
        static void LesAnimOffset(Les __instance, string _playAnimation)
        {
            AnimationStatus.PlayingAnimation = _playAnimation;
            ChaControl[] chaFemales = (ChaControl[])chaFemalesFieldInfo.GetValue(__instance);
            HItemCtrl item = (HItemCtrl)itemFieldInfo.GetValue(__instance);

            if (AnimationStatus.FemaleSpeedTween != null)
            {
                AnimationStatus.AnimSequence.Kill();
                for (int j = 0; j < chaFemales.Length; j++)
                {
                    if (chaFemales[j].visibleAll && !(chaFemales[j].objTop == null))
                    {
                        chaFemales[j].setAnimatorParamFloat("speed", 0f);
                    }
                }

                if (item.GetItem() != null)
                {
                    item.setAnimatorParamFloat("speed", 0f);
                }
       //         HS2TakeChargePlugin.Instance.Log.LogInfo(string.Format("Status: {0} {1} Female Sp: {2} Time: {3} Female2 Time: {4}", AnimationStatus.AnimSequence.IsPlaying(), AnimationStatus.PlayingAnimation, AnimationStatus.FemaleSpeed, chaFemales[0].animBody.GetCurrentAnimatorStateInfo(0).normalizedTime, chaFemales[1].animBody.GetCurrentAnimatorStateInfo(0).normalizedTime));
            }

            TCAnimationTiming timing = HS2TakeChargePlugin.Instance.RuleSet.Timing(Singleton<HSceneFlagCtrl>.Instance.nowAnimationInfo.nameAnimation, PositionCategories.LESBIAN.ToString(), LesStageSwitch());

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

        static TCAnimationStage LesStageSwitch()
        {
            switch (AnimationStatus.PlayingAnimation)
            {
                case "Idle":
                    return TCAnimationStage.IDLE;
                case "WLoop":
                    return TCAnimationStage.SLOW_LOOP;
                case "SLoop":
                    return TCAnimationStage.FAST_LOOP;
                case "OLoop":
                    return TCAnimationStage.O_LOOP;
                case "Orgasm":
                    return TCAnimationStage.ORGASM;
                case "Orgasm_A":
                    return TCAnimationStage.POST_ORGASM;
                default:
                    return TCAnimationStage.IDLE;
            }
        }
    }
}