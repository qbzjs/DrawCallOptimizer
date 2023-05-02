using CodeBase.UI.Windows.Common;
using TMPro;
using UnityEngine;

namespace CodeBase.UI.Windows
{
    public class ErrorWindow : WindowBase
    {
        [SerializeField] private TextMeshProUGUI _errorText;

        public void Construct(GameObject hero) =>
            base.Construct(hero);

        // protected override void Initialize() =>
        //     _errorText.text = CurrentError;
    }
}