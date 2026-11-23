using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.EventSystems;
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
    static readonly Color Blood = new Color(0.9f, 0.3f, 0.3f);

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
        ImportArt();
        pixelFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/VT323-Regular.ttf");

        GameObject explosion = MakeExplosion();
        GameObject bullet = MakeBullet();
        GameObject small1 = MakeAsteroid("AsteroidSmall1", 0.45f, 5, explosion, new GameObject[0]);
        GameObject small2 = MakeAsteroid("AsteroidSmall2", 0.45f, 5, explosion, new GameObject[0]);
        GameObject[] asteroids =
        {
            MakeAsteroid("Asteroid1", 0.6f, 10, explosion, new[] { small1, small2 }),
            MakeAsteroid("Asteroid2", 0.6f, 10, explosion, new[] { small2, small1 }),
            MakeAsteroid("Asteroid3", 0.6f, 10, explosion, new[] { small1, small1 }),
        };
        GameObject holyWater = MakeHolyWater();
        GameObject heretic = MakeHeretic("Heretic", 1.2f, 1, 25, 2.5f, 2.5f, 0.25f, explosion, holyWater);
        GameObject boss = MakeHeretic("ArchHeretic", 2.6f, 8, 300, 1.6f, 1.4f, 1f, explosion, holyWater);
        GameObject relic = MakeRelic();
        GameObject gameState = MakeGameState();
        RuntimeAnimatorController knightController = MakeKnightAnimator();

        MakeMenu(gameState, knightController);
        MakeLevel("Level1", "System I - The Drift", "Space1.png", 6, 0, null, 16f, "Level2", gameState, bullet, asteroids, heretic, relic);
        MakeLevel("Level2", "System II - Heretic Space", "Space2.png", 7, 2, null, 13f, "Level3", gameState, bullet, asteroids, heretic, relic);
        MakeLevel("Level3", "System III - The Throne Void", "Space3.png", 7, 2, boss, 10f, "Victory", gameState, bullet, asteroids, heretic, relic);
        MakeEndScreen("Victory", gameState, "THE EMPEROR STIRS!",
            "All three relics are back on the Hollow Throne.\nThe Order is saved. For now.\n\nSCORE  {score}\n{best}",
            Gold, "Press SPACE to return to the menu");
        MakeEndScreen("GameOver", gameState, "FAITH LOST",
            "The cathedral-ship drifts off into the dark.\nThe Emperor sleeps on.\n\nSCORE  {score}\n{best}",
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

    // Used by the command line: builds the web version into Builds/WebGL
    public static void BuildWebGL()
    {
        BuildAll();
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, "Builds/WebGL",
            BuildTarget.WebGL, BuildOptions.None);
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

    static void ImportArt()
    {
        ImportSprite(Sprites + "HolyShip.png", 26);
        ImportSprite(Sprites + "Flame0.png", 26);
        ImportSprite(Sprites + "Flame1.png", 26);
        ImportSprite(Sprites + "Heretic.png", 20);
        ImportSprite(Sprites + "Relic.png", 20);
        ImportSprite(Sprites + "HolyWater.png", 20);
        ImportSprite(Sprites + "Bolt.png", 11);
        ImportSprite(Sprites + "Warp.png", 32);
        for (int i = 1; i <= 3; i++)
        {
            ImportSprite(Sprites + "Asteroid" + i + ".png", 20);
            ImportSprite(Sprites + "Space" + i + ".png", 18);
        }
        ImportSprite(Sprites + "AsteroidSmall1.png", 20);
        ImportSprite(Sprites + "AsteroidSmall2.png", 20);
        ImportSprite(Sprites + "StarsNear.png", 18);
        SliceSheet(Sprites + "Knight.png", 20, 24, 4, 2, 16);
        SliceSheet(Sprites + "Explosion.png", 24, 24, 6, 1, 20);

        ImportSprite(UIArt + "Panel.png", 25, new Vector4(8, 8, 8, 8));
        ImportSprite(UIArt + "Button.png", 25, new Vector4(4, 4, 4, 4));
        ImportSprite(UIArt + "ButtonHover.png", 25, new Vector4(4, 4, 4, 4));
        ImportSprite(UIArt + "SliderBack.png", 25, new Vector4(2, 2, 2, 2));
        ImportSprite(UIArt + "SliderFill.png", 25, new Vector4(2, 2, 2, 2));
        foreach (string s in new[] { "SliderKnob", "Logo", "FaithFull", "FaithEmpty", "RelicFull", "RelicEmpty",
                     "CrewNavigator", "CrewGunner", "CrewEngineer" })
        {
            ImportSprite(UIArt + s + ".png", 100);
        }
        AssetDatabase.Refresh();
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

    static Sprite[] SheetFrames(string file, int count)
    {
        Sprite[] frames = new Sprite[count];
        string prefix = Path.GetFileNameWithoutExtension(file) + "_";
        foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(Sprites + file))
        {
            Sprite s = o as Sprite;
            if (s != null)
            {
                frames[int.Parse(s.name.Substring(prefix.Length))] = s;
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

    // ---------- prefabs ----------

    static GameObject MakeExplosion()
    {
        GameObject go = SpriteObject("Explosion", "Explosion.png", 6);
        go.transform.localScale = Vector3.one * 1.4f;
        SpriteAnimator sa = go.AddComponent<SpriteAnimator>();
        sa.frames = SheetFrames("Explosion.png", 6);
        sa.fps = 14f;
        go.GetComponent<SpriteRenderer>().sprite = sa.frames[0];
        return SavePrefab(go, "Explosion");
    }

    static GameObject MakeBullet()
    {
        GameObject go = SpriteObject("Bolt", "Bolt.png", 3);
        Rigidbody2D rb = AddBody(go, 0f);
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        go.AddComponent<CircleCollider2D>().radius = 0.2f;
        go.AddComponent<MoveBullet>();
        return SavePrefab(go, "Bolt");
    }

    static GameObject MakeAsteroid(string name, float radius, int points, GameObject explosion, GameObject[] pieces)
    {
        GameObject go = SpriteObject(name, name + ".png", 2);
        go.tag = "Asteroid";
        AddBody(go, 0f).mass = pieces.Length > 0 ? 2f : 0.8f;
        go.AddComponent<CircleCollider2D>().radius = radius;

        // dust when it bumps into things (Practical 5)
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.playOnAwake = false;
        main.startLifetime = 0.4f;
        main.startSpeed = 2f;
        main.startSize = 0.08f;
        main.startColor = new Color(0.66f, 0.55f, 0.45f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var emission = ps.emission;
        emission.rateOverTime = 30f;
        ParticleSystemRenderer r = go.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = ParticleMaterial();
        r.sortingOrder = 4;

        AsteroidMove am = go.AddComponent<AsteroidMove>();
        am.pieces = pieces;
        am.explosion = explosion;
        am.points = points;
        return SavePrefab(go, name);
    }

    static GameObject MakeHolyWater()
    {
        GameObject go = SpriteObject("HolyWater", "HolyWater.png", 3);
        CircleCollider2D c = go.AddComponent<CircleCollider2D>();
        c.isTrigger = true;
        c.radius = 0.4f;
        go.AddComponent<HolyWater>();
        return SavePrefab(go, "HolyWater");
    }

    static GameObject MakeHeretic(string name, float scale, int health, int points, float force, float speed,
        float drop, GameObject explosion, GameObject holyWater)
    {
        GameObject go = SpriteObject(name, "Heretic.png", 2);
        go.tag = "Heretic";
        go.transform.localScale = Vector3.one * scale;
        AddBody(go, 0.5f).mass = scale;
        go.AddComponent<CircleCollider2D>().radius = 0.35f;
        HereticShip hs = go.AddComponent<HereticShip>();
        hs.health = health;
        hs.points = points;
        hs.chaseForce = force * scale;
        hs.maxSpeed = speed;
        hs.dropChance = drop;
        hs.explosion = explosion;
        hs.holyWater = holyWater;
        return SavePrefab(go, name);
    }

    static GameObject MakeRelic()
    {
        GameObject go = SpriteObject("Relic", "Relic.png", 2);
        go.tag = "Relic";
        go.transform.localScale = Vector3.one * 1.2f;
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
        gs.pickupSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Pickup.wav");
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
        Sprite[] frames = SheetFrames("Knight.png", 8);
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

    static void NewScene(string background)
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
        camGo.AddComponent<CameraShake>();

        SpriteObject("Background", background, -2);
        // two copies of the near star layer so it can wrap round while scrolling
        for (int i = 0; i < 2; i++)
        {
            GameObject stars = SpriteObject("Stars" + i, "StarsNear.png", -1);
            stars.transform.position = new Vector3(i * 17.78f, 0, 0);
            stars.AddComponent<ScrollStars>();
        }
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

    static void MakeEventSystem()
    {
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
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

    static Image MakeImage(Transform parent, string name, Sprite sprite, Vector2 anchor, Vector2 pos, Vector2 size, bool sliced)
    {
        GameObject go = new GameObject(name);
        Place(go, parent, anchor, pos, size);
        Image img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        return img;
    }

    static Image MakePanel(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        return MakeImage(parent, name, LoadSprite(UIArt + "Panel.png"), anchor, pos, size, true);
    }

    static Image MakeIcon(Transform parent, string name, string sprite, Vector2 anchor, Vector2 pos, float size)
    {
        return MakeImage(parent, name, LoadSprite(UIArt + sprite + ".png"), anchor, pos, new Vector2(size, size), false);
    }

    static Text MakeText(Transform parent, string name, string text, int size, TextAnchor align,
        Vector2 anchor, Vector2 pos, Vector2 box, Color color)
    {
        GameObject go = new GameObject(name);
        Place(go, parent, anchor, pos, box);
        Text t = go.AddComponent<Text>();
        t.font = pixelFont;
        t.text = text;
        t.fontSize = size;
        t.alignment = align;
        t.color = color;
        t.lineSpacing = 0.85f;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        go.AddComponent<Shadow>().effectDistance = new Vector2(4, -4);
        return t;
    }

    static Button MakeButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction action)
    {
        Image img = MakeImage(parent, label + " Button", LoadSprite(UIArt + "Button.png"), new Vector2(0.5f, 0.5f), pos, new Vector2(420, 84), true);
        Button b = img.gameObject.AddComponent<Button>();
        b.transition = Selectable.Transition.SpriteSwap;
        SpriteState ss = new SpriteState();
        ss.highlightedSprite = LoadSprite(UIArt + "ButtonHover.png");
        ss.pressedSprite = LoadSprite(UIArt + "ButtonHover.png");
        ss.selectedSprite = LoadSprite(UIArt + "ButtonHover.png");
        b.spriteState = ss;
        UnityEventTools.AddPersistentListener(b.onClick, action);
        MakeText(img.transform, "Label", label, 52, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, 2), new Vector2(400, 80), Gold);
        return b;
    }

    static Slider MakeSlider(Transform parent, string label, Vector2 pos)
    {
        MakeText(parent, label + " Label", label, 46, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f), pos + new Vector2(-130, 0), new Vector2(240, 60), Parchment);
        GameObject go = new GameObject(label + " Slider");
        Place(go, parent, new Vector2(0.5f, 0.5f), pos + new Vector2(110, 0), new Vector2(300, 30));
        Slider s = go.AddComponent<Slider>();
        Image back = MakeImage(go.transform, "Background", LoadSprite(UIArt + "SliderBack.png"), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300, 30), true);

        GameObject fillArea = new GameObject("Fill Area");
        RectTransform fa = Place(fillArea, go.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300, 30));
        Image fill = MakeImage(fillArea.transform, "Fill", LoadSprite(UIArt + "SliderFill.png"), new Vector2(0, 0.5f), Vector2.zero, new Vector2(0, 30), true);
        fill.rectTransform.anchorMin = new Vector2(0, 0);
        fill.rectTransform.anchorMax = new Vector2(0, 1);
        fill.rectTransform.sizeDelta = Vector2.zero;

        GameObject handleArea = new GameObject("Handle Slide Area");
        Place(handleArea, go.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(280, 30));
        Image knob = MakeImage(handleArea.transform, "Handle", LoadSprite(UIArt + "SliderKnob.png"), new Vector2(0, 0.5f), Vector2.zero, new Vector2(28, 48), false);
        knob.rectTransform.anchorMin = new Vector2(0, 0);
        knob.rectTransform.anchorMax = new Vector2(0, 1);
        knob.rectTransform.sizeDelta = new Vector2(28, 18);

        s.fillRect = fill.rectTransform;
        s.handleRect = knob.rectTransform;
        s.targetGraphic = knob;
        s.direction = Slider.Direction.LeftToRight;
        s.minValue = 0f;
        s.maxValue = 1f;
        return s;
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

    static void MakeLevel(string scene, string title, string background, int asteroids, int heretics, GameObject boss,
        float malfunctionEvery, string next, GameObject gameState, GameObject bullet, GameObject[] asteroidPrefabs,
        GameObject heretic, GameObject relic)
    {
        NewScene(background);
        PrefabUtility.InstantiatePrefab(gameState);
        MakeWalls();
        MakeEventSystem();

        // the player ship with a flickering pixel flame behind it
        GameObject ship = SpriteObject("HolyShip", "HolyShip.png", 3);
        ship.tag = "Player";
        ship.transform.position = new Vector3(-6.5f, -3f, 0);
        // turning is done with transform.Rotate, so stop bumps from spinning the ship
        AddBody(ship, 0.6f).freezeRotation = true;
        ship.AddComponent<PolygonCollider2D>();
        MoveShip ms = ship.AddComponent<MoveShip>();
        ms.bullet = bullet;
        GameObject flame = SpriteObject("Flame", "Flame0.png", 2);
        flame.transform.SetParent(ship.transform, false);
        flame.transform.localPosition = new Vector3(0, -0.66f, 0);
        flame.transform.localRotation = Quaternion.Euler(0, 0, 180);
        ms.flame = flame.GetComponent<SpriteRenderer>();
        ms.flameFrames = new[] { LoadSprite(Sprites + "Flame0.png"), LoadSprite(Sprites + "Flame1.png") };

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
        MakeText(b, "Faith Label", "FAITH", 58, TextAnchor.MiddleLeft, new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(200, 90), Gold);
        Image[] faith = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            faith[i] = MakeIcon(b, "Faith" + i, "FaithFull", new Vector2(0, 0.5f), new Vector2(190 + i * 62, 0), 52);
        }
        MakeText(b, "Relic Label", "RELICS", 58, TextAnchor.MiddleRight, new Vector2(0.5f, 0.5f), new Vector2(-150, 0), new Vector2(220, 90), Gold);
        Image[] relics = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            relics[i] = MakeIcon(b, "Relic" + i, "RelicEmpty", new Vector2(0.5f, 0.5f), new Vector2(30 + i * 62, 0), 52);
        }
        Text score = MakeText(b, "Score", "00000", 72, TextAnchor.MiddleRight, new Vector2(1, 0.5f), new Vector2(-40, 0), new Vector2(400, 90), Parchment);
        MakeText(b, "Score Label", "SCORE", 58, TextAnchor.MiddleRight, new Vector2(1, 0.5f), new Vector2(-250, 0), new Vector2(200, 90), Gold);
        MakeText(canvas.transform, "Pause Hint", "ESC / P  pause", 34, TextAnchor.LowerRight, new Vector2(1, 0), new Vector2(-24, 12), new Vector2(400, 50), new Color(1, 1, 1, 0.45f));

        // crew message box with a portrait of whoever caused the problem
        Image alertPanel = MakePanel(canvas.transform, "Alert", new Vector2(0.5f, 1), new Vector2(0, -140), new Vector2(1200, 170));
        Image portrait = MakeImage(alertPanel.transform, "Portrait", null, new Vector2(0, 0.5f), new Vector2(24, 0), new Vector2(132, 121), false);
        Text alert = MakeText(alertPanel.transform, "Alert Text", "", 54, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(60, 0), new Vector2(1000, 160), Gold);

        // pause menu
        GameObject pauseRoot = new GameObject("Pause");
        RectTransform pr = Place(pauseRoot, canvas.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(3000, 2000));
        pauseRoot.AddComponent<Image>().color = new Color(0.03f, 0.01f, 0.06f, 0.7f);
        Image pausePanel = MakePanel(pauseRoot.transform, "Pause Panel", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 720));
        Transform pp = pausePanel.transform;
        MakeText(pp, "Paused Title", "PAUSED", 110, TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(700, 130), Gold);
        Slider music = MakeSlider(pp, "MUSIC", new Vector2(0, 170));
        Slider sfx = MakeSlider(pp, "SOUND", new Vector2(0, 90));
        GameObject pmGo = new GameObject("PauseMenu");
        PauseMenu pm = pmGo.AddComponent<PauseMenu>();
        MakeButton(pp, "RESUME", new Vector2(0, -20), pm.Resume);
        MakeButton(pp, "MAIN MENU", new Vector2(0, -120), pm.MainMenu);
        MakeButton(pp, "QUIT", new Vector2(0, -220), pm.Quit);
        MakeText(pp, "Controls", "W thrust   A/D turn   SPACE or CLICK fire", 34, TextAnchor.LowerCenter, new Vector2(0.5f, 0), new Vector2(0, 22), new Vector2(720, 50), Parchment);
        pm.panel = pauseRoot;
        pm.musicSlider = music;
        pm.sfxSlider = sfx;

        GameObject lmGo = new GameObject("LevelManager");
        LevelManager lm = lmGo.AddComponent<LevelManager>();
        lm.asteroidPrefabs = asteroidPrefabs;
        lm.hereticPrefab = heretic;
        lm.bossPrefab = boss;
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
        lm.alertPortrait = portrait;
        lm.navigatorFace = LoadSprite(UIArt + "CrewNavigator.png");
        lm.gunnerFace = LoadSprite(UIArt + "CrewGunner.png");
        lm.engineerFace = LoadSprite(UIArt + "CrewEngineer.png");

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), Scenes + scene + ".unity");
    }

    static void MakeMenu(GameObject gameState, RuntimeAnimatorController knightController)
    {
        NewScene("Space1.png");
        PrefabUtility.InstantiatePrefab(gameState);

        GameObject knight = new GameObject("Brother Astro");
        SpriteRenderer sr = knight.AddComponent<SpriteRenderer>();
        sr.sprite = SheetFrames("Knight.png", 8)[0];
        sr.sortingOrder = 3;
        knight.transform.position = new Vector3(0, -1.55f, 0);
        knight.transform.localScale = Vector3.one * 1.4f;
        knight.AddComponent<Animator>().runtimeAnimatorController = knightController;
        knight.AddComponent<AstroAnimation>();

        GameObject ship = SpriteObject("Title Ship", "HolyShip.png", 2);
        ship.transform.position = new Vector3(0, 0.9f, 0);
        ship.transform.localScale = Vector3.one * 1.6f;

        Canvas canvas = MakeCanvas();
        Transform c = canvas.transform;
        MakeImage(c, "Logo", LoadSprite(UIArt + "Logo.png"), new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(110 * 9, 18 * 9), false);
        MakeText(c, "Subtitle", "Spaceship, but... the crew are useless holy knights", 50, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1), new Vector2(0, -230), new Vector2(1600, 70), Parchment);
        Text best = MakeText(c, "High Score", "HIGH SCORE 0", 40, TextAnchor.UpperRight, new Vector2(1, 1), new Vector2(-30, -20), new Vector2(500, 60), Gold);

        Image panel = MakePanel(c, "Orders", new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(1160, 240));
        MakeText(panel.transform, "Orders Text",
            "Recover three sacred relics and wake the Emperor\nW thrust   A/D turn   SPACE or CLICK fire   ESC pause",
            44, TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(1100, 120), Parchment);
        Text start = MakeText(panel.transform, "Press Start", "PRESS SPACE TO LAUNCH", 64, TextAnchor.LowerCenter,
            new Vector2(0.5f, 0), new Vector2(0, 20), new Vector2(1040, 80), Gold);

        GameObject menu = new GameObject("MenuController");
        MenuController mc = menu.AddComponent<MenuController>();
        mc.nextScene = "Level1";
        mc.blinkText = start;
        mc.bestText = best;
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), Scenes + "Menu.unity");
    }

    static void MakeEndScreen(string scene, GameObject gameState, string title, string body, Color color, string prompt)
    {
        NewScene(scene == "Victory" ? "Space3.png" : "Space2.png");
        PrefabUtility.InstantiatePrefab(gameState);
        Canvas canvas = MakeCanvas();
        Image panel = MakePanel(canvas.transform, "Panel", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1200, 720));
        Transform p = panel.transform;
        MakeText(p, "Title", title, 130, TextAnchor.UpperCenter, new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(1150, 160), color);
        Text bodyText = MakeText(p, "Body", body, 54, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0, -10), new Vector2(1100, 340), Parchment);
        bodyText.gameObject.AddComponent<ScoreText>();
        Text promptText = MakeText(p, "Prompt", prompt, 56, TextAnchor.LowerCenter, new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(1100, 80), Gold);

        GameObject menu = new GameObject("MenuController");
        MenuController mc = menu.AddComponent<MenuController>();
        mc.nextScene = "Menu";
        mc.resetGame = false;
        mc.blinkText = promptText;
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), Scenes + scene + ".unity");
    }
}
