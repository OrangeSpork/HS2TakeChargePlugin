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
    public static class HoushiHooks
    {
        [HarmonyPrefix, HarmonyPatch(typeof(Houshi), "AutoStartProcTrigger")]
        static void FixHoushiWheelValue(bool _start, ref float _wheel)
        {
            _wheel = GeneralHooks.FixWheelValue(_wheel);
        }

        private static FieldInfo chaHoushiFemalesFieldInfo = AccessTools.Field(typeof(Houshi), "chaFemales");
        private static FieldInfo chaHoushiMalesFieldInfo = AccessTools.Field(typeof(Houshi), "chaMales");
        private static FieldInfo houshiItemFieldInfo = AccessTools.Field(typeof(Houshi), "item");
        [HarmonyPostfix, HarmonyPatch(typeof(Houshi), "setAnimationParamater")]
        static void HoushiSpeedGambit(Houshi __instance)
        {
            if (!HS2TakeChargePlugin.Instance.AnimationOverrideActive())
            {
                return;
            }

            ChaControl[] chaFemales = (ChaControl[])chaHoushiFemalesFieldInfo.GetValue(__instance);
            ChaControl[] chaMales = (ChaControl[])chaHoushiMalesFieldInfo.GetValue(__instance);
            HItemCtrl item = (HItemCtrl)houshiItemFieldInfo.GetValue(__instance);

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
      //      HS2TakeChargePlugin.Instance.Log.LogInfo(string.Format("Status: {0} {1} Female Sp: {2} Time: {3} Male Time: {4}", AnimationStatus.AnimSequence.IsPlaying(), AnimationStatus.PlayingAnimation, AnimationStatus.FemaleSpeed, chaFemales[0].animBody.GetCurrentAnimatorStateInfo(0).normalizedTime, chaMales[0].animBody.GetCurrentAnimatorStateInfo(0).normalizedTime));
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Houshi), "setPlay")]
        static void HoushiAnimOffset(Houshi __instance, string _playAnimation)
        {
            AnimationStatus.PlayingAnimation = _playAnimation;
            ChaControl[] chaFemales = (ChaControl[])chaHoushiFemalesFieldInfo.GetValue(__instance);
            ChaControl[] chaMales = (ChaControl[])chaHoushiMalesFieldInfo.GetValue(__instance);
            HItemCtrl item = (HItemCtrl)houshiItemFieldInfo.GetValue(__instance);

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
            //    HS2TakeChargePlugin.Instance.Log.LogInfo(string.Format("Status: {0} {1} Female Sp: {2} Time: {3} Male Time: {4}", AnimationStatus.AnimSequence.IsPlaying(), AnimationStatus.PlayingAnimation, AnimationStatus.FemaleSpeed, chaFemales[0].animBody.GetCurrentAnimatorStateInfo(0).normalizedTime, chaMales[0].animBody.GetCurrentAnimatorStateInfo(0).normalizedTime));
            }

            TCAnimationTiming timing = HS2TakeChargePlugin.Instance.RuleSet.Timing(Singleton<HSceneFlagCtrl>.Instance.nowAnimationInfo.nameAnimation, PositionCategories.SERVICE.ToString(), HoushiStageSwitch());

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

        [HarmonyPrefix, HarmonyPatch(typeof(Houshi), "AutoOLoopProc")]
        static void HoushiCumFix(Houshi __instance, HSceneSprite ___sprite)
        {
            if (Singleton<HSceneFlagCtrl>.Instance.initiative >= 1 && Singleton<HSceneFlagCtrl>.Instance.feel_m >= 1f)
            {

                // Roll random in/out
                if (___sprite.IsFinishVisible(3) || ___sprite.IsFinishVisible(4))
                {
                    float finishRoll = UnityEngine.Random.value;
                    if (___sprite.IsFinishVisible(3) && finishRoll <= .33f)
                    {
                        Singleton<HSceneFlagCtrl>.Instance.click = HSceneFlagCtrl.ClickKind.FinishDrink;
                    }
                    else if (___sprite.IsFinishVisible(4) && (finishRoll > .33f && finishRoll <= .66f))
                    {
                        Singleton<HSceneFlagCtrl>.Instance.click = HSceneFlagCtrl.ClickKind.FinishVomit;
                    }
                    else
                    {
                        Singleton<HSceneFlagCtrl>.Instance.click = HSceneFlagCtrl.ClickKind.FinishOutSide;
                    }

                }
                else
                {
                    Singleton<HSceneFlagCtrl>.Instance.click = HSceneFlagCtrl.ClickKind.FinishOutSide;
                }

            }
        }

        static TCAnimationStage HoushiStageSwitch()
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
                case "Orgasm_OUT":
                case "Orgasm_IN":
                case "Drink_IN":
                case "Drink":
                case "Vomit_IN":
                case "Vomit":
                    return TCAnimationStage.ORGASM;
                case "Orgasm_OUT_A":
                case "Drink_A":
                case "Vomit_A":
                    return TCAnimationStage.POST_ORGASM;
                default:
                    return TCAnimationStage.IDLE;
            }
        }
    }
}
