using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferService.Infrastructure.ExternalServices.Fraud
{
    public class FraudHealthResponse
    {
        public bool Success { get; set; }

        public FraudHealthData? Data { get; set; }
    }

    public class FraudHealthData
    {
        public string Status { get; set; } = null!;

        public string Service { get; set; } = null!;

        public DateTime Timestamp { get; set; }
    }
}
