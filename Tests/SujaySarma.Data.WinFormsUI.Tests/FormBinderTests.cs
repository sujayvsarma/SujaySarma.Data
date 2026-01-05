using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.Core.Attributes;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace SujaySarma.Data.WinFormsUI.Tests;

[TestClass]
public class FormBinderTests
{
    [PersistenceContainer("Table1")]
    private class TestEntity
    {
        [PersistenceContainerMember("Name")]
        public string Name { get; set; } = string.Empty;

        [PersistenceContainerMember("Age")]
        public int Age { get; set; }

        [PersistenceContainerMember("IsActive")]
        public bool IsActive { get; set; }

        [PersistenceContainerMember("Role")]
        public string Role { get; set; } = string.Empty;

        [PersistenceContainerMember("Tags")]
        public string Tags { get; set; } = string.Empty;

        [PersistenceContainerMember("Notes")]
        public string Notes { get; set; } = string.Empty;
    }

    private class ValueItem
    {
        public int Id { get; set; }
        public string DisplayText { get; set; } = string.Empty;
    }

    #region Functional Tests - TextBox

    [TestMethod]
    [TestCategory("Functional")]
    public void AddTextBox_ValidMember_AddsControlSuccessfully()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity { Name = "Test" };
        TextBox textBox = new TextBox();

        // Act
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity);
        FormBinder<TestEntity> result = binder.AddTextBox(textBox, e => e.Name);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(binder, result);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Bind_TextBoxControl_PopulatesControlFromEntity()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity { Name = "John Doe" };
        TextBox textBox = new TextBox();
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity)
            .AddTextBox(textBox, e => e.Name);

        // Act
        binder.Bind();

        // Assert
        Assert.AreEqual("John Doe", textBox.Text);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Retrieve_TextBoxControl_ExtractsValueToEntity()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity();
        TextBox textBox = new TextBox { Text = "Jane Smith" };
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity)
            .AddTextBox(textBox, e => e.Name);

        // Act
        TestEntity result = binder.Retrieve();

        // Assert
        Assert.AreEqual("Jane Smith", result.Name);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Bind_TextBoxWithNumericValue_PopulatesControlFromEntity()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity { Age = 25 };
        TextBox textBox = new TextBox();
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity)
            .AddTextBox(textBox, e => e.Age);

        // Act
        binder.Bind();

        // Assert
        Assert.AreEqual("25", textBox.Text);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Retrieve_TextBoxWithNumericValue_ExtractsValueToEntity()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity();
        TextBox textBox = new TextBox { Text = "30" };
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity)
            .AddTextBox(textBox, e => e.Age);

        // Act
        TestEntity result = binder.Retrieve();

        // Assert
        Assert.AreEqual(30, result.Age);
    }

    #endregion

    #region Functional Tests - CheckBox

    [TestMethod]
    [TestCategory("Functional")]
    public void AddCheckBox_ValidMember_AddsControlSuccessfully()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity { IsActive = true };
        CheckBox checkBox = new CheckBox();

        // Act
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity);
        FormBinder<TestEntity> result = binder.AddCheckBox(checkBox, e => e.IsActive);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(binder, result);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Bind_CheckBoxControl_PopulatesControlFromEntity()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity { IsActive = true };
        CheckBox checkBox = new CheckBox();
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity)
            .AddCheckBox(checkBox, e => e.IsActive);

        // Act
        binder.Bind();

        // Assert
        Assert.IsTrue(checkBox.Checked);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Retrieve_CheckBoxControl_ExtractsValueToEntity()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity();
        CheckBox checkBox = new CheckBox { Checked = true, Enabled = true };
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity)
            .AddCheckBox(checkBox, e => e.IsActive);

        // Act
        TestEntity result = binder.Retrieve();

        // Assert
        Assert.IsTrue(result.IsActive);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Bind_CheckBoxControl_UncheckedState_PopulatesControlFromEntity()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity { IsActive = false };
        CheckBox checkBox = new CheckBox();
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity)
            .AddCheckBox(checkBox, e => e.IsActive);

        // Act
        binder.Bind();

        // Assert
        Assert.IsFalse(checkBox.Checked);
    }

    #endregion

    #region Functional Tests - Label

    [TestMethod]
    [TestCategory("Functional")]
    public void AddLabel_ValidMember_AddsControlSuccessfully()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity { Name = "Test" };
        Label label = new Label();

        // Act
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity);
        FormBinder<TestEntity> result = binder.AddLabel(label, e => e.Name);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(binder, result);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Bind_LabelControl_PopulatesControlFromEntity()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity { Name = "John Doe" };
        Label label = new Label();
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity)
            .AddLabel(label, e => e.Name);

        // Act
        binder.Bind();

        // Assert
        Assert.AreEqual("John Doe", label.Text);
    }

    #endregion

    #region Functional Tests - ComboBox

    [TestMethod]
    [TestCategory("Functional")]
    public void AddComboBox_WithDictionary_AddsControlSuccessfully()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity { Role = "Admin" };
        ComboBox comboBox = new ComboBox();
        Dictionary<string, string> roles = new Dictionary<string, string>
        {
            { "Admin", "Administrator" },
            { "User", "Regular User" }
        };

        // Act
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity);
        FormBinder<TestEntity> result = binder.AddComboBox(comboBox, e => e.Role, roles);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(binder, result);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void AddComboBox_WithEnumerable_AddsControlSuccessfully()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity { Age = 1 };
        ComboBox comboBox = new ComboBox();
        List<ValueItem> items = new List<ValueItem>
        {
            new ValueItem { Id = 1, DisplayText = "One" },
            new ValueItem { Id = 2, DisplayText = "Two" }
        };

        // Act
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity);
        FormBinder<TestEntity> result = binder.AddComboBox(comboBox, e => e.Age, items, "DisplayText", "Id");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(binder, result);
    }

    #endregion

    #region Functional Tests - ListBox

    [TestMethod]
    [TestCategory("Functional")]
    public void AddListBox_WithDictionary_AddsControlSuccessfully()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity { Tags = "Tag1" };
        ListBox listBox = new ListBox();
        Dictionary<string, string> tags = new Dictionary<string, string>
        {
            { "Tag1", "First Tag" },
            { "Tag2", "Second Tag" }
        };

        // Act
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity);
        FormBinder<TestEntity> result = binder.AddListBox(listBox, e => e.Tags, tags);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(binder, result);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void AddListBox_WithEnumerable_AddsControlSuccessfully()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity { Age = 1 };
        ListBox listBox = new ListBox();
        List<ValueItem> items = new List<ValueItem>
        {
            new ValueItem { Id = 1, DisplayText = "One" },
            new ValueItem { Id = 2, DisplayText = "Two" }
        };

        // Act
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity);
        FormBinder<TestEntity> result = binder.AddListBox(listBox, e => e.Age, items, "DisplayText", "Id");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(binder, result);
    }

    #endregion

    #region Functional Tests - CheckedListBox

    [TestMethod]
    [TestCategory("Functional")]
    public void AddCheckedListBox_WithDictionary_AddsControlSuccessfully()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity { Tags = "Tag1" };
        CheckedListBox checkedListBox = new CheckedListBox();
        Dictionary<string, string> tags = new Dictionary<string, string>
        {
            { "Tag1", "First Tag" },
            { "Tag2", "Second Tag" }
        };

        // Act
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity);
        FormBinder<TestEntity> result = binder.AddCheckedListBox(checkedListBox, e => e.Tags, tags);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(binder, result);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void AddCheckedListBox_WithEnumerable_AddsControlSuccessfully()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity { Age = 1 };
        CheckedListBox checkedListBox = new CheckedListBox();
        List<ValueItem> items = new List<ValueItem>
        {
            new ValueItem { Id = 1, DisplayText = "One" },
            new ValueItem { Id = 2, DisplayText = "Two" }
        };

        // Act
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity);
        FormBinder<TestEntity> result = binder.AddCheckedListBox(checkedListBox, e => e.Age, items, "DisplayText", "Id");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(binder, result);
    }

    #endregion

    #region Functional Tests - RadioButtonGroup

    [TestMethod]
    [TestCategory("Functional")]
    public void AddRadioButtonGroup_WithDictionary_AddsControlSuccessfully()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity { Role = "Admin" };
        Panel panel = new Panel();
        RadioButton rbOne = new RadioButton();
        RadioButton rbTwo = new RadioButton();
        panel.Controls.Add(rbOne);
        panel.Controls.Add(rbTwo);

        Dictionary<string, string> roles = new Dictionary<string, string>
        {
            { "Admin", "Administrator" },
            { "User", "Regular User" }
        };

        // Act
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity);
        FormBinder<TestEntity> result = binder.AddRadioButtonGroup(panel, e => e.Role, roles);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(binder, result);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void AddRadioButtonGroup_WithEnumerable_AddsControlSuccessfully()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity { Age = 1 };
        Panel panel = new Panel();
        RadioButton rbOne = new RadioButton();
        RadioButton rbTwo = new RadioButton();
        panel.Controls.Add(rbOne);
        panel.Controls.Add(rbTwo);

        List<ValueItem> items = new List<ValueItem>
        {
            new ValueItem { Id = 1, DisplayText = "One" },
            new ValueItem { Id = 2, DisplayText = "Two" }
        };

        // Act
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity);
        FormBinder<TestEntity> result = binder.AddRadioButtonGroup(panel, e => e.Age, items, "DisplayText", "Id");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(binder, result);
    }

    #endregion

    #region Functional Tests - Multiple Controls

    [TestMethod]
    [TestCategory("Functional")]
    public void Bind_MultipleControls_PopulatesAllControlsFromEntity()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity { Name = "John Doe", Age = 30, IsActive = true };
        TextBox textBoxName = new TextBox();
        TextBox textBoxAge = new TextBox();
        CheckBox checkBoxActive = new CheckBox();

        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity)
            .AddTextBox(textBoxName, e => e.Name)
            .AddTextBox(textBoxAge, e => e.Age)
            .AddCheckBox(checkBoxActive, e => e.IsActive);

        // Act
        binder.Bind();

        // Assert
        Assert.AreEqual("John Doe", textBoxName.Text);
        Assert.AreEqual("30", textBoxAge.Text);
        Assert.IsTrue(checkBoxActive.Checked);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Retrieve_MultipleControls_ExtractsAllValuesToEntity()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity();
        TextBox textBoxName = new TextBox { Text = "Jane Smith" };
        TextBox textBoxAge = new TextBox { Text = "25" };
        CheckBox checkBoxActive = new CheckBox { Checked = true };

        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity)
            .AddTextBox(textBoxName, e => e.Name)
            .AddTextBox(textBoxAge, e => e.Age)
            .AddCheckBox(checkBoxActive, e => e.IsActive);

        // Act
        TestEntity result = binder.Retrieve();

        // Assert
        Assert.AreEqual("Jane Smith", result.Name);
        Assert.AreEqual(25, result.Age);
        Assert.IsTrue(result.IsActive);
    }

    #endregion

    #region Functional Tests - Initializers

    [TestMethod]
    [TestCategory("Functional")]
    public void For_WithForm_CreatesNewEntityInstance()
    {
        // Arrange
        Form form = new Form();

        // Act
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form);

        // Assert
        Assert.IsNotNull(binder);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void For_WithFormAndEntity_UsesProvidedEntityInstance()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity { Name = "Test", Age = 25 };

        // Act
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity);
        TextBox textBox = new TextBox();
        binder.AddTextBox(textBox, e => e.Name);
        binder.Bind();

        // Assert
        Assert.AreEqual("Test", textBox.Text);
    }

    #endregion

    #region Negative Tests - AddTextBox

    [TestMethod]
    [TestCategory("Negative")]
    public void AddTextBox_NullMemberSelector_ThrowsArgumentNullException()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity();
        TextBox textBox = new TextBox();
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity);

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            binder.AddTextBox(textBox, null!));
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void AddTextBox_InvalidMemberSelector_ThrowsArgumentException()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity();
        TextBox textBox = new TextBox();
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity);

        // Act & Assert
        ArgumentException ex = Assert.ThrowsExactly<ArgumentException>(() =>
            binder.AddTextBox(textBox, e => e.Name.ToUpper()));

        // Assert exception message contains expected text
        Assert.Contains("simple member access", ex.Message);
        Assert.Contains("Functions, method calls", ex.Message);
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void AddTextBox_NestedPropertySelector_ThrowsArgumentException()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity();
        TextBox textBox = new TextBox();
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity);

        // Act & Assert
        ArgumentException ex = Assert.ThrowsExactly<ArgumentException>(() =>
            binder.AddTextBox(textBox, e => e.Name.Length));

        // Assert exception message contains expected text
        Assert.Contains("must be a direct member access", ex.Message);
    }

    #endregion

    #region Negative Tests - AddCheckBox

    [TestMethod]
    [TestCategory("Negative")]
    public void AddCheckBox_NullMemberSelector_ThrowsArgumentNullException()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity();
        CheckBox checkBox = new CheckBox();
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity);

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            binder.AddCheckBox(checkBox, null!));
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void AddCheckBox_InvalidMemberSelector_ThrowsArgumentException()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity();
        CheckBox checkBox = new CheckBox();
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity);

        // Act & Assert
        ArgumentException ex = Assert.ThrowsExactly<ArgumentException>(() =>
            binder.AddCheckBox(checkBox, e => !e.IsActive));

        // Assert exception message contains expected text
        Assert.Contains("simple member access", ex.Message);
    }

    #endregion

    #region Negative Tests - AddLabel

    [TestMethod]
    [TestCategory("Negative")]
    public void AddLabel_NullMemberSelector_ThrowsArgumentNullException()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity();
        Label label = new Label();
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity);

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            binder.AddLabel(label, null!));
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void AddLabel_InvalidMemberSelector_ThrowsArgumentException()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity();
        Label label = new Label();
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity);

        // Act & Assert
        ArgumentException ex = Assert.ThrowsExactly<ArgumentException>(() =>
            binder.AddLabel(label, e => e.Name.ToUpper()));

        // Assert exception message contains expected text
        Assert.Contains("simple member access", ex.Message);
    }

    #endregion

    #region Negative Tests - AddComboBox

    [TestMethod]
    [TestCategory("Negative")]
    public void AddComboBox_NullMemberSelector_ThrowsArgumentNullException()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity();
        ComboBox comboBox = new ComboBox();
        Dictionary<string, string> values = new Dictionary<string, string>();
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity);

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            binder.AddComboBox(comboBox, null!, values));
    }

    #endregion

    #region Negative Tests - AddListBox

    [TestMethod]
    [TestCategory("Negative")]
    public void AddListBox_NullMemberSelector_ThrowsArgumentNullException()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity();
        ListBox listBox = new ListBox();
        Dictionary<string, string> values = new Dictionary<string, string>();
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity);

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            binder.AddListBox(listBox, null!, values));
    }

    #endregion

    #region Negative Tests - AddCheckedListBox

    [TestMethod]
    [TestCategory("Negative")]
    public void AddCheckedListBox_NullMemberSelector_ThrowsArgumentNullException()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity();
        CheckedListBox checkedListBox = new CheckedListBox();
        Dictionary<string, string> values = new Dictionary<string, string>();
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity);

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            binder.AddCheckedListBox(checkedListBox, null!, values));
    }

    #endregion

    #region Negative Tests - AddRadioButtonGroup

    [TestMethod]
    [TestCategory("Negative")]
    public void AddRadioButtonGroup_NullMemberSelector_ThrowsArgumentNullException()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity();
        Panel panel = new Panel();
        Dictionary<string, string> values = new Dictionary<string, string>();
        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity);

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            binder.AddRadioButtonGroup(panel, null!, values));
    }

    #endregion

    #region Negative Tests - Initializers

    [TestMethod]
    [TestCategory("Negative")]
    public void For_WithNullEntity_ThrowsArgumentNullException()
    {
        // Arrange
        Form form = new Form();
        TestEntity? entity = null;

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            FormBinder<TestEntity>.For(form, entity!));
    }

    #endregion

    // Change the performance test assertions to use the correct method signature

    #region Performance Tests

    [TestMethod]
    [TestCategory("Performance")]
    public void Bind_MultipleControls_CompletesInReasonableTime()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity
        {
            Name = "John Doe",
            Age = 30,
            IsActive = true,
            Role = "Admin",
            Tags = "Tag1",
            Notes = "Test notes"
        };

        TextBox textBoxName = new TextBox();
        TextBox textBoxAge = new TextBox();
        CheckBox checkBoxActive = new CheckBox();
        TextBox textBoxRole = new TextBox();
        TextBox textBoxTags = new TextBox();
        TextBox textBoxNotes = new TextBox();

        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity)
            .AddTextBox(textBoxName, e => e.Name)
            .AddTextBox(textBoxAge, e => e.Age)
            .AddCheckBox(checkBoxActive, e => e.IsActive)
            .AddTextBox(textBoxRole, e => e.Role)
            .AddTextBox(textBoxTags, e => e.Tags)
            .AddTextBox(textBoxNotes, e => e.Notes);

        Stopwatch stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            binder.Bind();
        }

        stopwatch.Stop();

        // Assert - Note the parameter order: upperBound comes FIRST, then value
        Assert.IsLessThanOrEqualTo<long>(1000, stopwatch.ElapsedMilliseconds,
            $"Operation took {stopwatch.ElapsedMilliseconds}ms, expected <= 1000ms");
    }

    [TestMethod]
    [TestCategory("Performance")]
    public void Retrieve_MultipleControls_CompletesInReasonableTime()
    {
        // Arrange
        Form form = new Form();
        TestEntity entity = new TestEntity();

        TextBox textBoxName = new TextBox { Text = "John Doe" };
        TextBox textBoxAge = new TextBox { Text = "30" };
        CheckBox checkBoxActive = new CheckBox { Checked = true };
        TextBox textBoxRole = new TextBox { Text = "Admin" };
        TextBox textBoxTags = new TextBox { Text = "Tag1" };
        TextBox textBoxNotes = new TextBox { Text = "Test notes" };

        FormBinder<TestEntity> binder = FormBinder<TestEntity>.For(form, entity)
            .AddTextBox(textBoxName, e => e.Name)
            .AddTextBox(textBoxAge, e => e.Age)
            .AddCheckBox(checkBoxActive, e => e.IsActive)
            .AddTextBox(textBoxRole, e => e.Role)
            .AddTextBox(textBoxTags, e => e.Tags)
            .AddTextBox(textBoxNotes, e => e.Notes);

        Stopwatch stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            TestEntity result = binder.Retrieve();
        }

        stopwatch.Stop();

        // Assert - Note the parameter order: upperBound comes FIRST, then value
        Assert.IsLessThanOrEqualTo<long>(1000, stopwatch.ElapsedMilliseconds,
            $"Operation took {stopwatch.ElapsedMilliseconds}ms, expected <= 1000ms");
    }

    #endregion
}