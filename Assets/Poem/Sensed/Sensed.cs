using UnityEngine;

namespace Poem {

/// the poetry currently sensed by the player
class Sensed: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("the parent for spawned phrases")]
    [SerializeField] RectTransform m_PhraseParent;

    [Tooltip("a prefab for a sensed phrase")]
    [SerializeField] SensedPhrase m_PhrasePrefab;

    [Tooltip("the config")]
    [SerializeField] Config m_Config;

    // -- props --
    /// the sensed phrase labels
    SensedPhrase[] m_Phrases;

    // -- lifecycle --
    void Awake() {
        // create phrases
        m_Phrases = new SensedPhrase[m_Config.Phrases];
        for (var i = 0; i < m_Config.Phrases; i++) {
            // set prop
            var phrase = Instantiate(m_PhrasePrefab);
            m_Phrases[i] = phrase;

            // add to hierarchy
            var t = phrase.GetComponent<RectTransform>();
            t.name = $"Phrase.{i}";
            t.SetParent(m_PhraseParent);
            t.localScale = Vector3.one;
            t.anchoredPosition = Vector2.zero;
        }
    }

    // -- commands --
    /// show the first n phrases from the hits
    public void Accept(RaycastHit[] hits, int count) {
        for (var i = 0 ; i < m_Phrases.Length; i++) {
            var sensed = m_Phrases[i];
            if (i >= count) {
                sensed.Accept(null);
            } else {
                var phrase = hits[i].collider.GetComponent<Phrase>();
                sensed.Accept(phrase);
            }
        }
    }
}

}
