using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvSafeDelivery.ViewModels;

namespace AvSafeDelivery;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        // Try to map ViewModel type to a View type in the Views namespace.
        var vmFullName = param.GetType().FullName!;

        // Replace the typical ViewModel suffix with View and map ViewModels namespace to Views namespace.
        var name = vmFullName.Replace("ViewModel", "View", StringComparison.Ordinal)
                             .Replace(".ViewModels.", ".Views.", StringComparison.Ordinal);

        var type = Type.GetType(name);

        // fallback: try the simple replace (original behavior) in case of different namespaces
        if (type == null)
        {
            var alt = vmFullName.Replace("ViewModel", "View", StringComparison.Ordinal);
            type = Type.GetType(alt);
        }

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}