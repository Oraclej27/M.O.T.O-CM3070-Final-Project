// =============================================
// Script: LevelMusic.cs
// Purpose: Holds a reference to an AudioClip for background music. SoundController finds this in each scene. 
//
// Communicates with:
//   - SoundController: Found through FindObjectOfType; 

// Usage: Attached to an empty GameObject in each level.
// =============================================
using UnityEngine;

public class LevelMusic : MonoBehaviour
{
    public AudioClip levelMusic;

    void Start()
    {
        // To hold the music clip
    }
}