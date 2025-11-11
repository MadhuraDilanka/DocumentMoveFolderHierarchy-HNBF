using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace DocumentMoveApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Initialize form
            lblStatus.Text = "Ready";
        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtConnectionString.Text))
            {
                MessageBox.Show("Please enter a connection string.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (TestConnection(txtConnectionString.Text))
            {
                MessageBox.Show("Connection successful!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadLibraries();
            }
            else
            {
                MessageBox.Show("Connection failed. Please check your connection string.", 
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool TestConnection(string connectionString)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogError($"Connection test failed: {ex.Message}");
                return false;
            }
        }

        private DataTable GetAllLibaries()
        {
            try
            {
                DataTable dataTable = new DataTable();
                SqlConnection connection = new SqlConnection(txtConnectionString.Text);
                connection.Open();
                string commandText = @"SELECT ID, [Name] FROM Portal";

                using (SqlCommand cmd = new SqlCommand(commandText, connection))
                {
                    SqlDataReader sdr = cmd.ExecuteReader();
                    dataTable.Load(sdr);
                }
                connection.Close();
                return dataTable;
            }
            catch (Exception ex)
            {
                LogError($"GetAllLibaries failed: {ex.Message}");
                return null;
            }
        }

        private DataTable GetAllTagProfileByLibraryId(string id)
        {
            try
            {
                DataTable dataTable = new DataTable();
                SqlConnection connection = new SqlConnection(txtConnectionString.Text);
                connection.Open();
                string commandText = @"SELECT ID, [ProfileName] FROM ImportProfile WHERE Library_ID = " + id;

                using (SqlCommand cmd = new SqlCommand(commandText, connection))
                {
                    SqlDataReader sdr = cmd.ExecuteReader();
                    dataTable.Load(sdr);
                }
                connection.Close();
                return dataTable;
            }
            catch (Exception ex)
            {
                LogError($"GetAllTagProfileByLibraryId failed: {ex.Message}");
                return null;
            }
        }

        private void LoadLibraries()
        {
            try
            {
                DataTable dataTable = GetAllLibaries();
                
                if (dataTable == null || dataTable.Rows.Count == 0)
                {
                    lblStatus.Text = "No libraries found";
                    return;
                }

                // Add default "--Select--" row at the beginning
                DataRow newRow = dataTable.NewRow();
                newRow["ID"] = 0;
                newRow["Name"] = "--Select--";
                dataTable.Rows.InsertAt(newRow, 0);

                cmbLibrary.DataSource = dataTable;
                cmbLibrary.DisplayMember = "Name";
                cmbLibrary.ValueMember = "ID";
                cmbLibrary.SelectedIndex = 0;

                lblStatus.Text = $"Loaded {dataTable.Rows.Count - 1} libraries";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading libraries: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError($"Load libraries failed: {ex.Message}");
            }
        }

        private void cmbLibrary_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbLibrary.SelectedValue == null || cmbLibrary.SelectedValue.ToString() == "0")
            {
                cmbImportProfile.DataSource = null;
                return;
            }

            LoadImportProfiles(cmbLibrary.SelectedValue.ToString());
        }

        private void LoadImportProfiles(string libraryId)
        {
            try
            {
                DataTable dataTable = GetAllTagProfileByLibraryId(libraryId);
                
                if (dataTable == null || dataTable.Rows.Count == 0)
                {
                    cmbImportProfile.DataSource = null;
                    lblStatus.Text = "No import profiles found for selected library";
                    return;
                }

                // Add default "--Select--" row at the beginning
                DataRow newRow = dataTable.NewRow();
                newRow["ID"] = 0;
                newRow["ProfileName"] = "--Select--";
                dataTable.Rows.InsertAt(newRow, 0);

                cmbImportProfile.DataSource = dataTable;
                cmbImportProfile.DisplayMember = "ProfileName";
                cmbImportProfile.ValueMember = "ID";
                cmbImportProfile.SelectedIndex = 0;

                lblStatus.Text = $"Loaded {dataTable.Rows.Count - 1} import profiles";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading import profiles: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError($"Load import profiles failed: {ex.Message}");
            }
        }

        private async void btnProcess_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtConnectionString.Text))
            {
                MessageBox.Show("Please enter a connection string.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbLibrary.SelectedItem == null || cmbLibrary.SelectedValue.ToString() == "0")
            {
                MessageBox.Show("Please select a library.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbImportProfile.SelectedItem == null || cmbImportProfile.SelectedValue.ToString() == "0")
            {
                MessageBox.Show("Please select an import profile.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Disable controls during processing
            SetControlsEnabled(false);
            progressBar.Value = 0;
            lblStatus.Text = "Processing...";

            try
            {
                await System.Threading.Tasks.Task.Run(() => ProcessDocuments());
                MessageBox.Show("Processing completed successfully!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                lblStatus.Text = "Processing completed";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during processing: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError($"Processing failed: {ex.Message}");
                lblStatus.Text = "Processing failed";
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        private void ProcessDocuments()
        {
            string selectedLibrary = cmbLibrary.SelectedValue?.ToString() ?? "";
            int chunkSize = 5000;
            int offset = 0;
            int totalProcessed = 0;
            int successCount = 0;
            int errorCount = 0;

            while (true)
            {
                string query = $@"
                    SELECT * 
                    FROM (
                        SELECT 
                            ROW_NUMBER() OVER (ORDER BY DC.id) AS RawNum, 
                            DC.id, 
                            CS.sourcelocation + '\' + DC.path AS Location, 
                            CS.sourcelocation AS SourceLocation,
                            DC.[pagecount], 
                            DC.Library_ID,
                            DI.DateTime AS UploadDate,
                            DC.path
                        FROM 
                            dbo.document DC
                            INNER JOIN dbo.DocumentInformation AS DI ON DC.ID = DI.DocumentID
                            INNER JOIN dbo.importprofile IMP ON DC.importprofileid = IMP.id
                            INNER JOIN contentsource CS ON IMP.[source] = CS.id
                        WHERE 
                            DC.IsEncrypted = 1 
                            AND (DC.IsDecrypt = 0 OR DC.IsDecrypt IS NULL) 
                            AND (DC.DecryptStatus = 0 OR DC.DecryptStatus IS NULL)
                            AND DC.Library_ID = {selectedLibrary}
                    ) tbl
                    WHERE RawNum > {offset} AND RawNum <= {offset + chunkSize}
                    ORDER BY RawNum";

                DataTable dt = new DataTable();
                using (SqlConnection conn = new SqlConnection(txtConnectionString.Text))
                {
                    conn.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        adapter.Fill(dt);
                    }
                }

                if (dt.Rows.Count == 0)
                {
                    break; // No more documents to process
                }

                foreach (DataRow row in dt.Rows)
                {
                    try
                    {
                        string sourcePath = row["Location"].ToString() ?? "";
                        string sourceLocation = row["SourceLocation"].ToString() ?? "";
                        DateTime uploadDate = Convert.ToDateTime(row["UploadDate"]);
                        string fileName = Path.GetFileName(row["path"].ToString() ?? "");

                        // Create destination path with year/month structure in the same storage location
                        // Example: C:\Storage\abc.pdf -> C:\Storage\2025\11\abc.pdf
                        string year = uploadDate.Year.ToString();
                        string month = uploadDate.Month.ToString("00");
                        string destinationFolder = Path.Combine(sourceLocation, year, month);
                        string destinationPath = Path.Combine(destinationFolder, fileName);

                        // Create directory if it doesn't exist
                        if (!Directory.Exists(destinationFolder))
                        {
                            Directory.CreateDirectory(destinationFolder);
                        }

                        // Copy file
                        if (File.Exists(sourcePath))
                        {
                            File.Copy(sourcePath, destinationPath, true);
                            successCount++;
                            LogInfo($"Copied: {sourcePath} -> {destinationPath}");
                        }
                        else
                        {
                            errorCount++;
                            LogError($"Source file not found: {sourcePath}");
                        }

                        totalProcessed++;

                        // Update progress on UI thread
                        this.Invoke((MethodInvoker)delegate
                        {
                            lblStatus.Text = $"Processed: {totalProcessed} | Success: {successCount} | Errors: {errorCount}";
                        });
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        LogError($"Error processing document ID {row["id"]}: {ex.Message}");
                    }
                }

                offset += chunkSize;
            }

            // Final status update
            this.Invoke((MethodInvoker)delegate
            {
                lblStatus.Text = $"Completed! Total: {totalProcessed} | Success: {successCount} | Errors: {errorCount}";
            });
        }

        private void SetControlsEnabled(bool enabled)
        {
            txtConnectionString.Enabled = enabled;
            btnTestConnection.Enabled = enabled;
            cmbLibrary.Enabled = enabled;
            btnProcess.Enabled = enabled;
        }

        private void LogInfo(string message)
        {
            string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
            File.AppendAllText(logFile, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [INFO] {message}\r\n");
        }

        private void LogError(string message)
        {
            string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
            File.AppendAllText(logFile, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [ERROR] {message}\r\n");
        }


    }
}
