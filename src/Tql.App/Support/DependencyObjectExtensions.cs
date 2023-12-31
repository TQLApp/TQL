﻿using System.Windows.Controls.Primitives;

namespace Tql.App.Support;

internal static class DependencyObjectExtensions
{
    public static T? FindVisualParent<T>(this DependencyObject element)
    {
        // First try to resolve the visual parent of content elements
        // like text Run's.

        while (
            element != null
            && element is not Visual
            && element is FrameworkContentElement contentElement
        )
        {
            element = contentElement.Parent;
        }

        if (element == null)
            return default;

        for (
            var parent = VisualTreeHelper.GetParent(element);
            parent != null;
            parent = VisualTreeHelper.GetParent(parent)
        )
        {
            if (parent is T typedParent)
                return typedParent;
        }

        return default;
    }

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
            else if (childName != null)
            {
                // If the child's name is set for search
                if (
                    child is FrameworkElement frameworkElement
                    && frameworkElement.Name == childName
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

    public static T? FindSelectedItemVisualChild<T>(this Selector self, string name)
    {
        var selectedItem = self.SelectedItem;
        if (selectedItem == null)
            return default;

        var selectedItemObject = self.ItemContainerGenerator.ContainerFromItem(selectedItem);
        if (selectedItemObject == null)
            return default;

        var contentPresenter = FindVisualChild<ContentPresenter>(selectedItemObject);
        if (contentPresenter == null)
            return default;

        if (contentPresenter.ContentTemplate.FindName(name, contentPresenter) is T result)
            return result;

        return default;
    }
}
