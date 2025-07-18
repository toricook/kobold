using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Events
{
    public abstract class BaseEvent : IEvent
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }
}
