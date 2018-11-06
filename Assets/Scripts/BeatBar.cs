using UnityEngine;
using System.Collections;
namespace com.rak.BeatMaker
{
    public class BeatBar : MonoBehaviour
    {
        private InfoDisplay infoDisplay;
        private int beatNumber;
        private bool textInitalized = false;


        private void Start()
        {
            infoDisplay = GetComponentInChildren<InfoDisplay>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!textInitalized)
            {
                Initialize();
            }
            if (!BeatMap.IsRunning()) return;
            if (gameObject.activeSelf)
            {
                if (!BeatMap.IsRewinding)
                    transform.position += (transform.forward * Note.noteSpeed) * Time.deltaTime;
                else
                    transform.position -= (transform.forward * Note.noteSpeed) * Time.deltaTime;
            }
        }
        public void SetBeatNumber(int beatNumber)
        {
            this.beatNumber = beatNumber;
            Initialize();
        }
        public int GetBeatNumber() { return beatNumber; }

        private void Initialize()
        {
            infoDisplay = GetComponentInChildren<InfoDisplay>();
            if (infoDisplay == null) Debug.LogError("InfoDisplay not present");
            infoDisplay.SetText(beatNumber.ToString());
            textInitalized = true;
        }
    }
}