using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Match3
{
    public class Exit : MonoBehaviour
    {
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(Application.Quit);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveAllListeners();
        }
    }
}
