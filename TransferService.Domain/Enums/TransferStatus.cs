using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferService.Domain.Enums
{
    public enum TransferStatus
    {
        Pending = 1,
        Completed = 2,
        Cancelled = 3,
        Failed = 4
    }
}
