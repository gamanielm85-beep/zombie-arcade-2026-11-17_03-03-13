using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

// Menú: Zombie Game > Construir proyecto completo
public static class ZombieGameBuilder
{
    const string MatDir = "Assets/Materials";
    const string PrefabDir = "Assets/Prefabs";
    const string ScenePath = "Assets/Scenes/Nivel1.unity";

    [MenuItem("Zombie Game/Construir proyecto completo")]
    public static void BuildAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        // Asegura que el proyecto siempre tenga una escena jugable al abrir o compilar.
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

        // ---------- Carpetas y tags ----------
        EnsureFolder("Assets", "Materials");
        EnsureFolder("Assets", "Prefabs");
        EnsureFolder("Assets", "Scenes");
        EnsureFolder("Assets", "Scripts");
        foreach (var t in new[] { "Normal", "Especial", "Zombie" }) InternalEditorUtility.AddTag(t);

        // ---------- Texturas generadas por código ----------
        var tSuelo = MakeTex("T_Suelo", (x, y) =>
        {
            bool line = x % 32 < 2 || y % 32 < 2;
            float n = Mathf.PerlinNoise(x * 0.15f, y * 0.15f) * 0.15f;
            float v = 0.40f + n;
            return line ? new Color(0.12f, 0.12f, 0.14f) : new Color(v, v, v * 1.05f);
        });
        var tLadrillo = MakeTex("T_Ladrillo", (x, y) =>
        {
            int row = y / 16;
            int xo = (row % 2 == 0) ? 0 : 16;
            bool mortar = (y % 16 < 2) || ((x + xo) % 32 < 2);
            float n = Mathf.PerlinNoise(x * 0.2f, y * 0.2f) * 0.2f;
            return mortar ? new Color(0.18f, 0.17f, 0.17f)
                          : new Color(0.45f + n, 0.17f + n * 0.5f, 0.13f + n * 0.4f);
        });
        var tMadera = MakeTex("T_Madera", (x, y) =>
        {
            bool edge = x < 4 || y < 4 || x > 59 || y > 59;
            float g = Mathf.PerlinNoise(x * 0.05f, y * 0.8f) * 0.15f;
            return edge ? new Color(0.22f, 0.13f, 0.06f) : new Color(0.45f + g, 0.28f + g, 0.12f + g * 0.5f);
        });

        // ---------- Materiales (15, con propiedades distintas) ----------
        var mSuelo    = Mat("M_Suelo",    Color.white, 0.0f, 0.15f, null, tSuelo, new Vector2(10f, 10f));
        var mMuro     = Mat("M_Muro",     Color.white, 0.1f, 0.10f, null, tLadrillo, new Vector2(5f, 1f));
        var mRampa    = Mat("M_Rampa",    new Color(0.70f, 0.35f, 0.10f), 0.7f, 0.55f);
        var mJugador  = Mat("M_Jugador",  new Color(0.15f, 0.35f, 0.85f), 0.2f, 0.60f);
        var mCasco    = Mat("M_Casco",    new Color(0.20f, 0.20f, 0.22f), 0.9f, 0.70f);
        var mZombie   = Mat("M_Zombie",   new Color(0.25f, 0.55f, 0.20f), 0.0f, 0.05f);
        var mOjo      = Mat("M_Ojo",      new Color(1f, 0.1f, 0.1f), 0.0f, 0.5f, new Color(1f, 0.05f, 0.05f) * 3f);
        var mMunicion = Mat("M_Municion", new Color(1.00f, 0.55f, 0.05f), 0.0f, 0.30f, new Color(1f, 0.45f, 0f) * 1.5f);
        var mDorado   = Mat("M_Dorado",   new Color(1.00f, 0.80f, 0.20f), 1.0f, 0.90f, new Color(1f, 0.75f, 0.1f) * 0.8f);
        var mBala     = Mat("M_Bala",     new Color(1.00f, 1.00f, 0.40f), 0.0f, 0.50f, new Color(1f, 1f, 0.3f) * 3f);
        var mLapida   = Mat("M_Lapida",   new Color(0.45f, 0.50f, 0.47f), 0.0f, 0.10f);
        var mCaja     = Mat("M_Caja",     Color.white, 0.0f, 0.10f, null, tMadera, Vector2.one);
        var mMadera   = Mat("M_Madera",   new Color(0.25f, 0.15f, 0.07f), 0.0f, 0.10f);
        var mFuego    = Mat("M_Fuego",    new Color(1f, 0.6f, 0.1f), 0.0f, 0.5f, new Color(1f, 0.5f, 0.05f) * 4f);
        var mJefe     = Mat("M_Jefe",     new Color(0.40f, 0.10f, 0.50f), 0.3f, 0.40f, new Color(0.3f, 0.0f, 0.4f) * 0.6f);

        // ---------- Escena nueva ----------
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // ---------- Prefabs ----------
        // Munición (normal, +1, gira lento)
        var n = Prim(PrimitiveType.Cube, "Coleccionable_Normal", Vector3.zero, Vector3.one * 0.6f, mMunicion, null);
        var nCol = n.GetComponent<BoxCollider>(); nCol.isTrigger = true; nCol.size = Vector3.one * 1.6f;
        n.tag = "Normal";
        n.AddComponent<Collectible>().rotationSpeed = 90f;
        var nLight = n.AddComponent<Light>();
        nLight.type = LightType.Point; nLight.color = new Color(1f, 0.55f, 0.1f); nLight.range = 2.5f; nLight.intensity = 1.5f;
        var normalPrefab = SavePrefab(n, "Coleccionable_Normal");

        // Cráneo dorado (especial, +3, gira rápido, forma distinta)
        var s = new GameObject("Coleccionable_Especial");
        var sCol = s.AddComponent<SphereCollider>(); sCol.isTrigger = true; sCol.radius = 0.8f;
        Prim(PrimitiveType.Cube, "Gema_A", Vector3.zero, Vector3.one * 0.7f, mDorado, s.transform, false)
            .transform.localRotation = Quaternion.Euler(45f, 0f, 45f);
        Prim(PrimitiveType.Cube, "Gema_B", Vector3.zero, Vector3.one * 0.7f, mDorado, s.transform, false)
            .transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
        s.tag = "Especial";
        s.AddComponent<Collectible>().rotationSpeed = 240f;
        var sLight = s.AddComponent<Light>();
        sLight.type = LightType.Point; sLight.color = new Color(1f, 0.85f, 0.2f); sLight.range = 3.5f; sLight.intensity = 2.5f;
        var especialPrefab = SavePrefab(s, "Coleccionable_Especial");

        // Bala
        var b = Prim(PrimitiveType.Sphere, "Bala", Vector3.zero, Vector3.one * 0.3f, mBala, null);
        b.GetComponent<SphereCollider>().isTrigger = true;
        var brb = b.AddComponent<Rigidbody>();
        brb.useGravity = false;
        brb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        b.AddComponent<Bullet>();
        var bulletPrefab = SavePrefab(b, "Bala");

        // Zombie (modelo 3D COMPUESTO: cuerpo + cabeza + 2 brazos + 2 ojos)
        var z = new GameObject("Zombie");
        z.tag = "Zombie";
        var zCap = z.AddComponent<CapsuleCollider>();
        zCap.center = new Vector3(0f, 1f, 0f); zCap.height = 2f; zCap.radius = 0.45f;
        z.AddComponent<Rigidbody>();
        z.AddComponent<Zombie>();
        Prim(PrimitiveType.Capsule, "Cuerpo",   new Vector3(0f, 0.9f, 0f),       Vector3.one * 0.8f,              mZombie, z.transform, false);
        Prim(PrimitiveType.Sphere,  "Cabeza",   new Vector3(0f, 1.85f, 0f),      Vector3.one * 0.5f,              mZombie, z.transform, false);
        Prim(PrimitiveType.Cube,    "BrazoIzq", new Vector3(-0.5f, 1.3f, 0.45f), new Vector3(0.2f, 0.2f, 0.9f),   mZombie, z.transform, false);
        Prim(PrimitiveType.Cube,    "BrazoDer", new Vector3(0.5f, 1.3f, 0.45f),  new Vector3(0.2f, 0.2f, 0.9f),   mZombie, z.transform, false);
        Prim(PrimitiveType.Sphere,  "OjoIzq",   new Vector3(-0.12f, 1.9f, 0.2f), Vector3.one * 0.1f,              mOjo,    z.transform, false);
        Prim(PrimitiveType.Sphere,  "OjoDer",   new Vector3(0.12f, 1.9f, 0.2f),  Vector3.one * 0.1f,              mOjo,    z.transform, false);
        var zombiePrefab = SavePrefab(z, "Zombie");

        // Jefe final (modelo 3D COMPUESTO grande: cuerpo, cabeza, brazos, ojos, cuernos y hombreras)
        var jf = new GameObject("JefeZombie");
        jf.tag = "Zombie";
        var jCap = jf.AddComponent<CapsuleCollider>();
        jCap.center = new Vector3(0f, 1f, 0f); jCap.height = 2f; jCap.radius = 0.5f;
        var jrb = jf.AddComponent<Rigidbody>(); jrb.mass = 10f;
        var bossComp = jf.AddComponent<Boss>(); bossComp.minionPrefab = zombiePrefab;
        Prim(PrimitiveType.Capsule, "Cuerpo",    new Vector3(0f, 0.9f, 0f),       Vector3.one * 0.9f,             mJefe,  jf.transform, false);
        Prim(PrimitiveType.Sphere,  "Cabeza",    new Vector3(0f, 1.9f, 0f),       Vector3.one * 0.55f,            mJefe,  jf.transform, false);
        Prim(PrimitiveType.Cube,    "BrazoIzq",  new Vector3(-0.6f, 1.3f, 0.5f),  new Vector3(0.28f, 0.28f, 1f),  mJefe,  jf.transform, false);
        Prim(PrimitiveType.Cube,    "BrazoDer",  new Vector3(0.6f, 1.3f, 0.5f),   new Vector3(0.28f, 0.28f, 1f),  mJefe,  jf.transform, false);
        Prim(PrimitiveType.Sphere,  "OjoIzq",    new Vector3(-0.13f, 1.95f, 0.23f), Vector3.one * 0.12f,          mOjo,   jf.transform, false);
        Prim(PrimitiveType.Sphere,  "OjoDer",    new Vector3(0.13f, 1.95f, 0.23f),  Vector3.one * 0.12f,          mOjo,   jf.transform, false);
        Prim(PrimitiveType.Sphere,  "HombreraIzq", new Vector3(-0.55f, 1.55f, 0f), Vector3.one * 0.35f,          mCasco, jf.transform, false);
        Prim(PrimitiveType.Sphere,  "HombreraDer", new Vector3(0.55f, 1.55f, 0f),  Vector3.one * 0.35f,          mCasco, jf.transform, false);
        Prim(PrimitiveType.Cube,    "CuernoIzq", new Vector3(-0.2f, 2.3f, 0f),    new Vector3(0.08f, 0.4f, 0.08f), mCasco, jf.transform, false)
            .transform.localRotation = Quaternion.Euler(0f, 0f, 25f);
        Prim(PrimitiveType.Cube,    "CuernoDer", new Vector3(0.2f, 2.3f, 0f),     new Vector3(0.08f, 0.4f, 0.08f), mCasco, jf.transform, false)
            .transform.localRotation = Quaternion.Euler(0f, 0f, -25f);
        jf.transform.localScale = Vector3.one * 2.2f;
        var bossPrefab = SavePrefab(jf, "JefeZombie");

        // ---------- Contenedores de la jerarquía ----------
        var escenario = new GameObject("--Escenario--").transform;
        var plataformas = new GameObject("Plataformas").transform; plataformas.SetParent(escenario);
        var muros = new GameObject("Muros").transform; muros.SetParent(escenario);
        var decoracion = new GameObject("Decoracion").transform; decoracion.SetParent(escenario);
        var coleccionables = new GameObject("--Coleccionables--").transform;
        var enemigos = new GameObject("--Enemigos--").transform;
        var spawnPoints = new GameObject("SpawnPoints").transform; spawnPoints.SetParent(enemigos);
        var managers = new GameObject("--Managers--").transform;

        // ---------- Plataformas y rampa ----------
        Prim(PrimitiveType.Cube, "PlataformaBaja", new Vector3(0f, -0.5f, 0f), new Vector3(20f, 1f, 20f), mSuelo, plataformas);
        Prim(PrimitiveType.Cube, "PlataformaAlta", new Vector3(0f, 2.5f, 24f), new Vector3(20f, 1f, 10f), mSuelo, plataformas);

        float run = 9f, rise = 3f;
        float angle = Mathf.Atan2(rise, run) * Mathf.Rad2Deg;
        float length = Mathf.Sqrt(run * run + rise * rise);
        var rampRot = Quaternion.Euler(-angle, 0f, 0f);
        Vector3 surfaceCenter = new Vector3(0f, rise / 2f, 10f + run / 2f);
        Vector3 rampPos = surfaceCenter - (rampRot * Vector3.up) * 0.25f;
        var ramp = Prim(PrimitiveType.Cube, "Rampa", rampPos, new Vector3(6f, 0.5f, length), mRampa, plataformas);
        ramp.transform.rotation = rampRot;

        // ---------- Muros (la plataforma baja NO tiene muro oeste: borde abierto) ----------
        Prim(PrimitiveType.Cube, "Muro_Baja_Este",      new Vector3(9.5f, 1f, 0f),      new Vector3(1f, 2f, 20f), mMuro, muros);
        Prim(PrimitiveType.Cube, "Muro_Baja_Fondo",     new Vector3(0f, 1f, -9.5f),     new Vector3(20f, 2f, 1f), mMuro, muros);
        Prim(PrimitiveType.Cube, "Muro_Baja_FrenteIzq", new Vector3(-6.5f, 1f, 9.5f),   new Vector3(7f, 2f, 1f),  mMuro, muros);
        Prim(PrimitiveType.Cube, "Muro_Baja_FrenteDer", new Vector3(6.5f, 1f, 9.5f),    new Vector3(7f, 2f, 1f),  mMuro, muros);
        Prim(PrimitiveType.Cube, "Muro_Alta_Este",      new Vector3(9.5f, 4f, 24f),     new Vector3(1f, 2f, 10f), mMuro, muros);
        Prim(PrimitiveType.Cube, "Muro_Alta_Oeste",     new Vector3(-9.5f, 4f, 24f),    new Vector3(1f, 2f, 10f), mMuro, muros);
        Prim(PrimitiveType.Cube, "Muro_Alta_Fondo",     new Vector3(0f, 4f, 28.5f),     new Vector3(20f, 2f, 1f), mMuro, muros);
        Prim(PrimitiveType.Cube, "Muro_Alta_FrenteIzq", new Vector3(-6.5f, 4f, 19.5f),  new Vector3(7f, 2f, 1f),  mMuro, muros);
        Prim(PrimitiveType.Cube, "Muro_Alta_FrenteDer", new Vector3(6.5f, 4f, 19.5f),   new Vector3(7f, 2f, 1f),  mMuro, muros);

        // ---------- Decoración: lápidas, cajas y antorchas ----------
        Lapida(new Vector3(-3.5f, 0f, -8.1f), 0f,  decoracion, mLapida);
        Lapida(new Vector3(3.5f, 0f, -8.1f),  0f,  decoracion, mLapida);
        Lapida(new Vector3(8.1f, 0f, -2f),    90f, decoracion, mLapida);
        Lapida(new Vector3(8.1f, 0f, 2.5f),   90f, decoracion, mLapida);
        Lapida(new Vector3(8.1f, 3f, 25.5f),  90f, decoracion, mLapida);
        Lapida(new Vector3(-8.1f, 3f, 22.5f), 90f, decoracion, mLapida);
        Lapida(new Vector3(-8.1f, 3f, 26f),   90f, decoracion, mLapida);

        Caja(new Vector3(-7.5f, 0.5f, 7.2f), 10f, decoracion, mCaja);
        Caja(new Vector3(-6.4f, 0.5f, 7.6f), 35f, decoracion, mCaja);
        Caja(new Vector3(7.5f, 0.5f, 7.5f),  20f, decoracion, mCaja);
        Caja(new Vector3(5.5f, 3.5f, 26.7f), 15f, decoracion, mCaja);
        Caja(new Vector3(-5f, 3.5f, 26.7f),  40f, decoracion, mCaja);

        Torch(new Vector3(8.3f, 0f, -8.3f),  decoracion, mMadera, mFuego);
        Torch(new Vector3(-8.3f, 0f, -8.3f), decoracion, mMadera, mFuego);
        Torch(new Vector3(8.3f, 0f, 8.3f),   decoracion, mMadera, mFuego);
        Torch(new Vector3(-8.3f, 0f, 8.3f),  decoracion, mMadera, mFuego);
        Torch(new Vector3(8.3f, 3f, 27.3f),  decoracion, mMadera, mFuego);
        Torch(new Vector3(-8.3f, 3f, 27.3f), decoracion, mMadera, mFuego);

        // ---------- Iluminación direccional + ambiente nocturno ----------
        var light = Object.FindFirstObjectByType<Light>();
        light.name = "Luz Direccional";
        light.type = LightType.Directional;
        light.color = new Color(0.65f, 0.75f, 1f);
        light.intensity = 1.1f;
        light.shadows = LightShadows.Soft;
        light.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
        light.transform.SetParent(escenario);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.14f, 0.16f, 0.25f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.05f, 0.06f, 0.10f);
        RenderSettings.fogStartDistance = 18f;
        RenderSettings.fogEndDistance = 55f;

        // ---------- Jugador (modelo compuesto: cuerpo + casco + arma) ----------
        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Jugador";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1.1f, -5f);
        player.GetComponent<Renderer>().sharedMaterial = mJugador;
        var rb = player.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        player.AddComponent<PlayerController>();
        player.AddComponent<FallRespawn>();
        player.AddComponent<PlayerCollector>();
        player.AddComponent<PlayerHealth>();
        Prim(PrimitiveType.Sphere, "Casco", new Vector3(0f, 0.9f, 0.05f), Vector3.one * 0.6f, mCasco, player.transform, false);
        Prim(PrimitiveType.Cube, "Arma", new Vector3(0.4f, 0.1f, 0.55f), new Vector3(0.15f, 0.15f, 0.9f), mCasco, player.transform, false);
        var muzzle = new GameObject("Muzzle").transform;
        muzzle.SetParent(player.transform, false);
        muzzle.localPosition = new Vector3(0.4f, 0.1f, 1.05f);
        var shooter = player.AddComponent<PlayerShooter>();
        shooter.bulletPrefab = bulletPrefab;
        shooter.muzzle = muzzle;

        // ---------- Cámara ----------
        var cam = Camera.main;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.06f, 0.10f);
        cam.transform.position = player.transform.position + new Vector3(0f, 12f, -9f);
        cam.transform.rotation = Quaternion.Euler(53f, 0f, 0f);
        cam.gameObject.AddComponent<CameraFollow>().target = player.transform;

        // ---------- Coleccionables: 6 normales + 3 especiales ----------
        Vector3[] normales =
        {
            new Vector3(-6f, 1f, -6f), new Vector3(6f, 1f, -6f), new Vector3(-6f, 1f, 4f),
            new Vector3(6f, 1f, 4f),   new Vector3(0f, 1f, 2f),  new Vector3(-6f, 4f, 23f)
        };
        Vector3[] especiales =
        {
            new Vector3(0f, 4f, 26f), new Vector3(7.5f, 4f, 22f), new Vector3(-8.5f, 1f, 0f)
        };
        int i = 1;
        foreach (var p in normales)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(normalPrefab, coleccionables);
            go.name = "Normal_" + i++; go.transform.position = p;
        }
        i = 1;
        foreach (var p in especiales)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(especialPrefab, coleccionables);
            go.name = "Especial_" + i++; go.transform.position = p;
        }

        // ---------- Spawners de zombies ----------
        Vector3[] spawns =
        {
            new Vector3(-7f, 0.1f, -7f), new Vector3(7f, 0.1f, -7f),
            new Vector3(-7f, 3.1f, 27f), new Vector3(7f, 3.1f, 27f)
        };
        var spawnList = new Transform[spawns.Length];
        for (int k = 0; k < spawns.Length; k++)
        {
            var sp = new GameObject("Spawn_" + (k + 1)).transform;
            sp.SetParent(spawnPoints); sp.position = spawns[k];
            spawnList[k] = sp;
        }

        // ---------- Managers ----------
        var gm = new GameObject("GameManager"); gm.transform.SetParent(managers);
        gm.AddComponent<GameManager>().targetScore = 12;
        var wm = new GameObject("WaveManager"); wm.transform.SetParent(managers);
        var waves = wm.AddComponent<WaveManager>();
        waves.zombiePrefab = zombiePrefab;
        waves.bossPrefab = bossPrefab;
        waves.spawnPoints = spawnList;
        waves.totalWaves = 4;

        // Puntos de paso en la rampa (para que los zombies cambien de plataforma)
        var wpObj = new GameObject("RampWaypoints"); wpObj.transform.SetParent(managers);
        var wps = wpObj.AddComponent<RampWaypoints>();
        var wpBottom = new GameObject("Rampa_Base").transform; wpBottom.SetParent(wpObj.transform);
        wpBottom.position = new Vector3(0f, 0f, 8f);
        var wpTop = new GameObject("Rampa_Cima").transform; wpTop.SetParent(wpObj.transform);
        wpTop.position = new Vector3(0f, 3f, 21f);
        wps.bottom = wpBottom;
        wps.top = wpTop;

        // ---------- Guardar escena y registrarla en el Build ----------
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Zombie Game", "Proyecto construido.\nEscena: " + ScenePath + "\nPresiona Play.", "OK");
    }

    // ---------- Decoración ----------
    static void Lapida(Vector3 pos, float rotY, Transform parent, Material m)
    {
        var root = new GameObject("Lapida");
        root.transform.SetParent(parent, false);
        root.transform.position = pos;
        root.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
        var bc = root.AddComponent<BoxCollider>();
        bc.center = new Vector3(0f, 0.8f, 0f); bc.size = new Vector3(0.8f, 1.6f, 0.25f);
        Prim(PrimitiveType.Cube, "Base", new Vector3(0f, 0.6f, 0f), new Vector3(0.8f, 1.2f, 0.25f), m, root.transform, false);
        var top = Prim(PrimitiveType.Cylinder, "Remate", new Vector3(0f, 1.2f, 0f), new Vector3(0.8f, 0.125f, 0.8f), m, root.transform, false);
        top.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }

    static void Caja(Vector3 pos, float rotY, Transform parent, Material m)
    {
        var c = Prim(PrimitiveType.Cube, "Caja", pos, Vector3.one, m, parent);
        c.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
    }

    static void Torch(Vector3 basePos, Transform parent, Material wood, Material fire)
    {
        var root = new GameObject("Antorcha");
        root.transform.SetParent(parent, false);
        root.transform.position = basePos;
        Prim(PrimitiveType.Cylinder, "Poste", new Vector3(0f, 0.45f, 0f), new Vector3(0.15f, 0.45f, 0.15f), wood, root.transform, false);
        var flame = Prim(PrimitiveType.Sphere, "Llama", new Vector3(0f, 1.05f, 0f), Vector3.one * 0.3f, fire, root.transform, false);
        var l = flame.AddComponent<Light>();
        l.type = LightType.Point; l.color = new Color(1f, 0.55f, 0.2f); l.range = 9f; l.intensity = 4f;
        flame.AddComponent<TorchFlicker>();
    }

    // ---------- Utilidades ----------
    static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name)) AssetDatabase.CreateFolder(parent, name);
    }

    static Texture2D MakeTex(string name, System.Func<int, int, Color> f)
    {
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, f(x, y));
        tex.Apply();

        string path = MatDir + "/" + name + ".png";
        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    static Material Mat(string name, Color color, float metallic, float smoothness,
                        Color? emission = null, Texture2D tex = null, Vector2? tiling = null)
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        bool urp = sh != null;
        if (!urp) sh = Shader.Find("Standard");

        var m = new Material(sh);
        m.SetColor(urp ? "_BaseColor" : "_Color", color);
        m.SetFloat("_Metallic", metallic);
        m.SetFloat(urp ? "_Smoothness" : "_Glossiness", smoothness);

        if (tex != null)
        {
            string prop = urp ? "_BaseMap" : "_MainTex";
            m.SetTexture(prop, tex);
            m.SetTextureScale(prop, tiling ?? Vector2.one);
        }
        if (emission.HasValue)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emission.Value);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }

        string path = MatDir + "/" + name + ".mat";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(m, path);
        return AssetDatabase.LoadAssetAtPath<Material>(path);
    }

    static GameObject Prim(PrimitiveType type, string name, Vector3 pos, Vector3 scale, Material mat,
                           Transform parent, bool keepCollider = true)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        if (!keepCollider) Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    static GameObject SavePrefab(GameObject go, string name)
    {
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabDir + "/" + name + ".prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }
}
