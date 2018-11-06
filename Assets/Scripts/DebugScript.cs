using UnityEngine;
using System.Collections;
using com.rak.BeatMaker;

public class DebugScript : MonoBehaviour
{
    public bool run = false;
    // Update is called once per frame
    void Update()
    {
        if (run)
        {
            NoteDetails change = new NoteDetails();
            change.gridPosition = new Vector2(0, 1);
            change.inverted = true;
            change.timeToSpawn = 2.375f;
            MiniMap.AddNoteChange(change);
            run = false;
        }
    }
}
