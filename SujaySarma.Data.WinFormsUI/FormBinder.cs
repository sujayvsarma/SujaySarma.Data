using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.Core.TypeDiscovery;
using SujaySarma.Data.WinFormsUI.ControlBinders;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms;

namespace SujaySarma.Data.WinFormsUI;

/// <summary>
/// Performs data binding for all controls on a Windows Form.
/// </summary>
public class FormBinder<TEntity>
{

    /// <summary>
    /// Retrieve the values of bound controls, setting them to the previously registered members of the entity instance.
    /// </summary>
    /// <returns>Instance of the entity populated updated from the bound controls.</returns>
    public TEntity Retrieve()
    {
        foreach (IControlBinder controlBinder in _boundControls)
        {
            controlBinder.BindEntityMember(_entity!);
        }

        return _entity;
    }


    /// <summary>
    /// Bind the controls on the form to the previously registered members of the entity instance.
    /// </summary>
    public void Bind()
    {
        foreach (IControlBinder controlBinder in _boundControls)
        {
            controlBinder.BindControl(_entity!);
        }
    }

    /// <summary>
    /// Handles INotifyPropertyChanged if TEntity implements it.
    /// </summary>
    /// <param name="sender">(Unused)</param>
    /// <param name="e">PropertyName contains the name of the property that was updated.</param>
    private void Entity_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        foreach(IControlBinder controlBinder in _boundControls)
        {
            if ((e.PropertyName is null) || ((e.PropertyName is not null) && controlBinder.BindsProperty(e.PropertyName)))
            {
                controlBinder.RefreshControl();
            }
        }
    }

    #region Add controls

    #region List Box

    /// <summary>
    /// Bind a <see cref="ListBox"/> control to the provided enumeration of <paramref name="valueSource"/>.
    /// </summary>
    /// <typeparam name="TValue">Type of values that are to be bound to the <see cref="ListBox"/>.</typeparam>
    /// <param name="listBox">The <see cref="ListBox"/> that is to be bound.</param>
    /// <param name="memberSelector">A lambda expression selector that selects a member property or field from the <typeparamref name="TEntity"/> to be bound with 
    /// the <paramref name="listBox"/>. The use of functions are not allowed.</param>
    /// <param name="valueSource">A <see cref="Dictionary{TKey, TValue}"/> source of values that would be populated on the <see cref="ListBox"/>s.</param>
    /// <returns>Instance of self.</returns>
    public FormBinder<TEntity> AddListBox<TValue>(ListBox listBox, Expression<Func<TEntity, object>> memberSelector, Dictionary<string, TValue> valueSource)
    {
        MemberInfo member = MemberSelectorParser.ExtractMember(memberSelector);
        if (!_entityInfo.TryGetMember(member.Name, out PersistenceContainerMemberInfo? memberInfo))
        {
            throw new ArgumentException(
                $"Member '{member.Name}' is not a valid property or field of type '{typeof(TEntity).GetUsableTypeName()}'.",
                nameof(memberSelector));
        }

        _boundControls.Add(new ListBoxBinder<TValue>(listBox, memberInfo, valueSource));
        return this;
    }

    /// <summary>
    /// Bind a <see cref="ListBox"/> control to the provided enumeration of <paramref name="valueSource"/>.
    /// </summary>
    /// <typeparam name="TValue">Type of values that are to be bound to the <see cref="ListBox"/>.</typeparam>
    /// <param name="listBox">The <see cref="ListBox"/> that is to be bound.</param>
    /// <param name="memberSelector">A lambda expression selector that selects a member property or field from the <typeparamref name="TEntity"/> to be bound with 
    /// the <paramref name="listBox"/>. The use of functions are not allowed.</param>
    /// <param name="valueSource">An <see cref="IEnumerable{TValue}"/> source of values that would be populated on the <see cref="RadioButton"/>s.</param>
    /// <param name="displayMember">[optional] The property or field of <typeparamref name="TValue"/> that is to be displayed on the UI. Set to NULL if <typeparamref name="TValue"/> is a primitive (eg: string, int, etc).</param>
    /// <param name="valueMember">[optional] The property or field of <typeparamref name="TValue"/> that is to be used as the member item's internal value. Set to NULL if <typeparamref name="TValue"/> is a primitive (eg: string, int, etc).</param>
    /// <returns>Instance of self.</returns>
    public FormBinder<TEntity> AddListBox<TValue>(ListBox listBox, Expression<Func<TEntity, object>> memberSelector,
        IEnumerable<TValue> valueSource, string? displayMember = null, string? valueMember = null)
    {
        MemberInfo member = MemberSelectorParser.ExtractMember(memberSelector);
        if (!_entityInfo.TryGetMember(member.Name, out PersistenceContainerMemberInfo? memberInfo))
        {
            throw new ArgumentException(
                $"Member '{member.Name}' is not a valid property or field of type '{typeof(TEntity).GetUsableTypeName()}'.",
                nameof(memberSelector));
        }

        _boundControls.Add(new ListBoxBinder<TValue>(listBox, memberInfo, valueSource, displayMember, valueMember));
        return this;
    }

    //NEW FEATURE: Add dependency-binding.
    /// <summary>
    /// Bind a <see cref="ListBox"/> control to the provided enumeration of <paramref name="enumerableValueSourceFunction"/>.
    /// </summary>
    /// <typeparam name="TValue">Type of values that are to be bound to the <see cref="ListBox"/>.</typeparam>
    /// <param name="listBox">The <see cref="ListBox"/> that is to be bound.</param>
    /// <param name="memberSelector">A lambda expression selector that selects a member property or field from the <typeparamref name="TEntity"/> to be bound with 
    /// the <paramref name="listBox"/>. The use of functions are not allowed.</param>
    /// <param name="bindOnlyWhenChanged">Indicates to the binder that <paramref name="enumerableValueSourceFunction"/> should be evaluated only when this (<paramref name="bindOnlyWhenChanged"/>) control changes its current value or selection.</param>
    /// <param name="enumerableValueSourceFunction">An enumeration of values of type <typeparamref name="TValue"/> that are to be bound to the <paramref name="listBox"/> as choices.</param>
    /// <param name="displayMember">[optional] The property or field of <typeparamref name="TValue"/> that is to be displayed on the UI. Set to NULL if <typeparamref name="TValue"/> is a primitive (eg: string, int, etc).</param>
    /// <param name="valueMember">[optional] The property or field of <typeparamref name="TValue"/> that is to be used as the member item's internal value. Set to NULL if <typeparamref name="TValue"/> is a primitive (eg: string, int, etc).</param>
    /// <returns>Instance of self.</returns>
    public FormBinder<TEntity> AddDependentListBox<TValue>(ListBox listBox, Expression<Func<TEntity, object>> memberSelector, 
        Control bindOnlyWhenChanged, Func<object, IEnumerable<TValue>> enumerableValueSourceFunction, 
            string? displayMember = null, string? valueMember = null)
    {
        MemberInfo member = MemberSelectorParser.ExtractMember(memberSelector);
        if (!_entityInfo.TryGetMember(member.Name, out PersistenceContainerMemberInfo? memberInfo))
        {
            throw new ArgumentException(
                $"Member '{member.Name}' is not a valid property or field of type '{typeof(TEntity).GetUsableTypeName()}'.",
                nameof(memberSelector));
        }

        _boundControls.Add(new ListBoxBinder<TValue>(
                listBox, memberInfo, bindOnlyWhenChanged, enumerableValueSourceFunction, 
                    displayMember, valueMember, 
                        ControlBinders.BindingDirection.TwoWay
            ));

        return this;
    }

    #endregion

    #region Combo Box (Dropdown List)

    /// <summary>
    /// Bind a <see cref="ComboBox"/> control to the provided enumeration of <paramref name="valueSource"/>.
    /// </summary>
    /// <typeparam name="TValue">Type of values that are to be bound to the <see cref="ComboBox"/>.</typeparam>
    /// <param name="comboBox">The <see cref="ComboBox"/> that is to be bound.</param>
    /// <param name="memberSelector">A lambda expression selector that selects a member property or field from the <typeparamref name="TEntity"/> to be bound with 
    /// the <paramref name="comboBox"/>. The use of functions are not allowed.</param>
    /// <param name="valueSource">A <see cref="Dictionary{TKey, TValue}"/> source of values that would be populated on the <see cref="ComboBox"/>s.</param>
    /// <returns>Instance of self.</returns>
    public FormBinder<TEntity> AddComboBox<TValue>(ComboBox comboBox, Expression<Func<TEntity, object>> memberSelector, Dictionary<string, TValue> valueSource)
    {
        MemberInfo member = MemberSelectorParser.ExtractMember(memberSelector);
        if (!_entityInfo.TryGetMember(member.Name, out PersistenceContainerMemberInfo? memberInfo))
        {
            throw new ArgumentException(
                $"Member '{member.Name}' is not a valid property or field of type '{typeof(TEntity).GetUsableTypeName()}'.",
                nameof(memberSelector));
        }

        _boundControls.Add(new ComboBoxBinder<TValue>(comboBox, memberInfo, valueSource));
        return this;
    }

    /// <summary>
    /// Bind a <see cref="ComboBox"/> control to the provided enumeration of <paramref name="valueSource"/>.
    /// </summary>
    /// <typeparam name="TValue">Type of values that are to be bound to the <see cref="ComboBox"/>.</typeparam>
    /// <param name="comboBox">The <see cref="ComboBox"/> that is to be bound.</param>
    /// <param name="memberSelector">A lambda expression selector that selects a member property or field from the <typeparamref name="TEntity"/> to be bound with 
    /// the <paramref name="comboBox"/>. The use of functions are not allowed.</param>
    /// <param name="valueSource">An <see cref="IEnumerable{TValue}"/> source of values that would be populated on the <see cref="RadioButton"/>s.</param>
    /// <param name="displayMember">[optional] The property or field of <typeparamref name="TValue"/> that is to be displayed on the UI. Set to NULL if <typeparamref name="TValue"/> is a primitive (eg: string, int, etc).</param>
    /// <param name="valueMember">[optional] The property or field of <typeparamref name="TValue"/> that is to be used as the member item's internal value. Set to NULL if <typeparamref name="TValue"/> is a primitive (eg: string, int, etc).</param>
    /// <returns>Instance of self.</returns>
    public FormBinder<TEntity> AddComboBox<TValue>(ComboBox comboBox, Expression<Func<TEntity, object>> memberSelector,
        IEnumerable<TValue> valueSource, string? displayMember = null, string? valueMember = null)
    {
        MemberInfo member = MemberSelectorParser.ExtractMember(memberSelector);
        if (!_entityInfo.TryGetMember(member.Name, out PersistenceContainerMemberInfo? memberInfo))
        {
            throw new ArgumentException(
                $"Member '{member.Name}' is not a valid property or field of type '{typeof(TEntity).GetUsableTypeName()}'.",
                nameof(memberSelector));
        }
        _boundControls.Add(new ComboBoxBinder<TValue>(comboBox, memberInfo, valueSource, displayMember, valueMember));
        return this;
    }

    //NEW FEATURE: Add dependency-binding.
    /// <summary>
    /// Bind a <see cref="ComboBox"/> control to the provided enumeration of <paramref name="enumerableValueSourceFunction"/>.
    /// </summary>
    /// <typeparam name="TValue">Type of values that are to be bound to the <see cref="ComboBox"/>.</typeparam>
    /// <param name="comboBox">The <see cref="ComboBox"/> that is to be bound.</param>
    /// <param name="memberSelector">A lambda expression selector that selects a member property or field from the <typeparamref name="TEntity"/> to be bound with 
    /// the <paramref name="comboBox"/>. The use of functions are not allowed.</param>
    /// <param name="bindOnlyWhenChanged">Indicates to the binder that <paramref name="enumerableValueSourceFunction"/> should be evaluated only when this (<paramref name="bindOnlyWhenChanged"/>) control changes its current value or selection.</param>
    /// <param name="enumerableValueSourceFunction">An enumeration of values of type <typeparamref name="TValue"/> that are to be bound to the <paramref name="comboBox"/> as choices.</param>
    /// <param name="displayMember">[optional] The property or field of <typeparamref name="TValue"/> that is to be displayed on the UI. Set to NULL if <typeparamref name="TValue"/> is a primitive (eg: string, int, etc).</param>
    /// <param name="valueMember">[optional] The property or field of <typeparamref name="TValue"/> that is to be used as the member item's internal value. Set to NULL if <typeparamref name="TValue"/> is a primitive (eg: string, int, etc).</param>
    /// <returns>Instance of self.</returns>
    public FormBinder<TEntity> AddDependentComboBox<TValue>(ComboBox comboBox, Expression<Func<TEntity, object>> memberSelector,
        Control bindOnlyWhenChanged, Func<object, IEnumerable<TValue>> enumerableValueSourceFunction,
            string? displayMember = null, string? valueMember = null)
    {
        MemberInfo member = MemberSelectorParser.ExtractMember(memberSelector);
        if (!_entityInfo.TryGetMember(member.Name, out PersistenceContainerMemberInfo? memberInfo))
        {
            throw new ArgumentException(
                $"Member '{member.Name}' is not a valid property or field of type '{typeof(TEntity).GetUsableTypeName()}'.",
                nameof(memberSelector));
        }

        _boundControls.Add(new ComboBoxBinder<TValue>(
                comboBox, memberInfo, bindOnlyWhenChanged, enumerableValueSourceFunction,
                    displayMember, valueMember,
                        ControlBinders.BindingDirection.TwoWay
            ));

        return this;
    }

    #endregion

    #region CheckedListBox

    /// <summary>
    /// Bind a <see cref="CheckedListBox"/> control to the provided enumeration of <paramref name="valueSource"/>.
    /// </summary>
    /// <typeparam name="TValue">Type of values that are to be bound to the <see cref="CheckedListBox"/>.</typeparam>
    /// <param name="checkedListBox">The <see cref="CheckedListBox"/> that is to be bound.</param>
    /// <param name="memberSelector">A lambda expression selector that selects a member property or field from the <typeparamref name="TEntity"/> to be bound with 
    /// the <paramref name="checkedListBox"/>. The use of functions are not allowed.</param>
    /// <param name="valueSource">A <see cref="Dictionary{TKey, TValue}"/> source of values that would be populated on the <see cref="CheckedListBox"/>s.</param>
    /// <returns>Instance of self.</returns>
    public FormBinder<TEntity> AddCheckedListBox<TValue>(CheckedListBox checkedListBox, Expression<Func<TEntity, object>> memberSelector, Dictionary<string, TValue> valueSource)
    {
        MemberInfo member = MemberSelectorParser.ExtractMember(memberSelector);
        if (!_entityInfo.TryGetMember(member.Name, out PersistenceContainerMemberInfo? memberInfo))
        {
            throw new ArgumentException(
                $"Member '{member.Name}' is not a valid property or field of type '{typeof(TEntity).GetUsableTypeName()}'.",
                nameof(memberSelector));
        }

        _boundControls.Add(new CheckedListBoxBinder<TValue>(checkedListBox, memberInfo, valueSource));
        return this;
    }

    /// <summary>
    /// Bind a <see cref="CheckedListBox"/> control to the provided enumeration of <paramref name="valueSource"/>.
    /// </summary>
    /// <typeparam name="TValue">Type of values that are to be bound to the <see cref="CheckedListBox"/>.</typeparam>
    /// <param name="checkedListBox">The <see cref="CheckedListBox"/> that is to be bound.</param>
    /// <param name="memberSelector">A lambda expression selector that selects a member property or field from the <typeparamref name="TEntity"/> to be bound with 
    /// the <paramref name="checkedListBox"/>. The use of functions are not allowed.</param>
    /// <param name="valueSource">An <see cref="IEnumerable{TValue}"/> source of values that would be populated on the <see cref="RadioButton"/>s.</param>
    /// <param name="displayMember">[optional] The property or field of <typeparamref name="TValue"/> that is to be displayed on the UI. Set to NULL if <typeparamref name="TValue"/> is a primitive (eg: string, int, etc).</param>
    /// <param name="valueMember">[optional] The property or field of <typeparamref name="TValue"/> that is to be used as the member item's internal value. Set to NULL if <typeparamref name="TValue"/> is a primitive (eg: string, int, etc).</param>
    /// <returns>Instance of self.</returns>
    public FormBinder<TEntity> AddCheckedListBox<TValue>(CheckedListBox checkedListBox, Expression<Func<TEntity, object>> memberSelector,
        IEnumerable<TValue> valueSource, string? displayMember = null, string? valueMember = null)
    {
        MemberInfo member = MemberSelectorParser.ExtractMember(memberSelector);
        if (! _entityInfo.TryGetMember(member.Name, out PersistenceContainerMemberInfo? memberInfo))
        {
            throw new ArgumentException(
                $"Member '{member.Name}' is not a valid property or field of type '{typeof(TEntity).GetUsableTypeName()}'.",
                nameof(memberSelector));
        }
        _boundControls.Add(new CheckedListBoxBinder<TValue>(checkedListBox, memberInfo, valueSource, displayMember, valueMember));
        return this;
    }


    //NEW FEATURE: Add dependency-binding.
    /// <summary>
    /// Bind a <see cref="CheckedListBox"/> control to the provided enumeration of <paramref name="enumerableValueSourceFunction"/>.
    /// </summary>
    /// <typeparam name="TValue">Type of values that are to be bound to the <see cref="CheckedListBox"/>.</typeparam>
    /// <param name="checkedListBox">The <see cref="CheckedListBox"/> that is to be bound.</param>
    /// <param name="memberSelector">A lambda expression selector that selects a member property or field from the <typeparamref name="TEntity"/> to be bound with 
    /// the <paramref name="checkedListBox"/>. The use of functions are not allowed.</param>
    /// <param name="bindOnlyWhenChanged">Indicates to the binder that <paramref name="enumerableValueSourceFunction"/> should be evaluated only when this (<paramref name="bindOnlyWhenChanged"/>) control changes its current value or selection.</param>
    /// <param name="enumerableValueSourceFunction">An enumeration of values of type <typeparamref name="TValue"/> that are to be bound to the <paramref name="checkedListBox"/> as choices.</param>
    /// <param name="displayMember">[optional] The property or field of <typeparamref name="TValue"/> that is to be displayed on the UI. Set to NULL if <typeparamref name="TValue"/> is a primitive (eg: string, int, etc).</param>
    /// <param name="valueMember">[optional] The property or field of <typeparamref name="TValue"/> that is to be used as the member item's internal value. Set to NULL if <typeparamref name="TValue"/> is a primitive (eg: string, int, etc).</param>
    /// <returns>Instance of self.</returns>
    public FormBinder<TEntity> AddDependentCheckedListBox<TValue>(CheckedListBox checkedListBox, Expression<Func<TEntity, object>> memberSelector,
        Control bindOnlyWhenChanged, Func<object, IEnumerable<TValue>> enumerableValueSourceFunction,
            string? displayMember = null, string? valueMember = null)
    {
        MemberInfo member = MemberSelectorParser.ExtractMember(memberSelector);
        if (!_entityInfo.TryGetMember(member.Name, out PersistenceContainerMemberInfo? memberInfo))
        {
            throw new ArgumentException(
                $"Member '{member.Name}' is not a valid property or field of type '{typeof(TEntity).GetUsableTypeName()}'.",
                nameof(memberSelector));
        }

        _boundControls.Add(new CheckedListBoxBinder<TValue>(
                checkedListBox, memberInfo, bindOnlyWhenChanged, enumerableValueSourceFunction,
                    displayMember, valueMember,
                        ControlBinders.BindingDirection.TwoWay
            ));

        return this;
    }

    #endregion

    #region Radio Buttons Group

    /// <summary>
    /// Bind the child <see cref="RadioButton"/> controls of a parent <see cref="Control"/> to a member of the entity type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TValue">Type of values that are to be bound to the <see cref="RadioButton"/>s.</typeparam>
    /// <param name="groupParentControl">The container control (eg: Panel, Groupbox, Form, etc) that holds the collection of <see cref="RadioButton"/>s that are to be bound.</param>
    /// <param name="memberSelector">A lambda expression selector that selects a member property or field from the <typeparamref name="TEntity"/> to be bound with 
    /// the <see cref="RadioButton"/>s in <paramref name="groupParentControl"/>. The use of functions are not allowed.</param>
    /// <param name="values">A <see cref="Dictionary{TKey, TValue}"/> source of values that would be populated on the <see cref="RadioButton"/>s.</param>
    /// <returns>Instance of self.</returns>
    public FormBinder<TEntity> AddRadioButtonGroup<TValue>(Control groupParentControl, Expression<Func<TEntity, object>> memberSelector, Dictionary<string, TValue> values)
    {
        MemberInfo member = MemberSelectorParser.ExtractMember(memberSelector);
        if (!_entityInfo.TryGetMember(member.Name, out PersistenceContainerMemberInfo? memberInfo))
        {
            throw new ArgumentException(
                $"Member '{member.Name}' is not a valid property or field of type '{typeof(TEntity).GetUsableTypeName()}'.",
                nameof(memberSelector));
        }

        _boundControls.Add(new RadioButtonGroupBinder<TValue>(groupParentControl, memberInfo, valueSourceDictionary: values, bindingDirection: ControlBinders.BindingDirection.TwoWay));
        return this;
    }

    /// <summary>
    /// Bind the child <see cref="RadioButton"/> controls of a parent <see cref="Control"/> to a member of the entity type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TValue">Type of values that are to be bound to the <see cref="RadioButton"/>s.</typeparam>
    /// <param name="groupParentControl">The container control (eg: Panel, Groupbox, Form, etc) that holds the collection of <see cref="RadioButton"/>s that are to be bound.</param>
    /// <param name="memberSelector">A lambda expression selector that selects a member property or field from the <typeparamref name="TEntity"/> to be bound with 
    /// the <see cref="RadioButton"/>s in <paramref name="groupParentControl"/>. The use of functions are not allowed.</param>
    /// <param name="valueSource">An <see cref="IEnumerable{TValue}"/> source of values that would be populated on the <see cref="RadioButton"/>s.</param>
    /// <param name="displayMember">[optional] The property or field of <typeparamref name="TValue"/> that is to be displayed on the UI. Set to NULL if <typeparamref name="TValue"/> is a primitive (eg: string, int, etc).</param>
    /// <param name="valueMember">[optional] The property or field of <typeparamref name="TValue"/> that is to be used as the member item's internal value. Set to NULL if <typeparamref name="TValue"/> is a primitive (eg: string, int, etc).</param>
    /// <returns>Instance of self.</returns>
    public FormBinder<TEntity> AddRadioButtonGroup<TValue>(Control groupParentControl, Expression<Func<TEntity, object>> memberSelector,
        IEnumerable<TValue> valueSource, string? displayMember = null, string? valueMember = null)
    {
        MemberInfo member = MemberSelectorParser.ExtractMember(memberSelector);
        if (!_entityInfo.TryGetMember(member.Name, out PersistenceContainerMemberInfo? memberInfo))
        {
            throw new ArgumentException(
                $"Member '{member.Name}' is not a valid property or field of type '{typeof(TEntity).GetUsableTypeName()}'.",
                nameof(memberSelector));
        }

        _boundControls.Add(new RadioButtonGroupBinder<TValue>(groupParentControl, memberInfo, valueSource, displayMember, valueMember, ControlBinders.BindingDirection.TwoWay));
        return this;
    }

    #endregion

    /// <summary>
    /// Bind a <see cref="CheckBox"/> control to a member of the entity type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="checkBox">The <see cref="CheckBox"/> control to bind.</param>
    /// <param name="memberSelector">A lambda expression selector that selects a member property or field 
    /// from the <typeparamref name="TEntity"/> to be bound with <paramref name="checkBox"/>. The use of functions 
    /// are not allowed.</param>
    /// <returns>An instance of self.</returns>
    public FormBinder<TEntity> AddCheckBox(CheckBox checkBox, Expression<Func<TEntity, object>> memberSelector)
    {
        MemberInfo member = MemberSelectorParser.ExtractMember(memberSelector);
        if (!_entityInfo.TryGetMember(member.Name, out PersistenceContainerMemberInfo? memberInfo))
        {
            throw new ArgumentException(
                $"Member '{member.Name}' is not a valid property or field of type '{typeof(TEntity).GetUsableTypeName()}'.",
                nameof(memberSelector));
        }
        _boundControls.Add(new CheckboxBinder(checkBox, memberInfo));
        return this;
    }

    /// <summary>
    /// Bind a <see cref="TextBox"/> control to a member of the entity type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="textBox">The <see cref="TextBox"/> control to bind.</param>
    /// <param name="memberSelector">A lambda expression selector that selects a member property or field 
    /// from the <typeparamref name="TEntity"/> to be bound with <paramref name="textBox"/>. The use of functions 
    /// are not allowed.</param>
    /// <returns>An instance of self.</returns>
    public FormBinder<TEntity> AddTextBox(TextBox textBox, Expression<Func<TEntity, object>> memberSelector)
    {
        MemberInfo member = MemberSelectorParser.ExtractMember(memberSelector);
        if (! _entityInfo.TryGetMember(member.Name, out PersistenceContainerMemberInfo? memberInfo))
        {
            throw new ArgumentException(
                $"Member '{member.Name}' is not a valid property or field of type '{typeof(TEntity).GetUsableTypeName()}'.",
                nameof(memberSelector));
        }
        _boundControls.Add(new TextboxBinder(textBox, memberInfo));
        return this;
    }

    /// <summary>
    /// Bind a <see cref="Label"/> control to a member of the entity type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="label">The <see cref="Label"/> control to bind.</param>
    /// <param name="memberSelector">A lambda expression selector that selects a member property or field 
    /// from the <typeparamref name="TEntity"/> to be bound with <paramref name="label"/>. The use of functions 
    /// are not allowed.</param>
    /// <returns>An instance of self.</returns>
    public FormBinder<TEntity> AddLabel(Label label, Expression<Func<TEntity, object>> memberSelector)
    {
        MemberInfo member = MemberSelectorParser.ExtractMember(memberSelector);
        if (! _entityInfo.TryGetMember(member.Name, out PersistenceContainerMemberInfo? memberInfo))
        {
            throw new ArgumentException(
                $"Member '{member.Name}' is not a valid property or field of type '{typeof(TEntity).GetUsableTypeName()}'.",
                nameof(memberSelector));
        }

        _boundControls.Add(new LabelBinder(label, memberInfo));
        return this;
    }

    #endregion

    #region Initialisers

    /// <summary>
    /// Initialise an instance of FormBinder for the specified form <paramref name="form"/> bound to a new 
    /// instance of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="form">Instance of the form holding the controls to bind to.</param>
    /// <returns>Instance of FormBinder.</returns>
    public static FormBinder<TEntity> For(Form form)
    {
        TEntity entity = (TEntity?)Activator.CreateInstance(typeof(TEntity), nonPublic: true)
            ?? throw new TypeLoadException($"Could not create instance of type '{typeof(TEntity).GetUsableTypeName()}'.");

        return new FormBinder<TEntity>(form, entity);
    }

    /// <summary>
    /// Initialise an instance of FormBinder for the specified <paramref name="form"/> bound to the 
    /// instance <paramref name="entity"/> of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="form">Instance of the form holding the controls to bind to.</param>
    /// <param name="entity">Instance of the entity of type <typeparamref name="TEntity"/> this form is bound to.</param>
    /// <returns>Instance of FormBinder.</returns>
    public static FormBinder<TEntity> For(Form form, TEntity entity)
    {
        if (entity is null)
        {
            throw new ArgumentNullException(nameof(entity), "Instance of entity cannot be NULL.");
        }

        return new FormBinder<TEntity>(form, entity);
    }

    /// <summary>
    /// Initialise the form-level binder.
    /// </summary>
    /// <param name="form">Instance of the form being bound to.</param>
    /// <param name="entity">Instance of the entity of type <typeparamref name="TEntity"/> this form is bound to.</param>
    private FormBinder(Form form, TEntity entity)
    {
        if (entity is null)
        {
            throw new ArgumentNullException(nameof(entity), "Instance of entity cannot be NULL.");
        }

        if (!TypeDiscoveryFactory.TryResolve(entity.GetType(), out PersistenceContainerInfo? pci))
        {
            throw new TypeLoadException($"Could not resolve type information for entity type '{typeof(TEntity).GetUsableTypeName()}'.");
        }

        _entityInfo = pci;

        _form = form;
        _entity = entity;
        _boundControls = new List<IControlBinder>();

        // Bind to INotifyPropertyChanged if applicable.
        if (entity is INotifyPropertyChanged npc)
        {
            npc.PropertyChanged += Entity_PropertyChanged;
        }
    }

    #endregion

    /// <summary>
    /// Instance of the form.
    /// </summary>
    private readonly Form _form;

    /// <summary>
    /// Instance of the entity this form is bound to.
    /// </summary>
    private readonly TEntity _entity;

    /// <summary>
    /// Metadata about the entity type.
    /// </summary>
    private readonly PersistenceContainerInfo _entityInfo;

    /// <summary>
    /// Collection of bound controls on the current form.
    /// </summary>
    private readonly List<IControlBinder> _boundControls;


    /// <summary>
    /// Parses simple member selector expressions like (e => e.PropertyName) to extract
    /// the property or field metadata.
    /// </summary>
    private sealed class MemberSelectorParser
    {
        /// <summary>
        /// Extracts the <see cref="MemberInfo"/> from a member selector expression.
        /// </summary>
        /// <param name="selector">A lambda expression that selects a member property or field. Example: (e => e.Id)</param>
        /// <returns>The <see cref="MemberInfo"/> for the selected property or field.</returns>
        public static MemberInfo ExtractMember(Expression<Func<TEntity, object>> selector)
        {
            if (selector is null)
            {
                throw new ArgumentNullException(nameof(selector), "Selector expression cannot be NULL.");
            }

            Expression body = selector.Body;

            // Unwrap any Convert/ConvertChecked nodes (these appear when boxing value types to object)
            while ((body is UnaryExpression unary) && ((unary.NodeType is ExpressionType.Convert) || (unary.NodeType is ExpressionType.ConvertChecked)))
            {
                body = unary.Operand;
            }

            // The body should now be a MemberExpression
            if (body is not MemberExpression memberExpression)
            {
                throw new ArgumentException(
                    $"Selector expression must be a simple member access (e.g., 'e => e.PropertyName'). " +
                    $"Functions, method calls, and complex expressions are not allowed. Expression type: {body.NodeType}",
                    nameof(selector));
            }

            // The member expression MUST directly reference the entity parameter
            // We do NOT support chained property access (e.g., e => e.Name.Length)
            if (memberExpression.Expression is not ParameterExpression parameter)
            {
                throw new ArgumentException(
                    $"Selector expression must be a direct member access of the entity (e.g., 'e => e.PropertyName'). " +
                    $"Chained property access (e.g., 'e => e.Name.Length') is not supported for data binding.",
                    nameof(selector));
            }

            // Validate that the parameter is the entity type
            if (parameter.Type != typeof(TEntity))
            {
                throw new ArgumentException(
                    $"Selector expression must access a member of type '{typeof(TEntity).Name}'. " +
                    $"Found parameter of type '{parameter.Type.Name}' instead.",
                    nameof(selector));
            }

            // Return the MemberInfo (PropertyInfo or FieldInfo)
            return memberExpression.Member;
        }
    }
}
