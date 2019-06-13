using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static System.Math;

namespace SearchAThing.Sci
{

    public static class MUCollection
    {

        //-------------------------------------------------------------------
        // Enumerate Measure Units
        //-------------------------------------------------------------------

        static List<MeasureUnit> _MeasureUnits;
        /// <summary>
        /// through reflector returns all measure units along all physical quantities declared in MUCollection
        /// </summary>
        public static IEnumerable<MeasureUnit> MeasureUnits
        {
            get
            {
                if (_MeasureUnits == null)
                {
                    _MeasureUnits = new List<MeasureUnit>();

                    var nestedTypes = typeof(MUCollection).GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

                    foreach (var nestedType in nestedTypes)
                    {
                        var tNestedTypeTypes = nestedType.GetFields(BindingFlags.Public | BindingFlags.Static);

                        foreach (var tNestedTypeType in tNestedTypeTypes)
                        {
                            var mu = (MeasureUnit)tNestedTypeType.GetValue(null);

                            _MeasureUnits.Add(mu);
                        }
                    }
                }

                return _MeasureUnits;
            }
        }

        //-------------------------------------------------------------------
        // Measure Units
        //-------------------------------------------------------------------

        public static class Adimensional
        {
            public static readonly MeasureUnit adim = new MeasureUnit(PQCollection.Adimensional, "adim");
        }

        public static class Frequency
        {
            public static readonly MeasureUnit hz = new MeasureUnit(PQCollection.Frequency, "hz");
        }

        #region Length

        public static class Length
        {

            public static readonly MeasureUnit mm = new MeasureUnit(PQCollection.Length, "mm");
            public static readonly MeasureUnit cm = new MeasureUnit(PQCollection.Length, "cm", mm, 1e1);
            public static readonly MeasureUnit m = new MeasureUnit(PQCollection.Length, "m", mm, 1e3);
            public static readonly MeasureUnit km = new MeasureUnit(PQCollection.Length, "km", m, 1e3);

            public static readonly MeasureUnit inch = new MeasureUnit(PQCollection.Length, "in", mm, 25.4);
            public static readonly MeasureUnit ft = new MeasureUnit(PQCollection.Length, "ft", inch, 12);
            public static readonly MeasureUnit yard = new MeasureUnit(PQCollection.Length, "yard", ft, 3);
            public static readonly MeasureUnit links = new MeasureUnit(PQCollection.Length, "links", ft, 0.66);
        }

        #endregion

        #region Length2

        public static class Length2
        {
            /// <summary>
            /// retrieve mm2 when input length=mm, etc
            /// </summary>            
            public static MeasureUnit Auto(MeasureUnit length)
            {
                if (length.Equals(Length.mm)) return mm2;
                if (length.Equals(Length.cm)) return cm2;
                if (length.Equals(Length.m)) return m2;
                if (length.Equals(Length.inch)) return inch2;                

                throw new Exception($"undefined auto Length for unit [{length}]");
            }

            public static readonly MeasureUnit mm2 = new MeasureUnit(PQCollection.Length2, "mm2");
            public static readonly MeasureUnit cm2 = new MeasureUnit(PQCollection.Length2, "cm2", mm2, Pow((1.0).Convert(Length.cm, Length.mm), 2));
            public static readonly MeasureUnit m2 = new MeasureUnit(PQCollection.Length2, "m2", mm2, Pow((1.0).Convert(Length.m, Length.mm), 2));
            public static readonly MeasureUnit inch2 = new MeasureUnit(PQCollection.Length2, "in2", mm2, Pow((1.0).Convert(Length.inch, Length.mm), 2));
        }

        #endregion

        #region Length3

        public static class Length3
        {
            /// <summary>
            /// retrieve mm3 when input length=mm, etc
            /// </summary>            
            public static MeasureUnit Auto(MeasureUnit length)
            {
                if (length.Equals(Length.mm)) return mm3;
                if (length.Equals(Length.cm)) return cm3;
                if (length.Equals(Length.m)) return m3;
                if (length.Equals(Length.inch)) return in3;

                throw new Exception($"undefined auto Length for unit [{length}]");
            }

            public static readonly MeasureUnit mm3 = new MeasureUnit(PQCollection.Length3, "mm3");
            public static readonly MeasureUnit cm3 = new MeasureUnit(PQCollection.Length3, "cm3", mm3, Pow((1.0).Convert(Length.cm, Length.mm), 3));
            public static readonly MeasureUnit m3 = new MeasureUnit(PQCollection.Length3, "m3", mm3, Pow((1.0).Convert(Length.m, Length.mm), 3));
            public static readonly MeasureUnit in3 = new MeasureUnit(PQCollection.Length3, "in3", mm3, Pow((1.0).Convert(Length.inch, Length.mm), 3));
            public static readonly MeasureUnit lt = new MeasureUnit(PQCollection.Length3, "lt", mm3, 1e6);
        }

        #endregion

        #region Length4

        public static class Length4
        {
            /// <summary>
            /// retrieve mm4 when input length=mm, etc
            /// </summary>            
            public static MeasureUnit Auto(MeasureUnit length)
            {
                if (length.Equals(Length.mm)) return mm4;
                if (length.Equals(Length.cm)) return cm4;
                if (length.Equals(Length.m)) return m4;
                if (length.Equals(Length.inch)) return in4;

                throw new Exception($"undefined auto Length for unit [{length}]");
            }

            public static readonly MeasureUnit mm4 = new MeasureUnit(PQCollection.Length4, "mm4");
            public static readonly MeasureUnit cm4 = new MeasureUnit(PQCollection.Length4, "cm4", mm4, Pow((1.0).Convert(Length.cm, Length.mm), 4));
            public static readonly MeasureUnit m4 = new MeasureUnit(PQCollection.Length4, "m4", mm4, Pow((1.0).Convert(Length.m, Length.mm), 4));
            public static readonly MeasureUnit in4 = new MeasureUnit(PQCollection.Length4, "in4", mm4, Pow((1.0).Convert(Length.inch, Length.mm), 4));            
        }

        #endregion

        #region VolumetricFlowRate

        public static class VolumetricFlowRate
        {
            public static readonly MeasureUnit m3_s = new MeasureUnit(PQCollection.VolumetricFlowRate, "m3_s");
            public static readonly MeasureUnit lt_s = new MeasureUnit(PQCollection.VolumetricFlowRate, "lt_s", m3_s, (1.0).Convert(Length3.lt, Length3.m3));
            public static readonly MeasureUnit lt_min = new MeasureUnit(PQCollection.VolumetricFlowRate, "lt_min", lt_s, 1.0 / (1.0).Convert(Time.min, Time.sec));
        }

        #endregion

        #region Mass

        public static class Mass
        {
            public static readonly MeasureUnit g = new MeasureUnit(PQCollection.Mass, "g");
            public static readonly MeasureUnit kg = new MeasureUnit(PQCollection.Mass, "kg", g, 1e3);
            public static readonly MeasureUnit T = new MeasureUnit(PQCollection.Mass, "T", kg, 1e3);
        }

        #endregion

        #region Time

        public static class Time
        {
            public static readonly MeasureUnit sec = new MeasureUnit(PQCollection.Time, "sec");
            public static readonly MeasureUnit min = new MeasureUnit(PQCollection.Time, "min", sec, 60);
            public static readonly MeasureUnit hr = new MeasureUnit(PQCollection.Time, "hr", min, 60);
        }

        #endregion

        #region ElectricCurrent

        public static class ElectricCurrent
        {
            public static readonly MeasureUnit A = new MeasureUnit(PQCollection.ElectricCurrent, "A");
        }

        #endregion

        #region Temperature

        public static class Temperature
        {
            public static readonly MeasureUnit C = new MeasureUnit(PQCollection.Temperature, "C", NonLinearConvFunctor);
            public static readonly MeasureUnit K = new MeasureUnit(PQCollection.Temperature, "K", NonLinearConvFunctor);
            public static readonly MeasureUnit F = new MeasureUnit(PQCollection.Temperature, "F", NonLinearConvFunctor);

            static Func<MeasureUnit, MeasureUnit, double, double> _nonLinearConvFunctor;
            public static Func<MeasureUnit, MeasureUnit, double, double> NonLinearConvFunctor
            {
                get
                {
                    if (_nonLinearConvFunctor == null)
                    {
                        _nonLinearConvFunctor = (muFrom, muTo, valueFrom) =>
                        {
                            if (muFrom == muTo) return valueFrom;

                            if (muFrom == C)
                            {
                                if (muTo == K) return valueFrom + 273.15;
                                if (muTo == F) return valueFrom * (9.0 / 5) + 32;
                            }
                            else if (muFrom == K)
                            {
                                if (muTo == C) return valueFrom - 273.15;
                                if (muTo == F) return valueFrom * (9.0 / 5) - 459.67;
                            }
                            else if (muFrom == F)
                            {
                                if (muTo == C) return (valueFrom - 32) * (5.0 / 9);
                                if (muTo == K) return (valueFrom + 459.67) * (5.0 / 9);
                            }

                            throw new NotImplementedException($"not yet implemented non linear conversion from [{muFrom}] to [{muTo}]");
                        };
                    }
                    return _nonLinearConvFunctor;
                }
            }

        }

        #endregion

        #region AmountOfSubstance

        public static class AmountOfSubstance
        {
            public static readonly MeasureUnit mol = new MeasureUnit(PQCollection.AmountOfSubstance, "mol");
        }

        #endregion

        #region LuminousIntensity

        public static class LuminousIntensity
        {
            public static readonly MeasureUnit cd = new MeasureUnit(PQCollection.LuminousIntensity, "cd");
        }

        #endregion

        //-------------------------------------------------------------------

        #region PlaneAngle

        public static class PlaneAngle
        {
            public static readonly MeasureUnit rad = new MeasureUnit(PQCollection.PlaneAngle, "rad");
            public static readonly MeasureUnit grad = new MeasureUnit(PQCollection.PlaneAngle, "grad", rad, PI / 180.0);
            public static readonly MeasureUnit deg = new MeasureUnit(PQCollection.PlaneAngle, "deg", rad, PI / 180.0);
        }

        #endregion

        #region Pressure

        public static class Pressure
        {
            public static readonly MeasureUnit Pa = new MeasureUnit(PQCollection.Pressure, "Pa");
            public static readonly MeasureUnit kPa = new MeasureUnit(PQCollection.Pressure, "kPa", Pa, 1e3);
            /// <summary>
            /// N/mm2
            /// </summary>
            public static readonly MeasureUnit MPa = new MeasureUnit(PQCollection.Pressure, "MPa", kPa, 1e3);
            public static readonly MeasureUnit GPa = new MeasureUnit(PQCollection.Pressure, "GPa", MPa, 1e3);
            public static readonly MeasureUnit bar = new MeasureUnit(PQCollection.Pressure, "bar", Pa, 1e5);

            public static MeasureUnit Auto(MeasureUnit force, MeasureUnit length)
            {
                #region force=[N]
                if (force.Equals(Force.N))
                {
                    if (length.Equals(Length.m)) return Pa;
                }
                #endregion

                #region force=[kN]
                else if (force.Equals(Force.kN))
                {
                    if (length.Equals(Length.m)) return Pressure.kPa;
                    if (length.Equals(Length.mm)) return Pressure.GPa;
                }
                #endregion

                throw new NotImplementedException($"pressure mu automatic not defined for input force=[{force.Name}] and length=[{length.Name}]");
            }
        }

        #endregion

        #region Power

        public static class Power
        {
            public static readonly MeasureUnit W = new MeasureUnit(PQCollection.Power, "W");
        }

        #endregion

        #region Acceleration

        public static class Acceleration
        {
            public static readonly MeasureUnit m_s2 = new MeasureUnit(PQCollection.Acceleration, "m_s2");
        }

        #endregion

        #region Turbidity

        public static class Turbidity
        {
            public static readonly MeasureUnit FNU = new MeasureUnit(PQCollection.Turbidity, "FNU");
        }

        #endregion

        #region Force

        public static class Force
        {
            public static readonly MeasureUnit N = new MeasureUnit(PQCollection.Force, "N");
            public static readonly MeasureUnit kN = new MeasureUnit(PQCollection.Force, "kN", N, 1e3);
        }

        #endregion

        #region Speed

        public static class Speed
        {
            public static readonly MeasureUnit m_s = new MeasureUnit(PQCollection.Speed, "m_s");
        }

        #endregion

        #region BendingMoment

        public static class BendingMoment
        {
            public static readonly MeasureUnit Nm = new MeasureUnit(PQCollection.BendingMoment, "Nm");
            public static readonly MeasureUnit kNm = new MeasureUnit(PQCollection.BendingMoment, "kNm", Nm, 1e3);
        }

        #endregion

        #region Energy

        public static class Energy
        {
            public static readonly MeasureUnit J = new MeasureUnit(PQCollection.Energy, "J");
        }

        #endregion                

        #region ElectricalConductance

        public static class ElectricalConductance
        {
            public static readonly MeasureUnit S = new MeasureUnit(PQCollection.ElectricalConductance, "S");
        }

        #endregion

        #region ElectricalConductivity

        public static class ElectricalConductivity
        {
            public static readonly MeasureUnit S_m = new MeasureUnit(PQCollection.ElectricalConductivity, "S_m");
        }

        #endregion

    }

}
