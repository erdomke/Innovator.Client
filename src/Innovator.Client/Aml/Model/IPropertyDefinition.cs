namespace Innovator.Client.Model
{
  /// <summary>
  /// The definition of a property, either a normal itemtype property or an xProperty
  /// </summary>
  /// <seealso cref="Innovator.Client.IItem" />
  public interface IPropertyDefinition : IItem
  {
    /// <summary>Retrieve the <c>column_alignment</c> property of the item</summary>
    IProperty_Text ColumnAlignment();
    /// <summary>Retrieve the <c>column_width</c> property of the item</summary>
    IProperty_Number ColumnWidth();
    /// <summary>Retrieve the <c>data_source</c> property of the item</summary>
    IProperty_Item<ItemType> DataSource();
    /// <summary>Retrieve the <c>data_type</c> property of the item</summary>
    IProperty_Text DataType();
    /// <summary>Retrieve the <c>default_value</c> property of the item</summary>
    IProperty_Text DefaultValue();
    /// <summary>Retrieve the <c>is_required</c> property of the item</summary>
    IProperty_Boolean IsRequired();
    /// <summary>Retrieve the <c>label</c> property of the item</summary>
    IProperty_Text Label();
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    IProperty_Text NameProp();
    /// <summary>Retrieve the <c>pattern</c> property of the item</summary>
    IProperty_Text Pattern();
    /// <summary>Retrieve the <c>prec</c> property of the item</summary>
    IProperty_Number Prec();
    /// <summary>Retrieve the <c>readonly</c> property of the item</summary>
    IProperty_Boolean Readonly();
    /// <summary>Retrieve the <c>scale</c> property of the item</summary>
    IProperty_Number Scale();
    /// <summary>Retrieve the <c>stored_length</c> property of the item</summary>
    IProperty_Number StoredLength();
    /// <summary>Retrieve the <c>track_history</c> property of the item</summary>
    IProperty_Boolean TrackHistory();
  }
}
