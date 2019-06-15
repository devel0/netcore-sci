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

    public class MeasureJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        static Type doubleTypeof = typeof(double);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Measure res = null;

            if (reader.Value.GetType() == doubleTypeof)
                res = new Measure((double)reader.Value, MUCollection.Adimensional.adim);
            else
                res = Measure.TryParse((string)reader.Value);
            if (res == null) return existingValue;

            return res;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var measure = (Measure)value;

            if (measure.MU == MUCollection.Adimensional.adim)
                writer.WriteValue(measure.Value);
            else
                writer.WriteValue(measure.ToString(includePQ: true));
        }
    }

}