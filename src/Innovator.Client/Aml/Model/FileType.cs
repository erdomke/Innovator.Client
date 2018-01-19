using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type FileType </summary>
  [ArasName("FileType")]
  public class FileType : Item
  {
    protected FileType() { }
    public FileType(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static FileType() { Innovator.Client.Item.AddNullItem<FileType>(new FileType { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>description</c> property of the item</summary>
    [ArasName("description")]
    public IProperty_Text Description()
    {
      return this.Property("description");
    }
    /// <summary>Retrieve the <c>extension</c> property of the item</summary>
    [ArasName("extension")]
    public IProperty_Text Extension()
    {
      return this.Property("extension");
    }
    /// <summary>Retrieve the <c>label</c> property of the item</summary>
    [ArasName("label")]
    public IProperty_Text Label()
    {
      return this.Property("label");
    }
    /// <summary>Retrieve the <c>mimetype</c> property of the item</summary>
    [ArasName("mimetype")]
    public IProperty_Text Mimetype()
    {
      return this.Property("mimetype");
    }
    /// <summary>Retrieve the <c>name</c> property of the item</summary>
    [ArasName("name")]
    public IProperty_Text NameProp()
    {
      return this.Property("name");
    }
    /// <summary>Retrieve the <c>pattern</c> property of the item</summary>
    [ArasName("pattern")]
    public IProperty_Text Pattern()
    {
      return this.Property("pattern");
    }
    /// <summary>Retrieve the <c>priority</c> property of the item</summary>
    [ArasName("priority")]
    public IProperty_Number Priority()
    {
      return this.Property("priority");
    }
    /// <summary>Retrieve the <c>rule_type</c> property of the item</summary>
    [ArasName("rule_type")]
    public IProperty_Text RuleType()
    {
      return this.Property("rule_type");
    }
  }
}