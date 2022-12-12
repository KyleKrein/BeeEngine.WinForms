﻿namespace BeeEngine
{
    public static class ForeachExtensions
    {
        public static CustomIntEnumerator GetEnumerator(this Range range)
        {
            return new CustomIntEnumerator(range);
        }
        public static CustomIntEnumerator GetEnumerator(this int endnumber)
        {
            return new CustomIntEnumerator(new Range(0, endnumber));
        }
    }

    public ref struct CustomIntEnumerator
    {
        private int _current;
        private readonly int _end;

        public CustomIntEnumerator(Range range)
        {
            _current = range.Start.Value - 1;
            _end = range.End.Value;
        }

        public int Current => _current;
        public bool MoveNext()
        {
            _current++;
            return _current < _end;
        }

    }
}