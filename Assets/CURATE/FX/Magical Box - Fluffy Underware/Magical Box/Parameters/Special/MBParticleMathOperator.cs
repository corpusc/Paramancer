// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Math Operator Parameter
/// </summary>
/// <remarks>
/// See also: \ref parammathop "Math Operator parameter reference"
/// </remarks>
[MBParameterInfo(Menu = "Special/MathOperator", Needs = "",
                 CanAnimateBirth = MBParameterAnimationMode.None,
                 CanAnimateLife = MBParameterAnimationMode.Mandatory)]
public class MBParticleMathOperator : MBParameter
{
    /// <summary>
    /// The target to apply the value to
    /// </summary>
    public MBParticleMathOperatorTarget Target;
    /// <summary>
    /// The operator used to apply the value
    /// </summary>
    public MBParticleMathOperatorOp Operator;
    /// <summary>
    /// Delay in seconds
    /// </summary>
    public float Delay;
    /// <summary>
    /// Random deviation from Delay
    /// </summary>
    public float DelayRandomPercent;
    /// <summary>
    /// Synchronize all particles?
    /// </summary>
    public bool DelaySync;
    /// <summary>
    /// A Vector to apply to target
    /// </summary>
    public Vector3 Value;
    /// <summary>
    /// Speed for SineWave operator
    /// </summary>
    public float Speed = 1;
    /// <summary>
    /// Random deviation from Value
    /// </summary>
    public float RandomPercent;
    /// <summary>
    /// Whether value will be randomly negative or positive
    /// </summary>
    public bool RandomSign;
    /// <summary>
    /// Whether deltaTime should be applied to value
    /// </summary>
    public bool UseDeltaTime;
    
    int mSlotID=-1;
    float mLastGT;
    float mNextGT;

    public override void OnBirth(MBParticle PT)
    {
        base.OnBirth(PT);
        if (!DelaySync && PT.HasUserData(mSlotID))
            PT.UserData[mSlotID] = 0f;
    }

    public override bool OnLifetime(MBParticle PT)
    {
        if (DelaySync) {
            if (mLastGT==ParentEmitter.ParticleSystem.GlobalTime){
            } else if (mNextGT < ParentEmitter.ParticleSystem.GlobalTime) {
                    mNextGT = PT.ParticleSystem.GlobalTime + Delay + Delay * DelayRandomPercent;
                    mLastGT = ParentEmitter.ParticleSystem.GlobalTime;
            } else
                return true;
        }
        else {
            if (PT.HasUserDataValue(mSlotID) && (float)PT.UserData[mSlotID] < PT.ParticleSystem.GlobalTime) {
                PT.UserData[mSlotID] = PT.ParticleSystem.GlobalTime + Delay + Delay * DelayRandomPercent;
            }
            else
                return true;
        }

        Vector3 v = (Value + Value * RandomPercent);
        if (RandomSign)
            v *= MBUtility.RandomSign();

        switch (Target) {
            case MBParticleMathOperatorTarget.Position:
                DoOp(ref PT.Position, v);
                break;
            case MBParticleMathOperatorTarget.DistanceToCenter:
                Vector3 dir=(PT.Parent.Position-PT.Position);
                float d=dir.magnitude;
                DoOp(ref d, v.x);
                PT.Position = PT.Parent.Position - dir.normalized * d;
                break;
            case MBParticleMathOperatorTarget.Size:
                DoOp(ref PT.Scale, v);
                break;
            case MBParticleMathOperatorTarget.Acceleration:
                DoOp(ref PT.Acceleration, v.x);
                break;
            case MBParticleMathOperatorTarget.Friction:
                DoOp(ref PT.Friction, v.x);
                break;
            case MBParticleMathOperatorTarget.Heading:
                DoOp(ref PT.Heading, v);
                break;
            case MBParticleMathOperatorTarget.Mass:
                DoOp(ref PT.Mass, v.x);
                break;
            case MBParticleMathOperatorTarget.Rotation:
                DoOp(ref PT.Rotation, v);
                break;
        }
        
        return true;
    }

    public override void Reset()
    {
        base.Reset();
        Order = 210;
        RandomPercent = 0;
        Value = Vector3.zero;
    }

    public override void OnPlay()
    {
        base.OnPlay();
        mLastGT = 0;
        mNextGT = 0;
        mSlotID = ParentEmitter.RegisterParticleUserData("MathOp"+GetInstanceID());
    }

    void DoOp(ref float target, float value)
    {
        if (UseDeltaTime) value *= ParentEmitter.ParticleSystem.DeltaTime;
        switch (Operator) {
            case MBParticleMathOperatorOp.Add:
                target += value;
                break;
            case MBParticleMathOperatorOp.Multiply:
                target *= value;
                break;
            case MBParticleMathOperatorOp.SineWave:
                float s = Mathf.Sin(ParentEmitter.ParticleSystem.GlobalTime * Speed);
                target += value * s;
                break;
        }
    }

    void DoOp(ref Vector3 target, Vector3 value)
    {
        if (UseDeltaTime) value *= ParentEmitter.ParticleSystem.DeltaTime;
        switch (Operator) {
            case MBParticleMathOperatorOp.Add: 
                target += value;
                break;
            case MBParticleMathOperatorOp.Multiply:
                target.Scale(value);
                break;
            case MBParticleMathOperatorOp.SineWave:
                float s=Mathf.Sin(ParentEmitter.ParticleSystem.GlobalTime*Speed);
                target+=new Vector3(value.x * s, value.y * s, value.z * s);
                break;
        }
    }
    
}

/// <summary>
/// Math Operator Parameter Target
/// </summary>
public enum MBParticleMathOperatorTarget
{
    // Vector3
    Position = 0,
    Heading  = 1,
    Rotation = 2,
    DistanceToCenter=3,
    // Vector2
    Size = 5,
    // Float
    Acceleration = 9,
    Friction = 10,
    Mass = 11
}

/// <summary>
/// Math Operator Parameter Operation
/// </summary>
public enum MBParticleMathOperatorOp
{
    /// <summary>
    /// Add value
    /// </summary>
    Add = 0,
    /// <summary>
    /// Multiply value
    /// </summary>
    Multiply = 1,
    /// <summary>
    /// Add sine wave
    /// </summary>
    SineWave = 2
}