﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kobold.Core.Abstractions
{
    public interface ISystem
    {
        void Update(float deltaTime);
    }
}
