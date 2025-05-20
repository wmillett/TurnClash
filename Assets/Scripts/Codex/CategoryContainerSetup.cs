// using UnityEngine;
// using UnityEngine.UI;

// public class CategoryContainerSetup : MonoBehaviour
// {
//     [SerializeField] private float topMargin = 5f; // Distance from the top of parent - smaller value = higher position
//     [SerializeField] private float containerHeight = 40f;

//     private void Start()
//     {
//         // SetupContainer();
//     }

//     // public void SetupContainer()
//     // {
//     //     RectTransform categoryRect = GetComponent<RectTransform>();
//     //     if (categoryRect != null)
//     //     {
//     //         // Reset scale to prevent size issues
//     //         categoryRect.localScale = Vector3.one;
            
//     //         // Set proper anchoring and size
//     //         categoryRect.anchorMin = new Vector2(0, 1);  // Anchor to top-left
//     //         categoryRect.anchorMax = new Vector2(1, 1);  // Anchor to top-right
//     //         categoryRect.pivot = new Vector2(0.5f, 1);   // Pivot at top-center
            
//     //         // Position it with a small margin from the top (smaller = higher position)
//     //         categoryRect.anchoredPosition = new Vector2(0, -topMargin);
            
//     //         // Set the fixed height and make it stretch horizontally
//     //         categoryRect.sizeDelta = new Vector2(0, containerHeight);
//     //     }
//     // }
// }
