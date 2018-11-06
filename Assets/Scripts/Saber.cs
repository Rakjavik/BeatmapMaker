using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
namespace com.rak.BeatMaker
{
    public partial class Saber : MonoBehaviour, IGrabber
    {
        public bool IgnoreErrors = false;
        public SteamVR_TrackedObject controller;
        public BeatMap map;
        public InfoDisplay infoDisplay;
        public GameObject hand;
        public SaberSelector selector;
        public Saber otherSaber;

        private BoxCollider boxCollider;
        private Rigidbody thisRigidBody;
        private SteamVR_Controller.Device steamVRController;
        private bool initialized = false;
        private List<Note> notesInContact;
        private Vector3 previousPosition;
        private Vector3 velocity;
        private bool isLeft;
        private bool triggerDown = false;
        private bool menuButtonDown = false;
        private float triggerHeldFor = 0;
        private float gripHeldFor = 0;
        private float actionCoolDown = .5f;
        private float currentCooldown = 0;
        private int debugCutNumber = 0;
        private bool grabbing = false;
        private FlatMenu flatMenu;

        // Use this for initialization
        public void Initialize(bool vrPlayer)
        {
            if (!vrPlayer)
            {
                transform.SetParent(null);
                if (!gameObject.activeSelf)
                    gameObject.SetActive(true);
                initialized = true;
            }
            else
            {
                steamVRController = SteamVR_Controller.Input((int)controller.index);
                if (!steamVRController.valid)
                    Debug.LogError(gameObject.name + " on device index " + controller.index + "is not Active!");
                else
                    initialized = true;
                if (initialized)
                {
                    transform.localScale = new Vector3(.02f, .02f, 1);
                    transform.SetParent(controller.transform);
                    transform.localPosition = new Vector3(0, 0, .4f);
                    transform.localRotation = Quaternion.identity;
                }
            }
            thisRigidBody = GetComponent<Rigidbody>();
            selector = GetComponentInChildren<SaberSelector>();
            isLeft = gameObject.name.ToLower().Contains("left");
            boxCollider = GetComponent<BoxCollider>();
            notesInContact = new List<Note>();
            previousPosition = transform.position;
            vibrationQueue = new List<VibrationRequest>();
        }

        private void Update()
        {
            if (!initialized && BeatMap.isVrPlayer)
            {
                steamVRController = SteamVR_Controller.Input((int)controller.index);
                if (!steamVRController.valid)
                {
                    if (!IgnoreErrors)
                        Debug.LogError(gameObject.name + " on device index " + controller.index + "is not Active!");
                }
                else
                {
                    Debug.Log("Controller initialized successfully - " + controller.name);
                    initialized = true;
                }
            }
            if (triggerDown) triggerHeldFor += Time.deltaTime;
            // Calculate direction saber is heading //
            if (transform.position != previousPosition)
            {
                Vector3 deltaPosition = (previousPosition - transform.position);
                velocity = deltaPosition / Time.deltaTime;
                previousPosition = transform.position;
            }

            if (currentCooldown > 0) currentCooldown += Time.deltaTime;
            if (currentCooldown > actionCoolDown)
            {
                currentCooldown = 0;
            }
            if (vibrate)
            {
                vibrationQueue[0].Update(Time.deltaTime);
                if (vibrationQueue[0].done)
                {
                    vibrationQueue.Remove(vibrationQueue[0]);
                    if (vibrationQueue.Count == 0)
                    {
                        vibrate = false;
                    }
                }
            }
            if (IsOnCoolDown())
            {
                Debug.LogWarning("Current cooldown - " + currentCooldown);
            }
            if (!IsOnCoolDown())
                ProcessControls(BeatMap.isVrPlayer);
        }

        private void ProcessControls(bool vrControls)
        {
            bool triggerCooldown = false;
            BeatMap.EditorState currentState = BeatMap.GetCurrentState();
            if (vrControls && steamVRController.GetPressDown(EVRButtonId.k_EButton_SteamVR_Trigger) ||
                Input.GetKeyDown(KeyCode.Space) && !vrControls)
            {
                triggerDown = true;
                if(!BeatMap.Inverted)
                    map.InvertNotes();
            }
            else if (vrControls && steamVRController.GetPressUp(EVRButtonId.k_EButton_SteamVR_Trigger) ||
                Input.GetKeyUp(KeyCode.Space) && !vrControls)
            {
                triggerDown = false;
                if (BeatMap.Inverted)
                    map.InvertNotes();
            }
            else if (vrControls && steamVRController.GetPressDown(EVRButtonId.k_EButton_ApplicationMenu) ||
                Input.GetKeyDown(KeyCode.A) && !vrControls)
            {
                menuButtonDown = true;
                bool bothControllersPressing = otherSaber.menuButtonDown;
                
                if(bothControllersPressing)
                {
                    if(currentState == BeatMap.EditorState.Editing || 
                        currentState == BeatMap.EditorState.PlaybackPaused)
                    {
                        map.ResumeSong();
                        triggerCooldown = true;
                        return;
                    }
                    else if (currentState == BeatMap.EditorState.Playback ||
                        currentState == BeatMap.EditorState.Recording)
                    {
                        map.PauseSong();
                        triggerCooldown = true;
                        return;
                    }
                }
                else
                {
                    if(currentState == BeatMap.EditorState.Editing)
                    {
                        if (selector.GetAttachedObject() != null)
                        {
                            Note note = selector.GetAttachedObject().GetComponent<Note>();
                            if (note != null)
                            {
                                if (!note.noteDetails.inverted)
                                {
                                    note.MakeNeutral();
                                    if(isLeft)
                                        note.Invert(true);
                                    DetachSelector();
                                    triggerCooldown = true;
                                }
                            }
                        }
                    }
                }
            }
            else if (vrControls && steamVRController.GetPressUp(EVRButtonId.k_EButton_ApplicationMenu) ||
                Input.GetKeyUp(KeyCode.A) && !vrControls)
            {
                menuButtonDown = false;
            }
            if (vrControls && steamVRController.GetPressDown(EVRButtonId.k_EButton_SteamVR_Touchpad) ||
                Input.GetKeyDown(KeyCode.Y) && !vrControls)
            {
                
            }
            if (vrControls && steamVRController.GetTouchDown(EVRButtonId.k_EButton_SteamVR_Touchpad) ||
                Input.GetKeyDown(KeyCode.T) && !vrControls)
            {
                if (currentState == BeatMap.EditorState.Recording)
                {
                    BeatMap.currentNoteMode = BeatMap.NoteMode.HalfNote;
                }
            }
            else if (vrControls && steamVRController.GetTouchUp(EVRButtonId.k_EButton_SteamVR_Touchpad) ||
                Input.GetKeyUp(KeyCode.T) && !vrControls)
            {
                if (currentState == BeatMap.EditorState.Recording)
                {
                    BeatMap.currentNoteMode = BeatMap.NoteMode.WholeNote;
                }
            }
            if (vrControls && steamVRController.GetPressDown(EVRButtonId.k_EButton_Grip) ||
                Input.GetKeyDown(KeyCode.G) && !vrControls)
            {
                if (gripHeldFor == 0) // Fresh push
                {

                    if (selector.HasTarget())
                    {
                        BeatMap.Log("Calling Grab on object " + selector.GetAttachedObject().name);
                        Grab(selector.GetAttachedObject());
                        if (selector.GetAttachedObject().GetComponent<MiniMap>() != null)
                            map.PauseSong();
                        triggerCooldown = true;
                    }
                }
                gripHeldFor += Time.deltaTime;
            }
            else if (vrControls && steamVRController.GetPressUp(EVRButtonId.k_EButton_Grip) ||
                Input.GetKeyUp(KeyCode.G) && !vrControls)
            {
                BeatMap.Log("Grip let go, grabbing? " + grabbing);
                gripHeldFor = 0;
                if (grabbing)
                {
                    UnGrab();
                    triggerCooldown = true;
                }
            }
            // KEYBOARD ONLY //
            if (!vrControls)
            {
                if (Input.GetKeyDown(KeyCode.W))
                {
                    transform.position += transform.forward;
                }
                else if (Input.GetKeyDown(KeyCode.A))
                {
                    transform.position += -transform.right;
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    transform.position += -transform.forward;
                }
                else if (Input.GetKeyDown(KeyCode.D))
                {
                    transform.position += transform.right;
                }
                else if (Input.GetKeyDown(KeyCode.Q))
                {
                    transform.position += transform.up;
                }
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    transform.position += -transform.up;
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    BeatMap.SkipToBeat(BeatMap.GetCurrentBeat() + 10);
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    BeatMap.SkipToBeat(BeatMap.GetCurrentBeat() - 10);
                }
                else if (Input.GetKeyDown(KeyCode.P) && !IsOnCoolDown() && isLeft)
                {
                    triggerCooldown = true;
                    if(BeatMap.GetCurrentState() == BeatMap.EditorState.Editing)
                    {
                        map.ResumeSong();
                    }
                    else if (BeatMap.GetCurrentState() == BeatMap.EditorState.PlaybackPaused)
                    {
                        map.ResumeSong();
                    }
                    else if (BeatMap.GetCurrentState() == BeatMap.EditorState.Playback)
                    {
                        map.PauseSong();
                    }
                    else if (BeatMap.GetCurrentState() == BeatMap.EditorState.Recording)
                    {
                        map.PauseSong();
                    }
                }
            }
            if (triggerCooldown) currentCooldown += Time.deltaTime;
        }
        private bool UnGrab()
        {
            IGrabbable grabbable = selector.GetAttachedObject().GetComponent<IGrabbable>();
            if (grabbable == null) grabbable = selector.GetAttachedObject().GetComponentInParent<IGrabbable>();
            if (grabbable != null)
            {
                grabbable.UnGrab();
            }
            else
            {
                BeatMap.Log("Ungrab called on an object that doesn't have a grabbable " + selector.GetAttachedObject().name);
            }
            grabbing = false;
            return true;
        }
        private bool Grab(GameObject gameObject)
        {
            BeatMap.Log("Saber called grab on " + gameObject.name);
            IGrabbable grabbable = gameObject.GetComponent<IGrabbable>();
            if (grabbable == null) grabbable = gameObject.GetComponentInParent<IGrabbable>();
            if (grabbable == null)
            {
                Debug.LogWarning("Object not grabbable - " + gameObject.name);
                return false;
            }
            if (grabbable.IsGrabbed())
            {
                Debug.LogWarning("Object already grabbed - " + gameObject.name);
                return false;
            }
            grabbable.Grab(this);
            grabbing = true;
            addVibration(.5f, .5f, true);
            return true;
        }

        public Note.SlashDirection GetSlashDirection(Note note)
        {
            return GetSlashDirection(note.transform);
        }
        public bool IsOnCoolDown() { return currentCooldown > 0; }
        private Note.SlashDirection GetSlashDirection(Transform target)
        {
            float dotUp = Vector3.Dot(target.up, velocity.normalized);
            float dotRight = Vector3.Dot(target.right, velocity.normalized);
            float minVelocity = .5f;
            //Debug.Log("Cut #" + debugCutNumber + " Dot up, dotright - " + dotUp + "-" + dotRight);
            debugCutNumber++;
            bool movingRight = dotRight < -minVelocity;
            bool movingUp = dotUp < -minVelocity;
            bool movingLeft = dotRight > minVelocity;
            bool movingDown = dotUp > minVelocity;

            if (movingDown && !movingLeft && !movingRight)
            {
                return Note.SlashDirection.DOWN;
            }
            else if (movingDown && movingLeft)
            {
                return Note.SlashDirection.DOWNLEFT;
            }
            else if (movingDown && movingRight)
            {
                return Note.SlashDirection.DOWNRIGHT;
            }
            else if (movingLeft && !movingUp && !movingDown)
            {
                return Note.SlashDirection.LEFT;
            }
            else if (movingUp && !movingLeft && !movingRight)
            {
                return Note.SlashDirection.UP;
            }
            else if (movingUp && movingRight)
            {
                return Note.SlashDirection.UPRIGHT;
            }
            else if (movingUp && movingLeft)
            {
                return Note.SlashDirection.UPLEFT;
            }
            else if (movingRight && !movingUp && !movingDown)
            {
                return Note.SlashDirection.RIGHT;
            }

            return Note.SlashDirection.NONE;
        }
        public Note.NoteColor GetSaberColor()
        {
            if (name.ToLower().IndexOf("left") > 0)
            {
                return Note.NoteColor.LEFT;
            }
            else
            {
                return Note.NoteColor.RIGHT;
            }
        }
        public void AddContact(Note note)
        {
            notesInContact.Add(note);
        }
        public void RemoveContact(Note note)
        {
            notesInContact.Remove(note);
        }
        public void Attach(GameObject gameObject)
        {
            selector.Attach(gameObject);
        }
        public void DetachSelector()
        {
            selector.Detach();
        }
        #region Getters/Setters
        public Transform getControllerTransform()
        {
            return transform.parent;
        }
        public Vector3 GetVelocity()
        {
            return velocity;
        }
        public bool IsTriggerDown()
        {
            return triggerDown;
        }
        public SteamVR_Controller.Device GetMotionController() { return steamVRController; }
        #endregion

        #region Vibration
        private List<VibrationRequest> vibrationQueue;
        private bool vibrate = false;

        public void addVibration(float strength, float duration, bool overrideAllOthers)
        {
            if(BeatMap.isVrPlayer)
                addVibration(strength, duration, overrideAllOthers, null);
        }
        public void addVibration(VibrationRequest vr, bool overrideAllOthers)
        {
            addVibration(0, 0, overrideAllOthers, vr);
        }
        private void addVibration(float strength, float duration, bool overrideAllOthers, VibrationRequest vr)
        {
            if (vr == null) vr = new VibrationRequest(duration, strength, steamVRController, this);
            if (overrideAllOthers || vibrationQueue.Count == 0)
            {
                vibrationQueue = new List<VibrationRequest>();
                vibrationQueue.Add(vr);
            }
            else if (vibrationQueue.Count > 0)
            {
                // If the new requested vibration is harder then current, override //
                if (vibrationQueue[0].vibrateStrength < vr.vibrateStrength)
                {
                    // Current request will last longer then new request //
                    if (vibrationQueue[0].stopVibratingAt > duration)
                    {
                        // Move current request behind new request //
                        vibrationQueue[1] = vibrationQueue[0];
                        vibrationQueue[0] = vr;
                    }
                }

            }
            vibrate = true;
        }

        public Rigidbody GetRigidBody()
        {
            return thisRigidBody;
        }

        public Vector3 GetJointAnchorPoint()
        {
            return new Vector3(0, 2, 0);
        }
        #endregion
    }
}