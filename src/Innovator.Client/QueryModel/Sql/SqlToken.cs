using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel
{
  internal class SqlToken : SqlNode
  {
    public string Text { get; set; }

    public override string ToString()
    {
      return this.Text;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode() ^ (this.Text ?? "").GetHashCode();
    }

    public override bool Equals(SqlNode obj)
    {
      var literal = obj as SqlToken;
      if (literal == null) return false;
      return base.Equals(obj) && this.Text == literal.Text;
    }
  }
}
