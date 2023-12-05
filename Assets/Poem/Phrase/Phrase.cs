using UnityEngine;

#if UNITY_EDITOR
using System.Threading;
#endif

namespace Poem {

/// a phrase in a text
sealed class Phrase: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip(".")]
    [SerializeField] string m_Text;

    // -- refs --
    [Header("refs")]
    [Tooltip("the persistent store")]
    [SerializeField] Store m_Store;

    // -- props --
    /// the expected text, if any
    string m_Expected;

    /// the current sensor of this phrase, if any
    SensedPhrase m_Accepted;

    /// the attached collider
    Collider m_Collider;

    // -- lifecycle --
    void Awake() {
        // bind events
        m_Store.OnLoad(OnStoreLoad);
        m_Collider = GetComponent<Collider>();
    }

    // -- commands --
    /// accept perception from a particular sensor
    public void Accept(SensedPhrase sensed) {
        m_Accepted = sensed;
    }

    /// become unperceived
    public void Reset() {
        m_Accepted = null;
    }

    // -- c/edit
    /// expect the character to appear
    public void Expect(char ch) {
        if (m_Expected == null) {
            m_Expected = "";
        }

        var curr = m_Expected;
        var code = (KeyCode)ch;

        m_Expected = code switch {
            KeyCode.Delete or KeyCode.Backspace =>
                curr.Substring(0, Mathf.Max(curr.Length - 1, 0)),
            > KeyCode.Space and < KeyCode.Delete =>
                curr + ch,
            _ => curr
        };
    }

    /// assume the expected text's end
    public void Assume() {
        m_Text = m_Expected;
        m_Expected = null;
        m_Store.SavePhrase(IntoRec());
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

    /// the current sensor of this phrase, if any
    public SensedPhrase Sensed {
        get => m_Accepted;
    }

    /// the closes point on the phrase collider
    public Vector3 ClosestPoint(Vector3 src) {
        return m_Collider.ClosestPoint(src);
    }

    // -- events --
    /// when the store finishes loading the text
    void OnStoreLoad() {
        var rec = m_Store.FindPhrase(Id);
        if (rec != null && !rec.Text.IsEmpty()) {
            m_Text = rec.Text;
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
        var text = m_Text;
        var textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
        gameObject.name = textInfo.ToTitleCase(text);
    }
    #endif
}

}