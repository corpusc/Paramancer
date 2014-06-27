// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Base class of all parameters
/// </summary>
/// <remarks>
/// \sa \ref paramref
/// \sa \ref codeextendparameter
/// </remarks>
public class MBParameter : MBObject
{
    /// <summary>
    /// Color used by parameter visualization
    /// </summary>
    public Color GizmoColor1 = new Color(0.6f, 0, 0.6f);
    /// <summary>
    /// Color used by parameter visualization
    /// </summary>
    public Color GizmoColor2 = new Color(1, 0, 1);

    #region ### Unity Callbacks ###

    protected override void OnDrawGizmos() { } // don't draw a gizmo by default
    protected override void OnDrawGizmosSelected() { } // don't draw a gizmo by default

    #endregion

    #region ### Public Methods and Properties ###

    /// <summary>
    /// Is this parameter animated over the emitter's duration?
    /// </summary>
    /// <remarks>
    /// Check this flag to determine if you need to handle curves or static values in OnBirth()
    /// </remarks>
    public bool AnimatedBirth;
    /// <summary>
    /// Does this parameter animate particle's over their lifetime?
    /// </summary>
    /// <remarks>If true OnLifeTime() will be called every frame the particle exists</remarks>
    public bool AnimatedLife;

    /// <summary>
    /// If this parameter causes the Particle to die (by returning false in OnLifetime()), set the reason here
    /// </summary>
    public MBDeathReason DeathReason;

    /// <summary>
    /// Gets the next parameter in order
    /// </summary>
    /// <returns>the next parameter according to the parameter order or null</returns>
    public MBParameter NextParameter
    {
        get
        {
            int idx=ParentEmitter.Parameters.IndexOf(this)+1;
            return (idx == ParentEmitter.Parameters.Count) ? null: ParentEmitter.Parameters[idx];
        }
    }

    /// <summary>
    /// Determines the order of execution
    /// </summary>
    /// <remarks>
    /// Parameters are applied in increasing Order.
    /// \sa \ref paramorder
    /// </remarks>
    public int Order;
    
    /// <summary>
    /// Gets the corresponding ParameterInfo attribute
    /// </summary>
    public MBParameterInfo ParameterInfo
    {
        get
        {
            return (MBParameterInfo)System.Attribute.GetCustomAttribute(GetType(), typeof(MBParameterInfo));
        }
    }

    /// <summary>
    /// Gets the emitter this parameter belongs to
    /// </summary>
    public MBEmitter ParentEmitter
    {
        get
        {
            return Parent as MBEmitter;
        }
    }

    /// <summary>
    /// Gets the previous parameter in order
    /// </summary>
    /// <returns>the previous parameter according to the parameter order or null</returns>
    public MBParameter PreviousParameter
    {
        get
        {
            int idx = ParentEmitter.Parameters.IndexOf(this) -1;
            return (idx<0) ? null : ParentEmitter.Parameters[idx];
        }
    }

    #endregion

    #region ### Virtual Methods each parameter wants to override ###

    public override void Destroy()
    {
        if (ParentEmitter)
            ParentEmitter.RemoveParameter(this);
    }


    /// <summary>
    /// Called on each MBEmitter.Play()
    /// </summary>
    /// <remarks>Use this to initialize your parameter</remarks>
    public virtual void OnPlay()
    {
    }

    /// <summary>
    /// Sets a particle's initial parameters
    /// </summary>
    /// <remarks>
    /// This is called once on particle's birth. If AnimatedBirth is true you should animate values here (use PT.DeltaTime and PT.Emitter.AgePercent)
    /// </remarks>
    public virtual void OnBirth(MBParticle PT) { }

    /// <summary>
    /// Change particle values over its lifetime
    /// </summary>
    /// <remarks>This is called each frame a particle exists. You should animate values using PT.AgePercent and PT.DeltaTime</remarks>
    /// <returns>false if Particle should die. In this case set a DeathReason!</returns>
    public virtual bool OnLifetime(MBParticle PT) { return true; }

    /// <summary>
    /// Validate/Check/Limit fields and properties here
    /// </summary>
    /// <remarks>This is called by the Editor window after values were changed.</remarks>
    public virtual void Validate() { }

    /// <summary>
    /// Restore default settings
    /// </summary>
    /// <remarks>Called by the user in the parameter editor</remarks>
    public override void Reset()
    {
        switch (ParameterInfo.CanAnimateBirth) {
            case MBParameterAnimationMode.Mandatory: AnimatedBirth = true; break;
            default: AnimatedBirth = false; break;
        }
        switch (ParameterInfo.CanAnimateLife) {
            case MBParameterAnimationMode.Mandatory: AnimatedLife = true; break;
            default: AnimatedLife = false; break;
        }
        Purge();
    }

    #endregion

    #region ### Internal Publics. Don't use them unless you know what you're doing ###

    #endregion

    public override int CompareTo(object obj)
    {
        if (obj is MBParameter)
            return -((MBParameter)obj).Order.CompareTo(Order);
        else
            return base.CompareTo(obj);
    }
}

/// <summary>
/// Class to define usage of this parameter for the editor.
/// </summary>
public class MBParameterInfo : System.Attribute
{
    /// <summary>
    /// As shown in the Add-Menu
    /// </summary>
    public string Menu;
    /// <summary>
    /// Needs other parameters to work properly?
    /// </summary>
    /// <remarks>Content will be shown below the parameter header in the editor</remarks>
    public string Needs;
    /// <summary>
    /// Usage notes
    /// </summary>
    /// <remarks>Content will be shown below the parameter header in the editor</remarks>
    public string Note;
    /// <summary>
    /// Exclude usage of other parameters?
    /// </summary>
    /// /// <remarks>Content will be shown below the parameter header in the editor</remarks>
    public string Excludes;
    /// <summary>
    /// Is this parameter able to animate particles over their lifetime?
    /// </summary>
    public MBParameterAnimationMode CanAnimateLife;
    /// <summary>
    /// Is this parameter able to animate particles initial values over emitters duration?
    /// </summary>
    public MBParameterAnimationMode CanAnimateBirth;

    public MBParameterInfo()
    {
        Menu = "Missing MBParameterInfo attribute";
    }
    
}

/// <summary>
/// Defines how Birth/Life-Animations are supported by a parameter
/// </summary>
/// <remarks>This is used by the editor to determine allowed animation modes</remarks>
public enum MBParameterAnimationMode
{
    /// <summary>
    /// This animation type can't animate
    /// </summary>
    None=0,
    /// <summary>
    /// This animation type may be animated or not
    /// </summary>
    Optional=1,
    /// <summary>
    /// This animation type must be always animated
    /// </summary>
    Mandatory=2
}
