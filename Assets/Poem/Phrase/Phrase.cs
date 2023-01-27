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

    // -- props --
    /// the edited text, if any
    string m_Expected;

    // -- lifecycle --
    void Awake() {
        // bind events
        m_Store.OnLoad(OnStoreLoad);
    }

    // -- commands --
    /// expect the character to appear
    public void Expect(char ch) {
        if (m_Expected == null) {
            m_Expected = "";
        }

        m_Expected += ch;
    }

    /// assume the expected text's end
    public void Assume() {
        m_Text = m_Expected;
        m_Expected = null;
    }

    // -- queries --
    /// the object id
    int Id {
        get => transform.GetSiblingIndex();
    }

    /// .
    public string Text {
        get => m_Expected ?? m_Text;
    }

    /// if the phrase is expecting new text
    public bool IsExpecting {
        get => m_Expected != null;
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