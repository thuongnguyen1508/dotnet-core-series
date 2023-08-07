using Newtonsoft.Json;
using Series.KeyValueDatabase.Enums;
using Series.KeyValueDatabase.Models;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Series.KeyValueDatabase.Extensions
{
    public static class RedisExtension
    {
        private static readonly DateTime _scoreTime = new DateTime(2016, 1, 1);

        private static readonly ConcurrentDictionary<string, byte[]> luaScripts = new ConcurrentDictionary<string, byte[]>();

        private static readonly ConcurrentDictionary<string, string[]> fields = new ConcurrentDictionary<string, string[]>();

        private static readonly Encoding encoding = Encoding.UTF8;

        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        };

        public static object Deserialize(this ISerializer serializer, byte[] serializedObject, Type t)
        {
            return JsonConvert.DeserializeObject(encoding.GetString(serializedObject), t, settings);
        }

        public static double GetScore()
        {
            return (DateTime.Now - _scoreTime).TotalSeconds;
        }

        public static async Task<LoadedLuaScript> ScriptLoadAsync(this IRedisDatabase cache, string script, EndPoint endPoint = null)
        {
            if (endPoint == null)
            {
                endPoint = cache.Database.Multiplexer.GetEndPoints().FirstOrDefault();
            }

            IServer server = cache.Database.Multiplexer.GetServer(endPoint);
            return await LuaScript.Prepare(script).LoadAsync(server);
        }

        public static List<T> HashToModel<T>(this RedisResult[] results, ISerializer serializer, params string[] fields)
        {
            List<T> list = new List<T>();
            for (int i = 0; i < results.Length; i++)
            {
                RedisResult[] array = (RedisResult[])results[i];
                PropertyInfo[] properties = typeof(T).GetProperties();
                object obj = Activator.CreateInstance(typeof(T));
                for (int j = 0; j < fields.Length; j++)
                {
                    if (!array[j].IsNull)
                    {
                        string field = fields[j];
                        PropertyInfo propertyInfo = properties.FirstOrDefault((PropertyInfo c) => c.Name == field);
                        if (!(propertyInfo == null))
                        {
                            propertyInfo.SetValue(obj, Convert.ChangeType(serializer.Deserialize((byte[])array[j], propertyInfo.PropertyType), propertyInfo.PropertyType));
                        }
                    }
                }

                list.Add((T)obj);
            }

            return list;
        }

        public static string[] GetFields<T>()
        {
            Type typeFromHandle = typeof(T);
            if (!fields.TryGetValue(typeFromHandle.FullName, out var value))
            {
                PropertyInfo[] properties = typeFromHandle.GetProperties();
                value = (fields[typeFromHandle.FullName] = (from c in properties
                                                            where c.CanRead && c.CanWrite && c.PropertyType.IsPublic
                                                            select c into prop
                                                            select prop.Name).ToArray());
            }

            return value;
        }

        public static T HashToModel<T>(this HashEntry[] results, ISerializer serializer, params string[] fields)
        {
            new List<T>();
            PropertyInfo[] properties = typeof(T).GetProperties();
            T val = (T)Activator.CreateInstance(typeof(T));
            for (int j = 0; j < results.Length; j++)
            {
                HashEntry hashEntry = results[j];
                int i;
                for (i = 0; i < fields.Length; i++)
                {
                    if (!hashEntry.Value.IsNull)
                    {
                        PropertyInfo propertyInfo = properties.FirstOrDefault((PropertyInfo c) => c.Name == fields[i]);
                        if (!(propertyInfo == null) && !((RedisValue)propertyInfo.Name != hashEntry.Name))
                        {
                            propertyInfo.SetValue(val, Convert.ChangeType(serializer.Deserialize(hashEntry.Value, propertyInfo.PropertyType), propertyInfo.PropertyType));
                        }
                    }
                }
            }

            return val;
        }

        public static async Task<bool> AddAsync<T>(this IRedisDatabase cache, string key, T value, double minutes)
        {
            return await cache.AddAsync(key, value, DateTimeOffset.UtcNow.AddMinutes(minutes));
        }

        public static async Task<T> GetAsync<T>(this IRedisDatabase cache, long key, Func<Task<T>> getData)
        {
            T val = await cache.GetAsync<T>(key.ToString());
            if (val == null)
            {
                val = await getData();
            }

            return val;
        }

        public static async Task<T> GetAsync<T>(this IRedisDatabase cache, string key, Func<Task<T>> getData)
        {
            T val = await cache.GetAsync<T>(key);
            if (val == null)
            {
                val = await getData();
            }

            return val;
        }

        public static async Task<T> GetOrAddAsync<T>(this IRedisDatabase cache, string key, Func<Task<T>> getData, double minutes)
        {
            DateTimeOffset? expiresAt = null;
            if (minutes > 0.0)
            {
                expiresAt = DateTimeOffset.UtcNow.AddMinutes(minutes);
            }

            return await cache.GetOrAddAsync(key, getData, expiresAt);
        }

        public static async Task<T> GetOrAddAsync<T>(this IRedisDatabase cache, long key, Func<Task<T>> getData, CacheExpiration expiration)
        {
            return await cache.GetOrAddAsync(key.ToString(), getData, DateTimeOffset.UtcNow.AddMinutes((double)expiration));
        }

        public static async Task<T> GetOrAddAsync<T>(this IRedisDatabase cache, string key, Func<Task<T>> getData, CacheExpiration expiration)
        {
            return await cache.GetOrAddAsync(key, getData, DateTimeOffset.UtcNow.AddMinutes((double)expiration));
        }

        public static async Task<T> GetOrAddAsync<T>(this IRedisDatabase cache, string key, Func<Task<T>> getData, DateTimeOffset? expiresAt = null)
        {
            T resp = await cache.GetAsync<T>(key);
            if (resp == null)
            {
                resp = await getData();
                if (resp != null)
                {
                    if (!expiresAt.HasValue)
                    {
                        await cache.AddAsync(key, resp);
                    }
                    else
                    {
                        await cache.AddAsync(key, resp, expiresAt.Value);
                    }
                }
            }

            return resp;
        }

        public static async Task<T> HashGetOrAddAsync<T>(this IRedisDatabase cache, string hashKey, long key, Func<Task<T>> getData)
        {
            return await cache.HashGetOrAddAsync(hashKey, key.ToString(), getData);
        }

        public static async Task<T> HashGetOrAddAsync<T>(this IRedisDatabase cache, string hashKey, string key, Func<Task<T>> getData)
        {
            T resp = await cache.HashGetAsync<T>(hashKey, key);
            if (resp == null)
            {
                resp = await getData();
                if (resp != null)
                {
                    await cache.HashSetAsync(hashKey, key, resp);
                }
            }

            return resp;
        }

        public static async Task<T> HashGetAsync<T>(this IRedisDatabase cache, string hashKey, long key, Func<Task<T>> getData)
        {
            return await cache.HashGetAsync(hashKey, key.ToString(), getData);
        }

        public static async Task<T> HashGetAsync<T>(this IRedisDatabase cache, string hashKey, string key, Func<Task<T>> getData)
        {
            T val = await cache.HashGetAsync<T>(hashKey, key);
            if (val == null)
            {
                val = await getData();
            }

            return val;
        }

        public static async Task<List<string>> ListRangeAsync(this IRedisDatabase cache, string key, int start, int end)
        {
            List<string> res = new List<string>();
            RedisValue[] array = await cache.Database.ListRangeAsync(key, start, end);
            foreach (string item in array)
            {
                res.Add(item);
            }

            return res;
        }

        public static async Task<List<T>> ListRangeAsync<T>(this IRedisDatabase cache, string key, int start, int stop)
        {
            List<T> res = new List<T>();
            RedisValue[] array = await cache.Database.ListRangeAsync(key, start, stop);
            foreach (RedisValue redisValue in array)
            {
                res.Add(cache.Serializer.Deserialize<T>(redisValue));
            }

            return res;
        }

        public static async Task<List<string>> SortedSetRangeByRankAsync(this IRedisDatabase cache, string key, int start, int stop, bool ascending = true)
        {
            Order order = ((!ascending) ? Order.Descending : Order.Ascending);
            return (await cache.Database.SortedSetRangeByRankAsync(key, start, stop, order)).ToStringArray().ToList();
        }

        public static async Task<List<string>> SortedSetRangeByScoreAsync(this IRedisDatabase cache, string key, int start, int stop, bool ascending = true)
        {
            Order order = ((!ascending) ? Order.Descending : Order.Ascending);
            return (await cache.Database.SortedSetRangeByScoreAsync(key, 0.0, double.MaxValue, Exclude.None, order, start, stop)).ToStringArray().ToList();
        }

        public static async Task<bool> SortedSetAddAsync(this IRedisDatabase cache, string key, string member, double score = 0.0)
        {
            if (score == 0.0)
            {
                score = GetScore();
            }

            return await cache.Database.SortedSetAddAsync(key, member, score);
        }

        public static async Task<long> SortedSetAddAsync(this IRedisDatabase cache, string key, Dictionary<string, double> entries)
        {
            int num = 0;
            double score = GetScore();
            SortedSetEntry[] array = new SortedSetEntry[entries.Count];
            foreach (KeyValuePair<string, double> entry in entries)
            {
                array[num] = new SortedSetEntry(entry.Key, (entry.Value == 0.0) ? score : entry.Value);
                num++;
            }

            return await cache.Database.SortedSetAddAsync(key, array);
        }

        public static async Task<SortedSetRangeDetailResult<T>> SortedSetRangeDetailAsync<T>(this IRedisDatabase cache, string key, string prefix, int start, int stop)
        {
            List<RedisKey> keys = new List<RedisKey> { key, prefix };
            RedisValue[] obj = new RedisValue[2]
            {
                start,
                default(RedisValue)
            };
            int num = stop - 1;
            stop = num;
            obj[1] = num;
            RedisValue[] values = obj;
            string scriptKey = "SortedSetRangeDetailAsync";
            if (!luaScripts.TryGetValue(scriptKey, out var value))
            {
                string script = "\r\n                local array = {}\r\n                local k = ''\r\n                local members = redis.call('ZREVRANGE', KEYS[1], ARGV[1], ARGV[2])\r\n                array[1] = members;\r\n                for _,m in ipairs(members) do\r\n                    k = KEYS[2] .. m\r\n                    array[#array + 1] = redis.call('get', k)\r\n                end\r\n                return array\r\n                ";
                LoadedLuaScript loadedLuaScript = await cache.ScriptLoadAsync(script);
                value = (luaScripts[scriptKey] = loadedLuaScript.Hash);
            }

            RedisResult[] array = (RedisResult[])(await cache.Database.ScriptEvaluateAsync(value, keys.ToArray(), values));
            SortedSetRangeDetailResult<T> sortedSetRangeDetailResult = new SortedSetRangeDetailResult<T>();
            if (array == null || array.Length == 0)
            {
                return sortedSetRangeDetailResult;
            }

            bool flag = true;
            RedisResult[] array2 = array;
            foreach (RedisResult redisResult in array2)
            {
                if (flag)
                {
                    flag = false;
                    sortedSetRangeDetailResult.Keys = ((RedisValue[])redisResult).ToStringArray();
                }
                else if (redisResult.IsNull)
                {
                    sortedSetRangeDetailResult.Items.Add(default(T));
                }
                else
                {
                    sortedSetRangeDetailResult.Items.Add(cache.Serializer.Deserialize<T>((byte[])redisResult));
                }
            }

            return sortedSetRangeDetailResult;
        }

        public static async Task RemoveAllKeysAsync(this IRedisDatabase cache, IEnumerable<string> keys)
        {
            await Task.WhenAll(keys.Select(async (string key) => await cache.RemoveAsync(key)));
        }

        public static async Task<bool> ValidLockAsync(this IRedisDatabase cache, string redisKey, string requestId, int id, int minutes = 5)
        {
            return (await cache.LockAsync(redisKey, requestId, id, null, minutes)).IsValid(requestId);
        }

        public static async Task<LockInfo> LockAsync(this IRedisDatabase cache, string key, string requestId, int id, string description, int minutes = 5)
        {
            LockInfo item = await cache.GetAsync<LockInfo>(key);
            if (item == null)
            {
                item = new LockInfo
                {
                    RequestId = requestId,
                    Id = id,
                    Description = description,
                    ExpireAt = DateTime.Now.AddMinutes(minutes)
                };
                await cache.AddAsync(key, item, DateTimeOffset.Now.AddMinutes(minutes));
            }

            return item;
        }

        public static async Task UnLockAsync(this IRedisDatabase cache, string key, string requestId, bool force = false)
        {
            if (force)
            {
                await cache.RemoveAsync(key);
                return;
            }

            LockInfo lockInfo = await cache.GetAsync<LockInfo>(key);
            if (lockInfo != null && lockInfo.IsValid(requestId))
            {
                await cache.RemoveAsync(key);
            }
        }
    }
}

