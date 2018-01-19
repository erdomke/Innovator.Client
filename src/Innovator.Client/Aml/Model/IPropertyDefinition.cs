namespace Innovator.Client.Model
{
  public interface IPropertyDefinition
  {
    IProperty_Text ColumnAlignment();
    IProperty_Number ColumnWidth();
    IProperty_Item<ItemType> DataSource();
    IProperty_Text DataType();
    IProperty_Text DefaultValue();
    IProperty_Boolean IsRequired();
    IProperty_Text Label();
    IProperty_Text NameProp();
    IProperty_Text Pattern();
    IProperty_Number Prec();
    IProperty_Boolean Readonly();
    IProperty_Number Scale();
    IProperty_Number StoredLength();
    IProperty_Boolean TrackHistory();
  }
}