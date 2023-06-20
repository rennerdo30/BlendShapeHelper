using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniGLTF;
using UnityEditor;
using UnityEngine;
using VRM;

[CustomEditor(typeof(GameObject))]
public class BelndShapeHelper : Editor
{
    [MenuItem("BSH/Clothing - Static Items")]
    private static void ClothingEditor()
    {
        ClothingDialogWindow window = EditorWindow.GetWindow<ClothingDialogWindow>();
        window.titleContent = new GUIContent("BSH - Clothing");
        window.Show();
    }
}


public class ClothingDialogWindow : EditorWindow
{
    private bool first = true;

    private BSHClothing bSHClothing = null;

    private int selectedModelOption = 0;
    private List<string> modelOptionsStr = new List<string>();
    private Dictionary<string, GameObject> modelMap = new Dictionary<string, GameObject>();
    private GameObject selectedModel = null;

    private List<Material> materials = new List<Material>();


    private int selectedClothingOption = 0;
    private List<ClothingObject> clothingObjects = new List<ClothingObject>();
    private List<string> clothingObjectsStr = new List<string>();

    private bool showNewClothing = false;
    private string newClothingName = string.Empty;
    private Dictionary<Material, bool> newClothingMaterials = new Dictionary<Material, bool>();

    private void OnGUI()
    {
        if (first)
        {
            Refresh();
            first = false;
        }

        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Select VRM Model to Edit:");
        selectedModelOption = EditorGUILayout.Popup(selectedModelOption, modelOptionsStr.ToArray());
        GUILayout.EndHorizontal();

        if (selectedModel != null)
        {
            GUILayout.Label("Selected Model: " + selectedModel.name);

            GUILayout.Space(25);


            GUILayout.Label("Clothings:");
            if (GUILayout.Button("Update All Clothings"))
            {
                UpdateBlendShapes();
            }

            selectedClothingOption = EditorGUILayout.Popup(selectedClothingOption, clothingObjectsStr.ToArray());

            if (GUILayout.Button("Create New Clothing"))
            {
                showNewClothing = true;
                newClothingName = $"BSH - {Random.Range(1000, 9999)}";
                newClothingMaterials = new Dictionary<Material, bool>();
                if (materials.Count > 0)
                {
                    foreach (Material material in materials)
                    {
                        if (!newClothingMaterials.ContainsKey(material))
                        {
                            newClothingMaterials.Add(material, false);
                        }
                    }
                }

                newClothingMaterials = newClothingMaterials.OrderBy(x => x.Key.name).ToDictionary(x => x.Key, x => x.Value);

            }

            if (showNewClothing)
            {
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();
                GUILayout.Label("Name:");
                newClothingName = GUILayout.TextField(newClothingName);
                GUILayout.EndHorizontal();

                GUILayout.Label("Materials:");
                if (materials.Count > 0)
                {

                    for (int i = 0; i < newClothingMaterials.Keys.Count; i++)
                    {
                        var mat = newClothingMaterials.Keys.ElementAt(i);
                        newClothingMaterials[mat] = GUILayout.Toggle(newClothingMaterials[mat], mat.name);

#if false
                        GUILayout.BeginVertical();
                        Rect previewRect = GUILayoutUtility.GetRect(100, 100);
                        EditorGUI.DrawPreviewTexture(previewRect, mat.mainTexture);
                        GUILayout.EndVertical();
#endif
                    }

                }

                if (GUILayout.Button("Save Clothing"))
                {
                    ClothingObject clothingObject = new ClothingObject();
                    clothingObject.name = newClothingName;
                    clothingObject.materials = new List<string>();

                    foreach (var entry in newClothingMaterials)
                    {
                        if (entry.Value)
                        {
                            clothingObject.materials.Add(AssetDatabase.GetAssetPath(entry.Key));
                        }
                    }

                    bSHClothing.clothingModels.Find(cm => cm.model == selectedModel).clothingObjects.Add(clothingObject);
                    EditorUtility.DisplayDialog("New Clothing Created", "New Clothing Entry Created!", "Ok");

                    UpdateBlendShapes();

                    showNewClothing = false;
                }
            }


            if (GUI.changed)
            {
                Refresh();
            }

            GUILayout.EndVertical();
        }
    }

    private void UpdateBlendShapes()
    {
        var objs = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var obj in objs)
        {
            var vrmBlendShapeProxy = obj.GetComponent<VRMBlendShapeProxy>();
            if (vrmBlendShapeProxy == null)
            {
                continue;
            }

            if (bSHClothing == null)
            {
                Debug.Log("clothing is null");
                continue;
            }

            var blendShape = vrmBlendShapeProxy.BlendShapeAvatar;
            var clips = blendShape.Clips;

            string assetPath = AssetDatabase.GetAssetPath(blendShape);
            Debug.Log("Asset Path: " + assetPath);

            if (assetPath == null)
            {
                Debug.Log("No Asset Path found...");
                continue;
            }


            string shapeClipFolder = Path.GetDirectoryName(assetPath) + "/BSH";
            if (!AssetDatabase.IsValidFolder(shapeClipFolder))
            {
                string guid = AssetDatabase.CreateFolder(Path.GetDirectoryName(assetPath), "BSH");
            }

            bSHClothing.clothingModels.Find(cm => cm.model == selectedModel).clothingObjects.ForEach(group =>
            {
                string clipName = group.name + " [BSH]";

                var clip = clips.Find(clp => clp.name == clipName);
                if (clip != null)
                {
                    clips.Remove(clip);
                }

                string filename = shapeClipFolder + "/" + clipName + ".asset";
                AssetDatabase.DeleteAsset(filename.ToUnityRelativePath());
                BlendShapeClip newClip = BlendShapeAvatar.CreateBlendShapeClip(filename.ToUnityRelativePath());
                newClip.name = clipName;

                newClip.MaterialValues = new MaterialValueBinding[group.materials.Count];
                for (int i = 0; i < group.materials.Count; i++)
                {
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(group.materials[i]);

                    Debug.Log("setting " + material.name);

                    var color = material.GetColor("_Color");
                    color.a = 0f;

                    material.SetFloat("_Mode", 1); // Cutout
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_OFF");
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;

                    material.SetColor("_Color", color);
                    

                    newClip.MaterialValues[i].MaterialName = material.name;
                    newClip.MaterialValues[i].ValueName = "_Color";
                    newClip.MaterialValues[i].TargetValue = new Vector4(color.r, color.g, color.b, 255f);
                }

                clips.Add(newClip);
            });
        }
    }

    private void Refresh()
    {
        modelOptionsStr.Clear();
        modelMap.Clear();

        clothingObjects.Clear();
        clothingObjectsStr.Clear();

        var objs = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        {
            foreach (var obj in objs)
            {
                BSHClothing clothing = obj.GetComponent<BSHClothing>();
                if (obj.transform.name == "BSH" && clothing != null)
                {
                    bSHClothing = clothing;
                    break;
                }
            }

            if (bSHClothing == null)
            {
                GameObject obj = new GameObject();
                bSHClothing = obj.AddComponent<BSHClothing>();
            }
        }

        foreach (var obj in objs)
        {
            var vrmMeta = obj.gameObject.GetComponent<VRMMeta>();
            if (vrmMeta != null)
            {
                string name = vrmMeta.Meta.Title + " (" + obj.name + ")";

                modelOptionsStr.Add(name);
                modelMap.Add(name, obj);

                var clMdl = bSHClothing.clothingModels.Find(cg => cg.model == obj.gameObject);
                if (clMdl.Equals(default(ClothingModel)))
                {
                    clMdl = new ClothingModel();
                    clMdl.model = obj;
                    clMdl.clothingObjects = new List<ClothingObject>();

                    bSHClothing.clothingModels.Add(clMdl);
                }
            }
        }

        if (selectedModelOption >= 0 && selectedModelOption < modelOptionsStr.Count)
        {
            materials.Clear();
            var currentModel = modelMap[modelOptionsStr[selectedModelOption]];

            selectedModel = currentModel;

            FindMaterialsRecursive(materials, currentModel);
        }


        ClothingModel clothingModel = bSHClothing.clothingModels.Find(cg => cg.model == selectedModel.gameObject);
        if (clothingModel.Equals(default(ClothingModel)))
        {
            clothingModel = new ClothingModel();
            clothingModel.model = selectedModel;
            clothingModel.clothingObjects = new List<ClothingObject>();

            bSHClothing.clothingModels.Add(clothingModel);
        }

        if (clothingModel.clothingObjects != null)
        {
            foreach (var clothingGroup in clothingModel.clothingObjects)
            {
                clothingObjectsStr.Add(clothingGroup.name);
            }
        }
    }

    private void FindMaterialsRecursive(List<Material> materials, GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material[] l_materials = renderer.sharedMaterials;
            materials.AddRange(l_materials);
        }

        for (int i = 0; i < obj.transform.childCount; i++)
        {
            GameObject child = obj.transform.GetChild(i).gameObject;
            FindMaterialsRecursive(materials, child);
        }
    }
}
