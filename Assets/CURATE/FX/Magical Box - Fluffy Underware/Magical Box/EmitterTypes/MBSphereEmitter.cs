// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Provides a spherical emitter type
/// </summary>
[MBEmitterTypeInfo(Menu="3D/Sphere")]
public class MBSphereEmitter : MBEmitterType
{
    public float Hollow;
    public float Arc = 0;

    public override Vector3 GetPosition(MBParticle PT)
    {
        float r = Random.Range(Hollow, 1);
        float l = -Random.Range(0, Mathf.PI * 2);
        float h = Random.Range(-Mathf.PI/2+Arc*Mathf.PI/2, Mathf.PI / 2);
        float ch = Mathf.Cos(h);
        Vector3 v = new Vector3(r * Mathf.Cos(l) * ch,
                                r * Mathf.Sin(h),
                                  r * Mathf.Sin(l) * ch);
        v.Scale(Scale);

        return v;
    }

    public override void Validate()
    {
        base.Validate();
        Hollow = Mathf.Clamp01(Hollow);
    }

    public override void Reset()
    {
        base.Reset();
        Hollow = 0;
    }


    protected override void DoGizmos()
    {
        base.DoGizmos();
        Gizmos.color = GizmoColor1;
        Vector3 scale = MBUtility.Scale(Scale, Transform.lossyScale);
        Gizmos.matrix = Matrix4x4.TRS(Transform.position, Transform.rotation, scale);
        Gizmos.DrawWireSphere(Vector3.zero, 1);
        Gizmos.color = GizmoColor2;
        Gizmos.matrix = Matrix4x4.TRS(Transform.position, Transform.rotation, scale*Hollow);
        Gizmos.DrawWireSphere(Vector3.zero, 1);
    }

}