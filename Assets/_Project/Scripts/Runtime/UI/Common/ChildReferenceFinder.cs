using UnityEngine;

namespace WheelOfFortune.UI
{
    /// <summary>
    /// Finds child components by GameObject name. Used from OnValidate so references are
    /// assigned automatically from the naming convention instead of being dragged by hand.
    /// </summary>
    public static class ChildReferenceFinder
    {
        public static T Find<T>(Component root, string childName) where T : Component
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                if (child.name == childName && child.TryGetComponent(out T component))
                {
                    return component;
                }
            }

            Debug.LogWarning($"{root.name}: child '{childName}' with {typeof(T).Name} was not found.", root);
            return null;
        }
    }
}
