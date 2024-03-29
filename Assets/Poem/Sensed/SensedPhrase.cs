using TMPro;
using UnityEngine;

namespace Poem {

/// a single phrase sensed by the player
sealed class SensedPhrase: MonoBehaviour {
    /// the state in the text printing process
    enum PrintingState {
        None,
        Delete,
        Write,
    }

    /// a hit phrase
    public record Hit {
        /// .
        public Phrase Phrase;

        /// the last hit point
        public Vector3 Point;

        /// the current hit offset
        public HitOffset Offset;
    }

    /// a hit offset
    public struct HitOffset {
        /// .
        public float Distance;

        /// .
        public Vector2 Direction;
    }

    // -- tuning --
    [Header("tuning")]
    [Tooltip("an eased position for the move")]
    [SerializeField] EaseVec2 m_Move;

    [Tooltip("the time to print the next character")]
    [SerializeField] EaseTimer m_Print;

    [Tooltip("the radius curve by normalized distance")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_Radius")]
    [SerializeField] EaseCurve m_RadiusByDist;

    [Tooltip("the alpha curve by normalized distance")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_Alpha")]
    [SerializeField] EaseCurve m_AlphaByDist;

    [Tooltip("the distance range")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_Distance")]
    [SerializeField] FloatRange m_DistRange;

    // -- audio --
    [Header("audio")]
    [Tooltip("the audio fade")]
    [SerializeField] EaseTimer m_Fade;

    [Tooltip("the pitch offset by direction (colinearity w/ up)")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_PitchOffsetByDirection")]
    [SerializeField] EaseCurve m_PitchByDir;

    [Tooltip("the volume by normalized distance")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_GainByDistance")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_VolumeByDistance")]
    [SerializeField] EaseCurve m_VolumeByDist;

    [Tooltip("the balance by direction (colinearity w/ right)")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_BalanceByDirection")]
    [SerializeField] EaseCurve m_BalanceByDir;

    // -- refs --
    [Header("refs")]
    [Tooltip("the phrase text")]
    [SerializeField] TMP_Text m_Label;

    // -- props --
    /// the target string
    string m_DstText = "";

    /// the current printing state
    PrintingState m_State = PrintingState.None;

    /// if this is accepting a phrase
    bool m_IsAccepting;

    /// the accepted phrase
    Hit m_Accepted = new();

    /// the pct to start the next move from
    float m_NextMovePct;

    // -- lifecycle --
    void Awake() {
        m_Label.text = "";
    }

    void Start() {
        InitAudio();
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

        if (m_Fade.IsActive) {
            m_Fade.Tick();
        }
    }

    void OnAudioFilterRead(float[] data, int channels) {
        PlayAudio(data, channels);
    }

    // -- commands --
    /// clear your senses
    public void Clear() {
        m_IsAccepting = false;
    }

    /// accept and sense a phrase
    public void Accept(Hit hit) {
        m_IsAccepting = true;

        // accept new link to phrase if changed
        var phrase = hit.Phrase;
        if (phrase != m_Accepted.Phrase) {
            Tag.Sense.I($"{name} - accept phrase {m_Accepted.Phrase} -> {phrase}");
            m_Accepted.Phrase = phrase;
            m_Accepted.Phrase.Accept(this);
        }

        // update relative position of accepted phrase
        m_Accepted.Point = hit.Point;
        m_Accepted.Offset = hit.Offset;

        // show the text; print by character if not expecting
        var dstText = phrase.Text;
        if (phrase.IsExpecting) {
            Expect(dstText);
        } else {
            Print(dstText);
        }

        // move the label unless it's deleting
        var srcText = m_Label.text;
        if (m_State != PrintingState.Delete && (!srcText.IsEmpty() || !dstText.IsEmpty())) {
            Move();
        }
    }

    /// update a phrase no longer in sense range but that hasn't vanished yet
    public void Linger(in HitOffset offset) {
        m_Accepted.Offset = offset;
        Move();
    }

    /// reset the sensed phrase
    public void Reset() {
        // if we have no phrase, there's nothing to reset
        if (m_Accepted.Phrase == null) {
            return;
        }

        // if there is text to delete, delete it
        if (!m_DstText.IsEmpty()) {
            Print("");
        }

        // // TODO: this is for debugging and should be deleted
        // if (m_Volume != 0f) {
        //     m_Volume = 0f;
        //     Tag.Audio.I($"{m_Name} - \"{m_Accepted.Phrase.Text}\" - stop");
        // }
    }

    /// expect the text to appear
    void Expect(string text) {
        SwitchToNone();
        m_DstText = text;
        Resize();
        m_Label.text = text;
    }

    /// move the label into position
    void Move() {
        var offset = m_Accepted.Offset;
        var dist = m_DistRange.Unlerp(offset.Distance);

        // move alpha
        m_Label.alpha = m_AlphaByDist.Evaluate(dist);

        // move position
        var rad = m_RadiusByDist.Evaluate(dist);
        var dst = rad * offset.Direction;

        if (m_Move.Dst != dst) {
            var src = m_Label.rectTransform.anchoredPosition;
            m_Move.Start(src, dst, m_NextMovePct);
            m_NextMovePct = 0f;
        }
    }

    /// resize label to fit phrase
    void Resize() {
        var size = m_Label.GetPreferredValues(m_DstText);
        m_Label.rectTransform.sizeDelta = size;
    }

    // -- c/print
    /// start printing the text
    void Print(string text) {
        var curr = m_Label.text;

        // don't switch state when printing the same text
        if (m_DstText == text) {
            // unless we're gonna restart printing on text we're deleting
            // TODO: does this need to start the print timer again? e.g. bottom of this method
            if (m_State == PrintingState.Delete && text != "" && curr != text) {
                SwitchToWrite(shouldResume: true);
            }

            return;
        }

        // otherwise, update the text
        m_DstText = text;

        // and switch to the initial state
        switch (curr.Length, m_DstText.Length) {
        case (0, 0):
            SwitchToNone(); break;
        case (0, _):
            SwitchToWrite(); break;
        case (_, _):
            SwitchToDelete(); break;
        }

        // start printing if not already
        if (!m_Print.IsActive && m_State != PrintingState.None) {
            m_Print.Start();
        }
    }

    /// print a single character
    void PrintOne() {
        // print a character and update state
        switch (m_State) {
        case PrintingState.Delete:
            DeleteOne(); break;
        case PrintingState.Write:
            WriteOne(); break;
        }

        // continue until finished
        if (m_State != PrintingState.None) {
            m_Print.Start();
        }
    }

    // -- c/delete
    /// start deleting the current phrase
    void SwitchToDelete() {
        SwitchTo(PrintingState.Delete);

        // start audio fade out
        // TODO: this needs to consider current fade as well
        var len = 0;
        if (m_Accepted.Phrase) {
            len = m_Accepted.Phrase.Text.Length;
        }

        Tag.Sense.I($"fade out {m_Fade.Raw} -> {1f - m_Fade.Raw}");
        var pct = 1f - m_Fade.Raw;
        m_Fade.Start(
            raw: pct,
            // dur: m_Print.Duration * len
            dur: pct == 0f ? m_Fade.Duration : m_Print.Duration * len
        );
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
            FinishDelete(); break;
        case (0, _):
            SwitchToWrite(); break;
        }
    }

    /// finish deleting the phrase
    void FinishDelete() {
        SwitchToNone();

        // break link between previously sensed phrase
        m_Accepted.Phrase.Reset();
        m_Accepted.Phrase = null;
    }

    // -- c/write
    /// start writing the target phrase
    void SwitchToWrite(bool shouldResume = false) {
        SwitchTo(PrintingState.Write);

        // resize the label to fit the new phrase
        Resize();

        // snap to the initial move position when not resuming from delete
        if (!shouldResume) {
            m_NextMovePct = 1f;
        }

        // start audio fade in
        Tag.Sense.I($"fade inn {m_Fade.Raw} -> {1f - m_Fade.Raw}");
        var len = m_DstText.Length;
        var pct = 1f - m_Fade.Raw;
        m_Fade.Start(
            raw: pct,
            dur: pct == 0f ? m_Fade.Duration : m_Print.Duration * len
        );
    }

    /// write a character and stop once at target
    void WriteOne() {
        // write one character
        var curr = m_Label.text;
        var next = m_DstText[..(curr.Length + 1)];
        m_Label.text = next;

        // stop printing once at target
        if (next == m_DstText) {
            SwitchToNone();
        }
    }

    // -- c/finish
    /// finish printing the phrase
    void SwitchToNone() {
        SwitchTo(PrintingState.None);
    }

    // -- c/state
    /// .
    void SwitchTo(PrintingState next) {
        var curr = m_State;
        if (curr == next) {
            return;
        }

        m_State = next;
    }

    // -- audio --
    const float k_2PI = 2f * Mathf.PI;

    /// the name of the go
    string m_Name;

    /// the sample rate
    int m_SampleRate;

    /// the audio wave phase
    double m_Phase;

    /// if audio is currently active
    bool m_IsAudioActive;

    // -- a/debug
    /// the current fade pct
    float m_FadePct;

    /// the current volume
    float m_Volume;

    /// the current pitch
    float m_Pitch;

    /// the current balance
    float m_Balance;

    // -- a/commands --
    /// .
    void InitAudio() {
        m_Name = name;
        m_SampleRate = AudioSettings.outputSampleRate;
        m_IsAudioActive = true;
    }

    /// .
    void PlayAudio(float[] data, int channels) {
        var isBlank = (
            m_Accepted.Phrase == null &&
            m_State != PrintingState.Delete
        );

        if (!m_IsAudioActive || isBlank) {
            return;
        }

        // precalculate inputs
        var offset = m_Accepted.Offset;
        var dist = m_DistRange.Unlerp(offset.Distance);
        var dirDotUp = Vector2.Dot(offset.Direction, Vector2.up);
        var dirDotRight = Vector2.Dot(offset.Direction, Vector2.right);

        // sample props
        var fade = m_Fade.Pct;
        if (m_State == PrintingState.Delete) {
            fade = 1f - fade;
        }

        var volume = m_VolumeByDist.Evaluate(dist) * fade;
        var pitch = 440 + m_PitchByDir.Evaluate(dirDotUp);
        var balance = m_BalanceByDir.Evaluate(dirDotRight);

        // if (volume != m_Volume || pitch != m_Pitch || balance != m_Balance || fade != m_FadePct) {
        //     Tag.Audio.I($"{m_Name} - \"{m_Accepted.Phrase?.Text}\" - play v {volume} p {pitch} b {balance} f {fade} ({m_FadePct})");
        // }

        m_FadePct = fade;
        m_Volume = volume;
        m_Pitch = pitch;
        m_Balance = balance;

        // ignore audio where volume is 0
        if (volume == 0) {
            return;
        }

        // generate audio
        var delta = pitch * k_2PI / m_SampleRate;
        for (var i = 0; i < data.Length; i += channels) {
            m_Phase += delta;

            var sample = volume * Mathf.Sin((float)(m_Phase));
            for (var c = 0; c < channels; c++) {
                data[i + c] = sample;
            }

            if (m_Phase > k_2PI) {
                m_Phase = 0;
            }
        }
    }

    // -- queries --
    /// if this is free to accept a phrase
    public bool IsFree {
        get => (
            m_Accepted.Phrase == null &&
            m_State == PrintingState.None
        );
    }

    /// if this is accepting a phrase
    public bool IsAccepting {
        get => m_IsAccepting;
    }

    /// the hit out of normal sense range, if any
    public Hit OutOfRangeHit {
        get => m_State == PrintingState.Delete ? m_Accepted : null;
    }
}

}