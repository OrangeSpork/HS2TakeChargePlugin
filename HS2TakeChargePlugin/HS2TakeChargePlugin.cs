using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
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
    [BepInPlugin(GUID,  PluginName, Version)]
    public class HS2TakeChargePlugin : BaseUnityPlugin
    {
        public const string GUID = "orange.spork.hs2takechargeplugin";
        public const string PluginName = "HS2TakeChargePlugin";
        public const string Version = "1.0.0";

        public static ConfigEntry<KeyboardShortcut> TakeChargeKey { get; set; }
        public static ConfigEntry<KeyboardShortcut> AutoKey { get; set; }


        public static HS2TakeChargePlugin Instance { get; set; }

        internal BepInEx.Logging.ManualLogSource Log => Logger;

        public HS2TakeChargePlugin()
        {
            if (Instance != null)
            {
                throw new InvalidOperationException("Singleton Only.");
            }

            Instance = this;

            TakeChargeKey = Config.Bind("Hotkeys", "Enable Take Charge Mode", new KeyboardShortcut(KeyCode.T, KeyCode.LeftControl));
            AutoKey = Config.Bind("Hotkeys", "Enable Auto Mode", new KeyboardShortcut(KeyCode.A, KeyCode.LeftControl));

            PatchMe();
        }

        void Update()
        {
            if (TakeChargeKey.Value.IsUp())
            {
                if (HSceneFlagCtrl.Instance.initiative == 0)
                {
                    Log.LogMessage("Take Charge: Active");                    
                }
                else
                {
                    Log.LogMessage("Take Charge: Disabled");
                }
                HSceneFlagCtrl.Instance.click = HSceneFlagCtrl.ClickKind.LeaveItToYou;
            }
            if (AutoKey.Value.IsUp())
            {
                if (HSceneFlagCtrl.Instance.initiative == 0)
                {
                    Log.LogMessage("Auto H: Enabled");
                }
                else
                {
                    Log.LogMessage("Auto H: Disabled");
                }
                HSceneFlagCtrl.Instance.initiative = 2;
            }            
        }

        public void PatchMe()
        {
            Harmony harmony = new Harmony(GUID);

            MethodInfo hSceneManagerStartMethod = AccessTools.Method(typeof(HSceneManager), "Start", null, null);
            harmony.Patch(hSceneManagerStartMethod, new HarmonyMethod(typeof(HS2TakeChargePlugin), "SetupHResourceTables"), null, null, null);

            MethodInfo hSceneStartMethod = AccessTools.Method(typeof(HScene), "Start", null, null);
            harmony.Patch(hSceneStartMethod, new HarmonyMethod(typeof(HS2TakeChargePlugin), "SetupHScene"), null, null, null);

            MethodInfo houshiOverride = AccessTools.Method(typeof(Houshi), "AutoStartProcTrigger", new Type[] { typeof(bool), typeof(float) }, null);
            harmony.Patch(houshiOverride, new HarmonyMethod(typeof(HS2TakeChargePlugin), "FixWheelValue"), null, null, null);

            MethodInfo sonyuOverride = AccessTools.Method(typeof(Sonyu), "AutoStartProcTrigger", new Type[] { typeof(bool), typeof(float) }, null);
            harmony.Patch(sonyuOverride, new HarmonyMethod(typeof(HS2TakeChargePlugin), "FixWheelValue2"), null, null, null);

            MethodInfo houshiCumFix = AccessTools.Method(typeof(Houshi), "AutoOLoopProc", new Type[] { typeof(int), typeof(float), typeof(int), typeof(HScene.AnimationListInfo) }, null);
            harmony.Patch(houshiCumFix, new HarmonyMethod(typeof(HS2TakeChargePlugin), "HoushiCumFix"), null, null, null);
        }

        static void HoushiCumFix(Houshi __instance, HSceneSprite ___sprite)
        {
            if (HSceneFlagCtrl.Instance.initiative == 1 && HSceneFlagCtrl.Instance.feel_m >= 1f)
            {

                // Roll random in/out
                if (___sprite.IsFinishVisible(3) || ___sprite.IsFinishVisible(4))
                {
                    float finishRoll = UnityEngine.Random.value;
                    if (___sprite.IsFinishVisible(3) && finishRoll <= .33f)
                    {
                        HSceneFlagCtrl.Instance.click = HSceneFlagCtrl.ClickKind.FinishDrink;
                    }
                    else if (___sprite.IsFinishVisible(4) && (finishRoll > .33f && finishRoll <= .66f))
                    {
                        HSceneFlagCtrl.Instance.click = HSceneFlagCtrl.ClickKind.FinishVomit;
                    }
                    else
                    {
                        HSceneFlagCtrl.Instance.click = HSceneFlagCtrl.ClickKind.FinishOutSide;
                    }

                }
                else
                {
                    HSceneFlagCtrl.Instance.click = HSceneFlagCtrl.ClickKind.FinishOutSide;
                }

            }
        }

        static void FixWheelValue(bool _start, ref float _wheel)
        {
            if (HSceneFlagCtrl.Instance.initiative > 0)
            {
                _wheel = 0.1f;
            }
        }

        static void FixWheelValue2(bool _start, ref float wheel)
        {
            if (HSceneFlagCtrl.Instance.initiative > 0)
            {
                wheel = 0.1f;
            }
        }

        static void SetupHResourceTables(HSceneManager __instance)
        {
            HAutoCtrl.HAutoInfo autoInfo = new HAutoCtrl.HAutoInfo();
            autoInfo.start.minmax = new Vector2(5, 5);
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
            HS2TakeChargePlugin.Instance.Log.LogInfo("Setup HResource Tables");
        }

        static void SetupHScene(HScene __instance)
        {
            __instance.ctrlAuto.Load(null, 0);
            HS2TakeChargePlugin.Instance.Log.LogInfo("HScene Load");
        }
    }
}
