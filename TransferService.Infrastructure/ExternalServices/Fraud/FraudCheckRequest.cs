using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferService.Infrastructure.ExternalServices.Fraud
{
    public class FraudCheckRequest
    {
        public string TransactionId { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public string ToUserId { get; set; } = null!;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "TRY";

        public FraudMetadata Metadata { get; set; } = new();
    }

    public class FraudMetadata
    {
        public string Description { get; set; } = "Money transfer";

        public string Category { get; set; } = "transfer";

        public string Location { get; set; } = "Istanbul, TR";
    }
}
