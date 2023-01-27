using UnityEngine;

#if UNITY_EDITOR
using System.Threading;
#endif

namespace Poem {

/// a phrase in a text
class Phrase: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip(".")]
    [SerializeField] string m_Text;

    // -- refs --
    [Header("refs")]
    [Tooltip("the persistent store")]
    [SerializeField] Store m_Store;

    // -- lifecycle --
    void Awake() {
        // bind events
        m_Store.OnLoad(OnStoreLoad);
    }

    // -- queries --
    /// the object id
    int Id {
        get => transform.GetSiblingIndex();
    }

    /// .
    public string Text {
        get => m_Text;
    }

    // -- events --
    /// when the store finishes loading the text
    void OnStoreLoad() {
        var text = m_Store.FindPhraseText(Id);
        if (text != null && !text.IsEmpty()) {
            m_Text = text;
        }
    }

    // -- encoding --
    public PhraseRec IntoRec() {
        return new PhraseRec(
            id:   Id,
            text: m_Text
        );
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