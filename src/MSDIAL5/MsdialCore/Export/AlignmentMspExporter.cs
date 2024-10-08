﻿using CompMs.Common.Components;
using CompMs.Common.DataObj.Result;
using CompMs.MsdialCore.Algorithm.Annotation;
using CompMs.MsdialCore.DataObj;
using CompMs.MsdialCore.MSDec;
using CompMs.MsdialCore.Parameter;
using System;
using System.IO;

namespace CompMs.MsdialCore.Export
{
    public sealed class AlignmentMspExporter : IAlignmentSpectraExporter
    {
        private readonly IMatchResultRefer<MoleculeMsReference?, MsScanMatchResult?> _refer;
        private readonly ParameterBase _parameter;

        public AlignmentMspExporter(IMatchResultRefer<MoleculeMsReference?, MsScanMatchResult?> refer, ParameterBase parameter) {
            _refer = refer ?? throw new ArgumentNullException(nameof(refer));
            _parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        }

        void IAlignmentSpectraExporter.Export(Stream stream, AlignmentSpotProperty spot, MSDecResult msdecResult) {
            SpectraExport.SaveSpectraTableAsNistFormat(stream, spot, msdecResult.Spectrum, _refer, _parameter);
        }
    }
}
