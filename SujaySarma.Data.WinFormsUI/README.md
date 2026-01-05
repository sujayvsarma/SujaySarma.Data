# SujaySarma.Data.WinFormsUI

**Painless data binding for Windows Forms applications with the SujaySarma.Data.* ORM ecosystem.**

[![NuGet](https://img.shields.io/nuget/v/SujaySarma.Data.WinFormsUI.svg)](https://www.nuget.org/packages/SujaySarma.Data.WinFormsUI)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

---

## Overview

`SujaySarma.Data.WinFormsUI` provides a fluent, type-safe API for binding Windows Forms controls to business entities decorated with `SujaySarma.Data.Core` attributes. It eliminates boilerplate code for synchronizing UI controls with data models, supporting both one-way (data-to-UI) and two-way (data↔UI) binding.

This library is **exclusively** designed to work with the `SujaySarma.Data.*` ORM frameworks and will **not** work with other ORM systems like Entity Framework.

---

## Installation

```
$ dotnet add package SujaySarma.Data.WinFormsUI
```


**NuGet Package:** [SujaySarma.Data.WinFormsUI](https://www.nuget.org/packages/SujaySarma.Data.WinFormsUI)

**Current Version:** `10.0.0.0`

**Target Frameworks:** `.NET 6.0-windows`, `.NET 8.0-windows`, `.NET 10.0-windows`

---

## Features

- **Type-Safe Binding** – Uses lambda expressions to bind controls to entity properties/fields
- **One-Way & Two-Way Binding** – Automatic detection based on control state (read-only → one-way, editable → two-way)
- **Fluent API** – Chainable methods for binding multiple controls
- **Supported Controls** – TextBox, Label, CheckBox, ComboBox, ListBox, CheckedListBox, RadioButton groups
- **Automatic Type Conversion** – Handles conversion between control values and entity properties
- **ORM Integration** – Seamless integration with `SujaySarma.Data.Core` attributes

---

## Quick Start

### 1. Decorate Your Entity

```c#
using SujaySarma.Data.Core.Attributes;

[PersistenceContainer("Customers")] 
public class Customer 
{ 
	[PersistenceContainerMember("CustomerId")] 
	public int CustomerId { get; set; }

	[PersistenceContainerMember("Name")]
	public string Name { get; set; } = string.Empty;

	[PersistenceContainerMember("Email")]
	public string Email { get; set; } = string.Empty;

	[PersistenceContainerMember("IsActive")]
	public bool IsActive { get; set; }

	[PersistenceContainerMember("Role")]
	public string Role { get; set; } = "User";
}
```

### 2. Bind Controls in Your Form

```c#
using SujaySarma.Data.WinFormsUI;

public partial class CustomerForm : Form 
{ 
	private FormBinder<Customer> _binder; 
	private Customer _customer;

	public CustomerForm()
    {
        InitializeComponent();

        // Create a new customer instance
        _customer = new Customer();

        // Initialize the binder
        _binder = FormBinder<Customer>.For(this, _customer)
            .AddTextBox(txtName, c => c.Name)
            .AddTextBox(txtEmail, c => c.Email)
            .AddCheckBox(chkIsActive, c => c.IsActive)
            .AddComboBox(cboRole, c => c.Role, new Dictionary<string, string>
            {
                { "Admin", "Administrator" },
                { "User", "Regular User" },
                { "Guest", "Guest User" }
            })
            .AddLabel(lblCustomerId, c => c.CustomerId);
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        // Extract values from controls back to the entity
        _customer = _binder.Retrieve();

        // Save to database (using your ORM)
        // ...

        MessageBox.Show("Customer saved successfully!");
    }

    private void LoadCustomer(int customerId)
    {
        // Load customer from database
        // _customer = ...;

        // Populate controls from entity
        _binder = FormBinder<Customer>.For(this, _customer)
            .AddTextBox(txtName, c => c.Name)
            .AddTextBox(txtEmail, c => c.Email)
            .AddCheckBox(chkIsActive, c => c.IsActive)
            .AddComboBox(cboRole, c => c.Role, new Dictionary<string, string>
            {
                { "Admin", "Administrator" },
                { "User", "Regular User" }
            })
            .AddLabel(lblCustomerId, c => c.CustomerId);

        _binder.Bind();
    }
}
```


---

## API Reference

### `FormBinder<TEntity>`

The primary class for binding Windows Forms controls to entity properties/fields.

#### Static Factory Methods

```c#
static FormBinder<TEntity> For(Form form)
```

Creates a binder for the specified form with a **new** instance of `TEntity`.

**Parameters:**
- `form` - The Windows Form containing the controls to bind

**Returns:** `FormBinder<TEntity>`

**Example:**

```c#
FormBinder<Customer> binder = FormBinder<Customer>.For(this);
```

```c#
static FormBinder<TEntity> For(Form form, TEntity entity)
```

Creates a binder for the specified form bound to an **existing** entity instance.

**Parameters:**
- `form` - The Windows Form containing the controls to bind
- `entity` - Existing entity instance to bind to

**Returns:** `FormBinder<TEntity>`

**Example:**

```c#
Customer customer = LoadCustomer(customerId); 
FormBinder<Customer> binder = FormBinder<Customer>.For(this, customer);
```
---

#### Control Binding Methods

Method | Description
-------|---------------------
AddTextBox() | Binds a `TextBox` to an entity property/field.
AddLabel() | Binds a `Label` to an entity property/field.
AddCheckBox() | Binds a `CheckBox` to a boolean entity property/field.
AddComboBox() | Binds a `ComboBox` to an entity property/field with optional value-display mapping.
AddListBox() | Binds a `ListBox` to an entity collection property/field.
AddCheckedListBox() | Binds a `CheckedListBox` to an entity collection property/field.
AddRadioButtonGroup() | Binds a group of `RadioButton` controls to an entity property/field.

> When binding a group of radio buttons (`AddRadioButtonGroup`), you need to provide the containing control to the method. For example, if you have a Panel or a GroupBox control on the form and that panel/groupbox has the radio buttons, then you must provide the panel or groupbox to AddRadioButtonGroup.


---

#### Data Synchronization Methods

##### `Bind()`

Populates all bound controls from the current entity values (entity → controls).

##### `Retrieve()`

Extracts values from all bound controls back to the entity (controls → entity).

**Returns:** The updated entity instance


---

## Advanced Patterns

### Master-Detail Form

```c#
public partial class OrderForm : Form 
{ 
    private FormBinder<Order> _orderBinder; 
    private FormBinder<Customer> _customerBinder;

    private void LoadOrder(int orderId)
    {
        Order order = LoadOrderFromDb(orderId);
        Customer customer = LoadCustomerFromDb(order.CustomerId);

        // Bind order details
        _orderBinder = FormBinder<Order>.For(this, order)
            .AddTextBox(txtOrderId, o => o.OrderId)
            .AddTextBox(txtOrderDate, o => o.OrderDate)
            .AddTextBox(txtTotal, o => o.TotalAmount);

        // Bind customer details
        _customerBinder = FormBinder<Customer>.For(this, customer)
            .AddLabel(lblCustomerName, c => c.Name)
            .AddLabel(lblCustomerEmail, c => c.Email);

        _orderBinder.Bind();
        _customerBinder.Bind();
    }

    private void btnSave_Click(object sender, EventArgs e) 
    { 
        // Extract data from controls 
        Customer customer = _binder.Retrieve();

        // Perform data validations here...

        // Save to database
        SaveCustomer(customer);
        MessageBox.Show("Customer saved successfully!");
    }
}
```


### Binding to Enums

You can bind controls to enum types directly, using the special overload that accepts an enum as its value source. To have the binder display custom text on the control instead of stringifying the enum's name, use the `Display` attribute annotation with the `Name` value on the enum's members:

```c#

using System.ComponentModel.DataAnnotations;

enum Demo
{
    // No 'Display', binder will render "One"
    One,
    
    // No 'Display', binder will render "HowMany"
    HowMany,

    // Has a Display annotation, binder will render "This is annotated"
    [Display(Name = "This Is Annotated")]
    ThisIsAnnotated
}

```

---

## Supported Data Types

### TextBox & Label Bindings

| .NET Type | Display Format |
|-----------|----------------|
| `string` | As-is |
| `int`, `long`, `decimal`, etc. | Numeric string |
| `DateTime` | `"MMM dd, yyyy HH:mm:ss.fff"` |
| `DateOnly` | `"MMM dd, yyyy"` |
| `TimeOnly` | `"HH:mm:ss"` |
| `DateTimeOffset` | `"MMM dd, yyyy HH:mm:ss.fff"` |
| `Guid` | `"'<guid>'"` |
| Enums | See 'Binding to Enums' section above. |

### CheckBox Bindings

- **Only** `bool` type supported

### ComboBox / ListBox / RadioButton Bindings

- Any type (`TValue` generic parameter)
- Primitives: Use direct values
- Complex types: Specify `displayMember` and `valueMember`

---

## Requirements

- **Windows Forms** application (.NET 6.0-windows, .NET 8.0-windows, or .NET 10.0-windows)
- **SujaySarma.Data.Core** 10.0.0.0 or higher
- Entities decorated with `[PersistenceContainer]` and `[PersistenceContainerMember]` attributes

---

## Limitations

- **ORM Requirement:** Entities **must** use `SujaySarma.Data.Core` attributes. This library will **not** work with Entity Framework or other ORMs.
- **Member Selectors:** Only simple member access is supported (e.g., `c => c.Name`). **Chained properties** (`c => c.Address.City`) and **method calls** (`c => c.Name.ToUpper()`) are **not supported**.
- **DataGridView:** Not currently supported (use built-in WinForms DataGridView binding).

---

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Submit a pull request with clear descriptions

For issues, feature requests, or feedback, please [create an issue](https://github.com/sujayvsarma/SujaySarma.Data/issues) on GitHub.

---

## License

This library is licensed under the [MIT License](LICENSE).
Copyright (c) 2025 and beyond, Sujay V. Sarma. All rights reserved.

---

## Author

**Sujay V. Sarma**

- GitHub: [@sujayvsarma](https://github.com/sujayvsarma)
- Repository: [SujaySarma.Data](https://github.com/sujayvsarma/SujaySarma.Data)

---

## Important Notes

> ⚠️ **ORM Compatibility:** This library is **exclusively** designed for `SujaySarma.Data.*` ORMs. It will **not** work with Entity Framework, Dapper, or other ORM systems.

> 🔒 **Type Safety:** The fluent API uses lambda expressions to provide compile-time type safety and IntelliSense support.

> 🚀 **Version 10.0.0.0:** Complete rewrite – NOT backwards compatible with previous versions.

---