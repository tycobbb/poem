using UnityEngine;

namespace Poem {

/// .
[CreateAssetMenu(fileName = "Config", menuName = "Poem/Config")]
sealed class Config: ScriptableObject {
    // -- fields --
    [Header("fields")]
    [Tooltip("the number of simultaneous phrases")]
    public int Phrases;
}

}
