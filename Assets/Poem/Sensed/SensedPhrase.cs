using TMPro;
using UnityEngine;

namespace Poem {

/// a single phrase sensed by the player
class SensedPhrase: MonoBehaviour {
    /// the state in the text printing process
    enum PrintingState {
        None,
        Delete,
        Write,
    }

    /// a hit phrase
    public ref struct Hit {
        /// .
        public Phrase Phrase;
        /// .
        public float Distance;
        /// .
        public Vector3 Direction;
    }

    // -- tuning --
    [Header("tuning")]
    [Tooltip("the time to print the next character")]
    [SerializeField] EaseTimer m_Print;

    [Tooltip("the eased value for a move")]
    [SerializeField] EaseVec2 m_Move;

    [Tooltip("the radius curve")]
    [SerializeField] EaseCurve m_Radius;

    [Tooltip("the distance range")]
    [SerializeField] FloatRange m_Distance;

    // -- refs --
    [Header("refs")]
    [Tooltip("the phrase text")]
    [SerializeField] TMP_Text m_Label;

    // -- props --
    /// the target string
    string m_DstText = "";

    /// the current printing state
    PrintingState m_PrintingState = PrintingState.None;

    /// if this is accepting a phrase
    bool m_IsAccepting = false;

    /// the accepted phrase
    Phrase m_Accepted;

    // -- lifecycle --
    void Awake() {
        m_Label.text = "";
    }

    void Update() {
        if (m_Print.IsActive) {
            m_Print.Tick();

            if (m_Print.Pct == 1f) {
                PrintOne();
            }
        }

        if (m_Move.IsActive) {
            m_Move.Tick();
            m_Label.rectTransform.anchoredPosition = m_Move.Current;
        }
    }

    // -- commands --
    /// clear your senses
    public void Clear() {
        m_IsAccepting = false;
    }

    /// accept and sense a phrase
    public void Accept(Hit hit) {
        m_IsAccepting = true;

        // accept link if changed
        var accepted = hit.Phrase;
        if (accepted != m_Accepted) {
            Debug.Log($"[sensed] link {accepted} to {this}");
            m_Accepted = hit.Phrase;
            m_Accepted.Accept(this);
        }

        // show the text; print by character if not expecting
        var dstText = accepted.Text;
        if (accepted.IsExpecting) {
            Expect(dstText);
        } else if (dstText != m_DstText) {
            Print(dstText);
        }

        // if the text is the same, do nothing
        if (dstText != m_DstText) {
            Print(dstText);
        }

        // move the label unless it's deleting
        var srcText = m_Label.text;
        if (m_PrintingState != PrintingState.Delete && (!srcText.IsEmpty() || !dstText.IsEmpty())) {
            Move(hit);
        }
    }

    /// reset the sensed phrase
    public void Reset() {
        // if there is text to reset, reset it
        if (!m_DstText.IsEmpty()) {
            Print("");
        }

        // break link between previously sensed phrase
        if (m_Accepted != null) {
            m_Accepted.Reset();
            m_Accepted = null;
        }
    }

    /// expect the text to appear
    void Expect(string text) {
        m_DstText = text;
        m_Label.text = text;
    }

    /// move the label into position
    void Move(Hit hit) {
        var rad = m_Radius.Evaluate(m_Distance.Unlerp(hit.Distance));
        var dst = rad * new Vector2(hit.Direction.x, hit.Direction.y);

        if (m_Move.Dst != dst) {
            var src = m_Label.rectTransform.anchoredPosition;
            m_Move.Start(src, dst);
        }
    }

    // -- c/print
    /// start printing the text
    void Print(string text) {
        m_DstText = text;

        // and switch to the initial state
        switch (m_Label.text.Length, m_DstText.Length) {
        case (0, 0):
            SwitchToNone(); break;
        case (0, _):
            SwitchToWrite(); break;
        case (_, _):
            SwitchToDelete(); break;
        }

        // start printing if not already
        if (!m_Print.IsActive && m_PrintingState != PrintingState.None) {
            m_Print.Start();
        }
    }

    /// print a single character
    void PrintOne() {
        // print a character and update state
        switch (m_PrintingState) {
        case PrintingState.Delete:
            DeleteOne(); break;
        case PrintingState.Write:
            WriteOne(); break;
        }

        // continue until finished
        if (m_PrintingState != PrintingState.None) {
            m_Print.Start();
        }
    }

    // -- c/delete
    /// start deleting the current phrase
    void SwitchToDelete() {
        m_PrintingState = PrintingState.Delete;
    }

    /// delete a character and switch state once empty
    void DeleteOne() {
        // delete one character
        var curr = m_Label.text;
        var next = curr.Substring(0, curr.Length - 1);
        m_Label.text = next;

        // switch to the next state once empty
        switch (next.Length, m_DstText.Length) {
        case (0, 0):
            SwitchToNone(); break;
        case (0, _):
            SwitchToWrite(); break;
        }
    }

    // -- c/write
    /// start writing the target phrase
    void SwitchToWrite() {
        m_PrintingState = PrintingState.Write;

        // resize the label to fit the new phrase
        var size = m_Label.GetPreferredValues(m_DstText);
        m_Label.rectTransform.sizeDelta = size;
    }

    /// write a character and stop once at target
    void WriteOne() {
        // write one character
        var curr = m_Label.text;
        var next = m_DstText.Substring(0, curr.Length + 1);
        m_Label.text = next;

        // stop printing once at target
        if (next == m_DstText) {
            SwitchToNone();
        }
    }

    // -- c/finish
    /// finish printing the phrase
    void SwitchToNone() {
        m_PrintingState = PrintingState.None;
    }

    // -- queries --
    /// if this is accepting a phrase
    public bool IsAccepting {
        get => m_IsAccepting;
    }
}

}
