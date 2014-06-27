// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Heading parameter
/// </summary>
/// <remarks>
/// See also: \ref paramheading "Heading parameter reference"
/// </remarks>
[MBParameterInfo(Menu="Physics/Heading",
                 CanAnimateBirth=MBParameterAnimationMode.None,
                 CanAnimateLife=MBParameterAnimationMode.Mandatory)]
public class MBParticleHeading : MBParameter
{
    /// <summary>
    /// Whether heading values are relative or absolute
    /// </summary>
    public bool Relative;
    /// <summary>
    /// Lifetime heading curve x-axis in degrees
    /// </summary>
    public AnimationCurve CurveLifeX;
    /// <summary>
    /// Lifetime heading curve y-axis in degrees
    /// </summary>
    public AnimationCurve CurveLifeY;
    /// <summary>
    /// Lifetime heading curve z-axis in degrees
    /// </summary>
    public AnimationCurve CurveLifeZ;

    public override bool OnLifetime(MBParticle PT)
    {
        if (Relative) {
            PT.Heading = Quaternion.Euler(new Vector3(CurveLifeX.Evaluate(PT.AgePercent) * PT.DeltaTime,
                                                      CurveLifeY.Evaluate(PT.AgePercent) * PT.DeltaTime,
                                                      CurveLifeZ.Evaluate(PT.AgePercent) * PT.DeltaTime)) *PT.Heading;
        }
        else
            PT.Heading = new Vector3(CurveLifeX.Evaluate(PT.AgePercent),
                                      CurveLifeY.Evaluate(PT.AgePercent),
                                      CurveLifeZ.Evaluate(PT.AgePercent)).normalized;
        return true;
    }

    public override void Reset()
    {
        base.Reset();
        Order = 70;
        Relative = true;
        CurveLifeX = AnimationCurve.Linear(0, 0, 1, 0);
        CurveLifeY = AnimationCurve.Linear(0, 0, 1, 0);
        CurveLifeZ = AnimationCurve.Linear(0, 0, 1, 0);
    }
    
}
