using System;
using System.Collections.Generic;

using SearchAThing.Util;
using System.Runtime.Serialization;

namespace SearchAThing.Sci
{

    [DataContract(IsReference = true)]
    public class Project
    {

        [DataMember]
        public MUDomain MUDomain { get; private set; }

        public Project(MUDomain mud)
        {
            MUDomain = mud;
        }

        public void Save(string dstPathfilename, bool binary = true, IEnumerable<Type> knownTypes = null)
        {
            throw new NotImplementedException();
            //this.Serialize(dstPathfilename, binary, knownTypes);
        }

    }

}
