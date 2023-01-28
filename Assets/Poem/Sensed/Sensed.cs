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

    [Tooltip(".")]
    [SerializeField] Config m_Config;

    // -- props --
    /// the nearest phrase, if any
    Phrase m_Nearest;

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
    public void Accept(int count, RaycastHit[] hits, Ray sense) {
        // clear senses
        m_Nearest = null;
        for (var i = 0 ; i < m_Phrases.Length; i++) {
            m_Phrases[i].Clear();
        }

        // sort hits by distance
        Array.Sort<RaycastHit>(hits, 0, count, new CompareByDistance());

        // share storage for the hit
        var args = new SensedPhrase.Hit();

        // sense the phrases
        var j = 0;
        for (var i = 0 ; i < count; i++) {
            // find the hit
            var hit = hits[i];
            var t = hit.transform;

            // find the hit phrase
            var phrase = t.GetComponent<Phrase>();
            if (phrase == null) {
                Debug.LogError($"[player] hit {t.name} but it had no phrase");
                continue;
            }

            // track the neareset phrase
            if (i == 0) {
                m_Nearest = phrase;
            }

            // set the hit props
            args.Phrase = phrase;
            args.Distance = hit.distance;
            args.Direction = Vector3.ProjectOnPlane(
                Vector3.Normalize(t.position - sense.origin),
                sense.direction
            );

            // maintain the existing sensor, if any
            if (phrase.Sensed != null) {
                phrase.Sensed.Accept(args);
            }
            // otherwise, find one that is free
            else {
                for (; j < m_Phrases.Length; j++) {
                    var sensed = m_Phrases[j];

                    // the first free sensor accepts this phrase
                    if (sensed.IsFree) {
                        sensed.Accept(args);
                        break;
                    }
                }
            }
        }

        // reset any remaining free sensors
        for (; j < m_Phrases.Length; j++) {
            var sensed = m_Phrases[j];
            if (!sensed.IsAccepting) {
                sensed.Reset();
            }
        }
    }

    /// expect the character to appear
    public void Expect(char ch) {
        if (m_Nearest == null) {
            Debug.LogError($"[sensed] there was no phrase to expect");
            return;
        }

        m_Nearest.Expect(ch);
    }

    /// assume the expected text's end
    public void Assume() {
        m_Nearest.Assume();
    }

    // -- queries --
    /// if there is any sensed phrase
    public bool Any {
        get => m_Nearest != null;
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
