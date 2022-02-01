﻿using CompMs.App.Msdial.Model.Search;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace CompMs.App.Msdial.ViewModel.Lcms
{
    public class LcmsCompoundSearchViewModel : CompoundSearchVM
    {
        public LcmsCompoundSearchViewModel(CompoundSearchModel model) : base(model) {
            ParameterHasErrors = ParameterVM.Select(parameter =>
                new[]
                {
                    parameter.Ms1Tolerance.ObserveHasErrors,
                    parameter.Ms2Tolerance.ObserveHasErrors,
                    parameter.RtTolerance.ObserveHasErrors,
                }.CombineLatestValuesAreAllFalse()
                .Inverse())
            .Switch()
            .ToReadOnlyReactivePropertySlim()
            .AddTo(Disposables);

            SearchCommand = new IObservable<bool>[]
            {
                IsBusy,
                ParameterHasErrors,
            }.CombineLatestValuesAreAllFalse()
            .ToReactiveCommand().AddTo(Disposables);

            Compounds = ParameterVM.Select(parameter =>
                new[]
                {
                    parameter.Ms1Tolerance.ToUnit(),
                    parameter.Ms2Tolerance.ToUnit(),
                    parameter.RtTolerance.ToUnit(),
                }.Merge())
                .Switch()
                .Where(_ => !ParameterHasErrors.Value)
                .Select(_ => Observable.FromAsync(SearchAsync))
                .Switch()
                .ToReadOnlyReactivePropertySlim()
                .AddTo(Disposables);

            SearchCommand.Execute();
        }
    }
}