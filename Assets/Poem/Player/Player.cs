using UnityEngine;
using UnityEngine.InputSystem;

namespace Poem {

/// .
sealed class Player: MonoBehaviour {
    // -- statics --
    /// .
    static int s_PoemMask = -1;

    // -- tuning --
    [Header("tuning")]
    [Tooltip("the distance the player can sense")]
    [SerializeField] EaseCurve m_SenseDist;

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

    [Tooltip("the input actions")]
    [SerializeField] InputActionAsset m_Input;

    [Tooltip("the write action ref")]
    [SerializeField] InputActionReference m_Write;

    [Tooltip(".")]
    [SerializeField] Config m_Config;

    // -- props --
    /// a buffer for phrase raycast hits
    RaycastHit[] m_Hits;

    /// the movement action amap
    InputActionMap m_Movement;

    /// .
    Keyboard m_Keyboard;

    // -- lifecycle --
    void Awake() {
        if (s_PoemMask == -1) {
            s_PoemMask = LayerMask.GetMask("Poem");
        }

        // set props
        m_Hits = new RaycastHit[m_Config.Phrases];
        m_Keyboard = Keyboard.current;
        m_Movement = m_Input.FindActionMap("Player", true);

        // bind events
        m_Write.action.actionMap.Enable();
        m_Write.action.performed += OnWrite;
    }

    void Update() {
        var sense = SenseRay();

        // sense range, higher when looking up
        var range = m_SenseDist.Evaluate(Mathf.Max(Vector3.Dot(sense.direction, Vector3.up), 0f));

        // cast for phrases
        var count = Physics.RaycastNonAlloc(
            sense,
            m_Hits,
            range,
            s_PoemMask,
            QueryTriggerInteraction.Ignore
        );

        // and show them
        m_Sensed.Accept(count, m_Hits, sense);
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
    /// .
    void StartWriting() {
        m_Movement.Disable();
        m_Keyboard.onTextInput += OnTextInput;
    }

    /// .
    void Write(char ch) {
        m_Sensed.Expect(ch);
    }

    /// .
    void StopWriting() {
        m_Movement.Enable();
        m_Keyboard.onTextInput -= OnTextInput;
        m_Sensed.Assume();
    }

    // -- queries --
    /// the ray for the sense cast
    Ray SenseRay() {
        return m_Sensor.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
    }

    /// .
    bool IsWriting {
        get => !m_Movement.enabled;
    }

    // -- events --
    /// when the write input fires
    void OnWrite(InputAction.CallbackContext action) {
        if (!m_Sensed.Any) {
            return;
        }

        if (IsWriting) {
            StopWriting();
        } else {
            StartWriting();
        }
    }

    /// when the keyboard input fires
    void OnTextInput(char ch) {
        Write(ch);
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
