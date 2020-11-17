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

namespace HS2TakeChargePlugin
{

    public static class GeneralHooks
    {

        public static HAutoCtrl.AutoTime SpankTimer;

        [HarmonyPrefix, HarmonyPatch(typeof(HSceneManager), "Start")]
        static void SetupHResourceTables(HSceneManager __instance)
        {
            HAutoCtrl.HAutoInfo autoInfo = new HAutoCtrl.HAutoInfo();
            autoInfo.start.minmax = new Vector2(HS2TakeChargePlugin.StartWaitMin.Value, HS2TakeChargePlugin.StartWaitMax.Value);
            autoInfo.start.time = (HS2TakeChargePlugin.StartWaitMin.Value + HS2TakeChargePlugin.StartWaitMax.Value) / 2;
            autoInfo.reStart.minmax = new Vector2(HS2TakeChargePlugin.RestartWaitMin.Value, HS2TakeChargePlugin.RestartWaitMax.Value);
            autoInfo.reStart.time = (HS2TakeChargePlugin.RestartWaitMin.Value + HS2TakeChargePlugin.RestartWaitMax.Value) / 2;
            autoInfo.speed.minmax = new Vector2(3, 8);
            autoInfo.speed.time = 5;
            autoInfo.lerpTimeSpeed = 2f;
            autoInfo.loopChange.minmax = new Vector2(HS2TakeChargePlugin.LoopChangeWaitMin.Value, HS2TakeChargePlugin.LoopChangeWaitMax.Value);
            autoInfo.loopChange.time = (HS2TakeChargePlugin.LoopChangeWaitMax.Value + HS2TakeChargePlugin.LoopChangeWaitMin.Value) / 2;
            autoInfo.motionChange.minmax = new Vector2(HS2TakeChargePlugin.PositionChangeWaitMin.Value, HS2TakeChargePlugin.PositionChangeWaitMax.Value);
            autoInfo.motionChange.time = (HS2TakeChargePlugin.PositionChangeWaitMax.Value + HS2TakeChargePlugin.PositionChangeWaitMin.Value) / 2;
            autoInfo.rateWeakLoop = 50;
            autoInfo.rateHit = 50;
            autoInfo.rateAddMotionChange = 10;
            autoInfo.rateRestartMotionChange = HS2TakeChargePlugin.ChanceToRestartPosition.Value;
            autoInfo.pull.minmax = new Vector2(HS2TakeChargePlugin.PulloutWaitMin.Value, HS2TakeChargePlugin.PulloutWaitMax.Value);
            autoInfo.pull.time = (HS2TakeChargePlugin.PulloutWaitMin.Value + HS2TakeChargePlugin.PulloutWaitMax.Value) / 2;
            autoInfo.rateInsertPull = HS2TakeChargePlugin.InsertedChanceToPullOut.Value;
            autoInfo.rateNotInsertPull = HS2TakeChargePlugin.NonInsertedChanceToPullOut.Value;

            HAutoCtrl.AutoLeaveItToYou autoLeaveItToYou = new HAutoCtrl.AutoLeaveItToYou();
            autoLeaveItToYou.time.minmax = new Vector2(30, 50);
            autoLeaveItToYou.time.Reset();
            autoLeaveItToYou.baseTime = autoLeaveItToYou.time.minmax;
            autoLeaveItToYou.rate = 50;

            HSceneManager.HResourceTables.HAutoInfo = autoInfo;
            HSceneManager.HResourceTables.HAutoLeaveItToYou = autoLeaveItToYou;

            SpankTimer = new HAutoCtrl.AutoTime();
            SpankTimer.minmax = new Vector2(HS2TakeChargePlugin.SpankingWaitMin.Value, HS2TakeChargePlugin.SpankingWaitMax.Value);
            SpankTimer.time = (HS2TakeChargePlugin.SpankingWaitMin.Value + HS2TakeChargePlugin.SpankingWaitMax.Value);
            SpankTimer.Reset();
        }


        [HarmonyPrefix, HarmonyPatch(typeof(HScene), "Start")]
        static void SetupHScene(HScene __instance)
        {
            __instance.ctrlAuto.Load(null, 0);
        }

        public static float FixWheelValue(float _wheel)
        {
            if (Singleton<HSceneFlagCtrl>.Instance.initiative > 0)
            {
                _wheel = 0.1f;
            }
            return _wheel;
        }
   
        [HarmonyPostfix, HarmonyPatch(typeof(HAutoCtrl), "GetAnimation")]
        static void SelectAutoAnimation(List<HScene.AnimationListInfo>[] _listAnim, int _initiative, bool _isFirst, ref HScene.StartMotion __result)
        {
            if (_initiative == 2)
            {
                __result = HS2TakeChargePlugin.Instance.RandomSelectAnimation(_listAnim);
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(HScene), "IsIdle")]
        static bool HsceneIsIdleAdditions(Animator _anim, HScene __instance, ref bool __result)
        {
            if (HS2TakeChargePlugin.ResetArousalOnChange.Value)
            {
                Singleton<HSceneFlagCtrl>.Instance.feel_m = 0f;
                Singleton<HSceneFlagCtrl>.Instance.feel_f = 0f;
            }

            if (HS2TakeChargePlugin.ResetToIdleOnChange.Value)
            {
                __result = true;
                return false;
            }            
            else
            {
               return true;
            }
        }
        
       
    }
}
