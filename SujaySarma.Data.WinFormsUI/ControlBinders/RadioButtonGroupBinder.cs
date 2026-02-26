using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.Core.TypeDiscovery;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace SujaySarma.Data.WinFormsUI.ControlBinders;

/// <summary>
/// Helps bind a group of <see cref="RadioButton"/> controls to a provided list of source, 
/// providing the ability to select from one of them.
/// </summary>
/// <typeparam name="TValue">The type of source that would be a part of the control's presentation.</typeparam>
internal sealed class RadioButtonGroupBinder<TValue> : IControlBinder
{
    /// <summary>
    /// The direction of binding.
    /// </summary>
    private BindingDirection Direction
    {
        get; init;

    }

    /// <summary>
    /// The member property or field participating in the binding.
    /// </summary>
    private PersistenceContainerMemberInfo EntityMemberPartner
    {
        get; init;
    }

    /// <summary>
    /// The data type of the member property or field being bound to this control.
    /// </summary>
    private Type MemberDataType
    {
        get; init;
    }

    /// <summary>
    /// The data object (class/struct/record) that was originally bound 
    /// to the control binder. This property is to be populated by the implementing binder.
    /// </summary>
    private object? DataContext
    {
        get; set;

    } = null;


    /// <summary>
    /// Sets the value of the control using the current value of the specified member (<see cref="EntityMemberPartner"/>) 
    /// of the entity instance (<paramref name="dataContext"/>).
    /// </summary>
    /// <param name="dataContext">Instance of the entity whose member is being bound 
    /// (this should be the class/struct/record and not its member!).</param>
    public void BindControl(object dataContext)
    {
        object value = dataContext.GetValue(EntityMemberPartner)
            ?? throw new InvalidOperationException($"'RadioButtonGroup': Value of property or field '{EntityMemberPartner.Member.Name}' is NULL.");

        foreach (RadioButton rb in _radioButtons)
        {
            if (rb.Tag == value)
            {
                rb.Checked = true;
                break;
            }
        }
    }

    /// <summary>
    /// Sets the value of the <see cref="EntityMemberPartner"/> member of the entity instance (<paramref name="dataContext"/>) 
    /// from the current value of the control.
    /// </summary>
    /// <param name="dataContext">Instance of the entity whose member is being bound 
    /// (this should be the class/struct/record and not its member!).</param>
    public void BindEntityMember(object dataContext)
    {
        if (Direction is BindingDirection.OneWay)
        {
            throw new InvalidOperationException($"'RadioButtonGroup': Control binding is one-way only.");
        }

        foreach (RadioButton rb in _radioButtons)
        {
            if (rb.Checked)
            {
                dataContext.SetValue(EntityMemberPartner, rb.Tag);
                break;
            }
        }
    }

    /// <summary>
    /// Tests if this binder has a binding for the provided <paramref name="propertyName" />.
    /// </summary>
    /// <param name="propertyName">Name of the property to check for.</param>
    /// <returns>True if the binding exists.</returns>
    public bool BindsProperty(string propertyName)
    {
        return (EntityMemberPartner.Member.Name == propertyName);
    }

    /// <summary>
    /// Refreshes the value displayed on the control from the instance of the entity/property.
    /// </summary>
    public void RefreshControl()
    {
        if (DataContext is not null)
        {
            BindControl(DataContext);
        }
    }

    /// <summary>
    /// Initialise the binder.
    /// </summary>
    /// <param name="parentControl">The control that contains all the related <see cref="RadioButton"/> controls.</param>
    /// <param name="member">The member property or field participating in the binding.</param>
    /// <param name="valueSourceEnumerable">An enumeration of source of type <typeparamref name="TValue"/> that are to be bound to the radio buttons in <paramref name="parentControl"/> as choices.</param>
    /// <param name="displayMember">The property of <typeparamref name="TValue"/> that is to be displayed on the UI.</param>
    /// <param name="valueMember">The property of <typeparamref name="TValue"/> that is to be used as the member item's internal value.</param>
    /// <param name="bindingDirection">The direction of binding.</param>
    public RadioButtonGroupBinder(Control parentControl, PersistenceContainerMemberInfo member,
        IEnumerable<TValue> valueSourceEnumerable, string? displayMember = null, string? valueMember = null, BindingDirection bindingDirection = BindingDirection.TwoWay)
    {
        if (!member.Member.TryGetPropertyOrFieldDataType(out Type? type))
        {
            throw new ArgumentException($"The member '{member.Member.Name}' is not a property or field.", nameof(member));
        }

        // materialise.
        List<TValue> source = valueSourceEnumerable.Materialise<TValue>(acceptNullElements: false, throwExceptionOnNull: true);
        if (source.Count == 0)
        {
            throw new ArgumentException("The provided value source does not contain any elements.", nameof(valueSourceEnumerable));
        }

        if (parentControl.Controls.Count is 0)
        {
            throw new ArgumentException("The provided parent control does not contain any controls.", nameof(parentControl));
        }

        _radioButtons = parentControl.Controls.OfType<RadioButton>().ToList();
        if (_radioButtons.Count is 0)
        {
            throw new ArgumentException("The provided parent control does not contain any RadioButton controls.", nameof(parentControl));
        }

        MemberDataType = type.IfNullableGetActualType();
        EntityMemberPartner = member;
        Direction = bindingDirection;

        if (string.IsNullOrWhiteSpace(displayMember) && string.IsNullOrWhiteSpace(valueMember))
        {
            // Populate values from a source collection of primitives (eg string, etc) with no Display/Value member.
            PopulatePrimitives();
            return;
        }

        Type typeOfValue = typeof(TValue);
        MemberInfo? displayMemberInfo = null, valueMemberInfo = null;
        if (!typeOfValue.TryGetProperty(displayMember!, BindingFlags.Public | BindingFlags.Instance, out PropertyInfo? dpi))
        {
            if (!typeOfValue.TryGetField(displayMember!, BindingFlags.Public | BindingFlags.Instance, out FieldInfo? dfi))
            {
                throw new ArgumentException($"The type '{typeof(TValue).GetUsableTypeName()}' does not contain a property or field named '{displayMember}'.", nameof(displayMember));
            }

            displayMemberInfo = dfi;
        }
        else
        {
            displayMemberInfo = dpi;
        }

        if (!typeOfValue.TryGetProperty(valueMember!, BindingFlags.Public | BindingFlags.Instance, out PropertyInfo? vpi))
        {
            if (!typeOfValue.TryGetField(valueMember!, BindingFlags.Public | BindingFlags.Instance, out FieldInfo? vfi))
            {
                throw new ArgumentException($"The type '{typeof(TValue).GetUsableTypeName()}' does not contain a property or field named '{valueMember}'.", nameof(valueMember));
            }

            valueMemberInfo = vfi;
        }
        else
        {
            valueMemberInfo = vpi;
        }


        for (int i = 0; i < source.Count; i++)
        {
            if (i >= _radioButtons.Count)
            {
                break;
            }

            RadioButton rb = _radioButtons[i];
            rb.Checked = false;

            TValue val = source[i];
            rb.Text = val.GetValue(displayMemberInfo)?.ToString() ?? string.Empty;
            rb.Tag = val.GetValue(valueMemberInfo);
        }


        // Populate values from a source collection of primitives (eg string, etc) with no Display/Value member.
        void PopulatePrimitives()
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (i >= _radioButtons.Count)
                {
                    break;
                }

                RadioButton rb = _radioButtons[i];
                rb.Text = source[i]?.ToString() ?? string.Empty;
                rb.Tag = source[i]!;
                rb.Checked = false;
            }
        }
    }

    /// <summary>
    /// Initialise the binder.
    /// </summary>
    /// <param name="parentControl">The control that contains all the related <see cref="RadioButton"/> controls.</param>
    /// <param name="member">The member property or field participating in the binding.</param>
    /// <param name="valueSourceDictionary">An enumeration of source of type <typeparamref name="TValue"/> that are to be bound to the radio buttons in <paramref name="parentControl"/> as choices.</param>
    /// <param name="bindingDirection">The direction of binding.</param>
    public RadioButtonGroupBinder(Control parentControl, PersistenceContainerMemberInfo member, Dictionary<string, TValue> valueSourceDictionary, BindingDirection bindingDirection)
    {
        if (!member.Member.TryGetPropertyOrFieldDataType(out Type? type))
        {
            throw new ArgumentException($"'RadioButtonGroup': The member '{member.Member.Name}' is not a property or field.", nameof(member));
        }

        if (parentControl.Controls.Count is 0)
        {
            throw new ArgumentException("The provided parent control does not contain any controls.", nameof(parentControl));
        }

        _radioButtons = parentControl.Controls.OfType<RadioButton>().ToList();
        if (_radioButtons.Count is 0)
        {
            throw new ArgumentException("'RadioButtonGroup': The provided parent control does not contain any RadioButton controls.", nameof(parentControl));
        }

        MemberDataType = type.IfNullableGetActualType();
        EntityMemberPartner = member;
        Direction = bindingDirection;

        int radioButtonIndex = 0;
        foreach (KeyValuePair<string, TValue> kvp in valueSourceDictionary)
        {
            RadioButton rb = _radioButtons[radioButtonIndex];
            rb.Text = kvp.Key;
            rb.Tag = kvp.Value;
            rb.Checked = false;

            if (++radioButtonIndex > _radioButtons.Count)
            {
                break;
            }
        }
    }

    /// <summary>
    /// The captured radio buttons.
    /// </summary>
    private readonly List<RadioButton> _radioButtons;
}
