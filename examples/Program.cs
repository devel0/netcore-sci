using System;
using System.Collections.Generic;
using SearchAThing.Sci;
using SearchAThing;
using System.Linq;
using netDxf;
using System.IO;
using SearchAThing.Sci.Tests;

namespace examples
{
    class Program
    {
        static void Main(string[] args)
        {            
            DxfKitTimeline();
        }

        static void DxfKitTimeline()
        {
            var dxf = new netDxf.DxfDocument();

            var ents = dxf.DrawTimeline(new List<(DateTime from, DateTime to)>()
            {
                (new DateTime(2000,1,1), new DateTime(2001,7,31)),
                (new DateTime(2004,2,1), new DateTime(2007,4,28)),
                (new DateTime(2007,4,1), new DateTime(2008,12,31)),
                (new DateTime(2008,8,1), new DateTime(2012,10,31)),
                (new DateTime(2012,11,1), new DateTime(2015,10,31)),
                (new DateTime(2016,4,1), new DateTime(2017,12,31)),
                (new DateTime(2019,1,7), new DateTime(2019,7,7))
            });
            dxf.AddEntities(ents);

            dxf.Viewport.ShowGrid = false;
            dxf.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "timeline.dxf"), isBinary: true);
        }
    }
}
