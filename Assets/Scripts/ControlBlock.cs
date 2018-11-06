using UnityEngine;
using System.Collections;
namespace com.rak.BeatMaker
{
    public class ControlBlock : MonoBehaviour
    {
        public enum Controls { REWIND, PAUSE, PLAY, INVERT, SAVE }

        public Controls thisControl;
        public BeatMap map;
        public static float cooldown = .75f;
        public bool IsOnCooldown
        {
            get
            {
                return currentCooldown > 0;
            }
        }

        private MeshRenderer meshRenderer;
        private float currentCooldown;

        private void Start()
        {
            currentCooldown = 0;
            meshRenderer = GetComponent<MeshRenderer>();
            if (thisControl == Controls.PAUSE)
            {
                if (BeatMap.IsRunning())
                {
                    meshRenderer.enabled = true;
                }
                else
                {
                    meshRenderer.enabled = false;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsOnCooldown) return;
            currentCooldown += Time.deltaTime;
            if (thisControl == Controls.INVERT)
            {
                map.InvertNotes();
            }
            else if (thisControl == Controls.PAUSE)
            {
                map.PauseSong();
            }
            else if (thisControl == Controls.PLAY)
            {
                map.ResumeSong();
            }
            else if (thisControl == Controls.REWIND)
            {
                map.Rewind();
            }
            else if (thisControl == Controls.SAVE)
            {
                map.SaveCurrentMapToDisk();
            }
        }
        private void Update()
        {
            if (currentCooldown > 0) currentCooldown += Time.deltaTime;
            if (currentCooldown > cooldown) currentCooldown = 0;
            if (thisControl == Controls.PAUSE)
            {
                if (BeatMap.IsRunning() && !meshRenderer.enabled)
                {
                    meshRenderer.enabled = true;
                }
                else if (!BeatMap.IsRunning() && meshRenderer.enabled)
                {
                    meshRenderer.enabled = false;
                }
            }
        }
    }
}