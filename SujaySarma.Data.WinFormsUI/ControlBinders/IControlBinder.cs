namespace SujaySarma.Data.WinFormsUI.ControlBinders;

/// <summary>
/// An interface definition that needs to be implemented by all control binders.
/// </summary>
internal interface IControlBinder
{
    /// <summary>
    /// Sets the value of the control using the current value of the specified member of the entity instance (<paramref name="dataContext"/>).
    /// </summary>
    /// <param name="dataContext">Instance of the entity whose member is being bound (this should be the class/struct/record and not its member!).</param>
    public void BindControl(object dataContext);

    /// <summary>
    /// Sets the value of the member of the entity instance (<paramref name="dataContext"/>) 
    /// from the current value of the control.
    /// </summary>
    /// <param name="dataContext">Instance of the entity whose member is being bound (this should be the class/struct/record and not its member!).</param>
    public void BindEntityMember(object dataContext);

    /// <summary>
    /// Tests if this binder has a binding for the provided <paramref name="propertyName" />.
    /// </summary>
    /// <param name="propertyName">Name of the property to check for.</param>
    /// <returns>True if the binding exists.</returns>
    public bool BindsProperty(string propertyName);

    /// <summary>
    /// Refreshes the value displayed on the control from the instance of the entity/property.
    /// </summary>
    public void RefreshControl();
}
