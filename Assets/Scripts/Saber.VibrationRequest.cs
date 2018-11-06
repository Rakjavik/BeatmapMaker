using System.Collections;
using UnityEngine;
namespace com.rak.BeatMaker
{
    public partial class Saber
    {
        /// <summary>
        /// VIBRATION
        /// </summary>
        public class VibrationRequest
        {
            public float beenVibratingFor = 0;
            public float stopVibratingAt = 0;
            public float vibrateStrength = 0;

            public bool done;
            private bool initialized = false;
            public SteamVR_Controller.Device controller;
            public MonoBehaviour parent;

            public VibrationRequest(float duration, float strength, SteamVR_Controller.Device controller, MonoBehaviour parent)
            {
                this.parent = parent;
                this.stopVibratingAt = duration;
                this.vibrateStrength = strength;
                this.controller = controller;
                done = false;
            }
            public void Update(float deltaTime)
            {
                beenVibratingFor += deltaTime;
                if (!initialized)
                {
                    Coroutine coroutine = parent.StartCoroutine(vibrationThread(stopVibratingAt, vibrateStrength, controller));
                    initialized = true;
                }
                if (beenVibratingFor > stopVibratingAt)
                {
                    done = true;
                }
            }
            public float getTimeLeft()
            {
                if (done)
                {
                    return 0;
                }
                return stopVibratingAt - beenVibratingFor;
            }
            private IEnumerator vibrationThread(float length, float strength, SteamVR_Controller.Device device)
            {
                for (float i = 0; i < length; i += Time.deltaTime)
                {
                    device.TriggerHapticPulse((ushort)Mathf.Lerp(0, 3999, strength));
                    yield return null; //every single frame for the duration of "length" you will vibrate at "strength" amount
                }
            }
        }
    }
}