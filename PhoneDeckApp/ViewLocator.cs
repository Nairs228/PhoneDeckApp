using Avalonia.Controls;
using Avalonia.Controls.Templates;
using PhoneDeckApp.ViewModels;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PhoneDeckApp
{
    /// <summary>
    /// Given a view model, returns the corresponding view if possible.
    /// </summary>
    [RequiresUnreferencedCode(
        "Default implementation of ViewLocator involves reflection which may be trimmed away.",
        Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
    public class ViewLocator : IDataTemplate
    {
        public Control? Build(object? data)
        {
            if (data is null) return null;

            var viewModelName = data.GetType().Name;
            var viewName = viewModelName.Replace("ViewModel", "View");

            var assembly = typeof(ViewLocator).Assembly;
            var allTypes = string.Join("\n", assembly.GetTypes().Select(t => t.FullName));
            var type = assembly.GetTypes().FirstOrDefault(t => t.Name == viewName);

            return type != null
                ? (Control)Activator.CreateInstance(type)!
                : new TextBlock { Text = $"Ищу: {viewName}\n\nВсе типы:\n{allTypes}" };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}
