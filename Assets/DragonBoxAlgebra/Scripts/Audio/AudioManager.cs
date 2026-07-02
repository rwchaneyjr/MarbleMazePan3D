using UnityEngine;

namespace DragonBoxAlgebra.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource _source;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _source = GetComponent<AudioSource>();
            _source.playOnAwake = false;
        }

        public void PlayCombine() => PlayTone(520f, 0.12f, 0.35f);
        public void PlayMergeToOne() => PlayTone(740f, 0.1f, 0.3f);
        public void PlayCardPlay() => PlayTone(380f, 0.08f, 0.25f);
        public void PlayUndo() => PlayTone(260f, 0.1f, 0.22f);
        public void PlayWin() => PlayTone(880f, 0.25f, 0.4f);

        private void PlayTone(float frequency, float duration, float volume)
        {
            _source.PlayOneShot(CreateTone(frequency, duration, volume));
        }

        private static AudioClip CreateTone(float frequency, float duration, float volume)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = 1f - t / duration;
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * volume * envelope;
            }

            var clip = AudioClip.Create("Tone", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
