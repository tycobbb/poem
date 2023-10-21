using System;
using UnityEngine;

namespace Poem {

/// a timer w/ a progress curve
[Serializable]
public record EaseTimer {
    // -- constants --
    // the sentinel time for an inacitve timer
    const float k_Inactive = -1f;

    // -- cfg --
    [Tooltip("the timer duration")]
    [SerializeField] float m_Duration;

    [Tooltip("the timer curve")]
    [SerializeField] AnimationCurve m_Curve;

    // -- props --
    /// the elapsed time
    float m_Elapsed;

    /// the uncurved percent through the timer
    float m_RawPct;

    // -- lifetime --
    public EaseTimer(): this(0f) {}
    public EaseTimer(
        float duration,
        AnimationCurve curve = null
    ) {
        m_Elapsed = k_Inactive;
        m_Duration = duration;
        m_Curve = curve;
    }

    // -- commands --
    /// start the timer (optionally, at a particular raw percent)
    public void Start(float pct = 0f, float duration = -1f) {
        if (duration != -1f) {
            m_Duration = duration;
        }

        m_Elapsed = pct * m_Duration;
    }

    /// advance the timer based on current time
    public void Tick() {
        // if not active, abort
        if (m_Elapsed == k_Inactive) {
            return;
        }

        // tick timer
        m_Elapsed += Time.deltaTime;

        // check progress
        var k = m_Elapsed / m_Duration;

        // if complete, clamp and stop the timer
        if (k >= 1f) {
            k = 1f;
            m_Elapsed = k_Inactive;
        }

        // save current progress
        m_RawPct = k;
    }

    // -- queries --
    /// if the timer is active
    public bool IsActive {
        get => m_Elapsed != k_Inactive;
    }

    /// the curved progress
    public float Pct {
        get => PctFrom(m_RawPct);
    }

    /// curve an arbitrary progress pct
    public float PctFrom(float value) {
        if (m_Curve == null || m_Curve.length == 0) {
            return value;
        }

        return m_Curve.Evaluate(value);
    }

    /// the uncurved progress
    public float Raw {
        get => m_RawPct;
    }

    /// the current duration
    public float Duration {
        get => m_Duration;
    }
}

}