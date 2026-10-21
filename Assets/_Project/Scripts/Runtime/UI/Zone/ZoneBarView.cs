using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Core;

namespace WheelOfFortune.UI.Zone
{
    /// <summary>
    /// Top bar showing nearby zone numbers with the current zone in a fixed highlight at the center.
    /// The highlight is drawn above the strip, so the current number has its own text on top of it while the
    /// cells slide underneath. Cells are recycled: after sliding one cell to the left the container snaps back
    /// and the numbers are rewritten, so an endless strip needs only a fixed number of cells.
    /// </summary>
    public sealed class ZoneBarView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ZoneCellView _cellPrefab;
        [SerializeField] private RectTransform _cellContainer;
        [SerializeField] private Image _currentHighlightImage;
        [SerializeField] private TMP_Text _currentNumberText;

        [Header("Layout")]
        [SerializeField, Min(1)] private int _cellsEachSide = 6;
        [SerializeField, Min(1f)] private float _cellWidth = 82f;

        [Header("Styles")]
        [SerializeField] private ZoneStyle _normalStyle = new ZoneStyle
        {
            HighlightColor = Color.white,
            CurrentNumberColor = new Color(0.1f, 0.1f, 0.1f),
            UpcomingNumberColor = Color.white
        };
        [SerializeField] private ZoneStyle _safeStyle = new ZoneStyle
        {
            HighlightColor = Color.white,
            CurrentNumberColor = Color.white,
            UpcomingNumberColor = new Color(0.45f, 0.85f, 0.2f)
        };
        [SerializeField] private ZoneStyle _superStyle = new ZoneStyle
        {
            HighlightColor = new Color(1f, 0.76f, 0f),
            CurrentNumberColor = new Color(0.1f, 0.1f, 0.1f),
            UpcomingNumberColor = new Color(1f, 0.76f, 0f)
        };
        [SerializeField, Range(0f, 1f)] private float _pastNumberAlpha = 0.35f;

        [Header("Animation")]
        [SerializeField, Min(0f)] private float _slideDuration = 0.4f;
        [SerializeField] private Ease _slideEase = Ease.OutCubic;
        [SerializeField] private float _currentNumberPunch = 0.25f;
        [SerializeField, Min(0f)] private float _currentNumberPunchDuration = 0.3f;

        private readonly List<ZoneCellView> _cells = new List<ZoneCellView>();
        private Func<int, ZoneType> _getZoneType;
        private Tween _slideTween;
        private int _shownZone;

        public void Initialize(Func<int, ZoneType> getZoneType, int zone)
        {
            _getZoneType = getZoneType ?? throw new ArgumentNullException(nameof(getZoneType));
            CreateCells();
            Show(zone);
        }

        public void SetZone(int zone)
        {
            // Finish any running slide first so its completion does not overwrite this zone.
            _slideTween?.Kill(complete: true);

            if (zone != _shownZone + 1)
            {
                // Restarts and jumps are shown immediately; only single steps forward slide.
                Show(zone);
                return;
            }

            _slideTween = _cellContainer
                .DOAnchorPosX(-_cellWidth, _slideDuration)
                .SetEase(_slideEase)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    Show(zone);
                    PunchCurrentNumber();
                });
        }

        private void OnDisable()
        {
            _slideTween?.Kill(complete: true);
            _currentNumberText.transform.DOKill(complete: true);
        }

        private void CreateCells()
        {
            if (_cells.Count > 0) return;

            // One extra cell on the right fills the gap while the strip slides left.
            int cellCount = _cellsEachSide * 2 + 2;
            for (int i = 0; i < cellCount; i++)
            {
                ZoneCellView cell = Instantiate(_cellPrefab, _cellContainer);
                cell.name = $"{_cellPrefab.name}_{i}";
                cell.RectTransform.anchoredPosition = new Vector2((i - _cellsEachSide) * _cellWidth, 0f);
                _cells.Add(cell);
            }
        }

        private void Show(int zone)
        {
            _shownZone = zone;
            _cellContainer.anchoredPosition = Vector2.zero;

            for (int i = 0; i < _cells.Count; i++)
            {
                int cellZone = zone - _cellsEachSide + i;
                if (cellZone < ZoneProgression.FirstZone)
                {
                    _cells[i].Hide();
                }
                else
                {
                    _cells[i].Show(cellZone, GetNumberColor(cellZone, zone));
                }
            }

            ZoneStyle currentStyle = GetStyle(zone);
            _currentHighlightImage.sprite = currentStyle.HighlightSprite;
            _currentHighlightImage.color = currentStyle.HighlightColor;
            _currentNumberText.text = zone.ToString();
            _currentNumberText.color = currentStyle.CurrentNumberColor;
        }

        private void PunchCurrentNumber()
        {
            Transform textTransform = _currentNumberText.transform;
            textTransform.DOKill(complete: true);
            textTransform.DOPunchScale(Vector3.one * _currentNumberPunch, _currentNumberPunchDuration, vibrato: 4)
                .SetLink(gameObject);
        }

        /// <summary>
        /// Cell colors never use the current style: the current cell is covered by the highlight, and when it slides
        /// out it must stay readable on the dark strip.
        /// </summary>
        private Color GetNumberColor(int cellZone, int currentZone)
        {
            Color color = GetStyle(cellZone).UpcomingNumberColor;
            if (cellZone < currentZone) color.a *= _pastNumberAlpha;
            return color;
        }

        private ZoneStyle GetStyle(int zone) => _getZoneType(zone) switch
        {
            ZoneType.Safe => _safeStyle,
            ZoneType.Super => _superStyle,
            _ => _normalStyle
        };
    }
}
