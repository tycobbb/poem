using UnityEngine;

namespace Poem {

/// .
sealed class App: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("the persistence store")]
    [SerializeField] Store m_Store;

    // -- lifecyle --
    void Awake() {
        // load any persisted state
        m_Store.Load();
    }

    async void OnApplicationQuit() {
        // persist any state
        await m_Store.Save();
    }
}

}