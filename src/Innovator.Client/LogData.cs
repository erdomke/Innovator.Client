using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client
{
  internal class LogData : IEnumerable<KeyValuePair<string, object>>, IDisposable
  {
    private List<KeyValuePair<string, object>> _values;
    private Stopwatch _st = Stopwatch.StartNew();
    private Action<int, string, IEnumerable<KeyValuePair<string, object>>> _logListener;

    public int Level { get; set; }
    public string Name { get; set; }

    public LogData(int level, string name, Action<int, string, IEnumerable<KeyValuePair<string, object>>> logListener)
    {
      Level = level;
      Name = name;
      _logListener = logListener ?? Factory.LogListener;
      _values = new List<KeyValuePair<string, object>>();
      Add("start_utc", DateTime.UtcNow);
    }
    public LogData(int level, string name, Action<int, string, IEnumerable<KeyValuePair<string, object>>> logListener, IEnumerable<KeyValuePair<string, object>> values)
    {
      Level = level;
      Name = name;
      _logListener = logListener ?? Factory.LogListener;
      _values = values.ToList();
      Add("start_utc", DateTime.UtcNow);
    }

    public void Add(string name, object value)
    {
      _values.Add(new KeyValuePair<string, object>(name, value));
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
      return _values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public void Dispose()
    {
      if (_st.IsRunning)
      {
        _st.Stop();
        _values.Add(new KeyValuePair<string, object>("duration", _st.Elapsed));
        _logListener(this.Level, this.Name, _values);
      }
    }
  }
}
