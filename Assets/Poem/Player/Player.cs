using TMPro;
using UnityEngine;

namespace Poem {

class Player: MonoBehaviour {
    // -- statics --
    /// .
    static int s_PoemMask = -1;

    // -- refs --
    [Header("refs")]
    [Tooltip("the direction the player is looking")]
    [SerializeField] Transform m_Look;

    [Tooltip("the player's sense of the poerty")]
    [SerializeField] TMP_Text m_Poem;

    // -- props --
    /// the hit phrase
    RaycastHit[] m_Hits = new RaycastHit[1];

    // -- lifecycle --
    void Awake() {
        if (s_PoemMask == -1) {
            s_PoemMask = LayerMask.GetMask("Poem");
        }
    }

    void Update() {
        // find the phrase
        var hits = Physics.RaycastNonAlloc(
            transform.position,
            m_Look.forward,
            m_Hits,
            float.MaxValue,
            s_PoemMask,
            QueryTriggerInteraction.Ignore
        );

        // and read the phrase
        var phrase = null as Phrase;
        if (hits > 0) {
            phrase = m_Hits[0].transform.GetComponent<Phrase>();
        }

        Read(phrase);
    }

    // -- commands --
    /// read the phrase
    void Read(Phrase phrase) {
        var text = "poem";
        if (phrase != null) {
            text = phrase.Text;
        }

        m_Poem.text = text;
    }
}

}
