using System;
using System.Diagnostics;
using System.Linq;
/*using System.Windows.Forms;*/
using System.Xml.Linq;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Wiff_Converter
{
    public enum ExportFormat
    {
        TimeExplicit = 0,
        WavelengthExplicit = 1
    }

    public partial class fMain
    {
        // Properties
        public Dictionary<string, string> delimiters;
        public Dictionary<string, string> decimalSeparators;
        public Dictionary<string, ExportFormat> exportFormats;
        public string[] chosenFilepaths;

        // Methods
        public fMain()
        {
            delimiters = new Dictionary<string, string>
            {
                { "Comma", "," },
                { "Tabulator", "\t" },
                { "Space", " " },
                { "Colon", ":" },
                { "Semicolon", ";" },
                { "Dot", "." }
            };
            decimalSeparators = new Dictionary<string, string>
            {
                { "Dot", "." },
                { "Comma", "," }
            };
            exportFormats = new Dictionary<string, ExportFormat>
            {
                { "Time-explicit (scans in columns)", ExportFormat.TimeExplicit },
                { "Wavelength-explicit (scans in rows)", ExportFormat.WavelengthExplicit }
            };
        }

        private double? TryParse(string text, NumberStyles ns, IFormatProvider formatProvider)
        {
            if (text.Trim() == String.Empty)
                return null;

            if (double.TryParse(text, ns, formatProvider, out double result))
                return result;

            return null;
        }

        private async void cliConvert()
        {
            if (chosenFilepaths is null) return; // Make this an arg and delete null check.


            // Takes args from the form and sets them for use during conversion
            chosenFilepaths = ofd.FileNames; // Originally set via gui, add as command line arg
            string dirPath = Path.GetDirectoryName(chosenFilepaths[0]);
            tbOutputDir.Text = fwd.SelectedPath;

            // ---------
            string fileExt = ".wiff";
            string delimiter = ",";
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            int sigFigures = 6;
            ExportFormat exportFormat = ".csv";

            bool norm2TIC = False;

            // Misc. Parsing?
            double? t0, t1, w0, w1, m0, m1;
            t0 = TryParse(tbCropT0.Text.Replace(',', '.'), NumberStyles.Any, nfi);
            t1 = TryParse(tbCropT1.Text.Replace(',', '.'), NumberStyles.Any, nfi);
            w0 = TryParse(tbCropW0.Text.Replace(',', '.'), NumberStyles.Any, nfi);
            w1 = TryParse(tbCropW1.Text.Replace(',', '.'), NumberStyles.Any, nfi);
            m0 = TryParse(tbCropM0.Text.Replace(',', '.'), NumberStyles.Any, nfi);
            m1 = TryParse(tbCropM1.Text.Replace(',', '.'), NumberStyles.Any, nfi);


            // Convert files in parallel (this is why this is an async method?)
            // Make a non-parallel method?
            await Task.Run<ConcurrentQueue<Exception>>(() =>
            {
                var exceptions = new ConcurrentQueue<Exception>();

                Parallel.ForEach(chosenFilepaths, filepath =>
                {
                    try
                    {
                        // This will read and write the needed files at dirPath.
                        Reader r = new Reader(filepath);
                        r.SaveAbsorptionMatrix(nfi, delimiter, fileExt, dirPath, exportFormat, sigFigures, w0, w1, t0, t1);
                        r.SaveMSMatrix(nfi, delimiter, fileExt, dirPath, exportFormat, norm2TIC, sigFigures, m0, m1, t0, t1);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Enqueue(ex);
                    }

                });

                return exceptions;

            });
        }

    }
}
