using System.IO;
using System.Text;
using UnityEngine;

namespace com.rak.BeatMaker.IO
{
    public partial class Utilities
    {
        private const bool DEBUG = false;

        public static BeatSaberJSONClass LoadFromDisk(string path,string fileName)
        {
            Debug.LogWarning("Loading map from disk - " + path + fileName);
            StringBuilder builder = new StringBuilder();
            StreamReader reader = new StreamReader(path + fileName);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                builder.AppendLine(line);
            }
            reader.Close();
            BeatSaberJSONClass beatSaberMap = new BeatSaberJSONClass();
            BeatSaberJSONClass.LevelJSON loadedMapData = JsonUtility.FromJson<BeatSaberJSONClass.LevelJSON>(builder.ToString());
            beatSaberMap.level = loadedMapData;

            builder = new StringBuilder();
            reader = new StreamReader(path + "info.json");
            while ((line = reader.ReadLine()) != null)
            {
                builder.AppendLine(line);
            }
            reader.Close();
            BeatSaberJSONClass.InfoJSON infoJSON =
                JsonUtility.FromJson<BeatSaberJSONClass.InfoJSON>(builder.ToString());
            beatSaberMap.info = infoJSON;

            return beatSaberMap;
        }

        public static AudioClip StartAudioInitialize(string fullPath)
        {
            WWW www = new WWW("file://" + fullPath);
            return  www.GetAudioClip(false, false, AudioType.OGGVORBIS);
        }

        public static BeatSaberJSONClass SaveToDisk(BeatMapData data)
        {
            Debug.LogWarning("Save to disk called with note count " + data.notes.Length);
            Debug.LogWarning("data song file name - " + data.songFileName);
            BeatSaberJSONClass beatSaberJSON = BeatSaberJSONClass.ConvertUnityDataToBSData(data);
            string levelJSON = JsonUtility.ToJson(beatSaberJSON.level, false);
            BeatSaberJSONClass.InfoJSON info = GenerateInfoFile(data);
            string infoJSON = JsonUtility.ToJson(info,false);
            string songFolder = BeatMap.savePath + data.songName +"/";
            string levelFileName;
            if (data.difficulty == BeatSaveDifficulty.Easy) levelFileName = "Easy.json";
            else if (data.difficulty == BeatSaveDifficulty.Normal) levelFileName = "Normal.json";
            else if (data.difficulty == BeatSaveDifficulty.Hard) levelFileName = "Hard.json";
            else if (data.difficulty == BeatSaveDifficulty.Expert) levelFileName = "Expert.json";
            else levelFileName = "ExpertPlus.json";
            if (!Directory.Exists(songFolder)) Directory.CreateDirectory(songFolder);
            string levelOutputPath = songFolder + levelFileName;
            string infoOutputPath = songFolder + "info.json";
            WriteToDisk(infoOutputPath, infoJSON);
            WriteToDisk(levelOutputPath, levelJSON);
            BeatMap.Log("Save complete to file " + levelOutputPath);
            return beatSaberJSON;
        }


        private static void WriteToDisk(string fullPath,string content)
        {
            if (File.Exists(fullPath)) File.Delete(fullPath);
            FileStream fileStream = new FileStream(fullPath, FileMode.CreateNew);
            StreamWriter writer = new StreamWriter(fileStream);
            writer.Write(content);
            writer.Flush();
            writer.Close();
        }
        private static BeatSaberJSONClass.InfoJSON GenerateInfoFile(BeatMapData data)
        {
            BeatSaberJSONClass.InfoJSON info = new BeatSaberJSONClass.InfoJSON();
            info.authorName = data.mapArtist;
            info.songName = data.songName;
            info.songSubName = data.songArtist;
            info.beatsPerMinute = (int)data.beatsPerMinute;
            info.previewStartTime = 0;
            info.previewDuration = 10;
            info.coverImagePath = "cover.jpg";
            info.environmentName = "DefaultEnvironment";
            info.difficultyLevels = new BeatSaberJSONClass.DifficultyLevel[1];
            info.difficultyLevels[0] = BeatSaberJSONClass.DifficultyLevel.Generate(data.difficulty,
                data.songFileName, (int)data.songOffset);
            
            return info;
        }
    }
}
