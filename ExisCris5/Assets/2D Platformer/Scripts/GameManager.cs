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

        public Color HiddenArtifactColor = new Color(0, 0, 0, 0.5f);
        public GameObject ArtifactTemplate;
        public RectTransform ArtifactsContainer;

        private PlayerController player;

        void Start()
        {
            player = GameObject.Find("Player").GetComponent<PlayerController>();

            var artifactsInScene = GameObject.FindGameObjectsWithTag("Artifact");
            foreach (var artifactObject in artifactsInScene)
            {
                if (!artifactObject.activeInHierarchy)
                    continue;

                var spriteRenderer = artifactObject.GetComponent<SpriteRenderer>();
                AddArtifact(spriteRenderer.sprite, false);
            }
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

        public void AddArtifact(Sprite sprite, bool visible = true)
        {
            var image = FindArtifact(sprite);

            if (!image)
            {
                var artifact = Instantiate(ArtifactTemplate, ArtifactsContainer);
                artifact.SetActive(true);
                image = artifact.GetComponent<Image>();
                image.sprite = sprite;
            }

            image.color = visible ? Color.white : HiddenArtifactColor;
        }

        private Image FindArtifact(Sprite sprite)
        {
            foreach (Transform child in ArtifactsContainer.transform)
            {
                var image = child.GetComponent<Image>();

                if (image.sprite == sprite)
                    return image;
            }

            return null;
        }

        private void ReloadLevel()
        {
            Application.LoadLevel(Application.loadedLevel);
        }
    }
}