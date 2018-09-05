using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenFSM.Objects
{
    public abstract class Delegates
    {
        public delegate void Start();
        public delegate void DoThings();
        public delegate void Exit();
    }
}
