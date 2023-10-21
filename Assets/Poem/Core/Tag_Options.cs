namespace Poem {

/// a set of tag options
partial record Tag {
    // -- options --
    public static Tag Store = new Tag("store", "#00aabb");
    public static Tag Audio = new Tag("audio", "#bbccaa");
    public static Tag Sense = new Tag("sense", "#ff00ff");
    public static Tag Playr = new Tag("playr", "#aaffaa");
}

}