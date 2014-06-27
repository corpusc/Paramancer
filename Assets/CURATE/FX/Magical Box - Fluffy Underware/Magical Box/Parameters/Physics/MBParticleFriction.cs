// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Friction parameter
/// </summary>
/// <remarks>
/// See also: \ref paramfriction "Friction parameter reference"
/// </remarks>
[MBParameterInfo(Menu = "Physics/Friction",
                 CanAnimateBirth = MBParameterAnimationMode.Optional,
                 CanAnimateLife = MBParameterAnimationMode.Optional)]
public class MBParticleFriction : MBParameter
{
    /// <summary>
    /// Base friction
    /// </summary>
    public float Base;
    /// <summary>
    /// Random deviation from Base
    /// </summary>
    public float RandomPercent;
    /// <summary>
    /// Base friction curve
    /// </summary>
    public AnimationCurve CurveBase;
    /// <summary>
    /// Lifetime friction curve
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

        PT.Friction = (init + init * r);
    }

    public override bool OnLifetime(MBParticle PT)
    {
        PT.Friction = CurveLife.Evaluate(PT.AgePercent);
        return true;
    }

    public override void Reset()
    {
        base.Reset();
        Order = 80;
    }

}
