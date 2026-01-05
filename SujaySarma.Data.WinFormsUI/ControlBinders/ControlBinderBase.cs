using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.Core.TypeDiscovery;

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SujaySarma.Data.WinFormsUI.ControlBinders;


/// <summary>
/// The base class for our control binders.
/// </summary>
/// <typeparam name="TControl">The type of control being bound.</typeparam>
internal abstract partial class ControlBinderBase<TControl> : IControlBinder
    where TControl : Control
{

    /// <summary>
    /// The direction of binding.
    /// </summary>
    protected BindingDirection Direction
    {
        get; init;

    }

    /// <summary>
    /// The member property or field participating in the binding.
    /// </summary>
    protected PersistenceContainerMemberInfo EntityMemberPartner
    {
        get; init;
    }

    /// <summary>
    /// The control participating in the binding.
    /// </summary>
    protected TControl ControlPartner
    {
        get; init;
    }

    /// <summary>
    /// The data object (class/struct/record) that was originally bound 
    /// to the control binder. This property is to be populated by the implementing binder.
    /// </summary>
    protected object? DataContext
    {
        get; set;

    } = null;

    /// <summary>
    /// The data type of the member property or field being bound to this control.
    /// </summary>
    protected Type MemberDataType
    {
        get; init;
    }

    /// <summary>
    /// Sets the value of the control using the current value of the specified member (<see cref="EntityMemberPartner"/>) 
    /// of the entity instance (<paramref name="dataContext"/>).
    /// </summary>
    /// <param name="dataContext">Instance of the entity whose member is being bound 
    /// (this should be the class/struct/record and not its member!). Implementers must always use this instance rather than 
    /// the <see cref="DataContext"/> property.</param>
    public abstract void BindControl(object dataContext);

    /// <summary>
    /// Sets the value of the <see cref="EntityMemberPartner"/> member of the entity instance (<paramref name="dataContext"/>) 
    /// from the current value of the control.
    /// </summary>
    /// <param name="dataContext">Instance of the entity whose member is being bound 
    /// (this should be the class/struct/record and not its member!). Implementers must always use this instance rather than 
    /// the <see cref="DataContext"/> property.</param>
    public virtual void BindEntityMember(object dataContext)
    {
        if (Direction is BindingDirection.OneWay)
        {
            throw new InvalidOperationException($"'{ControlPartner.GetType().Name}.{ControlPartner.Name}': Control binding is one-way only.");
        }

        dataContext.SetValue(EntityMemberPartner, ControlPartner.Text);
    }


    /// <summary>
    /// Initialise the control binder.
    /// </summary>
    /// <param name="member">The member property or field participating in the binding.</param>
    /// <param name="control">The control of type <typeparamref name="TControl"/> participating in the binding.</param>
    /// <param name="direction">The direction of binding.</param>
    protected ControlBinderBase(PersistenceContainerMemberInfo member, TControl control, BindingDirection direction = BindingDirection.TwoWay)
    {
        if (!member.Member.TryGetPropertyOrFieldDataType(out Type? type))
        {
            throw new ArgumentException($"The member '{member.Member.Name}' is not a property or field.", nameof(member));
        }

        MemberDataType = type.IfNullableGetActualType();
        EntityMemberPartner = member;
        ControlPartner = control;
        Direction = direction;
    }


    /// <summary>
    /// Type conversion cache used by implementing binders to convert types to strings.
    /// </summary>
    protected static readonly Dictionary<Type, Func<object, string>> TypesToStringConverterCache = new()
        {
            { typeof(bool), value => (bool)value ? "1" : "0" },
            { typeof(char), value => value!.ToString()! },
            { typeof(sbyte), value => value.ToString()! },
            { typeof(byte), value => value.ToString()! },
            { typeof(short), value => value.ToString()! },
            { typeof(ushort), value => value.ToString()! },
            { typeof(int), value => value.ToString()! },
            { typeof(uint), value => value.ToString()! },
            { typeof(long), value => value.ToString()! },
            { typeof(ulong), value => value.ToString()! },
            { typeof(float), value => ((float)value).ToString("R") },
            { typeof(double), value => ((double)value).ToString("R") },
            { typeof(decimal), value => ((decimal)value).ToString("G") },
            { typeof(string), value => value.ToString()! },
            { typeof(DateTime), value => $"{(DateTime)value:MMM dd, yyyy HH:mm:ss.fff}" },
            { typeof(DateOnly), value => $"{(DateOnly)value:MMM dd, yyyy}" },
            { typeof(TimeOnly), value => $"{(TimeOnly)value:HH:mm:ss}" },
            { typeof(DateTimeOffset), value => $"{(DateTimeOffset)value:MMM dd, yyyy HH:mm:ss.fff}" },
            { typeof(Guid), value => $"'{value}'" }
        };
}
