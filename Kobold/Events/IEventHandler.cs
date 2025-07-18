using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Events
{
    public interface IEventHandler<in T> where T : IEvent
    {
        void Handle(T eventData);
    }
}
