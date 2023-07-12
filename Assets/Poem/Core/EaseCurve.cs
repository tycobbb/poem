using System;
using UnityEngine;

namespace Poem {

/// a normalized curve with a min & max value
[Serializable]
public struct EaseCurve {
    // -- fields --
    [Tooltip("the range")]
    [SerializeField] FloatRange m_Range;

    [Tooltip("the easing curve")]
    [SerializeField] AnimationCurve m_Curve;

    // -- queries --
    /// evaluate the curve in the range
    public float Evaluate(float k) {
        if (m_Curve != null && m_Curve.length != 0) {
            k = m_Curve.Evaluate(k);
        }

        return m_Range.Lerp(k);
    }
}

}