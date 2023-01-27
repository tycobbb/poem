namespace Poem {

/// a file read & written by the store
interface StoreFile {
    // -- props --
    /// if this file has data
    bool HasData { get; }
}

}