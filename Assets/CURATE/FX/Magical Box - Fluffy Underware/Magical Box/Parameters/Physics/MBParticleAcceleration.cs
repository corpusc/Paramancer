// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Acceleration parameter
/// </summary>
/// <remarks>
/// See also: \ref paramaccel "Acceleration parameter reference"
/// </remarks>
[MBParameterInfo(Menu = "Physics/Acceleration",
                 CanAnimateBirth = MBParameterAnimationMode.Optional,
                 CanAnimateLife = MBParameterAnimationMode.Optional)]
public class MBParticleAcceleration : MBParameter
{
    /// <summary>
    /// Base acceleration
    /// </summary>
    public float Base;
    /// <summary>
    /// Random deviation from Base
    /// </summary>
    public float RandomPercent;
    /// <summary>
    /// Base acceleration curve
    /// </summary>
    public AnimationCurve CurveBase;
    /// <summary>
    /// Lifetime acceleration curve
    /// </summary>
    public AnimationCurve CurveLife;

    public override void OnBirth(MBParticle PT)
    {
        base.OnBirth(PT);
        float init;
        float r = Random.Range(-1f, 1f) * RandomPercent;
        if (AnimatedBirth)
            init = CurveBase.Evaluate(PT.Parent.AgePercent);
        else
            init = Base;

        PT.Acceleration = (init + init * r);
    }

    public override bool OnLifetime(MBParticle PT)
    {
        PT.Acceleration = CurveLife.Evaluate(PT.AgePercent);
        return true;
    }

    public override void Reset()
    {
        base.Reset();
        Order = 60;
    }

}
