// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;


/// <summary>
/// Fixed color parameter.
/// </summary>
/// <remarks>
/// See also: \ref paramcolorfixed "Fixed color parameter reference"
/// </remarks>
[MBParameterInfo(Menu = "Color/Fixed",
                 CanAnimateBirth = MBParameterAnimationMode.Optional,
                 CanAnimateLife = MBParameterAnimationMode.Optional)]
public class MBParticleColorFixed : MBParticleColorBase
{
    /// <summary>
    /// The color mode to use
    /// </summary>
    public MBFixedColorMode ColorMode = MBFixedColorMode.Fixed;


    public override void OnBirth(MBParticle PT)
    {
        DeathReason = MBDeathReason.Color;

        switch (ColorMode){
          case MBFixedColorMode.Fixed:
                PT.Color = Colors[0].Color;
                break;
            case MBFixedColorMode.GradientKeyRandom:
                PT.Color = Colors[Random.Range(0, Colors.Count)].Color;
                break;
            case MBFixedColorMode.GradientRandom:
                PT.Color = GetGradientColor(Random.Range(0f, 1f),false);
                break;
            case MBFixedColorMode.Timeline:
                PT.Color = GetGradientColor(PT.Parent.AgePercent,false);
                break;
            case MBFixedColorMode.TimelineKeys:
                PT.Color = GetGradientColor(PT.Parent.AgePercent, true);
                break;
        }
        PT.mbColor = PT.Color;

    }

    public override void Reset()
    {
        base.Reset();
        if (ColorMode != MBFixedColorMode.Fixed && Colors.Count < 2)
            AddColorKey(1, Color.gray);
        AnimatedLife = true;
    }

    public override void Validate()
    {
        base.Validate();
        if (ColorMode != MBFixedColorMode.Fixed && Colors.Count < 2)
            AddColorKey(1, Color.gray);

    }

}
/// <summary>
/// Defines how the fixed color will be calculated
/// </summary>
public enum MBFixedColorMode
{
    /// <summary>
    /// Use a single color
    /// </summary>
    Fixed=0,
    /// <summary>
    /// Use a random key color
    /// </summary>
    GradientKeyRandom=1,
    /// <summary>
    /// Use a random color from the gradient
    /// </summary>
    GradientRandom=2,
    /// <summary>
    /// Use a color from the timeline gradient based on emitter's duration
    /// </summary>
    Timeline=3,
    /// <summary>
    /// Use a color from the timeline gradient based on emitter's duration, but limit to key colors
    /// </summary>
    TimelineKeys=4
}

