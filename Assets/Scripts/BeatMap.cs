using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.XR;
using com.rak.BeatMaker.IO;

namespace com.rak.BeatMaker
{
    public class BeatMap : MonoBehaviour
    {
        public enum EditorState { Playback, Recording, Editing, PlaybackPaused, Started, StartNewMapLoad, PlaybackLoad, INIT }
        public enum NoteMode { WholeNote, HalfNote, QuarterNote, EighthNote, DoubleNote }
        public static Vector3 GetSpawnPosition(Vector2 gridPosition)
        {
            return spawnPositions[(int)gridPosition.x, (int)gridPosition.y];
        }
        public static Vector3 GetSpawnPosition(Vector2 gridPosition, bool rewinding)
        {
            if (!rewinding)
                return spawnPositions[(int)gridPosition.x, (int)gridPosition.y];
            else
            {
                Vector3 spawnPosition = spawnPositions[(int)gridPosition.x, (int)gridPosition.y];
                spawnPosition.z = destroyNoteAtZ;
                return spawnPosition;
            }
        }
        public static float GetSpawnPositionZ()
        {
            return spawnPositions[0, 0].z;
        }
        public static string savePath = "C:/Users/Rakjavik/AppData/LocalLow/DefaultCompany/BeatMapMaker/";
        public static float destroyNoteAtZ = 6.93f;

        public static GameObject NoteContainer { get; private set; }
        public static ControlsDiagram controlsDiagram;
        public static MiniMap MinimapInstance;
        public static NoteMode currentNoteMode = NoteMode.EighthNote;
        public static float CurrentFactor
        {
            get
            {
                Debug.LogWarning("Current notes mode - " + currentNoteMode);
                return Note.GetFactor(currentNoteMode);
            }
        }
        public static float timeToReachPlayer = 3.817781f;
        public static float beatsToReachPlayer;
        public static string currentSongName;

        public static bool isVrPlayer;
        public static bool debug = true;
        public static float showGuardAtZ = 7f;
        public static BeatSaveDifficulty currentDifficulty = BeatSaveDifficulty.Normal;
        public static bool Inverted { get; private set; }
        public static int currentFrame = 0;
        public static bool IsRewinding
        {
            get
            {
                return beatManager.rewinding;
            }
        }
        public static float GetMostRecentBeat
        {
            get
            {
                return beatManager.GetCurrentRelativeBeat();
            }
        }

        public Transform[] spawnBlocks;
        public GameObject notePrefab;
        public Material[] materials;
        public GameObject barPrefab;
        public Text[] debugText;
        public Saber leftSaber;
        public Saber rightSaber;
        public MiniMap miniMap;
        public GameObject guard;
        public FlatMenu flatMenu;
        public bool AutoPlay = true;
        
        #region State Handling
        private static bool running;
        public static bool IsRunning() { return running; }
        public static void SetRunning(bool running)
        {
            if (running == BeatMap.running)
                Debug.LogWarning("Running already set to same value");
            else
            {
                BeatMap.running = running;
            }
        }
        public static EditorState GetCurrentState() { return state; }
        public static void SetCurrentState(EditorState newState)
        {
            bool makeChange = false;
            if (newState == EditorState.Editing)
            {
                if (state == EditorState.Recording || 
                    state == EditorState.Started)
                {
                    makeChange = true;
                }
            }
            else if (newState == EditorState.Recording)
            {
                if (state == EditorState.Editing)
                {
                    makeChange = true;
                }
            }
            else if (newState == EditorState.INIT)
            { 
                makeChange = true;
            }
            else if (newState == EditorState.PlaybackLoad)
            {
                if (state == EditorState.INIT)
                    makeChange = true;
            }
            else if (newState == EditorState.Started)
            {
                if (state == EditorState.PlaybackLoad || state == EditorState.StartNewMapLoad)
                    makeChange = true;
            }
            else if (newState == EditorState.StartNewMapLoad)
            {
                if (state == EditorState.INIT)
                    makeChange = true;
            }
            else if (newState == EditorState.PlaybackPaused)
            {
                if (state == EditorState.Playback ||
                    state == EditorState.Started)
                    makeChange = true;
            }
            else if (newState == EditorState.Playback)
            {
                if (state == EditorState.PlaybackPaused)
                    makeChange = true;
            }
            if (makeChange)
            {
                state = newState;
                if(initialized)
                    controlsDiagram.UpdateMe(state);
                Debug.LogWarning("State changed to " + newState);
            }
            else Debug.LogError("Invalid state change current/requested " + state + "/" + newState);
        }
        private static EditorState state;
        #endregion

        private static Vector3[,] spawnPositions;
        private static BeatMapData beatMapData;
        private static List<NoteDetails> waitingNotes;
        private static List<Note> activeNotes;
        private static List<NoteDetails> completedNotes;
        private static Text[] currentDebugText;
        private static int lastBeatBarPlacedAtBeat;
        private static Camera mainCamera;
        private static AudioClip song;
        private static bool initialized = false;
        private static int currentDebugLine = 0;
        private static List<string> debugMessages = new List<string>();
        private static Material[] staticMats;
        private static Utilities.BeatManager beatManager;
        private static float mostRecentBeat = 0;
        public static GameObject BarPrefab;
        private static GameObject NotePrefab;

        public bool isVrPlayerOverride;
        private bool audioIsLoading = false;

        private AudioSource audioSource;
        private BeatSaberJSONClass json;

        private void Start()
        {
            MinimapInstance = miniMap;
            SetCurrentState(EditorState.INIT);
            // Set static objects from unity editor //
            BarPrefab = barPrefab;
            NotePrefab = notePrefab;
            controlsDiagram = GameObject.FindGameObjectWithTag("ControlsWindow").
                GetComponent<ControlsDiagram>();
            controlsDiagram.UpdateMe(EditorState.INIT);
            BeatMap.currentDebugText = debugText;
            BeatMap.staticMats = materials;
            isVrPlayer = isVrPlayerOverride;
            spawnPositions = new Vector3[4, 3];
            spawnPositions[0, 0] = spawnBlocks[1].position;
            spawnPositions[0, 1] = spawnBlocks[5].position;
            spawnPositions[0, 2] = spawnBlocks[9].position;
            spawnPositions[1, 0] = spawnBlocks[0].position;
            spawnPositions[1, 1] = spawnBlocks[4].position;
            spawnPositions[1, 2] = spawnBlocks[8].position;
            spawnPositions[2, 0] = spawnBlocks[2].position;
            spawnPositions[2, 1] = spawnBlocks[6].position;
            spawnPositions[2, 2] = spawnBlocks[10].position;
            spawnPositions[3, 0] = spawnBlocks[3].position;
            spawnPositions[3, 1] = spawnBlocks[7].position;
            spawnPositions[3, 2] = spawnBlocks[11].position;
            BeatMap.Log("Spawn position0 - " + spawnPositions[0, 0]);
            mainCamera = Camera.main;
            NoteContainer = new GameObject("NoteContainer");
            Log("Main camera - " + mainCamera.name);
            audioSource = GetComponent<AudioSource>();
            leftSaber.Initialize(isVrPlayer);
            rightSaber.Initialize(isVrPlayer);
        }
        public void Initialize(BeatSaberJSONClass data,NoteMode noteMode,EditorState requestedState)
        {
            Initialize(BeatSaberJSONClass.ConvertBSDataToEditorData(data),noteMode,requestedState);
        }
        public void Initialize(BeatMapData data, NoteMode noteMode, EditorState requestedState)
        {
            if (BeatMap.state != EditorState.Started)
            {
                Debug.Log("Trying to initialize when not in started state - " + state);
                return;
            }
            Debug.LogWarning("Audio loaded, Initializing with note mode " + currentNoteMode);
            
            waitingNotes = new List<NoteDetails>();
            activeNotes = new List<Note>();
            completedNotes = new List<NoteDetails>();
            beatManager = new Utilities.BeatManager(data.beatsPerMinute,song);
            audioSource.clip = song;
            beatsToReachPlayer = timeToReachPlayer * beatManager.beatsPerSecond; // Convert from seconds to beats
            Debug.Log("Time to reach player Seconds-Beats -- " + timeToReachPlayer + "-" + beatsToReachPlayer);
            Debug.Log("BPM - " + data.beatsPerMinute + " BPS - " + beatManager.beatsPerSecond);
            Debug.Log("Total beats - " + beatManager.GetTotalBeats);
            lastBeatBarPlacedAtBeat = beatManager.GetCurrentWholeBeat;
            beatMapData = data;
            waitingNotes = new List<NoteDetails>();
            activeNotes = new List<Note>();
            completedNotes = new List<NoteDetails>();
            if (data.notes == null)
            {
                beatMapData.notes = new NoteDetails[0];
            }
            Log("Loading notes, size - " + data.notes.Length);
            for (int count = 0; count < data.notes.Length; count++)
            {
                data.notes[count].inverted = false;
                data.notes[count].timeToSpawn = RoundToRelativeBeat(data.notes[count].timeToSpawn, noteMode);
                waitingNotes.Add(data.notes[count]);
            }
            // When in creative mode, we keep the inverted objects in memory //
            if (requestedState == EditorState.Recording || requestedState == EditorState.Editing)
            {
                Debug.Log("Creating inverted array, waitingnotes size - " + waitingNotes.Count + " - " +
                    "total beats - " + beatManager.GetTotalBeats);
                NoteDetails[] invertedArray = BeatMapData.InvertNoteArray(waitingNotes.ToArray(), beatManager.GetTotalBeats, currentNoteMode);
                for (int count = 0; count < invertedArray.Length; count++)
                {
                    invertedArray[count].inverted = true;
                    waitingNotes.Add(invertedArray[count]);
                }
                Log("Inverted array size - " + invertedArray.Length);
            }
            if (requestedState == EditorState.Editing || requestedState == EditorState.PlaybackPaused)
            {
                // If we have an inverted map on top of the regular map, disable note displah for performance //
                miniMap.showNotes = true;
            }
            else if (requestedState == EditorState.Playback || requestedState == EditorState.Recording)
            {
                miniMap.showNotes = false;
            }
            SetRunning(false);
            SetCurrentState(requestedState);
            miniMap.Initialize(beatManager.GetTotalBeats,beatsToReachPlayer);
            initialized = true;
        }
        public void LoadSelectedMap()
        {
            SetCurrentState(EditorState.PlaybackLoad);
            Debug.LogWarning("Loading selected map");
            string[] location = flatMenu.GetSelectedMapFullPath();
            json = LoadFromDisk(location[0], location[1], location[2]);
            beatMapData = BeatSaberJSONClass.ConvertBSDataToEditorData(json);
            // Load from disk continues from update method when audio is loaded //
        }
        public void SaveCurrentMapToDisk()
        {
            BeatMapData dataToSave = new BeatMapData();
            if (beatMapData == null)
            {
                BeatMap.Log("Current map data not available to save");
                return;
            }
            if (state != EditorState.Editing && state != EditorState.Recording &&
                state != EditorState.PlaybackPaused)
            {
                BeatMap.Log("Call to save when not in proper state - " + state);
                return;
            }
            else
            {
                Debug.LogWarning("data song file name - " + beatMapData.songFileName);
                dataToSave = BeatMapData.CopyBMDNotNotes(dataToSave, beatMapData);
                float throwAwayFirstBeats = (dataToSave.beatsPerMinute / 16.5f);
                NoteDetails[] notes = GetAllNotes(true);
                List<NoteDetails> filtered = new List<NoteDetails>();
                for (int count = 0; count < notes.Length; count++)
                {
                    if(notes[count].timeToSpawn < throwAwayFirstBeats)
                    {

                    }
                    else
                    {
                        Debug.LogWarning("Accepting note " + notes[count].timeToSpawn + "-" +
                            notes[count].inverted);
                        filtered.Add(notes[count]);
                    }
                }
                Debug.LogWarning("Notes - " + notes.Length + " filtered - " + filtered.Count);
                dataToSave.notes = filtered.ToArray();
            }
            Log("Call to save with # of notes " + dataToSave.notes.Length);
            Utilities.SaveToDisk(dataToSave);
        }

        private void Update()
        {
            if (!initialized)
            {
                // Initialization //

                if (audioIsLoading)
                {
                    AudioIsInitialized();
                }
            }
            // INITIALIZED //
            else
            {
                // Start Update //
                currentFrame++;
                if (running)
                {
                    beatManager.Update();
                    // Song may be completed //
                    if ((state == EditorState.Playback || state == EditorState.Recording) 
                        && !audioSource.isPlaying) PauseSong();
                    if (mainCamera.transform.position.z >= showGuardAtZ &&
                        !guard.activeSelf)
                    {
                        guard.SetActive(true);
                        BeatMap.Log("Guard activated");
                        PauseSong();
                    }
                }
                // Check if player is back in a good position and resume song //
                else if (guard.activeSelf && mainCamera.transform.position.z < showGuardAtZ)
                {
                    guard.SetActive(false);
                    BeatMap.Log("Guard deactivated");
                    ResumeSong();
                }
                // If we're not running, we are done //
                else
                {
                    return;
                }

                // Check if the next beat has started //
                if (beatManager.NextBeatStarted(true,mostRecentBeat))
                {
                    mostRecentBeat = beatManager.GetCurrentRelativeBeat();
                    // Check if any active notes are complete //
                    foreach (Note note in activeNotes)
                    {
                        if ((note.transform.position.z < destroyNoteAtZ && !beatManager.rewinding) ||
                            (note.transform.position.z > spawnPositions[0, 0].z) && beatManager.rewinding)
                        {
                            note.gameObject.SetActive(false);
                            if (!beatManager.rewinding)
                                completedNotes.Add(note.noteDetails);
                            else
                                waitingNotes.Add(note.noteDetails);
                        }
                    }

                    // Wait list is determined by rewinding //
                    List<NoteDetails> waitList;
                    if (!beatManager.rewinding)
                    {
                        waitList = waitingNotes;
                    }
                    else
                    {
                        waitList = completedNotes;
                    }

                    // Temp list for removing notes from waiting list //
                    List<NoteDetails> notesDoneWaiting = new List<NoteDetails>();
                    for (int count = 0; count < waitList.Count; count++)
                    {
                        NoteDetails noteDetails = waitList[count];
                        float playerRelativeTimeToSpawn = beatManager.currentBeat + beatsToReachPlayer;
                        if ((playerRelativeTimeToSpawn >= noteDetails.timeToSpawn && !beatManager.rewinding) ||
                            (beatManager.currentBeat <= noteDetails.timeToSpawn && beatManager.rewinding))
                        {
                            Note note = noteDetails.note;
                            if (note == null)
                            {
                                note = Instantiate(notePrefab).GetComponent<Note>();
                            }
                            note.Initialize(noteDetails, materials, GetSpawnPosition(noteDetails.gridPosition, beatManager.rewinding));
                            note.transform.SetParent(NoteContainer.transform);
                            notesDoneWaiting.Add(noteDetails);
                            activeNotes.Add(note);
                            note.Refresh();
                        }
                    }

                    foreach (NoteDetails note in notesDoneWaiting)
                    {
                        if (!beatManager.rewinding)
                            waitingNotes.Remove(note);
                        else
                            completedNotes.Remove(note);
                    }
                }
            }
        }
        private void AudioIsInitialized()
        {
            if (flatMenu.hideOnLoad) flatMenu.gameObject.SetActive(false);
            if (song.loadState == AudioDataLoadState.Loaded)
            {
                audioSource.clip = song;
                audioSource.Play();
                audioSource.Pause();
                if (state == EditorState.StartNewMapLoad)
                {
                    beatMapData.notes = null;
                    SetCurrentState(EditorState.Started);
                    Initialize(beatMapData, currentNoteMode, EditorState.Editing);
                }
                else // State should be EditorState.PlaybackLoad
                {
                    SetCurrentState(EditorState.Started);
                    Initialize(json, currentNoteMode, EditorState.PlaybackPaused);
                }
                controlsDiagram.Initialize(GetCurrentState());
                audioIsLoading = false;
                if (AutoPlay) ResumeSong();
            }
        }
        public void InvertNotes()
        {
            if (BeatMap.state == EditorState.Playback) return;
            BeatMap.Log("Inverting map from " + BeatMap.Inverted + 
                " Waiting note size " + waitingNotes.Count);
            BeatMap.Inverted = !BeatMap.Inverted;
            InvertArray(waitingNotes);
            InvertArray(activeNotes);
            InvertArray(completedNotes);
        }
        private void InvertArray(List<Note> notes)
        {
            for (int count = 0; count < notes.Count; count++)
            {
                NoteDetails currentNote = notes[count].noteDetails;
                currentNote.inverted = !currentNote.inverted;
                notes[count].Refresh();
            }
        }
        private void InvertArray(List<NoteDetails> notes)
        {
            for (int count = 0; count < notes.Count; count++)
            {
                NoteDetails currentNote = notes[count];
                currentNote.inverted = !currentNote.inverted;
            }
        }

        public void StartNewMap()
        {
            if(!flatMenu.IsReadyForNew())
            {
                BeatMap.Log("Start new called, but not ready");
                return;
            }
            currentNoteMode = flatMenu.selectedMode;
            StartNewMap(0, flatMenu.selectedBPM,flatMenu.selectedAudioPath,
                flatMenu.selectedSongArtist,flatMenu.selectedMapArtist,flatMenu.selectedNewSongName,
                flatMenu.selectedDifficulty,flatMenu.selecedNewSongFileName);
        }
        private void StartNewMap(int songOffset, float bpm,string fullPathToAudio,
            string songArtist,string mapArtist,string songName,BeatSaveDifficulty difficulty,
            string songFileName)
        {
            Debug.LogWarning("Selectednewsong - " + songFileName);
            SetCurrentState(EditorState.StartNewMapLoad);
            BeatMap.currentDifficulty = difficulty;
            beatMapData = BeatMapData.GenerateBMDInfo(mapArtist, songArtist,songName
                , songOffset, bpm,difficulty,songFileName);
            song = Utilities.StartAudioInitialize(fullPathToAudio);
            audioIsLoading = true;
        }
        public bool AddNoteDetail(NoteDetails details)
        {
            if (completedNotes.Contains(details)) return false;
            else if (waitingNotes.Contains(details)) return false;
            foreach (Note note in activeNotes)
            {
                if (note.noteDetails.timeToSpawn == details.timeToSpawn)
                {
                    if (note.noteDetails.gridPosition == details.gridPosition)
                    {
                        return false;
                    }
                }
            }
            completedNotes.Add(details);
            return true;
        }
        public void PauseSong()
        {
            Debug.LogWarning("Pause called running, state " + running + "-" + BeatMap.state);
            if (running)
            {
                audioSource.Pause();
                SetRunning(false);
                if (state == EditorState.Recording)
                    SetCurrentState(EditorState.Editing);
                else SetCurrentState(EditorState.PlaybackPaused);
            }
        }
        public void ResumeSong()
        {
            Debug.LogWarning("Resume called running, state " + running + "-" + BeatMap.state);
            if (!running)
            {
                if (beatManager.rewinding)
                {
                    //Debug.Log("Current beat - " + currentBeat + " BPS - " + (beatMapData.beatsPerMinute / 60));
                    //Debug.Log("Setting to " + currentBeat * (beatMapData.beatsPerMinute / 60));
                    beatManager.SetRewinding(false);
                    float skipToTime = beatManager.currentBeat / (beatMapData.beatsPerMinute / 60);
                    if (skipToTime < 0) skipToTime = 0;
                    audioSource.time = skipToTime;
                }
                SetRunning(true);
                if (state == EditorState.Editing)
                {
                    SetCurrentState(EditorState.Recording);
                    leftSaber.DetachSelector();
                    rightSaber.DetachSelector();
                }
                else if (state == EditorState.PlaybackPaused)
                    SetCurrentState(EditorState.Playback);
                // If we have a negative beat, don't start the song, let the update method handle it //
                if (!(BeatMap.GetCurrentBeat() < 0))
                {
                    audioSource.time = GetCurrentBeat() / beatManager.beatsPerSecond;
                    audioSource.UnPause();
                }
            }
        }
        public void SkipTo()
        {
            PauseSong();
            SkipToBeat(110);
        }
        public void Rewind()
        {
            if (!running && !beatManager.rewinding)
            {
                beatManager.SetRewinding(true);
                SetRunning(true);
            }
        }
        
        public static void SkipToBeat(float beat)
        {
            if(!(state == EditorState.PlaybackPaused || state == EditorState.Editing))
            {
                Debug.LogWarning("SkipToBeat called when not in proper state -" + state);
                return;
            }
            BeatMap.Log("Skipping to beat " + beat);
            
            // Disable all active notes and bars on screen //
            foreach (Note note in activeNotes)
            {
                note.gameObject.SetActive(false);
            }
            
            // Reinitialize lists //
            waitingNotes = new List<NoteDetails>();
            activeNotes = new List<Note>();
            completedNotes = new List<NoteDetails>();

            // Get the beat manager up to date //
            float noteSpeedInBeats = beatManager.GetNoteSpeedInBeats();
            beatManager.SkipToBeat(beat, noteSpeedInBeats);
            float lastBeatToPlace = beatManager.GetClosestBeatToPlayer();
            // List of notes that will be spawned as active //
            List<NoteDetails> notesToSpawn = new List<NoteDetails>();
            // Check each note in our map data for notes that fall within the skipped time //
            for (int count = 0; count < beatMapData.notes.Length; count++)
            {
                if (beatMapData.notes[count].timeToSpawn <= beat &&
                    beatMapData.notes[count].timeToSpawn >= lastBeatToPlace)
                {
                    notesToSpawn.Add(beatMapData.notes[count]);
                }
                else
                {
                    if (beatMapData.notes[count].timeToSpawn < beat)
                        completedNotes.Add(beatMapData.notes[count]);
                    else
                        waitingNotes.Add(beatMapData.notes[count]);
                }
            }
            // Place Notes //
            for(int count = 0; count < notesToSpawn.Count; count++)
            {
                Note note = notesToSpawn[count].note;
                if (note == null)
                {
                    note = Instantiate(NotePrefab).GetComponent<Note>();
                    note.noteDetails = notesToSpawn[count];
                }
                float spaceBetween = noteSpeedInBeats;
                Vector3 position = GetSpawnPosition(note.noteDetails.gridPosition);
                float adjustedSpawnTime = beat-note.noteDetails.timeToSpawn;
                position.z -= (adjustedSpawnTime * spaceBetween);
                position.z = RoundToRelativeBeat(position.z,currentNoteMode);
                note.Initialize(notesToSpawn[count], staticMats, position);
                
                activeNotes.Add(note);
            }
            SetRunning(false);
            MinimapInstance.SetZBasedOnBeat(beat);
            initialized = true;
        }
        public BeatSaberJSONClass LoadFromDisk(string path,string fileName,string audioClipName)
        {
            BeatSaberJSONClass beatSaberMap = Utilities.LoadFromDisk(path,fileName);
            song = Utilities.StartAudioInitialize(path + flatMenu.selectedLoadSongFileName);
            audioIsLoading = true;
            Debug.Log("Audio loading - " + path+audioClipName);
            return beatSaberMap;
        }
        public float GetSongTimeInSecs() { return audioSource.time; }
        public int GetNumberOfNotesInMap()
        {
            int count = waitingNotes.Count;
            count += activeNotes.Count;
            count += completedNotes.Count;
            return count;
        }
        public void SetNoteMode(NoteMode mode) { currentNoteMode = mode; }
        public void SetNoteModeWhole() { SetNoteMode(NoteMode.WholeNote); }
        public void SetNoteModeHalf() { SetNoteMode(NoteMode.HalfNote); }
        public void SetNoteModeQuarter() { SetNoteMode(NoteMode.QuarterNote); }

        public static void Log(string message)
        {
            int maxLines = 10;
            if (debug)
            {
                message = Time.time + " - " + message;
                Debug.Log(message);
                debugMessages.Add(message);
                if (debugMessages.Count > maxLines)
                    debugMessages.RemoveAt(0);

                string newText = "";
                foreach (string line in debugMessages)
                {
                    newText += line;
                    newText += "\n";
                }
                foreach (Text text in currentDebugText)
                {
                    text.text = newText;
                }
            }
            
        }

        public static float GetCurrentBeat() { return beatManager.currentBeat; }
        
        public static void DeleteNote(Note note)
        {
            activeNotes.Remove(note);
            note.gameObject.SetActive(false);
        }
        public static float GetCurrentMapsBPM() { return beatMapData.beatsPerMinute; }
        public static NoteDetails[] GetAllNotes(bool discardInverted)
        {
            List<NoteDetails> notes = new List<NoteDetails>();
            foreach (NoteDetails note in waitingNotes)
            {
                if(!discardInverted || !note.inverted)
                    notes.Add(note);
            }
            foreach (NoteDetails note in completedNotes)
            {
                if (!discardInverted || !note.inverted)
                    notes.Add(note);
            }
            foreach (Note note in activeNotes)
            {
                if (!discardInverted || !note.noteDetails.inverted)
                    notes.Add(note.noteDetails);
            }
            return notes.ToArray();
        }
        public static float RoundToRelativeBeat(float notRounded,NoteMode mode)
        {
            float factor = Note.GetFactor(mode);
            float rounded = ((float)Math.Round((notRounded*100)*factor, MidpointRounding.ToEven) / factor) / 100f;
            if (((int)rounded) + 1 - rounded < .01f)
            {
                rounded = (int)rounded + 1;
            }
            return rounded;
        }
    }
}