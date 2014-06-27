// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Timeline color parameter
/// </summary>
/// <remarks>
/// See also: \ref paramcolortimeline "Timeline color reference"
/// </remarks>
[MBParameterInfo(Menu = "Color/Timeline",
                 CanAnimateBirth = MBParameterAnimationMode.None,
                 CanAnimateLife = MBParameterAnimationMode.Mandatory)]
public class MBParticleColorTimeline : MBParticleColorBase
{

    public override void OnBirth(MBParticle PT)
    {
        base.OnBirth(PT);
        DeathReason = MBDeathReason.Color;
        PT.Color = Colors[0].Color;
        PT.mbColor = PT.Color;
    }

    public override bool OnLifetime(MBParticle PT)
    {
        // PT.mbColor store's the origional color!
        PT.mbColor = GetGradientColor(PT.AgePercent,false);

        return base.OnLifetime(PT);
    }

    public override void Reset()
    {
        base.Reset();
        AddColorKey(1, Color.gray);
    }

}

