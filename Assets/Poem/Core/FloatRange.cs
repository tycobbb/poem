using System;
using UnityEngine;

namespace Poem {

/// a value range
[Serializable]
struct FloatRange {
    // -- fields --
    [Tooltip("the min value")]
    [SerializeField] float Min;

    [Tooltip("the max value")]
    [SerializeField] float Max;

    // -- queries --
    /// interpolate between the min & max
    public float Lerp(float k) {
        return Mathf.Lerp(Min, Max, k);
    }

    /// normalize the value between min & max
    public float Unlerp(float val) {
        return Mathf.InverseLerp(Min, Max, val);
    }
}

}