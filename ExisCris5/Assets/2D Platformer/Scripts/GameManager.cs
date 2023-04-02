using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Platformer
{
    public class GameManager : MonoBehaviour
    {
        public AudioClip MusicSound;
        public AudioClip GotCoinSound;
        public AudioClip GotArtifactSound;
        public AudioClip AllArtifactsSound;
        public float MusicVolume = 1;

        public Sprite RobotSprite;
        public Sprite HumanSprite;
        public AudioClip ProfessionDescription;

        public Image RobotContainer;
        public Image HumanContainer;
        public Button DescriptionButton;
        public GameObject[] ControlsObjects;
        public GameObject OverlayObject;

        public int coinsCounter = 0;

        public GameObject playerGameObject;
        public GameObject deathPlayerPrefab;
        public Text coinText;

        public float DeathRestartDelay = 3;
        public AudioClip RestartSound;

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

            RobotContainer.sprite = RobotSprite;
            HumanContainer.sprite = HumanSprite;
            DescriptionButton.onClick.AddListener(OnDescriptionButtonClick);
        }

        private void OnDescriptionButtonClick()
        {
            HumanContainer.gameObject.SetActive(true);
            
            if (!SoundPlayer.Instance.IsPlaying(ProfessionDescription))
                SoundPlayer.Instance.Play(ProfessionDescription);
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
                Invoke("ReloadLevel", DeathRestartDelay);
            }
        }

        public void ToggleUseJoystick()
        {
            player.UseJoystick = !player.UseJoystick;
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

                if (remainingArtifacts == 0)
                {
                    foreach (var controlsObject in ControlsObjects)
                        controlsObject.SetActive(false);

                    OverlayObject.SetActive(true);

                    player.IsUserControlled = false;

                    RobotContainer.gameObject.SetActive(true);
                }
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
            SoundPlayer.Instance.AddToQueue(RestartSound);
            Application.LoadLevel(Application.loadedLevel);
        }
    }
}