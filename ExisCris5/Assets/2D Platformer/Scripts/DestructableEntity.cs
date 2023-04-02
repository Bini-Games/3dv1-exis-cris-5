using UnityEngine;

namespace Platformer
{
    public class DestructableEntity : MonoBehaviour
    {
        public AudioClip DeathSound;

        public void Die()
        {
            SoundPlayer.Instance.Play(DeathSound);
            Destroy(gameObject);
        }
    }
}