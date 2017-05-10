namespace Innovator.Client.Model
{
  public class ArasNameAttribute : System.Attribute
  {
    private string _name;

    public string Name { get { return _name; } }

    public ArasNameAttribute(string name)
    {
      _name = name;
    }
  }
}
