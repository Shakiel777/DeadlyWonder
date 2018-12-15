using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DynamicObject)), CanEditMultipleObjects]
public class DynamicObjectEditor : Editor {

    public SerializedProperty
        prop_dynamicType,
        prop_useType,
        prop_interactType,
        prop_keyType,
        prop_keyid,
        prop_intAnim,
        prop_useAnim,
        prop_backUseAnim,
        prop_ignoreCol,
        prop_useJammPlanks,
        prop_jammPlanks,
        prop_interactEvent,
        prop_disabledEvent,
        prop_lockedText,
        prop_unlockSound,
        prop_lockedTrySound,
        prop_soundVolume,
        prop_doorOpenSnd,
        prop_doorCloseSnd,
        prop_moveX,
        prop_interactPos,
        prop_minMove,
        prop_maxMove,
        prop_reverseMove,
        prop_leverUpSnd,
        prop_upLock,
        prop_angleStop,
        prop_valveRotSnd,
        prop_valveSndAfter,
        prop_valveRotSpeed,
        prop_valveRotTime,
        prop_debugLever,
        prop_debugDoor;

    void OnEnable()
    {
        prop_dynamicType = serializedObject.FindProperty("dynamicType");
        prop_useType = serializedObject.FindProperty("useType");
        prop_interactType = serializedObject.FindProperty("interactType");
        prop_keyType = serializedObject.FindProperty("keyType");
        prop_keyid = serializedObject.FindProperty("keyID");
        prop_intAnim = serializedObject.FindProperty("m_animationObj");
        prop_useAnim = serializedObject.FindProperty("useAnim");
        prop_backUseAnim = serializedObject.FindProperty("backUseAnim");
        prop_ignoreCol = serializedObject.FindProperty("IgnoreColliders");
        prop_useJammPlanks = serializedObject.FindProperty("useJammedPlanks");
        prop_jammPlanks = serializedObject.FindProperty("Planks");
        prop_interactEvent = serializedObject.FindProperty("InteractEvent");
        prop_disabledEvent = serializedObject.FindProperty("DisabledEvent");
        prop_lockedText = serializedObject.FindProperty("CustomLockedText");
        prop_unlockSound = serializedObject.FindProperty("UnlockSound");
        prop_lockedTrySound = serializedObject.FindProperty("LockedTry");
        prop_soundVolume = serializedObject.FindProperty("soundVolume");
        prop_doorOpenSnd = serializedObject.FindProperty("Open");
        prop_doorCloseSnd = serializedObject.FindProperty("Close");
        prop_moveX = serializedObject.FindProperty("moveX");
        prop_interactPos = serializedObject.FindProperty("InteractPos");
        prop_minMove = serializedObject.FindProperty("MinMove");
        prop_maxMove = serializedObject.FindProperty("MaxMove");
        prop_reverseMove = serializedObject.FindProperty("reverseMove");
        prop_leverUpSnd = serializedObject.FindProperty("LeverUpSound");
        prop_upLock = serializedObject.FindProperty("upLock");
        prop_angleStop = serializedObject.FindProperty("angleStop");
        prop_valveRotSnd = serializedObject.FindProperty("valveTurnSounds");
        prop_valveSndAfter = serializedObject.FindProperty("valveSoundAfter");
        prop_valveRotSpeed = serializedObject.FindProperty("valveTurnSpeed");
        prop_valveRotTime = serializedObject.FindProperty("valveTurnTime");
        prop_debugLever = serializedObject.FindProperty("DebugLeverAngle");
        prop_debugDoor = serializedObject.FindProperty("DebugDoorAngle");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(prop_dynamicType);
        DynamicObject.dynamic dynamic = (DynamicObject.dynamic)prop_dynamicType.enumValueIndex;
        DynamicObject.type useType = (DynamicObject.type)prop_useType.enumValueIndex;
        DynamicObject.i_type int_type = (DynamicObject.i_type)prop_interactType.enumValueIndex;
        DynamicObject.key_type keyType = (DynamicObject.key_type)prop_keyType.enumValueIndex;

        switch (dynamic)
        {
            case DynamicObject.dynamic.Door:
                EditorGUILayout.PropertyField(prop_useType, new GUIContent("Use Type"));
                EditorGUILayout.PropertyField(prop_interactType, new GUIContent("Interact Type"));

                if (useType == DynamicObject.type.Locked)
                {
                    EditorGUILayout.PropertyField(prop_keyType, new GUIContent("Key Type"));

                    if (keyType == DynamicObject.key_type.Inventory)
                    {
                        EditorGUILayout.PropertyField(prop_keyid, new GUIContent("Inventory KeyID:"));
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(prop_ignoreCol, new GUIContent("Ignore Colliders"), true);
                EditorGUILayout.PropertyField(prop_lockedText, new GUIContent("Custom Locked Text"));

                if (useType == DynamicObject.type.Jammed)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Jammed Door Planks", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(prop_useJammPlanks, new GUIContent("Use Planks"));
                    EditorGUILayout.PropertyField(prop_jammPlanks, new GUIContent("Planks"), true);
                }

                if (int_type == DynamicObject.i_type.Animation)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(prop_intAnim, new GUIContent("Animation Object"));
                    EditorGUILayout.PropertyField(prop_useAnim, new GUIContent("Open Anim"));
                    EditorGUILayout.PropertyField(prop_backUseAnim, new GUIContent("Close Anim"));
                }

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Door Properties", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(prop_doorOpenSnd, new GUIContent("Open Sound"));
                EditorGUILayout.PropertyField(prop_doorCloseSnd, new GUIContent("Close Sound"));
                EditorGUILayout.PropertyField(prop_unlockSound, new GUIContent("Unlock Sound"));
                EditorGUILayout.PropertyField(prop_lockedTrySound, new GUIContent("Locked Try Sound"));
                EditorGUILayout.PropertyField(prop_soundVolume, new GUIContent("Sound Volume"));
                if(int_type == DynamicObject.i_type.Mouse)
                {
                    EditorGUILayout.PropertyField(prop_debugDoor, new GUIContent("Debug Door Angle"));
                }
                break;
            case DynamicObject.dynamic.Drawer:
                EditorGUILayout.PropertyField(prop_useType, new GUIContent("Use Type"));
                EditorGUILayout.PropertyField(prop_interactType, new GUIContent("Interact Type"));
                EditorGUILayout.PropertyField(prop_keyType, new GUIContent("Key Type"));

                if (useType == DynamicObject.type.Locked)
                {
                    if (keyType == DynamicObject.key_type.Inventory)
                    {
                        EditorGUILayout.PropertyField(prop_keyid, new GUIContent("Inventory KeyID:"));
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(prop_ignoreCol, new GUIContent("Ignore Colliders"), true);
                EditorGUILayout.PropertyField(prop_lockedText, new GUIContent("Custom Locked Text"));

                if (int_type == DynamicObject.i_type.Animation)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(prop_intAnim, new GUIContent("Animation Object"));
                    EditorGUILayout.PropertyField(prop_useAnim, new GUIContent("Open Anim"));
                    EditorGUILayout.PropertyField(prop_backUseAnim, new GUIContent("Close Anim"));
                }

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Drawer Properties", EditorStyles.boldLabel);

                if (int_type == DynamicObject.i_type.Mouse)
                {
                    EditorGUILayout.PropertyField(prop_unlockSound, new GUIContent("Unlock Sound"));
                    EditorGUILayout.PropertyField(prop_lockedTrySound, new GUIContent("Locked Try Sound"));
                    EditorGUILayout.PropertyField(prop_minMove, new GUIContent("Min Move"));
                    EditorGUILayout.PropertyField(prop_maxMove, new GUIContent("Max Move"));
                    EditorGUILayout.PropertyField(prop_moveX, new GUIContent("Move With X"));
                    EditorGUILayout.PropertyField(prop_reverseMove, new GUIContent("Reverse Move"));
                }
                else
                {
                    EditorGUILayout.PropertyField(prop_doorOpenSnd, new GUIContent("Open Sound"));
                    EditorGUILayout.PropertyField(prop_doorCloseSnd, new GUIContent("Close Sound"));
                    EditorGUILayout.PropertyField(prop_unlockSound, new GUIContent("Unlock Sound"));
                    EditorGUILayout.PropertyField(prop_lockedTrySound, new GUIContent("Locked Try Sound"));
                }
                EditorGUILayout.PropertyField(prop_soundVolume, new GUIContent("Sound Volume"));
                break;
            case DynamicObject.dynamic.Lever:
                EditorGUILayout.PropertyField(prop_interactType, new GUIContent("Interact Type"));
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(prop_ignoreCol, new GUIContent("Ignore Colliders"), true);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(prop_lockedText, new GUIContent("Custom Locked Text"));

                if (int_type == DynamicObject.i_type.Animation)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(prop_intAnim, new GUIContent("Animation Object"));
                    EditorGUILayout.PropertyField(prop_useAnim, new GUIContent("SwitchUp Anim"));
                    EditorGUILayout.PropertyField(prop_backUseAnim, new GUIContent("SwitchDown Anim"));
                }

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Lever Properties", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(prop_interactEvent, new GUIContent("Interact Event"));
                EditorGUILayout.PropertyField(prop_disabledEvent, new GUIContent("Disabled Event"));
                EditorGUILayout.PropertyField(prop_leverUpSnd, new GUIContent("Lever Up Sound"));
                EditorGUILayout.PropertyField(prop_upLock, new GUIContent("Up Lock"));
                EditorGUILayout.PropertyField(prop_reverseMove, new GUIContent("Reverse Rotation"));
                if (int_type == DynamicObject.i_type.Mouse)
                {
                    EditorGUILayout.PropertyField(prop_angleStop, new GUIContent("Lever Stop Angle"));
                    EditorGUILayout.PropertyField(prop_debugLever, new GUIContent("Debug Lever Angle"));
                }
                EditorGUILayout.PropertyField(prop_soundVolume, new GUIContent("Sound Volume"));
                break;
            case DynamicObject.dynamic.Valve:
                EditorGUILayout.PropertyField(prop_ignoreCol, new GUIContent("Ignore Colliders"), true);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(prop_interactEvent, new GUIContent("Interact Event"));

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Valve Properties", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(prop_valveRotSnd, new GUIContent("Turn Sounds"), true);
                EditorGUILayout.PropertyField(prop_soundVolume, new GUIContent("Turn Volume"));
                EditorGUILayout.PropertyField(prop_valveSndAfter, new GUIContent("Turn Sound After"));
                EditorGUILayout.PropertyField(prop_valveRotSpeed, new GUIContent("Turn Speed"));
                EditorGUILayout.PropertyField(prop_valveRotTime, new GUIContent("Turn Time"));
                break;
            case DynamicObject.dynamic.MovableInteract:
                EditorGUILayout.PropertyField(prop_ignoreCol, new GUIContent("Ignore Colliders"), true);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(prop_interactEvent, new GUIContent("Interact Event"));

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Movable Interact Properties", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(prop_minMove, new GUIContent("Min Move"));
                EditorGUILayout.PropertyField(prop_maxMove, new GUIContent("Max Move"));
                EditorGUILayout.PropertyField(prop_interactPos, new GUIContent("Interact Position"));
                EditorGUILayout.PropertyField(prop_moveX, new GUIContent("Move With X"));
                EditorGUILayout.PropertyField(prop_reverseMove, new GUIContent("Reverse Move"));
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
