using Innovator.Client;
using System;

namespace Innovator.Client.Model
{
  ///<summary>Class for the item type User </summary>
  [ArasName("User")]
  public class User : Item
  {
    protected User() { }
    public User(ElementFactory amlContext, params object[] content) : base(amlContext, content) { }
    static User() { Innovator.Client.Item.AddNullItem<User>(new User { _attr = ElementAttributes.ReadOnly | ElementAttributes.Null }); }

    /// <summary>Retrieve the <c>cell</c> property of the item</summary>
    [ArasName("cell")]
    public IProperty_Text Cell()
    {
      return this.Property("cell");
    }
    /// <summary>Retrieve the <c>company_name</c> property of the item</summary>
    [ArasName("company_name")]
    public IProperty_Text CompanyName()
    {
      return this.Property("company_name");
    }
    /// <summary>Retrieve the <c>default_vault</c> property of the item</summary>
    [ArasName("default_vault")]
    public IProperty_Item<Vault> DefaultVault()
    {
      return this.Property("default_vault");
    }
    /// <summary>Retrieve the <c>email</c> property of the item</summary>
    [ArasName("email")]
    public IProperty_Text Email()
    {
      return this.Property("email");
    }
    /// <summary>Retrieve the <c>esignature</c> property of the item</summary>
    [ArasName("esignature")]
    public IProperty_Text Esignature()
    {
      return this.Property("esignature");
    }
    /// <summary>Retrieve the <c>fax</c> property of the item</summary>
    [ArasName("fax")]
    public IProperty_Text Fax()
    {
      return this.Property("fax");
    }
    /// <summary>Retrieve the <c>first_name</c> property of the item</summary>
    [ArasName("first_name")]
    public IProperty_Text FirstName()
    {
      return this.Property("first_name");
    }
    /// <summary>Retrieve the <c>home_phone</c> property of the item</summary>
    [ArasName("home_phone")]
    public IProperty_Text HomePhone()
    {
      return this.Property("home_phone");
    }
    /// <summary>Retrieve the <c>label</c> property of the item</summary>
    [ArasName("label")]
    public IProperty_Text Label()
    {
      return this.Property("label");
    }
    /// <summary>Retrieve the <c>last_login_date</c> property of the item</summary>
    [ArasName("last_login_date")]
    public IProperty_Date LastLoginDate()
    {
      return this.Property("last_login_date");
    }
    /// <summary>Retrieve the <c>last_name</c> property of the item</summary>
    [ArasName("last_name")]
    public IProperty_Text LastName()
    {
      return this.Property("last_name");
    }
    /// <summary>Retrieve the <c>login_name</c> property of the item</summary>
    [ArasName("login_name")]
    public IProperty_Text LoginName()
    {
      return this.Property("login_name");
    }
    /// <summary>Retrieve the <c>logon_enabled</c> property of the item</summary>
    [ArasName("logon_enabled")]
    public IProperty_Boolean LogonEnabled()
    {
      return this.Property("logon_enabled");
    }
    /// <summary>Retrieve the <c>mail_stop</c> property of the item</summary>
    [ArasName("mail_stop")]
    public IProperty_Text MailStop()
    {
      return this.Property("mail_stop");
    }
    /// <summary>Retrieve the <c>manager</c> property of the item</summary>
    [ArasName("manager")]
    public IProperty_Item<User> Manager()
    {
      return this.Property("manager");
    }
    /// <summary>Retrieve the <c>pager</c> property of the item</summary>
    [ArasName("pager")]
    public IProperty_Text Pager()
    {
      return this.Property("pager");
    }
    /// <summary>Retrieve the <c>password</c> property of the item</summary>
    [ArasName("password")]
    public IProperty_Text Password()
    {
      return this.Property("password");
    }
    /// <summary>Retrieve the <c>picture</c> property of the item</summary>
    [ArasName("picture")]
    public IProperty_Text Picture()
    {
      return this.Property("picture");
    }
    /// <summary>Retrieve the <c>pwd_is_set_on</c> property of the item</summary>
    [ArasName("pwd_is_set_on")]
    public IProperty_Date PwdIsSetOn()
    {
      return this.Property("pwd_is_set_on");
    }
    /// <summary>Retrieve the <c>starting_page</c> property of the item</summary>
    [ArasName("starting_page")]
    public IProperty_Text StartingPage()
    {
      return this.Property("starting_page");
    }
    /// <summary>Retrieve the <c>telephone</c> property of the item</summary>
    [ArasName("telephone")]
    public IProperty_Text Telephone()
    {
      return this.Property("telephone");
    }
    /// <summary>Retrieve the <c>user_no</c> property of the item</summary>
    [ArasName("user_no")]
    public IProperty_Text UserNo()
    {
      return this.Property("user_no");
    }
    /// <summary>Retrieve the <c>working_directory</c> property of the item</summary>
    [ArasName("working_directory")]
    public IProperty_Text WorkingDirectory()
    {
      return this.Property("working_directory");
    }
  }
}