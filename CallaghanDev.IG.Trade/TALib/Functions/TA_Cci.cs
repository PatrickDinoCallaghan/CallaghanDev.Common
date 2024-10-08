/*
 * Technical Analysis Library for .NET
 * Copyright (c) 2020-2024 Anatolii Siryi
 *
 * This file is part of Technical Analysis Library for .NET.
 *
 * Technical Analysis Library for .NET is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * Technical Analysis Library for .NET is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with Technical Analysis Library for .NET. If not, see <https://www.gnu.org/licenses/>.
 */

using JetBrains.Annotations;
using System.Numerics;

namespace TALib;

public static partial class Functions
{
    [PublicAPI]
    public static Core.RetCode Cci<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        CciImpl(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    [PublicAPI]
    public static int CciLookback(int optInTimePeriod = 14) => optInTimePeriod < 2 ? -1 : optInTimePeriod - 1;

    /// <remarks>
    /// For compatibility with abstract API
    /// </remarks>
    [UsedImplicitly]
    private static Core.RetCode Cci<T>(
        T[] inHigh,
        T[] inLow,
        T[] inClose,
        Range inRange,
        T[] outReal,
        out Range outRange,
        int optInTimePeriod = 14) where T : IFloatingPointIeee754<T> =>
        CciImpl<T>(inHigh, inLow, inClose, inRange, outReal, out outRange, optInTimePeriod);

    private static Core.RetCode CciImpl<T>(
        ReadOnlySpan<T> inHigh,
        ReadOnlySpan<T> inLow,
        ReadOnlySpan<T> inClose,
        Range inRange,
        Span<T> outReal,
        out Range outRange,
        int optInTimePeriod) where T : IFloatingPointIeee754<T>
    {
        outRange = Range.EndAt(0);

        if (FunctionHelpers.ValidateInputRange(inRange, inHigh.Length, inLow.Length, inClose.Length) is not { } rangeIndices)
        {
            return Core.RetCode.OutOfRangeParam;
        }

        var (startIdx, endIdx) = rangeIndices;

        if (optInTimePeriod < 2)
        {
            return Core.RetCode.BadParam;
        }

        var lookbackTotal = CciLookback(optInTimePeriod);
        startIdx = Math.Max(startIdx, lookbackTotal);

        if (startIdx > endIdx)
        {
            return Core.RetCode.Success;
        }

        // Allocate a circular buffer equal to the requested period.
        Span<T> circBuffer = new T[optInTimePeriod];
        int circBufferIdx = default;
        var maxIdxCircBuffer = optInTimePeriod - 1;

        // Do the MA calculation using tight loops.

        // Add-up the initial period, except for the last value. Fill up the circular buffer at the same time.
        var i = startIdx - lookbackTotal;
        while (i < startIdx)
        {
            circBuffer[circBufferIdx++] = (inHigh[i] + inLow[i] + inClose[i]) / FunctionHelpers.Three<T>();
            i++;
            if (circBufferIdx > maxIdxCircBuffer)
            {
                circBufferIdx = 0;
            }
        }

        var timePeriod = T.CreateChecked(optInTimePeriod);
        var tPointZeroOneFive = T.CreateChecked(0.015);

        // Proceed with the calculation for the requested range.
        // The algorithm allows the input and output to be the same buffer.
        int outIdx = default;
        do
        {
            var lastValue = (inHigh[i] + inLow[i] + inClose[i]) / FunctionHelpers.Three<T>();
            circBuffer[circBufferIdx++] = lastValue;

            // Calculate the average for the whole period.
            var theAverage = CalcAverage(circBuffer, timePeriod);

            // Do the summation of the Abs(TypePrice - average) for the whole period.
            var tempReal2 = CalcSummation(circBuffer, theAverage);

            var tempReal = lastValue - theAverage;
            outReal[outIdx++] = !T.IsZero(tempReal) && !T.IsZero(tempReal2)
                ? tempReal / (tPointZeroOneFive * (tempReal2 / timePeriod))
                : T.Zero;

            // Move forward the circular buffer indexes.
            if (circBufferIdx > maxIdxCircBuffer)
            {
                circBufferIdx = 0;
            }

            i++;
        } while (i <= endIdx);

        outRange = new Range(startIdx, startIdx + outIdx);

        return Core.RetCode.Success;
    }

    private static T CalcAverage<T>(Span<T> circBuffer, T timePeriod) where T : IFloatingPointIeee754<T>
    {
        var theAverage = T.Zero;
        foreach (var t in circBuffer)
        {
            theAverage += t;
        }

        theAverage /= timePeriod;
        return theAverage;
    }

    private static T CalcSummation<T>(Span<T> circBuffer, T theAverage) where T : IFloatingPointIeee754<T>
    {
        var tempReal2 = T.Zero;
        foreach (var t in circBuffer)
        {
            tempReal2 += T.Abs(t - theAverage);
        }

        return tempReal2;
    }
}
