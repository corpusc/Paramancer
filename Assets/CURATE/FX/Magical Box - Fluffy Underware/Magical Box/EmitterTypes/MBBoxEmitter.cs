// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Provides an rectangle emitter
/// </summary>
[MBEmitterTypeInfo(Menu = "3D/Box")]
public class MBBoxEmitter : MBEmitterType
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
    /// <summary>
    /// Number of spawnpoints on the z-asis the rectangle will be divided into when evenly spread
    /// </summary>
    public int DistributionPointsZ;
    #endregion

    Vector3 mDistributionStep;
    int mDistributionX;
    int mDistributionY;
    int mDistributionZ;

    public override Vector3 GetPosition(MBParticle PT)
    {
        Bounds b = new Bounds(Vector3.zero, Scale);
        if (!EvenlySpread) {
            return new Vector3(Random.Range(b.min.x, b.max.x), Random.Range(b.min.y, b.max.y), Random.Range(b.min.z, b.max.z));
        }
        else {
            Vector3 v = new Vector3(b.min.x + mDistributionStep.x * mDistributionX,
                                    b.max.y - mDistributionStep.y * mDistributionY,
                                    b.max.z - mDistributionStep.z * mDistributionZ);
            if (++mDistributionX >= DistributionPointsX) {
                mDistributionX = 0;
                mDistributionY++;
            }
            if (mDistributionY >= DistributionPointsY) {
                mDistributionY = 0;
                mDistributionZ++;
            }
            if (mDistributionZ >= DistributionPointsZ)
                mDistributionZ = 0;
            return v;
        }

    }

    public override void OnPlay()
    {
        base.OnPlay();
        mDistributionX = 0;
        mDistributionY = 0;
        mDistributionZ = 0;
        mDistributionStep = new Vector3(Scale.x / Mathf.Max(1, DistributionPointsX - 1), Scale.y / Mathf.Max(1, DistributionPointsY - 1),Scale.z/Mathf.Max(1,DistributionPointsZ-1));
    }

    public override void Validate()
    {
        base.Validate();
        DistributionPointsX = Mathf.Max(1, DistributionPointsX);
        DistributionPointsY = Mathf.Max(1, DistributionPointsY);
        DistributionPointsZ = Mathf.Max(1, DistributionPointsZ);
        mDistributionStep = new Vector3(Scale.x / Mathf.Max(1, DistributionPointsX - 1), Scale.y / Mathf.Max(1, DistributionPointsY - 1), Scale.z / Mathf.Max(1, DistributionPointsZ - 1));
    }

    public override void Reset()
    {
        base.Reset();
        EvenlySpread = false;
        DistributionPointsX = 0;
        DistributionPointsY = 0;
        DistributionPointsZ = 0;
        mDistributionX = 0;
        mDistributionY = 0;
        mDistributionZ = 0;
    }

    protected override void DoGizmos()
    {
        base.DoGizmos();
        Gizmos.color = GizmoColor1;
        Vector3 scale = MBUtility.Scale(Scale, Transform.lossyScale);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, scale);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
