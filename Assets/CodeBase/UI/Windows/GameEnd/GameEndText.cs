﻿using CodeBase.Services.Localization;
using CodeBase.UI.Elements;
using TMPro;
using UnityEngine;

namespace CodeBase.UI.Windows.GameEnd
{
    public class GameEndText : BaseText
    {
        [SerializeField] private TextMeshProUGUI _writeReviewText;

        protected override void RuChosen()
        {
            Title.text = LocalizationConstants.GameEndTitleRu;
            _writeReviewText.text = LocalizationConstants.GameEndWriteReviewRu;
        }

        protected override void TrChosen()
        {
            Title.text = LocalizationConstants.GameEndTitleTr;
            _writeReviewText.text = LocalizationConstants.GameEndWriteReviewTr;
        }

        protected override void EnChosen()
        {
            Title.text = LocalizationConstants.GameEndTitleEn;
            _writeReviewText.text = LocalizationConstants.GameEndWriteReviewEn;
        }
    }
}