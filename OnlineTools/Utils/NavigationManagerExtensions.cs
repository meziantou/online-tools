#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace OnlineTools.Utils
{
    public static class NavigationManagerExtensions
    {
        public static void UpdateUrlUsingParameters(this NavigationManager navigationManager, IComponent component, bool replaceHistory = false)
        {
            var parameters = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (var property in GetProperties(component.GetType()))
            {
                var parameterName = GetQueryStringParameterName(property);
                if (parameterName == null)
                    continue;

                var value = property.GetValue(component);
                parameters[parameterName] = value;
            }

            // Compute the new URL
            var newUri = navigationManager.GetUriWithQueryParameters(parameters);
            if (newUri != navigationManager.Uri)
            {
                navigationManager.NavigateTo(newUri, new NavigationOptions { ReplaceHistoryEntry = replaceHistory });
            }
        }

        private static PropertyInfo[] GetProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        }

        private static string? GetQueryStringParameterName(PropertyInfo property)
        {
            if (property.GetCustomAttribute<ParameterAttribute>() is null)
                return null;

            var attribute = property.GetCustomAttribute<SupplyParameterFromQueryAttribute>();
            if (attribute is null)
                return null;

            return attribute.Name ?? property.Name;
        }
    }
}
