using UnityEngine;
using System.Collections;
namespace com.rak.BeatMaker
{
    public class SaberSelector : MonoBehaviour
    {
        private MeshRenderer meshRenderer;
        private GameObject attachedObject;
        // Use this for initialization
        void Start()
        {
            attachedObject = null;
            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.enabled = false;
        }

        public void Attach(GameObject gameObject)
        {
            BeatMap.Log("Attach called on saber selector, current attachmetn - " + attachedObject);
            if (attachedObject != null)
            {
                BeatMap.Log("Saber Selector called to attach when already attached");
                return;
            }
            attachedObject = gameObject;
            transform.position = gameObject.transform.position;
            transform.SetParent(gameObject.transform, true);
            meshRenderer.enabled = true;
        }
        public void Detach()
        {
            attachedObject = null;
            meshRenderer.enabled = false;
            transform.SetParent(null);
        }
        public GameObject GetAttachedObject()
        {
            return attachedObject;
        }
        public bool HasTarget()
        {
            return attachedObject != null;
        }
    }
}