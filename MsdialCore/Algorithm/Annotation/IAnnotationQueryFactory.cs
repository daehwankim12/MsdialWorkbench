﻿using CompMs.Common.DataObj;
using CompMs.Common.Interfaces;
using CompMs.Common.Parameter;
using CompMs.MsdialCore.Parameter;
using CompMs.MsdialCore.Utility;
using System;
using System.Collections.Generic;

namespace CompMs.MsdialCore.Algorithm.Annotation
{
    public interface IAnnotationQueryFactory<out T>
    {
        T Create(IMSIonProperty property, IMSScanProperty scan, IReadOnlyList<RawPeakElement> spectrum);
    }

    public class AnnotationQueryFactory : IAnnotationQueryFactory<AnnotationQuery>
    {
        private readonly PeakPickBaseParameter peakPickParameter;

        public AnnotationQueryFactory(PeakPickBaseParameter peakPickParameter, MsRefSearchParameterBase searchParameter = null) {
            this.peakPickParameter = peakPickParameter ?? throw new ArgumentNullException(nameof(peakPickParameter));
            SearchParameter = searchParameter;
        }

        public MsRefSearchParameterBase SearchParameter { get; set; }

        public AnnotationQuery Create(IMSIonProperty property, IMSScanProperty scan, IReadOnlyList<RawPeakElement> spectrum) {
            var isotopes = DataAccess.GetIsotopicPeaks(spectrum, (float)property.PrecursorMz, peakPickParameter.CentroidMs1Tolerance);
            return new AnnotationQuery(property, scan, isotopes, SearchParameter);
        }
    }
}