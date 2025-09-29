using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using EmpireOfHonor.Gameplay;
using EmpireOfHonor.Input;
using EmpireOfHonor.UI;

#if ENABLE_INPUT_SYSTEM && UNITY_INPUT_SYSTEM_EXISTS
#define INPUT_SYSTEM_ENABLED
#endif

#if INPUT_SYSTEM_ENABLED
using UnityEngine.InputSystem;
#endif

namespace EmpireOfHonor.Editor
{
    /// <summary>
    /// Provides a menu entry to build a ready-to-play scene with required components.
    /// </summary>
#if INPUT_SYSTEM_ENABLED
    public static class ReadySceneBuilder
    {
        private const string MenuPath = "Alaia Iva/Create READY Scene (Input System)";
        private const string ScenePath = "Assets/ReadyScene_InputSystem.unity";

        [MenuItem(MenuPath)]
        public static void CreateReadyScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "ReadyScene_InputSystem";

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = Vector3.one * 5f;

            TryAddNavMeshSurface(ground);

            var light = new GameObject("Directional Light");
            var lightComponent = light.AddComponent<Light>();
            lightComponent.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Input/EmpireOfHonor.inputactions");
            if (inputAsset == null)
            {
                Debug.LogError("Input action asset not found at Assets/Input/EmpireOfHonor.inputactions");
                return;
            }

            var heroData = CreateHero(inputAsset);
            var tacticalData = CreateTacticalCamera(inputAsset);
            tacticalData.Camera.gameObject.SetActive(false);

            var allies = CreateAllies(3);
            CreateEnemies(3);

            var manager = new GameObject("GameManager");
            var switcher = manager.AddComponent<CameraSwitcher_Input>();
            AssignCameraSwitcher(switcher, heroData.PlayerInput, heroData.TpsInput, heroData.Camera, tacticalData.Camera, tacticalData.Input);

            var commandOverlay = manager.AddComponent<CommandOverlay_Input>();
            AssignCommandOverlay(commandOverlay, inputAsset, tacticalData.Camera, allies);

            var switcherSerialized = new SerializedObject(switcher);
            switcherSerialized.FindProperty("commandOverlay").objectReferenceValue = commandOverlay;
            switcherSerialized.FindProperty("switchAction").objectReferenceValue = heroData.SwitchReference;
            switcherSerialized.ApplyModifiedPropertiesWithoutUndo();

            CreateCanvas();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
        }

        private struct HeroData
        {
            public GameObject Hero;
            public TPS_Input TpsInput;
            public PlayerInput PlayerInput;
            public Camera Camera;
            public InputActionReference SwitchReference;
        }

        private struct TacticalData
        {
            public Camera Camera;
            public Tactical_Input Input;
        }

        private static HeroData CreateHero(InputActionAsset inputAsset)
        {
            var hero = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hero.name = "Hero";
            hero.transform.position = new Vector3(0f, 1f, 0f);

            var characterController = hero.AddComponent<CharacterController>();
            characterController.height = 1.8f;
            characterController.center = new Vector3(0f, 0.9f, 0f);

            var health = hero.AddComponent<Health>();
            SetSerializedEnum(new SerializedObject(health).FindProperty("team"), (int)Health.Team.Player);

            hero.AddComponent<Weapon>();
            var tpsInput = hero.AddComponent<TPS_Input>();

            var cameraRoot = new GameObject("TPSCameraRoot");
            cameraRoot.transform.SetParent(hero.transform, false);
            cameraRoot.transform.localPosition = Vector3.zero;

            var cameraPivot = new GameObject("TPSCameraPivot");
            cameraPivot.transform.SetParent(cameraRoot.transform, false);
            cameraPivot.transform.localPosition = new Vector3(0f, 1.6f, 0f);

            var cameraObject = new GameObject("TPSCamera");
            cameraObject.transform.SetParent(cameraPivot.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 0f, -4.5f);
            cameraObject.transform.localRotation = Quaternion.identity;
            var cameraComponent = cameraObject.AddComponent<Camera>();

            var tpsSerialized = new SerializedObject(tpsInput);
            tpsSerialized.FindProperty("characterController").objectReferenceValue = characterController;
            tpsSerialized.FindProperty("cameraRoot").objectReferenceValue = cameraRoot.transform;
            tpsSerialized.FindProperty("cameraPivot").objectReferenceValue = cameraPivot.transform;
            tpsSerialized.FindProperty("moveAction").objectReferenceValue = CreateReference(inputAsset, "Player", "Move");
            tpsSerialized.FindProperty("lookAction").objectReferenceValue = CreateReference(inputAsset, "Player", "Look");
            tpsSerialized.FindProperty("jumpAction").objectReferenceValue = CreateReference(inputAsset, "Player", "Jump");
            tpsSerialized.FindProperty("sprintAction").objectReferenceValue = CreateReference(inputAsset, "Player", "Sprint");
            tpsSerialized.FindProperty("attackAction").objectReferenceValue = CreateReference(inputAsset, "Player", "Attack");
            tpsSerialized.ApplyModifiedPropertiesWithoutUndo();

            var playerInput = hero.AddComponent<PlayerInput>();
            playerInput.actions = inputAsset;
            playerInput.defaultActionMap = "Player";

            var switchReference = CreateReference(inputAsset, "Player", "SwitchCamera");

            return new HeroData
            {
                Hero = hero,
                TpsInput = tpsInput,
                PlayerInput = playerInput,
                Camera = cameraComponent,
                SwitchReference = switchReference
            };
        }

        private static TacticalData CreateTacticalCamera(InputActionAsset inputAsset)
        {
            var tactical = new GameObject("TacticalCamera");
            tactical.transform.position = new Vector3(0f, 20f, -10f);
            tactical.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
            var camera = tactical.AddComponent<Camera>();

            var tacticalInput = tactical.AddComponent<Tactical_Input>();
            var serialized = new SerializedObject(tacticalInput);
            serialized.FindProperty("panAction").objectReferenceValue = CreateReference(inputAsset, "Tactical", "Pan");
            serialized.FindProperty("rotateLeftAction").objectReferenceValue = CreateReference(inputAsset, "Tactical", "RotateLeft");
            serialized.FindProperty("rotateRightAction").objectReferenceValue = CreateReference(inputAsset, "Tactical", "RotateRight");
            serialized.FindProperty("zoomAction").objectReferenceValue = CreateReference(inputAsset, "Tactical", "Zoom");
            serialized.ApplyModifiedPropertiesWithoutUndo();

            tacticalInput.enabled = false;

            return new TacticalData
            {
                Camera = camera,
                Input = tacticalInput
            };
        }

        private static void AssignCameraSwitcher(CameraSwitcher_Input switcher, PlayerInput playerInput, TPS_Input tpsInput, Camera tpsCamera, Camera tacticalCamera, Tactical_Input tacticalInput)
        {
            var serialized = new SerializedObject(switcher);
            serialized.FindProperty("playerInput").objectReferenceValue = playerInput;
            serialized.FindProperty("tpsCamera").objectReferenceValue = tpsCamera;
            serialized.FindProperty("tacticalCamera").objectReferenceValue = tacticalCamera;
            serialized.FindProperty("tpsController").objectReferenceValue = tpsInput;
            serialized.FindProperty("tacticalController").objectReferenceValue = tacticalInput;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignCommandOverlay(CommandOverlay_Input overlay, InputActionAsset asset, Camera tacticalCamera, GameObject[] allies)
        {
            var serialized = new SerializedObject(overlay);
            serialized.FindProperty("tacticalCamera").objectReferenceValue = tacticalCamera;
            serialized.FindProperty("commandAction").objectReferenceValue = CreateReference(asset, "Tactical", "Command");
            serialized.FindProperty("holdAction").objectReferenceValue = CreateReference(asset, "Tactical", "Hold");
            serialized.FindProperty("modifierAltAction").objectReferenceValue = CreateReference(asset, "Tactical", "ModifierAlt");
            serialized.FindProperty("selectGroup1Action").objectReferenceValue = CreateReference(asset, "Tactical", "SelectGroup1");
            serialized.FindProperty("selectGroup2Action").objectReferenceValue = CreateReference(asset, "Tactical", "SelectGroup2");
            serialized.FindProperty("selectGroup3Action").objectReferenceValue = CreateReference(asset, "Tactical", "SelectGroup3");
            serialized.FindProperty("selectGroup4Action").objectReferenceValue = CreateReference(asset, "Tactical", "SelectGroup4");

            var groupsProp = serialized.FindProperty("groups");
            groupsProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
            {
                var groupProp = groupsProp.GetArrayElementAtIndex(i);
                groupProp.FindPropertyRelative("name").stringValue = $"Group {i + 1}";
                var unitsProp = groupProp.FindPropertyRelative("units");
                unitsProp.arraySize = 0;
            }

            for (int i = 0; i < allies.Length && i < 4; i++)
            {
                var unitsProp = groupsProp.GetArrayElementAtIndex(i).FindPropertyRelative("units");
                unitsProp.arraySize = 1;
                unitsProp.GetArrayElementAtIndex(0).objectReferenceValue = allies[i].GetComponent<UnitController>();
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject[] CreateAllies(int count)
        {
            var allies = new GameObject[count];
            for (int i = 0; i < count; i++)
            {
                var ally = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                ally.name = $"Ally_{i + 1}";
                ally.transform.position = new Vector3(2f + i * 1.5f, 1f, 2f);

                var agent = ally.AddComponent<NavMeshAgent>();
                agent.speed = 3.5f;

                var health = ally.AddComponent<Health>();
                SetSerializedEnum(new SerializedObject(health).FindProperty("team"), (int)Health.Team.Ally);

                ally.AddComponent<NavMeshAgentSnap>();
                ally.AddComponent<Weapon>();
                ally.AddComponent<UnitController>();

                allies[i] = ally;
            }

            return allies;
        }

        private static void CreateEnemies(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                enemy.name = $"Enemy_{i + 1}";
                enemy.transform.position = new Vector3(-2f - i * 1.5f, 1f, -3f);

                var agent = enemy.AddComponent<NavMeshAgent>();
                agent.speed = 3.2f;

                var health = enemy.AddComponent<Health>();
                SetSerializedEnum(new SerializedObject(health).FindProperty("team"), (int)Health.Team.Enemy);

                enemy.AddComponent<NavMeshAgentSnap>();
                enemy.AddComponent<Weapon>();
                enemy.AddComponent<SimpleEnemyAI>();
            }
        }

        private static void CreateCanvas()
        {
            var canvasObject = new GameObject("UI Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            var textObject = new GameObject("Controls Text");
            textObject.transform.SetParent(canvasObject.transform, false);
            var rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(20f, -20f);
            rect.sizeDelta = new Vector2(480f, 300f);

            var text = textObject.AddComponent<Text>();
            text.fontSize = 18;
            text.color = Color.white;

            textObject.AddComponent<SimpleUI>();
        }

        private static void TryAddNavMeshSurface(GameObject ground)
        {
#if UNITY_AINAVIGATION
            var surface = ground.AddComponent<Unity.AI.Navigation.NavMeshSurface>();
            surface.useGeometry = Unity.AI.Navigation.NavMeshCollectGeometry.RenderMeshes;
            surface.BuildNavMesh();
#else
            var surfaceType = Type.GetType("Unity.AI.Navigation.NavMeshSurface, Unity.AI.Navigation");
            if (surfaceType != null)
            {
                var surface = ground.AddComponent(surfaceType) as Component;
                surfaceType.GetMethod("BuildNavMesh")?.Invoke(surface, null);
            }
#endif
        }

        private static InputActionReference CreateReference(InputActionAsset asset, string mapName, string actionName)
        {
            var action = asset.FindAction($"{mapName}/{actionName}", throwIfNotFound: true);
            return InputActionReference.Create(action);
        }

        private static void SetSerializedEnum(SerializedProperty property, int value)
        {
            if (property == null)
            {
                return;
            }

            property.enumValueIndex = value;
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
#else
    public static class ReadySceneBuilder
    {
        private const string MenuPath = "Alaia Iva/Create READY Scene (Input System)";

        [MenuItem(MenuPath)]
        public static void CreateReadyScene()
        {
            Debug.LogError(
                "The READY scene builder requires the Unity Input System package. Please enable it in Project Settings > Player.");
        }
    }
#endif
}
