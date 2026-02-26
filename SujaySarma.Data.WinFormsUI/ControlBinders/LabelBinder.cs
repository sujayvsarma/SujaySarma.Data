using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.Core.TypeDiscovery;

using System;
using System.Windows.Forms;

namespace SujaySarma.Data.WinFormsUI.ControlBinders;

/// <summary>
/// Performs one-way data binding between a Windows Forms <see cref="Label"/> control and 
/// the given data source.
/// </summary>
internal sealed class LabelBinder : ControlBinderBase<Label>
{

    /// <inheritdoc />
    public override void BindControl(object dataContext)
    {
        base.BindControl(dataContext);

        if (base.MemberDataType.IsEnum)
        {
            // For enums, we convert to string via ToString()
            base.ControlPartner.Text = dataContext.GetValue(base.EntityMemberPartner)?.ToString() ?? string.Empty;
            return;
        }

        Func<object, string> converter = TypesToStringConverterCache[base.MemberDataType];
        base.ControlPartner.Text = converter(dataContext.GetValue(base.EntityMemberPartner) ?? string.Empty);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LabelBinder"/> class.
    /// </summary>
    /// <param name="label">The <see cref="Label"/> control that is to be bound.</param>
    /// <param name="member">The metadata about the member property or field that is to be bound to the <paramref name="label"/> control.</param>
    public LabelBinder(Label label, PersistenceContainerMemberInfo member)
        : this(label, member, BindingDirection.OneWay)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LabelBinder"/> class.
    /// </summary>
    /// <param name="label">The <see cref="Label"/> control that is to be bound.</param>
    /// <param name="member">The metadata about the member property or field that is to be bound to the <paramref name="label"/> control.</param>
    /// <param name="bindingDirection">The direction of binding: One-way or two-way.</param>
    public LabelBinder(Label label, PersistenceContainerMemberInfo member, BindingDirection bindingDirection)
        : base(member, label, bindingDirection)
    {
        // A label can only display strings. While anything maybe serialised to a string,
        // the only things that make sense from this context are: strings, numbers, enums and perhaps DateTime.
        if ((!TypesToStringConverterCache.ContainsKey(MemberDataType)) && (!MemberDataType.IsEnum))
        {
            throw new ArgumentException($"'{base.ControlPartner.Name}': Invalid member type. Only 'string', primitive numeric ('int', 'long', 'decimal', etc), 'DateTime' and 'enum' types are allowed.", nameof(member));
        }
    }
}
