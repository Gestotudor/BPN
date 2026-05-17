using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferService.Infrastructure.ExternalServices.Fraud
{
    public class FraudCheckResponse
    {
        public bool Success { get; set; }

        public FraudData? Data { get; set; }
    }

    public class FraudData
    {
        public string TransactionId { get; set; } = null!;

        public string RiskLevel { get; set; } = null!;

        public decimal RiskScore { get; set; }

        public List<string> RiskFactors { get; set; } = [];

        public bool ShouldBlock { get; set; }

        public List<string> Recommendations { get; set; } = [];

        public List<string> RequiredActions { get; set; } = [];

        public decimal ProcessingTime { get; set; }
    }
}
