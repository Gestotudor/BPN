using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferService.Infrastructure.ExternalServices.Exchange
{
    public class ExchangeRateResponse
    {
        public string From { get; set; } = null!;

        public string To { get; set; } = null!;

        public decimal Rate { get; set; }

        public long Timestamp { get; set; }

        public int ValidForSeconds { get; set; }
    }
}
