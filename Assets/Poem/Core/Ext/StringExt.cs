namespace Poem {

static class StringExt {
    /// if this is an empty string
    public static bool IsEmpty(this string str) {
        return str.Length == 0;
    }
}

}