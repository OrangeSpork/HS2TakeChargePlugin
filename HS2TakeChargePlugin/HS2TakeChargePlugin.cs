using AIChara;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Manager;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
        public const string Version = "1.1.0";

        public static ConfigEntry<KeyboardShortcut> TakeChargeKey { get; set; }
        public static ConfigEntry<KeyboardShortcut> AutoKey { get; set; }

        public static ConfigEntry<KeyboardShortcut> StopMale { get; set; }
        public static ConfigEntry<KeyboardShortcut> StopFemale { get; set; }

        public static ConfigEntry<bool> NoSpanking { get; set; }
        public static ConfigEntry<bool> AllowAllPositions { get; set; }


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

            StopMale = Config.Bind("Hotkeys", "Stop Male Arousal", new KeyboardShortcut(KeyCode.None));
            StopFemale = Config.Bind("Hotkeys", "Stop Female Arousal", new KeyboardShortcut(KeyCode.None));

            NoSpanking = Config.Bind("Options", "Auto Mode - No Spanking Animations", false, new ConfigDescription("Engage White Knight Mode"));
            AllowAllPositions = Config.Bind("Options", "Auto Mode - Ignore Location Animation Limits", false, new ConfigDescription("Some Animations Won't Fit the Location..."));

            PatchMe();
        }

        void Update()
        {
            if (TakeChargeKey.Value.IsUp())
            {
                if (Singleton<HSceneFlagCtrl>.Instance.initiative == 2)
                {
                    Log.LogMessage("Auto H: Disabled");
                    Singleton<HSceneFlagCtrl>.Instance.initiative = 0;
                }
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
                if (Singleton<HSceneFlagCtrl>.Instance.initiative == 0 || Singleton<HSceneFlagCtrl>.Instance.initiative == 1)
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
            if (StopMale.Value.IsUp())
            {
                Singleton<HSceneSprite>.Instance.objGaugeLockM.isOn = !Singleton<HSceneSprite>.Instance.objGaugeLockM.isOn;
                Log.LogMessage("Male Arousal " + (Singleton<HSceneFlagCtrl>.Instance.stopFeelMale ? "Locked" : "Unlocked"));
            }
            if (StopFemale.Value.IsUp())
            {
                Singleton<HSceneSprite>.Instance.objGaugeLockF.isOn = !Singleton<HSceneSprite>.Instance.objGaugeLockF.isOn;
                Log.LogMessage("Female Arousal " + (Singleton<HSceneFlagCtrl>.Instance.stopFeelFemale ? "Locked" : "Unlocked"));
            }

        }

        public void PatchMe()
        {
            Harmony harmony = new Harmony(GUID);
            harmony.PatchAll(typeof(Hooks));
        }

        private static FieldInfo modeControlFieldInfo = AccessTools.Field(typeof(HScene), "modeCtrl");
        public int GetCurrentModeControl()
        {
            return (int)modeControlFieldInfo.GetValue(Singleton<HSceneManager>.Instance.Hscene);
        }        

        public int DetermineModeByActionCtrl(int info1, int info2)
        {
            switch (info1)
            {
                case 0: return 0;
                case 1: return 1;
                case 2: return 2;
                case 3:
                    {
                        switch (info2)
                        {
                            case 0: return 2;
                            case 1: return 2;
                            case 2: return 3;
                            case 3: return 0;
                            case 4: return 4;
                            case 5: return 5;
                            case 6: return 5;
                            case 7: return 2;
                            default: return 2;
                        }
                    }
                case 4: return 6;
                case 5: return 7;
                case 6: return 8;
                default: return 0;
            }
        }

        public HScene.StartMotion RandomSelectAnimation(List<HScene.AnimationListInfo>[] animList, params int[] excludedCategories)
        {
            HAutoCtrl.AutoRandom autoRandom = new HAutoCtrl.AutoRandom();

            bool male = Singleton<HSceneManager>.Instance.player.sex == 0;
            bool futa = Singleton<HSceneManager>.Instance.bFutanari && !male;
            bool multipleFemales = Singleton<HSceneManager>.Instance.Hscene.GetFemales().Length > 1;
            bool fem1Present = Singleton<HSceneManager>.Instance.Hscene.GetFemales()[1] != null;
            bool multipleMales = Singleton<HSceneManager>.Instance.Hscene.GetMales().Length > 1;

            for (int info1 = 0; info1 < animList.Length; info1++)
            {
                for (int pos = 0; pos < animList[info1].Count; pos++)
                {
                    int mode = DetermineModeByActionCtrl(animList[info1][pos].ActionCtrl.Item1, animList[info1][pos].ActionCtrl.Item2);
                    if (!animList[info1][pos].nPositons.Contains(Singleton<HSceneFlagCtrl>.Instance.nPlace))
                    {
                        // Skip positions not available in location
                        if (!AllowAllPositions.Value)
                          continue;
                    }
                    if (mode == 4 && (male || futa))
                    {
                        //Skip masturbation if not female
                        continue;
                    }
                    if (mode == 5 && (male || futa ) && !fem1Present)
                    {
                        // Don't peep without a female subject? 
                        continue;
                    }
                    if (!multipleFemales && (mode == 6 || mode == 7))
                    {
                        // need multiple females for les/f2 scenes
                        continue;
                    }
                    if (!multipleMales && mode == 8)
                    {
                        // need multiple makes for m2 scenes
                        continue;
                    }
                    if (excludedCategories.Contains(mode))
                    {
                        // weeding these out
                        continue;
                    }

                    // Staying with Illusion Random logic for consistency...
                    HAutoCtrl.AutoRandom.AutoRandomDate autoRandomDate = new HAutoCtrl.AutoRandom.AutoRandomDate();
                    autoRandomDate.mode = info1;
                    autoRandomDate.id = animList[info1][pos].id;
                    autoRandom.Add(autoRandomDate, 10f);
                }
            }

            HAutoCtrl.AutoRandom.AutoRandomDate selectedAutoRandom = autoRandom.Random();
            return new HScene.StartMotion(selectedAutoRandom.mode, selectedAutoRandom.id);
        }

    }
}
