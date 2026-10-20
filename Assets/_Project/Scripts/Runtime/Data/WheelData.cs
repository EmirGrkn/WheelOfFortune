using System;
using System.Collections.Generic;
using UnityEngine;

namespace WheelOfFortune.Data
{
    [CreateAssetMenu(fileName = "wheel_", menuName = "Wheel Of Fortune/Wheel")]
    public sealed class WheelData : ScriptableObject
    {
        // The provided wheel sprites have 8 sockets, so the slice count is fixed by the art.
        public const int SliceCount = 8;

        [Header("Visuals")]
        [SerializeField] private Sprite _baseSprite;
        [SerializeField] private Sprite _indicatorSprite;
        [SerializeField] private string _title;
        [SerializeField] private Color _titleColor = Color.white;

        [Header("Slices (clockwise, starting from the top)")]
        [SerializeField] private WheelSliceData[] _slices = new WheelSliceData[SliceCount];

        public Sprite BaseSprite => _baseSprite;
        public Sprite IndicatorSprite => _indicatorSprite;
        public string Title => _title;
        public Color TitleColor => _titleColor;
        public IReadOnlyList<WheelSliceData> Slices => _slices;

        public bool HasBomb => Array.Exists(_slices, slice => slice != null && slice.IsBomb);

        private void OnValidate()
        {
            if (_slices == null || _slices.Length != SliceCount)
            {
                Array.Resize(ref _slices, SliceCount);
            }
        }
    }
}
