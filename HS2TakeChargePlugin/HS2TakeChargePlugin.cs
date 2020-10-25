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
    [BepInProcess("HoneySelect2.exe")]
    [BepInProcess("HoneySelect2VR.exe")]
    public class HS2TakeChargePlugin : BaseUnityPlugin
    {
        public const string GUID = "orange.spork.hs2takechargeplugin";
        public const string PluginName = "HS2TakeChargePlugin";
        public const string Version = "1.0.1";

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
                if (Singleton<HSceneFlagCtrl>.Instance.initiative == 0)
                {
                    Log.LogMessage("Take Charge: Active");                    
                }
                else
                {
                    Log.LogMessage("Take Charge: Disabled");
                }
                Singleton<HSceneFlagCtrl>.Instance.click = HSceneFlagCtrl.ClickKind.LeaveItToYou;
            }
            if (AutoKey.Value.IsUp())
            {
                if (Singleton<HSceneFlagCtrl>.Instance.initiative == 0)
                {
                    Log.LogMessage("Auto H: Enabled");
                    Singleton<HSceneFlagCtrl>.Instance.initiative = 2;
                }
                else
                {
                    Log.LogMessage("Auto H: Disabled");
                    Singleton<HSceneFlagCtrl>.Instance.initiative = 0;
                }                
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

            MethodInfo autoControlMethodSelector = AccessTools.Method(typeof(HAutoCtrl), "GetAnimation", new Type[] { typeof(List<HScene.AnimationListInfo>[]), typeof(int), typeof(bool) }, null);
            //   harmony.Patch(autoControlMethodSelector, new HarmonyMethod(typeof(HS2TakeChargePlugin), "SelectingAnimationLogger"), new HarmonyMethod(typeof(HS2TakeChargePlugin), "SelectedAnimationLogger"), null, null);
            harmony.Patch(autoControlMethodSelector, /* new HarmonyMethod(typeof(HS2TakeChargePlugin), "SelectingAnimationLogger") */ null, new HarmonyMethod(typeof(HS2TakeChargePlugin), "SelectedAnimationLogger"), null, null);
        }

        static void SelectingAnimationLogger(List<HScene.AnimationListInfo>[] _listAnim, int _initiative, bool _isFirst)
        {
            ManualLogSource log = HS2TakeChargePlugin.Instance.Log;

        //    log.LogInfo(string.Format("Selecting Animation: Init: {0} isFirst: {1} Player Sex: {2} Futa: {3} FC: {4} MC: {5}", _initiative, _isFirst, HSceneManager.Instance.player.sex, HSceneManager.Instance.bFutanari, HSceneManager.Instance.Hscene.GetFemales()?.Length, HSceneManager.Instance.Hscene.GetMales()?.Length));
            int i = 0, j = 0, a = 0;
            foreach (List<HScene.AnimationListInfo> animList in _listAnim)
            {
                foreach (HScene.AnimationListInfo ali in animList)
                {
             //       log.LogInfo(string.Format("({7}-{5},{0}) Considering Id: {0} Pos: {6} Name: {1} Ctrl1: {2} Ctrl2: {3} Init: {4}", ali.id, ali.nameAnimation, ali.ActionCtrl.Item1, ali.ActionCtrl.Item2, ali.nInitiativeFemale, i, j, a));
                    j++;
                    a++;
                }
                j = 0;
                i++;                
            }
        }

        static void SelectedAnimationLogger(List<HScene.AnimationListInfo>[] _listAnim, int _initiative, bool _isFirst, ref HScene.StartMotion __result)
        {
          //  HS2TakeChargePlugin.Instance.Log.LogInfo(string.Format("Selected {0} with {1}", __result.id, __result.mode));
            if (_initiative == 2)
            {
                __result = RandomSelectAnimation(_listAnim, 3, 4, 5, 6, 7, 8);
            //    HS2TakeChargePlugin.Instance.Log.LogInfo(string.Format("Overrode to {0} with {1}", __result.id, __result.mode));
            }
        }

        static HScene.StartMotion RandomSelectAnimation(List<HScene.AnimationListInfo>[] animList, params int[] excludedCategories)
        {
            HAutoCtrl.AutoRandom autoRandom = new HAutoCtrl.AutoRandom();

            bool male = Singleton<HSceneManager>.Instance.player.sex == 0;
            bool futa = Singleton<HSceneManager>.Instance.bFutanari && !male;
            bool multipleFemales = Singleton<HSceneManager>.Instance.Hscene.GetFemales().Length > 1;
            bool fem1Present = Singleton<HSceneManager>.Instance.Hscene.GetFemales()[1] != null;
            bool multipleMales = Singleton<HSceneManager>.Instance.Hscene.GetMales().Length > 1;

            for (int mode = 0; mode < animList.Length; mode++)
            {
                for (int pos = 0; pos < animList[mode].Count; pos++)
                {
                    if (animList[mode][pos].nPositons.Contains(Singleton<HSceneFlagCtrl>.Instance.nPlace))
                    {
                        // Skip positions not available in location
                        continue;
                    }
                    if (mode == 4 && (male || futa))
                    {
                        //Skip masturbation if not female
                        continue;
                    }
                    if ( mode == 5 && (male || futa ) && !fem1Present)
                    {
                        // Don't peep without a female subject? 
                        continue;
                    }
                    if (excludedCategories.Contains(mode))
                    {
                        // weeding these out
                        continue;
                    }

                    // Staying with Illusion Random logic for consistency...
                    HAutoCtrl.AutoRandom.AutoRandomDate autoRandomDate = new HAutoCtrl.AutoRandom.AutoRandomDate();
                    autoRandomDate.mode = mode;
                    autoRandomDate.id = animList[mode][pos].id;
                    autoRandom.Add(autoRandomDate, 10f);
                }
            }

            HAutoCtrl.AutoRandom.AutoRandomDate selectedAutoRandom = autoRandom.Random();
            return new HScene.StartMotion(selectedAutoRandom.mode, selectedAutoRandom.id);
        }

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

        static void FixWheelValue(bool _start, ref float _wheel)
        {
            if (Singleton<HSceneFlagCtrl>.Instance.initiative > 0)
            {
                _wheel = 0.1f;
            }
        }

        static void FixWheelValue2(bool _start, ref float wheel)
        {
            if (Singleton<HSceneFlagCtrl>.Instance.initiative > 0)
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
        }

        static void SetupHScene(HScene __instance)
        {
            __instance.ctrlAuto.Load(null, 0);
        }
    }
}
