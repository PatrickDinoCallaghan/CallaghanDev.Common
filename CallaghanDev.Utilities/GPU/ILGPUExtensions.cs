using ILGPU.Runtime;
using ILGPU;
using System.Diagnostics;
using ILGPU.Runtime.OpenCL;
using ILGPU.Runtime.CPU;
using DocumentFormat.OpenXml.Math;

namespace CallaghanDev.Utilities.GPU
{
    public static class ILGPUExtensions
    {
        public static Accelerator GetFastestAccelerator(Action action)
        {
            Context context = Context.CreateDefault();
            Context.DeviceCollection<CLDevice> accelerators = context.GetCLDevices();

            long fastestTime = long.MaxValue;
            int deviceIndex = 0;
            CLAccelerator fastestAccelerator = null;
            foreach (CLDevice Device in accelerators)
            {
                CLAccelerator accelerator = context.CreateCLAccelerator(deviceIndex);
                var watch = Stopwatch.StartNew();

                action();

                watch.Stop();

                if (watch.ElapsedMilliseconds < fastestTime)
                {
                    fastestTime = watch.ElapsedMilliseconds; fastestAccelerator = accelerator;
                }

                accelerator.Dispose();
                deviceIndex++;
            }


            return fastestAccelerator;
        }

        static void BenchmarkKernel(Index1D index, ArrayView<int> data)
        {
            // Simple operation for benchmarking purposes
            if (index < data.Length)
            {
                data[index] = index;
            }
        }
    }
}
