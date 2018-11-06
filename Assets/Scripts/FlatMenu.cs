using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System;

namespace com.rak.BeatMaker
{
    public class FlatMenu : MonoBehaviour
    {
        public bool hideOnLoad = true;
        public Dropdown dirDropDown;
        public Dropdown difficultyDropdown;
        public Dropdown newDifficultyDropdown;
        public TMPro.TMP_InputField newSongNameTextBox;
        public TMPro.TMP_InputField audioPathTextBox;
        public TMPro.TMP_InputField songArtistTextBox;
        public TMPro.TMP_InputField mapArtistTextBox;
        public Dropdown noteModeDropDown;
        public TMPro.TMP_InputField bpmTextBox;
        private int selectedDirectory;
        private string[] directories;
        private string[] files;
        public BeatMap.NoteMode selectedMode;
        public string selectedNewSongName;
        public string selectedAudioPath;
        public float selectedBPM;
        public string selectedMapArtist;
        public string selectedSongArtist;
        public string selectedLoadSongFileName;
        public string selecedNewSongFileName;
        public BeatSaveDifficulty selectedDifficulty;

        // Use this for initialization
        void Start()
        {
            directories = Directory.GetDirectories(BeatMap.savePath);
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            foreach (string directory in directories)
            {
                Dropdown.OptionData option = new Dropdown.OptionData();
                option.text = directory.Substring(BeatMap.savePath.Length);
                options.Add(option);
            }
            dirDropDown.AddOptions(options);
            List<string> noteModeOptionStrings = new List<string>();
            noteModeOptionStrings.Add("Whole Note");
            noteModeOptionStrings.Add("Half Note");
            noteModeOptionStrings.Add("Quarter Note");
            noteModeOptionStrings.Add("Eighth Note");
            noteModeOptionStrings.Add("Double Note");
            noteModeDropDown.ClearOptions();
            List<Dropdown.OptionData> noteModeOptions = new List<Dropdown.OptionData>();
            foreach (string mode in noteModeOptionStrings)
            {
                Dropdown.OptionData option = new Dropdown.OptionData();
                option.text = mode;
                noteModeOptions.Add(option);
            }
            noteModeDropDown.AddOptions(noteModeOptions);
            List<Dropdown.OptionData> newDifficultyOptions = new List<Dropdown.OptionData>();
            for (int count = 0; count < Enum.GetNames(typeof(BeatSaveDifficulty)).Length; count++)
            {
                BeatSaveDifficulty currentDiff = (BeatSaveDifficulty)count;
                Dropdown.OptionData option = new Dropdown.OptionData(currentDiff.ToString());
                newDifficultyOptions.Add(option);
            }
            newDifficultyDropdown.ClearOptions();
            newDifficultyDropdown.AddOptions(newDifficultyOptions);
            OnDirValueChange();
            OnNewValuesChanged();
        }
        public string[] GetSelectedMapFullPath()
        {
            string[] returnStrings = new string[3];
            returnStrings[0] = directories[selectedDirectory] += "/";
            returnStrings[1] = difficultyDropdown.options[difficultyDropdown.value].text + ".json";
            returnStrings[2] = selectedLoadSongFileName;
            return returnStrings;
        }
        public void OnDirValueChange()
        {
            selectedDirectory = dirDropDown.value;
            difficultyDropdown.ClearOptions();
            files = Directory.GetFiles(directories[selectedDirectory]);
            bool infoFound = false;
            List<BeatSaveDifficulty> foundDifficulties = new List<BeatSaveDifficulty>();
            foreach (string file in files)
            {
                if (file.ToLower().Contains("info.json")) infoFound = true;
                else if (file.ToLower().Contains("easy.json")) foundDifficulties.Add(BeatSaveDifficulty.Easy);
                else if (file.ToLower().Contains("normal.json")) foundDifficulties.Add(BeatSaveDifficulty.Normal);
                else if (file.ToLower().Contains("hard.json")) foundDifficulties.Add(BeatSaveDifficulty.Hard);
                else if (file.ToLower().Contains("expert.json")) foundDifficulties.Add(BeatSaveDifficulty.Expert);
                else if (file.ToLower().Contains("expertplus.json")) foundDifficulties.Add(BeatSaveDifficulty.ExpertPlus);
                else if (file.ToLower().Contains("ogg"))
                {
                    selectedLoadSongFileName = Path.GetFileName(file);
                }
            }
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            if (infoFound) {
                for (int count = 0; count < foundDifficulties.Count; count++)
                {
                    Dropdown.OptionData option = new Dropdown.OptionData();
                    option.text = foundDifficulties[count].ToString();
                    options.Add(option);
                }
                difficultyDropdown.AddOptions(options);
            }
        }
        public void OnNewValuesChanged()
        {
            selectedMode = (BeatMap.NoteMode)noteModeDropDown.value;
            selectedAudioPath = audioPathTextBox.text;
            selecedNewSongFileName = Path.GetFileName(selectedAudioPath);
            try
            {
                selectedBPM = float.Parse(bpmTextBox.text);
            }
            catch (FormatException e) { }
            selectedNewSongName = newSongNameTextBox.text;
            selectedMapArtist = mapArtistTextBox.text;
            selectedSongArtist = songArtistTextBox.text;
            selectedDifficulty = (BeatSaveDifficulty)newDifficultyDropdown.value;
        }
        public bool IsReadyForNew()
        {
            if (selectedAudioPath == null || selectedBPM == -1
                || selectedMode == BeatMap.NoteMode.DoubleNote ||
                selectedNewSongName == null || selectedMapArtist == null ||
                selectedSongArtist == null)
                return false;

            if(!File.Exists(selectedAudioPath))
            {
                Debug.LogError("Song file not found at - " + selectedAudioPath);
                return false;
            }
            return true;
        }
    }
}
