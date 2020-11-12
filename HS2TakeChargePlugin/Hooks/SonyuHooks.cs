using AIChara;
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
    public static class SonyuHooks
    {
        [HarmonyPrefix, HarmonyPatch(typeof(Sonyu), "AutoStartProcTrigger")]
        static void FixSonyuWheelValue(bool _start, ref float wheel)
        {
            wheel = GeneralHooks.FixWheelValue(wheel);
        }


        private static FieldInfo chaFemalesFieldInfo = AccessTools.Field(typeof(Sonyu), "chaFemales");
        private static FieldInfo chaMalesFieldInfo = AccessTools.Field(typeof(Sonyu), "chaMales");
        private static FieldInfo itemFieldInfo = AccessTools.Field(typeof(Sonyu), "item");
        [HarmonyPostfix, HarmonyPatch(typeof(Sonyu), "setAnimationParamater")]
        static void SonyuSpeedGambit(Sonyu __instance)
        {
            if (!HS2TakeChargePlugin.Instance.AnimationOverrideActive())
            {
                return;
            }

            ChaControl[] chaFemales = (ChaControl[])chaFemalesFieldInfo.GetValue(__instance);
            ChaControl[] chaMales = (ChaControl[])chaMalesFieldInfo.GetValue(__instance);
            HItemCtrl item = (HItemCtrl)itemFieldInfo.GetValue(__instance);

            if (chaFemales[0].visibleAll && chaFemales[0].objBodyBone != null)
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
    //        HS2TakeChargePlugin.Instance.Log.LogInfo(string.Format("Status: {0} {1} Female Sp: {2} Time: {3} Male Time: {4}", AnimationStatus.AnimSequence.IsPlaying(), AnimationStatus.PlayingAnimation, AnimationStatus.FemaleSpeed, chaFemales[0].animBody.GetCurrentAnimatorStateInfo(0).normalizedTime, chaMales[0].animBody.GetCurrentAnimatorStateInfo(0).normalizedTime));
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Sonyu), "setPlay")]
        static void SonyuAnimOffset(Sonyu __instance, string _playAnimation)
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
   //             HS2TakeChargePlugin.Instance.Log.LogInfo(string.Format("Status: {0} {1} Female Sp: {2} Time: {3} Male Time: {4}", AnimationStatus.AnimSequence.IsPlaying(), AnimationStatus.PlayingAnimation, AnimationStatus.FemaleSpeed, chaFemales[0].animBody.GetCurrentAnimatorStateInfo(0).normalizedTime, chaMales[0].animBody.GetCurrentAnimatorStateInfo(0).normalizedTime));
            }

            TCAnimationTiming timing = HS2TakeChargePlugin.Instance.RuleSet.Timing(Singleton<HSceneFlagCtrl>.Instance.nowAnimationInfo.nameAnimation, PositionCategories.SEX.ToString(), SonyuStageSwitch());

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

        static TCAnimationStage SonyuStageSwitch()
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
                case "OrgasmF_IN":
                case "OrgasmM_IN":
                case "OrgasmS_IN":
                case "OrgasmM_OUT":
                case "Vomit":
                    return TCAnimationStage.ORGASM;
                case "OrgasmM_OUT_A":
                case "Orgasm_IN_A":
                case "Pull":
                case "Drop":
                    return TCAnimationStage.POST_ORGASM;
                default:
                    return TCAnimationStage.IDLE;
            }
        }

    }
}
