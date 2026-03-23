using UnityEngine;
using UnityEditor;

//assigns rigidbodies to all children of a Fragmented Object for the purpose of havign desctructible structures

public class FragmentSetup : EditorWindow
{
    private GameObject fragmentParent;
    private float mass = 1f;
    private float drag = 0.5f;
    private float angularDrag = 0.5f;

    [MenuItem("Tools/Fragment Setup")]
    public static void ShowWindow()
    {
        GetWindow<FragmentSetup>("Fragment Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("Fragment Rigidbody Setup", EditorStyles.boldLabel);
        GUILayout.Space(8);

        fragmentParent = (GameObject)EditorGUILayout.ObjectField(
            "Fragment Parent Object", fragmentParent, typeof(GameObject), true);

        GUILayout.Space(4);
        mass = EditorGUILayout.FloatField("Mass per fragment", mass);
        drag = EditorGUILayout.FloatField("Drag", drag);
        angularDrag = EditorGUILayout.FloatField("Angular Drag", angularDrag);

        GUILayout.Space(12);

        if (GUILayout.Button("Add Rigidbodies to all children"))
        {
            if (fragmentParent == null)
            {
                EditorUtility.DisplayDialog("No object selected",
                    "Please assign a Fragment Parent Object first.", "OK");
                return;
            }

            int count = 0;
            foreach (Transform child in fragmentParent.transform)
            {
                // Fix MeshCollider convex first
                MeshCollider mc = child.GetComponent<MeshCollider>();
                if (mc == null)
                {
                    mc = child.gameObject.AddComponent<MeshCollider>();
                }
                mc.convex = true;

                // Add Rigidbody if missing
                if (child.GetComponent<Rigidbody>() == null)
                {
                    Rigidbody rb = child.gameObject.AddComponent<Rigidbody>();
                    rb.mass = mass;
                    rb.linearDamping = drag;
                    rb.angularDamping = angularDrag;
                    count++;
                }
            }

            EditorUtility.DisplayDialog("Done",
                $"Added Rigidbodies to {count} fragments with convex colliders.", "OK");

            EditorUtility.SetDirty(fragmentParent);
        }

        GUILayout.Space(4);

        if (GUILayout.Button("Fix convex on existing colliders"))
        {
            if (fragmentParent == null) return;

            int count = 0;
            foreach (Transform child in fragmentParent.transform)
            {
                MeshCollider mc = child.GetComponent<MeshCollider>();
                if (mc == null)
                {
                    mc = child.gameObject.AddComponent<MeshCollider>();
                    count++;
                }
                if (!mc.convex)
                {
                    mc.convex = true;
                    count++;
                }
            }

            EditorUtility.DisplayDialog("Done",
                $"Fixed convex on {count} MeshColliders.", "OK");

            EditorUtility.SetDirty(fragmentParent);
        }

        GUILayout.Space(4);

        if (GUILayout.Button("Disable all Rigidbodies (for prefab storage)"))
        {
            if (fragmentParent == null) return;

            foreach (Transform child in fragmentParent.transform)
            {
                Rigidbody rb = child.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;
            }
        }
    }
}
