using UnityEngine;

namespace Poem {

/// a phrase in a poem
class Phrase: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip(".")]
    [SerializeField] string m_Text;

    // -- queries --
    /// .
    public string Text {
        get => m_Text;
    }
}

}