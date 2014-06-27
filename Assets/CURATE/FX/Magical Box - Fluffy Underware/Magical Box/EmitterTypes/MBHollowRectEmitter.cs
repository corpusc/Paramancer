// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Provides a hollow rectangle emitter
/// </summary>
[MBEmitterTypeInfo(Menu = "2D/Hollow Rect")]
public class MBHollowRectEmitter : MBEmitterType
{
    public Vector2 Hollow;

    public override Vector3 GetPosition(MBParticle PT)
    {
        int runs = 0;
        Rect r = new Rect(-Scale.x * .5f, -Scale.y * .5f, Scale.x, Scale.y);
        Rect hr = new Rect(r.xMin * Hollow.x, r.yMin * Hollow.y, r.width * Hollow.x, r.height * Hollow.y);
        Vector3 v = Vector3.zero;
        while (runs++ < 100) {
            v.x = Random.Range(r.xMin, r.xMax);
            v.y = Random.Range(r.yMin, r.yMax);
            if (!hr.Contains(v))
                return v;
        }
        return new Vector3(r.xMin, r.yMin, 0);
    }

    public override void Reset()
    {
        base.Reset();
        Hollow = Vector2.zero;
    }

    protected override void DoGizmos()
    {
        base.DoGizmos();
        Gizmos.color = GizmoColor1;
        Vector3 scale = MBUtility.Scale(Scale, Transform.lossyScale);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, scale);
        MBUtility.DrawGizmoRect(new Rect(-.5f, -0.5f, 1, 1));
        Gizmos.color = GizmoColor2;
        Vector3 h = scale;
        h.Scale(new Vector3(Hollow.x, Hollow.y, 1));
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, h);
        MBUtility.DrawGizmoRect(new Rect(-.5f, -0.5f, 1, 1));
    }
}