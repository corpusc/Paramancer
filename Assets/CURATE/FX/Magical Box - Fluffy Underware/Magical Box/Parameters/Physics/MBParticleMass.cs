// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Mass parameter
/// </summary>
/// <remarks>
/// See also: \ref parammass "Mass parameter reference"
/// </remarks>
[MBParameterInfo(Menu="Physics/Mass",
                 CanAnimateBirth=MBParameterAnimationMode.Optional,
                 CanAnimateLife=MBParameterAnimationMode.Optional)]
public class MBParticleMass : MBParameter
{
    /// <summary>
    /// Base mass
    /// </summary>
    public float Base;
    /// <summary>
    /// Random deviation from Base
    /// </summary>
    public float RandomPercent;
    /// <summary>
    /// Base curve
    /// </summary>
    public AnimationCurve CurveBirth;
    /// <summary>
    /// Lifetime curve
    /// </summary>
    public AnimationCurve CurveLife;
    

    public override void OnBirth(MBParticle PT)
    {
        float r = Random.Range(-1f, 1f) * RandomPercent;
        if (AnimatedBirth)
            PT.Mass = CurveBirth.Evaluate(PT.Parent.AgePercent);
        else
            PT.Mass = Base;

        PT.Mass+=PT.Mass*r;
    }

    public override bool OnLifetime(MBParticle PT)
    {
        PT.Mass*=CurveLife.Evaluate(PT.AgePercent);
        return true;
    }

    public override void Reset()
    {
        base.Reset();
        Order = 110;
        Base = 1;
    }

    public override void Validate()
    {
        Base = Mathf.Max(1f, Base);
    }

    public override void Purge()
    {
        base.Purge();
        if (!AnimatedBirth)
            CurveBirth = null;
        if (!AnimatedLife)
            CurveLife = null;
    }

}
