using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer
{
    public class GameManager : MonoBehaviour
    {
        public int coinsCounter = 0;

        public GameObject playerGameObject;
        public GameObject deathPlayerPrefab;
        public Text coinText;

        public GameObject ArtifactTemplate;
        public RectTransform ArtifactsContainer;
        
        private PlayerController player;

        void Start()
        {
            player = GameObject.Find("Player").GetComponent<PlayerController>();
        }

        void Update()
        {
            coinText.text = coinsCounter.ToString();

            if (player.deathState)
            {
                playerGameObject.SetActive(false);
                GameObject deathPlayer = Instantiate(deathPlayerPrefab,
                    playerGameObject.transform.position, playerGameObject.transform.rotation);
                deathPlayer.transform.localScale = new Vector3(playerGameObject.transform.localScale.x,
                    playerGameObject.transform.localScale.y, playerGameObject.transform.localScale.z);
                player.deathState = false;
                Invoke("ReloadLevel", 3);
            }
        }

        public void AddArtifact(Sprite sprite)
        {
            var artifact = Instantiate(ArtifactTemplate, ArtifactsContainer);
            artifact.SetActive(true);
            var image = artifact.GetComponent<Image>();
            image.sprite = sprite;
        }
        
        private void ReloadLevel()
        {
            Application.LoadLevel(Application.loadedLevel);
        }
    }
}