﻿namespace Launcher.App.Support;

internal static class DependencyObjectExtensions
{
    public static T? FindVisualChild<T>(this DependencyObject parent, string? childName = null)
        where T : DependencyObject
    {
        var foundChild = default(T);

        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            // If the child is not of the request child type child
            var childType = child as T;
            if (childType == null)
            {
                // recursively drill down the tree
                foundChild = FindVisualChild<T>(child, childName);

                // If the child is found, break so we do not overwrite the found child.
                if (foundChild != null)
                    break;
            }
            else if (!string.IsNullOrEmpty(childName))
            {
                // If the child's name is set for search
                if (
                    child is FrameworkElement frameworkElement && frameworkElement.Name == childName
                )
                {
                    // if the child's name is of the request name
                    foundChild = (T)child;
                    break;
                }
            }
            else
            {
                // child element found.
                foundChild = (T)child;
                break;
            }
        }

        return foundChild;
    }
}
