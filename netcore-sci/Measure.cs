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

namespace SearchAThing.Sci
{

    [JsonConverter(typeof(MeasureJsonConverter))]
    public class Measure
    {

        public double Value { get; private set; }

        [JsonIgnore]
        public MeasureUnit MU { get; private set; }

        public Measure(double value, MeasureUnit mu)
        {
            Value = value;
            MU = mu;
        }

        /// <summary>
        /// use of exponential pref
        /// eg. 
        /// 120 with ExpPref=2 -> 1.2e2
        /// 120 with ExpPref=-1 -> 1200e-1
        /// </summary>
        public int? ExpPref { get; set; }

        #region operators

        /// <summary>
        /// scalar mul
        /// </summary>        
        public static Measure operator *(double s, Measure v)
        {
            return new Measure(v.Value * s, v.MU);
        }

        /// <summary>
        /// scalar mul
        /// </summary>        
        public static Measure operator *(Measure v, double s)
        {
            return new Measure(v.Value * s, v.MU);
        }

        /// <summary>
        /// scalar mul
        /// </summary>        
        public static Measure operator /(Measure v, double s)
        {
            return new Measure(v.Value / s, v.MU);
        }

        /// <summary>
        /// scalar mul
        /// </summary>        
        public static Measure operator -(Measure a)
        {
            return -1 * a;
        }

        #endregion

        public static implicit operator Measure(string str)
        {
            return TryParse(str);
        }

        /// <summary>
        /// Convert to the implicit measure of the given mu domain
        /// </summary>
        public Measure ConvertTo(IMUDomain mud)
        {
            if (MU == MUCollection.Adimensional.adim) return new Measure(Value, MU);

            return ConvertTo(mud.ByPhysicalQuantity(MU.PhysicalQuantity).MU);
        }

        public Measure ConvertTo(MeasureUnit toMU)
        {
            return new Measure(Convert(Value, MU, toMU), toMU);
        }

        /// <summary>
        /// convert given value from to measure units
        /// </summary>        
        public static double Convert(double value, MeasureUnit from, MeasureUnit to)
        {
            if (from.PhysicalQuantity.MUConversionType == MeasureUnitConversionTypeEnum.NonLinear)
                return from.PhysicalQuantity.NonLinearConversionFunctor(from, to, value);
            else
                return from.PhysicalQuantity.ConvertFactor(from, to) * value;
        }

        /// <summary>
        /// convert given value from to measure units
        /// to measure unit is given from the correspondent physical quantity measure unit in the given domain
        /// </summary>        
        public static double Convert(double value, MeasureUnit from, IMUDomain to)
        {
            if (from == MUCollection.Adimensional.adim) return value;

            return value.Convert(from, to.ByPhysicalQuantity(from.PhysicalQuantity).MU);
        }

        /// <summary>
        /// convert given value from to measure units
        /// from measure unit is given from the correspondent physical quantity measure unit in the given domain
        /// </summary>        
        public static double Convert(double value, IMUDomain from, MeasureUnit to)
        {
            if (from == MUCollection.Adimensional.adim) return value;

            return value.Convert(from.ByPhysicalQuantity(to.PhysicalQuantity).MU, to);
        }

        /// <summary>
        /// if specify culture use the given on or Invariant if not specified
        /// </summary>        
        public string ToString(bool includePQ = false, int? digits = null, CultureInfo culture = null)
        {
            var res = "";

            var mustr = "";
            if (MU != MUCollection.Adimensional.adim)
                mustr = MU.ToString();

            if (!ExpPref.HasValue || ExpPref.Value == 0)
            {
                var v = Value;
                if (digits.HasValue) v = Round(v, digits.Value);
                FormattableString fmt = $"{v} {(mustr.Length > 0 ? mustr : "")}";
                if (culture == null)
                    res = Invariant(fmt);
                else
                    res = fmt.ToString();
            }
            else
            {
                var v = Value / Pow(10, ExpPref.Value);
                if (digits.HasValue) v = Round(v, digits.Value);
                FormattableString fmt = $"{v}e{ExpPref.Value} {(mustr.Length > 0 ? mustr : "")}";
                if (culture == null)
                    res = Invariant(fmt);
                else
                    res = fmt.ToString();
            }

            if (includePQ) res += $" [{MU.PhysicalQuantity}]";

            return res.Trim();
        }

        public override string ToString()
        {
            return this.ToString(includePQ: false);
        }

        public string ToString(int digits)
        {
            return this.ToString(includePQ: false, digits: digits);
        }

        /// <summary>
        /// if specify culture use the given on or Invariant if not specified
        /// </summary>        
        public static Measure TryParse(string text, PhysicalQuantity pq = null, CultureInfo culture = null)
        {
            if (pq == null)
            {
                var pqstart = text.LastIndexOf('[') + 1;
                if (pqstart != 0)
                {
                    var pqname = text.Substring(pqstart, text.Length - pqstart - 1);
                    pq = PQCollection.PhysicalQuantities.First(w => w.Name == pqname);

                    text = text.Substring(0, pqstart - 1);
                }
            }

            if (pq != null && pq.Equals(PQCollection.Adimensional))
            {
                double n;
                if (double.TryParse(text, NumberStyles.Number | NumberStyles.AllowExponent, culture != null ? culture : CultureInfo.InvariantCulture, out n))
                {
                    var res = new Measure(n, MUCollection.Adimensional.adim);
                    var regex = new Regex("([0-9.]*)([eE])(.*)");
                    var q = regex.Match(text);
                    if (q.Success)
                        res.ExpPref = int.Parse(q.Groups[3].Value);

                    return res;
                }
            }
            else
            {
                var s = text.Trim();

                MeasureUnit mu = null;

                if (pq == null)
                {
                    var all_mus = MUCollection.MeasureUnits;

                    foreach (var _mu in all_mus.OrderByDescending(w => w.Name.Length))
                    {
                        if (s.EndsWith(_mu.ToString()))
                        {
                            mu = _mu;
                            break;
                        }
                    }

                    if (mu == null) return null;

                    // ambiguity between different pq with same mu name
                    if (all_mus.Count(r => r.ToString() == mu.ToString()) != 1) return null;
                }
                else
                {
                    foreach (var _mu in pq.MeasureUnits.OrderByDescending(w => w.Name.Length))
                    {
                        if (s.EndsWith(_mu.ToString()))
                        {
                            mu = _mu;
                            break;
                        }
                    }
                }

                if (mu == null) return null;

                s = s.StripEnd(mu.ToString());

                double n;
                if (double.TryParse(s, NumberStyles.Number | NumberStyles.AllowExponent, culture != null ? culture : CultureInfo.InvariantCulture, out n))
                {
                    var res = new Measure(n, mu);

                    var regex = new Regex("([0-9.]*)([eE])(.*)");
                    var q = regex.Match(s);
                    if (q.Success)
                    {
                        res.ExpPref = int.Parse(q.Groups[3].Value);
                    }

                    return res;
                }
            }

            return null;
        }

        /// <summary>
        /// return this measure rounded by the given tol
        /// this will not change current MU
        /// </summary>        
        public Measure MRound(Measure tol)
        {
            return Value.MRound(tol.ConvertTo(MU).Value) * MU;
        }

    }

}
