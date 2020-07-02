using System;
using Newtonsoft.Json;

namespace SearchAThing
{

    public class MeasureUnitJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        static Type doubleTypeof = typeof(double);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {           
            var q = Measure.TryParse($"1 {(string)reader.Value}");
            if (q == null)
                return existingValue;
            else
                return q.MU;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var mu = (MeasureUnit)value;

            writer.WriteValue(mu.ToString());
        }
    }

}