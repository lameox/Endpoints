﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lameox.Endpoints
{
    internal readonly struct PlainTextRequest
    {
        public string Text { get; internal init; }
    }
}
