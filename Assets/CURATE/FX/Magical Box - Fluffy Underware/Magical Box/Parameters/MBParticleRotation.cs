// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Rotation parameter
/// </summary>
/// <remarks>
/// See also: \ref paramrotation "Rotation parameter reference"
/// </remarks>
[MBParameterInfo(Menu = "Rotation",
                 CanAnimateBirth = MBParameterAnimationMode.None,
                 CanAnimateLife = MBParameterAnimationMode.Mandatory)]
public class MBParticleRotation : MBParameter
{
    /// <summary>
    /// Base rotation in degrees
    /// </summary>
    public Vector3 Base;
    /// <summary>
    /// Random deviation from Base
    /// </summary>
    public float RandomPercent;

    public override void OnBirth(MBParticle PT)
    {
        base.OnBirth(PT);
        PT.Rotation = Base + Base * Random.Range(-1f, 1f) * RandomPercent;
        
    }

    public override bool OnLifetime(MBParticle PT)
    {
        PT.Position -= ParentEmitter.Position;
        PT.Position = Quaternion.Euler(PT.Rotation * PT.DeltaTime) * PT.Position;
        PT.Position += ParentEmitter.Position;
        return true;
    }

    public override void Reset()
    {
        base.Reset();
        Order = 100;
        RandomPercent = 0;
        Purge();
    }
   
}


