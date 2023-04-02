using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer
{
    public class GameManager : MonoBehaviour
    {
        public AudioClip MusicSound;
        public AudioClip GotArtifactSound;
        public AudioClip AllArtifactsSound;
        public float MusicVolume = 1;

        public int coinsCounter = 0;

        public GameObject playerGameObject;
        public GameObject deathPlayerPrefab;
        public Text coinText;

        public Color HiddenArtifactColor = new Color(0, 0, 0, 0.5f);
        public GameObject ArtifactTemplate;
        public RectTransform ArtifactsContainer;

        private PlayerController player;

        private int remainingArtifacts;

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

                remainingArtifacts++;
            }

            if (!SoundPlayer.Instance.IsPlaying(MusicSound))
                SoundPlayer.Instance.Play(MusicSound, MusicVolume, true);
        }

        void Update()
        {
            coinText.text = coinsCounter.ToString();

            if (player.deathState)
            {
                var playerTransform = playerGameObject.transform;
                playerGameObject.SetActive(false);
                GameObject deathPlayer = Instantiate(deathPlayerPrefab,
                    playerTransform.position, playerTransform.rotation);
                deathPlayer.transform.localScale = playerTransform.localScale;
                player.deathState = false;
                // Invoke("ReloadLevel", 3);
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

            if (visible)
            {
                remainingArtifacts--;

                if (remainingArtifacts > 0)
                    SoundPlayer.Instance.Play(GotArtifactSound);
                else
                    SoundPlayer.Instance.Play(AllArtifactsSound);
            }
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