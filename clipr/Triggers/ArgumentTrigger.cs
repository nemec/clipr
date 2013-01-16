using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace clipr.Triggers
{
    public class ArgumentTrigger<T> : ITrigger<T> where T : class
    {
        public string PluginName
        {
            get { throw new NotImplementedException(); }
        }

        public ParserConfig<T> Config
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public char? ShortName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string LongName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void OnParse()
        {
            throw new NotImplementedException();
        }
    }
}
