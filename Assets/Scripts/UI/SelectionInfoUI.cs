using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TurnClash.Units;

namespace TurnClash.UI
{
    public class SelectionInfoUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject selectionPanel;
        [SerializeField] private TextMeshProUGUI unitNameText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI playerText;
        [SerializeField] private TextMeshProUGUI positionText;
        
        [Header("Settings")]
        [SerializeField] private bool hideWhenNoSelection = true;
        
        private UnitSelectable currentSelectedUnit;
        
        private void Start()
        {
            // Subscribe to selection events
            if (UnitSelectionManager.Instance != null)
            {
                UnitSelectionManager.Instance.OnUnitSelected += OnUnitSelected;
                UnitSelectionManager.Instance.OnSelectionCleared += OnSelectionCleared;
            }
            
            // Initially hide the panel
            if (hideWhenNoSelection && selectionPanel != null)
            {
                selectionPanel.SetActive(false);
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (UnitSelectionManager.Instance != null)
            {
                UnitSelectionManager.Instance.OnUnitSelected -= OnUnitSelected;
                UnitSelectionManager.Instance.OnSelectionCleared -= OnSelectionCleared;
            }
        }
        
        private void Update()
        {
            // Update UI if we have a selected unit
            if (currentSelectedUnit != null)
            {
                UpdateUnitInfo();
            }
        }
        
        private void OnUnitSelected(UnitSelectable selectedUnit)
        {
            // For single selection, show the selected unit
            // For multiple selection, show the first/last selected unit
            currentSelectedUnit = selectedUnit;
            
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(true);
            }
            
            UpdateUnitInfo();
        }
        
        private void OnSelectionCleared()
        {
            currentSelectedUnit = null;
            
            if (hideWhenNoSelection && selectionPanel != null)
            {
                selectionPanel.SetActive(false);
            }
            else
            {
                ClearUnitInfo();
            }
        }
        
        private void UpdateUnitInfo()
        {
            if (currentSelectedUnit == null) return;
            
            Unit unit = currentSelectedUnit.GetUnit();
            if (unit == null) return;
            
            // Update unit name
            if (unitNameText != null)
            {
                unitNameText.text = !string.IsNullOrEmpty(unit.name) ? unit.name : "Unknown Unit";
            }
            
            // Update health info
            if (healthText != null)
            {
                healthText.text = $"Health: {unit.health}/{unit.maxHealth}";
                
                // Color code health
                float healthPercentage = (float)unit.health / unit.maxHealth;
                if (healthPercentage > 0.6f)
                    healthText.color = Color.green;
                else if (healthPercentage > 0.3f)
                    healthText.color = Color.yellow;
                else
                    healthText.color = Color.red;
            }
            
            // Update player info
            if (playerText != null)
            {
                playerText.text = $"Player: {unit.player}";
                
                // Color code by player
                switch (unit.player)
                {
                    case Creature.Player.Player1:
                        playerText.color = Color.blue;
                        break;
                    case Creature.Player.Player2:
                        playerText.color = Color.red;
                        break;
                    default:
                        playerText.color = Color.white;
                        break;
                }
            }
            
            // Update position info
            if (positionText != null)
            {
                Vector2Int gridPos = unit.GetGridPosition();
                positionText.text = $"Position: ({gridPos.x}, {gridPos.y})";
            }
        }
        
        private void ClearUnitInfo()
        {
            if (unitNameText != null)
                unitNameText.text = "No Unit Selected";
                
            if (healthText != null)
            {
                healthText.text = "Health: --/--";
                healthText.color = Color.white;
            }
            
            if (playerText != null)
            {
                playerText.text = "Player: --";
                playerText.color = Color.white;
            }
            
            if (positionText != null)
                positionText.text = "Position: (--, --)";
        }
        
        // Public methods for manual control
        public void ShowSelectionInfo(bool show)
        {
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(show);
            }
        }
        
        public void SetHideWhenNoSelection(bool hide)
        {
            hideWhenNoSelection = hide;
        }
    }
} 