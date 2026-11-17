using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

// Construye todo el HUD y el menú de fin de partida por código.
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    Font font;
    Text scoreText, healthText, killsText, messageText, bannerText;
    Text waveTitle, waveSub, bossName, titleText, statsText, countdownText;
    RectTransform scoreFill, healthFill, bossFill;
    Image flash;
    GameObject endPanel, bossBar;
    System.Action onRestart;

    float messageTimer, bannerTimer, flashAlpha, autoRestart = -1f;
    bool restartCalled;

    static readonly Vector2 TL = new Vector2(0f, 1f);
    static readonly Vector2 TR = new Vector2(1f, 1f);
    static readonly Vector2 TC = new Vector2(0.5f, 1f);
    static readonly Vector2 BC = new Vector2(0.5f, 0f);
    static readonly Vector2 C = new Vector2(0.5f, 0.5f);

    void Awake()
    {
        Instance = this;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildUI();
    }

    public void SetRestartAction(System.Action a) { onRestart = a; }

    // ---------- API pública ----------
    public void UpdateHUD(int score, int target, int health, int maxHealth, int kills)
    {
        scoreText.text = $"PUNTOS   {score} / {target}";
        scoreFill.anchorMax = new Vector2(Mathf.Clamp01((float)score / Mathf.Max(target, 1)), 1f);
        healthText.text = $"VIDA   {Mathf.Max(health, 0)} / {maxHealth}";
        healthFill.anchorMax = new Vector2(Mathf.Clamp01((float)health / Mathf.Max(maxHealth, 1)), 1f);
        killsText.text = $"ZOMBIES ELIMINADOS: {kills}";
    }

    public void ShowMessage(string text, Color color)
    {
        messageText.text = text;
        messageText.color = color;
        messageTimer = 1.6f;
    }

    public void ShowBanner(string text, Color color, float seconds = 2f)
    {
        bannerText.text = text;
        bannerText.color = color;
        bannerTimer = seconds;
    }

    public void SetWaveInfo(string title, string sub)
    {
        waveTitle.text = title;
        waveSub.text = sub;
    }

    public void ShowBossBar(string bossTitle, float fraction)
    {
        bossBar.SetActive(true);
        bossName.text = bossTitle;
        bossFill.anchorMax = new Vector2(Mathf.Clamp01(fraction), 1f);
    }

    public void SetBossHealth(float fraction)
    {
        bossFill.anchorMax = new Vector2(Mathf.Clamp01(fraction), 1f);
    }

    public void HideBossBar() { bossBar.SetActive(false); }

    public void FlashDamage() { flashAlpha = 1f; }

    public void ShowEnd(bool win, int score, int kills, float autoRestartSeconds, string title = null)
    {
        HideBossBar();
        endPanel.SetActive(true);
        titleText.fontSize = title == null ? 110 : 80;
        titleText.text = title ?? (win ? "¡GANASTE!" : "GAME OVER");
        titleText.color = win ? new Color(0.35f, 1f, 0.45f) : new Color(1f, 0.25f, 0.25f);
        statsText.text = $"Puntaje: {score}      Zombies eliminados: {kills}";
        countdownText.text = "";
        autoRestart = win ? autoRestartSeconds : -1f;
    }

    // ---------- Update ----------
    void Update()
    {
        float dt = Time.unscaledDeltaTime;

        if (messageTimer > 0f)
        {
            messageTimer -= dt;
            var c = messageText.color;
            c.a = Mathf.Clamp01(messageTimer / 0.5f);
            messageText.color = c;
        }

        if (bannerTimer > 0f)
        {
            bannerTimer -= dt;
            var c = bannerText.color;
            c.a = Mathf.Clamp01(bannerTimer / 0.6f);
            bannerText.color = c;
        }

        if (flashAlpha > 0f)
        {
            flashAlpha = Mathf.MoveTowards(flashAlpha, 0f, dt * 1.5f);
            flash.color = new Color(1f, 0f, 0f, flashAlpha * 0.4f);
        }

        if (endPanel.activeSelf && !restartCalled)
        {
            var k = Keyboard.current;
            if (k != null && (k.enterKey.wasPressedThisFrame || k.rKey.wasPressedThisFrame))
            {
                CallRestart();
                return;
            }
            if (autoRestart > 0f)
            {
                autoRestart -= dt;
                countdownText.text = $"Reiniciando automáticamente en {Mathf.CeilToInt(Mathf.Max(autoRestart, 0f))}...";
                if (autoRestart <= 0f) CallRestart();
            }
        }
    }

    void CallRestart()
    {
        if (restartCalled) return;
        restartCalled = true;
        if (onRestart != null) onRestart();
    }

    // ---------- Construcción de la interfaz ----------
    void BuildUI()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            var module = es.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }

        var cgo = new GameObject("Canvas");
        cgo.transform.SetParent(transform, false);
        var canvas = cgo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = cgo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        cgo.AddComponent<GraphicRaycaster>();
        Transform root = cgo.transform;

        // --- HUD (arriba a la izquierda) ---
        var hud = Panel("HUD", root, new Color(0f, 0f, 0f, 0.5f), TL, TL, new Vector2(20, -185), new Vector2(440, -20));
        scoreText = Label("Puntos", hud, "", 28, new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleLeft, TL, TL, new Vector2(15, -48), new Vector2(405, -10));
        var scoreBg = Panel("PuntosBarra", hud, new Color(0.15f, 0.15f, 0.15f, 0.9f), TL, TL, new Vector2(15, -70), new Vector2(405, -54));
        scoreFill = Panel("Relleno", scoreBg, new Color(1f, 0.8f, 0.2f), Vector2.zero, new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
        healthText = Label("Vida", hud, "", 28, new Color(1f, 0.45f, 0.45f), TextAnchor.MiddleLeft, TL, TL, new Vector2(15, -105), new Vector2(405, -75));
        var healthBg = Panel("VidaBarra", hud, new Color(0.15f, 0.15f, 0.15f, 0.9f), TL, TL, new Vector2(15, -125), new Vector2(405, -109));
        healthFill = Panel("Relleno", healthBg, new Color(0.9f, 0.15f, 0.15f), Vector2.zero, new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        killsText = Label("Bajas", hud, "", 24, Color.white, TextAnchor.MiddleLeft, TL, TL, new Vector2(15, -160), new Vector2(405, -130));

        // --- Info de oleada (arriba a la derecha) ---
        var wave = Panel("Oleada", root, new Color(0f, 0f, 0f, 0.5f), TR, TR, new Vector2(-360, -100), new Vector2(-20, -20));
        waveTitle = Label("OleadaTitulo", wave, "", 34, new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleLeft, TL, TL, new Vector2(15, -45), new Vector2(325, -8));
        waveSub = Label("OleadaInfo", wave, "", 24, Color.white, TextAnchor.MiddleLeft, TL, TL, new Vector2(15, -75), new Vector2(325, -45));

        // --- Mensaje pequeño (arriba al centro) ---
        messageText = Label("Mensaje", root, "", 52, Color.white, TextAnchor.MiddleCenter, TC, TC, new Vector2(-500, -150), new Vector2(500, -80));
        messageText.color = new Color(1, 1, 1, 0);

        // --- Banner grande (centro de la pantalla) ---
        bannerText = Label("Banner", root, "", 100, Color.white, TextAnchor.MiddleCenter, C, C, new Vector2(-900, 100), new Vector2(900, 260));
        bannerText.color = new Color(1, 1, 1, 0);

        // --- Barra de vida del jefe (abajo al centro) ---
        var bossRt = Panel("JefeBarra", root, new Color(0, 0, 0, 0), BC, BC, new Vector2(-420, 30), new Vector2(420, 120));
        bossBar = bossRt.gameObject;
        bossName = Label("JefeNombre", bossRt, "JEFE ZOMBIE", 34, new Color(0.85f, 0.4f, 1f), TextAnchor.MiddleCenter, new Vector2(0, 0.5f), Vector2.one, Vector2.zero, Vector2.zero);
        var bossBg = Panel("JefeFondo", bossRt, new Color(0.1f, 0.05f, 0.12f, 0.9f), Vector2.zero, new Vector2(1f, 0.4f), Vector2.zero, Vector2.zero);
        bossFill = Panel("Relleno", bossBg, new Color(0.65f, 0.15f, 0.85f), Vector2.zero, new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        bossBar.SetActive(false);

        // --- Destello rojo al recibir daño ---
        flash = Panel("Destello", root, new Color(1f, 0f, 0f, 0f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).GetComponent<Image>();

        // --- Pantalla final ---
        var overlay = Panel("PantallaFinal", root, new Color(0f, 0f, 0f, 0.75f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, true);
        endPanel = overlay.gameObject;
        var card = Panel("Tarjeta", overlay, new Color(0.08f, 0.08f, 0.13f, 0.97f), C, C, new Vector2(-450, -280), new Vector2(450, 280), true);
        titleText = Label("Titulo", card, "", 110, Color.white, TextAnchor.MiddleCenter, C, C, new Vector2(-430, 90), new Vector2(430, 230));
        statsText = Label("Stats", card, "", 36, Color.white, TextAnchor.MiddleCenter, C, C, new Vector2(-430, 0), new Vector2(430, 80));
        countdownText = Label("Cuenta", card, "", 30, new Color(0.8f, 0.8f, 0.8f), TextAnchor.MiddleCenter, C, C, new Vector2(-430, -70), new Vector2(430, -20));

        var btnRt = Panel("BotonReiniciar", card, new Color(0.2f, 0.7f, 0.3f), C, C, new Vector2(-210, -185), new Vector2(210, -95), true);
        var btn = btnRt.gameObject.AddComponent<Button>();
        btn.targetGraphic = btnRt.GetComponent<Image>();
        btn.onClick.AddListener(CallRestart);
        Label("Texto", btnRt, "VOLVER A JUGAR", 40, Color.white, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        Label("Ayuda", card, "o presiona ENTER / R", 24, new Color(0.65f, 0.65f, 0.65f), TextAnchor.MiddleCenter, C, C, new Vector2(-300, -250), new Vector2(300, -205));

        endPanel.SetActive(false);
    }

    RectTransform Panel(string name, Transform parent, Color color, Vector2 aMin, Vector2 aMax,
                        Vector2 oMin, Vector2 oMax, bool raycast = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = oMin; rt.offsetMax = oMax;
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = raycast;
        return rt;
    }

    Text Label(string name, Transform parent, string text, int size, Color color, TextAnchor align,
               Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Outline));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = oMin; rt.offsetMax = oMax;

        var t = go.GetComponent<Text>();
        t.font = font;
        t.fontSize = size;
        t.fontStyle = FontStyle.Bold;
        t.color = color;
        t.alignment = align;
        t.text = text;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;

        var o = go.GetComponent<Outline>();
        o.effectColor = new Color(0f, 0f, 0f, 0.8f);
        o.effectDistance = new Vector2(2f, -2f);
        return t;
    }
}
