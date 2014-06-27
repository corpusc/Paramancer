// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using System;

/// <summary>
/// Lightning parameter
/// </summary>
/// <remarks>
/// See also: \ref paramlightning "Lightning parameter reference"
/// </remarks>
[MBParameterInfo(Menu = "Special/Lightning",Needs="Line-based Emitter (Line/Polygon/Edge)",
                 CanAnimateBirth = MBParameterAnimationMode.None,
                 CanAnimateLife = MBParameterAnimationMode.Mandatory)]
public class MBParticleLightning : MBParameter
{
    public float Speed;
    public float Amplitude;

    Perlin mPerlin;
    int mPtIdx;
    
    public override bool  OnLifetime(MBParticle PT)
    {
        MBLineEmitter emtype=PT.Parent.EmitterType as MBLineEmitter;
        if (emtype) {
            emtype.EvenlySpread = true;
            emtype.SetDistribution(PT.Parent.ParticleCount - 1, emtype.DistributeOverTotal);
            if (mPerlin == null)
                mPerlin = new Perlin();

            float timex = PT.ParticleSystem.GlobalTime * Speed * 0.1365143f;
            float timey = PT.ParticleSystem.GlobalTime * Speed * 1.21688f;
            float timez = PT.ParticleSystem.GlobalTime * Speed * 2.5564f;

            float step = (1.0f / (float)Mathf.Max(1, PT.Parent.ParticleCount - 1));
            // As we set absolute position here, we need to transform the particles from emitter space into particlesystem space            
            PT.Position = ParticleSystem.Transform.InverseTransformPoint(emtype.Transform.TransformPoint(emtype.GetDistributionPoint(mPtIdx, emtype.DistributeOverTotal)));
            Vector3 offset = new Vector3(mPerlin.Noise(timex + PT.Position.x, timex + PT.Position.y, timex + PT.Position.z),
                                       mPerlin.Noise(timey + PT.Position.x, timey + PT.Position.y, timey + PT.Position.z),
                                       mPerlin.Noise(timez + PT.Position.x, timez + PT.Position.y, timez + PT.Position.z));

            PT.Position += (offset * Amplitude * (float)mPtIdx * step);


            if (++mPtIdx >= PT.Parent.ParticleCount)
                mPtIdx = 0;
        }
        return true;
    }

    public override void Reset()
    {
        base.Reset();
        Order = 45;
        Speed = 1f;
        Amplitude = 1f;
        mPtIdx = 0;
    }

    public override void OnPlay()
    {
        base.OnPlay();
        mPtIdx = 0;
    }

}

/// <summary>
/// Class for perlin noise generation. Taken from Unity's procedural example.
/// </summary>
public class Perlin
{
    // Original C code derived from 
    // http://astronomy.swin.edu.au/~pbourke/texture/perlin/perlin.c
    // http://astronomy.swin.edu.au/~pbourke/texture/perlin/perlin.h
    const int B = 0x100;
    const int BM = 0xff;
    const int N = 0x1000;

    int[] p = new int[B + B + 2];
    float[,] g3 = new float[B + B + 2, 3];
    float[,] g2 = new float[B + B + 2, 2];
    float[] g1 = new float[B + B + 2];

    float s_curve(float t)
    {
        return t * t * (3.0F - 2.0F * t);
    }

    float lerp(float t, float a, float b)
    {
        return a + t * (b - a);
    }

    void setup(float value, out int b0, out int b1, out float r0, out float r1)
    {
        float t = value + N;
        b0 = ((int)t) & BM;
        b1 = (b0 + 1) & BM;
        r0 = t - (int)t;
        r1 = r0 - 1.0F;
    }

    float at2(float rx, float ry, float x, float y) { return rx * x + ry * y; }
    float at3(float rx, float ry, float rz, float x, float y, float z) { return rx * x + ry * y + rz * z; }

    public float Noise(float arg)
    {
        int bx0, bx1;
        float rx0, rx1, sx, u, v;
        setup(arg, out bx0, out bx1, out rx0, out rx1);

        sx = s_curve(rx0);
        u = rx0 * g1[p[bx0]];
        v = rx1 * g1[p[bx1]];

        return (lerp(sx, u, v));
    }

    public float Noise(float x, float y)
    {
        int bx0, bx1, by0, by1, b00, b10, b01, b11;
        float rx0, rx1, ry0, ry1, sx, sy, a, b, u, v;
        int i, j;

        setup(x, out bx0, out bx1, out rx0, out rx1);
        setup(y, out by0, out by1, out ry0, out ry1);

        i = p[bx0];
        j = p[bx1];

        b00 = p[i + by0];
        b10 = p[j + by0];
        b01 = p[i + by1];
        b11 = p[j + by1];

        sx = s_curve(rx0);
        sy = s_curve(ry0);

        u = at2(rx0, ry0, g2[b00, 0], g2[b00, 1]);
        v = at2(rx1, ry0, g2[b10, 0], g2[b10, 1]);
        a = lerp(sx, u, v);

        u = at2(rx0, ry1, g2[b01, 0], g2[b01, 1]);
        v = at2(rx1, ry1, g2[b11, 0], g2[b11, 1]);
        b = lerp(sx, u, v);

        return lerp(sy, a, b);
    }

    public float Noise(float x, float y, float z)
    {
        int bx0, bx1, by0, by1, bz0, bz1, b00, b10, b01, b11;
        float rx0, rx1, ry0, ry1, rz0, rz1, sy, sz, a, b, c, d, t, u, v;
        int i, j;

        setup(x, out bx0, out bx1, out rx0, out rx1);
        setup(y, out by0, out by1, out ry0, out ry1);
        setup(z, out bz0, out bz1, out rz0, out rz1);

        i = p[bx0];
        j = p[bx1];

        b00 = p[i + by0];
        b10 = p[j + by0];
        b01 = p[i + by1];
        b11 = p[j + by1];

        t = s_curve(rx0);
        sy = s_curve(ry0);
        sz = s_curve(rz0);

        u = at3(rx0, ry0, rz0, g3[b00 + bz0, 0], g3[b00 + bz0, 1], g3[b00 + bz0, 2]);
        v = at3(rx1, ry0, rz0, g3[b10 + bz0, 0], g3[b10 + bz0, 1], g3[b10 + bz0, 2]);
        a = lerp(t, u, v);

        u = at3(rx0, ry1, rz0, g3[b01 + bz0, 0], g3[b01 + bz0, 1], g3[b01 + bz0, 2]);
        v = at3(rx1, ry1, rz0, g3[b11 + bz0, 0], g3[b11 + bz0, 1], g3[b11 + bz0, 2]);
        b = lerp(t, u, v);

        c = lerp(sy, a, b);

        u = at3(rx0, ry0, rz1, g3[b00 + bz1, 0], g3[b00 + bz1, 2], g3[b00 + bz1, 2]);
        v = at3(rx1, ry0, rz1, g3[b10 + bz1, 0], g3[b10 + bz1, 1], g3[b10 + bz1, 2]);
        a = lerp(t, u, v);

        u = at3(rx0, ry1, rz1, g3[b01 + bz1, 0], g3[b01 + bz1, 1], g3[b01 + bz1, 2]);
        v = at3(rx1, ry1, rz1, g3[b11 + bz1, 0], g3[b11 + bz1, 1], g3[b11 + bz1, 2]);
        b = lerp(t, u, v);

        d = lerp(sy, a, b);

        return lerp(sz, c, d);
    }

    void normalize2(ref float x, ref float y)
    {
        float s;

        s = (float)Math.Sqrt(x * x + y * y);
        x = y / s;
        y = y / s;
    }

    void normalize3(ref float x, ref float y, ref float z)
    {
        float s;
        s = (float)Math.Sqrt(x * x + y * y + z * z);
        x = y / s;
        y = y / s;
        z = z / s;
    }

    public Perlin()
    {
        int i, j, k;
        System.Random rnd = new System.Random();

        for (i = 0; i < B; i++) {
            p[i] = i;
            g1[i] = (float)(rnd.Next(B + B) - B) / B;

            for (j = 0; j < 2; j++)
                g2[i, j] = (float)(rnd.Next(B + B) - B) / B;
            normalize2(ref g2[i, 0], ref g2[i, 1]);

            for (j = 0; j < 3; j++)
                g3[i, j] = (float)(rnd.Next(B + B) - B) / B;


            normalize3(ref g3[i, 0], ref g3[i, 1], ref g3[i, 2]);
        }

        while (--i != 0) {
            k = p[i];
            p[i] = p[j = rnd.Next(B)];
            p[j] = k;
        }

        for (i = 0; i < B + 2; i++) {
            p[B + i] = p[i];
            g1[B + i] = g1[i];
            for (j = 0; j < 2; j++)
                g2[B + i, j] = g2[i, j];
            for (j = 0; j < 3; j++)
                g3[B + i, j] = g3[i, j];
        }
    }
}
