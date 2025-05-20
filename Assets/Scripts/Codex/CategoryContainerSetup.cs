// using UnityEngine;
// using UnityEngine.UI;

// public class CategoryContainerSetup : MonoBehaviour
// {
//     public Transform categoryContainer; // Assign this in the Inspector

//     public void SetupButtonContainer()
//     {
//         // Add Horizontal Layout Group
//         HorizontalLayoutGroup horizontalLayoutGroup = categoryContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
//         horizontalLayoutGroup.childControlWidth = false;
//         horizontalLayoutGroup.childControlHeight = true;
//         horizontalLayoutGroup.childForceExpandWidth = false;
//         horizontalLayoutGroup.childForceExpandHeight = false;
//         horizontalLayoutGroup.spacing = 20; // Set the spacing between buttons

//         // Add Content Size Fitter
//         ContentSizeFitter contentSizeFitter = categoryContainer.gameObject.AddComponent<ContentSizeFitter>();
//         contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
//         contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

//         // Set the position and size of the ButtonContainer
//         RectTransform buttonContainerRect = categoryContainer.GetComponent<RectTransform>();
//         buttonContainerRect.anchorMin = new Vector2(0.5f, 1);
//         buttonContainerRect.anchorMax = new Vector2(0.5f, 1);
//         buttonContainerRect.pivot = new Vector2(0.5f, 1);
//         buttonContainerRect.anchoredPosition = new Vector2(0, -100); // Set the Y position to 100
//         buttonContainerRect.sizeDelta = new Vector2(Screen.width, 50); // Adjust the width as needed
//     }
// }
