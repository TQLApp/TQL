using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Tql.Utilities;

/// <summary>
/// Base class for DTO objects.
/// </summary>
/// <remarks>
/// This is a base class for DTO objects that provide proper integration
/// with WPF. It specifically implements <see cref="INotifyPropertyChanged"/>
/// and <see cref="INotifyDataErrorInfo"/> and provides an abstraction to
/// simplify proper usage.
/// </remarks>
public class DtoBase : INotifyPropertyChanged, INotifyDataErrorInfo
{
    private readonly Dictionary<string, Property> _properties = new();

    /// <inheritdoc/>
    public bool HasErrors => _properties.Values.Any(p => p.Error != null);

    /// <inheritdoc/>
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Add a new DTO property.
    /// </summary>
    /// <param name="name">Name of the property.</param>
    /// <param name="getError">Callback to generate errors messages.</param>
    /// <param name="coerceValue">Callback to coerce the value of the property.</param>
    protected void AddProperty(
        string name,
        Func<object?, string?>? getError = null,
        Func<object?, object?>? coerceValue = null
    )
    {
        var property = new Property(name, getError, coerceValue);

        property.SetValue(null);

        _properties.Add(name, property);
    }

    /// <summary>
    /// Utility method to validate that the value is not null or an empty string.
    /// </summary>
    /// <param name="value">Value to validate.</param>
    /// <returns>Whether the value is not null or an empty string.</returns>
    protected string? ValidateNotEmpty(object? value)
    {
        if (string.IsNullOrEmpty((string?)value))
            return Labels.ValidateNotEmpty;
        return null;
    }

    /// <summary>
    /// Utility method to validate that the value is a valid absolute URL.
    /// </summary>
    /// <param name="value">Value to validate.</param>
    /// <returns>Whether the value is a valid absolute URL.</returns>
    protected string? ValidateUrl(object? value)
    {
        if (value is string stringValue && !Uri.TryCreate(stringValue, UriKind.Absolute, out _))
            return Labels.ValidateUrl;
        return null;
    }

    /// <summary>
    /// Utility method to coerce an empty string to null.
    /// </summary>
    /// <param name="value">Value to coerce.</param>
    /// <returns>Coerced value.</returns>
    protected object? CoerceEmptyStringToNull(object? value) =>
        string.Empty.Equals(value) ? null : value;

    /// <summary>
    /// Gets the value of a property.
    /// </summary>
    /// <param name="name">Name of the property.</param>
    /// <returns>Value of the property.</returns>
    protected object? GetValue([CallerMemberName] string? name = null)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        return _properties[name].Value;
    }

    /// <summary>
    /// Sets the value of a property.
    /// </summary>
    /// <param name="value">Value of the property.</param>
    /// <param name="name">Name of the property.</param>
    protected void SetValue(object? value, [CallerMemberName] string? name = null)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        _properties[name].SetValue(value);

        OnPropertyChanged(new PropertyChangedEventArgs(name));
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasErrors)));
        OnErrorsChanged(new DataErrorsChangedEventArgs(name));
    }

    /// <summary>
    /// Get all errors of a property.
    /// </summary>
    /// <param name="propertyName">Property to get errors for.</param>
    /// <returns>Errors of a property.</returns>
    public IEnumerable<string> GetErrors(string? propertyName = null)
    {
        return _properties
            .Values.Where(p => p.Error != null && p.Name == propertyName)
            .Select(p => p.Error!);
    }

    IEnumerable INotifyDataErrorInfo.GetErrors(string? propertyName) => GetErrors(propertyName);

    private record Property(
        string Name,
        Func<object?, string?>? GetError,
        Func<object?, object?>? CoerceValue
    )
    {
        public object? Value { get; private set; }
        public string? Error { get; private set; }

        public void SetValue(object? value)
        {
            if (CoerceValue != null)
                value = CoerceValue(value);

            Value = value;
            Error = GetError?.Invoke(value);
        }
    }

    /// <summary>
    /// Clone the property values of a DTO object.
    /// </summary>
    /// <param name="clone">New instance to clone property values into.</param>
    /// <returns>Fully initialized clone.</returns>
    protected DtoBase Clone(DtoBase clone)
    {
        foreach (var property in _properties.Values)
        {
            clone._properties[property.Name].SetValue(property.Value);
        }

        return clone;
    }

    /// <summary>
    /// Raises the <see cref="ErrorsChanged"/> event.
    /// </summary>
    /// <param name="e">Event arguments.</param>
    protected virtual void OnErrorsChanged(DataErrorsChangedEventArgs e) =>
        ErrorsChanged?.Invoke(this, e);

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="e">Event arguments.</param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) =>
        PropertyChanged?.Invoke(this, e);
}
