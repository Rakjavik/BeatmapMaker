using UnityEngine;
using System.Collections;
using UnityEngine.UI;
namespace com.rak.BeatMaker
{
    public class InfoDisplay : MonoBehaviour
    {
        public AudioClip[] clips;
        public bool isBeatDisplay;
        public bool isNoteDisplay;
        private Text infoText;
        private AudioSource audioSource;
        public bool isSaberdisplay;
        // Use this for initialization
        void Start()
        {
            Initialize();
        }
        private void Initialize()
        {
            audioSource = GetComponent<AudioSource>();
            infoText = GetComponentInChildren<Text>();
        }
        public void Refresh()
        {
            Update();
        }
        private void Update()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            if (isNoteDisplay)
            {
                Note note = GetComponentInParent<Note>();
                if (note.noteDetails.inverted != BeatMap.Inverted)
                {
                    gameObject.SetActive(false);
                }
                else
                {
                    if(!gameObject.activeSelf)
                    {
                        gameObject.SetActive(true);
                    }
                    SetText(note.noteDetails.timeToSpawn.ToString());
                }
                return;
            }
            else if (isBeatDisplay)
            {
                SetText(GetComponentInParent<BeatBar>().GetBeatNumber().ToString());
            }
            else if (BeatMap.currentFrame % 15 == 0 && !isSaberdisplay)
            {
                string text = "Mode - " + BeatMap.GetCurrentState() + "\nCurrent Beat - " + BeatMap.GetCurrentBeat()
                    + "\nRecord Mode - ";
                if (BeatMap.currentNoteMode == BeatMap.NoteMode.HalfNote) text += "Half Notes";
                else if (BeatMap.currentNoteMode == BeatMap.NoteMode.QuarterNote) text += "Quarter Notes";
                else if (BeatMap.currentNoteMode == BeatMap.NoteMode.WholeNote) text += "Whole Notes";

                SetText(text);
            }
        }
        public void SetText(string text)
        {
            if (infoText == null) infoText = GetComponentInChildren<Text>();
            if (infoText == null) Debug.LogError("Text Object not found");
            infoText.text = text;
        }
        public void AppendText(string text)
        {
            infoText.text += text;
        }
        public void PlaySave()
        {
            if (!audioSource.isPlaying)
                audioSource.PlayOneShot(clips[0]);
        }
        public void PlayLoad()
        {
            if (!audioSource.isPlaying)
                audioSource.PlayOneShot(clips[1]);
        }
    }
}