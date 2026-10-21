using UnityEngine;
using WheelOfFortune.Core;
using WheelOfFortune.Data;
using WheelOfFortune.Gameplay;
using WheelOfFortune.UI.Popups;
using WheelOfFortune.UI.Rewards;
using WheelOfFortune.UI.Wheel;
using WheelOfFortune.UI.Zone;

namespace WheelOfFortune.Game
{
    /// <summary>
    /// Composition root of the game scene: creates the session and connects it to the views.
    /// Holds no game rules itself; those live in <see cref="GameSession"/>.
    /// </summary>
    public sealed class GameController : MonoBehaviour
    {
        [SerializeField] private ZoneConfig _zoneConfig;
        [SerializeField] private WheelView _wheelView;
        [SerializeField] private RewardPanelView _rewardPanelView;
        [SerializeField] private RewardFlyView _rewardFlyView;
        [SerializeField] private ZoneBarView _zoneBarView;
        [SerializeField] private NextZoneCardView _safeZoneCard;
        [SerializeField] private NextZoneCardView _superZoneCard;
        [SerializeField] private BustedPopupView _bustedPopup;
        [SerializeField] private CollectedPopupView _collectedPopup;

        private ZoneProgression _progression;
        private GameSession _session;
        private WheelData _currentWheel;

        private void Awake()
        {
            _progression = _zoneConfig.CreateProgression();
            _session = new GameSession(_progression, new SystemRandomProvider());
        }

        private void OnEnable()
        {
            _wheelView.SpinClicked += HandleSpinClicked;
            _rewardPanelView.ExitClicked += HandleExitClicked;
            _bustedPopup.GiveUpClicked += HandleGiveUpClicked;
            _collectedPopup.ContinueClicked += HandleContinueClicked;
            _session.ZoneChanged += HandleZoneChanged;
            _session.StateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            _wheelView.SpinClicked -= HandleSpinClicked;
            _rewardPanelView.ExitClicked -= HandleExitClicked;
            _bustedPopup.GiveUpClicked -= HandleGiveUpClicked;
            _collectedPopup.ContinueClicked -= HandleContinueClicked;
            _session.ZoneChanged -= HandleZoneChanged;
            _session.StateChanged -= HandleStateChanged;
        }

        private void Start()
        {
            _rewardPanelView.Bind(_session.Inventory);
            _zoneBarView.Initialize(_progression.GetZoneType, _session.CurrentZone);
            ShowZone(_session.CurrentZone);
            HandleStateChanged(_session.State);
        }

        private void HandleSpinClicked()
        {
            if (!_session.CanSpin) return;

            SpinOutcome outcome = _session.Spin(_currentWheel.Slices);
            _wheelView.PlaySpin(outcome.SliceIndex, () => HandleSpinFinished(outcome));
        }

        private void HandleSpinFinished(SpinOutcome outcome)
        {
            if (outcome.IsBomb)
            {
                _session.CompleteSpin();
                return;
            }

            // The reward is added only after it has visibly reached the panel.
            Vector3 from = _wheelView.GetSliceIconPosition(outcome.SliceIndex);
            Vector3 to = _rewardPanelView.GetFlyTarget(outcome.Reward);
            _rewardFlyView.Play(outcome.Reward.Icon, from, to, _session.CompleteSpin);
        }

        private void HandleExitClicked()
        {
            if (!_session.CanLeave) return;
            _session.Leave();
        }

        private void HandleGiveUpClicked() => _bustedPopup.Hide(_session.Restart);

        private void HandleContinueClicked() => _collectedPopup.Hide(_session.Restart);

        private void HandleZoneChanged(int zone)
        {
            ShowZone(zone);
            _zoneBarView.SetZone(zone);
        }

        private void HandleStateChanged(SessionState state)
        {
            _wheelView.SetSpinInteractable(state == SessionState.ReadyToSpin);
            _rewardPanelView.SetExitInteractable(_session.CanLeave);

            switch (state)
            {
                case SessionState.Busted:
                    _bustedPopup.Show();
                    break;
                case SessionState.Collected:
                    // Show first so the popup is active and its layout can be built while filling it.
                    _collectedPopup.Show();
                    _collectedPopup.SetRewards(_session.Inventory);
                    break;
            }
        }

        private void ShowZone(int zone)
        {
            _currentWheel = _zoneConfig.GetWheel(_progression.GetZoneType(zone));
            _wheelView.SetWheel(_currentWheel, slice => _progression.ScaleRewardAmount(slice.BaseAmount, zone));
            _safeZoneCard.SetZone(_progression.GetNextZone(zone, ZoneType.Safe));
            _superZoneCard.SetZone(_progression.GetNextZone(zone, ZoneType.Super));
        }
    }
}
