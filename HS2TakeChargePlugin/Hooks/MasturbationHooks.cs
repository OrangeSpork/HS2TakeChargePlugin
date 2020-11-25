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

namespace HS2TakeChargePlugin.Hooks
{
    public static class MasturbationHooks
    {

        [HarmonyPrefix, HarmonyPatch(typeof(Masturbation), "AutoStartProc")]
        static bool MasturbateAfterAnimChangeFix(int _start, Masturbation __instance)
        {
            if (_start == 0)
            {
                return true;
            }
            else if (Singleton<HSceneManager>.Instance.Hscene.ctrlAuto.IsChangeActionAtRestart())
            {
                Singleton<HSceneFlagCtrl>.Instance.isAutoActionChange = true;
                Singleton<HSceneManager>.Instance.Hscene.ctrlAuto.Reset();
                Singleton<HSceneManager>.Instance.Hscene.ctrlFlag.speed = 0f;
                Singleton<HSceneManager>.Instance.Hscene.ctrlFlag.loopType = 0;
                Singleton<HSceneManager>.Instance.Hscene.ctrlFlag.isNotCtrl = false;
                return false;
            }
            return true;
        }

        private static FieldInfo bAutoField = AccessTools.Field(typeof(Masturbation), "bAuto");

        [HarmonyPrefix, HarmonyPatch(typeof(Masturbation), "Proc")]
        static void MasturbateAutoFix(Masturbation __instance)
        {
            bAutoField.SetValue(__instance, (HS2TakeChargePlugin.Instance.GetCurrentModeControl() != 4 || Singleton<HSceneFlagCtrl>.Instance.initiative > 0));
        }

        private static FieldInfo chaFemalesFieldInfo = AccessTools.Field(typeof(Masturbation), "chaFemales");
        private static FieldInfo chaMalesFieldInfo = AccessTools.Field(typeof(Masturbation), "chaMales");
        private static FieldInfo itemFieldInfo = AccessTools.Field(typeof(Masturbation), "item");
        private static FieldInfo animParType = AccessTools.Field(typeof(Masturbation), "animPar");
        private static FieldInfo speedField = AccessTools.Field(animParType.FieldType, "speed");
        [HarmonyPostfix, HarmonyPatch(typeof(Masturbation), "setAnimationParamater")]
        static void MasturbationSpeedGambit(Masturbation __instance)
        {
            if (!HS2TakeChargePlugin.Instance.AnimationOverrideActive() && HS2TakeChargePlugin.Instance.ManualSpeedAdder == 0f)
            {
                return;
            }

            ChaControl[] chaFemales = (ChaControl[])chaFemalesFieldInfo.GetValue(__instance);
            ChaControl[] chaMales = (ChaControl[])chaMalesFieldInfo.GetValue(__instance);
            HItemCtrl item = (HItemCtrl)itemFieldInfo.GetValue(__instance);

            if (HS2TakeChargePlugin.Instance.AnimationOverrideActive())
            {
                if (chaFemales[0].visibleAll && chaFemales[0].objTop != null)
                {
                    chaFemales[0].setAnimatorParamFloat("speed", AnimationStatus.FemaleSpeed);
                    if (AnimationStatus.FemaleOffset != 0)
                        chaFemales[0].animBody.Play(AnimationStatus.PlayingAnimation, 0, (chaMales[0].animBody.GetCurrentAnimatorStateInfo(0).normalizedTime + AnimationStatus.FemaleOffset));
                }
                if (chaMales[0].objBodyBone != null && chaMales[0].animBody.runtimeAnimatorController != null)
                {
                    chaMales[0].setAnimatorParamFloat("speed", AnimationStatus.MaleSpeed);
                }
                if (item.GetItem() != null)
                {
                    item.setAnimatorParamFloat("speed", AnimationStatus.FemaleSpeed);
                }
            }
            else
            {
                float originalSpeed = (float)speedField.GetValue(animParType.GetValue(__instance));
                if (chaFemales[0].visibleAll && chaFemales[0].objTop != null)
                {
                    chaFemales[0].setAnimatorParamFloat("speed", originalSpeed + HS2TakeChargePlugin.Instance.ManualSpeedAdder);
                }
                if (chaMales[0].objBodyBone != null && chaMales[0].animBody.runtimeAnimatorController != null)
                {
                    chaMales[0].setAnimatorParamFloat("speed", originalSpeed + HS2TakeChargePlugin.Instance.ManualSpeedAdder);
                }
                if (item.GetItem() != null)
                {
                    item.setAnimatorParamFloat("speed", originalSpeed + HS2TakeChargePlugin.Instance.ManualSpeedAdder);
                }
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Masturbation), "setPlay")]
        static void MasturbationAnimOffset(Masturbation __instance, string _playAnimation)
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
      //          HS2TakeChargePlugin.Instance.Log.LogInfo(string.Format("Status: {0} {1} Female Sp: {2} Time: {3} Male Time: {4}", AnimationStatus.AnimSequence.IsPlaying(), AnimationStatus.PlayingAnimation, AnimationStatus.FemaleSpeed, chaFemales[0].animBody.GetCurrentAnimatorStateInfo(0).normalizedTime, chaMales[0].animBody.GetCurrentAnimatorStateInfo(0).normalizedTime));
            }

            TCAnimationTiming timing = HS2TakeChargePlugin.Instance.RuleSet.Timing(Singleton<HSceneFlagCtrl>.Instance.nowAnimationInfo.nameAnimation, PositionCategories.MASTURBATION.ToString(), MasturbationStageSwitch());

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

        static TCAnimationStage MasturbationStageSwitch()
        {
            switch (AnimationStatus.PlayingAnimation)
            {
                case "WLoop":
                    return TCAnimationStage.IDLE;
                case "MLoop":
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
