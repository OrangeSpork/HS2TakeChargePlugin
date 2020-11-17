using AIChara;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using HarmonyLib;
using HS2TakeChargePlugin.Hooks;
using Manager;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
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
        public const string Version = "1.3.0";

        public static ConfigEntry<KeyboardShortcut> TakeChargeKey { get; set; }
        public static ConfigEntry<KeyboardShortcut> AutoKey { get; set; }

        public static ConfigEntry<KeyboardShortcut> StopMale { get; set; }
        public static ConfigEntry<KeyboardShortcut> StopFemale { get; set; }

        public static ConfigEntry<bool> ResetToIdleOnChange { get; set; }
        public static ConfigEntry<bool> ResetArousalOnChange { get; set; }

        public static ConfigEntry<bool> AllowAllPositions { get; set; }

        public static ConfigEntry<bool> EnableSpeedOverride { get; set; }

        public static ConfigEntry<bool> AlwaysEnableSpeedOverride { get; set; }

        public static ConfigEntry<string> AnimRulesFile { get; set; }

        public static ConfigEntry<int> StartWaitMin { get; set; }
        public static ConfigEntry<int> StartWaitMax { get; set; }
        public static ConfigEntry<int> RestartWaitMin { get; set; }
        public static ConfigEntry<int> RestartWaitMax { get; set; }
        public static ConfigEntry<int> LoopChangeWaitMin { get; set; }
        public static ConfigEntry<int> LoopChangeWaitMax { get; set; }
        public static ConfigEntry<int> PositionChangeWaitMin { get; set; }
        public static ConfigEntry<int> PositionChangeWaitMax { get; set; }
        public static ConfigEntry<int> PulloutWaitMin { get; set; }
        public static ConfigEntry<int> PulloutWaitMax { get; set; }
        public static ConfigEntry<int> SpankingWaitMin { get; set; }
        public static ConfigEntry<int> SpankingWaitMax { get; set; }
        public static ConfigEntry<int> ChanceToRestartPosition { get; set; }
        public static ConfigEntry<int> InsertedChanceToPullOut { get; set; }
        public static ConfigEntry<int> NonInsertedChanceToPullOut { get; set; }


        public TCRuleSet RuleSet { get; set; }

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

            ResetToIdleOnChange = Config.Bind("Options", "Reset to Idle on Pos Change", false, new ConfigDescription("Reset to Idle loop animation when changing positions"));
            ResetArousalOnChange = Config.Bind("Options", "Reset Arousal Meters on Pos Change", false, new ConfigDescription("Reset arousal gauge when changing positions"));

            AllowAllPositions = Config.Bind("Options", "Auto Mode - Ignore Location Animation Limits", false, new ConfigDescription("Some Animations Won't Fit the Location..."));

            EnableSpeedOverride = Config.Bind("Options", "Enable Auto Speed Override", false, new ConfigDescription("Override normal speed control/limits in auto mode"));
            AlwaysEnableSpeedOverride = Config.Bind("Options", "Speed Override All Modes", false, new ConfigDescription("Override normal speed control/limits in all modes"));
            AnimRulesFile = Config.Bind("Options", "Anim Rules File", "UserData/HS2TakeChargeRules.xml", new ConfigDescription("Rules File Location"));


            StartWaitMin = Config.Bind("Timings", "Start Wait Min", 3, new ConfigDescription("Minimum Time (Seconds) to Wait at Start of Position [Change Requires Restart]"));
            StartWaitMax = Config.Bind("Timings", "Start Wait Max", 7, new ConfigDescription("Maximum Time (Seconds) to Wait at Start of Position [Change Requires Restart]"));
            RestartWaitMin = Config.Bind("Timings", "Restart Wait Min", 10, new ConfigDescription("Minimum Time (Seconds) to Wait before Restarting Position [Change Requires Restart]"));
            RestartWaitMax = Config.Bind("Timings", "Restart Wait Max", 15, new ConfigDescription("Maximum Time (Seconds) to Wait before Restartign Position [Change Requires Restart]"));
            LoopChangeWaitMin = Config.Bind("Timings", "Loop Change Wait Min", 4, new ConfigDescription("Minimum Time (Seconds) before switching Loops (Slow->Fast or Fast->Slow) [Change Requires Restart]"));
            LoopChangeWaitMax = Config.Bind("Timings", "Loop Change Wait Max", 10, new ConfigDescription("Maximum Time (Seconds) before switching Loops (Slow->Fast or Fast->Slow) [Change Requires Restart]"));
            PositionChangeWaitMin = Config.Bind("Timings", "Position Change Wait Min", 15, new ConfigDescription("Minimum Time (Seconds) before switching Positions (different H Scene) [Change Requires Restart]"));
            PositionChangeWaitMax = Config.Bind("Timings", "Position Change Wait Max", 40, new ConfigDescription("Maximum Time (Seconds) before switching Positions (different H Scene) [Change Requires Restart]"));
            PulloutWaitMin = Config.Bind("Timings", "Pullout Wait Min", 6, new ConfigDescription("Minimum Time (Seconds) wait before pulling out after finish (if relevant). [Change Requires Restart]"));
            PulloutWaitMax = Config.Bind("Timings", "Pullout Wait Max", 9, new ConfigDescription("Maximum Time (Seconds) wait before pullout out after finish (if relevant). [Change Requires Restart]"));
            SpankingWaitMin = Config.Bind("Timings", "Spanking Wait Min", 1, new ConfigDescription("Minimum Time (Seconds) between spanks [Change Requires Restart]"));
            SpankingWaitMax = Config.Bind("Timings", "Spanking Wait Max", 4, new ConfigDescription("Maximum Time (Seconds) between spanks [Change Requires Restart]"));
            ChanceToRestartPosition = Config.Bind("Timings", "% Position Change", 80, new ConfigDescription("Percentage chance to change positions after finish [Change Requires Restart]"));
            InsertedChanceToPullOut = Config.Bind("Timings", "% Chance to Pull Out", 70, new ConfigDescription("Percentage chance to pull out after finish (Doesn't effect finish type selection). Applies only if inserted. [Change Requires Restart]"));
            NonInsertedChanceToPullOut = Config.Bind("Timings", "% Chance to Pull Out When Not Inserted", 30, new ConfigDescription("<facepalm> - Apparently a position can have a Pull Out animation even when not inserted. % chance to play that if relevant. [Change Requires Restart]"));

            PatchMe();

            ReadRuleFile();
            StartCoroutine(UpdateRuleFile());
        }

        private void ReadRuleFile()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(TCRuleSet));
            if (File.Exists(AnimRulesFile.Value))
            {
                using (XmlReader reader = XmlReader.Create(File.OpenRead(AnimRulesFile.Value)))
                {
                    RuleSet = (TCRuleSet)xmlSerializer.Deserialize(reader);
                    reader.Close();
                }
            }
            else
            {
                RuleSet = GenerateDefaultRuleset();                
            }
        }

        public TCRuleSet GenerateDefaultRuleset()
        {
            Log.LogInfo("Generating Default Ruleset");

            TCRuleSet defaults = new TCRuleSet();
            TCRule peepingExclusionRule = new TCRule();
            peepingExclusionRule.Rule = PositionCategories.PEEPING.ToString();
            peepingExclusionRule.RuleType = TCRuleType.CATEGORY.ToString();
            peepingExclusionRule.ExcludeAlways = true;
            defaults.Rules.Add(peepingExclusionRule);

            TCRule defaultTimingRule = new TCRule();
            defaultTimingRule.Rule = "ALL";
            defaultTimingRule.RuleType = TCRuleType.ALL.ToString();
            defaultTimingRule
                .AddTiming(new TCAnimationTiming(TCAnimationStage.IDLE.ToString(), 0, 0, 1f, Ease.Linear.ToString(), LoopType.Yoyo.ToString(), 0f, 0f, 1f, Ease.Linear.ToString()))
                .AddTiming(new TCAnimationTiming(TCAnimationStage.SLOW_LOOP.ToString(), 0f, 1.75f, 2.5f, Ease.InOutCubic.ToString(), LoopType.Yoyo.ToString(), -.1f, 0.1f, 1.5f, Ease.InOutCubic.ToString()))
                .AddTiming(new TCAnimationTiming(TCAnimationStage.FAST_LOOP.ToString(), 1f, 2.75f, 2.5f, Ease.InOutCubic.ToString(), LoopType.Yoyo.ToString(), -.1f, 0.1f, 1.5f, Ease.InOutCubic.ToString()))
                .AddTiming(new TCAnimationTiming(TCAnimationStage.O_LOOP.ToString(), 1f, 2f, 2.5f, Ease.InOutCubic.ToString(), LoopType.Yoyo.ToString(), -.1f, 0.1f, 1.5f, Ease.InOutCubic.ToString()))
                .AddTiming(new TCAnimationTiming(TCAnimationStage.ORGASM.ToString(), 0, 0, 1f, Ease.Linear.ToString(), LoopType.Yoyo.ToString(), 0f, 0f, 1f, Ease.Linear.ToString()))
                .AddTiming(new TCAnimationTiming(TCAnimationStage.POST_ORGASM.ToString(), 0, 0, 1f, Ease.Linear.ToString(), LoopType.Yoyo.ToString(), 0f, 0f, 1f, Ease.Linear.ToString()));           
            defaults.Rules.Add(defaultTimingRule);

            TCRule caressTimingRule = new TCRule();
            caressTimingRule.Rule = PositionCategories.CARESS.ToString();
            caressTimingRule.RuleType = TCRuleType.CATEGORY.ToString();
            caressTimingRule
                .AddTiming(new TCAnimationTiming(TCAnimationStage.IDLE.ToString(), 0, 0, 1f, Ease.Linear.ToString(), LoopType.Yoyo.ToString(), 0f, 0f, 1f, Ease.Linear.ToString()))
                .AddTiming(new TCAnimationTiming(TCAnimationStage.SLOW_LOOP.ToString(), -.5f, 0.5f, 2.5f, Ease.InOutCubic.ToString(), LoopType.Yoyo.ToString(), -0.1f, 0.1f, 1.5f, Ease.InOutCubic.ToString()))
                .AddTiming(new TCAnimationTiming(TCAnimationStage.FAST_LOOP.ToString(), 0.5f, 1.5f, 2.5f, Ease.InOutCubic.ToString(), LoopType.Yoyo.ToString(), -0.1f, 0.1f, 1.5f, Ease.InOutCubic.ToString()))
                .AddTiming(new TCAnimationTiming(TCAnimationStage.O_LOOP.ToString(), 0f, 1f, 2.5f, Ease.InOutCubic.ToString(), LoopType.Yoyo.ToString(), -0.1f, 0.1f, 1.5f, Ease.InOutCubic.ToString()))
                .AddTiming(new TCAnimationTiming(TCAnimationStage.ORGASM.ToString(), 0, 0, 1f, Ease.Linear.ToString(), LoopType.Yoyo.ToString(), 0f, 0f, 1f, Ease.Linear.ToString()))
                .AddTiming(new TCAnimationTiming(TCAnimationStage.POST_ORGASM.ToString(), 0, 0, 1f, Ease.Linear.ToString(), LoopType.Yoyo.ToString(), 0f, 0f, 1f, Ease.Linear.ToString()));
            defaults.Rules.Add(caressTimingRule);

            return defaults;
        }

        public bool AnimationOverrideActive()
        {
            bool enabledOverride = EnableSpeedOverride.Value;
            bool alwaysOverride = AlwaysEnableSpeedOverride.Value;
            int initiative = Singleton<HSceneFlagCtrl>.Instance.initiative;

            if (alwaysOverride)
            {
                return true;
            }
            else if (enabledOverride && initiative == 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private IEnumerator UpdateRuleFile()
        {
            yield return new WaitUntil(() => (HSceneManager.HResourceTables != null && HSceneManager.HResourceTables.endHLoad && Singleton<BaseMap>.Instance != null && SingletonInitializer<BaseMap>.initialized ));

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(TCRuleSet));
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.CloseOutput = true;

            using (XmlWriter xmlWriter = XmlWriter.Create(File.Open(AnimRulesFile.Value, FileMode.Create), xmlWriterSettings))
            {
                if (RuleSet != null)
                {
                    xmlSerializer.Serialize(xmlWriter, RuleSet);
                }
                xmlWriter.WriteComment("  Rules provide detailed configuration overrides that override the defaults specified in plugin settings." +
                    "\nRule: Either a category or a particular animation or ALL (ANIMATION > CATEGORY > ALL (Defaults - AnimationTimings only)" +
                    "\nRule Type: One of ANIMATION, CATEGORY, ALL - Animation must be a particular animation name (see list below), Category must be a particular category name (see list below), All applies to everything" +
                    "\nCharacters: List of Character (by name, must be precise match) Excluded animations or categories will not be chosen for this char. If char has any Included animations and categories, only these will be picked from. ALL rule inclusions/exclusions are ignored." +
                    "\nRooms: List of Rooms (by name, see list below) Excluded animations or categories will not be chosen for this room. If room has any Included animations and categories, only these will be picked from. ALL rule inclusions/exclusions are ignored." +
                    "\nAnimationTimings: Detailed animation timing controls. Stage must be ALL or one of the specific stages, listed below. Ease and Loop type values accepted also listed below."
                    );
                xmlWriter.WriteComment(" Available Categories: [" + string.Join(",", Enum.GetNames(typeof(PositionCategories))) + "]");
                xmlWriter.WriteComment(" Available Stage Names: [" + string.Join(",", Enum.GetNames(typeof(TCAnimationStage))) + "]");
                xmlWriter.WriteComment(" Available Ease Types: [" + string.Join(",", Enum.GetNames(typeof(Ease))) + "]");
                xmlWriter.WriteComment(" Available Loop Types: [" + string.Join(",", Enum.GetNames(typeof(LoopType))) + "]");
                xmlWriter.WriteComment(" Available Room Names: [" + string.Join(",", AllRoomNames()) + "]");
                xmlWriter.WriteComment(" Sample Rule: \n" +
                    @"    <TCRule Rule=""SERVICE"" RuleType=""CATEGORY"" ExcludeAlways=""false"">
      <CharacterApplication>
        <Includes>
          <Include>CindyOnlyDoesService</Include>
        </Includes>
        <Excludes>
          <Exclude>DanaNeverDoesService</Exclude>
          <Exclude>SusanNeverDoesService</Exclude>
        </Excludes>
      </CharacterApplication>
      <RoomApplication>
        <Includes>
          <Include>OnlyServiceInTheGym</Include>
        </Includes>
        <Excludes>
          <Exclude>NoServiceInTheBath</Exclude>
        </Excludes>
      </RoomApplication>
      <AnimationTimings>
        <AnimationTiming Stage = ""ALL"" MinSpeed = ""0.5"" MaxSpeed = ""2"" SpeedEase = ""InOutCirc"" SpeedLoopTime = ""1.5"" LoopType = ""Yoyo"" MinFemaleOffset = ""-0.3"" MaxFemaleOffset = ""0.5"" FemaleOffsetEase = ""InOutSine"" femaleOffsetLoopTime = ""2.5"" />
      </AnimationTimings>
    </TCRule> ");
                Dictionary<PositionCategories, List<string>> animations = AllAnimationNames();
                foreach (PositionCategories cat in animations.Keys)
                {
                    xmlWriter.WriteComment(" Animations in Category " + cat + ": [" + string.Join(",", animations[cat]) + "]");
                }

                xmlWriter.Flush();
                xmlWriter.Close();
            }
        }

        Dictionary<PositionCategories, List<string>> AllAnimationNames()
        {
            Dictionary<PositionCategories, List<String>> animationDictionary = new Dictionary<PositionCategories, List<String>>();
            foreach (PositionCategories category in Enum.GetValues(typeof(PositionCategories))) {
                animationDictionary.Add(category, new List<string>());
            }

            foreach (List<HScene.AnimationListInfo> animInfoCategory in HSceneManager.HResourceTables.lstAnimInfo)
            {
                foreach (HScene.AnimationListInfo info in animInfoCategory)
                {
              //      Log.LogInfo(info.ActionCtrl.Item1 + ":" + info.ActionCtrl.Item2 + " " + info.nameAnimation);
                    switch (DetermineModeByActionCtrl(info.ActionCtrl.Item1, info.ActionCtrl.Item2))
                    {
                        case 0:
                            animationDictionary[PositionCategories.CARESS].Add(info.nameAnimation);
                            break;
                        case 1:
                            animationDictionary[PositionCategories.SERVICE].Add(info.nameAnimation);
                            break;
                        case 2:
                            animationDictionary[PositionCategories.SEX].Add(info.nameAnimation);
                            break;
                        case 3:
                            animationDictionary[PositionCategories.SPANKING].Add(info.nameAnimation);
                            break;
                        case 4:
                            animationDictionary[PositionCategories.MASTURBATION].Add(info.nameAnimation);
                            break;
                        case 5:
                            animationDictionary[PositionCategories.PEEPING].Add(info.nameAnimation);
                            break;
                        case 6:
                            animationDictionary[PositionCategories.LESBIAN].Add(info.nameAnimation);
                            break;
                        case 7:
                            animationDictionary[PositionCategories.MULTI_F2M1].Add(info.nameAnimation);
                            break;
                        case 8:
                            animationDictionary[PositionCategories.MULTI_F1M2].Add(info.nameAnimation);
                            break;
                    }
                }
            }
            return animationDictionary;
        }

        string[] AllRoomNames()
        {
            return BaseMap.infoTable.Values.Select(mi => mi.MapNames[0]).ToArray();
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
                    ReadRuleFile();
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
                    ReadRuleFile();
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
            harmony.PatchAll(typeof(GeneralHooks));
            harmony.PatchAll(typeof(AibuHooks));
            harmony.PatchAll(typeof(HoushiHooks));
            harmony.PatchAll(typeof(LesHooks));
            harmony.PatchAll(typeof(MasturbationHooks));
            harmony.PatchAll(typeof(MultiplayF1M2Hooks));
            harmony.PatchAll(typeof(MultiplayF2M1Hooks));
            harmony.PatchAll(typeof(PeepingHooks));
            harmony.PatchAll(typeof(SonyuHooks));
            harmony.PatchAll(typeof(SpnkingHooks));
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

        public HScene.StartMotion RandomSelectAnimation(List<HScene.AnimationListInfo>[] animList)
        {
            HAutoCtrl.AutoRandom inclusionAutoRandom = new HAutoCtrl.AutoRandom();
            HAutoCtrl.AutoRandom autoRandom = new HAutoCtrl.AutoRandom();

            bool male = Singleton<HSceneManager>.Instance.player.sex == 0;
            bool futa = Singleton<HSceneManager>.Instance.bFutanari && !male;
            bool multipleFemales = Singleton<HSceneManager>.Instance.Hscene.GetFemales().Length > 1;
            bool fem1Present = Singleton<HSceneManager>.Instance.Hscene.GetFemales()[1] != null;
            bool multipleMales = Singleton<HSceneManager>.Instance.Hscene.GetMales().Length > 1;

            string femaleName1 = Singleton<HSceneManager>.Instance.Hscene.GetFemales()[0] == null ? "" : Singleton<HSceneManager>.Instance.Hscene.GetFemales()[0].fileParam.fullname;
            string femaleName2 = Singleton<HSceneManager>.Instance.Hscene.GetFemales()[1] == null ? "" : Singleton<HSceneManager>.Instance.Hscene.GetFemales()[1].fileParam.fullname;
            string roomName = BaseMap.infoTable[Singleton<HSceneManager>.Instance.mapID].MapNames[0];
      //      Log.LogInfo(string.Format("Now Entering: {0} {3} with {1} {2}", Singleton<HSceneManager>.Instance.mapID, femaleName1, femaleName2, roomName));


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

                    TCRuleApplicationJudgement female1CharacterJudgement = Singleton<HSceneManager>.Instance.Hscene.GetFemales()[0] == null ? TCRuleApplicationJudgement.NEUTRAL : RuleSet.CharacterRuleJudgement(femaleName1, animList[info1][pos].nameAnimation, ((PositionCategories)mode).ToString());
                    TCRuleApplicationJudgement female2CharacterJudgement = Singleton<HSceneManager>.Instance.Hscene.GetFemales()[1] == null ? TCRuleApplicationJudgement.NEUTRAL : RuleSet.CharacterRuleJudgement(femaleName2, animList[info1][pos].nameAnimation, ((PositionCategories)mode).ToString());
                    TCRuleApplicationJudgement roomJudgement = RuleSet.RoomRuleJudgement(roomName, animList[info1][pos].nameAnimation, ((PositionCategories)mode).ToString());

                    if (RuleSet.ExcludeAlwaysCheck(animList[info1][pos].nameAnimation, ((PositionCategories)mode).ToString()) || female1CharacterJudgement == TCRuleApplicationJudgement.EXCLUDED || female2CharacterJudgement == TCRuleApplicationJudgement.EXCLUDED || roomJudgement == TCRuleApplicationJudgement.EXCLUDED)
                    {
             //           Log.LogInfo(string.Format("TC Rule Judgement: Excluding: {0} ({1}, {2}, {3})", animList[info1][pos].nameAnimation, femaleName1, femaleName2, roomName));
                        continue;
                    }

                    // Staying with Illusion Random logic for consistency...
                    HAutoCtrl.AutoRandom.AutoRandomDate autoRandomDate = new HAutoCtrl.AutoRandom.AutoRandomDate();
                    autoRandomDate.mode = info1;
                    autoRandomDate.id = animList[info1][pos].id;
                    if (female1CharacterJudgement == TCRuleApplicationJudgement.INCLUDED || female2CharacterJudgement == TCRuleApplicationJudgement.INCLUDED || roomJudgement == TCRuleApplicationJudgement.INCLUDED)
                    {
            //            Log.LogInfo(string.Format("TC Rule Judgement: Including: {0} ({1}, {2}, {3})", animList[info1][pos].nameAnimation, femaleName1, femaleName2, roomName));
                        inclusionAutoRandom.Add(autoRandomDate, 10f);
                    }
                    else
                    {                        
                        autoRandom.Add(autoRandomDate, 10f);
                    }                    
                }
            }

            if (!inclusionAutoRandom.IsEmpty())
            {
                HAutoCtrl.AutoRandom.AutoRandomDate selectedAutoRandom = inclusionAutoRandom.Random();
                return new HScene.StartMotion(selectedAutoRandom.mode, selectedAutoRandom.id);
            }
            else
            {
                HAutoCtrl.AutoRandom.AutoRandomDate selectedAutoRandom = autoRandom.Random();
                return new HScene.StartMotion(selectedAutoRandom.mode, selectedAutoRandom.id);
            }
        }

    }

    enum PositionCategories : int
    {
        CARESS = 0,
        SERVICE = 1,
        SEX = 2,
        SPANKING = 3,
        MASTURBATION = 4,
        PEEPING = 5,
        LESBIAN = 6,
        MULTI_F2M1 = 7,
        MULTI_F1M2 = 8
    }
}
