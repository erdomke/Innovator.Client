using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Innovator.Client
{
  public interface ILink<T> where T : ILink<T>
  {
    string Name { get; }
    T Next { get; set; }
  }
}
