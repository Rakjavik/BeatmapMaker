using System;
using System.Collections.Generic;
using UnityEngine;
namespace com.rak.BeatMaker
{
    public enum BeatSaveDifficulty { Easy,Normal,Hard,Expert,ExpertPlus }

    [Serializable]
    public class BeatSaberJSONClass
    {
        public InfoJSON info;
        public LevelJSON level;

        public static BeatMapData ConvertBSDataToEditorData(BeatSaberJSONClass fromBeatSaber)
        {
            BeatMapData bmd = new BeatMapData();
            bmd.beatsPerMinute = fromBeatSaber.info.beatsPerMinute;
            bmd.mapArtist = fromBeatSaber.info.authorName;
            bmd.songArtist = fromBeatSaber.info.songSubName;
            bmd.songName = fromBeatSaber.info.songName;
            bmd.songOffset = fromBeatSaber.info.difficultyLevels[0].offset / 100;
            List<NoteDetails> notes = new List<NoteDetails>();
            foreach (BSNote bsNote in fromBeatSaber.level._notes)
            {
                NoteDetails note = new NoteDetails();
                if (bsNote._type == 0) note.color = Note.NoteColor.LEFT;
                else note.color = Note.NoteColor.RIGHT;
                note.slashDirection = BSNote.GetSlashDirection(bsNote._cutDirection);
                note.gridPosition = new Vector2(bsNote._lineIndex, bsNote._lineLayer);
                note.timeToSpawn = bsNote._time;
                notes.Add(note);
            }
            bmd.notes = notes.ToArray();
            return bmd;
        }
        public static BeatSaberJSONClass ConvertUnityDataToBSData(BeatMapData data)
        {
            BeatSaberJSONClass bsaberJSON = new BeatSaberJSONClass();
            bsaberJSON.info = new InfoJSON();
            bsaberJSON.info.beatsPerMinute = (int)data.beatsPerMinute;
            bsaberJSON.info.authorName = data.mapArtist;
            bsaberJSON.info.songSubName = data.songArtist;
            bsaberJSON.info.songName = data.songName;
            // TODO merge with existing info if any //
            bsaberJSON.info.difficultyLevels = new DifficultyLevel[1];
            bsaberJSON.info.difficultyLevels[0] = DifficultyLevel.Generate(data.difficulty,
                data.songFileName,(int)data.songOffset);
            List<BSNote> notes = new List<BSNote>();
            for (int count = 0; count < data.notes.Length; count++)
            {
                if (!data.notes[count].inverted)
                {
                    BSNote bSNote = new BSNote();
                    if (data.notes[count].color == Note.NoteColor.LEFT) bSNote._type = 0;
                    else bSNote._type = 1;
                    bSNote._cutDirection = BSNote.GetBSaberCutDirection(data.notes[count].slashDirection);
                    bSNote._lineIndex = (int)data.notes[count].gridPosition.x;
                    bSNote._lineLayer = (int)data.notes[count].gridPosition.y;
                    bSNote._time = data.notes[count].timeToSpawn;
                    notes.Add(bSNote);
                }
            }
            BeatMap.Log("Notes exported with count " + notes.Count);
            LevelJSON level = new LevelJSON();
            level._beatsPerMinute = (int)data.beatsPerMinute;
            level._version = "1.0";
            level._beatsPerBar = 16;
            level._noteJumpSpeed = 10;
            level._shufflePeriod = .5f;
            level._notes = notes.ToArray();
            bsaberJSON.level = level;
            return bsaberJSON;
        }
        [Serializable]
        public class InfoJSON
        {
            public string songName;
            public string songSubName;
            public string authorName;
            public int beatsPerMinute;
            public int previewStartTime;
            public int previewDuration;
            public string coverImagePath;
            public string environmentName;
            public DifficultyLevel[] difficultyLevels;
        }
        [Serializable]
        public class DifficultyLevel
        {
            public static DifficultyLevel Generate(BeatSaveDifficulty difficulty,string audioFileName
                ,int offset)
            {
                DifficultyLevel level = new DifficultyLevel();
                level.difficulty = difficulty.ToString();
                level.difficultyRank = 4;//Expert
                level.audioPath = audioFileName;
                level.jsonPath = difficulty.ToString() + ".json";
                level.offset = offset;
                return level;
            }

            public string difficulty;
            public int difficultyRank;
            public string audioPath;
            public string jsonPath;
            public int offset;
            public int oldOffset;
        }
        [Serializable]
        public class LevelJSON
        {
            public string _version;
            public int _beatsPerMinute;
            public int _beatsPerBar;
            public int _noteJumpSpeed;
            public int _shuffle;
            public float _shufflePeriod;
            public Event[] _events;
            public BSNote[] _notes;
            public Obstacle[] _obstacles;
        }
        [Serializable]
        public class Event
        {
            public float _time;
            public int _type;
            public int _value;
        }
        [Serializable]
        public class BSNote
        {
            public float _time;
            public int _lineIndex;
            public int _lineLayer;
            public int _type;
            public int _cutDirection;

            public static Note.SlashDirection GetSlashDirection(int _cutDirection)
            {
                if (_cutDirection == 0) return Note.SlashDirection.UP;
                else if (_cutDirection == 1) return Note.SlashDirection.DOWN;
                else if (_cutDirection == 2) return Note.SlashDirection.LEFT;
                else if (_cutDirection == 3) return Note.SlashDirection.RIGHT;
                else if (_cutDirection == 4) return Note.SlashDirection.UPLEFT;
                else if (_cutDirection == 5) return Note.SlashDirection.UPRIGHT;
                else if (_cutDirection == 6) return Note.SlashDirection.DOWNLEFT;
                else if (_cutDirection == 7) return Note.SlashDirection.DOWNRIGHT;
                else if (_cutDirection == 8) return Note.SlashDirection.NONE;
                else
                {
                    Debug.LogError("Cutdirection not found, this shouldn't happen - " + _cutDirection);
                    return Note.SlashDirection.NONE;
                }
            }

            public static int GetBSaberCutDirection(Note.SlashDirection direction)
            {
                if (direction == Note.SlashDirection.UP) return 0;
                else if (direction == Note.SlashDirection.DOWN) return 1;
                else if (direction == Note.SlashDirection.LEFT) return 2;
                else if (direction == Note.SlashDirection.RIGHT) return 3;
                else if (direction == Note.SlashDirection.UPLEFT) return 4;
                else if (direction == Note.SlashDirection.UPRIGHT) return 5;
                else if (direction == Note.SlashDirection.DOWNLEFT) return 6;
                else if (direction == Note.SlashDirection.DOWNRIGHT) return 7;
                else if (direction == Note.SlashDirection.NONE) return 8;
                else
                {
                    Debug.LogError("SlashDirection not found, this shouldn't happen - " + direction);
                    return 8;
                }

            }
        }
        [Serializable]
        public class Obstacle
        {
            public float _time;
            public int _lineIndex;
            public int _type;
            public int _duration;
            public int _width;
        }
    }
}