using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace com.rak.BeatMaker
{
    public class MiniMap : MonoBehaviour, IGrabbable
    {
        public const float MIN_Z = .053f;//-.6858f;
        public const float MAX_Z = 6.01f;
        public const float NoteSpacingXY = 71;

        public GameObject noteBlue;
        public GameObject noteRed;
        public GameObject miniBarPrefab;
        public GameObject miniBarNoDisplayPrefab;
        public bool showNotes = true;

        private float totalBeats;
        private float currentWidthOfMinimap;
        private static NoteDetails[] notes;
        private Dictionary<NoteDetails,GameObject> littleNoteMap;
        private FixedJoint joint;
        private static bool resizing = false;
        private BeatBar currentTimeBar;
        private bool initialized = false;
        private static float zPerBeat;

        private static List<NoteDetails> changes;

        public static void AddNoteChange(NoteDetails change)
        {
            int noteIndex = BeatMapData.GetNoteWithSamePosition(change, notes);
            if(noteIndex > -1)
                changes.Add(notes[noteIndex]);
        }

        public bool IsGrabbed()
        {
            return resizing;
        }
        public void SetZBasedOnBeat(float beat)
        {
            float beatOffset = beat * zPerBeat;
            float newZ = MAX_Z - beatOffset;
            Vector3 newPosition = transform.position;
            newPosition.z = newZ;
            transform.position = newPosition;
            Debug.LogWarning("New Z - " + newZ);
        }
        public void Grab(IGrabber grabber)
        {
            Rigidbody thisRigidBody = GetComponent<Rigidbody>();
            BeatMap.Log("grab called on " + gameObject.name + " already grabbed? - " + IsGrabbed());
            if (IsGrabbed()) return;
            resizing = true;
            joint = gameObject.AddComponent<FixedJoint>();
            BeatMap.Log("Adding joint to " + gameObject.name);
            joint.connectedBody = grabber.GetRigidBody();
            joint.anchor = Vector3.zero;
            joint.connectedAnchor = grabber.GetJointAnchorPoint();
        }
        public void UnGrab()
        {
            resizing = false;
            if (joint != null) Destroy(joint);
            float relativeBeat = (MAX_Z+transform.position.z) * zPerBeat;
            Debug.LogWarning("Relative - " + relativeBeat);
            relativeBeat = BeatMap.RoundToRelativeBeat(relativeBeat, BeatMap.currentNoteMode);
            BeatMap.SkipToBeat(relativeBeat);
        }
        // Use this for initialization
        public void Initialize(int totalBeats,float beatsThatFit)
        {
            zPerBeat = (MAX_Z - MIN_Z) / totalBeats;
            changes = new List<NoteDetails>();
            littleNoteMap = new Dictionary<NoteDetails, GameObject>();
            currentWidthOfMinimap = 1;
            Debug.LogWarning("Minimap width - " + currentWidthOfMinimap);
            this.totalBeats = totalBeats;
            notes = BeatMap.GetAllNotes(true);
            GameObject container = new GameObject("NoteContainer");
            container.transform.SetParent(transform);
            container.transform.localPosition = Vector3.zero;
            foreach (NoteDetails note in notes)
            {
                GameObject smallNote;
                if (note.color == Note.NoteColor.LEFT)
                    smallNote = Instantiate(noteRed);
                else
                    smallNote = Instantiate(noteBlue);
                smallNote.transform.SetParent(container.transform);
                smallNote.transform.localPosition = Vector3.zero;
                Vector3 newPosition = new Vector3(
                    note.gridPosition.x / NoteSpacingXY, 
                    note.gridPosition.y / NoteSpacingXY, 
                    MIN_Z + note.timeToSpawn * zPerBeat);
                smallNote.transform.localPosition = newPosition;
                if (!showNotes) smallNote.GetComponent<MeshRenderer>().enabled = false;
                littleNoteMap.Add(note, smallNote);
            }
            GameObject barContainer = new GameObject("BarContainer");
            barContainer.transform.SetParent(transform);
            barContainer.transform.position = transform.position;
            float startAtZ = barContainer.transform.position.z;

            currentTimeBar = Instantiate(miniBarPrefab).GetComponent<BeatBar>();
            currentTimeBar.name = "TimeBar";
            currentTimeBar.transform.SetParent(null);
            currentTimeBar.transform.position = new Vector3(1.306f, 1.47f, MAX_Z+.0447f);
            currentTimeBar.SetBeatNumber(0);
            
            BeatBar spawnPointBar = Instantiate(miniBarNoDisplayPrefab,currentTimeBar.transform)
                .GetComponent<BeatBar>();
            
            float totalZ = beatsThatFit * zPerBeat;
            spawnPointBar.transform.localScale = Vector3.one;
            spawnPointBar.transform.localPosition = Vector3.zero;
            Vector3 newSpawnBarPosition = spawnPointBar.transform.position;
            newSpawnBarPosition.z += totalZ;

            spawnPointBar.transform.position = newSpawnBarPosition;
            SetZBasedOnBeat(BeatMap.GetCurrentBeat());
            GameObject miniMapContainer = new GameObject("Minimap Container");
            transform.SetParent(miniMapContainer.transform);
            initialized = true;
        }

        private void Update()
        {
            if (!initialized) return;
            if (IsGrabbed())
            {
                if(transform.position.z > MAX_Z)
                {
                    Vector3 newPosition = transform.position;
                    newPosition.z = MAX_Z;
                    transform.position = newPosition;
                }
                else if (transform.position.z < MIN_Z)
                {
                    Vector3 newPosition = transform.position;
                    newPosition.z = MIN_Z;
                    transform.position = newPosition;
                }
                float relativeBeat = totalBeats - (transform.position.z/zPerBeat)-45;
                relativeBeat = BeatMap.RoundToRelativeBeat(relativeBeat, BeatMap.currentNoteMode);
                currentTimeBar.GetComponentInChildren<Text>().text = (relativeBeat*totalBeats).ToString();
            }
            else
                currentTimeBar.GetComponentInChildren<Text>().text = BeatMap.GetMostRecentBeat.ToString();
            if (BeatMap.IsRunning())
            {
                //Debug.Log("Current Beat - " + BeatMap.GetCurrentBeat() + " current Z - " 
                    //+ transform.position.z + " ZDiff - " + (MAX_Z-transform.position.z));
                Vector3 newPosition = transform.position;
                newPosition.z = MAX_Z - (zPerBeat * BeatMap.GetCurrentBeat());
                transform.position = newPosition;
            }
            if(changes.Count > 0)
            {
                //Debug.LogWarning("Changes detected - " + changes.Count);
                foreach(NoteDetails change in changes)
                {
                    littleNoteMap[change].GetComponent<MeshRenderer>().enabled =
                        !littleNoteMap[change].GetComponent<MeshRenderer>().enabled;
                    Debug.LogWarning("Meshrendered inverted on game object " + littleNoteMap[change].name);
                }
                changes = new List<NoteDetails>();
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (IsGrabbed()) return;
            SaberSelector selector = other.GetComponent<SaberSelector>();
            if (selector == null)
                selector = other.GetComponentInChildren<SaberSelector>();
            if (selector == null) return;
            selector.Attach(gameObject);
            /*Saber saber = other.gameObject.transform.GetComponentInParent<Saber>();
            if (saber != null)
            {
                float zPoint = Vector3.Distance()
                float relativeZ = (totalBeats/currentWidthOfMinimap) * zPoint;
                if (relativeZ < currentWidthOfMinimap) relativeZ = currentWidthOfMinimap;
                else if (relativeZ > totalBeats) relativeZ = totalBeats;
                Debug.LogWarning("relative z - " + relativeZ);
                BeatMap.SkipToBeat(BeatMap.RoundToRelativeBeat(relativeZ,BeatMap.currentNoteMode));
                Vector3 newposition = new Vector3(transform.position.x, transform.position.y,
                    currentWidthOfMinimap - ((currentWidthOfMinimap / totalBeats * 2) * relativeZ));
                transform.position = newposition;
                Update();
            }
            else
            {
                BeatMap.Log("saber NOT found");
            }*/
        }
    }
}