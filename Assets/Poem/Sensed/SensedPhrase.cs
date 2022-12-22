using TMPro;
using UnityEngine;

namespace Poem {

/// a single phrase sensed by the player
class SensedPhrase: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the current alpha")]
    [SerializeField] EaseTimer m_Fade;

    // -- refs --
    [Header("refs")]
    [Tooltip("the phrase text")]
    [SerializeField] TMP_Text m_Label;

    // -- lifecycle --
    void Update() {
        if (m_Fade.IsActive) {
            m_Fade.Tick();
            m_Label.color = Color.Lerp(Color.clear, Color.black, m_Fade.Pct);
        }
    }

    // -- commands --
    public void Accept(Phrase phrase) {
        var text = phrase?.Text ?? "";
        if (text != m_Label.text) {
            m_Fade.Start();
        }

        m_Label.text = phrase?.Text ?? "";
    }
}

}
