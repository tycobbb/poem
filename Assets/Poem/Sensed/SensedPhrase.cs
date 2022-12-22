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

    // -- cfg --
    [Header("cfg")]
    [Tooltip("the time to print the next character")]
    [SerializeField] EaseTimer m_Print;

    // -- refs --
    [Header("refs")]
    [Tooltip("the phrase text")]
    [SerializeField] TMP_Text m_Label;

    // -- props --
    /// the target string
    string m_DstLabel;

    /// the current printing state
    PrintingState m_PrintingState = PrintingState.None;

    // -- lifecycle --
    void Update() {
        if (m_Print.IsActive) {
            m_Print.Tick();

            if (m_Print.Pct == 1f) {
                PrintOne();
            }
        }
    }

    // -- commands --
    /// accept and sense a phrase
    public void Accept(Phrase phrase) {
        // if the text is the same, do nothing
        var dst = phrase?.Text ?? "";
        if (dst == m_DstLabel) {
            return;
        }

        // otherwise, start printing the phrase
        m_DstLabel = dst;

        // and switch to the initial state
        switch (m_Label.text.Length, m_DstLabel.Length) {
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
        switch (next.Length, m_DstLabel.Length) {
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
        var size = m_Label.GetPreferredValues(m_DstLabel);
        m_Label.rectTransform.sizeDelta = size;
    }

    /// write a character and stop once at target
    void WriteOne() {
        // write one character
        var curr = m_Label.text;
        var next = m_DstLabel.Substring(0, curr.Length + 1);
        m_Label.text = next;

        // stop printing once at target
        if (next == m_DstLabel) {
            SwitchToNone();
        }
    }

    // -- c/finish
    /// finish printing the phrase
    void SwitchToNone() {
        m_PrintingState = PrintingState.None;
    }
}

}
