using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using System.Reflection;
using UnityEngine.Rendering;
using KmaxXR.Core;

namespace KmaxXR
{

    public class KmaxMenu
    {
        public const string COMPANY_NAME = "Kmax";
        const string GAMEOBJECT_EXT = "GameObject/" + COMPANY_NAME;

        [MenuItem(GAMEOBJECT_EXT + "/Add XRRig", false, 10)]
        static void AddXRRig()
        {
            CreateGameObjectFromPrefab<XRRig>(nameof(XRRig));
        }

        [MenuItem(GAMEOBJECT_EXT + "/Add XRRig", true)]
        static bool AddXRRigValid()
        {
            return GameObject.FindObjectOfType<XRRig>() == null;
        }

        [MenuItem(GAMEOBJECT_EXT + "/Convert to KmaxInputModule", false)]
        static void ConvertToStylus()
        {
            var cs = Selection.activeGameObject.GetComponents<BaseInputModule>();
            foreach (var item in cs)
            {
                Undo.DestroyObjectImmediate(item);
            }
            Undo.AddComponent<KmaxInputModule>(Selection.activeGameObject);
        }

        [MenuItem(GAMEOBJECT_EXT + "/Convert to KmaxInputModule", true)]
        static bool ConvertToStylusValid()
        {
            return Selection.activeGameObject != null &&
                Selection.activeGameObject.GetComponent<EventSystem>() != null;
        }

        [MenuItem(GAMEOBJECT_EXT + "/Fix Canvas", false)]
        public static void FixCanvas()
        {
            var caster = Selection.activeGameObject.GetComponent<KmaxUIRaycaster>();
            if (caster == null)
            {
                caster = Selection.activeGameObject.AddComponent<KmaxUIRaycaster>();
            }
            var fix = Selection.activeGameObject.GetComponent<UIScaler>();
            if (fix == null)
            {
                fix = Selection.activeGameObject.AddComponent<UIScaler>();
            }
            var vr = GameObject.FindObjectOfType<XRRig>();
            if (vr == null)
            {
                Debug.LogError("XRRig not found");
                return;
            }
            Undo.RegisterCompleteObjectUndo(Selection.activeTransform, "Fix Canvas");
            var canvas = Selection.activeGameObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            fix.FixSize(XRRig.ViewSize);
            fix.FixPose(XRRig.ScreenTrans);
            EditorUtility.SetDirty(Selection.activeTransform);
        }

        [MenuItem(GAMEOBJECT_EXT + "/Fix Canvas", true)]
        static bool FixCanvasValid()
        {
            if (Selection.activeGameObject == null) return false;
            var canvas = Selection.activeGameObject.GetComponent<Canvas>();
            if (canvas == null) return false;
            return true;
        }

        private static T CreateGameObject<T>(
            string name, bool setSelected = true, Transform parent = null)
            where T : Component
        {
            // Create the game object.
            GameObject gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent);
            gameObject.transform.SetAsLastSibling();

            // Register this operation with the Unity Editor's undo stack.
            Undo.RegisterCreatedObjectUndo(gameObject, $"Create {name}");

            // Determine whether to select the newly created Game Object
            // in the Unity Inspector window.
            if (setSelected)
            {
                Selection.activeGameObject = gameObject;
            }

            // Create the specified component.
            T component = gameObject.AddComponent<T>();

            return component;
        }

        const string PrefabAssetRelativePath = "Editor Resources";
        private static T CreateGameObjectFromPrefab<T>(
            string name, bool setSelected = true, Transform parent = null)
            where T : Component
        {
            // Attempt to find a reference to the prefab asset.
            GameObject prefab = FindAsset<GameObject>(
                $"{name} t:prefab", PrefabAssetRelativePath);

            if (prefab == null)
            {
                Debug.LogError(
                    $"Failed to create instance of {name}. " +
                    "Prefab not found.");

                return null;
            }

            // Create an instance of the prefab.
            var obj = PrefabUtility.InstantiatePrefab(prefab);
            GameObject gameObject = obj as GameObject;
            if (gameObject == null) return null;
            //GameObject gameObject = GameObject.Instantiate(prefab);
            gameObject.transform.SetParent(parent);
            gameObject.transform.SetAsLastSibling();
            //gameObject.name = name;

            // Register the operation with the Unity Editor's undo stack.
            Undo.RegisterCreatedObjectUndo(gameObject, $"Create {name}");

            // Determine whether to select the newly created prefab instance
            // in the Unity Inspector window.
            if (setSelected)
            {
                Selection.activeGameObject = gameObject;
            }

            return gameObject.GetComponent<T>();
        }

        private static T FindAsset<T>(string filter, string relativePath = null)
            where T : Object
        {
            string[] guids = AssetDatabase.FindAssets(filter);

            for (int i = 0; i < guids.Length; ++i)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (string.IsNullOrEmpty(relativePath) ||
                    assetPath.Contains(relativePath))
                {
                    return (T)AssetDatabase.LoadAssetAtPath(assetPath, typeof(T));
                }
            }

            return null;
        }
    }

}