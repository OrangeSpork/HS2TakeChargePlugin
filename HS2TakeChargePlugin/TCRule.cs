using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;

namespace HS2TakeChargePlugin
{
    [XmlType("TCRuleType")]
    public enum TCRuleType {
        ALL,
        CATEGORY,
        ANIMATION
    }

    [XmlType("TCAnimationStage")]
    public enum TCAnimationStage
    {
        ALL,
        IDLE,
        SLOW_LOOP,
        FAST_LOOP,
        O_LOOP,
        ORGASM,
        POST_ORGASM

    }

    public enum TCRuleApplicationJudgement
    {
        INCLUDED,
        EXCLUDED,
        NEUTRAL
    }

    enum TCRuleJudgementType 
    {
        CHARACTER,
        ROOM
    }


    [XmlType("TCRuleApplication")]
    public class TCRuleApplication
    {
        [XmlArray("Includes")]
        [XmlArrayItem("Include")]
        public List<string> Includes = new List<string>();
        [XmlArray("Excludes")]
        [XmlArrayItem("Exclude")]
        public List<string> Excludes = new List<string>();

        public bool Included(string name) { return Includes.Contains(name); }
        public bool Excluded(string name) { return Excludes.Contains(name); }
    }

    [XmlType("TCAnimationTiming")]
    [XmlInclude(typeof(TCAnimationStage))]
    public class TCAnimationTiming
    {
        [XmlAttribute("Stage", DataType = "string")]
        public string stage;

        [XmlAttribute("MinSpeed", DataType = "float")]
        public float minSpeed;
        [XmlAttribute("MaxSpeed", DataType = "float")]
        public float maxSpeed;
        [XmlAttribute("SpeedLoopTime", DataType = "float")]
        public float speedLoopTime;
        [XmlAttribute("SpeedEase", DataType = "string")]
        public string speedEase;
        [XmlAttribute("LoopType", DataType = "string")]
        public string loopType;
        [XmlAttribute("MinFemaleOffset", DataType = "float")]
        public float minFemaleOffset;
        [XmlAttribute("MaxFemaleOffset", DataType = "float")]
        public float maxFemaleOffset;
        [XmlAttribute("FemaleOffsetLoopTime", DataType = "float")]
        public float femaleOffsetLoopTime;
        [XmlAttribute("FemaleOffsetEase", DataType = "string")]
        public string femaleOffsetEase;

        public TCAnimationTiming()
        {
        }

        public TCAnimationTiming(string stage, float minSpeed, float maxSpeed, float speedLoopTime, string speedEase, string loopType, float minFemaleOffset, float maxFemaleOffset, float femaleOffsetLoopTime, string femaleOffsetEase)
        {
            this.stage = stage;
            this.minSpeed = minSpeed;
            this.maxSpeed = maxSpeed;
            this.speedLoopTime = speedLoopTime;
            this.speedEase = speedEase;
            this.loopType = loopType;
            this.minFemaleOffset = minFemaleOffset;
            this.maxFemaleOffset = maxFemaleOffset;
            this.femaleOffsetLoopTime = femaleOffsetLoopTime;
            this.femaleOffsetEase = femaleOffsetEase;
        }

        public LoopType LoopTypeEnum()
        {
            try
            {
                return (LoopType)Enum.Parse(typeof(LoopType), loopType);
            }
            catch
            {
                HS2TakeChargePlugin.Instance.Log.LogWarning(string.Format("Invalid Loop Type: {0}, reverting to default", loopType));
                return LoopType.Yoyo;
            }
        }

        public Ease FemaleOffsetEaseEnum()
        {
            try
            {
                return (Ease)Enum.Parse(typeof(Ease), femaleOffsetEase);
            }
            catch
            {
                HS2TakeChargePlugin.Instance.Log.LogWarning(string.Format("Invalid Offset Ease: {0}, reverting to default", speedEase));
                return Ease.Linear;
            }
        }

        public Ease SpeedEaseEnum()
        {
            try
            {
                return (Ease)Enum.Parse(typeof(Ease), speedEase);
            }
            catch
            {
                HS2TakeChargePlugin.Instance.Log.LogWarning(string.Format("Invalid Speed Ease: {0}, reverting to default", speedEase));
                return Ease.Linear;
            }
        }
        
    }

    [XmlType("TCRule")]
    [XmlInclude(typeof(TCRuleApplication))]
    [XmlInclude(typeof(TCAnimationTiming))]
    [XmlInclude(typeof(TCRuleType))]
    public class TCRule
    {
        [XmlAttribute("Rule", DataType = "string")]
        public String Rule;
        [XmlAttribute("RuleType", DataType = "string")]
        public string RuleType;
        [XmlAttribute("ExcludeAlways", DataType = "boolean")]
        public bool ExcludeAlways;

        public TCRuleApplication CharacterApplication = new TCRuleApplication();
        public TCRuleApplication RoomApplication = new TCRuleApplication();

        [XmlArray("AnimationTimings")]
        [XmlArrayItem("AnimationTiming")]
        public List<TCAnimationTiming> AnimationTiming = new List<TCAnimationTiming>();

        public TCRule AddTiming(TCAnimationTiming timing)
        {
            AnimationTiming.Add(timing);
            return this;
        }
    }

    [XmlRoot("TCRuleSet")]
    [XmlInclude(typeof(TCRule))]
    public class TCRuleSet
    {
        [XmlArray("TCRules")]
        [XmlArrayItem("TCRule")]
        public List<TCRule> Rules = new List<TCRule>();        

        public TCAnimationTiming Timing(string animationName, string categoryName, TCAnimationStage stage)
        {
            TCAnimationTiming timing = null;

            timing = ExtractStageTiming(RuleByTypeAndName(TCRuleType.ANIMATION, animationName), stage);
            if (timing == null)
            {
                timing = ExtractStageTiming(RuleByTypeAndName(TCRuleType.CATEGORY, categoryName), stage);
            }
            if (timing == null)
            {
                timing = ExtractStageTiming(RuleByTypeAndName(TCRuleType.ALL, TCRuleType.ALL.ToString()), stage);
            }
            if (timing == null)
            {
                timing = HS2TakeChargePlugin.Instance.GenerateDefaultRuleset().Timing(animationName, categoryName, stage);
            }
            return timing;
        }

        private TCAnimationTiming ExtractStageTiming(TCRule rule, TCAnimationStage stage)
        {
            if (rule == null)
            {
                return null;
            }
            else
            {
                TCAnimationTiming appliedTiming = rule.AnimationTiming.Find((timing) => timing.stage.Equals(stage.ToString(), StringComparison.OrdinalIgnoreCase));
                if (appliedTiming == null)
                {
                    appliedTiming = rule.AnimationTiming.Find((timing) => timing.stage.Equals(TCAnimationStage.ALL.ToString(), StringComparison.OrdinalIgnoreCase));
                }
                return appliedTiming;
            }
        }


        public TCRuleApplicationJudgement CharacterRuleJudgement(string characterName, string animationName, string categoryName)
        {
            TCRuleApplicationJudgement judgement = JudgeApplication(RuleByTypeAndName(TCRuleType.ANIMATION, animationName), TCRuleJudgementType.CHARACTER, characterName);
            if (judgement != TCRuleApplicationJudgement.NEUTRAL)
            {
                return judgement;
            } 
            else
            {
                return JudgeApplication(RuleByTypeAndName(TCRuleType.CATEGORY, categoryName), TCRuleJudgementType.CHARACTER, characterName);
            }
        }

        public TCRuleApplicationJudgement RoomRuleJudgement(string roomName, string animationName, string categoryName)
        {
            TCRuleApplicationJudgement judgement = JudgeApplication(RuleByTypeAndName(TCRuleType.ANIMATION, animationName), TCRuleJudgementType.ROOM, roomName);
            if (judgement != TCRuleApplicationJudgement.NEUTRAL)
            {
                return judgement;
            }
            else
            {
                return JudgeApplication(RuleByTypeAndName(TCRuleType.CATEGORY, categoryName), TCRuleJudgementType.ROOM, roomName);
            }
        }

        public bool ExcludeAlwaysCheck(string animationName, string categoryName)
        {
            TCRule rule = RuleByTypeAndName(TCRuleType.ANIMATION, animationName);
            if (rule != null)
            {
                return rule.ExcludeAlways;
            }
            else
            {
                rule = RuleByTypeAndName(TCRuleType.CATEGORY, categoryName);
                if (rule != null)
                {
                    return rule.ExcludeAlways;
                }
                else
                {
                    return false;
                }
            }
        }

        private TCRuleApplicationJudgement JudgeApplication(TCRule rule, TCRuleJudgementType judgmentType, string name)
        {            
            if (rule == null)
            {
                return TCRuleApplicationJudgement.NEUTRAL;
            }
            if ((judgmentType == TCRuleJudgementType.CHARACTER ? rule.CharacterApplication : rule.RoomApplication).Included(name)) 
            {
                return TCRuleApplicationJudgement.INCLUDED;
            }
            else if ((judgmentType == TCRuleJudgementType.CHARACTER ? rule.CharacterApplication : rule.RoomApplication).Excluded(name))
            {
                return TCRuleApplicationJudgement.EXCLUDED;
            }
            else
            {
                return TCRuleApplicationJudgement.NEUTRAL;
            }
        }

        private TCRule RuleByTypeAndName(TCRuleType ruleType, string name)
        {
            return Rules.FindAll((rule) => rule.RuleType.Equals(ruleType.ToString())).Find((rule) => rule.Rule.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
