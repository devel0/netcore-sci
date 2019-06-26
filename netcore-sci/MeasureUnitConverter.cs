using SearchAThing.Util;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System;
using static System.FormattableString;
using static System.Math;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SearchAThing.Sci
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