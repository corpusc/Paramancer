// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Provides a hollow box emitter
/// </summary>
[MBEmitterTypeInfo(Menu = "3D/Hollow Box")]
public class MBHollowBoxEmitter : MBEmitterType
{
    public Vector3 Hollow;

    public override Vector3 GetPosition(MBParticle PT)
    {
        int runs = 0;
        Bounds r = new Bounds(Vector3.zero,Scale);
        Vector3 hrs=Scale;
        hrs.Scale(Hollow);
        Bounds hr = new Bounds(Vector3.zero, hrs);
        Vector3 v = Vector3.zero;
        while (runs++ < 100) {
            v.x = Random.Range(r.min.x, r.max.x);
            v.y = Random.Range(r.min.y, r.max.y);
            v.z = Random.Range(r.min.z, r.max.z);
            if (!hr.Contains(v))
                return v;
        }
        return new Vector3(r.min.x, r.min.y, r.min.z);
    }

    public override void Reset()
    {
        base.Reset();
        Hollow = Vector3.zero;
    }

    protected override void DoGizmos()
    {
        base.DoGizmos();
        Gizmos.color = GizmoColor1;
        Vector3 scale = MBUtility.Scale(Scale, Transform.lossyScale);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, scale);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        Gizmos.color = GizmoColor2;
        Vector3 h = scale;
        h.Scale(Hollow);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, h);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}