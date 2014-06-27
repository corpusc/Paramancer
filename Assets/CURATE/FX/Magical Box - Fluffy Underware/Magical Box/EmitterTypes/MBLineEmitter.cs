// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Provides a line emitter type
/// </summary>
[MBEmitterTypeInfo(Menu = "Vector/Line")]
public class MBLineEmitter : MBEmitterType
{
    #region ### Inspector Fields ###
    /// <summary>
    /// Array of line points
    /// </summary>
    public Vector3[] Points = new Vector3[0];
    /// <summary>
    /// True to spawn particles evenly spread along the arc/cirlce
    /// </summary>
    public bool EvenlySpread;
    /// <summary>
    /// Number of spawnpoints the arc/circle will be divided into when evenly spread
    /// </summary>
    public int DistributionPoints;
    /// <summary>
    /// Distribute over total length (slower) or over segment lengths
    /// </summary>
    public bool DistributeOverTotal;

    #endregion

    protected int mDistribution;
    protected float mDistributionStep;

    protected float[] mLengths = new float[0];
    protected float mTotalLength;

    public override Vector3 GetPosition(MBParticle PT)
    {
        Vector3 v = Vector3.zero;
        if (Points.Length>0) {
            int idx = 0;
            if (!EvenlySpread) {
                idx = Mathf.Max(0, Random.Range(0, Points.Length / 2)) * 2;
                v = Vector3.Lerp(Points[idx], Points[idx+1], Random.value);
            }
            else if (DistributionPoints > 0) {
                v = GetDistributionPoint(mDistribution, DistributeOverTotal);
                mDistribution++;
                if (mDistribution >= DistributionPoints)
                    mDistribution = 0;
            }
        }
        return v;
    }

    /// <summary>
    /// Gets a distribution point in emitter's local scale
    /// </summary>
    /// <param name="distribution">distribution index</param>
    /// <param name="overTotal">whether segments length will taken into the calculation</param>
    /// <returns>a point in local space</returns>
    public virtual Vector3 GetDistributionPoint(int distribution, bool overTotal)
    {
        float f = mDistributionStep * distribution;
        float p = 0;
        int idx = 0;
        if (overTotal) {
            float t = 0;
            for (int i = 0; i < mLengths.Length; i++) {
                t += mLengths[i];
                if (t >= f) {
                    idx = i * 2;
                    p = (mLengths[i] - (t - f)) / Mathf.Max(0.00001f, mLengths[i]);
                    break;
                }
            }
        }
        else {
            idx = Mathf.Min(Points.Length - 2, ((int)f) * 2);
            p = f % 1;
        }
        idx = Mathf.Min(idx, Points.Length - 2);
        return Vector3.Lerp(Points[idx], Points[idx + 1], (p == 0 && f > 0) ? 1 : p);
    }

    public void SetDistribution(int points, bool overTotal)
    {
        if (points != DistributionPoints || DistributeOverTotal != overTotal) {
            DistributionPoints = points;
            DistributeOverTotal = overTotal;
            Validate();
        }
    }

    public override void Validate()
    {
        base.Validate();

        if (EvenlySpread && DistributionPoints > 0) {
            if (DistributeOverTotal) {
                CalcLengths();
                mDistributionStep = (mTotalLength / Mathf.Max(1, DistributionPoints - 1));
            }
            else
                mDistributionStep = (((float)Points.Length / 2) / (float)Mathf.Max(1, DistributionPoints - 1));
            
        }
        if (Emitter.IsTrail) {
            EvenlySpread = false;
            DistributionPoints = 0;
        }
    }

    public override void OnPlay()
    {
        base.OnPlay();

        if (EvenlySpread && DistributionPoints > 0) {
            if (DistributeOverTotal) {
                CalcLengths();
                mDistributionStep = (mTotalLength / Mathf.Max(1, DistributionPoints - 1));
            } else
                mDistributionStep = (((float)Points.Length / 2) / (float)Mathf.Max(1, DistributionPoints - 1));
        }
        
    }

    void CalcLengths()
    {
        mLengths = new float[Points.Length / 2];
        mTotalLength = 0;

        for (int p = 0; p < mLengths.Length; p++) {
            mLengths[p] = (Points[p * 2 + 1] - Points[p * 2]).magnitude;
            mTotalLength += mLengths[p];
        }
    }

    public override void Reset()
    {
        base.Reset();
        EvenlySpread = false;
        DistributionPoints = 0;
        DistributeOverTotal = false;
    }

    protected override void DoGizmos()
    {
        base.DoGizmos();
        Gizmos.color = GizmoColor1;
        Vector3 scale = MBUtility.Scale(Scale, Transform.lossyScale);
        Gizmos.matrix = Matrix4x4.TRS(Transform.position, Transform.rotation, scale);
        for (int i = 0; i < Points.Length-1; i += 2) {
            Gizmos.DrawLine(Points[i], Points[i + 1]);
        }
        
    }


}