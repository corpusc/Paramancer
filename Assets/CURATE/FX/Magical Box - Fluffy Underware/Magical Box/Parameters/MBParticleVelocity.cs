// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Velocity parameter
/// </summary>
/// <remarks>
/// See also: \ref paramvelocity "Velocity parameter reference"
/// </remarks>
[MBParameterInfo(Menu="Velocity",
                 CanAnimateBirth = MBParameterAnimationMode.Optional,
                 CanAnimateLife = MBParameterAnimationMode.Optional)]
public class MBParticleVelocity : MBParameter
{
    /// <summary>
    /// Base velocity in units/second
    /// </summary>
    public float Base;
    /// <summary>
    /// Random deviation from Base
    /// </summary>
    public float RandomPercent;
    /// <summary>
    /// Whether CurveLife defines an acceleration or a fixed velocity
    /// </summary>
    public bool Relative;
    /// <summary>
    /// Base Velocity curve
    /// </summary>
    public AnimationCurve CurveBase;
    /// <summary>
    /// Velocity change (Relative==true) or absolute (Relative==false) velocity
    /// </summary>
    public AnimationCurve CurveLife;
    

    public override void OnBirth(MBParticle PT)
    {
        float init;
        float r = Random.Range(-1f, 1f) * RandomPercent;
        if (AnimatedBirth)
            init = CurveBase.Evaluate(PT.Parent.AgePercent);
        else
            init = Base;

        PT.Velocity = (init+init*r)*PT.Heading;
    }

    public override bool OnLifetime(MBParticle PT)
    {
        if (Relative)
            PT.Velocity = PT.Velocity.magnitude*(1+CurveLife.Evaluate(PT.AgePercent)*PT.DeltaTime) * PT.Heading;
        else
            PT.Velocity = CurveLife.Evaluate(PT.AgePercent) * PT.Heading;
        return true;
    }

    public override void Reset()
    {
        base.Reset();
        Order = 50;
        Base = 1;
        RandomPercent = 0;
        Relative = false;
        
    }

    public override void  Purge()
    {
 	    base.Purge();
        if (!AnimatedBirth)
            CurveBase = null;
        if (!AnimatedLife)
            CurveLife = null;
    }
    


}
