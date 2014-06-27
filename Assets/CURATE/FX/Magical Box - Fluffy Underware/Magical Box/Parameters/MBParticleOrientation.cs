// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Orientation parameter
/// </summary>
/// <remarks>
/// See also: \ref paramorientation "Orientation parameter reference"
/// </remarks>
[MBParameterInfo(Menu = "Orientation",
                 CanAnimateBirth = MBParameterAnimationMode.None,
                 CanAnimateLife = MBParameterAnimationMode.Optional)]
public class MBParticleOrientation : MBParameter
{
    /// <summary>
    /// Orientation Mode used when AnimatedLife is true
    /// </summary>
    public MBOrientationMode Mode;
    /// <summary>
    /// Offset rotates source texture before calculating anything else
    /// </summary>
    public float Offset;
    /// <summary>
    /// Base orientation in degrees
    /// </summary>
    public float Base;
    /// <summary>
    /// Random angle in degrees offset to Base
    /// </summary>
    public float ArcRange;
    /// <summary>
    /// Angular Velocity in degrees/second
    /// </summary>
    /// <remarks>Only available when Mode is set to MBOrientationMode.Turn</remarks>
    public float FixedTurnSpeed;
    /// <summary>
    /// Random deviation from FixedTurnSpeed
    /// </summary>
    /// <remarks>Only available when Mode is set to MBOrientationMode.Turn</remarks>
    public float FixedRandomPercent;
    /// <summary>
    /// Random direction for FixedTurnSpeed
    /// </summary>
    /// <remarks>Only available when Mode is set to MBOrientationMode.Turn</remarks>
    public bool FixedRandomSign;


    public override void OnBirth(MBParticle PT)
    {
        PT.Orientation = Offset + Base + Random.Range(-ArcRange, ArcRange);
        if (Mode==MBOrientationMode.Turn){
            float sign=(FixedRandomSign) ? MBUtility.RandomSign() : 1;
            PT.AngularRotation = (FixedTurnSpeed + Random.Range(-FixedTurnSpeed, FixedTurnSpeed) * FixedRandomPercent ) * sign;
        }
    }

    public override bool OnLifetime(MBParticle PT)
    {
        switch (Mode) {
            case MBOrientationMode.ByVelocity:
                PT.Orientation = Offset - 90 - Mathf.Atan2(-PT.AbsVelocity.y, -PT.AbsVelocity.x) * Mathf.Rad2Deg;
                break;
            case MBOrientationMode.ByHeading:
                PT.Orientation = Offset - 90 - Mathf.Atan2(-PT.Heading.y, -PT.Heading.x) * Mathf.Rad2Deg;
                break;
            case MBOrientationMode.Turn:
                PT.Orientation += PT.AngularRotation * PT.DeltaTime;
                break;
        }
		PT.Orientation = PT.Orientation % 360;
        return true;   
    }

    public override void Reset()
    {
        base.Reset();
        Order = 90;
        Mode = MBOrientationMode.Turn;
        FixedTurnSpeed = 0;
    }

}

/// <summary>
/// Defines orientation mode when using lifetime animation
/// </summary>
public enum MBOrientationMode
{
    /// <summary>
    /// Align particle in relation to velocity vector
    /// </summary>
    ByVelocity=0,
    /// <summary>
    /// Align particle in relation to heading vector
    /// </summary>
    ByHeading=1,
    /// <summary>
    /// Steady turn
    /// </summary>
    Turn=2
}
