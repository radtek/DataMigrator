﻿using System;
using System.IO;
using System.Windows.Forms;
using DataMigrator.Common;
using DataMigrator.Common.Models;
using DataMigrator.Windows.Forms.Diagnostics;

namespace DataMigrator.Csv
{
    public partial class CsvConnectionControl : UserControl, IConnectionControl
    {
        public string FilePath
        {
            get { return txtFile.Text.Trim(); }
            set { txtFile.Text = value; }
        }

        public bool HasHeaderRow
        {
            get { return cbHasHeaderRow.Checked; }
            set { cbHasHeaderRow.Checked = value; }
        }

        public string ConnectionString
        {
            get
            {
                #region Checks

                if (!File.Exists(FilePath))
                {
                    TraceService.Instance.WriteMessage(TraceEvent.Error, "FilePath is invalid. Please try again.");
                    return string.Empty;
                }

                #endregion Checks

                return FilePath;
            }
        }

        public CsvConnectionControl()
        {
            InitializeComponent();
        }

        #region IConnectionControl Members

        public UserControl ControlContent => this;

        public ConnectionDetails ConnectionDetails
        {
            get
            {
                var connectionDetails = new ConnectionDetails
                {
                    Database = this.FilePath,
                    ProviderName = Constants.PROVIDER_NAME,
                    ConnectionString = this.ConnectionString
                };

                connectionDetails.ExtendedProperties.Add("HasHeaderRow", HasHeaderRow);
                return connectionDetails;
            }
            set
            {
                FilePath = value.Database;
                HasHeaderRow = ConnectionDetails.ExtendedProperties["HasHeaderRow"].GetValue<bool>();
            }
        }

        public bool ValidateConnection() => File.Exists(FilePath);

        #endregion IConnectionControl Members

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (dlgOpenFile.ShowDialog() == DialogResult.OK)
            {
                FilePath = dlgOpenFile.FileName;
            }
        }
    }
}