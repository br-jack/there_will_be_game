using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//assigns rigidbodies to all children of a Fragmented Object for the purpose of havign desctructible structures

public class FragmentSetup : EditorWindow
{
    private GameObject fragmentParent;
    private float mass = 1f;
    private float drag = 0.5f;
    private float angularDrag = 0.5f;
    private GameObject sourcePrefab;

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
            int count1 = 0;
            int count2 = 0;
            foreach (Transform child in fragmentParent.transform)
            {
                BoxCollider bc = child.GetComponent<BoxCollider>();
                if (bc != null)
                    DestroyImmediate(bc);
                    count1++;

                MeshCollider mc = child.GetComponent<MeshCollider>();
                if (mc == null)
                {
                    mc = child.gameObject.AddComponent<MeshCollider>();
                    count2++;
                }
                mc.convex = true;

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
                $"Added Rigidbodies to {count} fragments with convex colliders. \n added {count1} meshcolldiers from fragments. \n removed {count2} box colliders to fragments.", "OK");

            EditorUtility.SetDirty(fragmentParent);
        }

        GUILayout.Space(4);

        if (GUILayout.Button("Add Rigidbodies to all children Box colldier instead (swap if have mesh)"))
        {
            if (fragmentParent == null)
            {
                EditorUtility.DisplayDialog("No object selected",
                    "Please assign a Fragment Parent Object first.", "OK");
                return;
            }

            int count = 0;
            int count1 = 0;
            int count2 = 0;
            foreach (Transform child in fragmentParent.transform)
            {
                // Fix MeshCollider convex first
                MeshCollider mc = child.GetComponent<MeshCollider>();
                if (mc != null)
                    DestroyImmediate(mc);
                    count1++;

                if (child.GetComponent<BoxCollider>() == null)
                    child.gameObject.AddComponent<BoxCollider>();
                    count2++;

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
                $"Added Rigidbodies to {count} fragments. \n removed {count1} meshcolldiers from fragments. \n Added {count2} box colliders to fragments.", "OK");

            EditorUtility.SetDirty(fragmentParent);
        }

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

        GUILayout.Space(12);
        GUILayout.Label("Material Copying", EditorStyles.boldLabel);

        sourcePrefab = (GameObject)EditorGUILayout.ObjectField(
            "Original Prefab", sourcePrefab, typeof(GameObject), true);

        if (GUILayout.Button("Copy materials from original"))
        {
            if (fragmentParent == null || sourcePrefab == null)
            {
                EditorUtility.DisplayDialog("Missing reference",
                    "Please assign both the Fragment Parent and Original Prefab.", "OK");
                return;
            }

            // Collect all materials from the original prefab
            List<Material> sourceMaterials = new List<Material>();
            foreach (MeshRenderer mr in sourcePrefab.GetComponentsInChildren<MeshRenderer>())
            {
                foreach (Material m in mr.sharedMaterials)
                {
                    if (m != null && !sourceMaterials.Contains(m))
                        sourceMaterials.Add(m);
                }
            }

            if (sourceMaterials.Count == 0)
            {
                EditorUtility.DisplayDialog("No materials found",
                    "The original prefab has no materials to copy.", "OK");
                return;
            }

            // Apply to every fragment child
            int count = 0;
            foreach (Transform child in fragmentParent.transform)
            {
                MeshRenderer mr = child.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    mr.sharedMaterials = sourceMaterials.ToArray();
                    count++;
                    EditorUtility.SetDirty(child.gameObject);
                }
            }

            EditorUtility.DisplayDialog("Done",
                $"Applied {sourceMaterials.Count} material(s) to {count} fragments.", "OK");

            EditorUtility.SetDirty(fragmentParent);
        }

        if (GUILayout.Button("Swap material slots on all fragments"))
        {
            int count = 0;
            foreach (Transform child in fragmentParent.transform)
            {
                MeshRenderer mr = child.GetComponent<MeshRenderer>();
                if (mr != null && mr.sharedMaterials.Length >= 2)
                {
                    Material[] mats = mr.sharedMaterials;
                    Material temp = mats[0];
                    mats[0] = mats[1];
                    mats[1] = temp;
                    mr.sharedMaterials = mats;
                    count++;
                    EditorUtility.SetDirty(child.gameObject);
                }
            }
            EditorUtility.DisplayDialog("Done",
                $"Swapped material slots on {count} fragments.", "OK");
            EditorUtility.SetDirty(fragmentParent);
        }
    }
}
