using System.Text;
using System.Windows.Forms;

namespace GetTextEncoding
{
    internal partial class SettingsForm_GetTextEncoding : Form
    {


        #region Get and Set Options

        public string TextFileDirectory { get; set; }
        public bool ScanSubfolders { get; set; }
        public string FileExtension { get; set; }

       #endregion



        public SettingsForm_GetTextEncoding(string TextFileDirectory, bool ScanSubfolders, string FileExtension)
        {
            InitializeComponent();

            ExtentionTextBox.Text = FileExtension;
            IncludeSubfoldersCheckbox.Checked = ScanSubfolders;
            SelectedFolderTextbox.Text = TextFileDirectory;

        }






        private void SetFolderButton_Click(object sender, System.EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.ShowNewFolderButton = false;
                dialog.Description = "Please choose the location of your .txt files to analyze";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    SelectedFolderTextbox.Text = dialog.SelectedPath.ToString();
                }
            }
        }


        private void OKButton_Click(object sender, System.EventArgs e)
        {
            this.FileExtension = ExtentionTextBox.Text;
            this.ScanSubfolders = IncludeSubfoldersCheckbox.Checked;
            this.TextFileDirectory = SelectedFolderTextbox.Text;
            this.DialogResult = DialogResult.OK;
        }
    }
}
