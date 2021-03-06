﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace VideoScreensaver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window {
        private bool preview;
        private System.Windows.Point? lastMousePosition = null;  // Workaround for "MouseMove always fires when maximized" bug.

        // Determines whether debug info is printed on screen
        const bool debugMode = true;

        private double volume {
            get { return FullScreenMedia.Volume; }
            set {
                FullScreenMedia.Volume = Math.Max(Math.Min(value, 1), 0);
                PreferenceManager.WriteVolumeSetting(FullScreenMedia.Volume);
            }
        }

        public MainWindow(bool preview, System.Windows.Forms.Screen screen) {
            InitializeComponent();
            this.preview = preview;
            FullScreenMedia.Volume = PreferenceManager.ReadVolumeSetting();
            if (preview) {
                ShowError("When fullscreen, control volume with up/down arrows or mouse wheel.");
            }
            // Set the dimensions to that of the screen
            var bounds = screen.Bounds;
            this.Left = bounds.Left;
            this.Top = bounds.Top;
            this.Width = bounds.Width;
            this.Height = bounds.Height;

        }

        private void ScrKeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Up:
                case Key.VolumeUp:
                    volume += 0.1;
                    break;
                case Key.Down:
                case Key.VolumeDown:
                    volume -= 0.1;
                    break;
                case Key.VolumeMute:
                case Key.D0:
                    volume = 0;
                    break;
                default:
                    EndFullScreensaver();
                    break;
            }
        }

        private void ScrMouseWheel(object sender, MouseWheelEventArgs e) {
            volume += e.Delta / 1000.0;
        }

        private void ScrMouseMove(object sender, MouseEventArgs e) {
            // Workaround for bug in WPF.
            System.Windows.Point mousePosition = e.GetPosition(this);
            if (lastMousePosition != null && mousePosition != lastMousePosition) {
                EndFullScreensaver();
            }
            lastMousePosition = mousePosition;
        }

        private void ScrMouseDown(object sender, MouseButtonEventArgs e) {
            EndFullScreensaver();
        }

        private void ScrSizeChange(object sender, SizeChangedEventArgs e) {
            FullScreenMedia.Width = e.NewSize.Width;
            FullScreenMedia.Height = e.NewSize.Height;
        }

        // End the screensaver only if running in full screen. No-op in preview mode.
        private void EndFullScreensaver() {
            if (!preview) {
                //Close();
                Application.Current.Shutdown(); // This gets all windows, not just the current one
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            List<String> videoPaths = PreferenceManager.ReadVideoSettings();
            if (videoPaths.Count == 0) {
                ShowError("This screensaver needs to be configured before any video is displayed.");
            } else {
                // Maximise the window
                this.WindowState = WindowState.Maximized;
                PlayNextVideo(sender, e);
            }
        }

        private void ShowError(string errorMessage) {
            ErrorText.Text = errorMessage;
            ErrorText.Visibility = System.Windows.Visibility.Visible;
            if (preview) {
                ErrorText.FontSize = 12;
            }
        }

        private void MediaEnded(object sender, RoutedEventArgs e) {
            PlayNextVideo(sender, e);
        }

        /// <summary>
        /// Plays the next video in the series on the media object on the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayNextVideo(object sender, RoutedEventArgs e)
        {
            List<String> videoPaths = PreferenceManager.ReadVideoSettings();
            FullScreenMedia.Source = new System.Uri(videoPaths[new Random().Next(videoPaths.Count)]);
        }
    }
}
