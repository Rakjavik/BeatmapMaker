using UnityEngine;
using System.Collections;
namespace com.rak.BeatMaker
{
    public class ControlsDiagram : MonoBehaviour
    {
        public Material ViveWandEditMode;
        public Material ViveWandRecordMode;
        public Material ViveWandPlaybackMode;
        public Material ViveWandPlaybackPausedMode;
        public Material ViveWand;

        private Material currentMaterial;
        private BeatMap.EditorState lastKnownState;
        private bool initialized = false;

        public void Initialize(BeatMap.EditorState currentState)
        {
            lastKnownState = currentState;
            initialized = true;
        }

        public void Refresh()
        {
            if (!initialized) return;
            BeatMap.EditorState currentState = BeatMap.GetCurrentState();
            if(currentState == BeatMap.EditorState.Editing)
            {
                currentMaterial = ViveWandEditMode;
            }
            else if (currentState == BeatMap.EditorState.Recording)
            {
                currentMaterial = ViveWandRecordMode;
            }
            else if (currentState == BeatMap.EditorState.Playback)
            {
                currentMaterial = ViveWandPlaybackMode;
            }
            else if (currentState == BeatMap.EditorState.PlaybackPaused)
            {
                currentMaterial = ViveWandPlaybackPausedMode;
            }
            else
            {
                currentMaterial = ViveWand;
            }
        }
        public void UpdateMe(BeatMap.EditorState currentState)
        {
            if(currentState != lastKnownState)
            {
                lastKnownState = currentState;
                Refresh();
            }
        }
    }
}