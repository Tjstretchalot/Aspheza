using System;
using System.Collections.Generic;

namespace BaseBuilder.Engine.Utility
{
    public class BiDictionary<TFirst, TSecond>
    {
        private IDictionary<TFirst, TSecond> FirstToSecond;
        private IDictionary<TSecond, TFirst> SecondToFirst;

        public BiDictionary()
        {
            FirstToSecond = new Dictionary<TFirst, TSecond>();
            SecondToFirst = new Dictionary<TSecond, TFirst>();
        }

        public void Add(TFirst first, TSecond second)
        {
            FirstToSecond.Add(first, second);
            SecondToFirst.Add(second, first);
        }

        public bool TryGetValue(TFirst first, out TSecond second)
        {
            return FirstToSecond.TryGetValue(first, out second);
        }

        public bool TryGetValue(TSecond second, out TFirst first)
        {
            return SecondToFirst.TryGetValue(second, out first);
        }

        public bool TryGetByFirst(TFirst first, out TSecond second)
        {
            return FirstToSecond.TryGetValue(first, out second);
        }

        public bool TryGetBySecond(TSecond second, out TFirst first)
        {
            return SecondToFirst.TryGetValue(second, out first);
        }

        public TSecond GetByFirst(TFirst first)
        {
            return FirstToSecond[first];
        }

        public TFirst GetBySecond(TSecond second)
        {
            return SecondToFirst[second];
        }

        public TSecond this[TFirst first]
        {
            get { return GetByFirst(first);  }
            set { Add(first, value); }
        }

        public TFirst this[TSecond second]
        {
            get { return GetBySecond(second); }
            set { Add(value, second); }
        }
    }
}
