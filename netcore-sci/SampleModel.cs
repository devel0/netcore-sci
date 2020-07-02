namespace SearchAThing
{

    public class SampleModel : ISciModel
    {

        public IMUDomain MUDomain { get; private set; }

        public SampleModel()
        {
            MUDomain = new MUDomain();
        }        

    }

}
