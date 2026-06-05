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

        if (!string.IsNullOrWhiteSpace(displayMember))
        {
            if (!typeOfValue.TryGetProperty(displayMember, BindingFlags.Public | BindingFlags.Instance, out PropertyInfo? displayProperty))
            {
                if (typeOfValue.TryGetField(displayMember, BindingFlags.Public | BindingFlags.Instance, out FieldInfo? displayField))
                {
                    DisplayMember = displayField;
                }
                else
                {
                    throw new ArgumentException($"The type '{typeOfValue.Name}' does not contain a property or field named '{displayMember}'.");
                }
            }
            else
            {
                DisplayMember = displayProperty;
            }
        }

        if (!string.IsNullOrWhiteSpace(valueMember))
        {
            if (!typeOfValue.TryGetProperty(valueMember, BindingFlags.Public | BindingFlags.Instance, out PropertyInfo? valueProperty))
            {
                if (typeOfValue.TryGetField(valueMember, BindingFlags.Public | BindingFlags.Instance, out FieldInfo? valueField))
                {
                    ValueMember = valueField;
                }
                else
                {
                    throw new ArgumentException($"The type '{typeOfValue.Name}' does not contain a property or field named '{valueMember}'.");
                }
            }
            else
            {
                ValueMember = valueProperty;
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

            string display = ((DisplayMember is not null)
                                ? item.GetValue(DisplayMember)?.ToString()
                                : item.ToString())
                              ?? string.Empty;

            object? value = (ValueMember is not null)
                                ? (item.GetValue(ValueMember) ?? item)
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

        _control = control;
    }

    /// <summary>
    /// Initialise the list of values based control binder.
    /// </summary>
    /// <param name="member">The member property or field participating in the binding.</param>
    /// <param name="control">The control of type <typeparamref name="TControl"/> participating in the binding.</param>
    /// <param name="dictionarySource">A dictionary of values (Key: of type <see cref="string"/> is the displayed text on the UI, 
    /// Value: of type <typeparamref name="TValue"/> is the internal value of the item) to be bound to the <paramref name="control"/> as choices.</param>
    /// <param name="bindingDirection">The direction of binding.</param>
    protected ListValueBinderBase(PersistenceContainerMemberInfo member, TControl control,
        Dictionary<string, TValue> dictionarySource, BindingDirection bindingDirection = BindingDirection.TwoWay)
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

        Type dt = dictionarySource.GetType();
        DisplayMember = dt.GetProperty("Key");
        ValueMember = dt.GetProperty("Value");

        _control = control;
    }

    /// <summary>
    /// Initialise the list of values based control binder.
    /// </summary>
    /// <param name="member">The member property or field participating in the binding.</param>
    /// <param name="control">The control of type <typeparamref name="TControl"/> participating in the binding.</param>
    /// <param name="bindOnlyWhenChanged">Indicates to the binder that <paramref name="enumerableValueSourceFunction"/> should be evaluated only when this (<paramref name="bindOnlyWhenChanged"/>) control changes its current value or selection.</param>
    /// <param name="enumerableValueSourceFunction">An enumeration of values of type <typeparamref name="TValue"/> that are to be bound to the <paramref name="control"/> as choices.</param>
    /// <param name="displayMember">The property of <typeparamref name="TValue"/> that is to be displayed on the UI.</param>
    /// <param name="valueMember">The property of <typeparamref name="TValue"/> that is to be used as the member item's internal value.</param>
    /// <param name="bindingDirection">The direction of binding.</param>
    protected ListValueBinderBase(PersistenceContainerMemberInfo member, TControl control, Control bindOnlyWhenChanged,
        Func<object, IEnumerable<TValue>> enumerableValueSourceFunction, string? displayMember = null, string? valueMember = null, BindingDirection bindingDirection = BindingDirection.TwoWay)
        : base(member, control, bindingDirection)
    {
        Type typeOfValue = typeof(TValue);
        if (!string.IsNullOrWhiteSpace(displayMember))
        {
            if (!typeOfValue.TryGetProperty(displayMember, BindingFlags.Public | BindingFlags.Instance, out PropertyInfo? displayProperty))
            {
                if (typeOfValue.TryGetField(displayMember, BindingFlags.Public | BindingFlags.Instance, out FieldInfo? displayField))
                {
                    DisplayMember = displayField;
                }
                else
                {
                    throw new ArgumentException($"The type '{typeOfValue.Name}' does not contain a property or field named '{displayMember}'.");
                }
            }
            else
            {
                DisplayMember = displayProperty;
            }
        }

        if (!string.IsNullOrWhiteSpace(valueMember))
        {
            if (!typeOfValue.TryGetProperty(valueMember, BindingFlags.Public | BindingFlags.Instance, out PropertyInfo? valueProperty))
            {
                if (typeOfValue.TryGetField(valueMember, BindingFlags.Public | BindingFlags.Instance, out FieldInfo? valueField))
                {
                    ValueMember = valueField;
                }
                else
                {
                    throw new ArgumentException($"The type '{typeOfValue.Name}' does not contain a property or field named '{valueMember}'.");
                }
            }
            else
            {
                ValueMember = valueProperty;
            }
        }

        _bindingSource = new List<BindingKeyValuePair>();
        _control = control;

        SetConditionalUpstreamControl(bindOnlyWhenChanged, enumerableValueSourceFunction);
    }


    /// <summary>
    /// Find the index of <paramref name="value"/> in the bound collection.
    /// </summary>
    /// <param name="value">The value to search for.</param>
    /// <returns>Index of the value, or -1 if not found.</returns>
    protected int IndexOf(TValue value)
    {
        for (int i = 0; i < _bindingSource.Count; i++)
        {
            BindingKeyValuePair pair = _bindingSource[i];
            if ((pair.Value is TValue val) && EqualityComparer<TValue>.Default.Equals(val, value))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Find the index of <paramref name="value"/> in the bound collection.
    /// </summary>
    /// <param name="value">The value to search for.</param>
    /// <returns>Index of the value, or -1 if not found.</returns>
    protected int IndexOf(object value)
    {
        for (int i = 0; i < _bindingSource.Count; i++)
        {
            BindingKeyValuePair pair = _bindingSource[i];

            //BUGFIX: Equality comparer will not work with untyped/typed value mix. 
            //        .Equals handles it neatly. (stop listening to AI!)
            if (pair.Value.Equals(value))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Find the index of <paramref name="value"/> in the bound collection.
    /// </summary>
    /// <param name="value">The value to search for.</param>
    /// <returns>Index of the value, or -1 if not found.</returns>
    protected int IndexOf(string value)
    {
        for (int i = 0; i < _bindingSource.Count; i++)
        {
            BindingKeyValuePair pair = _bindingSource[i];
            if ((pair.Key == value) || (pair.Value.ToString() == value))
            {
                return i;
            }
        }

        return -1;
    }

    // Added to support dependency binding.
    /// <summary>
    /// Evaluates the <paramref name="triggerValue"/> by calling the provided <paramref name="valueFunction"/>. Then, it uses the values 
    /// returned by the <paramref name="valueFunction"/> to populate the control. If the control is bound to the entity instance, the value 
    /// is found and set.
    /// </summary>
    /// <param name="triggerValue">Value from the upstream trigger control.</param>
    /// <param name="valueFunction">Function that fetches the data to populate.</param>
    private void EvaluateTriggerValueAndPopulateControl(object? triggerValue, Func<object, IEnumerable<TValue>> valueFunction)
    {
        if (triggerValue is BindingKeyValuePair bkvp)
        {
            triggerValue = bkvp.Value;
        }

        List<TValue> valueSource = valueFunction(triggerValue!).Materialise<TValue>(acceptNullElements: false, throwExceptionOnNull: true);

        _control.DataSource = null;
        _bindingSource.Clear();

        if (valueSource.Any())
        {
            foreach (TValue item in valueSource)
            {
                if (item is null)
                {
                    throw new ArgumentException($"'{base.ControlPartner.Name}': Enumerated source contains one or more NULL values.");
                }

                string display = ((DisplayMember is not null)
                                    ? item.GetValue(DisplayMember)?.ToString()
                                    : item.ToString())
                                  ?? string.Empty;

                object? value = (ValueMember is not null)
                                    ? (item.GetValue(ValueMember) ?? item)
                                    : display;

                _bindingSource.Add(new BindingKeyValuePair()
                {
                    Key = display,
                    Value = value
                });
            }
        }

        _control.DataSource = _bindingSource;
        _control.DisplayMember = nameof(BindingKeyValuePair.Key);
        _control.ValueMember = nameof(BindingKeyValuePair.Value);

        if (DataContext is not null)
        {
            // Control has been bound to the entity instance.
            BindControl(DataContext);
        }
    }

    // Added to support dependency binding.
    /// <summary>
    /// Set up the conditional binding.
    /// </summary>
    /// <param name="triggerControl">Control that triggers our data fetch.</param>
    /// <param name="valueFunction">Function that fetches the data to populate.</param>
    private void SetConditionalUpstreamControl(Control triggerControl, Func<object, IEnumerable<TValue>> valueFunction)
    {
        // Only controls whose values can actively change in a UI are allowed as 'control'.
        // Other 'static display' controls (such as labels) do not make sense in our scenario.

        if (triggerControl is TextBox tb)
        {
            tb.TextChanged += (o, e) => EvaluateTriggerValueAndPopulateControl(tb.Text, valueFunction);
        }
        else if (triggerControl is RichTextBox rtb)
        {
            rtb.TextChanged += (o, e) => EvaluateTriggerValueAndPopulateControl(rtb.Text, valueFunction);
        }
        else if (triggerControl is RadioButton rb)
        {
            rb.CheckedChanged += (o, e) => EvaluateTriggerValueAndPopulateControl(rb.Checked, valueFunction);
        }
        else if (triggerControl is CheckBox cb)
        {
            cb.CheckedChanged += (o, e) => EvaluateTriggerValueAndPopulateControl(cb.Checked, valueFunction);
        }
        else if (triggerControl is MaskedTextBox mtb)
        {
            mtb.TextChanged += (o, e) => EvaluateTriggerValueAndPopulateControl(mtb.Text, valueFunction);
        }
        else if (triggerControl is ListView lv)
        {
            // Note: We are passing in a collection of [ListViewItem]s here, because we have no idea what the caller will want to evaluate of it!
            lv.SelectedIndexChanged += (o, e) => EvaluateTriggerValueAndPopulateControl(lv.SelectedItems, valueFunction);
        }
        else if (triggerControl is ListBox lb)
        {
            lb.SelectedValueChanged += (o, e) => EvaluateTriggerValueAndPopulateControl(lb.SelectedItems, valueFunction);
        }
        else if (triggerControl is DateTimePicker dtp)
        {
            dtp.ValueChanged += (o, e) => EvaluateTriggerValueAndPopulateControl(dtp.Value, valueFunction);
        }
        else if (triggerControl is ComboBox cmb)
        {
            // For controls where the user may type in values etc, we need some additional plumbing 
            // to make it more user-intuitive.
            if (cmb.DropDownStyle != ComboBoxStyle.DropDownList)
            {
                cmb.TextChanged += (o, e) =>
                {
                    if (cmb.DataSource is not null)
                    {
                        string typed = cmb.Text;

                        List<BindingKeyValuePair> data = (List<BindingKeyValuePair>)cmb.DataSource!;
                        BindingKeyValuePair? match = data.FirstOrDefault(bkvp =>
                            string.Equals(bkvp.Key, typed, StringComparison.OrdinalIgnoreCase));

                        if (match is not null)
                        {
                            // This should cause the SelectedValueChanged event to fire.
                            cmb.SelectedItem = match;

                            if (cmb.Text.Length > 0)
                            {
                                cmb.SelectionStart = cmb.Text.Length;
                                cmb.SelectionLength = 0;
                            }
                        }
                    }
                };
            }

            cmb.SelectedValueChanged += (o, e) => EvaluateTriggerValueAndPopulateControl(cmb.SelectedItem, valueFunction);
        }
        else if (triggerControl is CheckedListBox clb)
        {
            clb.SelectedValueChanged += (o, e) => EvaluateTriggerValueAndPopulateControl(clb.SelectedItems, valueFunction);
        }
        else if (triggerControl is CheckBox chk)
        {
            chk.CheckedChanged += (o, e) => EvaluateTriggerValueAndPopulateControl(chk.Checked, valueFunction);
        }
    }


    /// <summary>
    /// Metadata about the display member.
    /// </summary>
    protected MemberInfo? DisplayMember = null;

    /// <summary>
    /// Metadata about the value member.
    /// </summary>
    protected MemberInfo? ValueMember = null;

    /// <summary>
    /// The internal binding source.
    /// </summary>
    private readonly List<BindingKeyValuePair> _bindingSource;

    /// <summary>
    /// Reference to the control we are binding. (used only in dependency-binding scenario).
    /// </summary>
    private TControl _control;
}
