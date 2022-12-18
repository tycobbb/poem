using UnityEngine;

#if UNITY_EDITOR
using System.Threading;
#endif

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

    // -- debug --
    #if UNITY_EDITOR
    [ContextMenu("Sync Name")]
    void SyncName() {
        var text = m_Text.Split(' ')[0];
        var textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
        gameObject.name = textInfo.ToTitleCase(text);
    }
    #endif
}

}