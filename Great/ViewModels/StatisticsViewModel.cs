using GalaSoft.MvvmLight;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Great.ViewModels
{
    public class StatisticsViewModel : ViewModelBase
    {
        public Func<ChartPoint, string> PointLabel { get; set; }

        public StatisticsViewModel()
        {
            PointLabel = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);
        }
    }
}