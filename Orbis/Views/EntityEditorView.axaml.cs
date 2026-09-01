using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Orb.Engine.Graph;
using Orb.Engine.Types;
using Orbpad.Orbis.ViewModels;


namespace Orbpad.Orbis.Views;

public partial class EntityEditorView : UserControl
{
    private string? _editingPropertyName;

    public EntityEditorView()
    {
        InitializeComponent();
    }

    // ============================================================
    // EDITOR EVENTS
    // ============================================================

    public event EventHandler? SaveRequested;

    public event EventHandler? OpenRequested;

    public event EventHandler? CloseRequested;


    // ============================================================
    // SAVE / OPEN / CLOSE BUTTONS
    // ============================================================

    private void Save_Click(
        object? sender,
        RoutedEventArgs e)
    {
        SaveRequested?.Invoke(
            this,
            EventArgs.Empty);
    }


    private void Open_Click(
        object? sender,
        RoutedEventArgs e)
    {
        OpenRequested?.Invoke(
            this,
            EventArgs.Empty);
    }


    private void Close_Click(
        object? sender,
        RoutedEventArgs e)
    {
        CloseRequested?.Invoke(
            this,
            EventArgs.Empty);
    }


    // ============================================================
    // PROPERTY EDITOR
    // ============================================================

    private void SaveProperty_Click(object? sender, RoutedEventArgs e)
    {
        ClearPropertyError();

        string name = PropertyNameBox.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            ShowPropertyError("Property name is required.");
            return;
        }

        if (DataContext is not EntityEditorViewModel viewModel)
        {
            ShowPropertyError("The entity editor is not connected to a ViewModel.");
            return;
        }

        if (PropertyTypeComboBox.SelectedItem is not ComboBoxItem selectedItem)
        {
            ShowPropertyError("Select a property type.");
            return;
        }

        string typeName = selectedItem.Content?.ToString() ?? string.Empty;
        string rawValue = PropertyValueBox.Text ?? string.Empty;

        try
        {
            OrbValue value = CreateOrbValue(typeName, rawValue);

            if (_editingPropertyName is null)
                viewModel.AddProperty(name, value);
            else
                viewModel.UpdateProperty(_editingPropertyName, name, value);

            ClearPropertyForm();
        }
        catch (Exception ex)
        {
            ShowPropertyError(ex.Message);
        }
    }

    private void EditProperty_Click(object? sender, RoutedEventArgs e)
    {
        ClearPropertyError();

        if (sender is not Button button ||
            button.DataContext is not OrbProperty property)
        {
            ShowPropertyError("The selected property could not be loaded.");
            return;
        }

        _editingPropertyName = property.Name;
        PropertyEditorTitle.Text = $"Edit Property: {property.Name}";
        PropertySaveButton.Content = "Save Property";
        PropertyCancelButton.IsVisible = true;
        PropertyNameBox.Text = property.Name;
        PropertyValueBox.Text = property.Value?.Value?.ToString() ?? string.Empty;

        PropertyTypeComboBox.SelectedItem =
            FindPropertyTypeItem(property.Value?.Type);

        if (PropertyTypeComboBox.SelectedItem is null)
            PropertyTypeComboBox.SelectedIndex = 1;

        PropertyNameBox.Focus();
    }

    private void RemoveProperty_Click(object? sender, RoutedEventArgs e)
    {
        ClearPropertyError();

        if (sender is not Button button ||
            button.DataContext is not OrbProperty property)
        {
            ShowPropertyError("The selected property could not be removed.");
            return;
        }

        if (DataContext is not EntityEditorViewModel viewModel)
        {
            ShowPropertyError("The entity editor is not connected to a ViewModel.");
            return;
        }

        viewModel.RemoveProperty(property);

        if (string.Equals(_editingPropertyName, property.Name, StringComparison.Ordinal))
            ClearPropertyForm();
    }

    private void CancelPropertyEdit_Click(object? sender, RoutedEventArgs e)
    {
        ClearPropertyForm();
    }

    private ComboBoxItem? FindPropertyTypeItem(OrbValueType? type)
    {
        if (type is null)
            return null;

        string typeName = type.Value.ToString();

        foreach (var item in PropertyTypeComboBox.Items)
        {
            if (item is ComboBoxItem comboBoxItem &&
                string.Equals(comboBoxItem.Content?.ToString(), typeName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return comboBoxItem;
            }
        }

        return null;
    }


    private static OrbValue CreateOrbValue(
        string typeName,
        string rawValue)
    {
        return typeName switch
        {
            "Null" =>
                new OrbValue(
                    OrbValueType.Null,
                    null),

            "String" =>
                new OrbValue(
                    OrbValueType.String,
                    rawValue),

            "Boolean" =>
                CreateBooleanValue(
                    rawValue),

            "Integer" =>
                CreateIntegerValue(
                    rawValue),

            "Decimal" =>
                CreateDecimalValue(
                    rawValue),

            "DateTime" =>
                CreateDateTimeValue(
                    rawValue),

            "Guid" =>
                CreateGuidValue(
                    rawValue),

            _ =>
                throw new InvalidOperationException(
                    $"Unsupported property type '{typeName}'.")
        };
    }


    private static OrbValue CreateBooleanValue(
        string value)
    {
        if (!bool.TryParse(
                value,
                out bool result))
        {
            throw new FormatException(
                "Boolean values must be 'true' or 'false'.");
        }

        return new OrbValue(
            OrbValueType.Boolean,
            result);
    }


    private static OrbValue CreateIntegerValue(
        string value)
    {
        if (!long.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out long result))
        {
            throw new FormatException(
                "Integer values must be valid whole numbers.");
        }

        return new OrbValue(
            OrbValueType.Integer,
            result);
    }


    private static OrbValue CreateDecimalValue(
        string value)
    {
        if (!decimal.TryParse(
                value,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out decimal result))
        {
            throw new FormatException(
                "Decimal values must be valid decimal numbers.");
        }

        return new OrbValue(
            OrbValueType.Decimal,
            result);
    }


    private static OrbValue CreateDateTimeValue(
        string value)
    {
        if (!DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime result))
        {
            throw new FormatException(
                "DateTime value could not be parsed.");
        }

        return new OrbValue(
            OrbValueType.DateTime,
            result);
    }


    private static OrbValue CreateGuidValue(
        string value)
    {
        if (!Guid.TryParse(
                value,
                out Guid result))
        {
            throw new FormatException(
                "Guid value must be a valid GUID.");
        }

        return new OrbValue(
            OrbValueType.Guid,
            result);
    }


    // ============================================================
    // PROPERTY FORM
    // ============================================================

    private void ClearPropertyForm()
    {
        _editingPropertyName = null;
        PropertyEditorTitle.Text = "Add Property";
        PropertySaveButton.Content = "Add Property";
        PropertyCancelButton.IsVisible = false;
        PropertyNameBox.Text = string.Empty;
        PropertyValueBox.Text = string.Empty;
        PropertyTypeComboBox.SelectedIndex = 1;
        ClearPropertyError();
        PropertyNameBox.Focus();
    }


    private void ClearPropertyError()
    {
        PropertyErrorText.Text = string.Empty;
        PropertyErrorText.IsVisible = false;
    }


    private void ShowPropertyError(
        string message)
    {
        PropertyErrorText.Text =
            message;

        PropertyErrorText.IsVisible =
            true;
    }
}