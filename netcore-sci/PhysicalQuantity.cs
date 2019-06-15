using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using SearchAThing.Util;

namespace SearchAThing.Sci
{

    public enum MeasureUnitConversionTypeEnum
    {
        Linear,
        NonLinear
    };
    
    public class PhysicalQuantity : IEquatable<PhysicalQuantity>
    {

        static int global_static_id_counter;
                
        internal int id;

        public MeasureUnitConversionTypeEnum MUConversionType { get; private set; }

        /// <summary>
        /// conversion factor to the ref unit
        /// </summary>                
        List<double> linearConvFactors = new List<double>();

        double[,] conversionMatrix = null;
                
        public MeasureUnit LinearConversionRefMU { get; private set; }

        Dictionary<string, MeasureUnit> _dict_mu;
        Dictionary<string, MeasureUnit> dict_mu
        {
            get
            {
                if (_dict_mu == null) _dict_mu = MeasureUnits.ToDictionary(k => k.Name, v => v);
                return _dict_mu;
            }
        }
        public MeasureUnit ByName(string mu_name)
        {
            MeasureUnit mu = null;

            dict_mu.TryGetValue(mu_name, out mu);

            return mu;
        }

        /// <summary>
        /// Convert between nonlinear measure units
        /// </summary>
        internal Func<MeasureUnit, MeasureUnit, double, double> NonLinearConversionFunctor { get; private set; }
                
        List<MeasureUnit> measureUnits;
        public IEnumerable<MeasureUnit> MeasureUnits
        {
            get
            {
                if (measureUnitsContainerType != null)
                {
                    var t = measureUnitsContainerType;
                    measureUnitsContainerType = null;

                    var muType = typeof(MeasureUnit);
                    var mus = t.GetFields(BindingFlags.Public | BindingFlags.Static).Where(r => r.FieldType == muType).ToList();

                    foreach (var mu in mus) mu.GetValue(null); // wakeup measure unit field
                }

                return measureUnits;
            }
        }
                
        public string Name { get; private set; }

        Type measureUnitsContainerType;

        public PhysicalQuantity(string name, Type _measureUnitsContainerType = null, MeasureUnitConversionTypeEnum muConversionType = MeasureUnitConversionTypeEnum.Linear)
        {
            id = global_static_id_counter++;

            MUConversionType = muConversionType;

            Name = name;

            measureUnitsContainerType = _measureUnitsContainerType;

            measureUnits = new List<MeasureUnit>();
        }

        internal void RegisterMeasureUnit(MeasureUnit mu, MeasureUnit convRefUnit = null, double convRefFactor = 0)
        {
            if (MUConversionType == MeasureUnitConversionTypeEnum.NonLinear)
                throw new Exception($"MeasureUnit [{mu.Name}] need a non linear conversion rule");

            if (measureUnits.Any(w => w.Name == mu.Name))
                throw new Exception($"MeasureUnit [{mu.Name}] already registered");

            if (LinearConversionRefMU == null)
            {
                LinearConversionRefMU = mu;
                linearConvFactors.Add(1.0); // ref unit to itself
                if (mu.id != 0) throw new Exception("internal error");
            }
            else
            {
                linearConvFactors.Add(linearConvFactors[convRefUnit.id] * convRefFactor);
            }

            measureUnits.Add(mu);
            conversionMatrix = null;
        }

        internal void RegisterMeasureUnit(MeasureUnit mu, Func<MeasureUnit, MeasureUnit, double, double> convRefFunctor)
        {
            if (MUConversionType == MeasureUnitConversionTypeEnum.Linear)
                throw new Exception($"MeasureUnit [{mu.Name}] need a linear conversion factor");

            if (measureUnits.Any(w => w.Name == mu.Name))
                throw new Exception($"MeasureUnit [{mu.Name}] already registered");

            measureUnits.Add(mu);
            conversionMatrix = null;
            NonLinearConversionFunctor = convRefFunctor;
        }

        /// <summary>
        /// convert between linear measure units
        /// </summary>        
        public double ConvertFactor(MeasureUnit from, MeasureUnit to)
        {
            if (from.PhysicalQuantity.id != this.id || from.PhysicalQuantity.id != to.PhysicalQuantity.id)
                throw new Exception($"MeasureUnit physical quantity doesn't match");

            if (MUConversionType == MeasureUnitConversionTypeEnum.NonLinear)
                throw new Exception($"invalid usage of convert factor for non linear mu");

            if (conversionMatrix == null) RebuildConversionMatrix();

            return conversionMatrix[from.id, to.id];
        }

        private void RebuildNonlinearConversionMatrix()
        {

        }

        private void RebuildConversionMatrix()
        {
            var mus = measureUnits.Where(r => r.PhysicalQuantity.id == id).ToList();

            conversionMatrix = new double[mus.Count, mus.Count];
            var m = conversionMatrix;

            // https://searchathing.com/?p=1326#MeasureUnitConversionMatrixStructure

            // fill first column
            for (int r = 1; r < mus.Count; ++r) m[r, 0] = linearConvFactors[r];

            // fill diag
            for (int r = 0; r < mus.Count; ++r) m[r, r] = 1;

            // fill lower triangle
            for (int c = 1; c < mus.Count - 1; ++c)
            {
                for (int r = c; r < mus.Count; ++r)
                {
                    m[r, c] = m[r, 0] / m[c, 0];
                }
            }

            // fill upper triangle
            for (int c = 1; c < linearConvFactors.Count; ++c)
            {
                for (int r = 0; r < c; ++r)
                {
                    m[r, c] = 1.0 / m[c, r];
                }
            }
        }

        public bool Equals(PhysicalQuantity other)
        {
            return id == other.id;
        }

        public override string ToString()
        {
            return Name;
        }
    };


}