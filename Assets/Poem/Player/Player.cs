using TMPro;
using UnityEngine;

namespace Poem {

class Player: MonoBehaviour {
    // -- statics --
    /// .
    static int s_PoemMask = -1;


    // -- tuning --
    [Header("tuning")]
    [Tooltip("the distance the player can sense")]
    [SerializeField] float m_SenseDist;

    // -- cfg --
    [Header("cfg")]
    [Tooltip("the distance the player wraps at")]
    [SerializeField] float m_WrapDist;

    // -- refs --
    [Header("refs")]
    [Tooltip("the player's sense of the poerty")]
    [SerializeField] TMP_Text m_Poetry;

    [Tooltip("the player camera")]
    [SerializeField] Camera m_Sense;

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
            m_Sense.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)),
            m_Hits,
            m_SenseDist,
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

    void FixedUpdate() {
        var t = transform;

        // wrap around the world
        var pos = t.position;

        var dx = Mathf.Abs(pos.x) - m_WrapDist;
        if (dx > 0f) {
            pos.x = -Mathf.Sign(pos.x) * (m_WrapDist - dx);
        }

        var dz = Mathf.Abs(pos.z) - m_WrapDist;
        if (dz > 0f) {
            pos.z = -Mathf.Sign(pos.z) * (m_WrapDist - dz);
        }

        t.position = pos;
    }

    // -- commands --
    /// read the phrase
    void Read(Phrase phrase) {
        var text = "poem";
        if (phrase != null) {
            text = phrase.Text;
        }

        m_Poetry.text = text;
    }

    // -- debug --
    #if UNITY_EDITOR
    /// -- d/gizmos
    void OnDrawGizmos() {
        var look = m_Sense.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

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
