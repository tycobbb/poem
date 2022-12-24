using System;
using System.Collections.Generic;
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
    public void Accept(int count, RaycastHit[] hits, Vector3 src) {
        // sort hits by distance
        Array.Sort<RaycastHit>(hits, 0, count, new CompareByDistance());

        // share storage for the hit
        var args = new SensedPhrase.Hit();

        // sense the phrases
        for (var i = 0 ; i < m_Phrases.Length; i++) {
            var sensed = m_Phrases[i];
            if (i >= count) {
                sensed.Clear();
                continue;
            }

            var hit = hits[i];
            var t = hit.transform;

            // find the phrase
            args.Phrase = t.GetComponent<Phrase>();
            if (args.Phrase == null) {
                Debug.LogError($"[player] hit {t.name} but it had no phrase");
                continue;
            }

            // set the hit props
            args.Distance = hit.distance;
            args.Direction = Vector3.Normalize(t.position - src);

            // sense the phrase
            sensed.Accept(args);
        }
    }

    // -- helpers --
    /// .
    struct CompareByDistance: IComparer<RaycastHit> {
        int IComparer<RaycastHit>.Compare(RaycastHit l, RaycastHit r) {
            return l.distance.CompareTo(r.distance);
        }
    }
}

}
