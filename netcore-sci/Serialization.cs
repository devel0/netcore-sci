using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using UnitsNet.Serialization.JsonNet;
using Newtonsoft.Json.Converters;
using UnitsNet.Units;
using System;
using UnitsNet;
using JsonNet.ContractResolvers;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SearchAThing
{

    public static partial class SciToolkit
    {

        public static string ToSciJson(object o) => JsonConvert.SerializeObject(o, SciJsonSettings);

        public static JsonSerializerSettings SciJsonSettings
        {
            get
            {
                var res = new JsonSerializerSettings
                {
                    Error = errhandler,
                    TypeNameHandling = TypeNameHandling.All,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Formatting.Indented,
                    ContractResolver = new SciPropertiesResolver(),
                    Converters = {
                        new UnitsNetIQuantityJsonConverter(),
                        new StringEnumConverter(),

                        new UnitJsonConverter<LengthUnit>(),
                        new UnitJsonConverter<ForceUnit>(),
                        new UnitJsonConverter<PressureUnit>(),
                        new UnitJsonConverter<MassUnit>(),
                        new UnitJsonConverter<TemperatureUnit>(),
                        new UnitJsonConverter<EnergyUnit>(),
                        new UnitJsonConverter<AreaUnit>(),
                        new UnitJsonConverter<AreaMomentOfInertiaUnit>(),
                        new UnitJsonConverter<VolumeUnit>(),
                        new UnitJsonConverter<DensityUnit>(),
                    }
                };

                return res;
            }
        }

        private static void errhandler(object? sender, ErrorEventArgs e)
        {
            throw new Exception($"{e.ErrorContext.Error.Message} ; Path: {e.ErrorContext.Path}");
        }

        /// <summary>
        /// json converter to write unit using abbrev
        /// </summary>        
        public class UnitJsonConverter<TUnitType> : JsonConverter<TUnitType> where TUnitType : Enum
        {
            public override TUnitType? ReadJson(JsonReader reader, Type objectType, TUnitType? existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var s = (string?)reader.Value;

                if (s == null) return default(TUnitType);

                return UnitParser.Default.Parse<TUnitType>(s);
            }

            public override void WriteJson(JsonWriter writer, TUnitType? value, JsonSerializer serializer)
            {
                if (value != null)
                {
                    var res = UnitAbbreviationsCache.Default.GetDefaultAbbreviation(value);

                    writer.WriteValue(res);
                }
            }
        }

        /// <summary>
        /// addictional json resolver to ignore some Vector3D, Line3D properties during serialization without affecting source code
        /// </summary>
        public class SciPropertiesResolver : PrivateSetterContractResolver
        {
            static Type Vector3DType = typeof(Vector3D);
            static Type GeometryType = typeof(Geometry);

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var prop = base.CreateProperty(member, memberSerialization);

                prop.ShouldSerialize = _ => ShouldSerialize(member);

                return prop;
            }

            static bool ShouldSerialize(MemberInfo memberInfo)
            {
                var propertyInfo = memberInfo as PropertyInfo;
                if (propertyInfo == null) return false;

                if (propertyInfo.SetMethod != null) return true;

                var getMethod = propertyInfo.GetMethod;

                if (getMethod == null) return false;

                return Attribute.GetCustomAttribute(getMethod, typeof(CompilerGeneratedAttribute)) != null;
            }
        }


    }

}