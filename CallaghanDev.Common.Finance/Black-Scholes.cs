using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CallaghanDev.Common.Math;
using static CallaghanDev.Common.Finance.BlackScholes;
namespace CallaghanDev.Common.Finance
{

    public enum Option
    {
        Call,
        Put
    }
    public struct OptionData
    {
        public decimal AssetPrice { get; set; }
        public decimal StrikePrice { get; set; }
        public decimal OptionPrice { get; set; }
        public DateTime Maturity { get; set; }

        public Option type { get; set; }
    }
    public class BlackScholes
    {
        private readonly DateTime _maturity;
        private readonly decimal _riskFreeRate;

        /// <summary>
        /// Initializes a new instance of the BlackScholes class with the given maturity date and risk-free interest rate.
        /// </summary>
        /// <param name="maturity">The date of option maturity.</param>
        /// <param name="r">The annualized risk-free interest rate.</param>
        public BlackScholes(DateTime maturity, decimal r)
        {
            _maturity = maturity;
            _riskFreeRate = r;
        }

        /// <summary>
        /// Computes the intermediary value d1 in the Black-Scholes formula.
        /// </summary>
        /// <param name="S">Current stock price.</param>
        /// <param name="K">Strike price of the option.</param>
        /// <param name="sigma">Volatility of the stock price.</param>
        /// <param name="t">Time to maturity in years.</param>
        /// <returns>The calculated d1 value.</returns>
        private double CalculateD1(decimal S, decimal K, decimal sigma, decimal t)
        {
            return (System.Math.Log((double)(S / K)) + (double)((_riskFreeRate + (sigma * sigma) / (decimal)2) * t)) / ((double)sigma * System.Math.Sqrt((double)t));
        }

        /// <summary>
        /// Computes the intermediary value d2 in the Black-Scholes formula based on d1.
        /// </summary>
        /// <param name="d1">The previously computed d1 value.</param>
        /// <param name="sigma">Volatility of the stock price.</param>
        /// <param name="t">Time to maturity in years.</param>
        /// <returns>The calculated d2 value.</returns>
        private double CalculateD2(double d1, decimal sigma, decimal t)
        {
            return d1 - (double)sigma * System.Math.Sqrt((double)t);
        }

        /// <summary>
        /// Calculates the price of a European call option using the Black-Scholes formula.
        /// </summary>
        /// <param name="AssetPrice">Current stock price.</param>
        /// <param name="StrikePrice">Strike price of the option.</param>
        /// <param name="sigma">Volatility of the stock price.</param>
        /// <returns>The calculated call option price.</returns>
        public decimal CalculateCallPrice(decimal AssetPrice, decimal StrikePrice, decimal sigma)
        {
            decimal t = (decimal)(_maturity - DateTime.Now).TotalDays / 365;
            double d1 = CalculateD1(AssetPrice, StrikePrice, sigma, t);
            double d2 = CalculateD2(d1, sigma, t);

            return AssetPrice * (decimal)Function.NormalCdf(d1) - StrikePrice * (decimal)System.Math.Exp((double)(-_riskFreeRate * t)) * (decimal)Function.NormalCdf(d2);
        }

        /// <summary>
        /// Calculates the price of a European put option using the Black-Scholes formula.
        /// </summary>
        /// <param name="AssetPrice">Current stock price.</param>
        /// <param name="StrikePrice">Strike price of the option.</param>
        /// <param name="sigma">Volatility of the stock price.</param>
        /// <returns>The calculated put option price.</returns>
        public decimal CalculatePutPrice(decimal AssetPrice, decimal StrikePrice, decimal sigma)
        {
            decimal t = (decimal)(_maturity - DateTime.Now).TotalDays / 365;
            double d1 = CalculateD1(AssetPrice, StrikePrice, sigma, t);
            double d2 = CalculateD2(d1, sigma, t);

            return StrikePrice * (decimal)System.Math.Exp((double)(-_riskFreeRate * t)) * (decimal)Function.NormalCdf(-d2) - AssetPrice * (decimal)Function.NormalCdf(-d1);
        }

        public static (decimal Volatility, decimal RiskFreeRate) FindVolatilityAndRiskFreeRate(List<OptionData> optionDataList)
        {
            decimal sigma = 0.2m; // Initial guess for volatility
            decimal riskFreeRate = 0.05m; // Initial guess for risk-free rate
            decimal epsilon = 0.0001m; // Convergence tolerance
            int maxIterations = 1000; // Maximum iterations

            for (int i = 0; i < maxIterations; i++)
            {
                decimal totalDifference = 0m;
                decimal vegaSum = 0m;
                decimal priceDifferenceSum = 0m;

                foreach (var option in optionDataList)
                {
                    var bs = new BlackScholes(option.Maturity, riskFreeRate);

                    // Calculate the option price using the current sigma and risk-free rate
                    decimal calculatedPrice;
                    if (option.type == Option.Call)
                    {
                        calculatedPrice = bs.CalculateCallPrice(option.AssetPrice, option.StrikePrice, sigma);
                    }
                    else
                    {
                        calculatedPrice = bs.CalculatePutPrice(option.AssetPrice, option.StrikePrice, sigma);
                    }

                    // Calculate the difference between the market price and the calculated price
                    decimal priceDifference = calculatedPrice - option.OptionPrice;

                    // Accumulate total differences and vega for Newton-Raphson update
                    totalDifference += priceDifference;

                    // Approximate the derivative of the option price with respect to sigma (Vega)
                    decimal vega;
                    if (option.type == Option.Call)
                    {
                        vega = (bs.CalculateCallPrice(option.AssetPrice, option.StrikePrice, sigma + epsilon) - calculatedPrice) / epsilon;
                    }
                    else
                    {
                        vega = (bs.CalculatePutPrice(option.AssetPrice, option.StrikePrice, sigma + epsilon) - calculatedPrice) / epsilon;
                    }
                    vegaSum += vega;

                    // Store the price difference sum for the Newton-Raphson step
                    priceDifferenceSum += priceDifference;
                }

                // Check if the total difference is within tolerance
                if (System.Math.Abs(totalDifference) < epsilon)
                {
                    return (sigma, riskFreeRate);
                }

                // Adjust sigma and risk-free rate using the Newton-Raphson step
                // Approximation for interest rate sensitivity
                decimal interestRateSensitivity = totalDifference / vegaSum;
                sigma -= priceDifferenceSum / vegaSum;
                riskFreeRate -= interestRateSensitivity;
            }

            throw new Exception("Volatility and risk-free rate did not converge.");
        }

    }
}