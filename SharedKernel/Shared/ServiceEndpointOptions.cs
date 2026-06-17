using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Shared;

public class ServiceEndpointOptions
{
    public Dictionary<string, string> Services { get; set; }
        = new();
}