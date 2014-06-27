using UnityEngine;
using System.Collections;

/// <summary>
/// Provides a polygon line emitter type
/// </summary>
[MBEmitterTypeInfo(Menu = "Vector/Polygon")]
public class MBPolygonEmitter : MBLineEmitter
{
    
    public override Vector3 GetPosition(MBParticle PT)
    {
        Vector3 v = Vector3.zero;
        if (Points.Length > 0) {
            int idx = 0;
            if (!EvenlySpread) {
                idx = Mathf.Max(0, Random.Range(0, Points.Length - 1));
                v = Vector3.Lerp(Points[idx], Points[idx + 1], Random.value);
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

    public override Vector3 GetDistributionPoint(int distribution, bool overTotal)
    {
        float f = mDistributionStep * distribution;
        float p = 0;
        int idx = 0;
        if (overTotal) {
            float t = 0;
            for (int i = 0; i < mLengths.Length; i++) {
                t += mLengths[i];
                if (t >= f) {
                    idx = i;
                    p = (mLengths[i] - (t - f)) / Mathf.Max(0.00001f, mLengths[i]);
                    break;
                }
            }
        }
        else {
            idx = Mathf.Min(Points.Length - 2, ((int)f));
            p = f % 1;
        }
        return  Vector3.Lerp(Points[idx], Points[idx + 1], (p == 0 && f > 0) ? 1 : p);
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
                mDistributionStep = (((float)Points.Length-1) / (float)Mathf.Max(1, DistributionPoints - 1));

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
            }
            else
                mDistributionStep = (((float)Points.Length-1) / (float)Mathf.Max(1, DistributionPoints - 1));
        }

    }

    void CalcLengths()
    {
        mLengths = new float[Points.Length-1];
        mTotalLength = 0;

        for (int p = 0; p < mLengths.Length; p++) {
            mLengths[p] = (Points[p+1] - Points[p]).magnitude;
            mTotalLength += mLengths[p];
        }
    }

    public override void Reset()
    {
        base.Reset();
    }

    protected override void DoGizmos()
    {
        base.DoGizmos();
        Gizmos.color = GizmoColor1;
        Vector3 scale = MBUtility.Scale(Scale, Transform.lossyScale);
        Gizmos.matrix = Matrix4x4.TRS(Transform.position, Transform.rotation, scale);
        for (int i = 0; i < Points.Length - 1; i ++ ) {
            Gizmos.DrawLine(Points[i], Points[i + 1]);
        }

    }

}
