﻿using CompMs.Common.Enum;
using CompMs.CommonMVVM;
using CompMs.MsdialCore.DataObj;
using CompMs.MsdialCore.Export;
using CompMs.MsdialCore.Parser;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace CompMs.App.Msdial.Model.Export
{
    internal sealed class AlignmentSpectraExportGroupModel : BindableBase, IAlignmentResultExportModel
    {
        public AlignmentSpectraExportGroupModel(AlignmentMspExporter exporter, IEnumerable<ExportspectraType> spectraTypes) {
            SpectraTypes = new ObservableCollection<ExportspectraType>(spectraTypes);
            Formats = new ObservableCollection<AlignmentSpectraExportFormat> { new AlignmentSpectraExportFormat("Msp", "msp", exporter), };
        }

        public AlignmentSpectraExportGroupModel(IEnumerable<ExportspectraType> spectraTypes, params AlignmentSpectraExportFormat[] formats) {
            SpectraTypes = new ObservableCollection<ExportspectraType>(spectraTypes);
            Formats = new ObservableCollection<AlignmentSpectraExportFormat>(formats);
        }

        public ExportspectraType SpectraType {
            get => _spectraType;
            set => SetProperty(ref _spectraType, value);
        }
        private ExportspectraType _spectraType = ExportspectraType.deconvoluted;

        public ObservableCollection<ExportspectraType> SpectraTypes { get; }

        public ObservableCollection<AlignmentSpectraExportFormat> Formats { get; }

        public int CountExportFiles() {
            return Formats.Count(f => f.IsSelected);
        }

        public void Export(AlignmentFileBean alignmentFile, string exportDirectory, Action<string> notification) {
            var dt = DateTime.Now;
            var cts = new CancellationTokenSource();
            var resultContainer = AlignmentResultContainer.LoadLazy(alignmentFile, cts.Token);
            var msdecResults = MsdecResultsReader.ReadMSDecResults(alignmentFile.SpectraFilePath, out _, out _);
            var outNameTemplate = $"{{0}}_{alignmentFile.FileID}_{dt:yyyy_MM_dd_HH_mm_ss}.{{1}}";
            foreach (var format in Formats) {
                if (format.IsSelected) {
                    format.Export(resultContainer, msdecResults, outNameTemplate, exportDirectory, notification);
                }
            }
            cts.Cancel();
        }
    }
}