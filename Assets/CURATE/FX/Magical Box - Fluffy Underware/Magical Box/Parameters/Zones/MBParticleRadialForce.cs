// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Radial force zone parameter
/// </summary>
/// <remarks>
/// See also: \ref paramradialzone "Radial force zone parameter reference"
/// </remarks>
[MBParameterInfo(Menu = "Zones/Radial",
                 CanAnimateBirth = MBParameterAnimationMode.None,
                 CanAnimateLife = MBParameterAnimationMode.Mandatory)]
public class MBParticleRadialForce : MBParticleZoneBase
{
    /// <summary>
    /// Radius force is applied for
    /// </summary>
    public float Radius;
    /// <summary>
    /// Cut-out/inner radius
    /// </summary>
    public float Hollow;
    /// <summary>
    /// Falloff for non linear attraction
    /// </summary>
    public float Falloff;

    public override bool OnLifetime(MBParticle PT)
    {
        Vector3 dir = PT.Position - Center;
        float dist = dir.sqrMagnitude; 
        float ib=0;
        float ob=0;
        
        bool affected=false;

        if (Radius > 0) {
            ib = Radius * Hollow;
            ib *= ib;
            ob = Radius * Radius;
            if (dist >= ib && dist <= ob) 
                affected = true;
        } else
            affected=true;

        if (affected) {
            if (PT.AffectedByZones == null || !PT.AffectedByZones.Contains(this))
                OnParticleEnteringZone(PT);
            switch (Mode) {
                case MBParticleZoneMode.Attract:
                    if (Attraction != 0) {
                        PT.Force += dir.normalized * -Attraction / Mathf.Lerp(Mathf.Max(dist - ib, 1f), 1, Falloff);
                    }
                    OnParticleInsideZone(PT);
                    break;
                case MBParticleZoneMode.Freeze:
                    PT.Velocity = Vector3.zero;
                    OnParticleInsideZone(PT);
                    break;
                case MBParticleZoneMode.Kill:
                    OnParticleInsideZone(PT);
                    return false;
                case MBParticleZoneMode.Reflect:
                    if (Radius > 0) {
                        var R = new Ray(Center, dir);
                        // if hollow, check if particle comes from the inside
                        if (Hollow > 0 && (PT.Position - PT.Velocity - Center).sqrMagnitude < ib) {
                            PT.Position = R.GetPoint(Radius*Hollow);
                            PT.Heading = -R.direction;
                            PT.Velocity = PT.Velocity.magnitude * -R.direction;
                        }
                        else {
                            PT.Position = R.GetPoint(Radius);
                            PT.Heading = R.direction;
                            PT.Velocity = PT.Velocity.magnitude * R.direction;
                        }
                        OnParticleInsideZone(PT);
                    }
                    break;
                case MBParticleZoneMode.EventsOnly:
                    OnParticleInsideZone(PT);
                    break;
            }
        }
        else {
            if (PT.AffectedByZones != null && PT.AffectedByZones.Contains(this))
                OnParticleLeavingZone(PT);
        }
        return true;
    }

    public override void Reset()
    {
        base.Reset();
        Radius = 0;
        Hollow = 0;
        Falloff = 0;
    }

    public override void Validate()
    {
        base.Validate();
        Radius = Mathf.Max(0, Radius);
    }
    
    protected override void OnDrawGizmos()
    {
        if (ShouldDrawGizmos) {
            base.OnDrawGizmos();
            DoGizmo();
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        if (ShouldDrawGizmosSelected) {
            base.OnDrawGizmosSelected();
            DoGizmo();
        }
    }
    
    void DoGizmo()
    {
        Gizmos.color = GizmoColor1;
        
        if (WorldSpace)
            Gizmos.matrix = Matrix4x4.TRS(Transform.localPosition, Transform.localRotation, Transform.lossyScale);
        else
            Gizmos.matrix = Matrix4x4.TRS(Transform.position, Transform.rotation, Transform.lossyScale);
        if (Radius > 0) {
            Gizmos.color = Attraction < 0 ? GizmoColor2 : GizmoColor1;
            MBUtility.DrawGizmoCircle(Radius);
            Gizmos.color = Attraction < 0 ? GizmoColor1 : GizmoColor2;
            MBUtility.DrawGizmoCircle(Radius * Hollow);
        }
        else 
            Gizmos.DrawSphere(Vector3.zero, 0.2f);
        
        Gizmos.color = GizmoColor2;
        if (Attraction < 0)
            MBUtility.DrawGizmoArrow(new Vector3(0,Radius*Hollow,0), new Vector3(0, 1, 0), Radius-Radius*Hollow);
        else
            MBUtility.DrawGizmoArrow(new Vector3(0,Radius,0), new Vector3(0, -1, 0), Radius-Radius*Hollow);
    }

}

