﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

[CustomPropertyDrawer(typeof(YwLuaMonoBehaviourParams))]
public class YwLuaMonoBehaviourParamsPropertyDrawer : PropertyDrawer
{
    protected struct MethodMenuEventParam
    {
        public readonly SerializedProperty m_cListener;
        public readonly int m_nEventIdx;

        public MethodMenuEventParam(SerializedProperty cListener, int nEventIdx)
        {
            m_cListener = cListener;
            m_nEventIdx = nEventIdx;
        }
    }

    protected static readonly GUIContent m_cIconToolbarPlus = EditorGUIUtility.IconContent("Toolbar Plus", "Add to list.");
    protected static readonly GUIContent m_cIconToolbarPlusMore = EditorGUIUtility.IconContent("Toolbar Plus More", "Choose to add to list.");
    protected static readonly GUIContent m_cIconToolbarMinus = EditorGUIUtility.IconContent("Toolbar Minus", "Remove selection from list.");

    protected static readonly GUIContent m_cLabelClassName = new GUIContent("Lua Class Name");

    protected static readonly GUIStyle m_cGsHeaderBackground = "RL Header";
    protected static readonly GUIStyle m_cGsBoxBackground = "RL Background";
    protected static readonly GUIStyle m_cGsFooterBackground = "RL Footer";
    protected static readonly GUIStyle m_cGsPreButton = "RL FooterButton";
    protected static readonly GUIStyle m_cGsElementBackground = "RL Element";
    protected static readonly GUIStyle m_cGsElementBackground2 = "OL Box";
    protected static readonly GUIStyle m_cGsInvisibleRemoveButton = "InvisibleButton";

    protected static readonly float m_fSingleLineHeight = EditorGUIUtility.singleLineHeight;
    protected static readonly float m_fSingleLineGap = 3.0f;
    protected static readonly float m_fFooterROffset = 8.0f;
    protected static readonly float m_fFooterROffsetEx = 4.0f;
    protected static readonly float m_fFooterBtnROffset = 25.0f;
    protected static readonly float m_fFooterBtnWidth = 25.0f;
    protected static readonly float m_fFooterBtnHeight = 13.0f;
    protected static readonly float m_fElementLROffset = 6.0f;
    protected static readonly float m_fElementLROffsetEx = 2.0f;
    protected static readonly float m_fElementRemoveBtnSize = 16.0f;
    protected static readonly float m_fElementRemoveBtnLROffset = 22.0f;

    private int m_nControlID = -1;

    public override float GetPropertyHeight(SerializedProperty cProperty, GUIContent cLabel)
    {
        int nMethodsSize = cProperty.FindPropertyRelative("m_cMonoMethods").arraySize;
        int nGameObjSize = cProperty.FindPropertyRelative("m_aParameters").arraySize;
        nGameObjSize = (nGameObjSize > 0) ? nGameObjSize : 1;
        return m_fSingleLineHeight +    // Class name label.
            m_fSingleLineGap * 2 +      // Line gap.
            m_fSingleLineHeight +       // Method block header.
            m_fSingleLineGap + (m_fSingleLineHeight + m_fSingleLineGap) * (nMethodsSize + 1) + m_fSingleLineGap +    // Method block list.
            m_fSingleLineHeight +       // Method block footer.
            m_fSingleLineGap * 2 +      // Line gap.
            m_fSingleLineHeight +       // GameObject Parameters block header.
            m_fSingleLineGap + (m_fSingleLineHeight + m_fSingleLineGap) * nGameObjSize + m_fSingleLineGap +         // GameObject Parameters block list.
            m_fSingleLineHeight;        // GameObject Parameters block footer.
    }

    public override void OnGUI(Rect rcPosition, SerializedProperty cProperty, GUIContent cLabel)
    {
        Rect rcCurrent = OnClassNameGUI(rcPosition, cProperty, cLabel);
        rcCurrent = OnMethodsGUI(rcPosition, rcCurrent, cProperty, cLabel);
        OnParametersGUI(rcPosition, rcCurrent, cProperty, cLabel);
    }

    protected Rect OnClassNameGUI(Rect rcPosition, SerializedProperty cProperty, GUIContent cLabel)
    {
        Rect rcClassName = rcPosition;
        rcClassName.width -= 2.0f;
        rcClassName.height = m_fSingleLineHeight;

        EditorGUI.BeginProperty(rcClassName, cLabel, cProperty);
        EditorGUI.PropertyField(rcClassName, cProperty.FindPropertyRelative("m_strLuaClassName"), m_cLabelClassName);
        EditorGUI.EndProperty();

        return rcClassName;
    }

    protected Rect OnMethodsGUI(Rect rcPosition, Rect rcLastRect, SerializedProperty cProperty, GUIContent cLabel)
    {
        SerializedProperty cMethodList = cProperty.FindPropertyRelative("m_cMonoMethods");
        int nMethodCount = cMethodList.arraySize;
        int nToBeRemovedMethod = -1;
        bool bRemoveAllMethod = false;
        float fElementSingleLineHeight = m_fSingleLineHeight + m_fSingleLineGap;

        Rect rcHeader = new Rect(rcPosition.x, rcLastRect.yMax + m_fSingleLineGap * 2, rcPosition.width, m_fSingleLineHeight);
        Rect rcBoxBackground = new Rect(rcPosition.x, rcHeader.yMax, rcPosition.width, m_fSingleLineGap + fElementSingleLineHeight * (nMethodCount + 1) + m_fSingleLineGap);

        float fFooterBgROffset = rcPosition.xMax - m_fFooterROffset - m_fFooterBtnROffset * 1;
        Rect rcFooterBackground = new Rect(fFooterBgROffset, rcBoxBackground.yMax, rcBoxBackground.xMax - fFooterBgROffset, m_fSingleLineHeight);
        Rect rcFooterBtnAdd = new Rect(fFooterBgROffset + m_fFooterROffsetEx, rcFooterBackground.y - m_fFooterROffsetEx, m_fFooterBtnWidth, m_fFooterBtnHeight);
        Rect rcFooterBtnMinus = new Rect(rcHeader.xMax - m_fFooterROffsetEx - m_fFooterBtnROffset, rcHeader.y, m_fFooterBtnWidth, m_fFooterBtnHeight);
        Rect rcMethodArea = new Rect(rcHeader.x, rcHeader.y, rcHeader.width, rcFooterBackground.yMax - rcHeader.y);

        // Draw method gui style.
        DrawGUIStyle(m_cGsHeaderBackground, rcHeader, false, false, false, false);
        DrawGUIStyle(m_cGsBoxBackground, rcBoxBackground, false, false, false, false);
        DrawGUIStyle(m_cGsFooterBackground, rcFooterBackground, false, false, false, false);

        EditorGUI.BeginProperty(rcMethodArea, cLabel, cProperty);

        // Draw header string.
        Rect rcHeaderLabel = new Rect(rcHeader.x + m_fElementLROffset, rcHeader.y, rcHeader.width - m_fElementLROffset * 2, rcHeader.height);
        EditorGUI.LabelField(rcHeaderLabel, "Select MonoBehaviour event for Lua.");

        EditorGUI.BeginDisabledGroup(nMethodCount <= 0);
        if (GUI.Button(rcFooterBtnMinus, m_cIconToolbarMinus, m_cGsPreButton))
        {
            bRemoveAllMethod = true;
        }

        EditorGUI.EndDisabledGroup();

        // Draw method list.
        Rect rcElementBg = new Rect(rcBoxBackground.x, rcBoxBackground.y + m_fSingleLineGap, rcBoxBackground.width - m_fElementLROffsetEx, fElementSingleLineHeight);
        Rect rcElementLabel = new Rect(rcBoxBackground.x + m_fElementLROffset, rcBoxBackground.y + m_fSingleLineGap, rcBoxBackground.width - m_fElementLROffset, fElementSingleLineHeight);
        DrawGUIStyle(m_cGsElementBackground2, rcElementBg, false, false, false, false);
        EditorGUI.LabelField(rcElementLabel, "Awake (Built-in)");

        for (int i = 0; i < nMethodCount; i++)
        {
            Color clrBg = GUI.backgroundColor;
            GUI.backgroundColor = Color.black;
            GUI.Box(rcElementBg, "");
            GUI.backgroundColor = clrBg;

            rcElementBg.y = rcElementBg.yMax;
            rcElementLabel.y = rcElementLabel.yMax;

            EditorGUI.LabelField(rcElementLabel, Enum.GetName(typeof(YwLuaMonoBehaviourParams.EMonoMethod), cMethodList.GetArrayElementAtIndex(i).enumValueIndex));
            GUI.Button(new Rect(rcElementBg.xMax - m_fElementRemoveBtnLROffset, rcElementBg.y + m_fSingleLineGap, m_fElementRemoveBtnSize, m_fElementRemoveBtnSize), m_cIconToolbarMinus, m_cGsInvisibleRemoveButton);
        }

        if (GUI.Button(rcFooterBtnAdd, m_cIconToolbarPlusMore, m_cGsPreButton))
        {
            ShowMethodEventMenu(cMethodList);
        }

        if (nToBeRemovedMethod > -1)
        {

        }

        if (bRemoveAllMethod)
        {
            cMethodList.ClearArray();
        }

        EditorGUI.EndProperty();

        return rcMethodArea;
    }

    protected Rect OnParametersGUI(Rect rcPosition, Rect rcLastRect, SerializedProperty cProperty, GUIContent cLabel)
    {
        SerializedProperty cGameObjList = cProperty.FindPropertyRelative("m_aParameters");
        bool bListIsEmpty = cGameObjList.arraySize <= 0;
        int nGameObjCount = bListIsEmpty ? 1 : cGameObjList.arraySize;
        int nToBeRemovedGameObj = -1;
        bool bRemoveAllGameObj = false;
        float fElementSingleLineHeight = m_fSingleLineHeight + m_fSingleLineGap;

        Rect rcHeader = new Rect(rcPosition.x, rcLastRect.yMax + m_fSingleLineGap * 2, rcPosition.width, m_fSingleLineHeight);
        Rect rcBoxBackground = new Rect(rcPosition.x, rcHeader.yMax, rcPosition.width, m_fSingleLineGap + fElementSingleLineHeight * nGameObjCount + m_fSingleLineGap);

        float fFooterBgROffset = rcPosition.xMax - m_fFooterROffset - m_fFooterBtnROffset * 1;
        Rect rcFooterBackground = new Rect(fFooterBgROffset, rcBoxBackground.yMax, rcBoxBackground.xMax - fFooterBgROffset, m_fSingleLineHeight);
        Rect rcFooterBtnAdd = new Rect(fFooterBgROffset + m_fFooterROffsetEx, rcFooterBackground.y - m_fFooterROffsetEx, m_fFooterBtnWidth, m_fFooterBtnHeight);
        Rect rcFooterBtnMinus = new Rect(rcHeader.xMax - m_fFooterROffsetEx - m_fFooterBtnROffset, rcHeader.y, m_fFooterBtnWidth, m_fFooterBtnHeight);
        Rect rcMethodArea = new Rect(rcHeader.x, rcHeader.y, rcHeader.width, rcFooterBackground.yMax - rcHeader.y);

        // Draw method gui style.
        DrawGUIStyle(m_cGsHeaderBackground, rcHeader, false, false, false, false);
        DrawGUIStyle(m_cGsBoxBackground, rcBoxBackground, false, false, false, false);
        DrawGUIStyle(m_cGsFooterBackground, rcFooterBackground, false, false, false, false);

        EditorGUI.BeginProperty(rcMethodArea, cLabel, cProperty);

        // Draw header string.
        Rect rcHeaderLabel = new Rect(rcHeader.x + m_fElementLROffset, rcHeader.y, rcHeader.width - m_fElementLROffset * 2, rcHeader.height);
        EditorGUI.LabelField(rcHeaderLabel, "Select GameObjects for Lua.");

        EditorGUI.BeginDisabledGroup(bListIsEmpty);
        if (GUI.Button(rcFooterBtnMinus, m_cIconToolbarMinus, m_cGsPreButton))
        {
            bRemoveAllGameObj = true;
        }

        EditorGUI.EndDisabledGroup();

        // Draw method list.
        Rect rcElementBg = new Rect(rcBoxBackground.x, rcBoxBackground.y + m_fSingleLineGap, rcBoxBackground.width - m_fElementLROffsetEx, fElementSingleLineHeight);
        Rect rcElementLabel = new Rect(rcBoxBackground.x + m_fElementLROffset, rcBoxBackground.y + m_fSingleLineGap, rcBoxBackground.width - m_fElementLROffset, fElementSingleLineHeight);

        if (bListIsEmpty)
        {
            EditorGUI.LabelField(rcElementLabel, "List is empty!");
        }
        else
        {
            for (int i = 0; i < nGameObjCount; i++)
            {
                Color clrBg = GUI.backgroundColor;
                GUI.backgroundColor = Color.black;
                GUI.Box(rcElementBg, "");
                GUI.backgroundColor = clrBg;

                Rect rcElementObjField = rcElementLabel;
                rcElementObjField.width -= m_fFooterBtnWidth * 2;
                rcElementObjField.height = m_fSingleLineHeight;

                EditorGUI.ObjectField(rcElementObjField, new GUIContent("GameObject " + i), cGameObjList.GetArrayElementAtIndex(i).objectReferenceValue, typeof(GameObject), true);
                if (GUI.Button(new Rect(rcElementBg.xMax - m_fElementRemoveBtnLROffset, rcElementBg.y + m_fSingleLineGap, m_fElementRemoveBtnSize, m_fElementRemoveBtnSize), m_cIconToolbarMinus, m_cGsInvisibleRemoveButton))
                {
                    nToBeRemovedGameObj = i;
                }

                rcElementBg.y = rcElementBg.yMax;
                rcElementLabel.y = rcElementLabel.yMax;
            }
        }

        if (GUI.Button(rcFooterBtnAdd, m_cIconToolbarPlus, m_cGsPreButton))
        {
            int nInsertPos = bListIsEmpty ? 0 : nGameObjCount;
            cGameObjList.InsertArrayElementAtIndex(nInsertPos);
            cGameObjList.GetArrayElementAtIndex(nInsertPos).objectReferenceValue = null;
        }

        if (nToBeRemovedGameObj > -1)
        {
            cGameObjList.DeleteArrayElementAtIndex(nToBeRemovedGameObj);
        }

        if (bRemoveAllGameObj)
        {
            cGameObjList.ClearArray();
        }

        EditorGUI.EndProperty();

        return rcMethodArea;
    }

    protected void ShowMethodEventMenu(SerializedProperty cMethodList)
    {
        // Now create the menu, add items and show it.
        GenericMenu cMenu = new GenericMenu();
        Array aMethodEnum = Enum.GetValues(typeof(YwLuaMonoBehaviourParams.EMonoMethod));
        for (int i = 0; i < aMethodEnum.Length; i++)
        {
            bool bActive = true;
            int nEventIndex = (int)aMethodEnum.GetValue(i);

            // Check if we already have a Entry for the current eventType, if so, disable it
            for (int j = 0; j < cMethodList.arraySize; j++)
            {
                SerializedProperty cEvent = cMethodList.GetArrayElementAtIndex(j);
                if (cEvent.enumValueIndex == nEventIndex)
                {
                    bActive = false;
                    break;
                }
            }

            if (bActive)
            {
                cMenu.AddItem(new GUIContent(aMethodEnum.GetValue(i).ToString()), false, MethodEventMenuCallback, new MethodMenuEventParam(cMethodList, nEventIndex));
            }
            else
            {
                cMenu.AddDisabledItem(new GUIContent(aMethodEnum.GetValue(i).ToString()));
            }
        }

        cMenu.ShowAsContext();
        Event.current.Use();
    }

    protected void MethodEventMenuCallback(object cEventParam)
    {
        MethodMenuEventParam cEventInfo = (MethodMenuEventParam)cEventParam;
        int nEventSize = cEventInfo.m_cListener.arraySize;
        int nInsertPos = nEventSize;
        for (int i = nEventSize - 1; i >= 0; i--)
        {
            if (cEventInfo.m_nEventIdx < cEventInfo.m_cListener.GetArrayElementAtIndex(i).enumValueIndex)
            {
                nInsertPos = i;
            }
        }

        cEventInfo.m_cListener.InsertArrayElementAtIndex(nInsertPos);
        cEventInfo.m_cListener.GetArrayElementAtIndex(nInsertPos).enumValueIndex = cEventInfo.m_nEventIdx;
        cEventInfo.m_cListener.serializedObject.ApplyModifiedProperties();
    }

    protected void DrawGUIStyle(GUIStyle cStyle, Rect rcSize, bool bIsHover, bool bIsActive, bool bIsOn, bool bHasKeyboardFocus)
    {
        if (EventType.Repaint != Event.current.type)
        {
            return;
        }

        cStyle.Draw(rcSize, bIsHover, bIsActive, bIsOn, bHasKeyboardFocus);
    }

    protected bool HasKeyboardControl()
    {
        if (-1 == m_nControlID)
        {
            m_nControlID = GUIUtility.GetControlID(FocusType.Keyboard);
        }

        return GUIUtility.keyboardControl == m_nControlID;
    }
}
