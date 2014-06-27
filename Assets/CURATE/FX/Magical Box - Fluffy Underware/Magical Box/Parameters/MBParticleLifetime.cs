// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Lifetime parameter
/// </summary>
/// <remarks>
/// See also: \ref paramlifetime "Lifetime parameter reference"
/// </remarks>
[MBParameterInfo(Menu = "Lifetime", 
                 CanAnimateBirth=MBParameterAnimationMode.Optional,
                 CanAnimateLife=MBParameterAnimationMode.None)]
public class MBParticleLifetime : MBParameter
{
    /// <summary>
    /// Base lifetime in seconds
    /// </summary>
    public float Base;
    /// <summary>
    /// Random deviation from Base
    /// </summary>
    public float RandomPercent;
    /// <summary>
    /// Base Animation curve
    /// </summary>
    public AnimationCurve Curve;

    public override void OnBirth(MBParticle PT)
    {
        float r = Random.Range(-1f, 1f) * RandomPercent;

        if (AnimatedBirth)
            PT.Lifetime = Curve.Evaluate(PT.Parent.AgePercent);
        else
            PT.Lifetime = Base;

        PT.Lifetime += PT.Lifetime * r;
    }

    public override void Reset()
    {
        base.Reset();
        Order = 10;
        Base = 1;
        RandomPercent = 0;
    }
   
    public override void Purge()
    {
        if (!AnimatedBirth)
            Curve = null;
    }


}
