using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type rb_UserInputDataSource </summary>
  [ArasName("rb_UserInputDataSource")]
  public class rb_UserInputDataSource : Item, Irb_UserInputDataSource
  {
    protected rb_UserInputDataSource() { }
    public rb_UserInputDataSource(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static rb_UserInputDataSource() { Innovator.Client.Item.AddNullItem<rb_UserInputDataSource>(new rb_UserInputDataSource { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

  }
}