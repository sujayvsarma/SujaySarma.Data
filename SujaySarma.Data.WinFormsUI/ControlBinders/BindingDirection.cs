namespace SujaySarma.Data.WinFormsUI.ControlBinders;


/// <summary>
/// Direction of binding.
/// </summary>
public enum BindingDirection
{
    /// <summary>
    /// One way binding: Data to control.
    /// </summary>
    
    OneWay = 1,

    /// <summary>
    /// Two-way binding: Data to control and control to data.
    /// </summary>
    TwoWay = 2
}
