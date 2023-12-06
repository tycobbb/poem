using System;
using System.Collections.Generic;
using UnityEngine;

namespace Poem {

/// the poetry currently sensed by the player
sealed class Sensed: MonoBehaviour {
    // -- constants --
    /// the layer for targeting a lingering phrase
    static int s_TargetLayer = -1;

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
        // set statics
        if (s_TargetLayer == -1) {
            s_TargetLayer = LayerMask.NameToLayer("Target");
            Debug.Log($"target layer {s_TargetLayer}");
        }

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
    /// show all the hit phrases
    public void Accept(int count, RaycastHit[] hits, Ray cast) {
        // clear senses
        m_Nearest = null;
        for (var i = 0 ; i < m_Phrases.Length; i++) {
            m_Phrases[i].Clear();
        }

        // sort hits by distance
        Array.Sort(hits, 0, count, new CompareByDistance());

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
                Tag.Playr.I($"hit {t.name} but it had no phrase");
                continue;
            }

            // track the nearest phrase
            if (i == 0) {
                m_Nearest = phrase;
            }

            // set the hit props
            var dir = Vector3.ProjectOnPlane(
                Vector3.Normalize(t.position - cast.origin),
                cast.direction
            );

            args.Phrase = phrase;
            args.Point = hit.point;
            args.Offset.Distance = hit.distance;
            args.Offset.Direction = dir;

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

        // process any remaining sensors
        for (; j < m_Phrases.Length; j++) {
            var sensed = m_Phrases[j];
            if (sensed.IsAccepting) {
                continue;
            }

            // if there's no phrase to update on sensed phrase, reset
            var prevHit = sensed.OutOfRangeHit;
            if (prevHit == null) {
                sensed.Reset();
            }
            // otherwise, something is no longer in cast range, but move it according to current state
            else {
                var dst = Vector3.Distance(
                    prevHit.Point,
                    cast.origin
                );

                var dir = Vector3.ProjectOnPlane(
                    Vector3.Normalize(prevHit.Phrase.transform.position - cast.origin),
                    cast.direction
                );

                args.Offset.Distance = dst;
                args.Offset.Direction = dir;

                sensed.Linger(args.Offset);
            }
        }
    }

    /// expect the character to appear
    public void Expect(char ch) {
        if (m_Nearest == null) {
            Tag.Sense.E("there was no phrase to expect");
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