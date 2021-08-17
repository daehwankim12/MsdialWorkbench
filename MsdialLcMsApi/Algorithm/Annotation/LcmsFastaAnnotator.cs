﻿using CompMs.Common.Algorithm.Scoring;
using CompMs.Common.Components;
using CompMs.Common.DataObj.Property;
using CompMs.Common.DataObj.Result;
using CompMs.Common.Extension;
using CompMs.Common.FormulaGenerator.Function;
using CompMs.Common.Interfaces;
using CompMs.Common.Parameter;
using CompMs.Common.Utility;
using CompMs.MsdialCore.Algorithm;
using CompMs.MsdialCore.Algorithm.Annotation;
using CompMs.MsdialCore.DataObj;
using CompMs.MsdialCore.Parameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompMs.MsdialLcMsApi.Algorithm.Annotation {
    public class LcmsFastaAnnotator : ProteomicsStandardRestorableBase, ISerializableAnnotator<IMSIonProperty, IMSScanProperty, MoleculeDataBase> {

        private static readonly IComparer<IMSScanProperty> comparer = CompositeComparer.Build(MassComparer.Comparer, ChromXsComparer.RTComparer);

        public DataBaseRefer ReferObject { get; }

        public LcmsFastaAnnotator(IEnumerable<MoleculeMsReference> reference, MsRefSearchParameterBase parameter, ProteomicsParameter proteomicsParam,
            string annotatorID, SourceType type) : base(reference, parameter, proteomicsParam, annotatorID, type) {
            db.Sort(comparer);
            this.ReferObject = new DataBaseRefer(db);
        }

        public MsScanMatchResult Annotate(
        IMSIonProperty property,
        IMSScanProperty scan,
        IReadOnlyList<IsotopicPeak> isotopes,
        MsRefSearchParameterBase parameter = null) {
            if (parameter is null) {
                parameter = Parameter;
            }
            return FindCandidatesCore(property, scan, isotopes, parameter).FirstOrDefault();
        }

        public List<MsScanMatchResult> FindCandidates(
        IMSIonProperty property, IMSScanProperty scan, IReadOnlyList<IsotopicPeak> isotopes,
        MsRefSearchParameterBase parameter = null) {
            if (parameter is null) {
                parameter = Parameter;
            }
            return FindCandidatesCore(property, scan, isotopes, parameter);
        }

        private List<MsScanMatchResult> FindCandidatesCore(
       IMSIonProperty property, IMSScanProperty scan, IReadOnlyList<IsotopicPeak> isotopes, MsRefSearchParameterBase parameter) {
            (var lo, var hi) = SearchBoundIndex(property, this.db, this.Parameter.Ms1Tolerance);
            var results = new List<MsScanMatchResult>(hi - lo);
            for (var i = lo; i < hi; i++) {
                var candidate = this.db[i];
                if (this.Parameter.IsUseTimeForAnnotationFiltering
                    && Math.Abs(property.ChromXs.RT.Value - candidate.ChromXs.RT.Value) > this.Parameter.RtTolerance) {
                    continue;
                }
                var result = CalculateScoreCore(property, scan, isotopes, candidate, candidate.IsotopicPeaks, parameter, this.ProteomicsParameter, this.SourceType, this.Key);
                result.LibraryIDWhenOrdered = i;
                ValidateCore(result, property, scan, candidate, this.Parameter);
                results.Add(result);
            }
            return results.OrderByDescending(result => result.TotalScore).ToList();
        }

        public MsScanMatchResult CalculateScore(
        IMSIonProperty property, IMSScanProperty scan, IReadOnlyList<IsotopicPeak> isotopes, MoleculeMsReference reference, MsRefSearchParameterBase parameter = null) {
            if (parameter is null) {
                parameter = Parameter;
            }
            return CalculateScoreCore(property, scan, isotopes, reference, reference.IsotopicPeaks, parameter, this.ProteomicsParameter, this.SourceType, this.Key);
        }

        private static MsScanMatchResult CalculateScoreCore(
            IMSIonProperty property, IMSScanProperty scan, IReadOnlyList<IsotopicPeak> scanIsotopes,
            MoleculeMsReference reference, IReadOnlyList<IsotopicPeak> referenceIsotopes,
            MsRefSearchParameterBase msSearchParam, ProteomicsParameter proteomicsParam, SourceType type, string annotatorID) {

            var result = MsScanMatching.CompareMS2ScanProperties(scan, reference, msSearchParam, Common.Enum.TargetOmics.Proteomics, -1,
                null, null, proteomicsParam.AndromedaDelta, proteomicsParam.AndromedaMaxPeaks);

            result.Source = type;
            result.SourceKey = annotatorID;

            return result;
        }

        private static (int lo, int hi) SearchBoundIndex(IMSIonProperty property, IReadOnlyList<MoleculeMsReference> mspDB, double ms1Tolerance) {

            ms1Tolerance = CalculateMassTolerance(ms1Tolerance, property.PrecursorMz);
            var dummy = new MSScanProperty { PrecursorMz = property.PrecursorMz - ms1Tolerance };
            var lo = SearchCollection.LowerBound(mspDB, dummy, comparer);
            dummy.PrecursorMz = property.PrecursorMz + ms1Tolerance;
            var hi = SearchCollection.UpperBound(mspDB, dummy, lo, mspDB.Count, comparer);
            return (lo, hi);
        }

        public List<MoleculeMsReference> Search(IMSIonProperty property, MsRefSearchParameterBase parameter = null) {

            if (parameter is null) {
                parameter = Parameter;
            }

            (var lo, var hi) = SearchBoundIndex(property, db, parameter.Ms1Tolerance);
            return db.GetRange(lo, hi - lo);
        }

        private static double CalculateMassTolerance(double tolerance, double mass) {
            if (mass <= 500)
                return tolerance;
            var ppm = Math.Abs(MolecularFormulaUtility.PpmCalculator(500.00, 500.00 + tolerance));
            return MolecularFormulaUtility.ConvertPpmToMassAccuracy(mass, ppm);
        }

        public void Validate(
            MsScanMatchResult result,
            IMSIonProperty property, IMSScanProperty scan, IReadOnlyList<IsotopicPeak> isotopes,
            MoleculeMsReference reference, MsRefSearchParameterBase parameter = null) {

            if (parameter is null) {
                parameter = Parameter;
            }

            ValidateCore(result, property, scan, reference, parameter);
        }

        private static void ValidateCore(
            MsScanMatchResult result,
            IMSIonProperty property, IMSScanProperty scan,
            MoleculeMsReference reference,
            MsRefSearchParameterBase parameter) {

            ValidateBase(result, property, reference, parameter);
        }

        private static void ValidateBase(MsScanMatchResult result, IMSIonProperty property, MoleculeMsReference reference, MsRefSearchParameterBase parameter) {
            result.IsSpectrumMatch = result.WeightedDotProduct >= parameter.WeightedDotProductCutOff
                && result.SimpleDotProduct >= parameter.SimpleDotProductCutOff
                && result.ReverseDotProduct >= parameter.ReverseDotProductCutOff
                && result.MatchedPeaksPercentage >= parameter.MatchedPeaksPercentageCutOff
                && result.MatchedPeaksCount >= parameter.MinimumSpectrumMatch;

            var ms1Tol = CalculateMassTolerance(parameter.Ms1Tolerance, property.PrecursorMz);
            result.IsPrecursorMzMatch = Math.Abs(property.PrecursorMz - reference.PrecursorMz) <= ms1Tol;
            result.IsRtMatch = Math.Abs(property.ChromXs.RT.Value - reference.ChromXs.RT.Value) <= parameter.RtTolerance;
        }


        public MsScanMatchResult SelectTopHit(IEnumerable<MsScanMatchResult> results, MsRefSearchParameterBase parameter = null) {
            return results.Argmax(result => result.TotalScore);
        }

        public List<MsScanMatchResult> FilterByThreshold(IEnumerable<MsScanMatchResult> results, MsRefSearchParameterBase parameter = null) {
            if (parameter is null) {
                parameter = Parameter;
            }
            var filtered = new List<MsScanMatchResult>();
            foreach (var result in results) {
                if (Ms2Filtering(result, parameter)) {
                    continue;
                }
                if (result.TotalScore < parameter.TotalScoreCutoff) {
                    continue;
                }
                filtered.Add(result);
            }
            return filtered;
        }

        private static bool Ms2Filtering(MsScanMatchResult result, MsRefSearchParameterBase parameter) {
            if (!result.IsPrecursorMzMatch && !result.IsSpectrumMatch) {
                return false;
            }
            if (result.WeightedDotProduct < parameter.WeightedDotProductCutOff
                || result.SimpleDotProduct < parameter.SimpleDotProductCutOff
                || result.ReverseDotProduct < parameter.ReverseDotProductCutOff
                || result.MatchedPeaksPercentage < parameter.MatchedPeaksPercentageCutOff
                || result.MatchedPeaksCount < parameter.MinimumSpectrumMatch) {
                return false;
            }
            return true;
        }

        public List<MsScanMatchResult> SelectReferenceMatchResults(IEnumerable<MsScanMatchResult> results, MsRefSearchParameterBase parameter = null) {
            return FilterByThreshold(results, parameter)
                .Where(result => result.IsPrecursorMzMatch && result.IsSpectrumMatch)
                .ToList();
        }

        public override MoleculeMsReference Refer(MsScanMatchResult result) {
            return ReferObject.Refer(result);
        }
    }
}
