using UnityEngine;

namespace Poem {

/// the application
public class App: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("the persistence store")]
    [SerializeField] Store m_Store;

    // -- lifecyle --
    void Awake() {
        m_Store.Load();
    }
}

}