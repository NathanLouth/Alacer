using CefSharp;
using CefSharp.DevTools.IndexedDB;
using CefSharp.DevTools.LayerTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
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
using System.Windows.Shell;
using System.Windows.Threading;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig;
using static System.Net.WebRequestMethods;
using Page = UglyToad.PdfPig.Content.Page;
using System.Media;
using System.Diagnostics.Eventing.Reader;

namespace Alacer
{

    public partial class MainWindow : Window
    {
        DispatcherTimer refreshTimer = new DispatcherTimer();
        PdfPrintSettings printSettings = new PdfPrintSettings();
        string PDFpath = System.IO.Path.GetTempPath() + "\\9275849387.pdf";
        int refreshDelay = 90;
        int loadingDelay = 30;
        bool isRunning = false;
        bool containsText = false;
        string wavFile = @"C:\Program Files\Alacer\Alacer\Alert.wav";
        string PDFText = "";
        string pageText = "";

        public void EditStatusBox(string update, bool concat = false)
        {
            if (concat)
            {
                StatusBox.Text = StatusBox.Text + "- " + update + " ";
            }
            else
            {
                StatusBox.Text = " " + update + " ";
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            EditStatusBox("Initialize Components");

            if (System.IO.File.Exists(PDFpath))
            {
                System.IO.File.Delete(PDFpath);
                EditStatusBox("Old PDF Deleted");
            }
            else
            {
                EditStatusBox("No PDFs Found");
            }

            refreshTimer.Interval = TimeSpan.FromSeconds(90);
            refreshTimer.Tick += OnTimerTick;

            EditStatusBox("Timer Created", true);

        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(async () =>
            {
                EditStatusBox("Reloading Browser");

                Browser.Reload();

                EditStatusBox("Page Load Delay", true);

                await Task.Delay(loadingDelay * 1000);

                EditStatusBox("Creating PDF");

                printSettings.Landscape = true;
                printSettings.PrintBackground = true;
                await Browser.PrintToPdfAsync(PDFpath, printSettings);

                EditStatusBox("Getting Text From PDF");

                pageText = "";

                using (PdfDocument document = PdfDocument.Open(PDFpath))
                {
                    foreach (Page page in document.GetPages())
                    {
                        pageText += page.Text;

                    }
                }

                EditStatusBox("Checking if PDF Text Contains Lookup Text");

                bool trueCheck = pageText.Contains(lookUpBox.Text);

                if (checkBox.IsChecked == true)
                {
                    if (trueCheck)
                    {
                        EditStatusBox("Playing Alert");
                        PlayAlert(wavFile);
                    }
                    else
                    {
                        EditStatusBox("Text Not Found");
                    }
                }
                else
                {
                    if (!trueCheck)
                    {
                        EditStatusBox("Playing Alert");
                        PlayAlert(wavFile);
                    }
                    else
                    {
                        EditStatusBox("Text Found");
                    }
                }

                if (System.IO.File.Exists(PDFpath))
                {
                    System.IO.File.Delete(PDFpath);
                    EditStatusBox("PDF Deleted", true);
                }
                else
                {
                    EditStatusBox("No PDF Found (ERROR)", true);
                }

            }));

        }

        static void PlayAlert(string file)
        {

            SoundPlayer player = new SoundPlayer(file);

            player.PlayLooping();

            System.Windows.Forms.DialogResult res = System.Windows.Forms.MessageBox.Show("Alacer Alert", "Alacer Alert", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);

            player.Stop();
        }

        private void txtBoxAddress_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            if (e.Key == System.Windows.Input.Key.Enter)
            {
                string loadURL = txtBoxAddress.Text;

                if (!loadURL.EndsWith(".com") && !loadURL.EndsWith(".co.uk") && !loadURL.EndsWith("/"))
                {
                    loadURL = loadURL.Replace(" ", "+");
                    loadURL = $"https://www.google.com/search?q={loadURL}";
                }
                Browser.Load(loadURL);
            }

        }

        private void refreshDelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (refreshLabel == null) { return; }
            int roundedValue = (int)Math.Round(e.NewValue);
            refreshLabel.Content = "Delay: " + roundedValue.ToString() + " (sec)";
            refreshDelay = roundedValue;
            refreshTimer.Interval = TimeSpan.FromSeconds(refreshDelay);

            loadingDelaySlider.Maximum = refreshDelay - 30;
        }

        private void loadingDelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (loadingLabel == null) { return; }
            int roundedValue = (int)Math.Round(e.NewValue);
            loadingLabel.Content = "Delay: " + roundedValue.ToString() + " (sec)";
            loadingDelay = roundedValue;
        }

        private void EditControlState(bool State)
        {
            checkBox.IsEnabled = State;
            lookUpBox.IsEnabled = State;
            refreshDelaySlider.IsEnabled = State;
            loadingDelaySlider.IsEnabled = State;
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning)
            {
                RunButton.Content = "Start";
                isRunning = false;
                EditStatusBox("Stopped");
                EditControlState(true);
                refreshTimer.Stop();
            }
            else
            {
                RunButton.Content = "Stop";
                isRunning = true;
                EditStatusBox("Starting...");
                EditControlState(false);
                refreshTimer.Start();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            EditStatusBox("Cleaning Up Program");

            refreshTimer.Stop();

            EditStatusBox("Stop Timer", true);

            if (System.IO.File.Exists(PDFpath))
            {
                System.IO.File.Delete(PDFpath);
                EditStatusBox("PDF Deleted",true);
            }
            else
            {
                EditStatusBox("No PDF Found",true);
            }
        }

    }
}