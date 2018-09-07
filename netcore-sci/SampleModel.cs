using System;

namespace SearchAThing.Sci
{

    public class SampleModel : IModel
    {

        public IMUDomain MUDomain { get; private set; }

        public SampleModel()
        {
            MUDomain = new MUDomain();
        }        

    }

}
