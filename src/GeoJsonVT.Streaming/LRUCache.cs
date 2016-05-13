using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SInnovations.VectorTiles.GeoJsonVT.Streaming
{
   
    
    public class LRUCacheOptions
    {

        public long Capacity { get; set; } = 50000;
    }
    public class LRUCache<K, V>
    {
        private long capacity;
        private Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>> cacheMap = new Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>>();
        private LinkedList<LRUCacheItem<K, V>> lruList = new LinkedList<LRUCacheItem<K, V>>();

        public LRUCache(LRUCacheOptions options = null)
        {
            options = options ?? new LRUCacheOptions();
            this.capacity = options.Capacity;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public V Get(K key)
        {
            LinkedListNode<LRUCacheItem<K, V>> node;
            if (cacheMap.TryGetValue(key, out node))
            {
                V value = node.Value.Value;
                lruList.Remove(node);
                lruList.AddLast(node);
                return value;
            }
            return default(V);
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public V Add(K key, V val)
        {
            if (cacheMap.Count >= capacity)
            {
                return RemoveFirst();
            }

            LRUCacheItem<K, V> cacheItem = new LRUCacheItem<K, V>(key, val);
            LinkedListNode<LRUCacheItem<K, V>> node = new LinkedListNode<LRUCacheItem<K, V>>(cacheItem);
            lruList.AddLast(node);
            cacheMap.Add(key, node);
            return default(V);
        }

        private V RemoveFirst()
        {
            // Remove from LRUPriority
            LinkedListNode<LRUCacheItem<K, V>> node = lruList.First;
            lruList.RemoveFirst();

            // Remove from cache
            cacheMap.Remove(node.Value.Key);

            return node.Value.Value;
        }
    }

    public class LRUCacheItem<K, V>
    {
        public LRUCacheItem(K k, V v)
        {
            Key = k;
            Value = v;
        }
        public K Key { get; set; }
        public V Value { get; set; }
    }
}
