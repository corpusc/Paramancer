// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using System.Collections.Generic;


/// <summary>
/// Base class for zone parameters
/// </summary>
public class MBParticleZoneBase : MBParameter
{
    /// <summary>
    /// Attraction force toward center
    /// </summary>
    public float Attraction;
    /// <summary>
    /// Zone behaviour mode
    /// </summary>
    public MBParticleZoneMode Mode;
    /// <summary>
    /// This event is called once when a particle enters a zone
    /// </summary>
    public event MBEventHandler ParticleEnteringZone;
    /// <summary>
    /// SendMessage like event linking
    /// </summary>
    public MBSendMessageTarget ParticleEnteringZoneSM;
    /// <summary>
    /// This event is called each frame when a particle is inside the zone
    /// </summary>
    /// <remarks>See \ref events</remarks>
    public event MBEventHandler ParticleInsideZone;
    /// <summary>
    /// SendMessage like event linking
    /// </summary>
    /// <remarks>See \ref events</remarks>
    public MBSendMessageTarget ParticleInsideZoneSM;
    /// <summary>
    /// This event is called once when a particle exits a zone
    /// </summary>
    public event MBEventHandler ParticleLeavingZone;
    /// <summary>
    /// SendMessage like event linking
    /// </summary>
    public MBSendMessageTarget ParticleLeavingZoneSM;

    /// <summary>
    /// Whether this zone use world space instead of local space
    /// </summary>
    public bool WorldSpace;
    /// <summary>
    /// Whether this zone supports World & Local Space
    /// </summary>
    public bool SupportsWorldSpace=true;
    /// <summary>
    /// Gets the force field center in Particle system's space
    /// </summary>
    public Vector3 Center
    {
        get
        {
            return (WorldSpace) ? ParticleSystem.Transform.InverseTransformPoint(Transform.localPosition) : Position;
        }
    }

    public override void OnBirth(MBParticle PT)
    {
        DeathReason = MBDeathReason.Zone;
    }

    public override void Reset()
    {
        base.Reset();
        Order = 130;
        WorldSpace = true;
        Attraction = 1;
    }

    protected void OnParticleEnteringZone(MBParticle PT)
    {
        if (PT.AffectedByZones == null)
            PT.AffectedByZones = new List<MBParticleZoneBase>();
        PT.AffectedByZones.Add(this);
        if (ParticleEnteringZone != null)
            ParticleEnteringZone(new MBEvent(MBEventType.ParameterCallback, PT, this));
        else if (ParticleEnteringZoneSM != null && ParticleEnteringZoneSM.Target && !string.IsNullOrEmpty(ParticleEnteringZoneSM.MethodName))
            ParticleEnteringZoneSM.Invoke(new MBEvent(MBEventType.ParameterCallback, this, PT, this));
    }

    protected void OnParticleLeavingZone(MBParticle PT)
    {
        PT.AffectedByZones.Remove(this);
        if (PT.AffectedByZones.Count == 0)
            PT.AffectedByZones = null;
        if (ParticleLeavingZone != null)
            ParticleLeavingZone(new MBEvent(MBEventType.ParameterCallback, PT, this));
        else if (ParticleLeavingZoneSM != null && ParticleLeavingZoneSM.Target && !string.IsNullOrEmpty(ParticleLeavingZoneSM.MethodName))
            ParticleLeavingZoneSM.Invoke(new MBEvent(MBEventType.ParameterCallback, this, PT, this));
    }

    protected void OnParticleInsideZone(MBParticle PT)
    {
        if (ParticleInsideZone != null)
            ParticleInsideZone(new MBEvent(MBEventType.ParameterCallback, PT,this));
        else if (ParticleInsideZoneSM != null && ParticleInsideZoneSM.Target && !string.IsNullOrEmpty(ParticleInsideZoneSM.MethodName))
            ParticleInsideZoneSM.Invoke(new MBEvent(MBEventType.ParameterCallback,this, PT, this));
    }

}

public enum MBParticleZoneMode
{
    /// <summary>
    /// Use Attraction to apply force
    /// </summary>
    Attract = 0,
    /// <summary>
    /// Bounce off
    /// </summary>
    Reflect = 1,
    /// <summary>
    /// Freeze particle
    /// </summary>
    Freeze = 2,
    /// <summary>
    /// Kill particle
    /// </summary>
    Kill = 3,
    /// <summary>
    /// Only send zone related events
    /// </summary>
    EventsOnly=4
}

