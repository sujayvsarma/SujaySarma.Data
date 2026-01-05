using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.Core.TypeDiscovery;

using System;
using System.Windows.Forms;

namespace SujaySarma.Data.WinFormsUI.ControlBinders;

/// <summary>
/// Performs data binding between a Windows Forms <see cref="TextBox"/> control and 
/// the given data source. If the <see cref="TextBox"/>  is read-only, 
/// the binding is one-way; otherwise, it is two-way.
/// </summary>
internal sealed class TextboxBinder : ControlBinderBase<TextBox>
{

    /// <inheritdoc />
    public override void BindControl(object dataContext)
    {
        if (base.MemberDataType.IsEnum)
        {
            // For enums, we convert to string via ToString()
            base.ControlPartner.Text = dataContext.GetValue(base.EntityMemberPartner)?.ToString() ?? string.Empty;
            return;
        }

        Func<object, string> converter = TypesToStringConverterCache[base.MemberDataType];
        base.ControlPartner.Text = converter(dataContext.GetValue(base.EntityMemberPartner) ?? string.Empty);

        if (_keepEntityInSync)
        {
            DataContext = dataContext;
            base.ControlPartner.TextChanged += OnTextboxChanged;
        }
    }

    /// <summary>
    /// When the textbox's value changes, update the entity.
    /// </summary>
    private void OnTextboxChanged(object? sender, EventArgs e)
    {
        if (_keepEntityInSync && (DataContext is not null))
        {
            BindEntityMember(DataContext);
        }
    }


    /// <summary>
    /// Initialises a new instance of the <see cref="TextboxBinder"/> class.
    /// </summary>
    /// <param name="textbox">The <see cref="TextBox"/> control that is to be bound.</param>
    /// <param name="member">The metadata about the member property or field that is to be bound to the <paramref name="textbox"/> control.</param>
    public TextboxBinder(TextBox textbox, PersistenceContainerMemberInfo member)
        : this(textbox, member, (textbox.ReadOnly ? BindingDirection.OneWay : BindingDirection.TwoWay), false)
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="TextboxBinder"/> class.
    /// </summary>
    /// <param name="textbox">The <see cref="TextBox"/> control that is to be bound.</param>
    /// <param name="member">The metadata about the member property or field that is to be bound to the <paramref name="textbox"/> control.</param>
    /// <param name="bindingDirection">The direction of binding: One-way or two-way.</param>
    /// <param name="keepMemberInSync">[optional] When TRUE (requires <paramref name="bindingDirection"/> to be <see cref="BindingDirection.TwoWay"/>), 
    /// the binder hooks the TextChanged event to keep <paramref name="member"/> in sync with the value of the <paramref name="textbox"/>.</param>
    public TextboxBinder(TextBox textbox, PersistenceContainerMemberInfo member, BindingDirection bindingDirection, bool keepMemberInSync = false)
        : base(member, textbox, bindingDirection)
    {
        // A textbox can only display strings. While anything maybe serialised to a string,
        // the only things that make sense from this context are: strings, numbers, enums and perhaps DateTime.
        if ((!TypesToStringConverterCache.ContainsKey(MemberDataType)) && (!MemberDataType.IsEnum))
        {
            throw new ArgumentException($"'{base.ControlPartner.Name}': Invalid member type. Only 'string', primitive numeric ('int', 'long', 'decimal', etc), 'DateTime' and 'enum' types are allowed.", nameof(member));
        }

        if ((bindingDirection is BindingDirection.OneWay) && keepMemberInSync)
        {
            throw new ArgumentException($"'{base.ControlPartner.Name}': Cannot keep member in sync when binding direction is OneWay.", nameof(keepMemberInSync));
        }

        _keepEntityInSync = keepMemberInSync;
    }


    private readonly bool _keepEntityInSync = false;
}
