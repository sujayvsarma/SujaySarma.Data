using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.Core.TypeDiscovery;

using System;
using System.Windows.Forms;

namespace SujaySarma.Data.WinFormsUI.ControlBinders;

/// <summary>
/// Performs data binding between a Windows Forms <see cref="CheckBox"/> control and 
/// the given data source. If the <see cref="CheckBox"/>  is read-only (disabled), 
/// the binding is one-way; otherwise, it is two-way.
/// </summary>
internal sealed class CheckboxBinder : ControlBinderBase<CheckBox>
{
    /// <inheritdoc />
    public override void BindControl(object dataContext)
    {
        base.ControlPartner.Checked = (bool)(dataContext.GetValue(base.EntityMemberPartner) ?? false);

        if (_keepEntityInSync)
        {
            DataContext = dataContext;
            base.ControlPartner.CheckedChanged += OnCheckboxChanged;
        }
    }

    /// <inheritdoc />
    public override void BindEntityMember(object dataContext)
    {
        if (Direction is BindingDirection.OneWay)
        {
            throw new InvalidOperationException($"'{base.ControlPartner.GetType().Name}.{base.ControlPartner.Name}': Control binding is one-way only.");
        }

        dataContext.SetValue(base.EntityMemberPartner, base.ControlPartner.Checked);
    }

    /// <summary>
    /// When the checkbox's 'Checked' value changes, update the entity.
    /// </summary>
    private void OnCheckboxChanged(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="CheckboxBinder"/> class.
    /// </summary>
    /// <param name="checkbox">The <see cref="CheckBox"/> control that is to be bound.</param>
    /// <param name="member">The metadata about the member property or field that is to be bound to the <paramref name="checkbox"/> control.</param>
    public CheckboxBinder(CheckBox checkbox, PersistenceContainerMemberInfo member)
        : this(checkbox, member, (checkbox.Enabled ? BindingDirection.TwoWay : BindingDirection.OneWay), false)
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="CheckboxBinder"/> class.
    /// </summary>
    /// <param name="checkbox">The <see cref="CheckBox"/> control that is to be bound.</param>
    /// <param name="member">The metadata about the member property or field that is to be bound to the <paramref name="checkbox"/> control.</param>
    /// <param name="bindingDirection">The direction of binding: One-way or two-way.</param>
    /// <param name="keepMemberInSync">[optional] When TRUE (requires <paramref name="bindingDirection"/> to be <see cref="BindingDirection.TwoWay"/>), 
    /// the binder hooks the TextChanged event to keep <paramref name="member"/> in sync with the value of the <paramref name="checkbox"/>.</param>
    public CheckboxBinder(CheckBox checkbox, PersistenceContainerMemberInfo member, BindingDirection bindingDirection, bool keepMemberInSync = false)
        : base(member, checkbox, bindingDirection)
    {
        if (base.MemberDataType != typeof(bool))
        {
            throw new ArgumentException($"Checkbox control '{base.ControlPartner.Name}' can only be bound to boolean member types.", nameof(member));
        }

        if ((bindingDirection is BindingDirection.OneWay) && keepMemberInSync)
        {
            throw new ArgumentException($"'{base.ControlPartner.Name}': Cannot keep member in sync when binding direction is OneWay.", nameof(keepMemberInSync));
        }

        _keepEntityInSync = keepMemberInSync;
    }

    private readonly bool _keepEntityInSync = false;
}
