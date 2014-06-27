// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// THE Particle class. This is what it's all about!
/// </summary>
public class MBParticle
{
    #region Public Fields ###
    /// <summary>
    /// Acceleration in direction of heading in Units per Second
    /// </summary>
    public float Acceleration;
    /// <summary>
    /// Age in seconds
    /// </summary>
    /// <remarks>This will be set by the emitter before any parameters are called</remarks>
    public float Age;
    /// <summary>
    /// Age in percent (0..1)
    /// </summary>
    /// <remarks>This will be set by the emitter before any parameters are called</remarks>
    public float AgePercent;
    /// <summary>
    /// Current color
    /// </summary>
    public Color Color;
    /// <summary>
    /// Cumulated forces like Gravity, Forcefields etc...
    /// </summary>
    public Vector3 Force;
    /// <summary>
    /// Friction is applied to the resulting force for dampening
    /// </summary>
    public float Friction;
    /// <summary>
    /// Normalized acceleration vector
    /// </summary>
    public Vector3 Heading;
    /// <summary>
    /// The current MBEmitter.FramesUV index a.k.a. the shown texture frame
    /// </summary>
    public int ImageFrameIndex;
    /// <summary>
    /// Lifetime in seconds
    /// </summary>
    public float Lifetime;
    /// <summary>
    /// Mass
    /// </summary>
    public float Mass;
    /// <summary>
    /// Image Orientation
    /// </summary>
    public float Orientation;
    /// <summary>
    /// Gets the emitter this particle belongs to
    /// </summary>
    public MBEmitter Parent;
    /// <summary>
    /// Position in ParticleSystem's space
    /// </summary>
    public Vector3 Position;
    /// <summary>
    /// Rotation around emitter transform
    /// </summary>
    public Vector3 Rotation;
    /// <summary>
    /// Scale in world units
    /// </summary>
    public Vector3 Scale;
    /// <summary>
    /// Velocity in Units per Second
    /// </summary>
    public Vector3 Velocity;
    
    /// <summary>
    /// Unlike Velocity (which is set by the user), this is the total velocity taking forces, rotation etc.. into account. This will be set by the engine.
    /// </summary>
    /// <remarks>This is useful for parameters that rely on the actual speed/direction of a particle</remarks>
    public Vector3 AbsVelocity;

    /// <summary>
    /// User data array
    /// </summary>
    /// <remarks>
    /// This array is used by parameters that need to store extra data.
    /// \sa \ref particleuserdata
    /// </remarks>
    public object[] UserData;
    /// <summary>
    /// List of zones that this particle is inside
    /// </summary>
    public List<MBParticleZoneBase> AffectedByZones;

    #endregion

    public MBParticle()
    {
        Reset();
    }

    public void Reset()
    {
        Position = Vector3.zero;
        Scale = Vector3.one;
        Color = Color.white;
        mbColor = Color;
        //mbImageAnimTime = 0;
        ImageFrameIndex = 0;
        Heading = Vector3.zero;
        Velocity = Vector3.zero;
        Mass = 1;
        Acceleration = 0;
        Friction = 0;
        Force = Vector3.zero;
        Orientation = 0;
        AngularRotation = 0;
        Rotation = Vector3.zero;
        Parent = null;
        UserData = null;
    }

    #region ### Public Properties ###
    /// <summary>
    /// If you need a delta in your parameters, take this!
    /// </summary>
    public float DeltaTime { get { return ParticleSystem.DeltaTime; } }
    /// <summary>
    /// Gets the Particlesystem this particle belongs to
    /// </summary>
    public MBParticleSystem ParticleSystem { get { return Parent.ParticleSystem; } }

    /// <summary>
    /// Gets or sets position in world space
    /// </summary>
    /// <remarks>This involves a TransformPoint calculations, so use Position if possible</remarks>
    public Vector3 WorldPosition
    {
        get { return ParticleSystem.Transform.TransformPoint(Position); }
        set { Position = ParticleSystem.Transform.InverseTransformPoint(value); }
    }

    #endregion

    /// <summary>
    /// Let this particle die immediately
    /// </summary>
    public void Die()
    {
        Parent.KillParticle(this, MBDeathReason.User);
    }

    /// <summary>
    /// Checks if a particle has a certain user data slot
    /// </summary>
    /// <param name="slotID">the slot id obtained by MBEmitter.GetParticleUserDataID</param>
    /// <returns>true if found</returns>
    public bool HasUserData(int slotID)
    {
        return (UserData!=null && slotID > -1 && slotID < UserData.Length);
    }

    /// <summary>
    /// Checks if a particle has a certain user data slot filled with data
    /// </summary>
    /// <param name="slotID">the slot id obtained by MBEmitter.GetParticleUserDataID</param>
    /// <returns>true if found and the slot is not null</returns>
    public bool HasUserDataValue(int slotID)
    {
        return (UserData != null && slotID > -1 && slotID < UserData.Length && UserData[slotID]!=null);
    }

    /// <summary>
    /// Create an Object-Proxy holding this particle
    /// </summary>
    /// <returns>a MBParticleObject with it's value set to this particle</returns>
    public MBParticleObject ToObject()
    {
        return new MBParticleObject() { Value = this };
    }

    #region ### Internal Publics. Don't use them unless you know what you're doing ###
    /*! @name Internal Public
     * Don't use them unless you know what you're doing
     */
    //@{

    /// <summary>
    /// Internal field! Used by Orientation parameter
    /// </summary>
    /// <remarks>Used by Orientation parameter</remarks>
    public float AngularRotation;

    /// <summary>
    /// Internal Field! Color storage used for fades and animations
    /// </summary>
    public Color mbColor;
    
    //@}
    #endregion
}

/// <summary>
/// Proxy class, covering a MBParticle inside Object
/// </summary>
/// <remarks>Main purpose is for playMaker suppport, but may be used by other 3rd party scripts as well</remarks>
public class MBParticleObject : Object
{
    public MBParticle Value;
}