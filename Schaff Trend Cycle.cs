// -------------------------------------------------------------------------------
//   Schaff Trend Cycle - Cyclical Stoch over Stoch over MACD.
//   Falling below 75 is a sell signal.
//   Rising above 25 is a buy signal.
//   Developed by Doug Schaff.
//   Code adapted from the original TradeStation EasyLanguage version.
//   Copyright 2015-2022, EarnForex.com
//   https://www.earnforex.com/metatrader-indicators/Schaff-Trend-Cycle/
// -------------------------------------------------------------------------------

using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Indicators
{
    [Levels(25, 75)]
    [Indicator(AccessRights = AccessRights.None)]
    public class SchaffTrendCycle : Indicator
    {
        [Parameter("MA Short", DefaultValue = 23, MinValue = 1)]
        public int MAShort { get; set; }

        [Parameter("MA Long", DefaultValue = 50, MinValue = 1)]
        public int MALong { get; set; }

        [Parameter("Cycle", DefaultValue = 10, MinValue = 1)]
        public int Cycle { get; set; }

        [Output("Schaff Trend Cycle", LineColor = "DarkOrchid", Thickness = 2)]
        public IndicatorDataSeries ST2 { get; set; }

        private MovingAverage MA_Short;
        private MovingAverage MA_Long;
        private IndicatorDataSeries ST;
        private IndicatorDataSeries MACD;

        private double Factor = 0.5;

        protected override void Initialize()
        {
            MA_Short = Indicators.MovingAverage(Bars.ClosePrices, MAShort, MovingAverageType.Exponential);
            MA_Long = Indicators.MovingAverage(Bars.ClosePrices, MALong, MovingAverageType.Exponential);
            ST = CreateDataSeries();
            MACD = CreateDataSeries();
        }

        public override void Calculate(int index)
        {
            if (index < Cycle + 1)
                return;

            double LLV = 0, HHV = 0;
            int i;

            MACD[index] = MA_Short.Result[index] - MA_Long.Result[index];

            // Finding Max and Min on Cycle of MA differrences (MACD)
            for (i = index; i > index - Cycle; i--)
            {
                if (i == index)
                {
                    LLV = MACD[i];
                    HHV = MACD[i];
                }
                else
                {
                    if (LLV > MACD[i])
                        LLV = MACD[i];
                    if (HHV < MACD[i])
                        HHV = MACD[i];
                }
            }
            // Calculating first Stochastic
            if (HHV - LLV != 0)
                ST[index] = ((MACD[index] - LLV) / (HHV - LLV)) * 100;
            else
                ST[index] = ST[index - 1];

            // Smoothing first Stochastic
            if (!double.IsNaN(ST[index - 1]))
                ST[index] = Factor * (ST[index] - ST[index - 1]) + ST[index - 1];

            // Finding Max and Min on Cycle of first smoothed Stoch
            for (i = index; i > index - Cycle; i--)
            {
                if (i == index)
                {
                    LLV = ST[i];
                    HHV = ST[i];
                }
                else
                {
                    if (LLV > ST[i])
                        LLV = ST[i];
                    if (HHV < ST[i])
                        HHV = ST[i];
                }
            }
            // Calculating second Stochastic
            if (HHV - LLV != 0)
                ST2[index] = ((ST[index] - LLV) / (HHV - LLV)) * 100;
            else
                ST2[index] = ST2[index - 1];

            // Smoothing second Stochastic
            if (!double.IsNaN(ST2[index - 1]))
                ST2[index] = Factor * (ST2[index] - ST2[index - 1]) + ST2[index - 1];
        }
    }
}
