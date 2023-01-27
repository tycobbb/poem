using UnityEngine;

namespace Poem {

/// .
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
    [Tooltip("the sensed output")]
    [SerializeField] Sensed m_Sensed;

    [Tooltip("the camera")]
    [SerializeField] Camera m_Sensor;

    [Tooltip("the config")]
    [SerializeField] Config m_Config;

    // -- props --
    /// a buffer for phrase raycast hits
    RaycastHit[] m_Hits;

    // -- lifecycle --
    void Awake() {
        if (s_PoemMask == -1) {
            s_PoemMask = LayerMask.GetMask("Poem");
        }

        // set props
        m_Hits = new RaycastHit[m_Config.Phrases];
    }

    void Update() {
        var fwd = SenseRay();

        // cast for phrases
        var count = Physics.RaycastNonAlloc(
            fwd,
            m_Hits,
            m_SenseDist,
            s_PoemMask,
            QueryTriggerInteraction.Ignore
        );

        // and show them
        m_Sensed.Accept(count, m_Hits, src: fwd.origin);
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

    // -- queries --
    /// the ray for the sense cast
    Ray SenseRay() {
        return m_Sensor.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
    }

    // -- debug --
    #if UNITY_EDITOR
    /// -- d/gizmos
    void OnDrawGizmos() {
        var ray = SenseRay();
        var src = ray.origin;
        var dst = ray.GetPoint(2f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(src, 0.1f);
        Gizmos.DrawLine(src, dst);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(dst, 0.1f);
    }
    #endif
}

}
