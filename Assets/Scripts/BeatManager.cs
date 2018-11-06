using System.Collections.Generic;
using UnityEngine;

namespace com.rak.BeatMaker.IO
{
    public partial class Utilities
    {
        public class BeatManager
        {
            public static GameObject BarContainer { get; private set; }

            public AudioClip song { get; private set; }
            private List<BeatBar> bars;
            public float beatsPerSecond { get; private set; }
            public float bpm { get; private set; }
            public float currentBeat { get; private set; }
            public bool rewinding { get; private set; }
            public float lastBeat { get; private set; }
            public int lastBeatBarPlaced { get; private set; }
            public bool NextBeatStarted(bool relative,float lastBeat)
            {
                bool started;
                if (!rewinding)
                {
                    if (relative)
                        started = (GetCurrentRelativeBeat() > lastBeat);
                    else
                        started = (GetCurrentWholeBeat > lastBeat);
                }
                else
                {
                    if (relative)
                        started = (GetCurrentRelativeBeat() < lastBeat);
                    else
                        started = (GetCurrentWholeBeat < lastBeat);
                }
                return started;
            }
            public int GetCurrentWholeBeat { get
                {
                    return (int)currentBeat;
                } }
            public float GetCurrentRelativeBeat()
            {
                if (BeatMap.currentNoteMode == BeatMap.NoteMode.WholeNote)
                {
                    return (int)currentBeat;
                }
                else if (BeatMap.currentNoteMode == BeatMap.NoteMode.HalfNote)
                {
                    return ((int)(currentBeat * 2)) / 2f;
                }
                else
                {
                    return ((int)(currentBeat * 4)) / 4f;
                }
            }
            public int GetTotalBeats
            {
                get
                {
                    return (int)(beatsPerSecond * lengthOfSongInSeconds);
                }
            }
            public float GetNoteSpeedInBeats()
            {
                return Note.noteSpeed / beatsPerSecond;
            }
            public float GetClosestBeatToPlayer() { return currentBeat - BeatMap.beatsToReachPlayer; }
            public float GetZDelta() { return BeatMap.GetSpawnPositionZ()-BeatMap.destroyNoteAtZ; }

            private bool NextBeatStarted(bool relative)
            {
                bool started;
                if (DEBUG)
                    Debug.LogWarning("NextBeatStarted lastbeat-" + lastBeat + " whole/relative " +
                    GetCurrentWholeBeat.ToString() + "-" + GetCurrentRelativeBeat());
                if (!rewinding)
                {
                    if (relative)
                        started = (GetCurrentRelativeBeat() > lastBeat);
                    else
                        started = (GetCurrentWholeBeat > (int)lastBeat);
                    if (started) lastBeat = GetCurrentRelativeBeat();
                }
                else
                {
                    if (relative)
                        started = (GetCurrentRelativeBeat() < lastBeat);
                    else
                        started = (GetCurrentWholeBeat < (int)lastBeat);
                    if (started) lastBeat = GetCurrentRelativeBeat();
                }
                return started;
            }
            private float lengthOfSongInSeconds;
            private float destroyAtZ;
            private float spawnAtZ;

            // CONSTRUCTOR //
            public BeatManager(float bpm,AudioClip song)
            {
                this.song = song;
                currentBeat = 0;
                lastBeat = 0;
                lastBeatBarPlaced = 0;
                rewinding = false;
                this.bpm = bpm;
                lengthOfSongInSeconds = song.length;
                beatsPerSecond = (bpm / 60);
                bars = new List<BeatBar>();
                destroyAtZ = BeatMap.destroyNoteAtZ;
                spawnAtZ = BeatMap.GetSpawnPosition(new Vector2(0, 0)).z;
                BarContainer = new GameObject("BarContainer");
            }

            public void SetRewinding(bool rewinding)
            {
                if (this.rewinding == rewinding)
                    Debug.LogWarning("Setting rewinding to same status");
                this.rewinding = rewinding;
            }
            public void Update()
            {
                if (DEBUG) Debug.LogWarning("Current beat - " + currentBeat);
                // Increment or Decrement the current beat //
                if (!rewinding)
                    currentBeat += beatsPerSecond * Time.deltaTime;
                else
                    currentBeat -= beatsPerSecond * Time.deltaTime;

                // Next beat has started //
                if ((int)(currentBeat+BeatMap.beatsToReachPlayer) > lastBeatBarPlaced)
                {
                    lastBeatBarPlaced = (int)(currentBeat + BeatMap.beatsToReachPlayer);
                    // Create new bar //
                    BeatBar newBar = GameObject.Instantiate(BeatMap.BarPrefab).GetComponent<BeatBar>();
                    if(!rewinding)
                    {
                        Vector3 spawnPosition = new Vector3(0, 0, spawnAtZ);
                        newBar.transform.position = spawnPosition;
                        newBar.SetBeatNumber((int)(currentBeat+BeatMap.beatsToReachPlayer));
                        if (DEBUG)
                            Debug.Log("Creating bar for beat # - " + newBar.GetBeatNumber());
                        newBar.transform.SetParent(BarContainer.transform);
                        bars.Add(newBar);
                        
                        // Destory old bars //
                        List<BeatBar> barsToDestroy = new List<BeatBar>();
                        foreach(BeatBar bar in bars)
                        {
                            if(bar.transform.position.z < destroyAtZ)
                            {
                                barsToDestroy.Add(bar);
                            }
                        }
                        for(int count = 0; count < barsToDestroy.Count; count++)
                        {
                            barsToDestroy[count].gameObject.SetActive(false);
                            bars.Remove(barsToDestroy[count]);
                            GameObject.Destroy(barsToDestroy[count].gameObject);
                        }
                    }
                    else
                    {
                        Vector3 spawnPosition = new Vector3(0, 0, destroyAtZ);
                        newBar.transform.position = spawnPosition;
                        newBar.SetBeatNumber((int)currentBeat);
                        bars.Add(newBar);

                        // Destroy bars //
                        List<BeatBar> barsToDestroy = new List<BeatBar>();
                        foreach(BeatBar bar in bars)
                        {
                            if(bar.transform.position.z > spawnAtZ)
                            {
                                barsToDestroy.Add(bar);
                            }
                        }
                        for(int count = 0; count < barsToDestroy.Count; count++)
                        {
                            barsToDestroy[count].gameObject.SetActive(false);
                            bars.Remove(barsToDestroy[count]);
                            GameObject.Destroy(barsToDestroy[count].gameObject);
                        }
                    }
                }
            }
            public void SkipToBeat(float beat,float distancePerBeat)
            {
                this.currentBeat = beat;
                this.lastBeat = beat;
                // Destroy bars on screen //
                for (int count = 0; count < bars.Count; count++)
                {
                    bars[count].gameObject.SetActive(false);
                    GameObject.Destroy(bars[count].gameObject);
                }
                bars = new List<BeatBar>();
                // Create new bars to match current beat //
                float currentZ = spawnAtZ;
                int barCount = 0;
                while(currentZ > destroyAtZ)
                {
                    BeatBar bar = GameObject.Instantiate(BeatMap.BarPrefab,null).GetComponent<BeatBar>();
                    Vector3 barPosition = new Vector3(0, 0, spawnAtZ);
                    barPosition.z = currentZ;
                    bar.SetBeatNumber((int)beat - barCount);
                    bar.transform.position = barPosition;
                    bars.Add(bar);
                    currentZ -= distancePerBeat;
                    barCount++;
                }
            }
        }
    }
}
