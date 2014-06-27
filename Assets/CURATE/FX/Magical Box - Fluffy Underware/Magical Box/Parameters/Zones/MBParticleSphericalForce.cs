// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Spherical force zone parameter
/// </summary>
/// <remarks>
/// See also: \ref paramradialzone "Radial/Spherical force zone parameter reference"
/// </remarks>
[MBParameterInfo(Menu = "Zones/Spherical",
                 CanAnimateBirth = MBParameterAnimationMode.None,
                 CanAnimateLife = MBParameterAnimationMode.Mandatory)]
public class MBParticleSphericalForce : MBParticleRadialForce
{

    void DoGizmo()
    {
        Gizmos.color = GizmoColor1;
        Vector3 scale = Transform.lossyScale;
        if (WorldSpace)
            Gizmos.matrix = Matrix4x4.TRS(Transform.localPosition, Transform.rotation, scale);
        else
            Gizmos.matrix = Matrix4x4.TRS(Transform.position, Transform.rotation, scale);
        if (Radius > 0) {
            Gizmos.color = Attraction < 0 ? GizmoColor2 : GizmoColor1;
            Gizmos.DrawWireSphere(Vector3.zero, Radius);
            Gizmos.color = Attraction < 0 ? GizmoColor1 : GizmoColor2;
            Gizmos.DrawWireSphere(Vector3.zero,Radius * Hollow);
        }
        else
            Gizmos.DrawSphere(Vector3.zero, 0.2f);

        Gizmos.color = GizmoColor2;
        if (Attraction < 0)
            MBUtility.DrawGizmoArrow(new Vector3(0, Radius * Hollow, 0), new Vector3(0, 1, 0), Radius - Radius * Hollow);
        else
            MBUtility.DrawGizmoArrow(new Vector3(0, Radius, 0), new Vector3(0, -1, 0), Radius - Radius * Hollow);
    }

}

