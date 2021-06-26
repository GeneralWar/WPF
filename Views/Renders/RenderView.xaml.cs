﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace General.WPF
{
    /// <summary>
    /// RenderPanel.xaml 的交互逻辑
    /// </summary>
    public partial class RenderView : UserControl
    {
        private RenderHostWindow mHostWindow = null;
        public RenderHostWindow HostWindow => mHostWindow;

        public RenderView()
        {
            InitializeComponent();

            RenderHostWindow view = mHostWindow = new RenderHostWindow();
            mRenderHolder.Children.Add(view);
        }
    }
}
