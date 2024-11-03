using System;
using System.Collections.Generic;

namespace CallaghanDev.Common.Finance.IGRest.Responses
{
    public class InstrumentResponse
    {
        public Instrument Instrument { get; set; }
    }

    public class Instrument
    {
        public string Epic { get; set; }
        public string Expiry { get; set; }
        public string Name { get; set; }
        public bool ForceOpenAllowed { get; set; }
        public bool StopsLimitsAllowed { get; set; }
        public decimal LotSize { get; set; }
        public string Unit { get; set; }
        public string Type { get; set; }
        public bool ControlledRiskAllowed { get; set; }
        public bool StreamingPricesAvailable { get; set; }
        public string MarketId { get; set; }
        public List<Currency> Currencies { get; set; }
        public object SprintMarketsMinimumExpiryTime { get; set; } // null value as object
        public object SprintMarketsMaximumExpiryTime { get; set; } // null value as object
        public List<MarginDepositBand> MarginDepositBands { get; set; }
        public decimal MarginFactor { get; set; }
        public string MarginFactorUnit { get; set; }
        public SlippageFactor SlippageFactor { get; set; }
        public LimitedRiskPremium LimitedRiskPremium { get; set; }
        public object OpeningHours { get; set; } // null value as object
        public ExpiryDetails ExpiryDetails { get; set; }
        public object RolloverDetails { get; set; } // null value as object
        public object NewsCode { get; set; } // null value as object
        public object ChartCode { get; set; } // null value as object
        public string Country { get; set; }
        public string ValueOfOnePip { get; set; }
        public string OnePipMeans { get; set; }
        public string ContractSize { get; set; }
        public List<string> SpecialInfo { get; set; }
        public DealingRules DealingRules { get; set; }
        public Snapshot Snapshot { get; set; }
    }

    public class Currency
    {
        public string Code { get; set; }
        public string Symbol { get; set; }
        public decimal BaseExchangeRate { get; set; }
        public decimal ExchangeRate { get; set; }
        public bool IsDefault { get; set; }
    }

    public class MarginDepositBand
    {
        public decimal Min { get; set; }
        public decimal? Max { get; set; } // nullable decimal to accommodate "null" values
        public int Margin { get; set; }
        public string Currency { get; set; }
    }

    public class SlippageFactor
    {
        public string Unit { get; set; }
        public decimal Value { get; set; }
    }

    public class LimitedRiskPremium
    {
        public decimal Value { get; set; }
        public object Unit { get; set; } // null value as object
    }

    public class ExpiryDetails
    {
        public DateTime LastDealingDate { get; set; }
        public string SettlementInfo { get; set; }
    }

    public class DealingRules
    {
        public DealingRule MinStepDistance { get; set; }
        public DealingRule MinDealSize { get; set; }
        public DealingRule MinControlledRiskStopDistance { get; set; }
        public DealingRule MinNormalStopOrLimitDistance { get; set; }
        public DealingRule MaxStopOrLimitDistance { get; set; }
        public DealingRule ControlledRiskSpacing { get; set; }
        public string MarketOrderPreference { get; set; }
        public string TrailingStopsPreference { get; set; }
    }

    public class DealingRule
    {
        public string Unit { get; set; }
        public decimal Value { get; set; }
    }

    public class Snapshot
    {
        public string MarketStatus { get; set; }
        public decimal NetChange { get; set; }
        public decimal PercentageChange { get; set; }
        public string UpdateTime { get; set; }
        public int DelayTime { get; set; }
        public decimal Bid { get; set; }
        public decimal Offer { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public object BinaryOdds { get; set; } // null value as object
        public int DecimalPlacesFactor { get; set; }
        public int ScalingFactor { get; set; }
        public object ControlledRiskExtraSpread { get; set; } // null value as object
    }
}
