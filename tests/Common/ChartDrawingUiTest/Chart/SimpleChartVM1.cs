﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChartDrawingUiTest.Chart
{
    internal class SimpleChartVM1 : INotifyPropertyChanged
    {
        public class CategoryData
        {
            public string X { get; set; }
            public string Y { get; set; }
            public double Z { get; set; }
        }

        public ObservableCollection<CategoryData> Series
        {
            get => series;
            set => SetProperty(ref series, value);
        }

        public ObservableCollection<string> Xs
        {
            get => xs;
            set => SetProperty(ref xs, value);
        }

        public ObservableCollection<string> Ys
        {
            get => ys;
            set => SetProperty(ref ys, value);
        }

        public ObservableCollection<double> Zs
        {
            get => zs;
            set => SetProperty(ref zs, value);
        }

        private ObservableCollection<CategoryData> series;
        private ObservableCollection<string> xs;
        private ObservableCollection<string> ys;
        private ObservableCollection<double> zs;

        public SimpleChartVM1()
        {
            var ss = new List<CategoryData>();
            ss.Add(new CategoryData() { X = "a", Y = "D", Z = 1d, });
            ss.Add(new CategoryData() { X = "a", Y = "E", Z = 2d, });
            ss.Add(new CategoryData() { X = "b", Y = "D", Z = 3d, });
            ss.Add(new CategoryData() { X = "c", Y = "F", Z = 2d, });

            Series = new ObservableCollection<CategoryData>(ss);
            Xs = new ObservableCollection<string>(new HashSet<string>(ss.Select(d => d.X)));
            Ys = new ObservableCollection<string>(new HashSet<string>(ss.Select(d => d.Y)));
            Zs = new ObservableCollection<double>(new HashSet<double>(ss.Select(d => d.Z)));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void RaiseProerptyChanged(string propertyname) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        protected bool SetProperty<T>(ref T property, T value, [CallerMemberName]string propertyname = "")
        {
            if (value == null && property == null || value.Equals(property)) return false;
            property = value;
            RaiseProerptyChanged(propertyname);
            return true;
        }
    }
}
