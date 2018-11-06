using UnityEngine;
using System.Collections.Generic;

namespace com.rak.BeatMaker
{
    public class BeatMapData
    {
        public NoteDetails[] notes;
        public BeatSaveDifficulty difficulty;
        public float beatsPerMinute;
        public string mapName;
        public string songName;
        public string songFileName;
        public string songArtist;
        public string mapArtist;
        public float songOffset;

        public static BeatMapData GenerateBMDInfo(string mapArtist, string songArtist, string songName,
            int songOffset, float bpm,BeatSaveDifficulty difficulty,string songFileName)
        {
            BeatMapData data = new BeatMapData();
            data.mapArtist = mapArtist;
            data.songArtist = songArtist;
            data.songFileName = songFileName;
            data.songName = songName;
            data.songOffset = songOffset;
            data.beatsPerMinute = bpm;
            data.difficulty = difficulty;
            return data;
        }
        public static BeatMapData CopyBMDNotNotes(BeatMapData target,BeatMapData source)
        {
            target.beatsPerMinute = source.beatsPerMinute;
            target.difficulty = source.difficulty;
            target.mapArtist = source.mapArtist;
            target.mapName = source.mapName;
            target.songArtist = source.songArtist;
            target.songName = source.songName;
            target.songOffset = source.songOffset;
            target.songFileName = source.songFileName;
            return target;
        }
        public static NoteDetails[] GenerateNotesForNewMap(BeatMap.NoteMode modeToGenerate,
            int totalBeats)
        {
            Debug.Log("Generating notes for new map, #Beats-" + totalBeats + " Mode-" + modeToGenerate);
            int numberOfNotesToGenerate = (12 * totalBeats);
            numberOfNotesToGenerate = (int)(numberOfNotesToGenerate * Note.GetFactor(modeToGenerate));
            NoteDetails[] notes = new NoteDetails[numberOfNotesToGenerate];
            for (int count = 0; count < numberOfNotesToGenerate; count++)
            {
                notes[count] = new NoteDetails();
                int offSet = count % 12;
                int y = offSet / 4;
                int x = offSet % 4;
                Vector2 gridPosition = new Vector2(x, y);
                notes[count].gridPosition = gridPosition;
                notes[count].color = Note.NoteColor.NONE;
                notes[count].slashDirection = Note.SlashDirection.NONE;
                float spawnTime = (int)(count / 12f) / Note.GetFactor(modeToGenerate);
                float factor = Note.GetFactor(modeToGenerate);
                spawnTime = BeatMap.RoundToRelativeBeat(spawnTime,modeToGenerate);
                notes[count].timeToSpawn = spawnTime;
            }
            return notes;
        }
        public static NoteDetails[] InvertNoteArray(NoteDetails[] original, int totalBeats
            , BeatMap.NoteMode mode)
        {
            NoteDetails[] filled = GenerateNotesForNewMap(mode, totalBeats);
            List<NoteDetails> delta = new List<NoteDetails>();
            for (int count = 0; count < filled.Length; count++)
            {
                if (!hasNoteWithSamePosition(filled[count], original))
                    delta.Add(filled[count]);

            }
            Debug.LogWarning("Inverted size - " + delta.Count);
            return delta.ToArray();
        }

        public static int GetNoteWithSamePosition(NoteDetails note, NoteDetails[] listToSearch)
        {
            for (int count = 0; count < listToSearch.Length; count++)
            {
                if (note.gridPosition == listToSearch[count].gridPosition)
                {
                    if (Mathf.Abs(note.timeToSpawn - listToSearch[count].timeToSpawn) < .1f)
                    {
                        return count;
                    }
                }
            }
            return -1;
        }
        public static bool hasNoteWithSamePosition(NoteDetails note, NoteDetails[] listToSearch)
        {
            for (int count = 0; count < listToSearch.Length; count++)
            {
                if (note.gridPosition == listToSearch[count].gridPosition)
                {
                    if (Mathf.Abs(note.timeToSpawn - listToSearch[count].timeToSpawn) < .1f)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
        
    }
}