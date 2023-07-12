using UnityEngine;
using UnityEditor;
using System.Reflection;

using E = UnityEditor.EditorGUI;
using U = UnityEditor.EditorGUIUtility;

namespace Poem.Editor {

[CustomPropertyDrawer(typeof(EaseVec2))]
sealed class EaseVec2Drawer: PropertyDrawer {
    // -- constants --
    /// the gap between elements
    const float k_Gap1 = 2f;
    const float k_Gap2 = 6f;

    /// the width of the curve
    const float k_CurveWidth = 40f;

    /// the separator style
    static GUIStyle s_Separator;

    // -- commands --
    /// .
    void Init() {
        if (s_Separator != null) {
            return;
        }

        s_Separator = new GUIStyle(GUI.skin.label);
        s_Separator.alignment = TextAnchor.MiddleCenter;
    }

    // -- lifecycle --
    public override void OnGUI(Rect r, SerializedProperty prop, GUIContent label) {
        Init();

        E.BeginProperty(r, label, prop);
        var r0 = r;

        // get attrs
        var timer = prop.FindPropertyRelative("m_Timer");

        // draw label w/ indent
        E.LabelField(r, label);

        // reset indent so that it doesn't affect inline fields
        var indent = E.indentLevel;
        E.indentLevel = 0;

        // move rect past the label
        var lw = U.labelWidth + k_Gap1;
        r.x += lw;
        r.width -= lw;

        var obj = prop.serializedObject.targetObject;
        var field = obj.GetType().GetField(prop.name, BindingFlags.Instance | BindingFlags.NonPublic);
        var value = field.GetValue(obj);
        Debug.Log($"field {value}");

        // // draw the curve
        // var rc = r;
        // rc.width = k_CurveWidth;
        // rc.y -= 1;
        // rc.height += 1;
        // curve.animationCurveValue = E.CurveField(rc, curve.animationCurveValue);

        // // draw the range
        // var delta = rc.width + k_Gap2;
        // r.x += delta;
        // r.width -= delta;
        // FloatRangeDrawer.DrawInput(r, range);

        // reset indent level
        E.indentLevel = indent;

        // draw the timer as a nested property

        // E.PropertyField(r0, timer, includeChildren: true);
        E.EndProperty();

    }
}

}