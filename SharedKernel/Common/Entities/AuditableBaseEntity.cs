using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Common.Entities;

public abstract class AuditableBaseEntity
{
    [Key]
    public Guid id { get; set; } = default!;
    public DateTime created { get; set; }
    public DateTime updated { get; set; }
    public Guid? created_by { get; set; }
    public Guid? updated_by { get; set; }
}
