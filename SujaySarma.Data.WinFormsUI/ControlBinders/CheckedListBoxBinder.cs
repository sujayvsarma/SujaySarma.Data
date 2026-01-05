using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.Core.TypeDiscovery;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SujaySarma.Data.WinFormsUI.ControlBinders;

/// <summary>
/// Extends the <see cref="ListValueBinderBase{TControl, TValue}"/>, for the <see cref="CheckedListBox"/> control.
/// </summary>
/// <typeparam name="TValue">The type of values that would be a part of the control's presentation.</typeparam>

internal sealed class CheckedListBoxBinder<TValue> : ListValueBinderBase<CheckedListBox, TValue>
{
    /// <inheritdoc />
    public override void BindControl(object dataContext)
    {
        IEnumerable<TValue>? values = (IEnumerable<TValue>?)dataContext.GetValue(EntityMemberPartner);
        if (values is null)
        {
            return;
        }

        if (values.Any(v => (v is null)))
        {
            throw new InvalidOperationException($"'{base.ControlPartner.Name}': Null value found in entity member value.");
        }

        foreach (TValue val in values)
        {
            // val is already validated as non-NULL in the IF above.
            int index = ControlPartner.Items.IndexOf(val!);
            if (index >= 0)
            {
                ControlPartner.SetItemChecked(index, true);
            }
        }
    }

    /// <inheritdoc />
    public override void BindEntityMember(object dataContext)
    {
        if (Direction is BindingDirection.OneWay)
        {
            throw new InvalidOperationException($"'{base.ControlPartner.GetType().Name}.{base.ControlPartner.Name}': Control binding is one-way only.");
        }

        List<TValue> selectedValues = new List<TValue>();
        foreach (object? item in ControlPartner.CheckedItems)
        {
            if (item is null)
            {
                throw new InvalidOperationException($"'{base.ControlPartner.Name}': Null value found in control selected items.");
            }

            selectedValues.Add((TValue)item);
        }

        dataContext.SetValue(EntityMemberPartner, selectedValues);
    }

    /// <summary>
    /// Initialise the list of values based control binder.
    /// </summary>
    /// <param name="control">The control of type <see cref="CheckedListBox"/> participating in the binding.</param>
    /// <param name="member">The member property or field participating in the binding.</param>
    /// <param name="valueSource">An enumeration of values of type <typeparamref name="TValue"/> that are to be bound to the <paramref name="control"/> as choices.</param>
    /// <param name="displayMember">The property of <typeparamref name="TValue"/> that is to be displayed on the UI.</param>
    /// <param name="valueMember">The property of <typeparamref name="TValue"/> that is to be used as the member item's internal value.</param>
    /// <param name="bindingDirection">The direction of binding.</param>
    internal CheckedListBoxBinder(CheckedListBox control, PersistenceContainerMemberInfo member,
        IEnumerable<TValue> valueSource, string? displayMember = null, string? valueMember = null, BindingDirection bindingDirection = BindingDirection.TwoWay)
        : base(member, control, valueSource, displayMember, valueMember, bindingDirection)
    {
    }

    /// <summary>
    /// Initialise the list of values based control binder.
    /// </summary>
    /// <param name="control">The control of type <see cref="CheckedListBox"/> participating in the binding.</param>
    /// <param name="member">The member property or field participating in the binding.</param>
    /// <param name="valueSource">A dictionary of values (Key: of type <see cref="string"/> is the displayed text on the UI, 
    /// Value: of type <typeparamref name="TValue"/> is the internal value of the item) to be bound to the <paramref name="control"/> as choices.</param>
    /// <param name="bindingDirection">The direction of binding.</param>
    internal CheckedListBoxBinder(CheckedListBox control, PersistenceContainerMemberInfo member, Dictionary<string, TValue> valueSource, BindingDirection bindingDirection = BindingDirection.TwoWay)
        : base(member, control, dictionarySource: valueSource, bindingDirection)
    {
    }
}
