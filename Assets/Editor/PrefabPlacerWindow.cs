using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public class PrefabPlacerWindow : EditorWindow
{
    public GameObject[] prefabs;
    private GameObject activePrefab;

    private List<GameObject> createdObjects = new List<GameObject>();

    private GUIStyle titleStyle;
    private GUIStyle selectedStyle;

    [MenuItem("Tools/Prefab Placer")]
    public static void Open()
    {
        GetWindow<PrefabPlacerWindow>();
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;

        titleStyle = new GUIStyle()
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        selectedStyle = new GUIStyle(GUI.skin.button);
        selectedStyle.normal.textColor = Color.green;
        selectedStyle.fontStyle = FontStyle.Bold;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnGUI()
    {
        if (prefabs == null) prefabs = new GameObject[0];

        GUILayout.Space(5);
        GUILayout.Label("Prefab Placer", titleStyle);
        GUILayout.Space(10);

        SerializedObject so = new SerializedObject(this);
        SerializedProperty prop = so.FindProperty("prefabs");
        EditorGUILayout.PropertyField(prop, true);
        so.ApplyModifiedProperties();

        GUILayout.Space(10);
        GUILayout.Label("Prefabs disponibles:");

        int columns = 2;
        int count = 0;

        for (int i = 0; i < prefabs.Length; i++)
        {
            if (count % columns == 0)
                GUILayout.BeginHorizontal();

            GameObject p = prefabs[i];

            if (p != null)
            {
                GUIStyle style = (p == activePrefab) ? selectedStyle : GUI.skin.button;

                if (GUILayout.Button(p.name, style, GUILayout.Height(40)))
                {
                    activePrefab = p;
                }
            }

            count++;

            if (count % columns == 0)
                GUILayout.EndHorizontal();
        }

        if (count % columns != 0)
            GUILayout.EndHorizontal();

        GUILayout.Space(15);

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Deshacer último objeto", GUILayout.Height(30)) && createdObjects.Count > 0)
        {
            GameObject obj = createdObjects[^1];
            createdObjects.RemoveAt(createdObjects.Count - 1);
            Undo.DestroyObjectImmediate(obj);
        }

        GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f);
        if (GUILayout.Button("Eliminar todos los objetos", GUILayout.Height(35)))
        {
            for (int i = createdObjects.Count - 1; i >= 0; i--)
            {
                Undo.DestroyObjectImmediate(createdObjects[i]);
            }
            createdObjects.Clear();
        }

        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);
        GUILayout.Label($"Objetos en escena: {createdObjects.Count}");
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (activePrefab == null) return;

        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(activePrefab);
                Undo.RegisterCreatedObjectUndo(newObj, "Instantiate Prefab");

                newObj.transform.position = hit.point;

                createdObjects.Add(newObj);

                EditorSceneManager.MarkSceneDirty(newObj.scene);
            }

            if (e.type == EventType.MouseDrag && createdObjects.Count > 0)
            {
                GameObject last = createdObjects[^1];

                Vector3 dir = hit.point - last.transform.position;

                if (dir.sqrMagnitude > 0.0001f)
                {
                    dir.y = 0f;
                    last.transform.rotation = Quaternion.LookRotation(dir);
                }

                e.Use();
            }
        }
    }
}