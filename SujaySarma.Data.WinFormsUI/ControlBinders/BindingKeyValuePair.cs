namespace SujaySarma.Data.WinFormsUI.ControlBinders;

/// <summary>
/// Represents a key-value pair for binding purposes.
/// </summary>
public class BindingKeyValuePair
{

    /// <summary>
    /// The displayable value.
    /// </summary>
    public string Key
    {
        get;
        set;

    } = default!;

    /// <summary>
    /// The internal value.
    /// </summary>
    public object Value
    {
        get;
        set;

    } = default!;


}