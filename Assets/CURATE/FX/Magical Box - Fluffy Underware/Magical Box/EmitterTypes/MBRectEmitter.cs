// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Provides an rectangle emitter
/// </summary>
[MBEmitterTypeInfo(Menu = "2D/Rect")]
public class MBRectEmitter : MBEmitterType
{
    #region ### Inspector Fields ###
    /// <summary>
    /// True to spawn particles evenly spread along the rectangle
    /// </summary>
    public bool EvenlySpread;
    /// <summary>
    /// Number of spawnpoints on the x-asis the rectangle will be divided into when evenly spread
    /// </summary>
    public int DistributionPointsX;
    /// <summary>
    /// Number of spawnpoints on the y-asis the rectangle will be divided into when evenly spread
    /// </summary>
    public int DistributionPointsY;
    #endregion

    Vector2 mDistributionStep;
    int mDistributionX;
    int mDistributionY;

    public override Vector3 GetPosition(MBParticle PT)
    {
        
        Rect r = new Rect(-Scale.x * .5f, -Scale.y * .5f, Scale.x, Scale.y);
        if (!EvenlySpread)
            return new Vector3(Random.Range(r.xMin, r.xMax), Random.Range(r.yMin, r.yMax), 0);
        else{
            Vector3 v = new Vector3(r.xMin + mDistributionStep.x * mDistributionX,
                                    r.yMax - mDistributionStep.y * mDistributionY, 0);
            if (++mDistributionX >= DistributionPointsX) {
                mDistributionX = 0;
                mDistributionY++;
            }
            if (mDistributionY >= DistributionPointsY)
                mDistributionY = 0;
            return v;
        }
        
    }

    public override void OnPlay()
    {
        base.OnPlay();
        mDistributionX = 0;
        mDistributionY = 0;
        mDistributionStep = new Vector2(Scale.x / Mathf.Max(1, (DistributionPointsX - 1)), Scale.y / Mathf.Max(1, (DistributionPointsY - 1)));
    }

    public override void Validate()
    {
        base.Validate();
        DistributionPointsX = Mathf.Max(1, DistributionPointsX);
        DistributionPointsY = Mathf.Max(1, DistributionPointsY);
        mDistributionStep = new Vector2(Scale.x / Mathf.Max(1, (DistributionPointsX - 1)), Scale.y / Mathf.Max(1, (DistributionPointsY - 1)));
    }

    public override void Reset()
    {
        base.Reset();
        EvenlySpread = false;
        DistributionPointsX = 0;
        DistributionPointsY = 0;
        mDistributionX = 0;
        mDistributionY = 0;
    }

    protected override void DoGizmos()
    {
        base.DoGizmos();
        Gizmos.color = GizmoColor1;
        Vector3 scale = MBUtility.Scale(Scale, Transform.lossyScale);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, scale);
        MBUtility.DrawGizmoRect(new Rect(-.5f, -0.5f, 1, 1));
    }
}
