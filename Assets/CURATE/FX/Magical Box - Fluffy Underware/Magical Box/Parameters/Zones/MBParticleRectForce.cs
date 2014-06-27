// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Rectangle force zone parameter
/// </summary>
/// <remarks>
/// See also: \ref paramrectzone "Rectangle force zone parameter reference
/// </remarks>
[MBParameterInfo(Menu = "Zones/Rect",
                 CanAnimateBirth = MBParameterAnimationMode.None,
                 CanAnimateLife = MBParameterAnimationMode.Mandatory)]
public class MBParticleRectForce : MBParticleZoneBase {
    Rect mZoneRect;

    public override bool OnLifetime(MBParticle PT)
    {
        Vector3 ptOriginPos = (WorldSpace) ? PT.WorldPosition : Transform.InverseTransformPoint(PT.WorldPosition);
        if (WorldSpace) {
            Matrix4x4 mMove = Matrix4x4.TRS(-Transform.localPosition, Quaternion.identity, Vector3.one);
            Matrix4x4 mRot = Matrix4x4.TRS(Vector3.zero, Quaternion.Inverse(Transform.localRotation), Vector3.one);
            ptOriginPos = (mRot * mMove).MultiplyPoint3x4(ptOriginPos);
        }
        if (mZoneRect.Contains(ptOriginPos)) {
            if (PT.AffectedByZones == null || !PT.AffectedByZones.Contains(this))
                OnParticleEnteringZone(PT);

            switch (Mode) {
                case MBParticleZoneMode.Attract:
                    if (Attraction != 0)
                        PT.Force += Transform.up * -Attraction;
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
                    PT.Velocity *= -1;
                    PT.Heading *= -1;
                    OnParticleInsideZone(PT);
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
        Attraction = 1;
        Scale = Vector3.one;
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

    public override void Validate()
    {
        base.Validate();
        mZoneRect = new Rect(-Scale.x, -Scale.y, Scale.x * 2, Scale.y * 2);
    }

    public override void OnPlay()
    {
        base.OnPlay();
        mZoneRect = new Rect(-Scale.x, -Scale.y, Scale.x * 2, Scale.y * 2);
    }

    void DoGizmo()
    {
        Gizmos.color = GizmoColor1;
        if (WorldSpace)
            Gizmos.matrix = Matrix4x4.TRS(Transform.localPosition, Transform.localRotation, Transform.lossyScale);
        else
            Gizmos.matrix = Matrix4x4.TRS(Transform.position, Transform.rotation, Transform.lossyScale);
        MBUtility.DrawGizmoRect(mZoneRect);
        if (Attraction < 0)
            MBUtility.DrawGizmoArrow(Vector3.zero, new Vector3(0,1,0),Scale.y);
        else
            MBUtility.DrawGizmoArrow(Vector3.zero, new Vector3(0,-1,0),Scale.y);
    }
}
