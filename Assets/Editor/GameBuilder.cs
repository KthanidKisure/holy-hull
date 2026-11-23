using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.UI;

// Sets up the whole project: sprite import settings, tags, prefabs, the knight animator,
// all six scenes and the build list. Run from the menu: Holy Hull > Build Everything.
// Safe to run again; it overwrites what it made before.
public static class GameBuilder
{
    const string Sprites = "Assets/Sprites/";
    const string UIArt = "Assets/UI/";
    const string Prefabs = "Assets/Prefabs/";
    const string Scenes = "Assets/Scenes/";
    const string Anim = "Assets/Animation/";

    static readonly Color Gold = new Color(1f, 0.84f, 0.38f);
    static readonly Color Parchment = new Color(0.94f, 0.9f, 0.8f);
    static readonly Color Blood = new Color(0.86f, 0.25f, 0.25f);

    static Font titleFont;
    static Font pixelFont;

    [MenuItem("Holy Hull/Build Everything")]
    public static void BuildAll()
    {
        Directory.CreateDirectory(Prefabs);
        Directory.CreateDirectory(Scenes);
        Directory.CreateDirectory(Anim);

        AddTag("Asteroid");
        AddTag("Heretic");
        AddTag("Relic");
        UseOldInputManager();

        ImportSprite(Sprites + "HolyShip.png", 32);
        ImportSprite(Sprites + "Heretic.png", 24);
        ImportSprite(Sprites + "Relic.png", 24);
        ImportSprite(Sprites + "Asteroid1.png", 20);
        ImportSprite(Sprites + "Asteroid2.png", 20);
        ImportSprite(Sprites + "Asteroid3.png", 20);
        ImportSprite(Sprites + "Starfield.png", 27);
        ImportSprite(Sprites + "Warp.png", 32);
        ImportSprite(Sprites + "Bolt.png", 16);
        ImportSprite(UIArt + "Panel.png", 25, new Vector4(8, 8, 8, 8));
        foreach (string icon in new[] { "FaithFull", "FaithEmpty", "RelicFull", "RelicEmpty" })
        {
            ImportSprite(UIArt + icon + ".png", 100);
        }
        SliceSheet(Sprites + "Knight.png", 20, 24, 4, 2, 16);
        AssetDatabase.Refresh();

        titleFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/PirataOne-Regular.ttf");
        pixelFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/VT323-Regular.ttf");

        GameObject explosion = MakeExplosion();
        GameObject bullet = MakeBullet(explosion);
        GameObject[] asteroids = { MakeAsteroid(1), MakeAsteroid(2), MakeAsteroid(3) };
        GameObject heretic = MakeHeretic();
        GameObject relic = MakeRelic();
        GameObject gameState = MakeGameState();
        RuntimeAnimatorController knightController = MakeKnightAnimator();

        MakeMenu(gameState, knightController);
        MakeLevel("Level1", "System I - The Drift", 6, 0, 16f, "Level2", gameState, bullet, asteroids, heretic, relic);
        MakeLevel("Level2", "System II - Heretic Space", 7, 2, 13f, "Level3", gameState, bullet, asteroids, heretic, relic);
        MakeLevel("Level3", "System III - The Throne Void", 9, 4, 10f, "Victory", gameState, bullet, asteroids, heretic, relic);
        MakeEndScreen("Victory", gameState, "The Emperor Stirs!",
            "All three relics are back on the Hollow Throne.\nThe Order is saved. For now.\n\nSCORE  {score}",
            Gold, "Press SPACE to return to the menu");
        MakeEndScreen("GameOver", gameState, "Faith Lost",
            "The cathedral-ship drifts off into the dark.\nThe Emperor sleeps on.\n\nSCORE  {score}",
            Blood, "Press SPACE to try again");

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(Scenes + "Menu.unity", true),
            new EditorBuildSettingsScene(Scenes + "Level1.unity", true),
            new EditorBuildSettingsScene(Scenes + "Level2.unity", true),
            new EditorBuildSettingsScene(Scenes + "Level3.unity", true),
            new EditorBuildSettingsScene(Scenes + "Victory.unity", true),
            new EditorBuildSettingsScene(Scenes + "GameOver.unity", true),
        };

        PlayerSettings.productName = "Holy Hull";
        PlayerSettings.companyName = "CM2119";
        PlayerSettings.runInBackground = true;
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(Scenes + "Menu.unity");
        Debug.Log("Holy Hull: build finished");
    }

    // Used by the command line: builds a Windows player into Builds/Windows
    public static void BuildWindows()
    {
        BuildAll();
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, "Builds/Windows/HolyHull.exe",
            BuildTarget.StandaloneWindows64, BuildOptions.None);
    }

    // ---------- project settings ----------

    static void AddTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        }
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }

    // The course scripts use Input.GetKey, which needs the old Input Manager switched on
    static void UseOldInputManager()
    {
        Object[] settings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
        if (settings.Length == 0) return;
        SerializedObject so = new SerializedObject(settings[0]);
        SerializedProperty prop = so.FindProperty("activeInputHandler");
        if (prop != null && prop.intValue == 1)
        {
            prop.intValue = 2;
            so.ApplyModifiedProperties();
        }
    }

    static void ImportSprite(string path, float ppu, Vector4 border = default)
    {
        TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(path);
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = ppu;
        ti.spriteBorder = border;
        ti.filterMode = FilterMode.Point;
        ti.mipmapEnabled = false;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.SaveAndReimport();
    }

    // Cuts a sprite sheet into a grid, the same as Slice in the Sprite Editor
    static void SliceSheet(string path, int cellW, int cellH, int cols, int rows, float ppu)
    {
        TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(path);
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Multiple;
        ti.spritePixelsPerUnit = ppu;
        ti.filterMode = FilterMode.Point;
        ti.mipmapEnabled = false;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.SaveAndReimport();

        SpriteDataProviderFactories factory = new SpriteDataProviderFactories();
        factory.Init();
        ISpriteEditorDataProvider dp = factory.GetSpriteEditorDataProviderFromObject(ti);
        dp.InitSpriteEditorDataProvider();

        string baseName = Path.GetFileNameWithoutExtension(path);
        List<SpriteRect> rects = new List<SpriteRect>();
        int n = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                rects.Add(new SpriteRect
                {
                    name = baseName + "_" + n,
                    rect = new Rect(col * cellW, (rows - 1 - row) * cellH, cellW, cellH),
                    alignment = SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                    spriteID = GUID.Generate()
                });
                n++;
            }
        }
        dp.SetSpriteRects(rects.ToArray());
        dp.Apply();
        ti.SaveAndReimport();
    }

    static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static Sprite[] KnightFrames()
    {
        Sprite[] frames = new Sprite[8];
        foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(Sprites + "Knight.png"))
        {
            Sprite s = o as Sprite;
            if (s != null)
            {
                frames[int.Parse(s.name.Substring("Knight_".Length))] = s;
            }
        }
        return frames;
    }

    static Material ParticleMaterial()
    {
        return AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");
    }

    static GameObject SavePrefab(GameObject go, string name)
    {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, Prefabs + name + ".prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject SpriteObject(string name, string sprite, int order)
    {
        GameObject go = new GameObject(name);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite(Sprites + sprite);
        sr.sortingOrder = order;
        return go;
    }

    static Rigidbody2D AddBody(GameObject go, float drag)
    {
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = drag;
        rb.angularDamping = 0.5f;
        return rb;
    }

    static Gradient Fade(Color from, Color to)
    {
        Gradient g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(from, 0f), new GradientColorKey(to, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        return g;
    }

    // ---------- prefabs ----------

    static GameObject MakeExplosion()
    {
        GameObject go = new GameObject("Explosion");
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.95f, 0.6f), new Color(1f, 0.45f, 0.15f));
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 45) });
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f;
        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = Fade(Color.white, new Color(0.7f, 0.1f, 0.1f));
        ParticleSystemRenderer r = go.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = ParticleMaterial();
        r.sortingOrder = 5;
        go.AddComponent<AutoDestroy>().delay = 2f;
        return SavePrefab(go, "Explosion");
    }

    static GameObject MakeBullet(GameObject explosion)
    {
        GameObject go = SpriteObject("Bolt", "Bolt.png", 3);
        Rigidbody2D rb = AddBody(go, 0f);
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        go.AddComponent<CircleCollider2D>().radius = 0.14f;
        go.AddComponent<MoveBullet>().explosion = explosion;
        return SavePrefab(go, "Bolt");
    }

    static GameObject MakeAsteroid(int variant)
    {
        GameObject go = SpriteObject("Asteroid" + variant, "Asteroid" + variant + ".png", 2);
        go.tag = "Asteroid";
        AddBody(go, 0f).mass = 2f;
        go.AddComponent<CircleCollider2D>().radius = 0.6f;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.playOnAwake = false;
        main.startLifetime = 0.5f;
        main.startSpeed = 2f;
        main.startSize = 0.1f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var emission = ps.emission;
        emission.rateOverTime = 40f;
        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = Fade(new Color(1f, 0.75f, 0.4f), new Color(0.45f, 0.35f, 0.3f));
        ParticleSystemRenderer r = go.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = ParticleMaterial();
        r.sortingOrder = 4;

        go.AddComponent<AsteroidMove>();
        return SavePrefab(go, "Asteroid" + variant);
    }

    static GameObject MakeHeretic()
    {
        GameObject go = SpriteObject("Heretic", "Heretic.png", 2);
        go.tag = "Heretic";
        go.transform.localScale = Vector3.one * 1.2f;
        AddBody(go, 0.5f);
        go.AddComponent<CircleCollider2D>().radius = 0.35f;
        go.AddComponent<HereticShip>();
        return SavePrefab(go, "Heretic");
    }

    static GameObject MakeRelic()
    {
        GameObject go = SpriteObject("Relic", "Relic.png", 2);
        go.tag = "Relic";
        go.transform.localScale = Vector3.one * 1.3f;
        CircleCollider2D c = go.AddComponent<CircleCollider2D>();
        c.isTrigger = true;
        c.radius = 0.35f;
        go.AddComponent<Relic>();
        return SavePrefab(go, "Relic");
    }

    static GameObject MakeGameState()
    {
        GameObject go = new GameObject("GameState");
        go.AddComponent<AudioSource>().playOnAwake = false;
        go.AddComponent<AudioSource>().playOnAwake = false;
        GameState gs = go.AddComponent<GameState>();
        gs.warpSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Warp.wav");
        gs.relicSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Relic.wav");
        gs.hitSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Hit.wav");
        gs.explosionSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Explosion.wav");
        gs.shootSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Shoot.wav");
        gs.music = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Hymn.wav");
        return SavePrefab(go, "GameState");
    }

    static AnimationClip MakeClip(string name, Sprite[] frames, int first)
    {
        AnimationClip clip = new AnimationClip { frameRate = 8 };
        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[5];
        for (int i = 0; i < 5; i++)
        {
            keys[i] = new ObjectReferenceKeyframe { time = i / 8f, value = frames[first + (i % 4)] };
        }
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        AssetDatabase.CreateAsset(clip, Anim + name + ".anim");
        return clip;
    }

    // Idle = frames 0-3, Run = frames 4-7, switched by the Running bool (Animations practical)
    static RuntimeAnimatorController MakeKnightAnimator()
    {
        Sprite[] frames = KnightFrames();
        AnimationClip idle = MakeClip("Idle", frames, 0);
        AnimationClip run = MakeClip("Run", frames, 4);

        AnimatorController ac = AnimatorController.CreateAnimatorControllerAtPath(Anim + "Knight.controller");
        ac.AddParameter("Running", AnimatorControllerParameterType.Bool);
        AnimatorStateMachine sm = ac.layers[0].stateMachine;
        AnimatorState idleState = sm.AddState("Idle");
        idleState.motion = idle;
        AnimatorState runState = sm.AddState("Run");
        runState.motion = run;
        sm.defaultState = idleState;

        AnimatorStateTransition toRun = idleState.AddTransition(runState);
        toRun.hasExitTime = false;
        toRun.duration = 0f;
        toRun.AddCondition(AnimatorConditionMode.If, 0, "Running");
        AnimatorStateTransition toIdle = runState.AddTransition(idleState);
        toIdle.hasExitTime = false;
        toIdle.duration = 0f;
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "Running");
        return ac;
    }

    // ---------- UI helpers ----------

    static void NewScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        camGo.transform.position = new Vector3(0, 0, -10);
        camGo.AddComponent<AudioListener>();
        SpriteObject("Background", "Starfield.png", 0);
    }

    static Canvas MakeCanvas()
    {
        GameObject go = new GameObject("Canvas");
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    static RectTransform Place(GameObject go, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    static Image MakePanel(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        Place(go, parent, anchor, pos, size);
        Image img = go.AddComponent<Image>();
        img.sprite = LoadSprite(UIArt + "Panel.png");
        img.type = Image.Type.Sliced;
        return img;
    }

    static Image MakeIcon(Transform parent, string name, string sprite, Vector2 anchor, Vector2 pos, float size)
    {
        GameObject go = new GameObject(name);
        Place(go, parent, anchor, pos, new Vector2(size, size));
        Image img = go.AddComponent<Image>();
        img.sprite = LoadSprite(UIArt + sprite + ".png");
        return img;
    }

    static Text MakeText(Transform parent, string name, string text, Font font, int size, TextAnchor align,
        Vector2 anchor, Vector2 pos, Vector2 box, Color color)
    {
        GameObject go = new GameObject(name);
        Place(go, parent, anchor, pos, box);
        Text t = go.AddComponent<Text>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.alignment = align;
        t.color = color;
        t.lineSpacing = 0.9f;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        go.AddComponent<Shadow>().effectDistance = new Vector2(4, -4);
        return t;
    }

    // Invisible walls around the camera so nothing leaves the screen (Lecture 2: collision zones)
    static void MakeWalls()
    {
        GameObject walls = new GameObject("Walls");
        string matPath = "Assets/Bouncy.physicsMaterial2D";
        PhysicsMaterial2D bouncy = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(matPath);
        if (bouncy == null)
        {
            bouncy = new PhysicsMaterial2D("Bouncy") { bounciness = 0.8f, friction = 0f };
            AssetDatabase.CreateAsset(bouncy, matPath);
        }
        // the top wall sits just under the HUD bar
        float w = 8.9f, top = 4.25f, bottom = -5f, t = 1f;
        Vector2[] pos = { new Vector2(0, top + t / 2), new Vector2(0, bottom - t / 2), new Vector2(-w - t / 2, 0), new Vector2(w + t / 2, 0) };
        Vector2[] size = { new Vector2(w * 2 + 2 * t, t), new Vector2(w * 2 + 2 * t, t), new Vector2(t, 12f), new Vector2(t, 12f) };
        for (int i = 0; i < 4; i++)
        {
            GameObject wall = new GameObject("Wall" + i);
            wall.transform.SetParent(walls.transform);
            wall.transform.position = pos[i];
            BoxCollider2D bc = wall.AddComponent<BoxCollider2D>();
            bc.size = size[i];
            bc.sharedMaterial = bouncy;
        }
    }

    // ---------- scenes ----------

    static void MakeLevel(string scene, string title, int asteroids, int heretics, float malfunctionEvery, string next,
        GameObject gameState, GameObject bullet, GameObject[] asteroidPrefabs, GameObject heretic, GameObject relic)
    {
        NewScene();
        PrefabUtility.InstantiatePrefab(gameState);
        MakeWalls();

        // the player ship with an engine particle trail
        GameObject ship = SpriteObject("HolyShip", "HolyShip.png", 3);
        ship.tag = "Player";
        ship.transform.position = new Vector3(-6.5f, -3f, 0);
        ship.transform.localScale = Vector3.one * 1.25f;
        // turning is done with transform.Rotate, so stop bumps from spinning the ship
        AddBody(ship, 0.6f).freezeRotation = true;
        ship.AddComponent<PolygonCollider2D>();
        MoveShip ms = ship.AddComponent<MoveShip>();
        ms.bullet = bullet;

        GameObject engine = new GameObject("Engine");
        engine.transform.SetParent(ship.transform, false);
        engine.transform.localPosition = new Vector3(0, -0.45f, 0);
        engine.transform.localRotation = Quaternion.Euler(90, 0, 0);
        ParticleSystem eps = engine.AddComponent<ParticleSystem>();
        eps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = eps.main;
        main.playOnAwake = false;
        main.startLifetime = 0.35f;
        main.startSpeed = 3f;
        main.startSize = 0.12f;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.85f, 0.4f), new Color(1f, 0.45f, 0.1f));
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var em = eps.emission;
        em.rateOverTime = 60f;
        var sh = eps.shape;
        sh.shapeType = ParticleSystemShapeType.Cone;
        sh.angle = 10f;
        sh.radius = 0.05f;
        var col = eps.colorOverLifetime;
        col.enabled = true;
        col.color = Fade(Color.white, new Color(0.8f, 0.2f, 0.1f));
        ParticleSystemRenderer er = engine.GetComponent<ParticleSystemRenderer>();
        er.sharedMaterial = ParticleMaterial();
        er.sortingOrder = 2;

        // the warp gate, hidden until the relic is collected
        GameObject warp = SpriteObject("Warp", "Warp.png", 1);
        warp.transform.position = new Vector3(7f, 2.8f, 0);
        CircleCollider2D wc = warp.AddComponent<CircleCollider2D>();
        wc.isTrigger = true;
        wc.radius = 0.5f;
        warp.AddComponent<BlackHole>().whereToGo = next;

        // HUD: a bar along the top with faith crosses, relic chalices and the score
        Canvas canvas = MakeCanvas();
        Image bar = MakePanel(canvas.transform, "HUD Bar", new Vector2(0.5f, 1), new Vector2(0, 0), new Vector2(1940, 110));
        Transform b = bar.transform;
        MakeText(b, "Faith Label", "Faith", titleFont, 50, TextAnchor.MiddleLeft, new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(200, 90), Gold);
        Image[] faith = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            faith[i] = MakeIcon(b, "Faith" + i, "FaithFull", new Vector2(0, 0.5f), new Vector2(170 + i * 62, 0), 52);
        }
        MakeText(b, "Relic Label", "Relics", titleFont, 50, TextAnchor.MiddleRight, new Vector2(0.5f, 0.5f), new Vector2(-110, 0), new Vector2(200, 90), Gold);
        Image[] relics = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            relics[i] = MakeIcon(b, "Relic" + i, "RelicEmpty", new Vector2(0.5f, 0.5f), new Vector2(30 + i * 62, 0), 52);
        }
        Text score = MakeText(b, "Score", "00000", pixelFont, 72, TextAnchor.MiddleRight, new Vector2(1, 0.5f), new Vector2(-40, 0), new Vector2(400, 90), Parchment);
        MakeText(b, "Score Label", "Score", titleFont, 50, TextAnchor.MiddleRight, new Vector2(1, 0.5f), new Vector2(-250, 0), new Vector2(200, 90), Gold);

        Image alertPanel = MakePanel(canvas.transform, "Alert", new Vector2(0.5f, 1), new Vector2(0, -140), new Vector2(1150, 150));
        Text alert = MakeText(alertPanel.transform, "Alert Text", "", pixelFont, 54, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1100, 140), Gold);

        GameObject lmGo = new GameObject("LevelManager");
        LevelManager lm = lmGo.AddComponent<LevelManager>();
        lm.asteroidPrefabs = asteroidPrefabs;
        lm.hereticPrefab = heretic;
        lm.relicPrefab = relic;
        lm.warp = warp;
        lm.howManyAsteroids = asteroids;
        lm.howManyHeretics = heretics;
        lm.malfunctionEvery = malfunctionEvery;
        lm.levelName = title;
        lm.faithIcons = faith;
        lm.relicIcons = relics;
        lm.faithFull = LoadSprite(UIArt + "FaithFull.png");
        lm.faithEmpty = LoadSprite(UIArt + "FaithEmpty.png");
        lm.relicFull = LoadSprite(UIArt + "RelicFull.png");
        lm.relicEmpty = LoadSprite(UIArt + "RelicEmpty.png");
        lm.scoreText = score;
        lm.alertPanel = alertPanel.gameObject;
        lm.alertText = alert;

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), Scenes + scene + ".unity");
    }

    static void MakeMenu(GameObject gameState, RuntimeAnimatorController knightController)
    {
        NewScene();
        PrefabUtility.InstantiatePrefab(gameState);

        GameObject knight = new GameObject("Brother Astro");
        SpriteRenderer sr = knight.AddComponent<SpriteRenderer>();
        sr.sprite = KnightFrames()[0];
        sr.sortingOrder = 3;
        knight.transform.position = new Vector3(0, -1.55f, 0);
        knight.transform.localScale = Vector3.one * 1.4f;
        knight.AddComponent<Animator>().runtimeAnimatorController = knightController;
        knight.AddComponent<AstroAnimation>();

        GameObject ship = SpriteObject("Title Ship", "HolyShip.png", 2);
        ship.transform.position = new Vector3(0, 1f, 0);
        ship.transform.localScale = Vector3.one * 2f;

        Canvas canvas = MakeCanvas();
        Transform c = canvas.transform;
        MakeText(c, "Title", "Holy Hull", titleFont, 190, TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(1600, 230), Gold);
        MakeText(c, "Subtitle", "Spaceship, but... the crew are useless holy knights", pixelFont, 50, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1), new Vector2(0, -250), new Vector2(1600, 70), Parchment);

        Image panel = MakePanel(c, "Orders", new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(1100, 230));
        MakeText(panel.transform, "Orders Text",
            "Recover three sacred relics and wake the Emperor\nW thrust    A / D turn    SPACE fire",
            pixelFont, 46, TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(1040, 120), Parchment);
        Text start = MakeText(panel.transform, "Press Start", "Press SPACE to launch", titleFont, 62, TextAnchor.LowerCenter,
            new Vector2(0.5f, 0), new Vector2(0, 18), new Vector2(1040, 80), Gold);

        GameObject menu = new GameObject("MenuController");
        MenuController mc = menu.AddComponent<MenuController>();
        mc.nextScene = "Level1";
        mc.blinkText = start;
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), Scenes + "Menu.unity");
    }

    static void MakeEndScreen(string scene, GameObject gameState, string title, string body, Color color, string prompt)
    {
        NewScene();
        PrefabUtility.InstantiatePrefab(gameState);
        Canvas canvas = MakeCanvas();
        Image panel = MakePanel(canvas.transform, "Panel", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1200, 700));
        Transform p = panel.transform;
        MakeText(p, "Title", title, titleFont, 140, TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(1150, 180), color);
        Text bodyText = MakeText(p, "Body", body, pixelFont, 54, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -10), new Vector2(1100, 300), Parchment);
        bodyText.gameObject.AddComponent<ScoreText>();
        Text promptText = MakeText(p, "Prompt", prompt, titleFont, 56, TextAnchor.LowerCenter, new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(1100, 80), Gold);

        GameObject menu = new GameObject("MenuController");
        MenuController mc = menu.AddComponent<MenuController>();
        mc.nextScene = "Menu";
        mc.resetGame = false;
        mc.blinkText = promptText;
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), Scenes + scene + ".unity");
    }
}
