using UnityEngine;

// Genera todos los sonidos por código (no hace falta importar audio).
public class SoundManager : MonoBehaviour
{
    public enum Sfx { Shoot, Pickup, Special, ZombieDie, Hurt, Jump, Win, Lose, Groan, Respawn, Roar, Wave }

    public static SoundManager Instance;
    const int Rate = 44100;

    AudioSource sfx, music;
    AudioClip[] clips;

    static readonly float[] SpecialNotes = { 523f, 659f, 784f, 1047f };
    static readonly float[] WinNotes = { 523f, 659f, 784f, 1047f, 1319f };
    static readonly float[] LoseNotes = { 392f, 349f, 311f, 262f };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Boot()
    {
        if (Instance == null) new GameObject("SoundManager").AddComponent<SoundManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfx = gameObject.AddComponent<AudioSource>();
        sfx.spatialBlend = 0f; sfx.playOnAwake = false;

        music = gameObject.AddComponent<AudioSource>();
        music.spatialBlend = 0f; music.loop = true; music.volume = 0.22f;

        BuildClips();
        music.clip = Make("music", 4f, t =>
            (Sine(55f, t) * 0.5f + Sine(82.5f, t) * 0.3f + Sine(110f, t) * 0.15f)
            * (0.7f + 0.3f * Sine(0.5f, t)) * 0.5f);
        music.Play();
    }

    public static void Play(Sfx s, float volume = 1f)
    {
        if (Instance == null) return;
        Instance.sfx.PlayOneShot(Instance.clips[(int)s], volume * 0.7f);
    }

    void BuildClips()
    {
        clips = new AudioClip[12];

        clips[(int)Sfx.Shoot] = Make("shoot", 0.18f, t =>
            Noise() * Mathf.Exp(-t * 35f) * 0.5f +
            Mathf.Sin(SweepPhase(900f, 120f, 0.18f, t)) * Mathf.Exp(-t * 16f) * 0.7f);

        clips[(int)Sfx.Pickup] = Make("pickup", 0.16f, t =>
            Sine(t < 0.07f ? 880f : 1320f, t) * (1f - t / 0.16f) * 0.5f);

        clips[(int)Sfx.Special] = Make("special", 0.5f, t =>
        {
            int i = Mathf.Min((int)(t / 0.12f), 3);
            float lt = t - i * 0.12f;
            float f = SpecialNotes[i];
            return (Sine(f, t) * 0.6f + Sine(f * 2f, t) * 0.2f) * Mathf.Exp(-lt * 7f)
                   * Mathf.Clamp01((0.5f - t) / 0.1f) * 0.7f;
        });

        clips[(int)Sfx.ZombieDie] = Make("zombieDie", 0.45f, t =>
            (Saw(200f - 300f * t, t) * 0.4f + Noise() * 0.3f) * (1f - t / 0.45f));

        clips[(int)Sfx.Hurt] = Make("hurt", 0.25f, t =>
            (Mathf.Sign(Mathf.Sin(SweepPhase(220f, 80f, 0.25f, t))) * 0.4f + Noise() * 0.15f)
            * Mathf.Exp(-t * 9f));

        clips[(int)Sfx.Jump] = Make("jump", 0.15f, t =>
            Mathf.Sin(SweepPhase(300f, 700f, 0.15f, t)) * (1f - t / 0.15f) * 0.4f);

        clips[(int)Sfx.Win] = Make("win", 1.1f, t =>
        {
            int i = Mathf.Min((int)(t / 0.16f), 4);
            float lt = t - i * 0.16f;
            float f = WinNotes[i];
            return (Sine(f, t) * 0.5f + Sine(f * 2f, t) * 0.15f) * Mathf.Exp(-lt * 3f)
                   * Mathf.Clamp01((1.1f - t) / 0.2f) * 0.8f;
        });

        clips[(int)Sfx.Lose] = Make("lose", 1.4f, t =>
        {
            int i = Mathf.Min((int)(t / 0.33f), 3);
            float lt = t - i * 0.33f;
            float f = LoseNotes[i];
            return (Sine(f, t) * 0.5f + Saw(f * 0.5f, t) * 0.15f) * Mathf.Exp(-lt * 2.5f)
                   * Mathf.Clamp01((1.4f - t) / 0.2f);
        });

        clips[(int)Sfx.Groan] = Make("groan", 0.8f, t =>
            Saw(80f - 15f * t, t) * 0.35f * (0.6f + 0.4f * Sine(7f, t)) * Mathf.Sin(Mathf.PI * t / 0.8f));

        clips[(int)Sfx.Respawn] = Make("respawn", 0.3f, t =>
            Mathf.Sin(SweepPhase(200f, 900f, 0.3f, t)) * (1f - t / 0.3f) * 0.5f);

        clips[(int)Sfx.Roar] = Make("roar", 1.2f, t =>
            (Saw(100f - 50f * t, t) * 0.5f + Noise() * 0.25f) * (0.7f + 0.3f * Sine(14f, t))
            * Mathf.Sin(Mathf.PI * Mathf.Min(t / 1.2f, 1f)));

        clips[(int)Sfx.Wave] = Make("wave", 0.7f, t =>
            (Saw(196f, t) * 0.25f + Saw(294f, t) * 0.2f) * Mathf.Sin(Mathf.PI * t / 0.7f));
    }

    static AudioClip Make(string name, float duration, System.Func<float, float> f)
    {
        int n = (int)(Rate * duration);
        var data = new float[n];
        for (int i = 0; i < n; i++)
            data[i] = Mathf.Clamp(f((float)i / Rate), -1f, 1f);
        var clip = AudioClip.Create(name, n, 1, Rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    static float Sine(float f, float t) { return Mathf.Sin(2f * Mathf.PI * f * t); }
    static float Saw(float f, float t) { float p = f * t; return 2f * (p - Mathf.Floor(p)) - 1f; }
    static float Noise() { return Random.value * 2f - 1f; }
    static float SweepPhase(float f0, float f1, float dur, float t)
    {
        return 2f * Mathf.PI * (f0 * t + (f1 - f0) * t * t / (2f * dur));
    }
}
