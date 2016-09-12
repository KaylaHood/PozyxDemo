using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Animations;

[CustomEditor(typeof(RiderMountLayer))]
public class MountLayerEditor : Editor
{

    private SerializedObject serObj; 
    RiderMountLayer MountedComponent;

    private void OnEnable()
    {
        serObj = new SerializedObject(target);
        MountedComponent = (RiderMountLayer)target;
    }

    public override void OnInspectorGUI()
    {
        serObj.Update();

        DrawDefaultInspector();

        EditorGUILayout.LabelField("ADD MOUNTED LAYER TO CURRENT ANIMATOR (TPCs)");

        if (GUILayout.Button(new GUIContent("Add Mounted Layer", "Used for adding the parameters and Layer from the Mounted Animator to your custom character controller animator ")))
        {
            AddLayerMounted();
        } 
        serObj.ApplyModifiedProperties();

    }

    void AddLayerMounted()
    {
        AnimatorController MountedLayerFile = AssetDatabase.LoadAssetAtPath("Assets/Horse/FBX/Animations/Mounted Layer.controller", typeof(AnimatorController)) as AnimatorController;
        AnimatorControllerLayer MountedLayer = MountedLayerFile.layers[1];


        Animator anim = MountedComponent.GetComponent<Animator>();
        AnimatorController AnimController = (AnimatorController)anim.runtimeAnimatorController;


        int MountedLayer_Index = anim.GetLayerIndex("Mounted");
        //Debug.Log(MountedLayer_Index);

        if (MountedLayer_Index == -1)
        {
            AnimController.AddLayer(MountedLayer);
        }
        UpdateParametersOnAnimator(AnimController);
    }

    // Copy all parameters to the new animator
    void UpdateParametersOnAnimator(AnimatorController AnimController)
    {
        AnimatorControllerParameter[] parameters = AnimController.parameters;

        if (!SearchParameter(parameters, "Horizontal"))
            AnimController.AddParameter("Horizontal", AnimatorControllerParameterType.Float);

        if (!SearchParameter(parameters, "Speed"))
            AnimController.AddParameter("Speed", AnimatorControllerParameterType.Float);

        if (!SearchParameter(parameters, "HorseFloat"))
            AnimController.AddParameter("HorseFloat", AnimatorControllerParameterType.Float);

        if (!SearchParameter(parameters, "Jumping"))
            AnimController.AddParameter("Jumping", AnimatorControllerParameterType.Bool);

        if (!SearchParameter(parameters, "FowardPressed"))
            AnimController.AddParameter("FowardPressed", AnimatorControllerParameterType.Bool);

        if (!SearchParameter(parameters, "Galloping"))
            AnimController.AddParameter("Galloping", AnimatorControllerParameterType.Bool);

        if (!SearchParameter(parameters, "Trotting"))
            AnimController.AddParameter("Trotting", AnimatorControllerParameterType.Bool);

        if (!SearchParameter(parameters, "Shift"))
            AnimController.AddParameter("Shift", AnimatorControllerParameterType.Bool);

        if (!SearchParameter(parameters, "Stand"))
            AnimController.AddParameter("Stand", AnimatorControllerParameterType.Bool);

        if (!SearchParameter(parameters, "Mount"))
            AnimController.AddParameter("Mount", AnimatorControllerParameterType.Bool);

        if (!SearchParameter(parameters, "MountSide"))
            AnimController.AddParameter("MountSide", AnimatorControllerParameterType.Bool);

        if (!SearchParameter(parameters, "Death"))
            AnimController.AddParameter("Death", AnimatorControllerParameterType.Trigger);

        if (!SearchParameter(parameters, "IKLeftFoot"))
            AnimController.AddParameter("IKLeftFoot", AnimatorControllerParameterType.Float);

        if (!SearchParameter(parameters, "IKRightFoot"))
            AnimController.AddParameter("IKRightFoot", AnimatorControllerParameterType.Float);

        if (!SearchParameter(parameters, "Swimming"))
            AnimController.AddParameter("Swimming", AnimatorControllerParameterType.Bool);

        if (!SearchParameter(parameters, "Inclination"))
            AnimController.AddParameter("Inclination", AnimatorControllerParameterType.Float);

        if (!SearchParameter(parameters, "Sleep"))
            AnimController.AddParameter("Sleep", AnimatorControllerParameterType.Bool);

        if (!SearchParameter(parameters, "HorseAttack"))
            AnimController.AddParameter("HorseAttack", AnimatorControllerParameterType.Bool);

        if (!SearchParameter(parameters, "HorseInt"))
            AnimController.AddParameter("HorseInt", AnimatorControllerParameterType.Int);

        if (!SearchParameter(parameters, "Falling"))
            AnimController.AddParameter("Falling", AnimatorControllerParameterType.Bool);

        if (!SearchParameter(parameters, "FallingBack"))
            AnimController.AddParameter("FallingBack", AnimatorControllerParameterType.Bool);


    }

    //Search for the parameters on the AnimatorController
    bool SearchParameter(AnimatorControllerParameter[] parameters, string name)
    {
        foreach (AnimatorControllerParameter item in parameters)
        {
            if (item.name == name)
            {
                return true;
            }
        }
        return false;
    }
}
