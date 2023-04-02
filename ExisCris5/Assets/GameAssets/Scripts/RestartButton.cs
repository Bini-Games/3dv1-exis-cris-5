using System.Collections;
using System.Collections.Generic;
using Platformer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartButton : MonoBehaviour
{
    public AudioClip RestartSound;

    public void RestartGame()
    {
        SoundPlayer.Instance.AddToQueue(RestartSound);
        // SoundPlayer.Instance.Play(RestartSound);
        
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Application.LoadLevel(Application.loadedLevel);
    }
}
