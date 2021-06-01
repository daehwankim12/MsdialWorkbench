﻿using CompMs.App.Msdial.Model.DataObj;
using CompMs.App.Msdial.ViewModel;
using CompMs.Common.Components;
using CompMs.Common.DataObj.Result;
using CompMs.Common.Extension;
using CompMs.CommonMVVM;
using CompMs.MsdialCore.Algorithm;
using CompMs.MsdialCore.Algorithm.Annotation;
using CompMs.MsdialCore.DataObj;
using CompMs.MsdialCore.MSDec;
using CompMs.MsdialCore.Parameter;
using CompMs.MsdialCore.Parser;
using CompMs.MsdialCore.Utility;
using CompMs.MsdialImmsCore.Parameter;
using NSSplash;
using NSSplash.impl;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CompMs.App.Msdial.Model.Imms
{
    class ImmsAnalysisModel : ViewModelBase
    {
        public ImmsAnalysisModel(
            AnalysisFileBean analysisFile,
            IDataProvider provider,
            ParameterBase parameter,
            IAnnotator<ChromatogramPeakFeature, MSDecResult> mspAnnotator,
            IAnnotator<ChromatogramPeakFeature, MSDecResult> textDBAnnotator) {

            this.analysisFile = analysisFile;
            this.provider = provider;
            this.parameter = parameter as MsdialImmsParameter;
            this.mspAnnotator = mspAnnotator;
            this.textDBAnnotator = textDBAnnotator;

            FileName = analysisFile.AnalysisFileName;
            peakAreaFile = analysisFile.PeakAreaBeanInformationFilePath;
            deconvolutionFile = analysisFile.DeconvolutionFilePath;

            var peaks = MsdialSerializer.LoadChromatogramPeakFeatures(peakAreaFile);
            Ms1Peaks = new ObservableCollection<ChromatogramPeakFeatureModel>(
                peaks.Select(peak => new ChromatogramPeakFeatureModel(peak, parameter.TargetOmics != CompMs.Common.Enum.TargetOmics.Metabolomics))
            );
            AmplitudeOrderMin = Ms1Peaks.DefaultIfEmpty().Min(peak => peak?.AmplitudeOrderValue) ?? 0;
            AmplitudeOrderMax = Ms1Peaks.DefaultIfEmpty().Max(peak => peak?.AmplitudeOrderValue) ?? 0;

            MsdecResultsReader.GetSeekPointers(deconvolutionFile, out _, out seekPointers, out _);

            EicModel = new Chart.EicModel(
                new EicLoader(provider, parameter, ChromXType.Drift, ChromXUnit.Msec, this.parameter.DriftTimeBegin, this.parameter.DriftTimeEnd)
                )
            {
                HorizontalTitle = "Drift time [1/k0]",
                VerticalTitle = "Abundance",
            };
            PlotModel = new Chart.AnalysisPeakPlotModel(Ms1Peaks, peak => peak.ChromXValue ?? 0, peak => peak.Mass)
            {
                HorizontalTitle = EicModel.HorizontalTitle,
                VerticalTitle = "m/z",
                HorizontalProperty = nameof(ChromatogramPeakWrapper.ChromXValue),
                VerticalProperty = nameof(ChromatogramPeakFeatureModel.Mass),
            };

            Target = PlotModel
                .ObserveProperty(m => m.Target)
                .ToReadOnlyReactivePropertySlim()
                .AddTo(Disposables);
            Target
                .Where(t => t != null)
                .Subscribe(t => PlotModel.GraphTitle = $"Spot ID: {t.InnerModel.MasterPeakID} Scan: {t.InnerModel.MS1RawSpectrumIdTop} Mass m/z: {t.InnerModel.Mass:N5}");
            Target
                .Where(t => t == null)
                .Subscribe(_ => PlotModel.GraphTitle = string.Empty);
            Target.Subscribe(async t => await OnTargetChangedAsync(t));
        }

        private readonly AnalysisFileBean analysisFile;
        private readonly MsdialImmsParameter parameter;
        private readonly IAnnotator<ChromatogramPeakFeature, MSDecResult> mspAnnotator, textDBAnnotator;
        private readonly string peakAreaFile;
        private readonly string deconvolutionFile;
        private readonly List<long> seekPointers;
        private readonly IDataProvider provider;
        
        public ObservableCollection<ChromatogramPeakFeatureModel> Ms1Peaks {
            get => ms1Peaks;
            set => SetProperty(ref ms1Peaks, value);
        }
        private ObservableCollection<ChromatogramPeakFeatureModel> ms1Peaks;

        public Chart.AnalysisPeakPlotModel PlotModel { get; }

        public Chart.EicModel EicModel { get; }

        public List<SpectrumPeakWrapper> Ms1Spectrum
        {
            get => ms1Spectrum;
            set {
                if (SetProperty(ref ms1Spectrum, value)) {
                    OnPropertyChanged(nameof(Ms1SpectrumMaxIntensity));
                }
            }
        }
        private List<SpectrumPeakWrapper> ms1Spectrum = new List<SpectrumPeakWrapper>();

        public double Ms1SpectrumMaxIntensity => Ms1Spectrum.Select(peak => peak.Intensity).DefaultIfEmpty().Max();

        public string Ms1SplashKey {
            get => ms1SplashKey;
            set => SetProperty(ref ms1SplashKey, value);
        }
        private string ms1SplashKey = string.Empty;

        public double Ms1Tolerance => parameter.CentroidMs1Tolerance;

        public List<SpectrumPeakWrapper> Ms2Spectrum {
            get => ms2Spectrum;
            set {
                if (SetProperty(ref ms2Spectrum, value)) {
                    OnPropertyChanged(nameof(Ms2MassMax));
                    OnPropertyChanged(nameof(Ms2MassMin));
                }
            }
        }
        private List<SpectrumPeakWrapper> ms2Spectrum = new List<SpectrumPeakWrapper>();

        public string RawSplashKey {
            get => rawSplashKey;
            set => SetProperty(ref rawSplashKey, value);
        }
        private string rawSplashKey = string.Empty;

        public List<SpectrumPeakWrapper> Ms2DecSpectrum {
            get => ms2DecSpectrum;
            set {
                if (SetProperty(ref ms2DecSpectrum, value)) {
                    OnPropertyChanged(nameof(Ms2MassMax));
                    OnPropertyChanged(nameof(Ms2MassMin));
                }
            }
        }
        private List<SpectrumPeakWrapper> ms2DecSpectrum = new List<SpectrumPeakWrapper>();

        public string DeconvolutionSplashKey {
            get => deconvolutionSplashKey;
            set => SetProperty(ref deconvolutionSplashKey, value);
        }
        private string deconvolutionSplashKey = string.Empty;

        public List<SpectrumPeakWrapper> Ms2ReferenceSpectrum {
            get => ms2ReferenceSpectrum;
            set {
                if (SetProperty(ref ms2ReferenceSpectrum, value)) {
                    OnPropertyChanged(nameof(Ms2MassMax));
                    OnPropertyChanged(nameof(Ms2MassMin));
                }
            }
        }
        private List<SpectrumPeakWrapper> ms2ReferenceSpectrum = new List<SpectrumPeakWrapper>();

        public double Ms2MassMin => Ms2Spectrum.Concat(Ms2ReferenceSpectrum).Concat(Ms2DecSpectrum).Select(peak => peak.Mass).DefaultIfEmpty().Min();
        public double Ms2MassMax => Ms2Spectrum.Concat(Ms2ReferenceSpectrum).Concat(Ms2DecSpectrum).Select(peak => peak.Mass).DefaultIfEmpty().Max();

        public ReadOnlyReactivePropertySlim<ChromatogramPeakFeatureModel> Target { get; }

        public string FileName {
            get => fileName;
            set => SetProperty(ref fileName, value);
        }
        private string fileName;

        public string DisplayLabel {
            get => displayLabel;
            set => SetProperty(ref displayLabel, value);
        }
        private string displayLabel;

        public double AmplitudeOrderMin { get; }
        public double AmplitudeOrderMax { get; }

        public int FocusID {
            get => focusID;
            set => SetProperty(ref focusID, value);
        }
        private int focusID;

        public double FocusDt {
            get => focusDt;
            set => SetProperty(ref focusDt, value);
        }
        private double focusDt;

        public double FocusMz {
            get => focusMz;
            set => SetProperty(ref focusMz, value);
        }
        private double focusMz;

        public double ChromMin => Ms1Peaks.Min(peak => peak.ChromXValue) ?? 0;
        public double ChromMax => Ms1Peaks.Max(peak => peak.ChromXValue) ?? 0;
        public double MassMin => Ms1Peaks.Min(peak => peak.Mass);
        public double MassMax => Ms1Peaks.Max(peak => peak.Mass);
        public double IntensityMin => Ms1Peaks.Min(peak => peak.Intensity);
        public double IntensityMax => Ms1Peaks.Max(peak => peak.Intensity);

        private MSDecResult msdecResult = null;

        private CancellationTokenSource cts;
        async Task OnTargetChangedAsync(ChromatogramPeakFeatureModel target) {
            cts?.Cancel();
            var localCts = cts = new CancellationTokenSource();

            try {
                await OnTargetChangedAsync(target, localCts.Token).ContinueWith(
                    t => {
                        localCts.Dispose();
                        if (cts == localCts)
                            cts = null;
                    }).ConfigureAwait(false);
            }
            catch (OperationCanceledException) {

            }
        }

        async Task OnTargetChangedAsync(ChromatogramPeakFeatureModel target, CancellationToken token) {
            if (target != null) {
                FocusID = target.InnerModel.MasterPeakID;
                FocusDt = target.ChromXValue ?? 0;
                FocusMz = target.Mass;
            }

            await Task.WhenAll(
                LoadMs1SpectrumAsync(target, token),
                EicModel.LoadEicAsync(target, token),
                LoadMs2SpectrumAsync(target, token),
                LoadMs2DecSpectrumAsync(target, token),
                LoadMs2ReferenceAsync(target, token)
            ).ConfigureAwait(false);
        }

        async Task LoadMs1SpectrumAsync(ChromatogramPeakFeatureModel target, CancellationToken token) {
            var ms1Spectrum = new List<SpectrumPeakWrapper>();
            var ms1SplashKey = string.Empty;

            if (target != null) {
                await Task.Run(() => {
                    if (target.MS1RawSpectrumIdTop < 0) {
                        return;
                    }
                    var spectra = DataAccess.GetCentroidMassSpectra(provider.LoadMs1Spectrums()[target.MS1RawSpectrumIdTop], parameter.MSDataType, 0, float.MinValue, float.MaxValue);
                    ms1Spectrum = spectra.Select(peak => new SpectrumPeakWrapper(peak)).ToList();
                    ms1SplashKey = CalculateSplashKey(spectra);
                }, token);
            }

            token.ThrowIfCancellationRequested();
            Ms1Spectrum = ms1Spectrum;
            Ms1SplashKey = ms1SplashKey;
        }

        async Task LoadMs2SpectrumAsync(ChromatogramPeakFeatureModel target, CancellationToken token) {
            var ms2Spectrum = new List<SpectrumPeakWrapper>(); 
            var rawSplashKey = string.Empty;

            if (target != null) {
                await Task.Run(() => {
                    if (target.MS2RawSpectrumId < 0)
                        return;
                    var spectra = DataAccess.GetCentroidMassSpectra(provider.LoadMsSpectrums()[target.MS2RawSpectrumId], parameter.MS2DataType, 0, float.MinValue, float.MaxValue);
                    if (parameter.RemoveAfterPrecursor)
                        spectra = spectra.Where(peak => peak.Mass <= target.Mass + parameter.KeptIsotopeRange).ToList();
                    token.ThrowIfCancellationRequested();
                    ms2Spectrum = spectra.Select(peak => new SpectrumPeakWrapper(peak)).ToList();
                    rawSplashKey = CalculateSplashKey(spectra);
                }, token).ConfigureAwait(false);
            }

            token.ThrowIfCancellationRequested();
            Ms2Spectrum = ms2Spectrum;
            RawSplashKey = rawSplashKey;
        }

        async Task LoadMs2DecSpectrumAsync(ChromatogramPeakFeatureModel target, CancellationToken token) {
            var ms2DecSpectrum = new List<SpectrumPeakWrapper>();
            var deconvolutionSplashKey = string.Empty;

            if (target != null) {
                await Task.Run(() => {
                    var idx = Ms1Peaks.IndexOf(target);
                    msdecResult = MsdecResultsReader.ReadMSDecResult(deconvolutionFile, seekPointers[idx]);
                    token.ThrowIfCancellationRequested();
                    ms2DecSpectrum = msdecResult.Spectrum.Select(spec => new SpectrumPeakWrapper(spec)).ToList();
                    deconvolutionSplashKey = CalculateSplashKey(msdecResult.Spectrum);
                }, token).ConfigureAwait(false);
            }

            token.ThrowIfCancellationRequested();
            Ms2DecSpectrum = ms2DecSpectrum;
            DeconvolutionSplashKey = deconvolutionSplashKey;
        }

        async Task LoadMs2ReferenceAsync(ChromatogramPeakFeatureModel target, CancellationToken token) {
            var ms2ReferenceSpectrum = new List<SpectrumPeakWrapper>();

            if (target != null) {
                await Task.Run(() => {
                    var representative = RetrieveMspMatchResult(target.InnerModel);
                    if (representative == null)
                        return;

                    var reference = mspAnnotator.Refer(representative);
                    if (reference != null) {
                        token.ThrowIfCancellationRequested();
                        ms2ReferenceSpectrum = reference.Spectrum.Select(peak => new SpectrumPeakWrapper(peak)).ToList();
                    }
                }, token).ConfigureAwait(false);
            }

            token.ThrowIfCancellationRequested();
            Ms2ReferenceSpectrum = ms2ReferenceSpectrum;
        }

        MsScanMatchResult RetrieveMspMatchResult(ChromatogramPeakFeature prop) {
            if (prop.MatchResults?.Representative is MsScanMatchResult representative) {
                if ((representative.Source & (SourceType.Unknown | SourceType.Manual)) == (SourceType.Unknown | SourceType.Manual))
                    return null;
                if (prop.MatchResults.TextDbBasedMatchResults.Contains(representative)) {
                    return null;
                }
                if ((representative.Source & SourceType.Unknown) == SourceType.None) {
                    return representative;
                }
            }
            return prop.MspBasedMatchResult;
        }

        static string CalculateSplashKey(IReadOnlyCollection<SpectrumPeak> spectra) {
            if (spectra.IsEmptyOrNull() || spectra.Count <= 2 && spectra.All(peak => peak.Intensity == 0))
                return "N/A";
            var msspectrum = new MSSpectrum(string.Join(" ", spectra.Select(peak => $"{peak.Mass}:{peak.Intensity}").ToArray()));
            return new Splash().splashIt(msspectrum);
        }
    }
}