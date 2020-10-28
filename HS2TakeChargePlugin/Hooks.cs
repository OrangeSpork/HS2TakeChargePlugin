using AIChara;
using HarmonyLib;
using Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HS2TakeChargePlugin
{
    public static class Hooks
    {

        [HarmonyPrefix, HarmonyPatch(typeof(HSceneManager), "Start")]
        static void SetupHResourceTables(HSceneManager __instance)
        {
            HAutoCtrl.HAutoInfo autoInfo = new HAutoCtrl.HAutoInfo();
            autoInfo.start.minmax = new Vector2(3, 7);
            autoInfo.start.time = 5;
            autoInfo.reStart.minmax = new Vector2(10, 15);
            autoInfo.reStart.time = 12;
            autoInfo.speed.minmax = new Vector2(3, 8);
            autoInfo.speed.time = 5;
            autoInfo.lerpTimeSpeed = 2f;
            autoInfo.loopChange.minmax = new Vector2(10, 15);
            autoInfo.loopChange.time = 12;
            autoInfo.motionChange.minmax = new Vector2(10, 40);
            autoInfo.motionChange.time = 25;
            autoInfo.rateWeakLoop = 50;
            autoInfo.rateHit = 50;
            autoInfo.rateAddMotionChange = 10;
            autoInfo.rateRestartMotionChange = 80;
            autoInfo.pull.minmax = new Vector2(8, 8);
            autoInfo.pull.time = 8;
            autoInfo.rateInsertPull = 70;
            autoInfo.rateNotInsertPull = 30;

            HAutoCtrl.AutoLeaveItToYou autoLeaveItToYou = new HAutoCtrl.AutoLeaveItToYou();
            autoLeaveItToYou.time.minmax = new Vector2(30, 50);
            autoLeaveItToYou.time.Reset();
            autoLeaveItToYou.baseTime = autoLeaveItToYou.time.minmax;
            autoLeaveItToYou.rate = 50;

            HSceneManager.HResourceTables.HAutoInfo = autoInfo;
            HSceneManager.HResourceTables.HAutoLeaveItToYou = autoLeaveItToYou;

            spankTimer = new HAutoCtrl.AutoTime();
            spankTimer.minmax = new Vector2(1, 4);
            spankTimer.time = 2;
            spankTimer.Reset();
        }


        [HarmonyPrefix, HarmonyPatch(typeof(HScene), "Start")]
        static void SetupHScene(HScene __instance)
        {
            __instance.ctrlAuto.Load(null, 0);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Houshi), "AutoStartProcTrigger")]
        static void FixHoushiWheelValue(bool _start, ref float _wheel)
        {
            _wheel = FixWheelValue(_wheel);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Sonyu), "AutoStartProcTrigger")]
        static void FixSonyuWheelValue(bool _start, ref float wheel)
        {
            wheel = FixWheelValue(wheel);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Les), "AutoStartProcTrigger")]
        static void FixLesWheelValue(bool _start, ref float _wheel)
        {
            _wheel = FixWheelValue(_wheel);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(MultiPlay_F1M2), "AutoStartProcTrigger")]
        static void FixF1M2WheelValue(bool _start, ref float _wheel)
        {
            _wheel = FixWheelValue(_wheel);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(MultiPlay_F2M1), "AutoStartProcTrigger")]
        static void FixF2M1WheelValue(bool _start, ref float wheel)
        {
            wheel = FixWheelValue(wheel);
        }

        static float FixWheelValue(float _wheel)
        {
            if (Singleton<HSceneFlagCtrl>.Instance.initiative > 0)
            {
                _wheel = 0.1f;
            }
            return _wheel;
        }

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
        private static HAutoCtrl.AutoTime spankTimer;

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
            if (!spankTimer.IsTime())
            {
                return false;
            }
            else
            {
                spankTimer.Reset();
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

        [HarmonyPostfix, HarmonyPatch(typeof(HAutoCtrl), "GetAnimation")]
        static void SelectAutoAnimation(List<HScene.AnimationListInfo>[] _listAnim, int _initiative, bool _isFirst, ref HScene.StartMotion __result)
        {
            if (_initiative == 2)
            {
                int[] excludedCategories;
                if (HS2TakeChargePlugin.NoSpanking.Value)
                {
                    excludedCategories = new int[] { 5, 3 };
                }
                else
                {
                    excludedCategories = new int[] { 5 };
                }
                __result = HS2TakeChargePlugin.Instance.RandomSelectAnimation(_listAnim, excludedCategories);
            }
        }
        
        [HarmonyPrefix, HarmonyPatch(typeof(Houshi), "AutoOLoopProc")]
        static void HoushiCumFix(Houshi __instance, HSceneSprite ___sprite)
        {
            if (Singleton<HSceneFlagCtrl>.Instance.initiative == 1 && Singleton<HSceneFlagCtrl>.Instance.feel_m >= 1f)
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
    }
}
