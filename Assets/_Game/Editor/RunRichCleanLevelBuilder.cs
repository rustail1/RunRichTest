#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using ButchersGames;
using RunRich.Data.Configs;
using RunRich.Data.Definitions;
using RunRich.Runtime.Bootstrap;
using RunRich.Runtime.Finish;
using RunRich.Runtime.Flow;
using RunRich.Runtime.Gates;
using RunRich.Runtime.Level;
using RunRich.Runtime.Pickups;
using RunRich.Runtime.Runner;
using RunRich.Runtime.UI;
using RunRich.Runtime.Wealth;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RunRich.EditorTools
{
    public sealed class RunRichCleanLevelBuilder : EditorWindow
    {
        private const string DefaultScenePath = "Assets/_Game/Scenes/Gameplay_Clean.unity";
        private const string GeneratedRoot = "Assets/_Game/Generated/CleanBuilder";
        private const string GeneratedPrefabs = GeneratedRoot + "/Prefabs";
        private const string GeneratedFonts = GeneratedRoot + "/Fonts";
        private const string GeneratedAnimations = GeneratedRoot + "/Animations";
        private const string GeneratedMaterials = GeneratedRoot + "/Materials";
        private const string GeneratedFontPath = GeneratedFonts + "/RunRichInterDynamic.asset";
        private const string RunnerIdleAnimationPath = "Assets/_Game/Animations/RunnerIdle.fbx";
        private const string RunnerWalkAnimationPath = "Assets/_Game/Animations/RunnerWalk.fbx";
        private const string RunnerAnimatorControllerPath = GeneratedAnimations + "/Runner_IdleWalk.controller";

        private const string InterFontPath = "Assets/ThirdParty/ReferenceAssets/Visual/Fonts/Inter-SemiBold.ttf";
        private const string PlayerModelPath = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/LowPoly/player.fbx";
        private const string MoneyPrefabPath = "Assets/_Game/Prefabs/Pickups/Money.prefab";
        private const string BottlePrefabPath = "Assets/_Game/Prefabs/Pickups/Bottle.prefab";

        private const string GroundMeshPath = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/ground.asset";
        private const string WaterMeshPath = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/Water.asset";
        private const string SolidGroundTexturePath = "Assets/ThirdParty/ReferenceAssets/Visual/Texture2D/white_pixel.png";
        private const string WaterTexturePath = "Assets/ThirdParty/ReferenceAssets/Visual/Texture2D/Water.png";
        private const string SkyboxMaterialPath = "Assets/ThirdParty/ReferenceAssets/Visual/Material/skybox.mat";
        private const string SkyboxTexturePath = "Assets/ThirdParty/ReferenceAssets/Visual/Texture2D/sky_blue.png";

        private const string GroundMaterialPath = "Assets/ThirdParty/ReferenceAssets/Visual/Material/Ground.mat";
        private const string WaterMaterialPath = "Assets/ThirdParty/ReferenceAssets/Visual/Material/Water.mat";
        private const string DoorMaterialPath = "Assets/ThirdParty/ReferenceAssets/Visual/Material/Door.mat";
        private const string GoodDoorMaterialPath = "Assets/ThirdParty/ReferenceAssets/Visual/Material/GoodDoor.mat";
        private const string BadDoorMaterialPath = "Assets/ThirdParty/ReferenceAssets/Visual/Material/BadDoor.mat";
        private const string BadItemMaterialPath = "Assets/ThirdParty/ReferenceAssets/Visual/Material/BadItem.mat";
        private const string PlayerMaterialPath = "Assets/ThirdParty/ReferenceAssets/Visual/Material/player_mat.mat";
        private const string PropsFlatMaterialPath = "Assets/ThirdParty/ReferenceAssets/Visual/Material/props_flat.mat";
        private const string PropsFlatBadMaterialPath = "Assets/ThirdParty/ReferenceAssets/Visual/Material/props_flat_bad.mat";
        private const string FinishLineMaterialPath = "Assets/ThirdParty/ReferenceAssets/Visual/Material/Plane_Finish.mat";
        private const string FinishLineMeshPath = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/Plane.asset";


        private const string DoorPoor0Path = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/Door_Poor_000.asset";
        private const string DoorPoor1Path = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/Door_Poor_001.asset";
        private const string DoorPoor2Path = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/Door_Poor_002.asset";
        private const string DoorDecent0Path = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/Door_Descent_00.asset";
        private const string DoorDecent1Path = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/Door_Descent_01.asset";
        private const string DoorDecent2Path = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/Door_Descent_02.asset";
        private const string DoorRich0Path = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/Door_Rich_00.asset";
        private const string DoorRich1Path = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/Door_Rich_01.asset";
        private const string DoorRich2Path = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/Door_Rich_02.asset";
        private const string DoorMillionPath = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/Door_Million_00.asset";
        private const string DoorMillionPrefabPath = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/LowPoly/Door_Million.fbx";
        private const string PartyMeshPath = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/Party.asset";
        private const string SchoolMeshPath = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/Hat_School.asset";
        private const string FlagMeshPath = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/Flag.asset";
        private const string ChoiceDoorMeshPath = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/ChoiceDoor.asset";
        private const string CheckpointMaterialPath = "Assets/ThirdParty/ReferenceAssets/Visual/Material/Checkpoints.mat";
        private const string EndBluePath = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/EndLevel_Blue.asset";
        private const string EndGreenPath = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/EndLevel_Green.asset";
        private const string EndOrangePath = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/EndLevel_Orange.asset";
        private const string EndYellowPath = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/EndLevel_Yellow.asset";
        private const string TrashMeshAPath = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/Trash.000.asset";
        private const string TrashMeshBPath = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/Trash.001.asset";
        private const string BenchMeshPath = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/Bench.asset";
        private const string PlantMeshPath = "Assets/ThirdParty/ReferenceAssets/Visual/Mesh/Plant_01.asset";
        private const string PropsMaterialPath = "Assets/ThirdParty/ReferenceAssets/Visual/Material/Props.mat";
        private const string SettingsSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/settings.asset";
        private const string ExitSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/Exit.asset";
        private const string BaseThumbnailSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/Base_Thumbnail.asset";
        private const string LevelReferenceThumbPath = "Assets/_Game/Generated/ReferenceMatch/Level01_ReferenceThumb.png";
        private const string CircleSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/Circle.asset";
        private const string GestureArrowSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/arrow_left_right.asset";
        private const string GestureFingerSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/hand.asset";
        private const string DollarSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/dollar_Logo.asset";
        private const string KeySpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/key.asset";
        private const string BlackPanelSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/9grid_black.asset";
        private const string BlackTransparentPanelSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/9grid_black_transparent.asset";
        private const string BluePanelSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/9grid_blue.asset";
        private const string OrangePanelSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/9grid_orange.asset";
        private const string YellowPanelSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/9grid_yellow.asset";
        private const string GreenPanelSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/9grid_green.asset";
        private const string WinPointerSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/arrow_selected.asset";
        private const string GaugeEmptySpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/gauge_empty.asset";
        private const string GaugeFullSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/gauge_full.asset";
        private const string CircleGaugeSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/circle_gauge.asset";
        private const string RestartSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/restart.asset";
        private const string WatchSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/watch.asset";
        private const string WinRaysSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/Ray_0.asset";
        private const string WinMoneySpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/50_coins.asset";
        private const string PositiveAuraSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/aura_yellow.asset";
        private const string PickupStarSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/star_gradiant.asset";
        private const string PickupGlowMaterialPath = "Assets/ThirdParty/ReferenceAssets/Visual/Material/sparkle.mat";
        private const string NegativeStainSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Texture2D/Stain.png";
        private const string WineDropSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Texture2D/winedrop.png";
        private const string NegativeStarsSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Texture2D/stars.png";
        private const string SparkleSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Texture2D/sparkle_0.png";
        private const string MoneyBurstSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/Dollar_Green.asset";
        private const string PlusSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/Plus.asset";
        private const string NegativePickupSpritePath = "Assets/ThirdParty/ReferenceAssets/Visual/Sprite/Negative.asset";

        // Audio below was matched against the supplied reference video by waveform correlation.
        private const string PositiveAudioPath = "Assets/ThirdParty/ReferenceAssets/Sounds/AudioClip/collect_coin.ogg";
        private const string NegativeAudioPath = "Assets/ThirdParty/ReferenceAssets/Sounds/AudioClip/Aou.ogg";
        private const string ChoicePositiveAudioPath = "Assets/ThirdParty/ReferenceAssets/Sounds/AudioClip/shortcutrun_sfx_joueur_boost_pont_V3.ogg";
        private const string CheckpointAudioPath = "Assets/ThirdParty/ReferenceAssets/Sounds/AudioClip/shortcutrun_sfx_envir_boost_champignon_01.ogg";
        private const string WinAudioPath = "Assets/ThirdParty/ReferenceAssets/Sounds/AudioClip/shortcutrun_sfx_jingle_victory.ogg";
        private const string MultiplierLowAudioPath = "Assets/ThirdParty/ReferenceAssets/Sounds/AudioClip/shortcutrun_sfx_joueur_bonus_multiplier_x01_to_x05_variation01.ogg";
        private const string MultiplierMidAudioPath = "Assets/ThirdParty/ReferenceAssets/Sounds/AudioClip/shortcutrun_sfx_joueur_bonus_multiplier_x05_to_x10_variation01.ogg";
        private const string MultiplierHighAudioPath = "Assets/ThirdParty/ReferenceAssets/Sounds/AudioClip/shortcutrun_sfx_joueur_bonus_multiplier_x10_to_x14_variation01.ogg";
        private const string ClickAudioPath = "Assets/ThirdParty/ReferenceAssets/Sounds/AudioClip/click.ogg";
        private static readonly string[] FootstepAudioPaths =
        {
            "Assets/ThirdParty/ReferenceAssets/Sounds/AudioClip/SFX_Footstep_4.ogg",
            "Assets/ThirdParty/ReferenceAssets/Sounds/AudioClip/SFX_Footstep_2.ogg",
            "Assets/ThirdParty/ReferenceAssets/Sounds/AudioClip/SFX_Footstep_3.ogg",
            "Assets/ThirdParty/ReferenceAssets/Sounds/AudioClip/SFX_Footstep_5.ogg",
            "Assets/ThirdParty/ReferenceAssets/Sounds/AudioClip/SFX_Footstep_1.ogg"
        };
        private static readonly string[] HeelAudioPaths =
        {
            "Assets/ThirdParty/ReferenceAssets/Sounds/AudioClip/HighHeels_1.ogg",
            "Assets/ThirdParty/ReferenceAssets/Sounds/AudioClip/HighHeels_2.ogg"
        };

        private string _scenePath = DefaultScenePath;
        private float _roadWidth = 5f;
        private float _roadThickness = 0.18f;
        private bool _createLevel02 = true;
        private Vector2 _scroll;

        [MenuItem("Tools/Run Rich/FINALIZE COMPLETE SCENE")]
        public static void FinalizeCompleteScene()
        {
            RebuildDefaultScene();
            EditorSceneManager.OpenScene(DefaultScenePath, OpenSceneMode.Single);
            AssetDatabase.SaveAssets();

            var validator = CreateInstance<RunRichCleanLevelBuilder>();
            try
            {
                validator._scenePath = DefaultScenePath;
                if (!validator.ValidateCleanScene())
                    throw new InvalidOperationException("RUN RICH FINAL: validation failed. Scene-ready marker must not be written.");
            }
            finally
            {
                DestroyImmediate(validator);
            }

            Debug.Log("RUN RICH FINAL: Gameplay_Clean rebuilt and validated with reference visuals, UI, Idle/Walk, VFX and matched SFX.");
        }

        [MenuItem("Tools/Run Rich/Clean Level Builder")]
        public static void Open()
        {
            var window = GetWindow<RunRichCleanLevelBuilder>("Run Rich Clean Builder");
            window.minSize = new Vector2(470f, 520f);
            window.Show();
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.LabelField("Run Rich — Clean Level Builder", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This builder creates a NEW gameplay scene from an empty Unity scene. It does not repair or reuse the old Gameplay scene hierarchy. Road, player, gates, finish, UI and level roots are physical scene objects before Play Mode. Runtime may spawn only Money/Bottle items from PickupStrip rows.",
                MessageType.Info);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);
            _scenePath = EditorGUILayout.TextField("Scene Path", _scenePath);
            _roadWidth = EditorGUILayout.FloatField("Road Width", _roadWidth);
            _roadThickness = EditorGUILayout.FloatField("Road Thickness", _roadThickness);
            _createLevel02 = EditorGUILayout.Toggle("Create Level 2 Copy", _createLevel02);

            EditorGUILayout.Space(10);
            var tmpReady = IsTmpReady();
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("TMP Essential Resources", tmpReady ? "READY" : "MISSING");
                if (!tmpReady && GUILayout.Button("Import", GUILayout.Width(90)))
                    ImportTmpEssentials();
            }

            EditorGUILayout.Space(10);
            using (new EditorGUI.DisabledScope(!tmpReady))
            {
                if (GUILayout.Button("CREATE FRESH GAMEPLAY SCENE FROM ZERO", GUILayout.Height(44)))
                    CreateFreshScene();
            }

            if (GUILayout.Button("Open Clean Scene", GUILayout.Height(28)))
                OpenCleanScene();

            if (GUILayout.Button("Validate Clean Scene", GUILayout.Height(28)))
                ValidateCleanScene();

            EditorGUILayout.Space(12);
            EditorGUILayout.HelpBox(
                "After creation: edit Gameplay_Clean.unity directly. Move road segments, gates, finish, waypoints and PickupStrip objects with normal Unity tools. Do not run V6/V8/old repair builders on this clean scene.",
                MessageType.Warning);

            EditorGUILayout.EndScrollView();
        }

        private static bool IsTmpReady()
        {
            return Resources.Load<TMP_Settings>("TMP Settings") != null;
        }

        private static void ImportTmpEssentials()
        {
            try
            {
                var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMP_Text).Assembly);
                if (packageInfo == null)
                {
                    EditorUtility.DisplayDialog("TMP", "Could not resolve the package that contains TextMesh Pro.", "OK");
                    return;
                }

                var packagePath = Path.Combine(packageInfo.resolvedPath, "Package Resources", "TMP Essential Resources.unitypackage");
                if (!File.Exists(packagePath))
                {
                    EditorUtility.DisplayDialog(
                        "TMP Essentials",
                        "Automatic package location was not found. Use Window > TextMeshPro > Import TMP Essential Resources, then reopen this builder.",
                        "OK");
                    return;
                }

                AssetDatabase.ImportPackage(packagePath, false);
                Debug.Log("RUN RICH CLEAN BUILDER: TMP Essential Resources import requested. Wait for Unity to finish importing, then press Create again.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    "TMP Essentials",
                    "Automatic import failed. Use Window > TextMeshPro > Import TMP Essential Resources.",
                    "OK");
            }
        }

        /// <summary>Deterministic entry point used by automated validation and project setup.</summary>
        public static void RebuildDefaultScene()
        {
            if (!IsTmpReady())
                throw new InvalidOperationException("TMP Essential Resources are required before rebuilding the test scene.");

            var builder = CreateInstance<RunRichCleanLevelBuilder>();
            try
            {
                builder._scenePath = DefaultScenePath;
                builder._createLevel02 = true;
                if (!builder.CreateFreshSceneCore(showCompletionDialog: false))
                    throw new InvalidOperationException("RUN RICH CLEAN BUILDER: scene rebuild did not complete successfully.");
            }
            finally
            {
                DestroyImmediate(builder);
            }
        }

        private void CreateFreshScene()
        {
            if (!IsTmpReady())
            {
                EditorUtility.DisplayDialog("TMP Required", "Import TMP Essential Resources first.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "Create clean scene?",
                    $"A completely new scene will be created at:\n{_scenePath}\n\nThe existing Gameplay.unity will NOT be modified.",
                    "Create",
                    "Cancel"))
                return;

            CreateFreshSceneCore(showCompletionDialog: true);
        }

        private bool CreateFreshSceneCore(bool showCompletionDialog)
        {
            EnsureRunRichRenderPipeline();
            EnsureFolder("Assets/_Game/Scenes");
            EnsureFolder(GeneratedRoot);
            EnsureFolder(GeneratedPrefabs);
            EnsureFolder(GeneratedFonts);
            EnsureFolder(GeneratedAnimations);
            EnsureFolder(GeneratedMaterials);

            PrepareRunnerIdleWalkAnimation();

            var font = GetOrCreateDynamicFont();
            if (font == null)
            {
                if (showCompletionDialog)
                    EditorUtility.DisplayDialog("Font", "Could not create the TMP font asset. Check Inter-SemiBold.ttf.", "OK");
                Debug.LogError("RUN RICH CLEAN BUILDER: dynamic TMP font creation failed.");
                return false;
            }

            var moneyDefinition = GetOrCreatePickupDefinition("Assets/_Game/Data/Definitions/MoneyPickup.asset", 2);
            var bottleDefinition = GetOrCreatePickupDefinition("Assets/_Game/Data/Definitions/BottlePickup.asset", -20);

            // Phase 2: use the authored pickup prefabs already present in the project.
            // Only gameplay wiring/collider is normalized; visual hierarchy/materials are preserved.
            var moneyPrefab = PrepareReferencePickupPrefab(
                MoneyPrefabPath,
                moneyDefinition,
                0.3000000119f,
                GetOrCreatePickupVisualMaterial(true),
                true);
            var bottlePrefab = PrepareReferencePickupPrefab(
                BottlePrefabPath,
                bottleDefinition,
                0.3000000119f,
                GetOrCreatePickupVisualMaterial(false),
                false);

            AssignDefinitionPrefab(moneyDefinition, moneyPrefab);
            AssignDefinitionPrefab(bottleDefinition, bottlePrefab);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            ConfigureReferencePresentation();

            var environment = new GameObject("Environment");
            CreateWorldGround(environment.transform);

            var levelRoot = new GameObject("LevelRoot");
            var level01 = BuildLevel("Level01", 1, levelRoot.transform, font, moneyDefinition, bottleDefinition);

            LevelBindings level02 = null;
            if (_createLevel02)
            {
                var copy = Instantiate(level01.gameObject, levelRoot.transform);
                copy.name = "Level02";
                level02 = copy.GetComponent<LevelBindings>();
                SetSerializedInt(level02, "levelNumber", 2);
                copy.SetActive(false);
            }

            var camera = CreateCamera();
            CreateDirectionalLight();
            CreateGlobalVolume();

            var ui = CreateUi(font, out var hud);
            _ = ui;
            CreateEventSystem();

            var systems = new GameObject("Systems");
            var bootstrap = systems.AddComponent<GameBootstrapper>();
            WireBootstrap(bootstrap, camera, hud, level01, level02);

            CreateVendorReferenceObject();

            // Phase 2 final: all gameplay reference materials are repaired in-place to use
            // URP ports of the extracted reference shaders, so no generic material replacement
            // pass is needed here.

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, _scenePath))
            {
                if (showCompletionDialog)
                    EditorUtility.DisplayDialog("Save failed", "Unity could not save the clean scene.", "OK");
                Debug.LogError($"RUN RICH CLEAN BUILDER: Unity could not save {_scenePath}.");
                return false;
            }

            SetAsOnlyBuildScene(_scenePath);
            Selection.activeGameObject = level01.gameObject;
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(_scenePath));

            Debug.Log($"RUN RICH CLEAN BUILDER: Created fresh authored scene at {_scenePath}. No old Gameplay hierarchy was reused.");
            if (showCompletionDialog)
            {
                EditorUtility.DisplayDialog(
                    "Clean scene created",
                    "Gameplay_Clean.unity was created from an empty scene.\n\nOpen the hierarchy and edit Level01 directly. Old Gameplay.unity was not changed.",
                    "OK");
            }

            return true;
        }

        private void OpenCleanScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(_scenePath) == null)
            {
                EditorUtility.DisplayDialog("Scene", $"Scene not found:\n{_scenePath}", "OK");
                return;
            }

            EditorSceneManager.OpenScene(_scenePath, OpenSceneMode.Single);
        }

        private bool ValidateCleanScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(_scenePath) == null)
            {
                Debug.LogError($"RUN RICH CLEAN VALIDATION: Scene missing: {_scenePath}");
                return false;
            }

            var scene = EditorSceneManager.OpenScene(_scenePath, OpenSceneMode.Single);
            var roots = scene.GetRootGameObjects();
            var missing = new List<string>();

            RequireRoot(roots, "Environment", missing);
            RequireRoot(roots, "LevelRoot", missing);
            RequireRoot(roots, "CamCont", missing);
            RequireRoot(roots, "Directional Light", missing);
            RequireRoot(roots, "Global Volume", missing);
            RequireRoot(roots, "UI", missing);
            RequireRoot(roots, "Systems", missing);
            RequireRoot(roots, "EventSystem", missing);
            ValidateConfirmedConfigValues(missing);
            ValidateNoMissingScripts(roots, missing);

            var levelRoot = FindRoot(roots, "LevelRoot");
            if (levelRoot == null || levelRoot.transform.Find("Level01") == null)
                missing.Add("LevelRoot/Level01");

            var level01 = levelRoot != null ? levelRoot.transform.Find("Level01") : null;
            var level02 = levelRoot != null ? levelRoot.transform.Find("Level02") : null;
            if (level02 == null)
            {
                missing.Add("LevelRoot/Level02");
            }
            else
            {
                var level02Bindings = level02.GetComponent<LevelBindings>();
                if (level02Bindings == null)
                    missing.Add("Level02/LevelBindings");
                else
                {
                    if (level02Bindings.LevelNumber != 2) missing.Add("Level02/levelNumber must be 2");
                    if (level02Bindings.Runner == null) missing.Add("Level02/Runner");
                    if (level02Bindings.Appearance == null) missing.Add("Level02/Appearance");
                    if (level02Bindings.AnimationView == null) missing.Add("Level02/AnimationView");
                    if (level02Bindings.GetPathPoints().Length < 2) missing.Add("Level02/path waypoints");
                }
            }

            var systemsRoot = FindRoot(roots, "Systems");
            var bootstrap = systemsRoot != null ? systemsRoot.GetComponent<GameBootstrapper>() : null;
            if (bootstrap == null)
            {
                missing.Add("Systems/GameBootstrapper");
            }
            else
            {
                var bootstrapSerialized = new SerializedObject(bootstrap);
                var levelsProperty = bootstrapSerialized.FindProperty("authoredLevels");
                if (levelsProperty == null || !levelsProperty.isArray || levelsProperty.arraySize != 2)
                {
                    missing.Add("GameBootstrapper/authoredLevels must contain Level01 + Level02");
                }
                else
                {
                    var first = levelsProperty.GetArrayElementAtIndex(0).objectReferenceValue as LevelBindings;
                    var second = levelsProperty.GetArrayElementAtIndex(1).objectReferenceValue as LevelBindings;
                    if (first == null || second == null || first == second)
                        missing.Add("GameBootstrapper/authoredLevels references are invalid");
                    else if (first.LevelNumber != 1 || second.LevelNumber != 2)
                        missing.Add("GameBootstrapper/authoredLevels order must be Level01, Level02");
                }
            }

            if (bootstrap != null)
            {
                var bootstrapSerialized = new SerializedObject(bootstrap);
                RequireSerializedObject(bootstrapSerialized, "gameplayCamera", "GameBootstrapper/gameplayCamera", missing);
                RequireSerializedObject(bootstrapSerialized, "runnerConfig", "GameBootstrapper/runnerConfig", missing);
                RequireSerializedObject(bootstrapSerialized, "cameraConfig", "GameBootstrapper/cameraConfig", missing);
                RequireSerializedObject(bootstrapSerialized, "wealthConfig", "GameBootstrapper/wealthConfig", missing);
                RequireSerializedObject(bootstrapSerialized, "hud", "GameBootstrapper/hud", missing);
            }

            ValidateCameraHierarchy(roots, missing);
            ValidateCoreRuntimeLogic(missing);
            ValidateSecondLevelStructure(level02, missing);

            if (level01 != null)
            {
                RequireChild(level01, "Path", missing);
                RequireChild(level01, "Track", missing);
                RequireChild(level01, "Player", missing);
                RequireChild(level01, "PickupStrips", missing);
                RequireChild(level01, "Obstacles", missing);
                RequireChild(level01, "Sections", missing);
                RequireChild(level01, "Checkpoints", missing);
                RequireChild(level01, "Final20ChoiceGate", missing);

                var playerRoot = level01.Find("Player");
                var playerAnimator = playerRoot != null ? playerRoot.GetComponentInChildren<Animator>(true) : null;
                if (playerAnimator == null)
                {
                    missing.Add("Level01/Player/Animator");
                }
                else
                {
                    if (playerAnimator.runtimeAnimatorController == null)
                        missing.Add("Level01/Player/AnimatorController");
                    else
                        ValidateAnimatorController(playerAnimator.runtimeAnimatorController, missing);
                    if (playerAnimator.avatar == null || !playerAnimator.avatar.isValid || !playerAnimator.avatar.isHuman)
                        missing.Add("Level01/Player/HumanoidAvatar");
                    if (playerAnimator.applyRootMotion)
                        missing.Add("Level01/Player/Animator applyRootMotion must be OFF");

                    ValidateHumanoidAnimationImport(RunnerIdleAnimationPath, "Idle", missing);
                    ValidateHumanoidAnimationImport(RunnerWalkAnimationPath, "Walk", missing);
                }

                var appearance = playerRoot != null ? playerRoot.GetComponent<PlayerAppearance>() : null;
                if (appearance == null)
                {
                    missing.Add("Level01/Player/PlayerAppearance");
                }
                else
                {
                    var appearanceSerialized = new SerializedObject(appearance);
                    RequireSerializedObject(appearanceSerialized, "hoboVisual", "PlayerAppearance/hoboVisual", missing);
                    RequireSerializedObject(appearanceSerialized, "poorVisual", "PlayerAppearance/poorVisual", missing);
                    RequireSerializedObject(appearanceSerialized, "decentVisual", "PlayerAppearance/decentVisual", missing);
                    RequireSerializedObject(appearanceSerialized, "richVisual", "PlayerAppearance/richVisual", missing);
                    RequireSerializedObject(appearanceSerialized, "millionaireVisual", "PlayerAppearance/millionaireVisual", missing);
                    RequireSerializedObject(appearanceSerialized, "animator", "PlayerAppearance/animator", missing);
                }

                var feedback = playerRoot != null ? playerRoot.GetComponentInChildren<RunnerFeedback>(true) : null;
                if (feedback == null)
                {
                    missing.Add("Level01/Player/RunnerFeedback");
                }
                else
                {
                    var feedbackSerialized = new SerializedObject(feedback);
                    RequireSerializedObject(feedbackSerialized, "positiveClip", "RunnerFeedback/positiveClip", missing);
                    RequireSerializedObject(feedbackSerialized, "negativeClip", "RunnerFeedback/negativeClip", missing);
                    RequireSerializedObject(feedbackSerialized, "checkpointClip", "RunnerFeedback/checkpointClip", missing);
                    RequireSerializedObject(feedbackSerialized, "winClip", "RunnerFeedback/winClip", missing);
                    RequireSerializedObject(feedbackSerialized, "multiplierLowClip", "RunnerFeedback/multiplierLowClip", missing);
                    RequireSerializedObject(feedbackSerialized, "multiplierMidClip", "RunnerFeedback/multiplierMidClip", missing);
                    RequireSerializedObject(feedbackSerialized, "multiplierHighClip", "RunnerFeedback/multiplierHighClip", missing);
                    RequireSerializedArray(feedbackSerialized, "footstepClips", "RunnerFeedback/footstepClips", missing);
                    RequireSerializedArray(feedbackSerialized, "heelClips", "RunnerFeedback/heelClips", missing);
                    RequireSerializedObject(feedbackSerialized, "billboardRoot", "RunnerFeedback/billboardRoot", missing);
                    RequireSerializedArray(feedbackSerialized, "moneyBursts", "RunnerFeedback/moneyBursts", missing);
                    RequireSerializedArray(feedbackSerialized, "negativeShards", "RunnerFeedback/negativeShards", missing);
                    RequireSerializedArray(feedbackSerialized, "negativeStars", "RunnerFeedback/negativeStars", missing);
                }

                var pickupTriggers = level01.GetComponentsInChildren<PickupTrigger>(true);
                for (var i = 0; i < pickupTriggers.Length; i++)
                {
                    if (pickupTriggers[i] != null &&
                        pickupTriggers[i].transform.Find("ReferencePickupPresentation") == null)
                    {
                        missing.Add("Pickup visual presentation missing");
                        break;
                    }
                }

                ValidatePickupPrefab(MoneyPrefabPath, "bills", "props_flat", missing);
                ValidatePickupPrefab(BottlePrefabPath, "bottle", "props_flat_bad", missing);

                var checkpointRoot = level01.Find("Checkpoints");
                var checkpointCount = checkpointRoot != null
                    ? checkpointRoot.GetComponentsInChildren<CheckpointTrigger>(true).Length
                    : 0;
                if (checkpointCount != 3)
                    missing.Add($"Checkpoints expected 3, got {checkpointCount}");

                var nullWorldSprites = level01.GetComponentsInChildren<SpriteRenderer>(true);
                for (var i = 0; i < nullWorldSprites.Length; i++)
                {
                    if (nullWorldSprites[i] != null && nullWorldSprites[i].sprite == null)
                        missing.Add($"SpriteRenderer without sprite: {GetHierarchyPath(nullWorldSprites[i].transform)}");
                }

                var final20Gate = level01.Find("Final20ChoiceGate");
                if (final20Gate != null)
                {
                    var badDoorVisual = final20Gate.Find("BadDoor/DoorVisual");
                    var goodDoorVisual = final20Gate.Find("GoodDoor/DoorVisual");
                    if (badDoorVisual == null) missing.Add("Final20/BadDoor/ChoiceDoor visual");
                    if (goodDoorVisual == null) missing.Add("Final20/GoodDoor/ChoiceDoor visual");

                    var badSide = final20Gate.Find("BadDoor");
                    var goodSide = final20Gate.Find("GoodDoor");
                    if (badSide != null && badSide.localPosition.x >= 0f)
                        missing.Add("Final20/BadDoor must be LEFT");
                    if (goodSide != null && goodSide.localPosition.x <= 0f)
                        missing.Add("Final20/GoodDoor must be RIGHT");

                    ValidateChoiceDoor(badSide, -20, "Final20/BadDoor", missing);
                    ValidateChoiceDoor(goodSide, 20, "Final20/GoodDoor", missing);
                    var choiceGroup = final20Gate.GetComponent<GateChoiceGroup>();
                    if (choiceGroup == null)
                        missing.Add("Final20/GateChoiceGroup");
                    else
                    {
                        var groupSerialized = new SerializedObject(choiceGroup);
                        var colliders = groupSerialized.FindProperty("choiceColliders");
                        if (colliders == null || !colliders.isArray || colliders.arraySize != 2)
                            missing.Add("Final20/GateChoiceGroup choiceColliders=2");
                    }
                }

                var sectionsRoot = level01.Find("Sections");
                var sectionEnd = sectionsRoot != null ? sectionsRoot.Find("SectionEndAB_Test") : null;
                var finishRoot = sectionEnd != null ? sectionEnd.Find("Finish") : null;
                if (finishRoot == null)
                {
                    missing.Add("Level01/Sections/SectionEndAB_Test/Finish");
                }
                else
                {
                    ValidateFinishGate(finishRoot, "Door_POOR", missing);
                    ValidateFinishGate(finishRoot, "Door_DECENT", missing);
                    ValidateFinishGate(finishRoot, "Door_RICH", missing);
                    ValidateFinishGate(finishRoot, "Door_MILLIONAIRE", missing);
                    RequireChild(finishRoot, "FinishPoorTrash_L", missing);
                    RequireChild(finishRoot, "FinishPoorTrash_R", missing);
                    RequireChild(finishRoot, "FinishDecentBench_L", missing);
                    RequireChild(finishRoot, "FinishDecentBench_R", missing);
                    RequireChild(finishRoot, "FinishRichPlant_L", missing);
                    RequireChild(finishRoot, "FinishRichPlant_R", missing);

                    var statusDoors = finishRoot.GetComponentsInChildren<StatusDoorTrigger>(true);
                    if (statusDoors.Length != 5)
                        missing.Add($"Finish/StatusDoorTrigger count expected 5, got {statusDoors.Length}");
                    ValidateStatusDoor(finishRoot, "DOOR_01", WealthState.Poor, 2, false, missing);
                    ValidateStatusDoor(finishRoot, "DOOR_02", WealthState.Decent, 3, false, missing);
                    ValidateStatusDoor(finishRoot, "DOOR_03", WealthState.Rich, 4, false, missing);
                    ValidateStatusDoor(finishRoot, "DOOR_04", WealthState.Millionaire, 5, false, missing);
                    ValidateStatusDoor(finishRoot, "FinalTrigger", WealthState.Hobo, 5, true, missing);

                    var finishMoney = finishRoot.parent != null
                        ? finishRoot.parent.Find("Items")
                        : null;
                    ValidateFinishMoneyLadder(finishMoney, missing);
                }
            }

            var uiRoot = FindRoot(roots, "UI");
            var gameplayHud = uiRoot != null ? uiRoot.GetComponent<GameplayHud>() : null;
            if (gameplayHud == null)
                missing.Add("UI/GameplayHud");

            if (uiRoot != null && uiRoot.transform.Find("SafeArea") == null)
                missing.Add("UI/SafeArea");
            if (uiRoot != null && uiRoot.GetComponentInChildren<TutorialSwipeAnimator>(true) == null)
                missing.Add("UI/TutorialSwipeAnimator");
            if (uiRoot != null)
            {
                var safe = uiRoot.transform.Find("SafeArea");
                if (safe != null)
                {
                    RequireChild(safe, "PersistentHUD", missing);
                    RequireChild(safe, "ReadyPanel", missing);
                    var readyPanel = safe.Find("ReadyPanel");
                    if (readyPanel != null)
                    {
                        var readyProgress = readyPanel.Find("ReadyLevelProgress");
                        if (readyProgress == null)
                            missing.Add("UI/ReadyLevelProgress");
                        else
                        {
                            RequireChild(readyProgress, "LevelPortraitStart", missing);
                            RequireChild(readyProgress, "LevelPortraitEnd", missing);
                            for (var i = 1; i <= 5; i++)
                            {
                                RequireChild(readyProgress, $"LevelSegment_{i}", missing);
                                RequireChild(readyProgress, $"LevelDot_{i}", missing);
                            }
                        }
                    }
                    RequireChild(safe, "GameplayPanel", missing);
                    RequireChild(safe, "WinPanel", missing);
                    RequireChild(safe, "LosePanel", missing);
                }
            }
            if (gameplayHud != null)
            {
                var hudSerialized = new SerializedObject(gameplayHud);
                RequireSerializedObject(hudSerialized, "readyPanel", "GameplayHud/readyPanel", missing);
                RequireSerializedObject(hudSerialized, "gameplayPanel", "GameplayHud/gameplayPanel", missing);
                RequireSerializedObject(hudSerialized, "winPanel", "GameplayHud/winPanel", missing);
                RequireSerializedObject(hudSerialized, "losePanel", "GameplayHud/losePanel", missing);
                RequireSerializedObject(hudSerialized, "restartButton", "GameplayHud/restartButton", missing);
                RequireSerializedObject(hudSerialized, "nextButton", "GameplayHud/nextButton", missing);
                RequireSerializedObject(hudSerialized, "runnerStatus", "GameplayHud/runnerStatus", missing);
            }

            if (missing.Count == 0)
            {
                Debug.Log("RUN RICH CLEAN VALIDATION: PASS. Scene is authored, wired and ready for Play Mode.");
                return true;
            }

            Debug.LogError("RUN RICH CLEAN VALIDATION: Missing: " + string.Join(", ", missing));
            return false;
        }

        private static void ValidateHumanoidAnimationImport(string assetPath, string label, ICollection<string> missing)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null)
            {
                missing.Add($"Animator/{label} importer missing");
                return;
            }

            if (!importer.importAnimation)
                missing.Add($"Animator/{label} importAnimation disabled");
            if (importer.animationType != ModelImporterAnimationType.Human)
                missing.Add($"Animator/{label} must be Humanoid");
            if (importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
                missing.Add($"Animator/{label} must create a Humanoid Avatar for retargeting");
            Avatar clipAvatar = null;
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                clipAvatar = asset as Avatar;
                if (clipAvatar != null) break;
            }
            if (clipAvatar == null || !clipAvatar.isValid || !clipAvatar.isHuman)
                missing.Add($"Animator/{label} Humanoid Avatar is invalid");

            var clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0)
                clips = importer.defaultClipAnimations;
            if (clips == null || clips.Length == 0)
            {
                missing.Add($"Animator/{label} has no animation clip");
                return;
            }

            for (var i = 0; i < clips.Length; i++)
            {
                if (!clips[i].loopTime)
                    missing.Add($"Animator/{label}/{clips[i].name} must loop");
            }
        }

        private static void ValidateAnimatorController(RuntimeAnimatorController runtimeController, ICollection<string> missing)
        {
            var controller = runtimeController as AnimatorController;
            if (controller == null)
            {
                missing.Add("Runner AnimatorController asset type");
                return;
            }

            var hasRunning = false;
            var hasRich = false;
            foreach (var parameter in controller.parameters)
            {
                if (parameter.name == "Running" && parameter.type == AnimatorControllerParameterType.Bool) hasRunning = true;
                if (parameter.name == "Rich" && parameter.type == AnimatorControllerParameterType.Float) hasRich = true;
            }
            if (!hasRunning) missing.Add("AnimatorController/Running bool");
            if (!hasRich) missing.Add("AnimatorController/Rich float");

            if (controller.layers == null || controller.layers.Length == 0)
            {
                missing.Add("AnimatorController/layer");
                return;
            }

            var hasIdle = false;
            var hasWalk = false;
            foreach (var child in controller.layers[0].stateMachine.states)
            {
                if (child.state == null) continue;
                if (child.state.name == "Idle" && child.state.motion != null) hasIdle = true;
                if (child.state.name == "Walk" && child.state.motion != null) hasWalk = true;
            }
            if (!hasIdle) missing.Add("AnimatorController/Idle clip");
            if (!hasWalk) missing.Add("AnimatorController/Walk clip");
        }

        private static void ValidateChoiceDoor(Transform side, int expectedDelta, string label, ICollection<string> missing)
        {
            if (side == null)
            {
                missing.Add(label);
                return;
            }

            var trigger = side.GetComponent<GateChoiceTrigger>();
            var collider = side.GetComponent<Collider>();
            if (trigger == null) missing.Add(label + "/GateChoiceTrigger");
            if (collider == null || !collider.isTrigger) missing.Add(label + "/trigger collider");
            var visual = side.Find("DoorVisual");
            if (visual == null)
                missing.Add(label + "/DoorVisual");
            else if (Mathf.Abs(visual.localScale.x - 4.45f) > 0.02f)
                missing.Add(label + "/DoorVisual width must match the 2.7m choice lane");

            if (collider is BoxCollider box && Mathf.Abs(box.size.x - 2.7f) > 0.02f)
                missing.Add(label + "/choice collider width=2.7");
            if (trigger != null)
            {
                var serialized = new SerializedObject(trigger);
                var delta = serialized.FindProperty("wealthDelta");
                if (delta == null || delta.intValue != expectedDelta)
                    missing.Add(label + $"/wealthDelta={expectedDelta}");
            }
        }

        private static void ValidateStatusDoor(
            Transform finishRoot,
            string name,
            WealthState expectedState,
            int expectedMultiplier,
            bool expectedMillionaire,
            ICollection<string> missing)
        {
            var node = finishRoot != null ? finishRoot.Find(name) : null;
            var trigger = node != null ? node.GetComponent<StatusDoorTrigger>() : null;
            if (trigger == null)
            {
                missing.Add($"Finish/{name}/StatusDoorTrigger");
                return;
            }

            var serialized = new SerializedObject(trigger);
            var minimumState = serialized.FindProperty("minimumState");
            var multiplier = serialized.FindProperty("multiplier");
            var millionaire = serialized.FindProperty("millionaire");
            if (minimumState == null || minimumState.enumValueIndex != (int)expectedState)
                missing.Add($"Finish/{name}/minimumState={expectedState}");
            if (multiplier == null || multiplier.intValue != expectedMultiplier)
                missing.Add($"Finish/{name}/multiplier={expectedMultiplier}");
            if (millionaire == null || millionaire.boolValue != expectedMillionaire)
                missing.Add($"Finish/{name}/millionaire={expectedMillionaire}");
        }

        private static void ValidateConfirmedConfigValues(ICollection<string> missing)
        {
            var runner = AssetDatabase.LoadAssetAtPath<RunnerConfigSO>("Assets/_Game/Data/Configs/RunnerConfig.asset");
            var wealth = AssetDatabase.LoadAssetAtPath<WealthConfigSO>("Assets/_Game/Data/Configs/WealthConfig.asset");
            var camera = AssetDatabase.LoadAssetAtPath<CameraConfigSO>("Assets/_Game/Data/Configs/CameraConfig.asset");
            var money = AssetDatabase.LoadAssetAtPath<PickupDefinitionSO>("Assets/_Game/Data/Definitions/MoneyPickup.asset");
            var bottle = AssetDatabase.LoadAssetAtPath<PickupDefinitionSO>("Assets/_Game/Data/Definitions/BottlePickup.asset");

            if (runner == null) missing.Add("RunnerConfig.asset");
            else
            {
                if (!Mathf.Approximately(runner.ForwardSpeed, 8f)) missing.Add("RunnerConfig/ForwardSpeed=8");
                if (!Mathf.Approximately(runner.SwerveSensitivity, 20f)) missing.Add("RunnerConfig/SwerveSensitivity=20");
                if (!Mathf.Approximately(runner.RoadHalfWidth, 2.5f)) missing.Add("RunnerConfig/track width=5");
                if (!Mathf.Approximately(runner.LateralSmoothTime, 0.1f)) missing.Add("RunnerConfig/SmoothDamp=0.1");
                if (!Mathf.Approximately(runner.LateralMaxSpeed, 80f)) missing.Add("RunnerConfig/maxSpeed=80");
                if (!Mathf.Approximately(runner.MaxTurnAngle, 35f)) missing.Add("RunnerConfig/visual yaw=35");
                if (!Mathf.Approximately(runner.VisualRotationSpeed, 10f)) missing.Add("RunnerConfig/visual rotation speed=10");
                if (runner.SlowSpeedWhenPoor) missing.Add("RunnerConfig/SlowSpeedWhenPoor must be OFF for confirmed RunningSpeed=8");
            }

            if (wealth == null) missing.Add("WealthConfig.asset");
            else
            {
                if (wealth.InitialWealth != 40 || wealth.MinimumWealth != 0 || wealth.MaximumWealth != 140)
                    missing.Add("WealthConfig/range or initial value");
                if (wealth.PoorThreshold != 31 || wealth.DecentThreshold != 61 ||
                    wealth.RichThreshold != 101 || wealth.MillionaireThreshold != 139)
                    missing.Add("WealthConfig/status thresholds");
            }

            if (camera == null) missing.Add("CameraConfig.asset");
            else
            {
                if (!Mathf.Approximately(camera.FieldOfView, 60f)) missing.Add("CameraConfig/FOV=60");
                if (!Mathf.Approximately(camera.NearClipPlane, 0.3f)) missing.Add("CameraConfig/near=0.3");
                if (!Mathf.Approximately(camera.FarClipPlane, 500f)) missing.Add("CameraConfig/far=500");
                if (!Mathf.Approximately(camera.FollowSmoothTime, 0.01f)) missing.Add("CameraConfig/follow smooth=0.01");
            }

            if (money == null || money.WealthDelta != 2) missing.Add("MoneyPickup/+2");
            if (bottle == null || bottle.WealthDelta != -20) missing.Add("BottlePickup/-20");
        }

        private static void ValidateNoMissingScripts(IEnumerable<GameObject> roots, ICollection<string> missing)
        {
            foreach (var root in roots)
            {
                if (root == null) continue;
                var transforms = root.GetComponentsInChildren<Transform>(true);
                for (var i = 0; i < transforms.Length; i++)
                {
                    var go = transforms[i] != null ? transforms[i].gameObject : null;
                    if (go != null && GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go) > 0)
                        missing.Add($"Missing script: {GetHierarchyPath(go.transform)}");
                }
            }
        }

        private static void ValidatePickupPrefab(string prefabPath, string expectedMeshName, string expectedMaterialName, ICollection<string> missing)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                missing.Add(prefabPath);
                return;
            }

            if (prefab.GetComponent<PickupTrigger>() == null) missing.Add($"{prefab.name}/PickupTrigger");
            var collider = prefab.GetComponent<SphereCollider>();
            if (collider == null || !collider.isTrigger) missing.Add($"{prefab.name}/trigger collider");
            if (prefab.transform.Find("ReferencePickupPresentation") == null) missing.Add($"{prefab.name}/ReferencePickupPresentation");
            var presentationRoot = prefab.transform.Find("ReferencePickupPresentation");
            if (presentationRoot != null)
            {
                if (presentationRoot.GetComponent<PickupPresentation>() == null)
                    missing.Add($"{prefab.name}/PickupPresentation component");
                var billboard = presentationRoot.Find("Billboard");
                if (billboard == null)
                    missing.Add($"{prefab.name}/Presentation/Billboard");
                else if (expectedMeshName.IndexOf("bills", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (billboard.Find("PlusMarker") == null) missing.Add($"{prefab.name}/PlusMarker");
                    if (billboard.Find("PickupAura") == null) missing.Add($"{prefab.name}/PickupAura");
                }
                else if (expectedMeshName.IndexOf("bottle", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (billboard.Find("NegativeMarker") == null) missing.Add($"{prefab.name}/NegativeMarker");
                }
            }

            var hasExpectedMesh = false;
            var hasExpectedMaterial = false;
            var filters = prefab.GetComponentsInChildren<MeshFilter>(true);
            for (var i = 0; i < filters.Length; i++)
            {
                var mesh = filters[i] != null ? filters[i].sharedMesh : null;
                if (mesh != null &&
                    AssetDatabase.GetAssetPath(mesh).EndsWith("/" + expectedMeshName + ".fbx", StringComparison.OrdinalIgnoreCase))
                    hasExpectedMesh = true;
            }

            var renderers = prefab.GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] is SpriteRenderer) continue;
                var materials = renderers[i].sharedMaterials;
                for (var j = 0; j < materials.Length; j++)
                {
                    var material = materials[j];
                    if (material != null && material.name.IndexOf(expectedMaterialName, StringComparison.OrdinalIgnoreCase) >= 0)
                        hasExpectedMaterial = true;
                }
            }

            if (!hasExpectedMesh) missing.Add($"{prefab.name}/{expectedMeshName} model");
            if (!hasExpectedMaterial) missing.Add($"{prefab.name}/{expectedMaterialName} material");
        }

        private static void ValidateFinishGate(Transform finishRoot, string doorName, ICollection<string> missing)
        {
            var door = finishRoot != null ? finishRoot.Find(doorName) : null;
            if (door == null)
            {
                missing.Add($"Finish/{doorName}");
                return;
            }

            var gate = door.GetComponent<GateOpenVisual>();
            if (gate == null)
            {
                missing.Add($"Finish/{doorName}/GateOpenVisual");
                return;
            }

            var serialized = new SerializedObject(gate);
            RequireSerializedObject(serialized, "leftLeaf", $"Finish/{doorName}/leftLeaf", missing);
            RequireSerializedObject(serialized, "rightLeaf", $"Finish/{doorName}/rightLeaf", missing);
            var openDistance = serialized.FindProperty("openDistance");
            var duration = serialized.FindProperty("duration");
            if (openDistance == null || openDistance.floatValue <= 0f)
                missing.Add($"Finish/{doorName}/openDistance");
            if (duration == null || duration.floatValue <= 0f)
                missing.Add($"Finish/{doorName}/open duration");
            if (door.Find("Visual") == null)
                missing.Add($"Finish/{doorName}/Visual frame");

            var leftProperty = serialized.FindProperty("leftLeaf");
            var rightProperty = serialized.FindProperty("rightLeaf");
            var leftLeaf = leftProperty != null ? leftProperty.objectReferenceValue as Transform : null;
            var rightLeaf = rightProperty != null ? rightProperty.objectReferenceValue as Transform : null;
            if (leftLeaf != null && rightLeaf != null)
            {
                if (leftLeaf == rightLeaf)
                {
                    missing.Add($"Finish/{doorName}/door leaves must be different objects");
                }
                else
                {
                    var visualRoot = door.Find("Visual");
                    if (visualRoot != null)
                    {
                        var leftRenderer = leftLeaf.GetComponentInChildren<Renderer>(true);
                        var rightRenderer = rightLeaf.GetComponentInChildren<Renderer>(true);
                        var leftWorld = leftRenderer != null ? leftRenderer.bounds.center : leftLeaf.position;
                        var rightWorld = rightRenderer != null ? rightRenderer.bounds.center : rightLeaf.position;
                        var leftX = visualRoot.InverseTransformPoint(leftWorld).x;
                        var rightX = visualRoot.InverseTransformPoint(rightWorld).x;
                        if (leftX >= rightX)
                            missing.Add($"Finish/{doorName}/leftLeaf-rightLeaf physical ordering is reversed");
                    }
                }
            }
        }

        private static void ValidateFinishMoneyLadder(Transform itemsRoot, ICollection<string> missing)
        {
            if (itemsRoot == null)
            {
                missing.Add("Finish/special money ladder");
                return;
            }

            var pickups = itemsRoot.GetComponentsInChildren<PickupTrigger>(true);
            var counts = new int[4];
            var specialCount = 0;
            for (var i = 0; i < pickups.Length; i++)
            {
                var pickup = pickups[i];
                if (pickup == null || !pickup.name.StartsWith("SpecialRich", StringComparison.Ordinal))
                    continue;

                specialCount++;
                var z = pickup.transform.localPosition.z;
                if (z > 8.5f && z < 12.63f) counts[0]++;
                else if (z > 15.14f && z < 27.77f) counts[1]++;
                else if (z > 30.14f && z < 42.75f) counts[2]++;
                else if (z > 45.14f && z < 55.30f) counts[3]++;
            }

            if (specialCount != 40)
                missing.Add($"Finish/SpecialRich expected 40, got {specialCount}");
            for (var i = 0; i < counts.Length; i++)
            {
                if (counts[i] != 10)
                    missing.Add($"Finish/money interval {i + 1} expected 10, got {counts[i]}");
            }
        }

        private static void ValidateCameraHierarchy(IEnumerable<GameObject> roots, ICollection<string> missing)
        {
            var camCont = FindRoot(roots, "CamCont");
            if (camCont == null) return;
            var distCam = camCont.transform.Find("DistCam");
            var mainCamera = distCam != null ? distCam.Find("Main Camera") : null;
            if (distCam == null) missing.Add("CamCont/DistCam");
            if (mainCamera == null) missing.Add("CamCont/DistCam/Main Camera");
            else
            {
                var camera = mainCamera.GetComponent<UnityEngine.Camera>();
                if (camera == null) missing.Add("Main Camera/Camera component");
                else
                {
                    if (!Mathf.Approximately(camera.fieldOfView, 60f)) missing.Add("Main Camera/FOV=60");
                    if (!Mathf.Approximately(camera.nearClipPlane, 0.3f)) missing.Add("Main Camera/near=0.3");
                    if (!Mathf.Approximately(camera.farClipPlane, 500f)) missing.Add("Main Camera/far=500");
                }
                if (!mainCamera.CompareTag("MainCamera")) missing.Add("Main Camera/tag=MainCamera");
                if (mainCamera.GetComponent<AudioListener>() == null) missing.Add("Main Camera/AudioListener");
            }
        }

        private static void ValidateSecondLevelStructure(Transform level02, ICollection<string> missing)
        {
            if (level02 == null) return;
            var pickups = level02.GetComponentsInChildren<PickupTrigger>(true);
            if (pickups.Length == 0) missing.Add("Level02/pickups");
            var checkpoints = level02.GetComponentsInChildren<CheckpointTrigger>(true);
            if (checkpoints.Length != 3) missing.Add($"Level02/checkpoints expected 3, got {checkpoints.Length}");
            var choices = level02.GetComponentsInChildren<GateChoiceTrigger>(true);
            if (choices.Length != 2) missing.Add($"Level02/Final20 choices expected 2, got {choices.Length}");
            var statusDoors = level02.GetComponentsInChildren<StatusDoorTrigger>(true);
            if (statusDoors.Length != 5) missing.Add($"Level02/finish status doors expected 5, got {statusDoors.Length}");
            var gates = level02.GetComponentsInChildren<GateOpenVisual>(true);
            if (gates.Length != 4) missing.Add($"Level02/finish opening gates expected 4, got {gates.Length}");
            var animator = level02.GetComponentInChildren<Animator>(true);
            if (animator == null || animator.runtimeAnimatorController == null)
                missing.Add("Level02/AnimatorController");
            else if (animator.applyRootMotion)
                missing.Add("Level02/Animator applyRootMotion must be OFF");
        }

        private static void ValidateCoreRuntimeLogic(ICollection<string> missing)
        {
            GameObject temporary = null;
            try
            {
                temporary = new GameObject("RunRich_RuntimeValidation_Transient")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                var actor = temporary.AddComponent<RunnerActor>();

                var flow = new GameFlow();
                var wealth = new RichPoorController(40, 0, 140, 31, 61, 101, 139, 140f, true, true);
                var finish = new FinishProgress();
                actor.Bind(flow, wealth, finish);
                flow.StartRun();
                if (!actor.TryApplyWealth(2)) missing.Add("RuntimeLogic/Money +2 rejected");
                wealth.Tick(1f);
                if (wealth.Value != 42 || wealth.State != WealthState.Poor) missing.Add("RuntimeLogic/Money +2 state");
                if (!actor.TryApplyWealth(-20)) missing.Add("RuntimeLogic/Bottle -20 rejected");
                wealth.Tick(1f);
                if (wealth.Value != 22 || wealth.State != WealthState.Hobo) missing.Add("RuntimeLogic/Bottle -20 state");
                if (!actor.TryApplyChoiceWealth(20)) missing.Add("RuntimeLogic/Final20 +20 rejected");
                wealth.Tick(1f);
                if (wealth.Value != 42 || wealth.State != WealthState.Poor) missing.Add("RuntimeLogic/Final20 +20 state");

                wealth.Force(82);
                actor.TryPassStatusDoor(WealthState.Poor, 2, false);
                actor.TryPassStatusDoor(WealthState.Decent, 3, false);
                wealth.Force(100);
                actor.TryPassStatusDoor(WealthState.Rich, 4, false);
                if (flow.State != GameFlowState.Won || finish.Multiplier != 3)
                    missing.Add("RuntimeLogic/finish shortfall must Win at earned x3");

                // Reference finish-ladder scenario: a DECENT ~62 run must be able to collect
                // ten +2 bills in every interval and advance all the way x2 -> x5.
                var fullFlow = new GameFlow();
                var fullWealth = new RichPoorController(62, 0, 140, 31, 61, 101, 139, 140f, true, true);
                var fullFinish = new FinishProgress();
                actor.Bind(fullFlow, fullWealth, fullFinish);
                fullFlow.StartRun();
                for (var i = 0; i < 10; i++) actor.TryApplyWealth(2);
                fullWealth.Tick(1f);
                actor.TryPassStatusDoor(WealthState.Poor, 2, false);
                for (var i = 0; i < 10; i++) actor.TryApplyWealth(2);
                fullWealth.Tick(1f);
                actor.TryPassStatusDoor(WealthState.Decent, 3, false);
                for (var i = 0; i < 10; i++) actor.TryApplyWealth(2);
                fullWealth.Tick(1f);
                actor.TryPassStatusDoor(WealthState.Rich, 4, false);
                for (var i = 0; i < 10; i++) actor.TryApplyWealth(2);
                fullWealth.Tick(1f);
                actor.TryPassStatusDoor(WealthState.Millionaire, 5, false);
                if (fullFlow.State != GameFlowState.Playing || fullFinish.Multiplier != 5 ||
                    fullWealth.State != WealthState.Millionaire)
                    missing.Add("RuntimeLogic/62 + finish bills must reach x5 millionaire");
                actor.TryPassStatusDoor(WealthState.Hobo, 5, true);
                if (fullFlow.State != GameFlowState.Won || fullFinish.Multiplier != 5)
                    missing.Add("RuntimeLogic/x5 final victory");
            }
            catch (Exception exception)
            {
                missing.Add("RuntimeLogic self-test exception: " + exception.GetType().Name + " " + exception.Message);
            }
            finally
            {
                if (temporary != null)
                    DestroyImmediate(temporary);
            }
        }

        private static string GetHierarchyPath(Transform transform)
        {
            if (transform == null) return "<null>";
            var path = transform.name;
            var parent = transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }

        private LevelBindings BuildLevel(
            string name,
            int levelNumber,
            Transform parent,
            TMP_FontAsset font,
            PickupDefinitionSO money,
            PickupDefinitionSO bottle)
        {
            var level = new GameObject(name);
            level.transform.SetParent(parent, false);
            var vendorLevel = level.AddComponent<Level>();
            var bindings = level.AddComponent<LevelBindings>();

            var path = new GameObject("Path");
            path.transform.SetParent(level.transform, false);
            var points = BuildLevel01Path();

            var waypointTransforms = new Transform[points.Length];
            for (var i = 0; i < points.Length; i++)
            {
                var waypoint = new GameObject($"Waypoint_{i:00}");
                waypoint.transform.SetParent(path.transform, false);
                waypoint.transform.localPosition = points[i];
                waypointTransforms[i] = waypoint.transform;
            }

            var spawn = new GameObject("PlayerSpawnPoint");
            spawn.transform.SetParent(path.transform, false);
            spawn.transform.localPosition = points[0];
            spawn.transform.localRotation = Quaternion.LookRotation((points[1] - points[0]).normalized, Vector3.up);
            SetObjectReference(vendorLevel, "playerSpawnPoint", spawn.transform);

            var track = new GameObject("Track");
            track.transform.SetParent(level.transform, false);
            var road = new GameObject("Road");
            road.transform.SetParent(track.transform, false);
            BuildRoad(road.transform, points);

            var player = CreatePlayer(level.transform, spawn.transform, road.transform, font);
            var pickupRoot = new GameObject("PickupStrips");
            pickupRoot.transform.SetParent(level.transform, false);
            var sections = BuildExactSections(level.transform);
            BuildExactPickups(pickupRoot.transform, sections, money, bottle);

            var obstacles = new GameObject("Obstacles");
            obstacles.transform.SetParent(level.transform, false);

            BuildFinal20Choice(level.transform, sections["Final20"], font);
            BuildCheckpoints(level.transform, sections);
            BuildFinish(sections["SectionEndAB_Test"], font);

            SetSerializedInt(bindings, "levelNumber", levelNumber);
            SetObjectReference(bindings, "runner", player.Actor);
            SetObjectReference(bindings, "appearance", player.Appearance);
            SetObjectReference(bindings, "animationView", player.AnimationView);
            SetObjectReference(bindings, "playerSpawnPoint", spawn.transform);
            SetObjectArray(bindings, "pathWaypoints", waypointTransforms);
            SetObjectArray(bindings, "pickupStrips", Array.Empty<PickupStrip>());

            return bindings;
        }

        private static Vector3[] BuildLevel01Path()
        {
            var points = new List<Vector3>
            {
                new(0f, 0f, 0f),
                new(0f, 0f, 7.5f),
                new(0f, 0f, 22.5f),
                new(0f, 0f, 37.5f)
            };

            AppendRightTurn(points, new Vector3(0f, 0f, 37.5f));
            points.Add(new Vector3(10.5f, 0f, 40.5f));
            points.Add(new Vector3(25.5f, 0f, 40.5f));
            points.Add(new Vector3(40.5f, 0f, 40.5f));
            AppendLeftTurn(points, new Vector3(40.5f, 0f, 40.5f));
            points.Add(new Vector3(43.5f, 0f, 51f));
            points.Add(new Vector3(43.5f, 0f, 66f));
            points.Add(new Vector3(43.5f, 0f, 81f));
            AppendRightTurn(points, new Vector3(43.5f, 0f, 81f));
            points.Add(new Vector3(54f, 0f, 84f));
            points.Add(new Vector3(69f, 0f, 84f));
            points.Add(new Vector3(166.5f, 0f, 84f));
            return points.ToArray();
        }

        private static void AppendRightTurn(List<Vector3> points, Vector3 start)
        {
            var center = start + Vector3.right * 3f;
            for (var i = 1; i <= 8; i++)
            {
                var angle = Mathf.Lerp(Mathf.PI, Mathf.PI * 0.5f, i / 8f);
                points.Add(center + new Vector3(Mathf.Cos(angle) * 3f, 0f, Mathf.Sin(angle) * 3f));
            }
        }

        private static void AppendLeftTurn(List<Vector3> points, Vector3 start)
        {
            var center = start + Vector3.forward * 3f;
            for (var i = 1; i <= 8; i++)
            {
                var angle = Mathf.Lerp(-Mathf.PI * 0.5f, 0f, i / 8f);
                points.Add(center + new Vector3(Mathf.Cos(angle) * 3f, 0f, Mathf.Sin(angle) * 3f));
            }
        }

        private static Dictionary<string, Transform> BuildExactSections(Transform level)
        {
            var root = new GameObject("Sections");
            root.transform.SetParent(level, false);
            var result = new Dictionary<string, Transform>();
            AddSection(result, root.transform, "SectionStart", new Vector3(0f, 0f, 0f), 0f);
            AddSection(result, root.transform, "Final7", new Vector3(0f, 0f, 7.5f), 0f);
            AddSection(result, root.transform, "Final6", new Vector3(0f, 0f, 22.5f), 0f);
            AddSection(result, root.transform, "TurnRight_01", new Vector3(0f, 0f, 37.5f), 0f);
            AddSection(result, root.transform, "SafeZone_01", new Vector3(3f, 0f, 40.5f), 90f);
            AddSection(result, root.transform, "Final3", new Vector3(10.5f, 0f, 40.5f), 90f);
            AddSection(result, root.transform, "Final20", new Vector3(25.5f, 0f, 40.5f), 90f);
            AddSection(result, root.transform, "TurnLeft_01", new Vector3(40.5f, 0f, 40.5f), 90f);
            AddSection(result, root.transform, "SafeZone_02", new Vector3(43.5f, 0f, 43.5f), 0f);
            AddSection(result, root.transform, "Final4", new Vector3(43.5f, 0f, 51f), 0f);
            AddSection(result, root.transform, "Final5", new Vector3(43.5f, 0f, 66f), 0f);
            AddSection(result, root.transform, "TurnRight_02", new Vector3(43.5f, 0f, 81f), 0f);
            AddSection(result, root.transform, "SafeZone_03", new Vector3(46.5f, 0f, 84f), 90f);
            AddSection(result, root.transform, "Final9", new Vector3(54f, 0f, 84f), 90f);
            AddSection(result, root.transform, "SectionEndAB_Test", new Vector3(69f, 0f, 84f), 90f);
            return result;
        }

        private static void AddSection(IDictionary<string, Transform> result, Transform parent, string name, Vector3 position, float yaw)
        {
            var section = new GameObject(name);
            section.transform.SetParent(parent, false);
            section.transform.localPosition = position;
            section.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            var items = new GameObject("Items");
            items.transform.SetParent(section.transform, false);
            var end = new GameObject("EndLevelPos");
            end.transform.SetParent(section.transform, false);
            result.Add(name, section.transform);
        }

        private static void BuildExactPickups(
            Transform pickupRoot,
            IReadOnlyDictionary<string, Transform> sections,
            PickupDefinitionSO rich,
            PickupDefinitionSO poor)
        {
            pickupRoot.gameObject.name = "PickupStrips";

            AddPickups(sections["Final7"], rich, "Rich",
                new Vector3(1.64f,.5f,3.44f), new Vector3(-1.82f,.5f,10.71f),
                new Vector3(1.64f,.5f,2.24f), new Vector3(1.64f,.5f,4.76f),
                new Vector3(-1.82f,.5f,9.36f), new Vector3(-1.82f,.5f,12.16f),
                new Vector3(-.10f,.5f,7.02f), new Vector3(.90f,.5f,7.02f),
                new Vector3(-1.15f,.5f,7.02f), new Vector3(-.11f,.5f,7.97f),
                new Vector3(-.04f,.5f,6.10f));

            AddPickups(sections["Final6"], poor, "Poor",
                new Vector3(-1.55f,.5f,3.35f), new Vector3(-.06f,.5f,10.76f));
            AddPickups(sections["Final6"], rich, "Rich",
                new Vector3(1.39f,.5f,3.44f), new Vector3(-1.82f,.5f,10.59f),
                new Vector3(1.39f,.5f,2.24f), new Vector3(1.39f,.5f,4.76f),
                new Vector3(-1.82f,.5f,8.97f), new Vector3(-1.82f,.5f,12.16f));

            AddPickups(sections["Final3"], rich, "Rich",
                new Vector3(1.09f,.5f,4.22f), new Vector3(-1.97f,.5f,5.09f),
                new Vector3(1.09f,.5f,5.93f), new Vector3(-1.08f,.5f,4.22f),
                new Vector3(-1.08f,.5f,5.93f), new Vector3(1.91f,.5f,5.09f),
                new Vector3(-.08f,.5f,5.09f), new Vector3(-1.97f,.5f,7.08f),
                new Vector3(1.91f,.5f,7.08f), new Vector3(-.08f,.5f,7.08f));
            AddPickups(sections["Final3"], poor, "Poor",
                new Vector3(-2.47f,.5f,10.17f), new Vector3(-1.45f,.5f,10.17f),
                new Vector3(-.48f,.5f,10.17f));

            AddPickups(sections["Final4"], poor, "Poor",
                new Vector3(-2.47f,.5f,10.17f), new Vector3(-1.45f,.5f,10.17f),
                new Vector3(-.48f,.5f,10.17f), new Vector3(.44f,.5f,5.01f),
                new Vector3(1.46f,.5f,5.01f), new Vector3(2.43f,.5f,5.01f));

            AddPickups(sections["Final5"], poor, "Poor",
                new Vector3(-1.45f,.5f,4.49f), new Vector3(-.48f,.5f,4.49f),
                new Vector3(.44f,.5f,4.55f), new Vector3(1.46f,.5f,4.55f));
            AddPickups(sections["Final5"], rich, "Rich",
                new Vector3(-1.97f,.5f,10.35f), new Vector3(1.09f,.5f,9.48f),
                new Vector3(1.09f,.5f,11.19f), new Vector3(-1.08f,.5f,9.48f),
                new Vector3(-1.08f,.5f,11.19f), new Vector3(1.91f,.5f,10.35f),
                new Vector3(-.08f,.5f,10.35f), new Vector3(-1.97f,.5f,12.34f),
                new Vector3(1.91f,.5f,12.34f), new Vector3(-.08f,.5f,12.34f),
                new Vector3(-1.97f,.5f,8.55f), new Vector3(-.08f,.5f,8.55f),
                new Vector3(1.91f,.5f,8.55f));

            AddPickups(sections["Final9"], rich, "Rich",
                new Vector3(-1.97f,.5f,6.18f), new Vector3(1.09f,.5f,5.31f),
                new Vector3(1.09f,.5f,7.02f), new Vector3(-1.08f,.5f,5.31f),
                new Vector3(-1.08f,.5f,7.02f), new Vector3(1.91f,.5f,6.18f),
                new Vector3(-.08f,.5f,6.18f), new Vector3(-1.97f,.5f,8.17f),
                new Vector3(1.91f,.5f,8.17f), new Vector3(-.08f,.5f,8.17f),
                new Vector3(-1.97f,.5f,4.38f), new Vector3(-.08f,.5f,4.38f),
                new Vector3(1.91f,.5f,4.38f), new Vector3(-1.08f,.5f,9.05f),
                new Vector3(1.91f,.5f,10.20f), new Vector3(1.09f,.5f,9.05f),
                new Vector3(-1.97f,.5f,10.20f), new Vector3(-.08f,.5f,10.20f));

            // Reference finish: money is staged BETWEEN x2/x3/x4/x5 rather than behind all gates.
            // Ten bills before x2 are especially important: a ~62-value run reaches ~82 here,
            // matching the reference capture before the first multiplier gate.
            AddPickups(sections["SectionEndAB_Test"], rich, "SpecialRich",
                // before x2 — ten collectable bills on one longitudinal sweep.
                new Vector3(-1.30f,.5f,9.40f), new Vector3(-.90f,.5f,9.70f), new Vector3(-.50f,.5f,10.00f), new Vector3(-.10f,.5f,10.30f),
                new Vector3(.30f,.5f,10.60f), new Vector3(.70f,.5f,10.90f), new Vector3(1.10f,.5f,11.20f), new Vector3(1.30f,.5f,11.50f),
                new Vector3(.90f,.5f,11.80f), new Vector3(.50f,.5f,12.10f),

                // x2 -> x3
                new Vector3(1.30f,.5f,17.40f), new Vector3(.90f,.5f,18.30f), new Vector3(.50f,.5f,19.20f), new Vector3(.10f,.5f,20.10f),
                new Vector3(-.30f,.5f,21.00f), new Vector3(-.70f,.5f,21.90f), new Vector3(-1.10f,.5f,22.80f), new Vector3(-1.30f,.5f,23.70f),
                new Vector3(-.90f,.5f,24.60f), new Vector3(-.50f,.5f,25.50f),

                // x3 -> x4
                new Vector3(-1.30f,.5f,32.40f), new Vector3(-.90f,.5f,33.30f), new Vector3(-.50f,.5f,34.20f), new Vector3(-.10f,.5f,35.10f),
                new Vector3(.30f,.5f,36.00f), new Vector3(.70f,.5f,36.90f), new Vector3(1.10f,.5f,37.80f), new Vector3(1.30f,.5f,38.70f),
                new Vector3(.90f,.5f,39.60f), new Vector3(.50f,.5f,40.50f),

                // x4 -> x5
                new Vector3(1.30f,.5f,47.00f), new Vector3(.90f,.5f,47.80f), new Vector3(.50f,.5f,48.60f), new Vector3(.10f,.5f,49.40f),
                new Vector3(-.30f,.5f,50.20f), new Vector3(-.70f,.5f,51.00f), new Vector3(-1.10f,.5f,51.80f), new Vector3(-1.30f,.5f,52.60f),
                new Vector3(-.90f,.5f,53.40f), new Vector3(-.50f,.5f,54.20f));
        }

        private static void AddPickups(Transform section, PickupDefinitionSO definition, string prefix, params Vector3[] localPositions)
        {
            var items = section.Find("Items");
            if (items == null || definition == null || definition.Prefab == null) return;
            for (var i = 0; i < localPositions.Length; i++)
            {
                var instance = PrefabUtility.InstantiatePrefab(definition.Prefab, items) as GameObject;
                if (instance == null) continue;
                instance.name = $"{prefix}_{i + 1:00}";
                instance.transform.localPosition = localPositions[i];
                instance.transform.localRotation = Quaternion.identity;
            }
        }

        private static void BuildFinal20Choice(Transform level, Transform final20, TMP_FontAsset font)
        {
            var root = new GameObject("Final20ChoiceGate");
            root.transform.SetParent(level, false);
            root.transform.SetPositionAndRotation(final20.TransformPoint(new Vector3(0f, 0f, 7.89f)), final20.rotation);
            var group = root.AddComponent<GateChoiceGroup>();
            // Reference video: this ±20 choice is the PARTY / SCHOOL gate.
            // Keep the authored ChoiceDoor mesh and gameplay values, but use the matching
            // extracted props and visible labels instead of Money/Bottle placeholder visuals.
            var bad = BuildFinal20Side(
                root.transform,
                "BadDoor",
                -1.5f,
                PartyMeshPath,
                GetReferenceMaterial(PropsFlatBadMaterialPath),
                GetReferenceMaterial(BadDoorMaterialPath),
                "ВЕЧЕРИНКА",
                font);
            var good = BuildFinal20Side(
                root.transform,
                "GoodDoor",
                1.5f,
                SchoolMeshPath,
                GetReferenceMaterial(PropsFlatMaterialPath),
                GetReferenceMaterial(GoodDoorMaterialPath),
                "ШКОЛА",
                font);
            SetObjectReference(bad.Trigger, "group", group);
            SetSerializedInt(bad.Trigger, "wealthDelta", -20);
            SetObjectReference(good.Trigger, "group", group);
            SetSerializedInt(good.Trigger, "wealthDelta", 20);
            SetObjectArray(group, "choiceColliders", new Collider[] { bad.Collider, good.Collider });

        }

        private static GateBuildResult BuildFinal20Side(
            Transform parent,
            string name,
            float x,
            string iconMeshPath,
            Material iconMaterial,
            Material frameMaterial,
            string label,
            TMP_FontAsset font)
        {
            var side = new GameObject(name);
            side.transform.SetParent(parent, false);
            side.transform.localPosition = new Vector3(x, 0f, 0f);

            // Use the extracted authored ChoiceDoor mesh. Its native X width is about 0.603;
            // x=4.45 makes each red/green half about 2.68 m wide, matching the 2.7 m trigger lane
            // and the supplied reference where the two frames nearly meet at the road centre.
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(ChoiceDoorMeshPath);
            if (mesh != null)
            {
                var visual = CreateMeshObject("DoorVisual", side.transform, mesh, frameMaterial);
                visual.transform.localPosition = new Vector3(0f, 1.20f, 0f);
                visual.transform.localRotation = Quaternion.identity;
                visual.transform.localScale = new Vector3(4.45f, 13.50f, 8.0f);
            }

            var iconMesh = AssetDatabase.LoadAssetAtPath<Mesh>(iconMeshPath);
            if (iconMesh != null)
            {
                var icon = CreateMeshObject("ChoiceIcon", side.transform, iconMesh, iconMaterial);
                icon.transform.localPosition = new Vector3(0f, 1.15f, -0.04f);
                icon.transform.localRotation = Quaternion.identity;
                FitMeshObject(icon, .92f);
            }

            var choicePanel = CreateWorldSprite(
                "ChoiceLabelPanel",
                side.transform,
                BlackPanelSpritePath,
                new Vector3(0f, 3.18f, -0.09f),
                0.25f,
                new Color(0.08f, 0.08f, 0.08f, 0.95f),
                40);
            choicePanel.transform.localScale = new Vector3(0.62f, 0.15f, 1f);

            var choiceLabel = CreateWorldText(
                "ChoiceLabel",
                side.transform,
                label,
                font,
                Color.white,
                new Vector3(0f, 3.18f, -0.12f),
                .29f,
                2.5f);
            choiceLabel.renderer.sortingOrder = 42;

            var collider = side.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.center = new Vector3(0f, 1.5f, 0f);
            collider.size = new Vector3(2.7f, 3f, .8f);
            var trigger = side.AddComponent<GateChoiceTrigger>();
            return new GateBuildResult(trigger, collider);
        }

        private static void BuildCheckpoints(Transform level, IReadOnlyDictionary<string, Transform> sections)
        {
            var root = new GameObject("Checkpoints");
            root.transform.SetParent(level, false);
            BuildCheckpoint(root.transform, "Checkpoint_01", sections["SafeZone_01"]);
            BuildCheckpoint(root.transform, "Checkpoint_02", sections["SafeZone_02"]);
            BuildCheckpoint(root.transform, "Checkpoint_03", sections["SafeZone_03"]);
        }

        private static void BuildCheckpoint(Transform parent, string name, Transform section)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.SetPositionAndRotation(section.position, section.rotation);
            var collider = root.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.center = new Vector3(0f, 3.13f, 1.08f);
            collider.size = new Vector3(6f, 8f, 2f);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0f, 6.24f);
            var material = GetReferenceMaterial(CheckpointMaterialPath)
                           ?? GetReferenceMaterial(GroundMaterialPath);
            var floorMesh = AssetDatabase.LoadAssetAtPath<Mesh>(GroundMeshPath);
            var floor = CreateMeshObject("Floor", visual.transform, floorMesh, material);
            FitReferenceMeshFootprint(floor, 6f, 2f);

            var flagMesh = AssetDatabase.LoadAssetAtPath<Mesh>(FlagMeshPath);
            var left = CreateMeshObject("FlagLeft", visual.transform, flagMesh, material);
            left.transform.localPosition = new Vector3(-2.43f, 0f, 0f);
            left.transform.localRotation = Quaternion.identity;
            left.transform.localScale = Vector3.one;
            var right = CreateMeshObject("FlagRight", visual.transform, flagMesh, material);
            right.transform.localPosition = new Vector3(2.43f, 0f, 0f);
            right.transform.localRotation = Quaternion.identity;
            right.transform.localScale = new Vector3(-1f, 1f, 1f);

            var checkpoint = root.AddComponent<CheckpointTrigger>();
            SetObjectReference(checkpoint, "leftFlagVisual", left.transform);
            SetObjectReference(checkpoint, "rightFlagVisual", right.transform);
        }

        private void BuildRoad(Transform parent, IReadOnlyList<Vector3> points)
        {
            var roadMesh = AssetDatabase.LoadAssetAtPath<Mesh>(GroundMeshPath);
            var roadMaterial = GetReferenceMaterial(GroundMaterialPath);

            if (roadMesh == null || roadMaterial == null)
            {
                Debug.LogError("RUN RICH CLEAN BUILDER: Missing reference ground mesh/material.");
                return;
            }

            _ = points;
            var index = 0;

            // Visual track width is 6 m (the authored ground mesh width).
            // Each 90-degree gameplay turn has a 3 m centerline radius, so a 6x6 plate centered
            // on the outside corner exactly covers the whole swept road width and joins the
            // incoming/outgoing straights without blue wedges or large overlaps.
            // Presentation-only extension behind the spawn. Gameplay path still begins at z=0,
            // but the reference camera sees white road below/behind the idle runner instead of water.
            BuildAuthoredRoadRun(parent, roadMesh, roadMaterial,
                new Vector3(0f, 0f, -15f), new Vector3(0f, 0f, 37.5f), ref index);
            BuildRoadCorner(parent, roadMesh, roadMaterial, new Vector3(0f, 0f, 40.5f), ref index);

            BuildAuthoredRoadRun(parent, roadMesh, roadMaterial,
                new Vector3(3f, 0f, 40.5f), new Vector3(40.5f, 0f, 40.5f), ref index);
            BuildRoadCorner(parent, roadMesh, roadMaterial, new Vector3(43.5f, 0f, 40.5f), ref index);

            BuildAuthoredRoadRun(parent, roadMesh, roadMaterial,
                new Vector3(43.5f, 0f, 43.5f), new Vector3(43.5f, 0f, 81f), ref index);
            BuildRoadCorner(parent, roadMesh, roadMaterial, new Vector3(43.5f, 0f, 84f), ref index);

            BuildAuthoredRoadRun(parent, roadMesh, roadMaterial,
                new Vector3(46.5f, 0f, 84f), new Vector3(166.5f, 0f, 84f), ref index);
        }

        private static void BuildRoadCorner(
            Transform parent,
            Mesh roadMesh,
            Material roadMaterial,
            Vector3 center,
            ref int index)
        {
            var corner = CreateMeshObject($"RoadCorner_{index++:00}", parent, roadMesh, roadMaterial);
            corner.transform.localPosition = center;
            corner.transform.localRotation = Quaternion.identity;
            // ground.asset is 6 x 7.5. Fit a square 6 x 6 plate over the entire quarter-turn.
            FitReferenceMeshFootprint(corner, 6.02f, 6.02f);
        }

        private static void BuildAuthoredRoadRun(
            Transform parent,
            Mesh roadMesh,
            Material roadMaterial,
            Vector3 start,
            Vector3 end,
            ref int index)
        {
            const float authoredLength = 7.5f;
            var delta = end - start;
            var length = delta.magnitude;
            if (length <= 0.0001f) return;

            var direction = delta / length;
            var cursor = 0f;
            while (cursor < length - 0.0001f)
            {
                var tileLength = Mathf.Min(authoredLength, length - cursor);
                var segment = CreateMeshObject($"Road_{index++:00}", parent, roadMesh, roadMaterial);
                segment.transform.localPosition = start + direction * (cursor + tileLength * 0.5f);
                segment.transform.localRotation = Quaternion.LookRotation(direction, Vector3.up);
                FitReferenceGroundToSegment(segment, tileLength + 0.02f);
                cursor += tileLength;
            }
        }

        private static void ConfigureReferencePresentation()
        {
            var groundMaterial = GetReferenceMaterial(GroundMaterialPath);
            var solidGround = AssetDatabase.LoadAssetAtPath<Texture2D>(SolidGroundTexturePath);
            if (groundMaterial != null && solidGround != null)
            {
                groundMaterial.mainTexture = solidGround;
                groundMaterial.color = Color.white;
                EditorUtility.SetDirty(groundMaterial);
            }

            var waterImporter = AssetImporter.GetAtPath(WaterTexturePath) as TextureImporter;
            if (waterImporter != null && waterImporter.wrapMode != TextureWrapMode.Repeat)
            {
                waterImporter.wrapMode = TextureWrapMode.Repeat;
                waterImporter.SaveAndReimport();
            }

            var waterMaterial = GetReferenceMaterial(WaterMaterialPath);
            var waterTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(WaterTexturePath);
            if (waterMaterial != null && waterTexture != null)
            {
                waterMaterial.SetTexture("_MainTex", waterTexture);
                waterMaterial.SetTextureScale("_MainTex", new Vector2(120f, 120f));
                waterMaterial.SetColor("_Color", Color.white);
                waterMaterial.SetFloat("_SrcBlend", (float)BlendMode.One);
                waterMaterial.SetFloat("_DstBlend", (float)BlendMode.Zero);
                waterMaterial.SetFloat("_ZWrite", 1f);
                waterMaterial.renderQueue = (int)RenderQueue.Geometry;
                waterMaterial.SetOverrideTag("RenderType", "Opaque");
                EditorUtility.SetDirty(waterMaterial);
            }

            var skybox = AssetDatabase.LoadAssetAtPath<Material>(SkyboxMaterialPath);
            var skyTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(SkyboxTexturePath);
            if (skybox != null && skyTexture != null)
            {
                skybox.SetTexture("_MainTex", skyTexture);
                skybox.SetColor("_Tint", Color.white);
                skybox.SetFloat("_Exposure", 1f);
                EditorUtility.SetDirty(skybox);
                RenderSettings.skybox = skybox;
                RenderSettings.ambientMode = AmbientMode.Skybox;
                RenderSettings.ambientIntensity = 1f;
            }

            AssetDatabase.SaveAssets();
            DynamicGI.UpdateEnvironment();
        }

        private static void CreateWorldGround(Transform parent)
        {
            var waterMesh = AssetDatabase.LoadAssetAtPath<Mesh>(WaterMeshPath);
            var waterMaterial = GetReferenceMaterial(WaterMaterialPath);

            if (waterMesh == null || waterMaterial == null)
            {
                Debug.LogError("RUN RICH CLEAN BUILDER: Missing reference Water.asset / Water.mat.");
                return;
            }

            var water = CreateMeshObject("WorldGround", parent, waterMesh, waterMaterial);
            water.transform.position = new Vector3(38f, -0.35f, 67f);
            FitReferenceMeshFootprint(water, 180f, 210f);
            var waterScale = water.transform.localScale;
            water.transform.localScale = new Vector3(waterScale.x, 0.05f, waterScale.z);
        }

        private static void EnsureRunRichRenderPipeline()
        {
            var pipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(
                "Assets/_Game/Settings/RunRich_URPAsset.asset");
            if (pipeline == null)
            {
                Debug.LogError("RUN RICH CLEAN BUILDER: RunRich_URPAsset.asset is missing.");
                return;
            }

            if (GraphicsSettings.defaultRenderPipeline != pipeline)
                GraphicsSettings.defaultRenderPipeline = pipeline;
            if (QualitySettings.renderPipeline != pipeline)
                QualitySettings.renderPipeline = pipeline;
        }

        private static void PrepareRunnerIdleWalkAnimation()
        {
            EnsureFolder(GeneratedAnimations);

            ConfigurePlayerHumanoidAvatar();
            var playerAvatar = LoadPlayerAvatar();
            if (playerAvatar == null)
            {
                Debug.LogError(
                    "RUN RICH CLEAN BUILDER: Player humanoid Avatar is invalid after import. " +
                    "Idle/Walk controller was not generated because retargeting would be unreliable.");
                return;
            }

            ConfigureLoopingHumanoidAnimation(RunnerIdleAnimationPath, playerAvatar);
            ConfigureLoopingHumanoidAnimation(RunnerWalkAnimationPath, playerAvatar);

            var idleClip = LoadPrimaryAnimationClip(RunnerIdleAnimationPath);
            var walkClip = LoadPrimaryAnimationClip(RunnerWalkAnimationPath);

            if (idleClip == null || walkClip == null)
            {
                Debug.LogError(
                    "RUN RICH CLEAN BUILDER: Idle/Walk clips could not be loaded. " +
                    $"Idle={RunnerIdleAnimationPath}, Walk={RunnerWalkAnimationPath}");
                return;
            }

            // Preserve the controller asset between rebuilds. Deleting/recreating it changed
            // its GUID on every FINALIZE COMPLETE SCENE run and broke deterministic rebuilds.
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(RunnerAnimatorControllerPath);
            if (controller == null)
                controller = AnimatorController.CreateAnimatorControllerAtPath(RunnerAnimatorControllerPath);

            controller.parameters = new[]
            {
                new AnimatorControllerParameter { name = "Running", type = AnimatorControllerParameterType.Bool },
                new AnimatorControllerParameter { name = "Rich", type = AnimatorControllerParameterType.Float }
            };

            var stateMachine = controller.layers[0].stateMachine;
            AnimatorState idle = null;
            AnimatorState walk = null;
            foreach (var child in stateMachine.states)
            {
                if (child.state == null) continue;
                if (child.state.name == "Idle") idle = child.state;
                else if (child.state.name == "Walk") walk = child.state;
            }
            if (idle == null) idle = stateMachine.AddState("Idle");
            if (walk == null) walk = stateMachine.AddState("Walk");

            foreach (var transition in idle.transitions)
                idle.RemoveTransition(transition);
            foreach (var transition in walk.transitions)
                walk.RemoveTransition(transition);

            idle.motion = idleClip;
            walk.motion = walkClip;
            idle.speed = 1f;
            walk.speed = 1f;
            stateMachine.defaultState = idle;

            var toWalk = idle.AddTransition(walk);
            toWalk.hasExitTime = false;
            toWalk.hasFixedDuration = true;
            toWalk.duration = 0.12f;
            toWalk.AddCondition(AnimatorConditionMode.If, 0f, "Running");

            var toIdle = walk.AddTransition(idle);
            toIdle.hasExitTime = false;
            toIdle.hasFixedDuration = true;
            toIdle.duration = 0.12f;
            toIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "Running");

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(RunnerAnimatorControllerPath, ImportAssetOptions.ForceUpdate);

            Debug.Log(
                $"RUN RICH CLEAN BUILDER: Created Idle/Walk animator: " +
                $"{idleClip.name} -> {walkClip.name}. Root Motion remains disabled on the player Animator.");
        }

        private static void ConfigurePlayerHumanoidAvatar()
        {
            var importer = AssetImporter.GetAtPath(PlayerModelPath) as ModelImporter;
            if (importer == null)
            {
                Debug.LogError($"RUN RICH CLEAN BUILDER: Player FBX is missing: {PlayerModelPath}");
                return;
            }

            var changed = importer.animationType != ModelImporterAnimationType.Human ||
                          importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            if (changed)
                importer.SaveAndReimport();
        }

        private static Avatar LoadPlayerAvatar()
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(PlayerModelPath))
            {
                if (asset is Avatar avatar && avatar.isValid && avatar.isHuman)
                    return avatar;
            }

            return null;
        }

        private static void ConfigureLoopingHumanoidAnimation(string assetPath, Avatar playerAvatar)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null)
            {
                Debug.LogError($"RUN RICH CLEAN BUILDER: Animation FBX is missing: {assetPath}");
                return;
            }

            if (playerAvatar == null || !playerAvatar.isValid || !playerAvatar.isHuman)
            {
                Debug.LogError($"RUN RICH CLEAN BUILDER: Cannot retarget {assetPath}; player Avatar is not a valid Humanoid.");
                return;
            }

            importer.importAnimation = true;
            importer.animationType = ModelImporterAnimationType.Human;
            // These FBXs have their own Mixamo hierarchy. Each clip needs its own
            // Humanoid Avatar; copying another clip's pose skews the left leg.
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.sourceAvatar = null;

            var clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0)
                clips = importer.defaultClipAnimations;
            if (clips != null && clips.Length > 0)
            {
                for (var i = 0; i < clips.Length; i++)
                {
                    clips[i].loopTime = true;
                    clips[i].loopPose = true;
                    clips[i].lockRootPositionXZ = true;
                    clips[i].lockRootHeightY = true;
                    clips[i].lockRootRotation = true;
                    clips[i].keepOriginalPositionXZ = false;
                    clips[i].keepOriginalPositionY = false;
                    clips[i].keepOriginalOrientation = true;
                    clips[i].heightFromFeet = true;
                }

                importer.clipAnimations = clips;
            }

            importer.SaveAndReimport();
        }

        private static AnimationClip LoadPrimaryAnimationClip(string assetPath)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (var asset in assets)
            {
                if (asset is AnimationClip clip &&
                    !clip.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase))
                    return clip;
            }

            return null;
        }

        private PlayerBuildResult CreatePlayer(Transform parent, Transform spawn, Transform road, TMP_FontAsset font)
        {
            var root = new GameObject("Player");
            root.transform.SetParent(parent, false);
            root.transform.SetPositionAndRotation(spawn.position, spawn.rotation);

            var body = root.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            var capsule = root.AddComponent<CapsuleCollider>();
            capsule.radius = 0.36f;
            capsule.height = 2.05f;
            capsule.center = new Vector3(0f, 1.025f, 0f);

            var actor = root.AddComponent<RunnerActor>();
            var appearance = root.AddComponent<PlayerAppearance>();
            var animationView = root.AddComponent<RunnerAnimationView>();

            // Original hierarchy: PlayerDefault -> ToMove -> VisualCont.
            // RunnerMotor still owns the authored path on the root; ToMove is the camera-position anchor.
            var toMove = new GameObject("ToMove");
            toMove.transform.SetParent(root.transform, false);
            SetObjectReference(actor, "cameraPositionTarget", toMove.transform);
            SetObjectReference(actor, "cameraRotationTarget", root.transform);

            var visualRoot = new GameObject("VisualCont");
            visualRoot.transform.SetParent(toMove.transform, false);
            // Road mesh top is Y=0.5 while the gameplay root follows the Y=0 path.
            visualRoot.transform.localPosition = new Vector3(0f, .47f, 0f);
            var grounding = visualRoot.AddComponent<RunnerVisualGrounding>();
            SetObjectReference(grounding, "road", road);
            SetObjectReference(actor, "steeringVisual", visualRoot.transform);

            var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerModelPath);
            GameObject visual = null;
            if (modelAsset != null)
            {
                visual = PrefabUtility.InstantiatePrefab(modelAsset, visualRoot.transform) as GameObject;
                if (visual != null)
                {
                    visual.name = "CharacterVisual";
                    visual.transform.localPosition = Vector3.zero;
                    visual.transform.localRotation = Quaternion.identity;
                    FitHierarchyToHeight(visual, 1.95f, true);
                }
            }

            var animator = visual != null ? visual.GetComponentInChildren<Animator>(true) : null;
            if (animator == null && visual != null) animator = visual.AddComponent<Animator>();
            if (animator != null)
            {
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                var playerAvatar = LoadPlayerAvatar();
                if (playerAvatar != null)
                    animator.avatar = playerAvatar;

                var runnerController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(RunnerAnimatorControllerPath);
                if (runnerController != null)
                {
                    animator.runtimeAnimatorController = runnerController;
                    animator.Rebind();
                    animator.Update(0f);
                }
                else
                    Debug.LogError($"RUN RICH CLEAN BUILDER: Runner animator controller is missing: {RunnerAnimatorControllerPath}");
            }

            var variants = visual != null
                ? visual.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                : Array.Empty<SkinnedMeshRenderer>();
            var hoboVisual = FindVariant(variants, "poor");
            var poorVisual = FindVariant(variants, "casual");
            var decentVisual = FindVariant(variants, "middle");
            var richVisual = FindVariant(variants, "bling");
            var millionaireVisual = FindVariant(variants, "cocktail");

            var playerMaterial = GetReferenceMaterial(PlayerMaterialPath);
            foreach (var variant in variants)
            {
                if (playerMaterial != null)
                    variant.sharedMaterial = playerMaterial;
                variant.gameObject.SetActive(false);
            }
            if (poorVisual != null) poorVisual.SetActive(true);

            SetObjectReference(appearance, "hoboVisual", hoboVisual);
            SetObjectReference(appearance, "poorVisual", poorVisual);
            SetObjectReference(appearance, "decentVisual", decentVisual);
            SetObjectReference(appearance, "richVisual", richVisual);
            SetObjectReference(appearance, "millionaireVisual", millionaireVisual);
            SetObjectReference(appearance, "animator", animator);
            SetObjectReference(animationView, "animator", animator);

            var feedback = CreateRunnerFeedback(root.transform, font);
            SetObjectReference(actor, "feedback", feedback);

            return new PlayerBuildResult(actor, appearance, animationView);
        }

        private static GameObject FindVariant(IEnumerable<SkinnedMeshRenderer> renderers, params string[] keywords)
        {
            foreach (var renderer in renderers)
            {
                var key = (renderer.name + " " + (renderer.sharedMesh != null ? renderer.sharedMesh.name : string.Empty)).ToLowerInvariant();
                foreach (var keyword in keywords)
                    if (key.Contains(keyword)) return renderer.gameObject;
            }

            return null;
        }

        private static RunnerFeedback CreateRunnerFeedback(Transform parent, TMP_FontAsset font)
        {
            var go = new GameObject("RunnerFeedback");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 1.05f, 0f);

            var audioSource = go.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 1f;

            var footstepSource = go.AddComponent<AudioSource>();
            footstepSource.playOnAwake = false;
            footstepSource.spatialBlend = 0f;
            footstepSource.volume = 1f;

            var billboard = new GameObject("FeedbackBillboard").transform;
            billboard.SetParent(go.transform, false);
            billboard.localPosition = Vector3.zero;

            var aura = CreateWorldSprite(
                "PositiveAura",
                billboard,
                PositiveAuraSpritePath,
                Vector3.zero,
                1f,
                new Color(0.18f, 1f, 0.28f, 0f),
                120);

            var stain = CreateWorldSprite(
                "NegativeStain",
                billboard,
                NegativeStainSpritePath,
                Vector3.zero,
                1f,
                new Color(1f, 0.12f, 0.08f, 0f),
                121);

            var sparkle = CreateWorldSprite(
                "Sparkle",
                billboard,
                SparkleSpritePath,
                Vector3.zero,
                0.7f,
                new Color(1f, 1f, 1f, 0f),
                122);

            var negativeDrops = new SpriteRenderer[7];
            for (var i = 0; i < negativeDrops.Length; i++)
            {
                negativeDrops[i] = CreateWorldSprite(
                    $"WineDrop_{i:00}",
                    billboard,
                    WineDropSpritePath,
                    Vector3.zero,
                    0.065f,
                    new Color(1f, 1f, 1f, 0f),
                    142 + i);
            }

            var negativeStars = new SpriteRenderer[5];
            for (var i = 0; i < negativeStars.Length; i++)
            {
                negativeStars[i] = CreateWorldSprite(
                    $"NegativeStars_{i:00}",
                    billboard,
                    NegativeStarsSpritePath,
                    Vector3.zero,
                    0.08f,
                    new Color(1f, 0.05f, 0.04f, 0f),
                    150 + i);
            }

            var moneySprites = new SpriteRenderer[8];
            for (var i = 0; i < moneySprites.Length; i++)
            {
                moneySprites[i] = CreateWorldSprite(
                    $"MoneyBurst_{i:00}",
                    billboard,
                    MoneyBurstSpritePath,
                    Vector3.zero,
                    0.16f,
                    new Color(1f, 1f, 1f, 0f),
                    123 + i);
            }

            var deltaText = CreateWorldText(
                "DeltaText",
                billboard,
                "+2 $",
                font,
                Color.green,
                new Vector3(0f, .28f, -.02f),
                .42f,
                2.6f);
            deltaText.renderer.sortingOrder = 150;
            deltaText.fontStyle = FontStyles.Bold;
            deltaText.outlineWidth = 0.12f;
            deltaText.outlineColor = new Color32(30, 45, 55, 180);

            var stateText = CreateWorldText(
                "StateTransitionText",
                billboard,
                "СОСТОЯТЕЛЬНЫЙ",
                font,
                new Color(0.94339621f, 0.827859402f, 0.15574935f, 1f),
                new Vector3(0f, .92f, -.03f),
                .46f,
                4.2f);
            stateText.renderer.sortingOrder = 151;
            stateText.fontStyle = FontStyles.Bold;
            stateText.outlineWidth = 0.12f;
            stateText.outlineColor = new Color32(30, 45, 55, 180);

            aura.gameObject.SetActive(false);
            stain.gameObject.SetActive(false);
            sparkle.gameObject.SetActive(false);
            foreach (var sprite in negativeDrops) sprite.gameObject.SetActive(false);
            foreach (var sprite in negativeStars) sprite.gameObject.SetActive(false);
            foreach (var sprite in moneySprites) sprite.gameObject.SetActive(false);
            deltaText.gameObject.SetActive(false);
            stateText.gameObject.SetActive(false);

            var feedback = go.AddComponent<RunnerFeedback>();
            SetObjectReference(feedback, "audioSource", audioSource);
            SetObjectReference(feedback, "footstepSource", footstepSource);
            SetObjectReference(feedback, "positiveClip", AssetDatabase.LoadAssetAtPath<AudioClip>(PositiveAudioPath));
            SetObjectReference(feedback, "negativeClip", AssetDatabase.LoadAssetAtPath<AudioClip>(NegativeAudioPath));
            SetObjectReference(feedback, "choicePositiveClip", AssetDatabase.LoadAssetAtPath<AudioClip>(ChoicePositiveAudioPath));
            SetObjectReference(feedback, "checkpointClip", AssetDatabase.LoadAssetAtPath<AudioClip>(CheckpointAudioPath));
            SetObjectReference(feedback, "winClip", AssetDatabase.LoadAssetAtPath<AudioClip>(WinAudioPath));
            SetObjectReference(feedback, "multiplierLowClip", AssetDatabase.LoadAssetAtPath<AudioClip>(MultiplierLowAudioPath));
            SetObjectReference(feedback, "multiplierMidClip", AssetDatabase.LoadAssetAtPath<AudioClip>(MultiplierMidAudioPath));
            SetObjectReference(feedback, "multiplierHighClip", AssetDatabase.LoadAssetAtPath<AudioClip>(MultiplierHighAudioPath));
            SetObjectArray(feedback, "footstepClips", LoadAudioClips(FootstepAudioPaths));
            SetObjectArray(feedback, "heelClips", LoadAudioClips(HeelAudioPaths));
            SetObjectReference(feedback, "billboardRoot", billboard);
            SetObjectReference(feedback, "positiveAura", aura);
            SetObjectReference(feedback, "negativeStain", stain);
            SetObjectReference(feedback, "sparkle", sparkle);
            SetObjectArray(feedback, "moneyBursts", moneySprites);
            SetObjectArray(feedback, "negativeShards", negativeDrops);
            SetObjectArray(feedback, "negativeStars", negativeStars);
            SetObjectReference(feedback, "deltaText", deltaText);
            SetObjectReference(feedback, "stateText", stateText);
            return feedback;
        }

        private static AudioClip[] LoadAudioClips(string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return Array.Empty<AudioClip>();

            var clips = new List<AudioClip>(paths.Length);
            foreach (var path in paths)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip != null) clips.Add(clip);
            }
            return clips.ToArray();
        }

        private static Sprite LoadSpriteAsset(string assetPath)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite != null) return sprite;

            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(assetPath))
                if (asset is Sprite candidate)
                    return candidate;

            Debug.LogWarning($"RUN RICH CLEAN BUILDER: Sprite not found at {assetPath}");
            return null;
        }

        private static SpriteRenderer CreateWorldSprite(
            string name,
            Transform parent,
            string spritePath,
            Vector3 localPosition,
            float uniformScale,
            Color color,
            int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one * uniformScale;

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = LoadSpriteAsset(spritePath);
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private static PickupStrip[] BuildPickupStrips(Transform parent, PickupDefinitionSO money, PickupDefinitionSO bottle)
        {
            var strips = new List<PickupStrip>();

            strips.Add(CreateStrip(
                parent,
                "OpeningStrip",
                new Vector3(0f, 0f, 8f),
                0f,
                money,
                bottle,
                new[]
                {
                    Row(null, money, money, null),
                    Row(money, money, money, money),
                    Row(null, money, money, null),
                    Row(money, money, money, money),
                    Row(null, money, money, null)
                }));

            strips.Add(CreateStrip(
                parent,
                "BottleStrip",
                new Vector3(-0.5f, 0f, 28f),
                -5f,
                money,
                bottle,
                new[]
                {
                    Row(money, null, null, money),
                    Row(null, bottle, money, null),
                    Row(money, null, null, money)
                }));

            strips.Add(CreateStrip(
                parent,
                "BeforeGateStrip",
                new Vector3(-3.0f, 0f, 40f),
                -8f,
                money,
                bottle,
                new[]
                {
                    Row(null, money, money, null),
                    Row(money, null, null, money),
                    Row(null, money, money, null)
                }));

            strips.Add(CreateStrip(
                parent,
                "AfterGateStrip",
                new Vector3(-3.5f, 0f, 63f),
                3f,
                money,
                bottle,
                new[]
                {
                    Row(money, money, money, money),
                    Row(null, money, money, null),
                    Row(money, money, money, money),
                    Row(null, money, money, null),
                    Row(money, money, money, money),
                    Row(null, money, money, null),
                    Row(money, money, money, money),
                    Row(null, money, money, null)
                }));

            return strips.ToArray();
        }

        private static PickupStrip CreateStrip(
            Transform parent,
            string name,
            Vector3 position,
            float yaw,
            PickupDefinitionSO money,
            PickupDefinitionSO bottle,
            PickupStrip.PickupRow[] rows)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            var strip = go.AddComponent<PickupStrip>();
            strip.Configure(1.8f, 1.3f, 0.25f, rows);
            return strip;
        }

        private static PickupStrip.PickupRow Row(
            PickupDefinitionSO s0,
            PickupDefinitionSO s1,
            PickupDefinitionSO s2,
            PickupDefinitionSO s3)
        {
            return new PickupStrip.PickupRow(s0, s1, s2, s3);
        }

        private static void BuildObstacles(Transform parent)
        {
            var root = new GameObject("Obstacles");
            root.transform.SetParent(parent, false);

            CreateObstacle(root.transform, "TrashObstacle_01", TrashMeshAPath, new Vector3(1.35f, 0f, 22f), 18f, -20);
            CreateObstacle(root.transform, "TrashObstacle_02", TrashMeshBPath, new Vector3(-4.7f, 0f, 74f), -12f, -25);
            CreateObstacle(root.transform, "TrashObstacle_03", TrashMeshAPath, new Vector3(-0.7f, 0f, 96f), 24f, -30);
        }

        private static void CreateObstacle(
            Transform parent,
            string name,
            string meshPath,
            Vector3 position,
            float yaw,
            int wealthDelta)
        {
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (mesh == null) return;

            var obstacle = CreateMeshObject(name, parent, mesh, GetReferenceMaterial(BadItemMaterialPath));
            obstacle.transform.localPosition = position;
            obstacle.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            FitMeshObject(obstacle, 1.25f);

            var trigger = obstacle.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 0.55f, 0f);
            trigger.size = new Vector3(1.15f, 1.1f, 1.15f);
            var obstacleTrigger = obstacle.AddComponent<ObstacleTrigger>();
            SetSerializedInt(obstacleTrigger, "wealthDelta", wealthDelta);
        }

        private static Transform BuildGate(Transform parent, TMP_FontAsset font, Vector3 position, Quaternion rotation)
        {
            var root = new GameObject("SchoolPartyGate");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;
            root.transform.localRotation = rotation;
            var group = root.AddComponent<GateChoiceGroup>();

            var party = BuildGateSide(
                root.transform,
                "Party",
                -1.3f,
                "ВЕЧЕРИНКА",
                -20,
                GetReferenceMaterial(PropsFlatBadMaterialPath),
                AssetDatabase.LoadAssetAtPath<Mesh>(PartyMeshPath),
                font);
            var school = BuildGateSide(
                root.transform,
                "School",
                1.3f,
                "ШКОЛА",
                20,
                GetReferenceMaterial(PropsFlatMaterialPath),
                AssetDatabase.LoadAssetAtPath<Mesh>(SchoolMeshPath),
                font);

            SetObjectReference(party.Trigger, "group", group);
            SetSerializedInt(party.Trigger, "wealthDelta", -20);
            SetObjectReference(school.Trigger, "group", group);
            SetSerializedInt(school.Trigger, "wealthDelta", 20);
            SetObjectArray(group, "choiceColliders", new Collider[] { party.Collider, school.Collider });
            return root.transform;
        }

        private static GateBuildResult BuildGateSide(
            Transform parent,
            string name,
            float x,
            string label,
            int wealthDelta,
            Material material,
            Mesh iconMesh,
            TMP_FontAsset font)
        {
            var side = new GameObject(name);
            side.transform.SetParent(parent, false);
            side.transform.localPosition = new Vector3(x, 0f, 0f);

            CreateFramePiece(side.transform, "LeftPost", new Vector3(-0.72f, 1.15f, 0f), new Vector3(0.16f, 2.3f, 0.18f), material);
            CreateFramePiece(side.transform, "RightPost", new Vector3(0.72f, 1.15f, 0f), new Vector3(0.16f, 2.3f, 0.18f), material);
            CreateFramePiece(side.transform, "TopBar", new Vector3(0f, 2.22f, 0f), new Vector3(1.6f, 0.20f, 0.18f), material);

            if (iconMesh != null)
            {
                var icon = CreateMeshObject("Icon", side.transform, iconMesh, material);
                icon.transform.localPosition = new Vector3(0f, 1.45f, 0.02f);
                FitMeshObject(icon, 0.95f);
            }

            CreateWorldText("Label", side.transform, label, font, Color.white, new Vector3(0f, 2.55f, -0.03f), 0.23f, 1.65f);

            var collider = side.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.center = new Vector3(0f, 1.1f, 0f);
            collider.size = new Vector3(1.45f, 2.2f, 0.7f);
            var trigger = side.AddComponent<GateChoiceTrigger>();
            return new GateBuildResult(trigger, collider);
        }

        private static void CreateFramePiece(Transform parent, string name, Vector3 localPosition, Vector3 scale, Material material)
        {
            var piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.name = name;
            piece.transform.SetParent(parent, false);
            piece.transform.localPosition = localPosition;
            piece.transform.localScale = scale;
            var collider = piece.GetComponent<Collider>();
            if (collider != null) DestroyImmediate(collider);
            SetRendererMaterial(piece, material);
        }

        private static Transform BuildFinish(Transform parent, TMP_FontAsset font)
        {
            var root = new GameObject("Finish");
            root.transform.SetParent(parent, false);

            var finishLineMesh = AssetDatabase.LoadAssetAtPath<Mesh>(FinishLineMeshPath);
            var finishLineMaterial = GetReferenceMaterial(FinishLineMaterialPath);
            if (finishLineMesh != null && finishLineMaterial != null)
            {
                var finishLine = CreateMeshObject("FinishLine", root.transform, finishLineMesh, finishLineMaterial);
                finishLine.transform.localPosition = new Vector3(0f, .04f, 8.5f);
                finishLine.transform.localRotation = Quaternion.identity;
                FitReferenceMeshFootprint(finishLine, 5f, .5f);
            }

            var x2Visual = BuildDoorVisual(root.transform, "Door_POOR", 15.14f, WealthState.Poor, font, "x2");
            var x3Visual = BuildDoorVisual(root.transform, "Door_DECENT", 30.14f, WealthState.Decent, font, "x3");
            var x4Visual = BuildDoorVisual(root.transform, "Door_RICH", 45.14f, WealthState.Rich, font, "x4");
            var x5Visual = BuildDoorVisual(root.transform, "Door_MILLIONAIRE", 60.14f, WealthState.Millionaire, font, "x5");

            BuildFinishReferenceEnvironment(root.transform);

            BuildStatusZone(root.transform, "DOOR_01", 12.63f, WealthState.Poor, 2, false, x2Visual);
            BuildStatusZone(root.transform, "DOOR_02", 27.77f, WealthState.Decent, 3, false, x3Visual);
            BuildStatusZone(root.transform, "DOOR_03", 42.75f, WealthState.Rich, 4, false, x4Visual);
            BuildStatusZone(root.transform, "DOOR_04", 55.30f, WealthState.Millionaire, 5, false, x5Visual);
            BuildStatusZone(root.transform, "FinalTrigger", 64.80f, WealthState.Hobo, 5, true, null);

            return root.transform;
        }

        private static void BuildFinishReferenceEnvironment(Transform parent)
        {
            var propsMaterial = GetReferenceMaterial(PropsMaterialPath);
            if (propsMaterial == null) return;

            // Reference finish presentation becomes progressively cleaner/richer. Use only
            // extracted meshes already present in the package; these objects have no colliders
            // and therefore cannot alter finish gameplay.
            var trashA = AssetDatabase.LoadAssetAtPath<Mesh>(TrashMeshAPath);
            var trashB = AssetDatabase.LoadAssetAtPath<Mesh>(TrashMeshBPath);
            if (trashA != null)
            {
                var item = CreateMeshObject("FinishPoorTrash_L", parent, trashA, propsMaterial);
                item.transform.localPosition = new Vector3(-2.12f, 0f, 14.25f);
                item.transform.localRotation = Quaternion.Euler(0f, 18f, 0f);
                FitMeshObject(item, 0.92f);
            }
            if (trashB != null)
            {
                var item = CreateMeshObject("FinishPoorTrash_R", parent, trashB, propsMaterial);
                item.transform.localPosition = new Vector3(2.12f, 0f, 14.25f);
                item.transform.localRotation = Quaternion.Euler(0f, -22f, 0f);
                FitMeshObject(item, 0.96f);
            }

            var bench = AssetDatabase.LoadAssetAtPath<Mesh>(BenchMeshPath);
            if (bench != null)
            {
                var left = CreateMeshObject("FinishDecentBench_L", parent, bench, propsMaterial);
                left.transform.localPosition = new Vector3(-2.20f, 0f, 29.20f);
                left.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                FitMeshObject(left, 1.80f);
                var right = CreateMeshObject("FinishDecentBench_R", parent, bench, propsMaterial);
                right.transform.localPosition = new Vector3(2.20f, 0f, 29.20f);
                right.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
                FitMeshObject(right, 1.80f);
            }

            var plant = AssetDatabase.LoadAssetAtPath<Mesh>(PlantMeshPath);
            if (plant != null)
            {
                var left = CreateMeshObject("FinishRichPlant_L", parent, plant, propsMaterial);
                left.transform.localPosition = new Vector3(-2.15f, 0f, 44.35f);
                FitMeshObject(left, 1.35f);
                var right = CreateMeshObject("FinishRichPlant_R", parent, plant, propsMaterial);
                right.transform.localPosition = new Vector3(2.15f, 0f, 44.35f);
                FitMeshObject(right, 1.35f);
            }
        }

        private static void BuildStatusZone(
            Transform parent,
            string name,
            float z,
            WealthState state,
            int multiplier,
            bool millionaire,
            GateOpenVisual gateVisual)
        {
            var zone = new GameObject(name);
            zone.transform.SetParent(parent, false);
            zone.transform.localPosition = new Vector3(0f, 0f, z);
            var collider = zone.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.center = new Vector3(0f, 2.5f, 0f);
            collider.size = new Vector3(10f, 5f, 1f);
            var trigger = zone.AddComponent<StatusDoorTrigger>();
            SetSerializedInt(trigger, "minimumState", (int)state);
            SetSerializedInt(trigger, "multiplier", multiplier);
            SetSerializedBool(trigger, "millionaire", millionaire);
            SetSerializedBool(trigger, "moveActive", !millionaire);
            SetObjectReference(trigger, "gateVisual", gateVisual);
        }

        private static GateOpenVisual BuildDoorVisual(
            Transform parent,
            string name,
            float z,
            WealthState state,
            TMP_FontAsset font,
            string label)
        {
            var door = new GameObject(name);
            door.transform.SetParent(parent, false);
            door.transform.localPosition = new Vector3(0f, 0f, z);

            var visualRoot = new GameObject("Visual");
            visualRoot.transform.SetParent(door.transform, false);
            visualRoot.transform.localScale = Vector3.one * 2.8f;

            Transform leftLeaf = null;
            Transform rightLeaf = null;
            var builtAny = false;

            if (state == WealthState.Millionaire)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(DoorMillionPrefabPath);
                if (prefab != null)
                {
                    var instance = PrefabUtility.InstantiatePrefab(prefab, visualRoot.transform) as GameObject;
                    if (instance != null)
                    {
                        instance.name = "Door_Million";
                        instance.transform.localPosition = Vector3.zero;
                        instance.transform.localRotation = Quaternion.identity;
                        instance.transform.localScale = Vector3.one;
                        leftLeaf = FindDeepTransform(instance.transform, "Door_Million_00");
                        rightLeaf = FindDeepTransform(instance.transform, "Door_Million_00.001");
                        builtAny = true;
                    }
                }
            }
            else
            {
                var doorMaterial = GetReferenceMaterial(DoorMaterialPath);
                var meshPaths = GetReferenceDoorMeshPaths(state);
                var parts = new List<Transform>();
                for (var i = 0; i < meshPaths.Length; i++)
                {
                    var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPaths[i]);
                    if (mesh == null) continue;

                    var part = CreateMeshObject(Path.GetFileNameWithoutExtension(meshPaths[i]), visualRoot.transform, mesh, doorMaterial);
                    part.transform.localPosition = Vector3.zero;
                    part.transform.localRotation = Quaternion.identity;
                    part.transform.localScale = Vector3.one;
                    parts.Add(part.transform);
                    builtAny = true;
                }

                if (state == WealthState.Poor && parts.Count >= 3)
                {
                    rightLeaf = parts[1];
                    leftLeaf = parts[2];
                }
                else if (state == WealthState.Decent && parts.Count >= 2)
                {
                    leftLeaf = parts[0];
                    rightLeaf = parts[1];
                }
                else if (state == WealthState.Rich && parts.Count >= 3)
                {
                    leftLeaf = parts[1];
                    rightLeaf = parts[2];
                }
            }

            if (!builtAny)
            {
                var fallbackMesh = AssetDatabase.LoadAssetAtPath<Mesh>(ChoiceDoorMeshPath);
                var doorMaterial = GetReferenceMaterial(DoorMaterialPath);
                if (fallbackMesh != null)
                {
                    var visual = CreateMeshObject("VisualFallback", visualRoot.transform, fallbackMesh, doorMaterial);
                    FitMeshObject(visual, 3.2f);
                }
            }

            OrderDoorLeavesByVisualX(visualRoot.transform, ref leftLeaf, ref rightLeaf);

            var gateOpen = door.AddComponent<GateOpenVisual>();
            SetObjectReference(gateOpen, "leftLeaf", leftLeaf);
            SetObjectReference(gateOpen, "rightLeaf", rightLeaf);
            SetSerializedFloat(gateOpen, "openDistance", .46f);
            SetSerializedFloat(gateOpen, "duration", .32f);

            if (!string.IsNullOrEmpty(label))
            {
                var sign = CreateWorldSprite(
                    "MultiplierSign",
                    door.transform,
                    MultiplierSignSpritePath(label),
                    new Vector3(0f, 5.48f, -0.20f),
                    0.24f,
                    Color.white,
                    50);
                sign.transform.localScale = new Vector3(0.52f, 0.34f, 1f);

                var multiplierLabel = CreateWorldText(
                    "Multiplier",
                    door.transform,
                    label,
                    font,
                    Color.white,
                    new Vector3(0f, 5.52f, -0.25f),
                    .42f,
                    1.8f);
                multiplierLabel.renderer.sortingOrder = 52;
                multiplierLabel.fontStyle = FontStyles.Bold;
            }

            return gateOpen;
        }

        private static void OrderDoorLeavesByVisualX(Transform visualRoot, ref Transform leftLeaf, ref Transform rightLeaf)
        {
            if (visualRoot == null || leftLeaf == null || rightLeaf == null || leftLeaf == rightLeaf)
                return;

            var leftRenderer = leftLeaf.GetComponentInChildren<Renderer>(true);
            var rightRenderer = rightLeaf.GetComponentInChildren<Renderer>(true);
            var leftWorld = leftRenderer != null ? leftRenderer.bounds.center : leftLeaf.position;
            var rightWorld = rightRenderer != null ? rightRenderer.bounds.center : rightLeaf.position;
            var leftX = visualRoot.InverseTransformPoint(leftWorld).x;
            var rightX = visualRoot.InverseTransformPoint(rightWorld).x;
            if (leftX <= rightX)
                return;

            var temp = leftLeaf;
            leftLeaf = rightLeaf;
            rightLeaf = temp;
        }

        private static Transform FindDeepTransform(Transform root, string targetName)
        {
            if (root == null) return null;
            if (string.Equals(root.name, targetName, StringComparison.Ordinal)) return root;
            for (var i = 0; i < root.childCount; i++)
            {
                var found = FindDeepTransform(root.GetChild(i), targetName);
                if (found != null) return found;
            }

            return null;
        }

        private static string MultiplierSignSpritePath(string label)
        {
            return label switch
            {
                "x2" => OrangePanelSpritePath,
                "x3" => YellowPanelSpritePath,
                "x4" => GreenPanelSpritePath,
                "x5" => BluePanelSpritePath,
                _ => BluePanelSpritePath
            };
        }

        private static string[] GetReferenceDoorMeshPaths(WealthState state)
        {
            switch (state)
            {
                case WealthState.Poor:
                    return new[] { DoorPoor0Path, DoorPoor1Path, DoorPoor2Path };
                case WealthState.Decent:
                    return new[] { DoorDecent0Path, DoorDecent1Path, DoorDecent2Path };
                case WealthState.Rich:
                    return new[] { DoorRich0Path, DoorRich1Path, DoorRich2Path };
                case WealthState.Millionaire:
                    return new[] { DoorMillionPath };
                default:
                    return Array.Empty<string>();
            }
        }

        private static void BuildFinishStep(
            Transform parent,
            int multiplier,
            float z,
            Mesh suppliedMesh,
            Material material,
            TMP_FontAsset font,
            bool completesRun)
        {
            var step = new GameObject($"x{multiplier}");
            step.transform.SetParent(parent, false);
            step.transform.localPosition = new Vector3(0f, 0f, z);

            var platformMesh = AssetDatabase.LoadAssetAtPath<Mesh>(GroundMeshPath);
            if (platformMesh != null)
            {
                var platform = CreateMeshObject("Platform", step.transform, platformMesh, material);
                platform.transform.localPosition = new Vector3(0f, -0.28f, 0f);
                FitReferenceMeshFootprint(platform, 5.1f, 4.4f);
            }

            if (suppliedMesh != null)
            {
                var visual = CreateMeshObject("EndLevelVisual", step.transform, suppliedMesh, material);
                visual.transform.localPosition = new Vector3(0f, 0.15f, 0f);
                FitMeshObject(visual, 3.6f);
            }

            CreateWorldText("MultiplierLabel", step.transform, $"x{multiplier}", font, Color.white, new Vector3(0f, 0.42f, -0.8f), 0.34f, 1.4f);

            var trigger = step.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 1f, 0f);
            trigger.size = new Vector3(5.1f, 2f, 0.8f);
            var finishTrigger = step.AddComponent<FinishStepTrigger>();
            SetSerializedInt(finishTrigger, "multiplier", multiplier);
            SetSerializedBool(finishTrigger, "completesRun", completesRun);
        }

        private static Camera CreateCamera()
        {
            var camCont = new GameObject("CamCont");
            var distCam = new GameObject("DistCam");
            distCam.transform.SetParent(camCont.transform, false);
            var go = new GameObject("Main Camera");
            go.transform.SetParent(distCam.transform, false);
            go.tag = "MainCamera";
            go.transform.localPosition = new Vector3(0f, 4.5f, -8f);
            go.transform.localRotation = Quaternion.Euler(17f, 0f, 0f);
            var camera = go.AddComponent<Camera>();
            camera.fieldOfView = 60f;
            camera.nearClipPlane = .3f;
            camera.farClipPlane = 500f;
            go.AddComponent<AudioListener>();
            return camera;
        }

        private static void CreateDirectionalLight()
        {
            var go = new GameObject("Directional Light");
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.shadows = LightShadows.Soft;
        }

        private static void CreateGlobalVolume()
        {
            var go = new GameObject("Global Volume");
            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            const string profilePath = GeneratedRoot + "/Level01Volume.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            if (profile == null)
            {
                profile = CreateInstance<VolumeProfile>();
                profile.name = "Level01Volume";
                AssetDatabase.CreateAsset(profile, profilePath);
            }
            volume.sharedProfile = profile;
        }

        private static GameObject CreateUi(TMP_FontAsset font, out GameplayHud hud)
        {
            var canvasGo = new GameObject("UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(880f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            hud = canvasGo.AddComponent<GameplayHud>();

            var safeArea = CreateRect("SafeArea", canvasGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var safeAreaFitter = safeArea.gameObject.AddComponent<SafeAreaFitter>();
            SetObjectReference(safeAreaFitter, "target", safeArea);

            // ----------------------------------------------------------
            // Persistent HUD — matches the reference video:
            // settings/exit top-left, wallet/key top-right, level + wealth top-center.
            // ----------------------------------------------------------
            var persistent = CreateRect("PersistentHUD", safeArea.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var readySettingsIcon = CreateSpriteImage(
                "SettingsButton",
                persistent.transform,
                SettingsSpritePath,
                new Vector2(0f, 1f),
                new Vector2(72f, 72f),
                new Vector2(49f, -49f),
                Color.white);

            var runExitIcon = CreateSpriteImage(
                "ExitButton",
                persistent.transform,
                ExitSpritePath,
                new Vector2(0f, 1f),
                new Vector2(72f, 72f),
                new Vector2(49f, -49f),
                Color.white);
            runExitIcon.gameObject.SetActive(false);

            var walletPanel = CreateSlicedSpriteImage(
                "WalletPanel",
                persistent.transform,
                BlackPanelSpritePath,
                new Vector2(1f, 1f),
                new Vector2(178f, 62f),
                new Vector2(-112f, -53f),
                Color.white);
            walletPanel.raycastTarget = false;

            var walletValue = CreateTmpText(
                "WalletValue",
                walletPanel.transform,
                "0",
                font,
                30f,
                TextAlignmentOptions.Center,
                new Vector2(0.39f, 0.5f),
                new Vector2(94f, 52f),
                new Vector2(-10f, 1f));
            walletValue.color = Color.white;
            ConfigureUiText(walletValue, 0.12f);

            CreateSpriteImage(
                "WalletIcon",
                walletPanel.transform,
                DollarSpritePath,
                new Vector2(0.83f, 0.5f),
                new Vector2(58f, 38f),
                new Vector2(-2f, 0f),
                Color.white);

            CreateSpriteImage(
                "KeyIcon",
                persistent.transform,
                KeySpritePath,
                new Vector2(1f, 1f),
                new Vector2(34f, 34f),
                new Vector2(-155f, -103f),
                Color.white);

            var keyValue = CreateTmpText(
                "KeyValue",
                persistent.transform,
                "0/3",
                font,
                24f,
                TextAlignmentOptions.Left,
                new Vector2(1f, 1f),
                new Vector2(72f, 36f),
                new Vector2(-95f, -103f));
            keyValue.color = Color.white;
            ConfigureUiText(keyValue, 0.12f);

            var levelText = CreateTmpText(
                "LevelText",
                persistent.transform,
                "Уровень 1",
                font,
                31f,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f),
                new Vector2(500f, 88f),
                new Vector2(0f, -72f));
            levelText.color = Color.white;
            ConfigureUiText(levelText, 0.10f);

            var wealthText = CreateTmpText(
                "WealthValue",
                persistent.transform,
                "40",
                font,
                56f,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f),
                new Vector2(340f, 76f),
                new Vector2(0f, -127f));
            wealthText.color = Color.white;
            ConfigureUiText(wealthText, 0.12f);

            // ----------------------------------------------------------
            // READY / tutorial.
            // ----------------------------------------------------------
            var ready = CreateRect("ReadyPanel", safeArea.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var readyProgress = CreateSlicedSpriteImage(
                "ReadyLevelProgress",
                ready.transform,
                BlackTransparentPanelSpritePath,
                new Vector2(0.5f, 1f),
                new Vector2(620f, 136f),
                new Vector2(0f, -178f),
                new Color(1f, 1f, 1f, 0.95f));
            readyProgress.raycastTarget = false;

            CreateSpriteImage(
                "LevelPortraitStart",
                readyProgress.transform,
                LevelReferenceThumbPath,
                new Vector2(0f, 0.5f),
                new Vector2(104f, 104f),
                new Vector2(58f, 0f),
                Color.white);

            CreateSpriteImage(
                "LevelPortraitEnd",
                readyProgress.transform,
                LevelReferenceThumbPath,
                new Vector2(1f, 0.5f),
                new Vector2(104f, 104f),
                new Vector2(-58f, 0f),
                Color.white);

            // The reference has five white progress cells above the numbered circles.
            for (var i = 0; i < 5; i++)
            {
                var x = -164f + i * 82f;
                var segment = CreateImage(
                    $"LevelSegment_{i + 1}",
                    readyProgress.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(76f, 28f),
                    new Vector2(x, 23f),
                    new Color(0.96f, 0.97f, 0.98f, 1f));
                segment.raycastTarget = false;

                var bubble = CreateImage(
                    $"LevelDot_{i + 1}",
                    readyProgress.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(46f, 46f),
                    new Vector2(x, -30f),
                    Color.white);
                bubble.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CircleSpritePath);
                bubble.type = Image.Type.Simple;
                bubble.preserveAspect = true;
                bubble.raycastTarget = false;

                var dotText = CreateTmpText(
                    "Number",
                    bubble.transform,
                    (i + 1).ToString(),
                    font,
                    22f,
                    TextAlignmentOptions.Center,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(38f, 38f),
                    Vector2.zero);
                dotText.color = new Color(0.08f, 0.10f, 0.12f, 1f);
            }

            var tutorialBubble = CreateSlicedSpriteImage(
                "TutorialBubble",
                ready.transform,
                BlackTransparentPanelSpritePath,
                new Vector2(0.5f, 0f),
                new Vector2(540f, 126f),
                new Vector2(0f, 535f),
                new Color(1f, 1f, 1f, 0.95f));
            tutorialBubble.raycastTarget = false;

            var tutorialText = CreateTmpText(
                "TutorialText",
                tutorialBubble.transform,
                "ПРОВЕДИТЕ ПО ЭКРАНУ, ЧТОБЫ\nПОВЕРНУТЬ",
                font,
                22f,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f),
                new Vector2(460f, 90f),
                Vector2.zero);
            tutorialText.color = Color.white;
            ConfigureUiText(tutorialText, 0.08f);

            CreateSpriteImage(
                "SwipeArrow",
                ready.transform,
                GestureArrowSpritePath,
                new Vector2(0.5f, 0f),
                new Vector2(500f, 112f),
                new Vector2(0f, 365f),
                Color.white);

            var tutorialFinger = CreateSpriteImage(
                "Finger",
                ready.transform,
                GestureFingerSpritePath,
                new Vector2(0.5f, 0f),
                new Vector2(100f, 132f),
                new Vector2(0f, 315f),
                Color.white);

            var swipeAnimator = ready.gameObject.AddComponent<TutorialSwipeAnimator>();
            SetObjectReference(swipeAnimator, "finger", tutorialFinger.rectTransform);

            // ----------------------------------------------------------
            // PLAYING — only player-follow status is needed; the rest lives in PersistentHUD.
            // ----------------------------------------------------------
            var gameplay = CreateRect("GameplayPanel", safeArea.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var runnerStatus = CreateRect(
                "RunnerStatus",
                gameplay.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(300f, 96f),
                Vector2.zero);

            var runnerStatusView = runnerStatus.gameObject.AddComponent<RunnerStatusView>();

            var stateText = CreateTmpText(
                "StateText",
                runnerStatus.transform,
                "БЕДНЫЙ",
                font,
                23f,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f),
                new Vector2(292f, 44f),
                new Vector2(0f, 22f));
            stateText.color = new Color(1f, 0.423569858f, 0f, 1f);
            ConfigureUiText(stateText, 0.10f);

            var gaugeBg = CreateSpriteImage(
                "GaugeBackground",
                runnerStatus.transform,
                GaugeEmptySpritePath,
                new Vector2(0.5f, 0.5f),
                new Vector2(235f, 38f),
                new Vector2(0f, -16f),
                Color.white);

            var gaugeFill = CreateSpriteImage(
                "GaugeFill",
                gaugeBg.transform,
                GaugeFullSpritePath,
                new Vector2(0.5f, 0.5f),
                new Vector2(235f, 38f),
                Vector2.zero,
                new Color(1f, 0.423569858f, 0f, 1f));
            gaugeFill.type = Image.Type.Filled;
            gaugeFill.fillMethod = Image.FillMethod.Horizontal;
            gaugeFill.fillOrigin = 0;
            gaugeFill.fillAmount = 40f / 140f;
            gaugeFill.preserveAspect = false;

            SetObjectReference(runnerStatusView, "stateText", stateText);
            SetObjectReference(runnerStatusView, "gaugeFill", gaugeFill);

            // ----------------------------------------------------------
            // WIN — reference-like result screen.
            // ----------------------------------------------------------
            var winPanel = CreateRect("WinPanel", safeArea.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;

            var winRaysImage = CreateSpriteImage(
                "WinRays",
                winPanel.transform,
                WinRaysSpritePath,
                new Vector2(0.5f, 0.56f),
                new Vector2(780f, 780f),
                Vector2.zero,
                new Color(1f, 1f, 1f, 0.20f));
            winRaysImage.raycastTarget = false;

            var winMoneyFx = new RectTransform[8];
            var winMoneyPositions = new[]
            {
                new Vector2(-280f, 255f), new Vector2(265f, 235f),
                new Vector2(-330f, 40f),  new Vector2(325f, 20f),
                new Vector2(-235f, -180f), new Vector2(250f, -205f),
                new Vector2(-95f, 325f), new Vector2(105f, 330f)
            };
            for (var i = 0; i < winMoneyFx.Length; i++)
            {
                var moneyImage = CreateSpriteImage(
                    $"WinMoney_{i:00}",
                    winPanel.transform,
                    WinMoneySpritePath,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(118f, 76f),
                    winMoneyPositions[i],
                    new Color(1f, 1f, 1f, 0.96f));
                moneyImage.raycastTarget = false;
                winMoneyFx[i] = moneyImage.rectTransform;
            }

            var winTitle = CreateTmpText(
                "WinTitle",
                winPanel.transform,
                "ВЫ ПОБЕДИЛИ",
                font,
                61f,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f),
                new Vector2(820f, 120f),
                new Vector2(0f, 180f));
            winTitle.color = Color.white;
            ConfigureUiText(winTitle, 0.22f);
            winTitle.outlineColor = new Color32(20, 125, 210, 230);

            CreateSpriteImage(
                "MultiplierGauge",
                winPanel.transform,
                CircleGaugeSpritePath,
                new Vector2(0.5f, 0f),
                new Vector2(430f, 250f),
                new Vector2(0f, 430f),
                Color.white);

            var winScore = CreateTmpText(
                "WinScoreText",
                winPanel.transform,
                string.Empty,
                font,
                40f,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0f),
                new Vector2(130f, 60f),
                new Vector2(0f, 405f));
            winScore.color = Color.clear;
            winScore.gameObject.SetActive(false);

            var winPointer = CreateSpriteImage(
                "MultiplierPointer",
                winPanel.transform,
                WinPointerSpritePath,
                new Vector2(0.5f, 0f),
                new Vector2(44f, 58f),
                new Vector2(-135f, 548f),
                Color.white);
            winPointer.raycastTarget = false;

            CreateTmpText(
                "GaugeX2",
                winPanel.transform,
                "x2",
                font,
                23f,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0f),
                new Vector2(70f, 45f),
                new Vector2(-135f, 465f)).color = Color.white;
            CreateTmpText(
                "GaugeX3",
                winPanel.transform,
                "x3",
                font,
                23f,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0f),
                new Vector2(70f, 45f),
                new Vector2(-45f, 515f)).color = Color.white;
            CreateTmpText(
                "GaugeX4",
                winPanel.transform,
                "x4",
                font,
                23f,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0f),
                new Vector2(70f, 45f),
                new Vector2(48f, 515f)).color = Color.white;
            CreateTmpText(
                "GaugeX5",
                winPanel.transform,
                "x5",
                font,
                23f,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0f),
                new Vector2(70f, 45f),
                new Vector2(137f, 465f)).color = Color.white;

            var adRewardButton = CreateSpriteButton(
                "AdRewardButton",
                winPanel.transform,
                OrangePanelSpritePath,
                new Vector2(0.5f, 0f),
                new Vector2(470f, 118f),
                new Vector2(0f, 274f));
            adRewardButton.interactable = false;

            var adRewardLabel = CreateTmpText(
                "Label",
                adRewardButton.transform,
                "ПОЛУЧИТЬ x4\n1760",
                font,
                26f,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f),
                new Vector2(360f, 94f),
                new Vector2(-25f, 0f));
            adRewardLabel.color = Color.white;
            ConfigureUiText(adRewardLabel, 0.08f);

            CreateSpriteImage(
                "WatchIcon",
                adRewardButton.transform,
                WatchSpritePath,
                new Vector2(0.88f, 0.5f),
                new Vector2(54f, 54f),
                Vector2.zero,
                Color.white);

            var nextButton = CreateSpriteButton(
                "NextLevelButton",
                winPanel.transform,
                BluePanelSpritePath,
                new Vector2(0.5f, 0f),
                new Vector2(470f, 118f),
                new Vector2(0f, 132f));

            var nextLabel = CreateTmpText(
                "Label",
                nextButton.transform,
                "ПОЛУЧИТЬ\n440",
                font,
                27f,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f),
                new Vector2(350f, 96f),
                new Vector2(-26f, 0f));
            nextLabel.color = Color.white;
            ConfigureUiText(nextLabel, 0.08f);

            CreateSpriteImage(
                "RewardMoney",
                nextButton.transform,
                DollarSpritePath,
                new Vector2(0.88f, 0.5f),
                new Vector2(74f, 48f),
                Vector2.zero,
                Color.white);

            winPanel.SetActive(false);

            // ----------------------------------------------------------
            // LOSE — simple reference-style retry state.
            // ----------------------------------------------------------
            var losePanel = CreateRect("LosePanel", safeArea.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;

            var loseCard = CreateSlicedSpriteImage(
                "LoseCard",
                losePanel.transform,
                BlackTransparentPanelSpritePath,
                new Vector2(0.5f, 0.5f),
                new Vector2(560f, 500f),
                new Vector2(0f, -10f),
                new Color(1f, 1f, 1f, 0.92f));

            var loseTitle = CreateTmpText(
                "LoseTitle",
                loseCard.transform,
                "НЕУДАЧА",
                font,
                52f,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.72f),
                new Vector2(480f, 84f),
                Vector2.zero);
            loseTitle.color = Color.white;
            ConfigureUiText(loseTitle, 0.16f);

            CreateSpriteImage(
                "RestartIcon",
                loseCard.transform,
                RestartSpritePath,
                new Vector2(0.5f, 0.5f),
                new Vector2(130f, 130f),
                new Vector2(0f, 10f),
                Color.white);

            var restartButton = CreateSpriteButton(
                "RestartButton",
                loseCard.transform,
                BluePanelSpritePath,
                new Vector2(0.5f, 0.17f),
                new Vector2(400f, 106f),
                Vector2.zero);

            var restartLabel = CreateTmpText(
                "Label",
                restartButton.transform,
                "ПОВТОРИТЬ",
                font,
                28f,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f),
                new Vector2(340f, 80f),
                Vector2.zero);
            restartLabel.color = Color.white;
            ConfigureUiText(restartLabel, 0.08f);

            losePanel.SetActive(false);

            var uiAudioGo = new GameObject("UIAudio");
            uiAudioGo.transform.SetParent(canvasGo.transform, false);
            var uiAudioSource = uiAudioGo.AddComponent<AudioSource>();
            uiAudioSource.playOnAwake = false;
            uiAudioSource.spatialBlend = 0f;

            var hudSerialized = new SerializedObject(hud);
            hudSerialized.FindProperty("readyPanel").objectReferenceValue = ready.gameObject;
            hudSerialized.FindProperty("gameplayPanel").objectReferenceValue = gameplay.gameObject;
            hudSerialized.FindProperty("winPanel").objectReferenceValue = winPanel;
            hudSerialized.FindProperty("losePanel").objectReferenceValue = losePanel;
            hudSerialized.FindProperty("levelText").objectReferenceValue = levelText;
            hudSerialized.FindProperty("wealthText").objectReferenceValue = wealthText;
            hudSerialized.FindProperty("multiplierText").objectReferenceValue = null;
            hudSerialized.FindProperty("winScoreText").objectReferenceValue = winScore;
            hudSerialized.FindProperty("progressFill").objectReferenceValue = null;
            hudSerialized.FindProperty("restartButton").objectReferenceValue = restartButton;
            hudSerialized.FindProperty("nextButton").objectReferenceValue = nextButton;
            hudSerialized.FindProperty("runnerStatus").objectReferenceValue = runnerStatusView;
            hudSerialized.FindProperty("readySettingsIcon").objectReferenceValue = readySettingsIcon.gameObject;
            hudSerialized.FindProperty("runExitIcon").objectReferenceValue = runExitIcon.gameObject;
            hudSerialized.FindProperty("winPointer").objectReferenceValue = winPointer.rectTransform;
            hudSerialized.FindProperty("adRewardLabel").objectReferenceValue = adRewardLabel;
            hudSerialized.FindProperty("uiAudioSource").objectReferenceValue = uiAudioSource;
            hudSerialized.FindProperty("clickClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(ClickAudioPath);
            hudSerialized.FindProperty("winRays").objectReferenceValue = winRaysImage.rectTransform;
            var winMoneyProperty = hudSerialized.FindProperty("winMoneyFx");
            winMoneyProperty.arraySize = winMoneyFx.Length;
            for (var i = 0; i < winMoneyFx.Length; i++)
                winMoneyProperty.GetArrayElementAtIndex(i).objectReferenceValue = winMoneyFx[i];
            hudSerialized.ApplyModifiedPropertiesWithoutUndo();

            return canvasGo;
        }

        private static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            _ = go;
        }

        private static void WireBootstrap(GameBootstrapper bootstrap, Camera camera, GameplayHud hud, LevelBindings level01, LevelBindings level02)
        {
            var runnerConfig = AssetDatabase.LoadAssetAtPath<RunnerConfigSO>("Assets/_Game/Data/Configs/RunnerConfig.asset");
            var cameraConfig = AssetDatabase.LoadAssetAtPath<CameraConfigSO>("Assets/_Game/Data/Configs/CameraConfig.asset");
            var wealthConfig = AssetDatabase.LoadAssetAtPath<WealthConfigSO>("Assets/_Game/Data/Configs/WealthConfig.asset");
            if (wealthConfig != null)
            {
                var wealthSerialized = new SerializedObject(wealthConfig);
                wealthSerialized.FindProperty("initialWealth").intValue = 40;
                wealthSerialized.FindProperty("minimumWealth").intValue = 0;
                wealthSerialized.FindProperty("maximumWealth").intValue = 140;
                wealthSerialized.FindProperty("changeSpeed").floatValue = 140f;
                wealthSerialized.FindProperty("hoboThreshold").intValue = 0;
                wealthSerialized.FindProperty("poorThreshold").intValue = 31;
                wealthSerialized.FindProperty("decentThreshold").intValue = 61;
                wealthSerialized.FindProperty("richThreshold").intValue = 101;
                wealthSerialized.FindProperty("millionaireThreshold").intValue = 139;
                wealthSerialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(wealthConfig);
            }

            var serialized = new SerializedObject(bootstrap);
            var levels = serialized.FindProperty("authoredLevels");
            levels.arraySize = level02 != null ? 2 : 1;
            levels.GetArrayElementAtIndex(0).objectReferenceValue = level01;
            if (level02 != null) levels.GetArrayElementAtIndex(1).objectReferenceValue = level02;
            serialized.FindProperty("gameplayCamera").objectReferenceValue = camera;
            serialized.FindProperty("runnerConfig").objectReferenceValue = runnerConfig;
            serialized.FindProperty("cameraConfig").objectReferenceValue = cameraConfig;
            serialized.FindProperty("wealthConfig").objectReferenceValue = wealthConfig;
            serialized.FindProperty("hud").objectReferenceValue = hud;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateVendorReferenceObject()
        {
            var go = new GameObject("ProvidedLevelManager_ReferenceOnly");
            var manager = go.AddComponent<LevelManager>();
            manager.enabled = false;
            var levels = AssetDatabase.LoadAssetAtPath<LevelsList>("Assets/_Game/Data/Levels/LevelList.asset");
            if (levels != null) SetObjectReference(manager, "levels", levels);
        }

        private static TMP_FontAsset GetOrCreateDynamicFont()
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(GeneratedFontPath);
            if (existing != null) return existing;

            var source = AssetDatabase.LoadAssetAtPath<Font>(InterFontPath);
            if (source == null)
            {
                Debug.LogError($"RUN RICH CLEAN BUILDER: Source font missing: {InterFontPath}");
                return null;
            }

            EnsureFolder(GeneratedFonts);
            var fontAsset = TMP_FontAsset.CreateFontAsset(source);
            if (fontAsset == null) return null;
            fontAsset.name = "RunRichInterDynamic";
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            fontAsset.isMultiAtlasTexturesEnabled = true;
            AssetDatabase.CreateAsset(fontAsset, GeneratedFontPath);

            if (fontAsset.material != null && !AssetDatabase.Contains(fontAsset.material))
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

            var atlases = fontAsset.atlasTextures;
            if (atlases != null)
            {
                foreach (var atlas in atlases)
                {
                    if (atlas != null && !AssetDatabase.Contains(atlas))
                        AssetDatabase.AddObjectToAsset(atlas, fontAsset);
                }
            }

            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(GeneratedFontPath);
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(GeneratedFontPath);
        }

        private static PickupDefinitionSO GetOrCreatePickupDefinition(string path, int wealthDelta)
        {
            var asset = AssetDatabase.LoadAssetAtPath<PickupDefinitionSO>(path);
            if (asset == null)
            {
                EnsureFolder(Path.GetDirectoryName(path)?.Replace('\\', '/'));
                asset = CreateInstance<PickupDefinitionSO>();
                AssetDatabase.CreateAsset(asset, path);
            }

            var serialized = new SerializedObject(asset);
            serialized.FindProperty("wealthDelta").intValue = wealthDelta;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static GameObject PrepareReferencePickupPrefab(
            string prefabPath,
            PickupDefinitionSO definition,
            float colliderRadius,
            Material visualMaterial,
            bool positivePresentation)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (source == null)
            {
                Debug.LogError($"RUN RICH CLEAN BUILDER: Missing authored pickup prefab: {prefabPath}");
                return null;
            }

            var contents = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var collider = contents.GetComponent<SphereCollider>() ?? contents.AddComponent<SphereCollider>();
                collider.isTrigger = true;
                collider.enabled = true;
                collider.center = Vector3.zero;
                collider.radius = colliderRadius;

                var trigger = contents.GetComponent<PickupTrigger>() ?? contents.AddComponent<PickupTrigger>();
                SetObjectReference(trigger, "definition", definition);

                RemoveNamedChildRecursive(contents.transform, "ReferencePickupIcon");
                RemoveNamedChildRecursive(contents.transform, "ReferencePickupPresentation");

                if (visualMaterial != null)
                {
                    foreach (var renderer in contents.GetComponentsInChildren<Renderer>(true))
                    {
                        if (renderer is SpriteRenderer)
                            continue;

                        renderer.sharedMaterial = visualMaterial;
                        PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
                    }
                }

                NormalizeReferencePickupModel(contents.transform, positivePresentation);
                CreateReferencePickupPresentation(contents.transform, positivePresentation);
                PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        private static void NormalizeReferencePickupModel(Transform pickupRoot, bool positive)
        {
            if (pickupRoot == null) return;

            Transform modelRoot = null;
            for (var i = 0; i < pickupRoot.childCount; i++)
            {
                var child = pickupRoot.GetChild(i);
                if (child.GetComponentInChildren<MeshRenderer>(true) != null ||
                    child.GetComponentInChildren<SkinnedMeshRenderer>(true) != null)
                {
                    modelRoot = child;
                    break;
                }
            }

            if (modelRoot == null) return;

            modelRoot.localPosition = positive
                ? new Vector3(0f, .109f, 0f)
                : new Vector3(0f, .064f, 0f);
            // Both FBXs show their useful painted side from local +X. Turn only
            // the visual mesh toward a runner approaching along the strip.
            modelRoot.localRotation = Quaternion.Euler(0f, positive ? 90f : 180f, 0f);
            // Use the imported mesh bounds, not FBX scale, to keep props near
            // their reference world sizes: bills ~0.8 m long, bottle ~0.8 m high.
            modelRoot.localScale = Vector3.one * (positive ? 2f : 1.2f);
            PrefabUtility.RecordPrefabInstancePropertyModifications(modelRoot);
        }

        private static void CreateReferencePickupPresentation(Transform pickupRoot, bool positive)
        {
            if (pickupRoot == null) return;

            var presentation = new GameObject("ReferencePickupPresentation");
            presentation.transform.SetParent(pickupRoot, false);

            var groundGlow = CreateWorldSprite(
                "GroundGlow",
                presentation.transform,
                PositiveAuraSpritePath,
                new Vector3(0f, 0.015f, 0f),
                positive ? 0.2f : 0.14f,
                positive ? new Color(0.6f, 1f, 0.2f, 0.8f) : new Color(1f, 0.12f, 0.08f, 0.35f),
                29);
            groundGlow.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var glowMaterial = GetReferenceMaterial(PickupGlowMaterialPath);
            if (glowMaterial != null) groundGlow.sharedMaterial = glowMaterial;

            var billboard = new GameObject("Billboard").transform;
            billboard.SetParent(presentation.transform, false);
            billboard.localPosition = new Vector3(0f, 0.03f, 0f);

            SpriteRenderer aura = null;
            if (positive)
            {
                aura = CreateWorldSprite(
                    "PickupAura",
                    billboard,
                    PositiveAuraSpritePath,
                    new Vector3(0f, 0.22f, -0.2f),
                    0.14f,
                    new Color(0.6f, 1f, 0.2f, 0.8f),
                    30);
                if (glowMaterial != null) aura.sharedMaterial = glowMaterial;
            }

            var marker = CreateWorldSprite(
                positive ? "PlusMarker" : "NegativeMarker",
                billboard,
                positive ? PlusSpritePath : NegativePickupSpritePath,
                positive ? new Vector3(0.16f, 0.38f, 0f) : new Vector3(0.17f, 0.37f, 0f),
                positive ? 0.050f : 0.047f,
                Color.white,
                31);

            if (positive)
            {
                var leftStar = CreateWorldSprite("PickupStarLeft", billboard, PickupStarSpritePath,
                    new Vector3(-0.28f, 0.16f, -0.07f), 0.035f,
                    new Color(1f, 1f, 0.65f, 0.8f), 32);
                var rightStar = CreateWorldSprite("PickupStarRight", billboard, PickupStarSpritePath,
                    new Vector3(0.27f, 0.1f, -0.07f), 0.025f,
                    new Color(1f, 1f, 0.65f, 0.7f), 32);
                if (glowMaterial != null)
                {
                    leftStar.sharedMaterial = glowMaterial;
                    rightStar.sharedMaterial = glowMaterial;
                }
            }

            var behaviour = presentation.AddComponent<PickupPresentation>();
            SetObjectReference(behaviour, "billboard", billboard);
            SetObjectReference(behaviour, "aura", aura);
            SetObjectReference(behaviour, "marker", marker);
        }

        private static void RemoveNamedChildRecursive(Transform root, string childName)
        {
            if (root == null || string.IsNullOrEmpty(childName))
                return;

            for (var i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i);
                if (child.name == childName)
                {
                    DestroyImmediate(child.gameObject);
                    continue;
                }

                RemoveNamedChildRecursive(child, childName);
            }
        }

        private static Transform InstantiateVisualOnly(string prefabPath, Transform parent, string name)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"RUN RICH CLEAN BUILDER: Missing authored visual prefab: {prefabPath}");
                return null;
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
            if (instance == null) return null;

            instance.name = name;
            RemoveNamedChildRecursive(instance.transform, "ReferencePickupIcon");
            foreach (var trigger in instance.GetComponents<PickupTrigger>())
                DestroyImmediate(trigger);
            foreach (var collider in instance.GetComponents<Collider>())
                DestroyImmediate(collider);
            return instance.transform;
        }

        private static void FitReferenceGroundToSegment(GameObject go, float targetLength)
        {
            var filter = go.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null) return;

            var size = filter.sharedMesh.bounds.size;
            if (size.x <= 0.0001f || size.z <= 0.0001f) return;

            // The reference ground mesh is authored at approximately 6 x 0.28 x 7.5.
            // Keep its authored width/height and only adapt Z to the centerline segment length.
            go.transform.localScale = new Vector3(1f, 1f, targetLength / size.z);
        }

        private static void FitReferenceMeshFootprint(GameObject go, float targetWidth, float targetDepth)
        {
            var filter = go.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null) return;

            var size = filter.sharedMesh.bounds.size;
            if (size.x <= 0.0001f || size.z <= 0.0001f) return;

            go.transform.localScale = new Vector3(
                targetWidth / size.x,
                1f,
                targetDepth / size.z);
        }

        private static GameObject CreateOrReplacePickupPrefab(
            string path,
            string name,
            string modelPath,
            Material material,
            PickupDefinitionSO definition,
            float targetHeight,
            float colliderRadius)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
                AssetDatabase.DeleteAsset(path);

            var root = new GameObject(name);
            var collider = root.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = colliderRadius;
            collider.center = Vector3.zero;
            var trigger = root.AddComponent<PickupTrigger>();
            SetObjectReference(trigger, "definition", definition);

            var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (modelAsset != null)
            {
                var visual = PrefabUtility.InstantiatePrefab(modelAsset, root.transform) as GameObject;
                if (visual != null)
                {
                    visual.name = "Visual";
                    visual.transform.localPosition = Vector3.zero;
                    visual.transform.localRotation = Quaternion.identity;
                    visual.transform.localScale = Vector3.one;
                    SetAllRendererMaterials(visual, material);
                    FitHierarchyToHeight(visual, targetHeight, true);
                }
            }

            EnsureFolder(GeneratedPrefabs);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);
            return prefab;
        }

        private static void AssignDefinitionPrefab(PickupDefinitionSO definition, GameObject prefab)
        {
            var serialized = new SerializedObject(definition);
            serialized.FindProperty("prefab").objectReferenceValue = prefab;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
        }

        private static RectTransform CreateRect(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 size,
            Vector2 anchoredPosition)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            return rect;
        }

        private static TextMeshProUGUI CreateTmpText(
            string name,
            Transform parent,
            string value,
            TMP_FontAsset font,
            float fontSize,
            TextAlignmentOptions alignment,
            Vector2 anchor,
            Vector2 size,
            Vector2 anchoredPosition)
        {
            var rect = CreateRect(name, parent, anchor, anchor, size, anchoredPosition);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = fontSize;
            text.text = value;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static TextMeshPro CreateWorldText(
            string name,
            Transform parent,
            string value,
            TMP_FontAsset font,
            Color color,
            Vector3 localPosition,
            float fontSize,
            float width)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.identity;
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width * 10f, 2f);
            var text = go.AddComponent<TextMeshPro>();
            text.font = font;
            text.fontSize = fontSize * 10f;
            text.text = value;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = false;
            return text;
        }

        private static Image CreateImage(
            string name,
            Transform parent,
            Vector2 anchor,
            Vector2 size,
            Vector2 anchoredPosition,
            Color color)
        {
            var rect = CreateRect(name, parent, anchor, anchor, size, anchoredPosition);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Image CreateSpriteImage(
            string name,
            Transform parent,
            string spritePath,
            Vector2 anchor,
            Vector2 size,
            Vector2 anchoredPosition,
            Color color)
        {
            var image = CreateImage(name, parent, anchor, size, anchoredPosition, color);
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        private static GameObject CreateFullPanel(string name, Transform parent, Color color)
        {
            var rect = CreateRect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return rect.gameObject;
        }

        private static Image CreateSlicedSpriteImage(
            string name,
            Transform parent,
            string spritePath,
            Vector2 anchor,
            Vector2 size,
            Vector2 anchoredPosition,
            Color color)
        {
            var image = CreateImage(name, parent, anchor, size, anchoredPosition, color);
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            image.type = Image.Type.Sliced;
            image.preserveAspect = false;
            return image;
        }

        private static Button CreateSpriteButton(
            string name,
            Transform parent,
            string spritePath,
            Vector2 anchor,
            Vector2 size,
            Vector2 anchoredPosition)
        {
            var image = CreateSlicedSpriteImage(name, parent, spritePath, anchor, size, anchoredPosition, Color.white);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            return button;
        }

        private static void ConfigureUiText(TMP_Text text, float outlineWidth)
        {
            if (text == null) return;
            text.enableWordWrapping = false;
            text.raycastTarget = false;
            text.outlineWidth = Mathf.Clamp01(outlineWidth);
            text.outlineColor = new Color32(20, 35, 50, 165);
        }

        private static Button CreateButton(string name, Transform parent, string label, TMP_FontAsset font, Vector2 anchor, Vector2 size)
        {
            var rect = CreateRect(name, parent, anchor, anchor, size, Vector2.zero);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0.18f, 0.78f, 0.35f, 0.96f);
            var button = rect.gameObject.AddComponent<Button>();
            var text = CreateTmpText("Label", rect.transform, label, font, 32f, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), size, Vector2.zero);
            text.color = Color.white;
            return button;
        }

        private static void CreateButtonVisual(
            string name,
            Transform parent,
            string label,
            TMP_FontAsset font,
            Vector2 anchor,
            Vector2 size,
            Vector2 anchoredPosition)
        {
            var rect = CreateRect(name, parent, anchor, anchor, size, anchoredPosition);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.85f);
            if (!string.IsNullOrEmpty(label))
            {
                var text = CreateTmpText("Label", rect.transform, label, font, 28f, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), size, Vector2.zero);
                text.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            }
        }

        private static GameObject CreateMeshObject(string name, Transform parent, Mesh mesh, Material material)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }

        private static void FitMeshObject(GameObject go, float targetLargestDimension)
        {
            var filter = go.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null) return;
            var size = filter.sharedMesh.bounds.size;
            var largest = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
            if (largest <= 0.0001f) return;
            go.transform.localScale = Vector3.one * (targetLargestDimension / largest);
        }

        private static void FitHierarchyToHeight(GameObject root, float targetHeight, bool placeOnGround)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            if (bounds.size.y <= 0.0001f) return;

            var factor = targetHeight / bounds.size.y;
            root.transform.localScale *= factor;

            if (!placeOnGround) return;
            renderers = root.GetComponentsInChildren<Renderer>(true);
            bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            root.transform.position += Vector3.up * (-bounds.min.y + root.transform.parent.position.y);
        }

        private static void SetAllRendererMaterials(GameObject root, Material material)
        {
            if (material == null) return;
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var count = renderer.sharedMaterials.Length;
                if (count <= 0) count = 1;
                var materials = new Material[count];
                for (var i = 0; i < count; i++) materials[i] = material;
                renderer.sharedMaterials = materials;
            }
        }

        private static void SetRendererMaterial(GameObject go, Material material)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null && material != null) renderer.sharedMaterial = material;
        }

        private static Material GetOrCreatePickupVisualMaterial(bool positive)
        {
            var source = GetReferenceMaterial(positive ? PropsFlatMaterialPath : PropsFlatBadMaterialPath);
            if (source == null) return null;

            var path = GeneratedMaterials + (positive
                ? "/props_flat_MoneyPickup.mat"
                : "/props_flat_bad_BottlePickup.mat");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(source);
                AssetDatabase.CreateAsset(material, path);
            }
            else
                material.CopyPropertiesFromMaterial(source);

            material.SetColor("_HColor", Color.white);
            material.SetColor("_SColor", positive
                ? new Color(0.55f, 0.55f, 0.55f)
                : new Color(0.42f, 0.42f, 0.42f));
            material.SetTexture("_EmissionMap", source.mainTexture);
            material.SetColor("_EmissionColor", positive
                ? new Color(0.32f, 0.32f, 0.32f)
                : new Color(0.14f, 0.14f, 0.14f));
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material GetReferenceMaterial(string sourceAssetPath)
        {
            if (string.IsNullOrWhiteSpace(sourceAssetPath))
                return null;

            var material = AssetDatabase.LoadAssetAtPath<Material>(sourceAssetPath);
            if (material == null)
                Debug.LogError($"RUN RICH CLEAN BUILDER: Missing reference material: {sourceAssetPath}");
            return material;
        }

        private static void SetObjectReference(Object target, string propertyName, Object value)
        {
            if (target == null) return;
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError($"RUN RICH CLEAN BUILDER: Property '{propertyName}' not found on {target.GetType().Name}.", target);
                return;
            }
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObjectArray<T>(Object target, string propertyName, T[] values) where T : Object
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError($"RUN RICH CLEAN BUILDER: Array property '{propertyName}' not found on {target.GetType().Name}.", target);
                return;
            }

            property.arraySize = values?.Length ?? 0;
            for (var i = 0; i < property.arraySize; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSerializedFloat(Object target, string propertyName, float value)
        {
            var so = new SerializedObject(target);
            var property = so.FindProperty(propertyName);
            if (property == null) return;
            property.floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSerializedInt(Object target, string propertyName, int value)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(propertyName);
            if (property != null) property.intValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSerializedBool(Object target, string propertyName, bool value)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(propertyName);
            if (property != null) property.boolValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static void SetAsOnlyBuildScene(string scenePath)
        {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
        }

        private static GameObject FindRoot(IEnumerable<GameObject> roots, string name)
        {
            foreach (var root in roots)
                if (root.name == name) return root;
            return null;
        }

        private static void RequireRoot(IEnumerable<GameObject> roots, string name, ICollection<string> missing)
        {
            if (FindRoot(roots, name) == null) missing.Add(name);
        }

        private static void RequireSerializedObject(
            SerializedObject serialized,
            string propertyName,
            string label,
            ICollection<string> missing)
        {
            if (serialized == null)
            {
                missing.Add(label);
                return;
            }

            var property = serialized.FindProperty(propertyName);
            if (property == null || property.objectReferenceValue == null)
                missing.Add(label);
        }

        private static void RequireSerializedArray(
            SerializedObject serialized,
            string propertyName,
            string label,
            ICollection<string> missing)
        {
            if (serialized == null)
            {
                missing.Add(label);
                return;
            }

            var property = serialized.FindProperty(propertyName);
            if (property == null || !property.isArray || property.arraySize == 0)
            {
                missing.Add(label);
                return;
            }

            for (var i = 0; i < property.arraySize; i++)
            {
                var item = property.GetArrayElementAtIndex(i);
                if (item == null || item.objectReferenceValue == null)
                {
                    missing.Add(label);
                    return;
                }
            }
        }

        private static void RequireChild(Transform parent, string name, ICollection<string> missing)
        {
            if (parent.Find(name) == null) missing.Add(parent.name + "/" + name);
        }

        private readonly struct PlayerBuildResult
        {
            public PlayerBuildResult(RunnerActor actor, PlayerAppearance appearance, RunnerAnimationView animationView)
            {
                Actor = actor;
                Appearance = appearance;
                AnimationView = animationView;
            }

            public RunnerActor Actor { get; }
            public PlayerAppearance Appearance { get; }
            public RunnerAnimationView AnimationView { get; }
        }

        private readonly struct GateBuildResult
        {
            public GateBuildResult(GateChoiceTrigger trigger, Collider collider)
            {
                Trigger = trigger;
                Collider = collider;
            }

            public GateChoiceTrigger Trigger { get; }
            public Collider Collider { get; }
        }
    }
}
#endif
