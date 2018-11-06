using System.Collections.Generic;
using UnityEngine;
namespace com.rak.BeatMaker
{

    public class HitBox
    {
        private BoxCollider hitBox;
        private BoxCollider hurtBox;
        private List<Hit> collisions;

        public HitBox(BoxCollider hitBox, BoxCollider hurtBox)
        {
            this.hitBox = hitBox;
            this.hurtBox = hurtBox;
        }

        public void AddCollision(Collision collision)
        {
            Hit hit = new Hit(collision.contacts[0].normal,
                collision.rigidbody.GetComponent<Saber>().GetSaberColor());
            AddCollision(hit);
        }

        public void AddCollision(Vector3 point, Note.NoteColor saberColor)
        {
            AddCollision(new Hit(point, saberColor));
        }
        private void AddCollision(Hit collision)
        {
            BeatMap.Log("Hit - " + collision.point + " By - " + collision.saberColor.ToString());
            collisions.Add(collision);
        }
        public void Disable()
        {
            hitBox.enabled = false;
            hurtBox.enabled = false;
        }
        public void ReEnable()
        {
            hitBox.enabled = true;
            hurtBox.enabled = true;
        }

        public struct Hit
        {
            public Vector3 point;
            public Note.NoteColor saberColor;
            public Hit(Vector3 point, Note.NoteColor saberColor)
            {
                this.point = point;
                this.saberColor = saberColor;
            }
        }
    }
}