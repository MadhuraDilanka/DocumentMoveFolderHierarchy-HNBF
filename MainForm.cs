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

        private void EnsureStoredProcedureExists(string connectionString)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Check if stored procedure exists
                    string checkQuery = @"
                        SELECT COUNT(*) 
                        FROM sys.objects 
                        WHERE object_id = OBJECT_ID(N'[dbo].[GetNewDocumentPathByFolderHierarchy]') 
                        AND type IN (N'P', N'PC')";

                    bool spExists = false;
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        spExists = (int)checkCmd.ExecuteScalar() > 0;
                    }

                    if (!spExists)
                    {
                        LogInfo("Creating stored procedure [GetNewDocumentPathByFolderHierarchy]...");

                        string createSpQuery = @"
CREATE PROCEDURE [dbo].[GetNewDocumentPathByFolderHierarchy]
    @Library_ID BIGINT,
    @ImportProfileId INT,
    @document_id BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @folder_path NVARCHAR(MAX) = '';
    DECLARE @file_name NVARCHAR(MAX);
    DECLARE @file_extention NVARCHAR(MAX);
    DECLARE @full_file_path NVARCHAR(MAX);
    DECLARE @thumbnail_path NVARCHAR(MAX);
    DECLARE @source_Type NVARCHAR(50);
    DECLARE @is_folderHierarchy BIT;
    DECLARE @is_date BIT;
    DECLARE @is_combined BIT;
    DECLARE @is_original_name BIT;
    DECLARE @FileNameDateFormat NVARCHAR(MAX);
    DECLARE @IndexDataTable NVARCHAR(255);
    DECLARE @Branch NVARCHAR(255);
    DECLARE @originalFileName NVARCHAR(255);
    DECLARE @sourse_id INT;

    SELECT @IndexDataTable = [IndexDataTable]
    FROM [dbo].[Portal]
    WHERE [ID] = @Library_ID;

    SELECT @sourse_id = [Source]
    FROM [dbo].[ImportProfile]
    WHERE [ID] = @ImportProfileId;

    DECLARE @sql NVARCHAR(MAX) = N'SELECT @BranchOut = [Branch] FROM ' 
                                 + QUOTENAME(@IndexDataTable) 
                                 + N' WHERE DocumentID = @DocID';
    EXEC sp_executesql @sql, 
         N'@DocID BIGINT, @BranchOut NVARCHAR(255) OUTPUT',
         @DocID = @document_id, 
         @BranchOut = @Branch OUTPUT;

    IF @Branch IS NOT NULL
    BEGIN
        SET @Branch = LTRIM(RTRIM(@Branch));
        WHILE CHARINDEX('  ', @Branch) > 0
            SET @Branch = REPLACE(@Branch, '  ', ' ');
    END

    SELECT 
        @file_name = [Path],
        @file_extention = RIGHT([Path], CHARINDEX('.', REVERSE([Path])) - 1),
        @source_Type = CS.SourceType
    FROM [dbo].[Document] D
    INNER JOIN [dbo].[ImportProfile] IP ON D.ImportProfileID = IP.ID
    INNER JOIN [dbo].[ContentSource] CS ON IP.Source = CS.ID
    WHERE D.ID = @document_id AND D.Library_ID = @Library_ID;

    SET @originalFileName = REVERSE(SUBSTRING(REVERSE(@file_name), 
                                              CHARINDEX('\', REVERSE(@file_name)) + 1, 
                                              LEN(@file_name)));
    SET @originalFileName = LEFT(@originalFileName, 
                                 LEN(@originalFileName) - CHARINDEX('.', REVERSE(@originalFileName)));

    SELECT 
        @is_folderHierarchy = [IsFolderHierarchy],
        @is_date = [IsDate],
        @is_combined = [IsCombined],
        @is_original_name = [IsOriginalName]
    FROM [dbo].[DocumentNaming]
    WHERE [SourceID] = @sourse_id AND [Library_ID] = @Library_ID;

    IF EXISTS (SELECT * FROM [dbo].[Folder] WHERE [ImportProfileID] = @sourse_id AND [Library_ID] = @Library_ID)
    BEGIN
        ;WITH cte AS
        (
            SELECT 
                [OrderId],
                [CombineSequence],
                [OptionName],
                LTRIM(RTRIM([OptionValue])) AS [OptionValue]
            FROM ConvertFolderTable(@sourse_id, @Library_ID)
        ),
        cte2 AS
        (
            SELECT
                T1.orderID,
                STUFF(
                    ISNULL((
                        SELECT '\' + LTRIM(RTRIM(T2.[OptionValue]))
                        FROM cte AS T2
                        WHERE T2.orderID = T1.orderID
                        GROUP BY T2.[OptionValue], T2.[CombineSequence]
                        ORDER BY T2.[CombineSequence]
                        FOR XML PATH(''), TYPE
                    ).value('.', 'NVARCHAR(MAX)'), ''),
                1, 1, '') AS F_PATH
            FROM cte AS T1
            GROUP BY T1.[OrderId]
        )
        SELECT @folder_path = COALESCE(@folder_path + '\', '') + LTRIM(RTRIM(F_PATH))
        FROM cte2;
    END

    IF @Branch IS NOT NULL AND @folder_path LIKE '%Branch%'
        SET @folder_path = REPLACE(@folder_path, 'Branch', @Branch);

    SET @file_extention = CASE 
                            WHEN LEFT(@file_extention, 1) = '.' THEN @file_extention 
                            ELSE '.' + @file_extention 
                          END;

    SET @thumbnail_path = ISNULL(@folder_path + '\', '') + @originalFileName;
    SET @full_file_path = ISNULL(@folder_path + '\', '') + @originalFileName + @file_extention;

    IF @source_Type = 'HTTP'
    BEGIN
        SET @thumbnail_path = 'CONTENT\' + @thumbnail_path;
        SET @full_file_path = 'CONTENT\' + @full_file_path;
    END

    SELECT 
        @document_id AS DocumentID,
        @Branch AS CleanBranchValue,
        @folder_path AS FolderHierarchyPath,
        @originalFileName AS FileName,
        @full_file_path AS NewFullPath,
        @thumbnail_path AS NewThumbnailPath;
END";

                        using (SqlCommand createCmd = new SqlCommand(createSpQuery, conn))
                        {
                            createCmd.CommandTimeout = 120;
                            createCmd.ExecuteNonQuery();
                        }

                        LogInfo("Stored procedure created successfully.");
                    }
                    else
                    {
                        LogInfo("Stored procedure already exists.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error ensuring stored procedure exists: {ex.Message}");
                throw new Exception($"Failed to create required stored procedure: {ex.Message}", ex);
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

            // Get values from UI controls before starting background thread
            string selectedLibrary = cmbLibrary.SelectedValue?.ToString() ?? "";
            string selectedImportProfile = cmbImportProfile.SelectedValue?.ToString() ?? "";
            string connectionString = txtConnectionString.Text;

            // Disable controls during processing
            SetControlsEnabled(false);
            progressBar.Value = 0;
            lblStatus.Text = "Checking stored procedure...";

            try
            {
                // Ensure stored procedure exists
                EnsureStoredProcedureExists(connectionString);
                lblStatus.Text = "Processing...";

                await System.Threading.Tasks.Task.Run(() => ProcessDocuments(selectedLibrary, selectedImportProfile, connectionString));
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

        private void ProcessDocuments(string selectedLibrary, string selectedImportProfile, string connectionString)
        {
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
                            DC.Library_ID = {selectedLibrary}
                    ) tbl
                    WHERE RawNum > {offset} AND RawNum <= {offset + chunkSize}
                    ORDER BY RawNum";

                DataTable dt = new DataTable();
                using (SqlConnection conn = new SqlConnection(connectionString))
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
                    SqlConnection updateConn = null;
                    try
                    {
                        long documentId = Convert.ToInt64(row["id"]);
                        string sourcePath = row["Location"].ToString() ?? "";
                        string sourceLocation = row["SourceLocation"].ToString() ?? "";

                        // Call stored procedure to get new path
                        string newRelativePath = "";
                        updateConn = new SqlConnection(connectionString);
                        updateConn.Open();

                        using (SqlCommand cmd = new SqlCommand("[dbo].[GetNewDocumentPathByFolderHierarchy]", updateConn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@Library_ID", selectedLibrary);
                            cmd.Parameters.AddWithValue("@ImportProfileId", selectedImportProfile);
                            cmd.Parameters.AddWithValue("@document_id", documentId);

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    newRelativePath = reader["NewFullPath"].ToString();
                                }
                            }
                        }

                        if (string.IsNullOrWhiteSpace(newRelativePath))
                        {
                            errorCount++;
                            LogError($"Failed to get new path for document ID {documentId}");
                            totalProcessed++;
                            this.Invoke((MethodInvoker)delegate
                            {
                                lblStatus.Text = $"Processed: {totalProcessed} | Success: {successCount} | Errors: {errorCount}";
                            });
                            continue;
                        }

                        // Build full destination path
                        string destinationPath = Path.Combine(sourceLocation, newRelativePath);
                        string destinationFolder = Path.GetDirectoryName(destinationPath);

                        // Create directory if it doesn't exist
                        if (!Directory.Exists(destinationFolder))
                        {
                            Directory.CreateDirectory(destinationFolder);
                        }

                        // Copy file
                        if (File.Exists(sourcePath))
                        {
                            File.Copy(sourcePath, destinationPath, true);

                            // Update document table with new path
                            using (SqlCommand updateCmd = new SqlCommand("UPDATE dbo.document SET [Path] = @NewPath WHERE ID = @DocumentId", updateConn))
                            {
                                updateCmd.Parameters.AddWithValue("@NewPath", newRelativePath);
                                updateCmd.Parameters.AddWithValue("@DocumentId", documentId);
                                updateCmd.ExecuteNonQuery();
                            }

                            successCount++;
                            LogInfo($"Copied and updated: {sourcePath} -> {destinationPath} | Document ID: {documentId}");
                        }
                        else
                        {
                            errorCount++;
                            LogError($"Source file not found: {sourcePath} | Document ID: {documentId}");
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
                    finally
                    {
                        if (updateConn != null && updateConn.State == ConnectionState.Open)
                        {
                            updateConn.Close();
                        }
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
