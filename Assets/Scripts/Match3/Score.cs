using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.Match3
{
    public class Score : MonoBehaviour
    {
        private Text _scoreText;
        public static UnityAction<uint> OnScoreChanged;

        private void Awake()
        {
            _scoreText = GetComponent<Text>();
            _scoreText.text = "Points: 0";
        }

        private void OnEnable()
        {
            OnScoreChanged += UpdateScoreText;
        }

        private void OnDisable()
        {
            OnScoreChanged -= UpdateScoreText;
        }

        private void UpdateScoreText(uint value)
        {
            _scoreText.text = "Points: " + value;
        }
    }
}
