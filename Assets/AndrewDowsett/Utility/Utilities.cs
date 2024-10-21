using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace AndrewDowsett.Utility
{
    public static class Utilities
    {
        /// <summary>
        /// Opens a link in the devices default browser.
        /// </summary>
        /// <param name="url">URL to open.</param>
        public static void OpenLink(string url)
        {
#if !UNITY_EDITOR && UNITY_WEBGL
        //string sanitizedUrl = Uri.EscapeUriString(url);
        OpenTab(url);
#endif
        }
        [DllImport("__Internal")]
        private static extern void OpenTab(string url);

        /// <summary>
        /// Get's a random number between min and max.
        /// </summary>
        /// <param name="min">min(inclusive)</param>
        /// <param name="max">max(exclusive)</param>
        /// <returns>Random int between two numbers.</returns>
        public static int GetRandomNumber(int minInclusive, int maxExclusive)
        {
            using (RNGCryptoServiceProvider rg = new RNGCryptoServiceProvider())
            {
                byte[] rno = new byte[3];
                rg.GetBytes(rno);
                return minInclusive + (BitConverter.ToUInt16(rno, 0) % (maxExclusive - minInclusive));
            }
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string result = string.Empty;
            for (int i = 0; i < length; i++)
            {
                result += chars[GetRandomNumber(0, chars.Length)];
            }
            return result;
        }
    }

    public class IntStringDictionary<T>
    {
        public Dictionary<int, T> intDictionary = new Dictionary<int, T>();
        public Dictionary<string, T> stringDictionary = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        Dictionary<int, string> intStringPairing = new Dictionary<int, string>();
        Dictionary<string, int> stringIntPairing = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public int Count { get { return intDictionary.Count; } }

        public void Add(int intValue, string stringValue, T value)
        {
            intDictionary.TryAdd(intValue, value);
            stringDictionary.TryAdd(stringValue, value);
            intStringPairing.TryAdd(intValue, stringValue);
            stringIntPairing.TryAdd(stringValue, intValue);
        }

        public void Remove(int key)
        {
            if (Find(key) == null) return;
            string stringKey = intStringPairing[key];
            intDictionary.Remove(key);
            stringDictionary.Remove(stringKey);
            intStringPairing.Remove(key);
            stringIntPairing.Remove(stringKey);
        }

        public void Remove(string key)
        {
            if (Find(key) == null) return;
            int intKey = stringIntPairing[key];
            intDictionary.Remove(intKey);
            stringDictionary.Remove(key);
            intStringPairing.Remove(intKey);
            stringIntPairing.Remove(key);
        }

        public bool ReplaceKey(string key, int newKey)
        {
            T find = Find(key);
            if (find == null) return false;
            Remove(key);
            Add(newKey, key, find);
            return true;
        }

        public bool ContainsKey(int key)
        {
            return Find(key) != null;
        }

        public bool ContainsKey(string key)
        {
            return Find(key) != null;
        }

        public T Find(int key)
        {
            T value;
            if (intDictionary.TryGetValue(key, out value))
            {
                return value;
            }
            return default(T);
        }

        public T Find(string key)
        {
            T value;
            if (stringDictionary.TryGetValue(key, out value))
            {
                return value;
            }
            return default(T);
        }

        public T this[int key]
        {
            get { return Find(key); }
        }

        public T this[string key]
        {
            get { return Find(key); }
        }

        public void Clear()
        {
            intDictionary.Clear();
            stringDictionary.Clear();
            intStringPairing.Clear();
            stringIntPairing.Clear();
        }
    }

    public class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> data;

        public PriorityQueue()
        {
            this.data = new List<T>();
        }

        public void Enqueue(T item)
        {
            data.Add(item);
            int ci = data.Count - 1; // child index; start at end
            while (ci > 0)
            {
                int pi = (ci - 1) / 2; // parent index
                if (data[ci].CompareTo(data[pi]) >= 0) break; // child item is larger than (or equal) parent so we're done
                T tmp = data[ci]; data[ci] = data[pi]; data[pi] = tmp;
                ci = pi;
            }
        }

        public T Dequeue()
        {
            // assumes pq is not empty; up to calling code
            int li = data.Count - 1; // last index (before removal)
            T frontItem = data[0];   // fetch the front
            data[0] = data[li];
            data.RemoveAt(li);

            --li; // last index (after removal)
            int pi = 0; // parent index. start at front of pq
            while (true)
            {
                int ci = pi * 2 + 1; // left child index of parent
                if (ci > li) break;  // no children so done
                int rc = ci + 1;     // right child
                if (rc <= li && data[rc].CompareTo(data[ci]) < 0) // if there is a rc (ci + 1), and it is smaller than left child, use the rc instead
                    ci = rc;
                if (data[pi].CompareTo(data[ci]) <= 0) break; // parent is smaller than (or equal to) smallest child so done
                T tmp = data[pi]; data[pi] = data[ci]; data[ci] = tmp; // swap parent and child
                pi = ci;
            }
            return frontItem;
        }

        public void RemoveElement(T element)
        {
            data.Remove(element);
        }

        public T Peek()
        {
            T frontItem = data[0];
            return frontItem;
        }

        public int Count()
        {
            return data.Count;
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < data.Count; ++i)
                s += data[i].ToString() + " ";
            s += "count = " + data.Count;
            return s;
        }

        public bool IsConsistent()
        {
            // is the heap property true for all data?
            if (data.Count == 0) return true;
            int li = data.Count - 1; // last index
            for (int pi = 0; pi < data.Count; ++pi) // each parent index
            {
                int lci = 2 * pi + 1; // left child index
                int rci = 2 * pi + 2; // right child index

                if (lci <= li && data[pi].CompareTo(data[lci]) > 0) return false; // if lc exists and it's greater than parent then bad.
                if (rci <= li && data[pi].CompareTo(data[rci]) > 0) return false; // check the right child too.
            }
            return true; // passed all checks
        } // IsConsistent
    } // PriorityQueue
}