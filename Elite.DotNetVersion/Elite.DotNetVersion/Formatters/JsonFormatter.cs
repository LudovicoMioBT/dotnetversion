using DevLab.JmesPath;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elite.DotNetVersion.Formatters
{

    class JsonFormatter : IFormatter
    {
        public string Query { get; }

        public JsonFormatter(string query)
        {
            this.Query = query ?? string.Empty;
        }

        public void Dispose()
        { }

        public async Task WriteAsync(TextWriter writer, object content)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Converters = (new JsonConverter[]
                    {
                        new VersionConverter(),
                        new FileInfoConverter()
                    }).ToList()
                };

                var json = JsonConvert.SerializeObject(content, Formatting.Indented, settings);

                if (string.IsNullOrEmpty(this.Query))
                    await writer.WriteLineAsync(json);
                else
                {

                    var jms = new JmesPath();
                    var result = jms.Transform(json, this.Query);
                    var tmp = JsonConvert.DeserializeObject(result);
                    await writer.WriteAsync(JsonConvert.SerializeObject(tmp, Formatting.Indented, settings));
                }
            }
            catch (Exception ex)
            {
                await writer.WriteLineAsync($"Error: {ex.Message}");
            }
        }

        class FileInfoConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(FileInfo);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var value = reader.ReadAsString();
                return new FileInfo(value);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(((FileInfo)value).FullName);
            }
        }
    }
}
