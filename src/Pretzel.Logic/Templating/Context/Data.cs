using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

using Pretzel.Logic.Templating.Context.DataParsing;

namespace Pretzel.Logic.Templating.Context
{
    public class Data : IDictionary<string, object>
    {
        private readonly IFileSystem fileSystem;
        private readonly string dataDirectory;
        private readonly Dictionary<string, Lazy<object>> cachedResults = new Dictionary<string, Lazy<object>>();
        private readonly IList<IDataParser> dataParsers;
        private readonly IDictionary<string, object> values = new Dictionary<string, object>();

        public Data(IFileSystem fileSystem, string dataDirectory)
        {
            this.fileSystem = fileSystem;
            this.dataDirectory = dataDirectory;
            dataParsers = new List<IDataParser>
            {
                new YamlJsonDataParser(fileSystem, "yml"),
                new YamlJsonDataParser(fileSystem, "json"),
                new CsvTsvDataParser(fileSystem, "csv"),
                new CsvTsvDataParser(fileSystem, "tsv", "\t")
            };
            Parse();
        }

        public object this[string key]
            { get => values[key]; set => values[key] = value; }

        public ICollection<string> Keys => values.Keys;

        public ICollection<object> Values => values.Values;

        public int Count => values.Count;

        public bool IsReadOnly => values.IsReadOnly;

        public void Add(string key, object value)
        {
            values.Add(key, value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            values.Add(item);
        }

        public void Clear()
        {
            values.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return values.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return values.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            values.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return values.Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return values.Remove(item);
        }

        public bool TryGetValue(string key, out object value)
        {
            return values.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)values).GetEnumerator();
        }

        private void Parse()
        {
            if (!fileSystem.Directory.Exists(dataDirectory))
            {
                return;
            }
            var files = fileSystem.Directory.EnumerateFiles(dataDirectory);
            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                foreach (var dataParser in dataParsers)
                {
                    if (dataParser.CanParse(dataDirectory, name))
                    {
                        values[name] = dataParser.Parse(dataDirectory, name);
                    }
                }
            }

            foreach (var subFolder in fileSystem.Directory.EnumerateDirectories(dataDirectory))
            {
                if (fileSystem.Directory.Exists(subFolder))
                {
                    var name = Path.GetFileNameWithoutExtension(subFolder);
                    values[name] = new Data(fileSystem, subFolder);
                }
            }

        }
    }

}
