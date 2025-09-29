using System;
using System.Collections.Generic;
using Common;
using Hentul.Inteerfaces;
using FBBM = FirstOrderMemory.BehaviourManagers.BlockBehaviourManagerFOM;

namespace Hentul
{
    /// <summary>
    /// MotorStreamProcessor:
    ///  - Owns a single FOM (Layer 4 analogue) block.
    ///  - Processes only SDR_SOM inputs via ProcessSDR (no generic object dispatch; IStreamProcessor.Process removed).
    ///  - Exposes L3B-equivalent output (bursting columns) and prediction list (empty).
    /// </summary>
    public class MotorStreamProcessor
    {
        private readonly FBBM _fom;
        private SDR_SOM? _lastInput;
        private ulong _lastCycle;

        public LogMode LogMode { get; }
        public SDR_SOM? LastInput => _lastInput;
        public ulong LastCycle => _lastCycle;

        public MotorStreamProcessor(int numColumns = 10, int z = 4, LogMode logMode = LogMode.None)
        {
            LogMode = logMode;
            _fom = new FBBM(numColumns, numColumns, z, LayerType.Layer_4, LogMode.None);
            _fom.Init(0);
        }

        /// <summary>
        /// Processes a TEMPORAL SDR through the motor FOM block.
        /// Caller is responsible for providing a correctly formed SDR (e.g. from MotorEncoder).
        /// </summary>
        public void ProcessSDR(SDR_SOM sdr, ulong cycleNum)
        {
            if (sdr == null) throw new ArgumentNullException(nameof(sdr));
            if (sdr.InputPatternType != iType.SPATIAL)
                throw new InvalidOperationException("MotorStreamProcessor expects a SPATIAL SDR_SOM.");

            _lastCycle = cycleNum;
            _lastInput = sdr;
            _fom.Fire(sdr, cycleNum);
        }

        /// <summary>
        /// L3B equivalent: bursting columns for the processed cycle.
        /// </summary>
        public SDR_SOM? GetL3B(ulong cycleNum) =>
            cycleNum == _lastCycle ? _fom.GetAllColumnsBurstingLatestCycle(cycleNum) : null;

        public IEnumerable<string> GetCurrentPredictions() => Array.Empty<string>();

        /// <summary>
        /// Optional diagnostics: all firing neurons (burst + predicted if flag set).
        /// </summary>
        public SDR_SOM? GetAllFiring(ulong cycleNum, bool includePredicted = false) =>
            cycleNum == _lastCycle ? _fom.GetAllNeuronsFiringLatestCycle(cycleNum, includePredicted) : null;
    }
}
