using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Common.Handlers.Commands;

public record DeleteCommand<TKey> where TKey : IEquatable<TKey>
{
    [FromRoute]
    public TKey id { get; set; }
}
