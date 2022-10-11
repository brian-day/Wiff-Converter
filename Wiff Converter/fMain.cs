using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System;

namespace Wiff_Converter
{
    public enum ExportFormat
    {
        TimeExplicit = 0,
        WavelengthExplicit = 1
    }

    public partial class fMain : Form
    {
        public Dictionary<string, string> delimiters;
        public Dictionary<string, string> decimalSeparators;
        public Dictionary<string, ExportFormat> exportFormats;
        public string[] chosenFilepaths;

        public fMain()
        {
            InitializeComponent();
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

        private void fMain_Load(object sender, EventArgs e)
        {
            cbExtension.Items.Add("csv");
            cbExtension.Items.Add("txt");

            cbDelimiter.Items.AddRange(delimiters.Keys.ToArray());
            cbDecimalSeparator.Items.AddRange(decimalSeparators.Keys.ToArray());
            cbExportFormat.Items.AddRange(exportFormats.Keys.ToArray());

            cbDelimiter.SelectedIndex = 0;
            cbDecimalSeparator.SelectedIndex = 0;
            cbExtension.SelectedIndex = 0;
            cbExportFormat.SelectedIndex = 1;


            lblNumberSig.Text = "Number of significant figures\nof exported values:";
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            if (chosenFilepaths is null)
                return;

            btnConvert.Enabled = false;

            string fileExt = cbExtension.Text;
            string delimiter = delimiters[cbDelimiter.Text];
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = decimalSeparators[cbDecimalSeparator.Text];
            string dirPath = tbOutputDir.Text;
            int sigFigures = (int)nudSignificantFigures.Value;
            ExportFormat exportFormat = exportFormats[cbExportFormat.Text];
            bool norm2TIC = cbNormalizeToTIC.Checked;

            try
            {
                foreach (string filename in chosenFilepaths)
                {
                    Reader r = new Reader(filename);
                    r.SaveAbsorptionMatrix(nfi, delimiter, fileExt, dirPath, exportFormat, sigFigures);
                    r.SaveMSMatrix(nfi, delimiter, fileExt, dirPath, exportFormat, norm2TIC, sigFigures);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            btnConvert.Enabled = true;
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                AddExtension = false,
                Filter = "Wiff files (*.wiff)|*.wiff",
                RestoreDirectory = true,
                Multiselect = true
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (ofd.FileNames.Length == 0)
                    return;

                chosenFilepaths = ofd.FileNames;

                tbFilenames.Text = string.Join(Environment.NewLine, chosenFilepaths);

                string dirPath = Path.GetDirectoryName(chosenFilepaths[0]);
                tbOutputDir.Text = dirPath is null ? "" : dirPath;
            }

        }

        private void btnChangeDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fwd = new FolderBrowserDialog();
            //fwd.InitialDirectory = tbOutputDir.Text;
            fwd.ShowNewFolderButton = true;

            if (fwd.ShowDialog() == DialogResult.OK)
            {
                tbOutputDir.Text = fwd.SelectedPath;
            }
        }
    }
}