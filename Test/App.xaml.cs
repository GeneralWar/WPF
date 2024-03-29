﻿using Genera.WPF.Test;
using General;
using System.Windows;

namespace Test
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            CommandLine commandLine = CommandLine.Create();
            string? testWindow = commandLine.GetString("test");
            switch (testWindow)
            {
                case "TreeView":
                    new TreeViewTestWindow().Show();
                    break;
                case "NumberInput":
                    new NumberInputTestWindow().Show();
                    break;
                case "PercentInput":
                    new PercentInputTestWindow().Show();
                    break;
                case "CheckBoxExpander":
                    new CheckBoxExpanderWindow().Show();
                    break;
                default:
                    new MainWindow().Show();
                    break;
            }

        }
    }
}
