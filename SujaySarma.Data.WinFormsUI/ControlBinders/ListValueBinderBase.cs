using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.Core.TypeDiscovery;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace SujaySarma.Data.WinFormsUI.ControlBinders;

/// <summary>
/// Extends the <see cref="ControlBinderBase{TControl}"/>, for controls that bind to a list of values 
/// and provide the ability to select one or more of them.
/// </summary>
/// <typeparam name="TControl">The type of control being bound. 
/// Must be one of: <see cref="CheckedListBox"/>, a <see cref="ListBox"/> or a <see cref="ComboBox"/>.</typeparam>
/// <typeparam name="TValue">The type of values that would be a part of the control's presentation.</typeparam>
internal abstract class ListValueBinderBase<TControl, TValue> : ControlBinderBase<TControl>
    where TControl : ListControl
{
    /// <summary>
    /// Initialise the list of values based control binder.
    /// </summary>
    /// <param name="member">The member property or field participating in the binding.</param>
    /// <param name="control">The control of type <typeparamref name="TControl"/> participating in the binding.</param>
    /// <param name="enumerableSource">An enumeration of values of type <typeparamref name="TValue"/> that are to be bound to the <paramref name="control"/> as choices.</param>
    /// <param name="displayMember">The property of <typeparamref name="TValue"/> that is to be displayed on the UI.</param>
    /// <param name="valueMember">The property of <typeparamref name="TValue"/> that is to be used as the member item's internal value.</param>
    /// <param name="bindingDirection">The direction of binding.</param>
    protected ListValueBinderBase(PersistenceContainerMemberInfo member, TControl control,
        IEnumerable<TValue> enumerableSource, string? displayMember = null, string? valueMember = null, BindingDirection bindingDirection = BindingDirection.TwoWay)
        : base(member, control, bindingDirection)
    {
        Type typeOfValue = typeof(TValue);
        MemberInfo? displayMemberInfo = null;
        MemberInfo? valueMemberInfo = null;

        if (!string.IsNullOrWhiteSpace(displayMember))
        {
            if (!typeOfValue.TryGetProperty(displayMember, BindingFlags.Public | BindingFlags.Instance, out PropertyInfo? displayProperty))
            {
                typeOfValue.TryGetField(displayMember, BindingFlags.Public | BindingFlags.Instance, out FieldInfo? displayField);
                displayMemberInfo = displayField;
            }
            else
            {
                displayMemberInfo = displayProperty;
            }
        }

        if (!string.IsNullOrWhiteSpace(valueMember))
        {
            if (!typeOfValue.TryGetProperty(valueMember, BindingFlags.Public | BindingFlags.Instance, out PropertyInfo? valueProperty))
            {
                typeOfValue.TryGetField(valueMember, BindingFlags.Public | BindingFlags.Instance, out FieldInfo? valueField);
                valueMemberInfo = valueField;
            }
            else
            {
                valueMemberInfo = valueProperty;
            }
        }

        // Materialise.
        List<TValue> source = enumerableSource.Materialise<TValue>(acceptNullElements: false, throwExceptionOnNull: true);
        if (!source.Any())
        {
            throw new ArgumentException($"'{base.ControlPartner.Name}': The provided value source does not contain any elements.", nameof(enumerableSource));
        }

        _bindingSource = new List<BindingKeyValuePair>();
        foreach (TValue item in source)
        {
            if (item is null)
            {
                throw new ArgumentException($"'{base.ControlPartner.Name}': '{nameof(enumerableSource)}' contains one or more NULL values.", nameof(enumerableSource));
            }

            string display = ((displayMemberInfo != null)
                                ? item.GetValue(displayMemberInfo)?.ToString()
                                : item.ToString())
                              ?? string.Empty;

            object? value = (valueMemberInfo != null)
                                ? (item.GetValue(valueMemberInfo) ?? item)
                                : display;

            _bindingSource.Add(new BindingKeyValuePair()
            {
                Key = display,
                Value = value
            });
        }

        control.DataSource = _bindingSource;
        control.DisplayMember = nameof(BindingKeyValuePair.Key);
        control.ValueMember = nameof(BindingKeyValuePair.Value);
    }

    /// <summary>
    /// Initialise the list of values based control binder.
    /// </summary>
    /// <param name="member">The member property or field participating in the binding.</param>
    /// <param name="control">The control of type <typeparamref name="TControl"/> participating in the binding.</param>
    /// <param name="dictionarySource">A dictionary of values (Key: of type <see cref="string"/> is the displayed text on the UI, 
    /// Value: of type <typeparamref name="TValue"/> is the internal value of the item) to be bound to the <paramref name="control"/> as choices.</param>
    /// <param name="bindingDirection">The direction of binding.</param>
    protected ListValueBinderBase(PersistenceContainerMemberInfo member, TControl control, Dictionary<string, TValue> dictionarySource, BindingDirection bindingDirection = BindingDirection.TwoWay)
        : base(member, control, bindingDirection)
    {
        _bindingSource = new List<BindingKeyValuePair>();
        foreach (KeyValuePair<string, TValue> item in dictionarySource)
        {
            _bindingSource.Add(
                    new BindingKeyValuePair()
                    {
                        Key = item.Key,
                        Value = ((item.Value is null) ? item.Key : item.Value)
                    }
                );
        }

        control.DataSource = _bindingSource;
        control.DisplayMember = "Key";
        control.ValueMember = "Value";
    }


    private readonly List<BindingKeyValuePair> _bindingSource;
}


/// <summary>
/// Represents a key-value pair for binding purposes.
/// </summary>
public class BindingKeyValuePair
{

    /// <summary>
    /// The displayable value.
    /// </summary>
    public string Key
    {
        get;
        set;

    } = default!;

    /// <summary>
    /// The internal value.
    /// </summary>
    public object Value
    {
        get;
        set;

    } = default!;


}