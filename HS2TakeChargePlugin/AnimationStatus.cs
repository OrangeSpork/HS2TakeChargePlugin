using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

namespace HS2TakeChargePlugin
{
    static class AnimationStatus
    {
        public static string PlayingAnimation;
        public static float FemaleOffset;
        public static float MaleSpeed;
        public static float FemaleSpeed;
        public static Sequence AnimSequence;
        public static TweenerCore<float, float, FloatOptions> FemaleSpeedTween;
        public static TweenerCore<float, float, FloatOptions> MaleSpeedTween;
        public static TweenerCore<float, float, FloatOptions> FemaleOffsetTween;
    }
}
