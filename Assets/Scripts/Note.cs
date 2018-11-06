using UnityEngine;
using System.Collections;
using System;
namespace com.rak.BeatMaker
{
    [Serializable]
    public class Note : MonoBehaviour
    {
        public enum SlashDirection { LEFT, UP, RIGHT, DOWN, UPLEFT, UPRIGHT, DOWNLEFT, DOWNRIGHT,NONE }
        public enum NoteColor { LEFT, RIGHT, NONE }

        public float timeAlive = 0;
        public BoxCollider hitBox;
        public BoxCollider hurtBox;
        public AudioClip hitSound;
        public NoteDetails noteDetails;

        public static float GetFactor(BeatMap.NoteMode mode)
        {
            if (mode == BeatMap.NoteMode.HalfNote) return 2;
            else if (mode == BeatMap.NoteMode.QuarterNote) return 4;
            else if (mode == BeatMap.NoteMode.WholeNote) return 1;
            else if (mode == BeatMap.NoteMode.DoubleNote) return .5f;
            else
                return 8;
        }
        public static float noteSpeed = 5f;
        public static int alphaWhenHighlighted = 180;
        public static Color GetNoteColor(NoteColor color)
        {
            if (color == NoteColor.LEFT)
            {
                return Color.red;
            }
            else if (color == NoteColor.RIGHT)
            {
                return Color.blue;
            }
            else
            {
                return Color.grey;
            }
        }

        private static float cooldown = .5f;
        private float currentCooldown = 0;
        public bool IsOnCooldown() { return currentCooldown > 0; }
        private HitBox HitBox;
        private MeshRenderer meshRenderer;
        public Material[] materials;
        private Saber saberTouched;

        public void Initialize(NoteDetails noteDetails, Material[] availableMats, Vector3 spawnPosition)
        {
            if (materials == null)
                materials = availableMats;
            HitBox = new HitBox(hitBox, hurtBox);
            materials = availableMats;
            meshRenderer = GetComponent<MeshRenderer>();
            this.noteDetails = noteDetails;
            transform.position = spawnPosition;
            Refresh();
        }

        public void Refresh()
        {
            if(noteDetails.inverted != BeatMap.Inverted)
            {
                meshRenderer.enabled = false;
                InfoDisplay display = GetComponent<InfoDisplay>();
                if (display != null) display.gameObject.SetActive(false);
            }
            else
            {
                meshRenderer.enabled = true;
                InfoDisplay display = GetComponent<InfoDisplay>();
                if (display != null) display.gameObject.SetActive(true);
            }

            Material newMaterial = new Material(materials[BeatSaberJSONClass.
                BSNote.GetBSaberCutDirection(noteDetails.slashDirection)]);
            Color newColor = GetNoteColor(noteDetails.color);
            newMaterial.color = newColor;
            meshRenderer.material = newMaterial;
            InfoDisplay infoDisplay = GetComponentInChildren<InfoDisplay>();
            if (infoDisplay != null)
            {
                infoDisplay.Refresh();
            }
        }
        public void Invert(bool invert)
        {
            if (noteDetails.inverted == invert)
                Debug.LogWarning("Trying to invert an already inverted note");
            noteDetails.inverted = invert;
            Refresh();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsOnCooldown() || BeatMap.Inverted != noteDetails.inverted)
            {
                Debug.LogWarning("Note is on cooldown");
                return;
            }
            saberTouched = other.GetComponent<Saber>();
            if (saberTouched != null)
            {
                bool refresh = false;
                currentCooldown += Time.deltaTime;
                BeatMap.EditorState currentState = BeatMap.GetCurrentState();
                // During playback, remove notes as they are hit //
                if (noteDetails.IsVisible() && (currentState == BeatMap.EditorState.Playback ||
                    currentState == BeatMap.EditorState.Recording))
                {
                    Debug.LogWarning("INversion map-note - " + BeatMap.Inverted + "-" + noteDetails.inverted);
                    Debug.LogWarning("Current beat - " + BeatMap.GetCurrentBeat());
                    Debug.LogWarning("Note beat - " + noteDetails.timeToSpawn);
                    Debug.LogWarning("Difference - " + (BeatMap.GetCurrentBeat() - noteDetails.timeToSpawn));
                    saberTouched.addVibration(.5f, .05f, false);
                    meshRenderer.enabled = false;
                    noteDetails.inverted = !noteDetails.inverted;
                    Debug.LogWarning("Note changed to " + noteDetails.inverted);
                    refresh = true;
                }
                // During editing, highlight if the Note color has already been set //
                else if (currentState == BeatMap.EditorState.Editing && !noteDetails.inverted)
                {
                    saberTouched.Attach(gameObject);
                }
                
                // During editing if set to neutral, record saber direction and color //
                if ((currentState == BeatMap.EditorState.Editing || currentState == BeatMap.EditorState.Recording) 
                    && noteDetails.color == NoteColor.NONE)
                {
                    SlashDirection direction = saberTouched.GetSlashDirection(this);
                    NoteColor color = saberTouched.GetSaberColor();
                    noteDetails.color = color;
                    noteDetails.slashDirection = direction;
                    refresh = true;
                }
                if(refresh)
                {
                    Refresh();
                    Debug.LogWarning("Notiying mini map of change inverted?- " + noteDetails.inverted);
                    MiniMap.AddNoteChange(noteDetails);
                }
            }
        }

        public void MakeNeutral()
        {
            noteDetails.slashDirection = SlashDirection.NONE;
            noteDetails.color = NoteColor.NONE;
            Refresh();
        }
        
        // Update is called once per frame
        void Update()
        {
            if(currentCooldown > 0)
            {
                currentCooldown += Time.deltaTime;
                if(currentCooldown > cooldown)
                {
                    currentCooldown = 0;
                }
            }
            timeAlive += Time.deltaTime;
            if (!BeatMap.IsRunning()) return;
            if (gameObject.activeSelf)
            {
                if (!BeatMap.IsRewinding)
                    transform.position += (transform.forward * noteSpeed) * Time.deltaTime;
                else
                    transform.position -= (transform.forward * noteSpeed) * Time.deltaTime;
            }
        }

    }
    [Serializable]
    public struct NoteDetails
    {
        public bool IsVisible() { return BeatMap.Inverted == inverted; }

        public Note note;
        public Vector3 gridPosition;
        public Note.SlashDirection slashDirection;
        public Note.NoteColor color;
        public float timeToSpawn;
        public bool inverted;
        public NoteDetails(Note note,Vector3 gridPosition, Note.SlashDirection slashDirection,
            Note.NoteColor color, float timeToSpawn,bool inverted)
        {
            this.inverted = inverted;
            this.note = note;
            this.gridPosition = gridPosition;
            this.slashDirection = slashDirection;
            this.color = color;
            this.timeToSpawn = timeToSpawn;
        }

    }
}