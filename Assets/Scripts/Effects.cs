using UnityEngine;

public static class Effects
{
    static Material mat;

    static Material GetMat()
    {
        if (mat != null) return mat;
        Shader sh = Shader.Find("Sprites/Default");
        if (sh == null) sh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        mat = new Material(sh);
        return mat;
    }

    public static void Burst(Vector3 pos, Color color, int count = 20, float speed = 4f,
                             float size = 0.15f, float life = 0.5f)
    {
        var go = new GameObject("FX");
        go.transform.position = pos;

        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = false;
        main.useUnscaledTime = true;   // se anima aunque el juego esté en pausa
        main.duration = 0.5f;
        main.startLifetime = life;
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.5f, speed);
        main.startSize = new ParticleSystem.MinMaxCurve(size * 0.5f, size);
        main.startColor = color;
        main.gravityModifier = 0.6f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        var sh = ps.shape;
        sh.shapeType = ParticleSystemShapeType.Sphere;
        sh.radius = 0.2f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        col.color = g;

        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        go.GetComponent<ParticleSystemRenderer>().sharedMaterial = GetMat();

        ps.Play();
        Object.Destroy(go, life + 1f);
    }
}
