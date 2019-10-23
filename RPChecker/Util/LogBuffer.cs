using System.Collections.Generic;
using System.Text;

namespace RPChecker.Util
{
    public class LogBuffer
    {
        private readonly LinkedList<string> _content;

        private readonly int _count;

        public LogBuffer()
        {
        }

        public LogBuffer(int count = 1024)
        {
            _content = new LinkedList<string>();
            _count = count;
        }

        public bool Inf { get; set; }

        public void Log(string line)
        {
            lock (_content)
            {
                _content.AddLast(line);
                if (!Inf && _content.Count > _count)
                {
                    _content.RemoveFirst();
                }
            }
        }

        public string Peek()
        {
            return _content.First?.Value;
        }

        public string Last()
        {
            return _content.Last?.Value;
        }

        public bool IsEmpty()
        {
            return _content.Count == 0;
        }

        public override string ToString()
        {
            lock (_content)
            {
                var ret = new StringBuilder();
                foreach (var node in _content)
                {
                    ret.AppendLine(node);
                }
                return ret.ToString();
            }
        }
    }
}