using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Poem {

/// the persistence container
sealed class Store: ScriptableObject {
    // -- props --
    /// the current poem on disk
    PoemRec m_Poem;

    /// a map of id to phrase text
    Dictionary<int, string> m_PhraseTextById = new Dictionary<int, string>();

    /// a callback on load
    Action m_OnLoad = () => {};

    // -- commands --
    /// load data from disk
    public async void Load() {
        // ensure we have a directory to read from
        Directory.CreateDirectory(RootPath);

        // load the records
        m_Poem = await LoadRecord<PoemRec>(PoemPath) ?? new PoemRec();

        // build the map
        if (m_Poem.Phrases != null) {
            foreach (var phrase in m_Poem.Phrases) {
                m_PhraseTextById.Add(phrase.Id, phrase.Text);
            }
        }

        // fire the onload callback
        m_OnLoad.Invoke();
        m_OnLoad = null;
    }

    /// add an action that fires once data has loaded
    public void OnLoad(Action action) {
        if (m_OnLoad == null) {
            action.Invoke();
        } else {
            m_OnLoad += action;
        }
    }

    // -- c/syncing
    /// sync the in-memory poem record
    public void SyncPoem() {
        // generate records for each phrase
        var records = FindPhraseObjects()
            .Select((p) => p.IntoRec())
            .ToArray();

        // update world record
        m_Poem.Phrases = records;
    }

    /// save the current state to file
    [ContextMenu("Save Store")]
    public async Task Save() {
        // sync state
        SyncPoem();

        // ensure we have a directory to write to
        Directory.CreateDirectory(RootPath);

        // write the records to disk
        await Task.WhenAll(
            SaveRecord<PoemRec>(PoemPath, m_Poem)
        );
    }

    /// delete the file at the ath
    public void Delete(string path) {
        File.Delete(ResolvePath(path));
    }

    /// copy the file path to clipboard
    public void CopyPath(string path) {
        GUIUtility.systemCopyBuffer = ResolvePath(path);
    }

    /// reset all state
    [ContextMenu("Reset Store")]
    void Reset() {
        File.Delete(PoemPath);
    }

    // -- queries --
    /// get the phrase text, if any, for this id
    public string FindPhraseText(int id) {
        m_PhraseTextById.TryGetValue(id, out var text);
        return text;
    }

    /// resolve a relative path to an absolute one
    string ResolvePath(string path) {
       return Path.Combine(RootPath, path);
    }

    /// find the in-world phrase objects
    Phrase[] FindPhraseObjects() {
        return GameObject
            .FindObjectsOfType<Phrase>();
    }

    // -- io --
    /// the data directory path
    string DataPath {
        #if UNITY_EDITOR
        get => Path.Combine(Application.dataPath, "..", "Artifacts", "data");
        #else
        get => Application.persistentDataPath;
        #endif
    }

    /// the root store path
    string RootPath {
        get => Path.Combine(
            DataPath,
            SceneManager.GetActiveScene().name
        );
    }

    /// the path to the world file
    string PoemPath {
        get => ResolvePath("poem.json");
    }

    /// write the record to disk at path
    async Task SaveRecord<F>(string path, F record) where F : StoreFile {
        // don't save empty files
        if (!record.HasData) {
            return;
        }

        // encode the json
        #if UNITY_EDITOR
        var json = JsonUtility.ToJson(record, true);
        #else
        var json = JsonUtility.ToJson(record);
        #endif

        // write the data to disk, truncating whatever is there
        byte[] data;
        using (var stream = new FileStream(path, FileMode.Create)) {
            data = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(data, 0, data.Length);
        }

        Debug.Log($"[store] saved file @ {RenderPath(path)} => {json}");
    }

    /// load the record from disk at path
    async Task<F> LoadRecord<F>(string path) where F: StoreFile {
        // check for file
        if (!File.Exists(path)) {
            Debug.Log($"[store] no file found @ {RenderPath(path)}");
            return default;
        }

        // read data from file
        byte[] data;
        using (var stream = new FileStream(path, FileMode.Open)) {
            data = new byte[stream.Length];
            var read = await stream.ReadAsync(data, 0, (int)stream.Length);

            if (read != stream.Length) {
                Debug.LogError($"[store] only read {read} of {stream.Length} bytes from file @ {RenderPath(path)}");
                throw new System.Exception("couldn't read the entire file!");
            }
        }

        // decode record from json
        var json = Encoding.UTF8.GetString(data);
        var record = JsonUtility.FromJson<F>(json);
        Debug.Log($"[store] loaded file @ {RenderPath(path)} => {json}");

        return record;
    }

    // -- helpers --
    /// debug; remove the project dir from the path (for display)
    string RenderPath(string path) {
        path = Path.GetFullPath(path);

        // strip project dir from path if necessary
        var dir = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        if (path.StartsWith(dir)) {
            path = path.Substring(dir.Length);
        }

        return path;
    }
}

}