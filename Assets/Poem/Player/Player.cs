using TMPro;
using UnityEngine;

namespace Poem {

class Player: MonoBehaviour {
    // -- statics --
    /// .
    static int s_PoemMask = -1;

    // -- refs --
    [Header("refs")]
    [Tooltip("the player camera")]
    [SerializeField] Camera m_Look;

    [Tooltip("the player's sense of the poerty")]
    [SerializeField] TMP_Text m_Poem;

    // -- props --
    /// a buffer of hit phrases
    RaycastHit[] m_Hits = new RaycastHit[10];

    // -- lifecycle --
    void Awake() {
        if (s_PoemMask == -1) {
            s_PoemMask = LayerMask.GetMask("Poem");
        }
    }

    void Update() {
        // cast for a phrase
        var hits = Physics.RaycastNonAlloc(
            m_Look.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)),
            m_Hits,
            float.MaxValue,
            s_PoemMask,
            QueryTriggerInteraction.Ignore
        );

        // find the closest one
        var phrase = null as Phrase;
        if (hits > 0) {
            var closest = m_Hits[0];
            for (var i = 0; i < hits; i++) {
                var hit = m_Hits[i];
                if (hit.distance < closest.distance) {
                    closest = hit;
                }
            }

            phrase = closest.collider.GetComponent<Phrase>();
        }

        // and read it
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

    // -- debug --
    #if UNITY_EDITOR
    /// -- d/gizmos
    void OnDrawGizmos() {
        var look = m_Look.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        var src = look.origin;
        var dst = look.GetPoint(2f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(src, 0.1f);
        Gizmos.DrawLine(src, dst);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(dst, 0.1f);
    }
    #endif
}

}
