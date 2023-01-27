using System;

namespace Poem {

/// the serialized world state
[Serializable]
record PoemRec: StoreFile {
    // -- props --
    /// all the flowers in the world
    public PhraseRec[] Phrases;

    // -- lifetime --
    /// create a record
    public PoemRec() {
    }

    // -- StoreFile --
    /// if this record has any data
    public bool HasData {
        get => Phrases != null;
    }
}

}