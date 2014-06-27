// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Gravity parameter
/// </summary>
/// <remarks>
/// See also: \ref paramgravity "Gravity parameter reference"
/// </remarks>
[MBParameterInfo(Menu = "Physics/Gravity",
                 CanAnimateBirth=MBParameterAnimationMode.None,
                 CanAnimateLife=MBParameterAnimationMode.Mandatory)]
public class MBParticleGravity : MBParameter
{
    /// <summary>
    /// Base gravity
    /// </summary>
    public Vector3 Base;

    public override bool OnLifetime(MBParticle PT)
    {
        PT.Force += Base;
        return true;
    }

    public override void Reset()
    {
        base.Reset();
        Order = 120;
        Base = new Vector3(0, -0.981f, 0);
    }


}

