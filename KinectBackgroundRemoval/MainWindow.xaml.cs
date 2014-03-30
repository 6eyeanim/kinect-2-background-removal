﻿using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KinectBackgroundRemoval
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor _sensor;
        MultiSourceFrameReader _reader;

        // 1) Create a background removal tool.
        BackgroundRemovalTool _backgroundRemovalTool;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Process.Start(@"C:\Windows\System32\KinectService.exe");

            _sensor = KinectSensor.Default;

            if (_sensor != null)
            {
                _sensor.Open();

                // 2) Initialize the background removal tool.
                _backgroundRemovalTool = new BackgroundRemovalTool(_sensor.CoordinateMapper);

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.BodyIndex);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            var colorFrame = reference.ColorFrameReference.AcquireFrame();
            var depthFrame = reference.DepthFrameReference.AcquireFrame();
            var bodyIndexFrame = reference.BodyIndexFrameReference.AcquireFrame();

            if (colorFrame != null && depthFrame != null && bodyIndexFrame != null)
            {
                // 3) Update the image source.
                camera.Source = _backgroundRemovalTool.GreenScreen(colorFrame, depthFrame, bodyIndexFrame);
            }

            colorFrame.Dispose();
            depthFrame.Dispose();
            bodyIndexFrame.Dispose();
        }
    }
}
