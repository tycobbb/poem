using System;
using UnityEngine;

namespace Poem {

/// a timer w/ a progress curve
[Serializable]
struct EaseVec2 {
    // -- cfg --
    [Tooltip("the timer")]
    [SerializeField] EaseTimer m_Timer;

    // -- props --
    /// the source value
    Vector2 m_Src;

    /// the destination value
    Vector2 m_Dst;

    // -- lifetime --
    /// start the timer (optionally, at a particular raw percent)
    public void Start(Vector2 src, Vector2 dst, float pct = 0.0f) {
        m_Src = src;
        m_Dst = dst;
        m_Timer.Start();
    }

    /// advance the timer based on current time
    public void Tick() {
        m_Timer.Tick();
    }

    // -- queries --
    /// if the timer is active
    public bool IsActive {
        get => m_Timer.IsActive;
    }

    /// .
    public Vector2 Current {
        get => Vector2.Lerp(m_Src, m_Dst, m_Timer.Pct);
    }

    /// .
    public Vector2 Dst {
        get => m_Dst;
    }
}

}