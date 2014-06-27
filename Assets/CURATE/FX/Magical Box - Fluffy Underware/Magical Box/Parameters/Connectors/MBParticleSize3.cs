// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Size parameter
/// </summary>
/// <remarks>
/// See also: \ref paramsize "Size parameter reference"
/// </remarks>
[MBParameterInfo(Menu = "Connector/Size 3D",
                 CanAnimateBirth = MBParameterAnimationMode.Optional,
                 CanAnimateLife = MBParameterAnimationMode.Optional)]
public class MBParticleSize3 : MBParameter
{
    /// <summary>
    /// Whether axis are handled independently or separate
    /// </summary>
    public bool ConstrainAxis;
    /// <summary>
    /// Base size in units
    /// </summary>
    public Vector3 Base;
    /// <summary>
    /// Random deviation from Base
    /// </summary>
    public float RandomPercent;
    /// <summary>
    /// Base x-axis size in units
    /// </summary>
    public AnimationCurve CurveBirthX;
    /// <summary>
    /// Base y-axis size in units
    /// </summary>
    public AnimationCurve CurveBirthY;
    /// <summary>
    /// Base z-axis size in units
    /// </summary>
    public AnimationCurve CurveBirthZ;
    /// <summary>
    /// Whether CurveLife defines a growth or a fix size
    /// </summary>
    public bool Relative;
    /// <summary>
    /// x-axis growth (Relative==true) or size (Relative==false)
    /// </summary>
    public AnimationCurve CurveLifeX;
    /// <summary>
    /// y-axis growth (Relative==true) or size (Relative==false)
    /// </summary>
    public AnimationCurve CurveLifeY;
    /// <summary>
    /// z-axis growth (Relative==true) or size (Relative==false)
    /// </summary>
    public AnimationCurve CurveLifeZ;
    /// <summary>
    /// Whether particles should die automatically when at least one axis falls below 0
    /// </summary>
    public bool DieOnZero;

    public override void OnBirth(MBParticle PT)
    {
        DeathReason = MBDeathReason.Size;
        float x, y,z;
        float r = Random.Range(-1f, 1f) * RandomPercent;
        if (AnimatedBirth) {
            x = CurveBirthX.Evaluate(PT.Parent.AgePercent);
            y = (ConstrainAxis) ? x : CurveBirthY.Evaluate(PT.Parent.AgePercent);
            z = (ConstrainAxis) ? x : CurveBirthZ.Evaluate(PT.Parent.AgePercent);
        }
        else {
            x = Base.x;
            y = (ConstrainAxis) ? x : Base.y;
            z = (ConstrainAxis) ? x : Base.z;
        }
        x += x * r;
        y += y * r;
        z += z * r;

        PT.Scale = new Vector3(x, y,z);
    }

    public override bool OnLifetime(MBParticle PT)
    {
        float x = CurveLifeX.Evaluate(PT.AgePercent);
        float y = (ConstrainAxis) ? x : CurveLifeY.Evaluate(PT.AgePercent);
        float z = (ConstrainAxis) ? x : CurveLifeZ.Evaluate(PT.AgePercent);
        if (Relative)
            PT.Scale.Scale(new Vector3(1 + x * PT.DeltaTime, 1 + y * PT.DeltaTime, 1+z*PT.DeltaTime));
        else
            PT.Scale = new Vector3(x, y, z);
        if (DieOnZero && (PT.Scale.x <= 0 || PT.Scale.y <= 0 || PT.Scale.z<=0)) {
            return false;
        }
        return true;

    }

    public override void Reset()
    {
        base.Reset();
        Order = 30;
        ConstrainAxis = true;
        Relative = true;
        Base = Vector3.one;
        RandomPercent = 0;
        DieOnZero = true;
    }

    public override void Purge()
    {
        base.Purge();
        if (!AnimatedBirth) {
            CurveBirthX = null;
            CurveBirthY = null;
            CurveBirthZ = null;
        }
        if (!AnimatedLife) {
            CurveLifeX = null;
            CurveLifeY = null;
            CurveLifeZ = null;
        }
    }

}
