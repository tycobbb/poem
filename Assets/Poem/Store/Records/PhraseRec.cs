using UnityEngine;
using System;

namespace Poem {

/// the serialized phrase state
[Serializable]
record PhraseRec {
    // -- props --
    /// the object id
    public int I;

    /// the phrase text
    public string T;

    // -- lifetime --
    [Obsolete("use the paramterized constructor")]
    public PhraseRec() {
    }

    /// create a new record
    public PhraseRec(
        int id,
        string text
    ) {
        I = id;
        T = text;
    }

    // -- queries --
    /// the object id
    public int Id => I;

    /// the phrase text
    public string Text => T;
}

}